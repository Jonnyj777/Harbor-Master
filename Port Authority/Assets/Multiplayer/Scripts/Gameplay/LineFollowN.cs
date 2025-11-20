using Edgegap;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineFollowN : NetworkBehaviour
{
    [Header("Line Drawing Settings")]

    [SerializeField]
    private LayerMask layerMask;
    private SyncList<Vector3> linePositions = new SyncList<Vector3>();
    public float timerDelayBetweenLinePoints = 0.01f;
    //public Color lineColor = Color.red;
    [SyncVar(hook = nameof(OnLineColorChanged))] public Vector3 lineColor;
    public float lineWidth = 1;
    private LineRenderer line;
    private float timer;

    [Header("Line Following Settings")]

    //public float heightOffset;
    public float truckSpeed = 20f;
    public static float baseBoatSpeed = 20f;
    public static float globalBoatSpeed; // The current effective value (changes when upgraded)

    private Rigidbody rb;
    private bool lineFollowing = false;
    [SyncVar] private bool drawingLine = false;
    private bool atPort = false;
    private bool isCrashed = false;
    private SyncList<Vector3> positions = new SyncList<Vector3>();
    private int moveIndex = 0;
    private float heightOffset = 0;

    //variables for network server-side authority and request management
    [SyncVar] private uint authorizedId = 0;
    [SyncVar] private bool isDragging = false;
    [SyncVar] private bool isDraggable = false;
    private NetworkIdentity unitIdentity;
    [SyncVar] private bool lineFinished = false;

    [Header("Path Processing")]
    [SerializeField] private bool smoothPath = true;
    [SerializeField][Range(1, 4)] private int smoothingIterations = 3;
    // Increase epsilon to reduce jitter (THIS IS THE NOTICEABLE VALUE)
    private float simplifyEpsilon = 1f; // RDP tolerance (world units)

    [Header("Delivery Settings")]
    public float delayPerCargo = 2.0f; // To be shared across child classes

    [Header("Rotation")]
    [Tooltip("How quickly the vehicle rotates to face the path direction.")]
    public float rotationSpeed = 2f;

    private void Awake()
    {
        if (globalBoatSpeed == 0)
        {
            globalBoatSpeed = baseBoatSpeed;
        }
    }

    private void Start()
    {
        unitIdentity = GetComponent<NetworkIdentity>();
        line = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        timer = timerDelayBetweenLinePoints;
        Renderer rend = GetComponent<Renderer>();
        heightOffset = rend.bounds.size.y * 0.5f;
        line.material = new Material(Shader.Find("Sprites/Default"));

        linePositions.Callback += OnLinePositionsChanged;

        if (CompareTag("Truck"))
        {
            SnapToSurface();
        }

    }


    public void OnLineColorChanged(Vector3 oldColorData, Vector3 newColorData)
    {
        line.startColor = line.endColor = new Color(newColorData.x, newColorData.y, newColorData.z);
        print("Color changed to: " + newColorData);
    }

    // Line Drawing Functions
    public void StartLine(Vector3 position)
    {
        line.positionCount = 1;
        line.startWidth = line.endWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        print("starting line with color: " + line.startColor);
    }

    [Server]
    public void ServerUpdateLine(Vector3 raycastMousePos)
    {
        //print("Update line");
        //Debug.DrawRay(Camera.main.ScreenToWorldPoint(mousePos), GetMousePosition(mousePos), Color.red);
        timer -= Time.deltaTime;
        if (timer <= 0) {
            linePositions.Add(raycastMousePos);
            //line.positionCount = linePositions.Count;
            //line.SetPositions(linePositions.ToArray());
            timer = timerDelayBetweenLinePoints;
        }
    }

    [Server]
    private void ClearPositions()
    {
        linePositions.Clear();
    }

    [Command]
    public void CmdDeleteLine()
    {
        lineFinished = true;
        line.positionCount = 0;
        ClearPositions();
        line.SetPositions(linePositions.ToArray());
        lineFollowing = false;
        lineFinished = false;
    }

    [Server]
    public void DeleteLine()
    {
        lineFinished = true;
        line.positionCount = 0;
        ClearPositions();
        line.SetPositions(linePositions.ToArray());
        lineFollowing = false;
        lineFinished = false;
    }

    Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
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
        if (!NetworkClient.localPlayer) return;

        NetworkPlayer playerAuthorizer = NetworkClient.localPlayer.GetComponent<NetworkPlayer>();
        playerAuthorizer.CmdRequestAuthority(unitIdentity);
    }

    public void StartDrag()
    {
        print("Start Drag");
        isDragging = true;
        isDraggable = true; ;
        atPort = false;
        lineFollowing = false;
        drawingLine = true;
        CmdRequestStart();
        StartLine(transform.position);
    }

    private void OnMouseDrag()
    {
        if (!isOwned || !isDraggable) return;

        CmdRequestMove(GetMousePosition());
    }


    private void OnMouseUp()
    {
        if (!isDragging || (!isServer && !isOwned) || !isDraggable) return;

        CmdReleaseSettings();

        NetworkPlayer playerPlayer = NetworkClient.localPlayer.GetComponent<NetworkPlayer>();
        playerPlayer.CmdRemoveAuthority(unitIdentity);

    }

    [Server]
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

    [Server]
    private void FollowLineTruck()
    {
        if (moveIndex >= positions.Count) return;
        // update position, direction, and rotation of object
        Vector3 currentPos = positions[moveIndex];

        Vector3 targetPos = new Vector3(currentPos.x, transform.position.y, currentPos.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, truckSpeed * Time.deltaTime);

        Vector3 direction = (targetPos - transform.position);
        direction.y = 0f;
        direction.Normalize();

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 nextPosXZ = new Vector3(currentPos.x, 0, currentPos.z);
        if (Vector3.Distance(currentPosXZ, nextPosXZ) <= 0.02f)
        {
            moveIndex++;
        }

        SnapToSurface();

        RemoveLineTraveled("truck");
        // remove the part of line already traveled on
    }

    [Server]
    private void RemoveLineTraveled(string vehicleType)
    {
        if (line.positionCount > 1)
        {
            if (vehicleType == "boat")
            {
                Vector3[] currentPositions = new Vector3[line.positionCount];
                line.GetPositions(currentPositions);

                currentPositions[0] = transform.position;

                //Vector3 flatCurrent = new Vector3(currentPositions[0].x, 0, currentPositions[0].z);
                //Vector3 flatNext = new Vector3(currentPositions[1].x, 0, currentPositions[1].z);

                if (Vector3.Distance(currentPositions[0], currentPositions[1]) < 0.05f)
                {
                    List<Vector3> temp = new List<Vector3>(currentPositions);
                    temp.RemoveAt(0);
                    currentPositions = temp.ToArray();
                    linePositions.RemoveAt(0);
                }

                //if (Vector3.Distance(flatCurrent, flatNext) < 0.5f)
                //{
                    //SyncList<Vector3> temp = new SyncList<Vector3>(currentPositions);
                    //temp.RemoveAt(0);
                    //currentPositions = temp.ToArray();
                    //linePositions.RemoveAt(0);
                //}
                line.positionCount = currentPositions.Length;
                line.SetPositions(currentPositions);
                //line.positionCount = currentPositions.Length;
                //line.SetPositions(currentPositions);
            }
            else if(vehicleType == "truck")
            {
                Vector3[] currentPositions = new Vector3[line.positionCount];
                line.GetPositions(currentPositions);

                currentPositions[0] = transform.position;

                Vector3 flatCurrent = new Vector3(currentPositions[0].x, 0, currentPositions[0].z);
                Vector3 flatNext = new Vector3(currentPositions[1].x, 0, currentPositions[1].z);
                print("dist: " + Vector3.Distance(flatCurrent, flatNext));
                if (Vector3.Distance(flatCurrent, flatNext) < 1.5f)
                {
                    print("removeing");
                    //List<Vector3> temp = new List<Vector3>(currentPositions);
                    linePositions.RemoveAt(0);
                    //currentPositions = temp.ToArray();
                }

                //line.positionCount = currentPositions.Length;
                //line.SetPositions(currentPositions);
            }
        }
    }

    [Server]
    private void FollowLineBoat()
    {
        if (moveIndex >= positions.Count) return;
        // update position and direction of object
        Vector3 currentPos = positions[moveIndex];
        transform.position = Vector3.MoveTowards(transform.position, currentPos, globalBoatSpeed * Time.deltaTime);

        Vector3 direction = (currentPos - transform.position).normalized;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        float distance = Vector3.Distance(currentPos, transform.position);
        if (distance <= 0.05f)
        {
            moveIndex++;
        }

        RemoveLineTraveled("boat");
        // remove the part of line already traveled on
        /*
        if (line.positionCount > 1)
        {
            Vector3[] currentPositions = new Vector3[line.positionCount];
            line.GetPositions(currentPositions);

            currentPositions[0] = transform.position;

            if (Vector3.Distance(currentPositions[0], currentPositions[1]) < 0.05f)
            {
                SyncList<Vector3> temp = new SyncList<Vector3>(currentPositions);
                temp.RemoveAt(0);
                currentPositions = temp.ToArray();
            }

            line.positionCount = currentPositions.Length;
            line.SetPositions(currentPositions);
        }
        */
    }

    public void SetLineFollowing(bool value)
    {
        lineFollowing = value;
    }

    public void SetAtPort(bool value)
    {
        atPort = value;
    }

    [Server]
    public void SetIsCrashed(bool value)
    {
        if (value)
        {
            DeleteLine();
        }
        isCrashed = value;

    }

    private void OnLinePositionsChanged(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
    {
        if (lineFinished) return;
        line.positionCount = linePositions.Count;
        List<Vector3> raw = new List<Vector3>(linePositions);
        List<Vector3> simplified = RamerDouglasPeucker(raw, simplifyEpsilon);

        if (smoothPath && simplified.Count >= 2)
        {
            line.SetPositions(ChaikinSmooth(simplified, smoothingIterations).ToArray());
        }
        else
        {
            line.SetPositions(linePositions.ToArray());
        }

    }

    [Command]
    public void CmdRequestMove(Vector3 raycastMousePos)
    {
        ServerUpdateLine(raycastMousePos);
    }

    [Command]
    public void CmdRequestStart()
    {
        isDragging = true;
        isDraggable = true;
        atPort = false;
        lineFollowing = false;
        drawingLine = true;
    }

    [Command]
    public void CmdReleaseSettings()
    {
        PopulatePositions();
        lineFollowing = true;
        drawingLine = false;
        moveIndex = 0;
        authorizedId = 0;
        isDraggable = false;

    }

    [Server]
    private void PopulatePositions()
    {
        positions.Clear();

        Vector3[] positionsArray = new Vector3[line.positionCount];
        line.GetPositions(positionsArray);

        for (int i = 0; i < positionsArray.Length; i++)
        {
            positions.Add(positionsArray[i]);
        }
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        CmdDeleteLine();
        //NetworkPlayer localPlayer = NetworkClient.localPlayer.GetComponent<NetworkPlayer>();
        //lineColor = localPlayer.lineColorData;
        Friend self = new Friend(SteamClient.SteamId);
        string colorName = SteamLobbyManager.Lobby.GetMemberData(self, "lineColor");
        print("color: " + colorName);

        CmdSetLineColor(colorName);
        StartDrag();

    }

    [Command]
    private void CmdSetLineColor(string colorName)
    {
        print("setting color to: " + colorName);
        Vector3 colorData;
        switch (colorName)
        {
            case "Orange":
                colorData = new Vector3(255f / 255f, 119f / 255f, 0f / 255f);
                break;
            case "Blue":
                colorData = new Vector3(14f / 255f, 165f / 255f, 233f / 255f);
                break;
            case "Pink":
                colorData = new Vector3(255f / 255f, 31f / 255f, 139f / 255f);
                break;
            case "Purple":
                colorData = new Vector3(182f / 255f, 27f / 255f, 243f / 255f);
                break;
            case "Red":
                colorData = new Vector3(233f / 255f, 14f / 255f, 18f / 255f);
                break;
            case "Yellow":
                colorData = new Vector3(255f / 255f, 217f / 255f, 0f / 255f);
                break;
            case "Green":
                colorData = new Vector3(22f / 255f, 218f / 255f, 35f / 255f);
                break;
            default:
                colorData = new Vector3(1.0f, 1.0f, 1.0f);
                break;
        }

        RpcApplyLineColor(colorData);
    }

    [ClientRpc]
    private void RpcApplyLineColor(Vector3 newColorData)
    {
        if (line.material == null) line.material = new Material(Shader.Find("Sprites/Default"));

        Color newLineColor = new Color(newColorData.x, newColorData.y, newColorData.z);

        lineColor = newColorData;
        line.startColor = line.endColor = newLineColor;
    }


    private void Update()
    {
        if (isServer)
        {
            if (isCrashed)
            {
                return;
            }

            //print("line Following: " + lineFollowing + " : drawingLine: " + drawingLine + " : Tag: " + CompareTag("Boat") + " : atPort: " + atPort);
            if (lineFollowing)
            {
                if (CompareTag("Boat"))
                {
                    FollowLineBoat();
                }
                else if (CompareTag("Truck"))
                {
                    FollowLineTruck();
                }

                // remove line after following finishes
                if (moveIndex > positions.Count - 1)
                {
                    print("finished");
                    DeleteLine();
                }
            }
            else if (!drawingLine && CompareTag("Boat") && !atPort)
            {
                transform.position += globalBoatSpeed * Time.deltaTime * transform.forward;
            }
        }
    }
}
