using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class LineFollow : MonoBehaviour
{
    [Header("Line Drawing Settings")]
    [SerializeField] private LayerMask layerMask;
    public float timerDelayBetweenLinePoints = 0.01f;
    public Color lineColor = Color.red;
    public float lineWidth = 1;
    private LineRenderer line;
    private List<Vector3> linePositions;
    private float timer;
    private readonly LineFollowPathLogic pathLogic = new LineFollowPathLogic();

    [Header("Path Output (movement)")]
    // Positions is the smoothed path that boats follow
    private Vector3[] positions = new Vector3[0];
    private int moveIndex = 0;

    [Header("Movement")]
    [SerializeField] private float truckSpeed = 20f;
    public static float globalBaseBoatSpeed = 20f;  // Modified by StoreScript.cs
    // Determines how much each boat type's speed is scaled compared to the baseSpeed
    [SerializeField] protected float boatTypeSpeedMultiplier = 1f; // Set for each boat prefab
    // The current effective value (changes per boat type; each type scales the same after speed upgrade)
    protected float EffectiveBoatSpeed => globalBaseBoatSpeed * boatTypeSpeedMultiplier;

    [Header("Rotation")]
    [Tooltip("How quickly the vehicle rotates to face the path direction.")]
    public float rotationSpeed = 2f;

    [Header("Path Processing")]
    [SerializeField] private bool smoothPath = true;
    [SerializeField] [Range(1, 4)] private int smoothingIterations = 3;
    // Increase epsilon to reduce jitter (THIS IS THE NOTICEABLE VALUE)
    private float simplifyEpsilon = 1f; // RDP tolerance (world units)

    [Header("Delivery Settings")]
    [SerializeField] public float boatLoadingDelay = 2.0f;
    public static float globalTruckLoadingDelay = 2.0f;  // Modified by StoreScript.cs

    private Rigidbody rb;
    private bool lineFollowing = false;
    private bool drawingLine = false;
    private bool isCrashed = false;
    private bool atPort = false;
    private bool isMovingCargo = false;
    private float heightOffset = 0;

    private void Start()
    {
        linePositions = new List<Vector3>();
        line = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        timer = timerDelayBetweenLinePoints;
        Renderer rend = GetComponent<Renderer>();
        heightOffset = rend.bounds.size.y * 0.5f;

        if (CompareTag("Truck"))
            SnapToSurface();
    }

    // Line Drawing Functions
    public void StartLine(Vector3 position)
    {
        linePositions.Clear();
        linePositions.Add(position);

        line.positionCount = 1;
        line.startWidth = line.endWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = lineColor;
        line.SetPosition(0, position);
    }

    public void UpdateLine()
    {
        if (Input.GetMouseButton(0))
        {
            Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), GetMousePosition(), Color.red);
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                linePositions.Add(GetMousePosition());
                line.positionCount = linePositions.Count; // Determines how many vertices the LineRenderer will render
                line.SetPositions(linePositions.ToArray()); // Applies the new positions to render
                timer = timerDelayBetweenLinePoints;
            }
        }
    }

    public void DeleteLine()
    {
        line.positionCount = 0;
        linePositions.Clear();
        positions = new Vector3[0];
        lineFollowing = false;
    }

    Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        int effectiveMask = layerMask.value;
        
        if (CompareTag("Truck"))
            effectiveMask |= LayerMask.GetMask("Default");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, effectiveMask))
        {
            Vector3 pos = hit.point + Vector3.up * 0.75f;

            if (CompareTag("Boat"))
            {
                pos.y = transform.position.y;
            }

                return pos;
        }

        Vector3 defaultPos = ray.origin + ray.direction * 10f;
        defaultPos.y = transform.position.y; 
        return defaultPos;
    }

    // Line Following Functions
    private void OnMouseDown()
    {
        DeleteLine();
        atPort = false;
        lineFollowing = false;
        drawingLine = true;
        StartLine(transform.position);
    }

    private void OnMouseDrag()
    {
        if (!isCrashed && !isMovingCargo)
        {
            UpdateLine();
        }
    }

    private void OnMouseUp()
    {
        drawingLine = false;

        // Convert raw list --> Simplify with RDP --> Smooth with Chaikin --> Return positions[]
        List<Vector3> raw = new List<Vector3>(linePositions);

        if (raw.Count == 0)
        {
            positions = new Vector3[0];
            return;
        }

        // Apply simplification via Ramer-Douglas-Peucker algo:
        // Reduces the "noise" in the path causing jitters.
        List<Vector3> simplified = pathLogic.Simplify(raw, simplifyEpsilon);

        // Optionally apply Chaikin smoothing
        if (smoothPath && simplified.Count >= 2)
        {
            positions = pathLogic.Smooth(simplified, smoothingIterations).ToArray();
        }
        else
        {
            positions = simplified.ToArray();
        }
        

        // Update the renderer and begin following
        line.positionCount = positions.Length;
        line.SetPositions(positions);
        moveIndex = 0;
        lineFollowing = true;
    }

    public void SnapToSurface()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, hit.normal);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            transform.position = hit.point + transform.up * heightOffset;
        }
    }
    private void FollowLineTruck()
    {
        // update position, direction, and rotation of object
        Vector3 currentPos = positions[moveIndex];
        Vector3 targetPos = new Vector3(currentPos.x, transform.position.y, currentPos.z);

        // Move toward path
        transform.position = Vector3.MoveTowards(transform.position, targetPos, truckSpeed * Time.deltaTime);

        // Rotate toward the future direction, not the immediate next point.
        int lookAheadIndex = Mathf.Min(moveIndex + 1, positions.Length - 1); // Helps prevent over-reacting to tiny path changes.
        Vector3 lookPoint = positions[lookAheadIndex];
        Vector3 direction = new Vector3(lookPoint.x, transform.position.y, lookPoint.z) - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Waypoint reached?
        Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 nextPosXZ = new Vector3(currentPos.x, 0, currentPos.z);
        if (Vector3.Distance(currentPosXZ, nextPosXZ) <= 0.02f)
        {
            moveIndex++;
        }

        SnapToSurface();

        // remove the part of line already traveled on
        if (line.positionCount > 1)
        {
            Vector3[] currentPositions = new Vector3[line.positionCount];
            line.GetPositions(currentPositions);

            currentPositions[0] = transform.position;

            Vector3 flatCurrent = new Vector3(currentPositions[0].x, 0, currentPositions[0].z);
            Vector3 flatNext = new Vector3(currentPositions[1].x, 0, currentPositions[1].z);

            if (Vector3.Distance(flatCurrent, flatNext) < 0.5f)
            {
                List<Vector3> temp = new List<Vector3>(currentPositions);
                temp.RemoveAt(0);
                currentPositions = temp.ToArray();
            }

            line.positionCount = currentPositions.Length;
            line.SetPositions(currentPositions);
        }
    }

    private void FollowLineBoat()
    {
        Vector3 currentPos = positions[moveIndex];

        // Move toward a point
        transform.position = Vector3.MoveTowards(transform.position, currentPos, EffectiveBoatSpeed * Time.deltaTime);

        // Rotate toward the future direction, not the immediate next point.
        int lookAheadIndex = Mathf.Min(moveIndex + 1, positions.Length - 1);
        Vector3 lookPoint = positions[lookAheadIndex];
        Vector3 direction = new Vector3(lookPoint.x, transform.position.y, lookPoint.z) - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // Did we reach the waypoint?
        if (Vector3.Distance(currentPos, transform.position) <= 0.05f)
        {
            moveIndex++;
        }

        // Remove the part of line already traveled on
        if (line.positionCount > 1)
        {
            Vector3[] currentPositions = new Vector3[line.positionCount];
            line.GetPositions(currentPositions);

            currentPositions[0] = transform.position;

            if (Vector3.Distance(currentPositions[0], currentPositions[1]) < 0.05f)
            {
                List<Vector3> temp = new List<Vector3>(currentPositions);
                temp.RemoveAt(0);
                currentPositions = temp.ToArray();
            }

            line.positionCount = currentPositions.Length;
            line.SetPositions(currentPositions);
        }
    }

    public void SetLineFollowing(bool value)
    {
        lineFollowing = value;
    }

    public void SetAtPort(bool value)
    {
        atPort = value;
    }

    public bool GetIsCrashed()
    {
        return isCrashed;
    }

    public float TruckSpeed
    {
        get => truckSpeed;
        set => truckSpeed = value;
    }

    public void SetIsCrashed(bool value)
    {
        if (value)
        {
            DeleteLine();
        }
        isCrashed = value;
    }

    public void SetIsMovingCargo(bool value)
    {
        if (value)
        {
            DeleteLine();
        }
        isMovingCargo = value;
    }

    private void Update()
    {
        if (isCrashed || isMovingCargo)
        {
            return;
        }

        if (lineFollowing)
        {
            // Directed vehicles follow a line to the end
            if (CompareTag("Boat"))
            {
                FollowLineBoat();
            }
            else if (CompareTag("Truck"))
            {
                FollowLineTruck();
            }

            // remove line after following finishes
            if (moveIndex > positions.Length - 1)
            {
                DeleteLine();
            }
        }
        else if (!drawingLine && CompareTag("Boat") && !atPort)
        {
            // Undirected boats move forward at a constant velocity
            transform.position += transform.forward * EffectiveBoatSpeed * Time.deltaTime;
        }
    }
}
