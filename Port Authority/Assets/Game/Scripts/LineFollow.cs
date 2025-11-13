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
    public float delayPerCargo = 2.0f; // To be shared across child classes

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
        List<Vector3> simplified = RamerDouglasPeucker(raw, simplifyEpsilon);

        // Optionally apply Chaikin smoothing
        if (smoothPath && simplified.Count >= 2)
        {
            positions = ChaikinSmooth(simplified, smoothingIterations).ToArray();
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

    private List<Vector3> ChaikinSmooth(List<Vector3> points, int iterations)
    {
        if (points == null || points.Count < 2)
            return new List<Vector3>(points);

        List<Vector3> smoothed = new List<Vector3>(points);

        for (int i = 0; i < iterations; i++)
        {
            List<Vector3> newPoints = new List<Vector3>();
            newPoints.Add(smoothed[0]); // Keep the first point

            for (int j = 0; j < smoothed.Count - 1; j++)
            {
                Vector3 p0 = smoothed[j];
                Vector3 p1 = smoothed[j + 1];

                Vector3 Q = 0.75f * p0 + 0.25f * p1;
                Vector3 R = 0.25f * p0 + 0.75f * p1;

                newPoints.Add(Q);
                newPoints.Add(R);
            }

            newPoints.Add(smoothed[^1]); // Keep the last point
            smoothed = newPoints;
        }

        return smoothed;
    }

    private List<Vector3> RamerDouglasPeucker(List<Vector3> points, float epsilon)
    {
        if (points == null || points.Count < 3)
        {
            return new List<Vector3>(points);
        }

        int index = -1;
        float maxDistance = 0f;

        // Find the furthest point
        for (int i = 1; i < points.Count - 1; i++)
        {
            // We want to determine if a point is far enough to consider keeping it (greater than epsilon)
            // Otherwise, we can throw it out (the line isn't changed by much in getting rid of it)
            float distance = PerpendicularDistance(points[i], points[0], points[^1]);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                index = i;
            }
        }

        if (maxDistance > epsilon)
        {
            // Recursive split
            List<Vector3> left = RamerDouglasPeucker(points.GetRange(0, index + 1), epsilon);
            List<Vector3> right = RamerDouglasPeucker(points.GetRange(index, points.Count - index), epsilon);

            left.RemoveAt(left.Count - 1);
            left.AddRange(right);
            return left;
        }
        else
        {
            return new List<Vector3> { points[0], points[^1] };
        }
    }

    private float PerpendicularDistance(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        // Perpendicular distance from a point to a line segment
        if (lineStart == lineEnd)
        {
            return Vector3.Distance(point, lineStart);
        }

        Vector3 direction = lineEnd - lineStart;
        Vector3 projected = Vector3.Project(point - lineStart, direction.normalized) + lineStart;
        
        return Vector3.Distance(point, projected);
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
