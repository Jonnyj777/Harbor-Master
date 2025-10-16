using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineFollow : NetworkBehaviour
{
    [Header("Line Drawing Settings")]

    [SerializeField]
    private LayerMask layerMask;
    private SyncList<Vector3> linePositions = new SyncList<Vector3>();
    public float timerDelayBetweenLinePoints = 0.01f;
    public Color lineColor = Color.red;
    public float lineWidth = 1;
    private LineRenderer line;
    private float timer;

    [Header("Line Following Settings")]

    //public float heightOffset;
    public float speed = 5f;
    private Rigidbody rb;
    private bool lineFollowing = false;
    private bool drawingLine = false;
    private bool atPort = false;
    private bool isCrashed = false;
    private Vector3[] positions;
    private int moveIndex = 0;
    private float heightOffset = 0;

    //variables for network server-side authority and request management
    [SyncVar] private uint authorizedId = 0;
    private bool isDragging = false;
    private bool isDraggable = false;
    private NetworkIdentity unitIdentity;
    private bool lineFinished = false;



    private void Start()
    {
        unitIdentity = GetComponent<NetworkIdentity>();
        line = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody>();
        timer = timerDelayBetweenLinePoints;
        Renderer rend = GetComponent<Renderer>();
        heightOffset = rend.bounds.size.y * 0.5f;

        linePositions.Callback += OnLinePositionsChanged;

        if (CompareTag("Truck"))
        {
            SnapToSurface();
        }
    }

    // Line Drawing Functions
    public void StartLine(Vector3 position)
    {
        line.positionCount = 1;
        line.startWidth = line.endWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = lineColor;
    }

    [Server]
    public void ServerUpdateLine(Vector3 raycastMousePos)
    {
        print("serverupdateline");
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

        NetworkAuthorizer playerAuthorizer = NetworkClient.localPlayer.GetComponent<NetworkAuthorizer>();
        playerAuthorizer.CmdRequestAuthority(unitIdentity);
    }

    public void StartDrag()
    {
        print("Start Drag");
        isDragging = true;
        isDraggable = true;
        DeleteLine();
        atPort = false;
        lineFollowing = false;
        drawingLine = true;
        StartLine(transform.position);
    }

    private void OnMouseDrag()
    {
        if(!isOwned || !isDraggable) return;

        CmdRequestMove(GetMousePosition());
    }

    private void OnMouseUp()
    {
        if (!isDragging || (!isServer && !isOwned) || !isDraggable) return;

        NetworkAuthorizer playerAuthorizer = NetworkClient.localPlayer.GetComponent<NetworkAuthorizer>();
        playerAuthorizer.CmdRemoveAuthority(unitIdentity);

        positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        lineFollowing = true;
        drawingLine = false;
        moveIndex = 0;
        isDragging = false;
        
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

    [Server]
    private void FollowLineTruck()
    {
        // update position, direction, and rotation of object
        Vector3 currentPos = positions[moveIndex];

        Vector3 targetPos = new Vector3(currentPos.x, transform.position.y, currentPos.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        Vector3 direction = (targetPos - transform.position);
        direction.y = 0f;
        direction.Normalize();

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 nextPosXZ = new Vector3(currentPos.x, 0, currentPos.z);
        if (Vector3.Distance(currentPosXZ, nextPosXZ) <= 0.02f)
        {
            moveIndex++;
        }

        SnapToSurface();

        RemoveLineTraveled();
        // remove the part of line already traveled on
    }

    [Server]
    private void RemoveLineTraveled()
    {

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

    [Server]
    private void FollowLineBoat()
    {
        // update position and direction of object
        Vector3 currentPos = positions[moveIndex];
        transform.position = Vector3.MoveTowards(transform.position, currentPos, speed * Time.deltaTime);

        Vector3 direction = (currentPos - transform.position).normalized;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        float distance = Vector3.Distance(currentPos, transform.position);
        if (distance <= 0.05f)
        {
            moveIndex++;
        }

        // remove the part of line already traveled on
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

    private void OnLinePositionsChanged(SyncList<Vector3>.Operation op, int index, Vector3 oldItem, Vector3 newItem)
    {
        if (lineFinished) return;
        line.positionCount = linePositions.Count;
        line.SetPositions(linePositions.ToArray());
    }

    [Command]
    public void CmdRequestMove(Vector3 raycastMousePos)
    {
        print("cmdrequestmove");
        ServerUpdateLine(raycastMousePos);
    }

    [Command]
    public void CmdReleaseControl()
    {
        if (isServer)
        {
            unitIdentity.RemoveClientAuthority();
        }

        positions = new Vector3[line.positionCount];
        line.GetPositions(positions);
        lineFollowing = true;
        drawingLine = false;
        moveIndex = 0;
        authorizedId = 0;
        isDraggable = false;

    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        print("OnStartAuthority");

        StartDrag();

    }

    private void Update()
    {
        if (isServer)
        {
            //if (Input.GetMouseButton(0))
            //{
                //CmdRequestMove(GetMousePosition());
                //Debug.DrawRay(Camera.main.ScreenToWorldPoint(Input.mousePosition), GetMousePosition(), Color.red);
            //}
            if (isCrashed)
            {
                return;
            }

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
                if (moveIndex > positions.Length - 1)
                {
                    print("finished");
                    DeleteLine();
                }
            }
            else if (!drawingLine && CompareTag("Boat") && !atPort)
            {
                transform.position += transform.forward * speed * Time.deltaTime;
            }
        }
    }
}
