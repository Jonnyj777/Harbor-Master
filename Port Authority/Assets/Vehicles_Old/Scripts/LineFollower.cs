using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LineFollower : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public VehicleType vehicleType = VehicleType.Boat;  // Default type: Boat
    private Renderer vehicleRenderer;
    private Color originalColor;  // used for a reset state later on (e.g., repairs on crashed vehicles)

    [Header("Path Settings")]
    public DrawLine drawControl;
    public float speed = 5f;
    public bool pathFinding = false;
    bool drawingLine = false;
    public bool atPort = false;
    Vector3[] positions;
    int moveIndex = 0;

    [Header("Crash Settings")]
    public CrashType crashType = CrashType.None;
    public bool isCrashed = false;
    public Color landCrashedColor = Color.red;
    public Color boatCrashedColor = Color.cyan;

    public enum CrashType
    {
        None,
        Land,
        Boat
    }

    [Header("Environment")]
    private Transform waterPlane;
    private Transform terrain;   // used for land vehicles later on
    private float waterLevel;

    private void Start()
    {
        vehicleRenderer = GetComponent<Renderer>();
        if (vehicleRenderer != null)
        {
            originalColor = vehicleRenderer.material.color;
        }

        //GameObject waterPlane = GameObject.Find("WaterPlane");
        //waterLevel = waterPlane.transform.position.y;
    }

    // draw line once the object is clicked
    private void OnMouseDown()
    {
        drawControl.DeleteLine();
        atPort = false;
        pathFinding = false;
        drawingLine = true;
        drawControl.StartLine(transform.position);
    }

    private void OnMouseDrag()
    {
        drawControl.UpdateLine();
    }

    private void OnMouseUp()
    {
        positions = new Vector3[drawControl.drawLine.positionCount];
        drawControl.drawLine.GetPositions(positions);
        pathFinding = true;
        drawingLine = false;
        moveIndex = 0;
    }

    private void Update()
    {
        // If the vehicle is crashed, do not process movement
        if (isCrashed)
        {
            return;
        }

        if (pathFinding)
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
                float angleOffset = Quaternion.Angle(transform.rotation, targetRotation); 
                if (angleOffset > 10f) 
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                } 
            }

            Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 nextPosXZ = new Vector3(currentPos.x, 0, currentPos.z);
            if (Vector3.Distance(currentPosXZ, nextPosXZ) <= 0.05f)
            {
                moveIndex++;
            }

            drawControl.SnapToSurface();

            // remove the part of line already traveled on
            if (drawControl.drawLine.positionCount > 1)
            {
                Vector3[] currentPositions = new Vector3[drawControl.drawLine.positionCount];
                drawControl.drawLine.GetPositions(currentPositions);

                currentPositions[0] = transform.position;

                Vector3 flatCurrent = new Vector3(currentPositions[0].x, 0, currentPositions[0].z);
                Vector3 flatNext = new Vector3(currentPositions[1].x, 0, currentPositions[1].z);

                if (Vector3.Distance(flatCurrent, flatNext) < 0.5f)
                {
                    List<Vector3> temp = new List<Vector3>(currentPositions);
                    temp.RemoveAt(0);
                    currentPositions = temp.ToArray();
                }

                drawControl.drawLine.positionCount = currentPositions.Length;
                drawControl.drawLine.SetPositions(currentPositions);
            }

            // remove line after following finishes
            if (moveIndex > positions.Length - 1)
            {
                pathFinding = false;
                drawControl.DeleteLine();
            }
            
        }
        else if (!drawingLine && vehicleType == VehicleType.Boat & !atPort)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            drawControl.SnapToSurface();
        }
    }

    // Collision detection to set crash state
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Building"))
        {
            pathFinding = false;
            drawControl.DeleteLine();
        }

        if (vehicleType == VehicleType.Land && other.CompareTag("Water"))
        {
            EnterCrashState(CrashType.Land);
        }

        if (vehicleType == VehicleType.Boat && other.CompareTag("Terrain"))
        {
            EnterCrashState(CrashType.Boat);
        }

        LineFollower vehicle = other.gameObject.GetComponent<LineFollower>();
        if (vehicle == null) return;

        // Truck-to-Truck collision
        if (vehicleType == VehicleType.Land && vehicle.vehicleType == VehicleType.Land)
        {
            EnterCrashState(CrashType.Land);
            vehicle.EnterCrashState(CrashType.Land);
        }

        // Boat-to-Boat collision
        if (vehicleType == VehicleType.Boat && vehicle.vehicleType == VehicleType.Boat)
        {
            EnterCrashState(CrashType.Boat);
            vehicle.EnterCrashState(CrashType.Boat);
        }
    }

    // Apply crash state behavior depending on vehicle type
    private void EnterCrashState(CrashType type)
    {
        isCrashed = true;
        crashType = type;  // Truck or Boat collision?

        LineFollower vehicle = GetComponent<LineFollower>();

        if (vehicle != null)
        {
            vehicle.speed = 0;
        }

        switch (type)
        {
            case CrashType.Land:
                if (vehicleRenderer != null)
                {
                    vehicleRenderer.material.color = landCrashedColor;
                }
                Debug.Log($"{gameObject.name} crashed into another land vehicle and is now an avoidable obstacle");
                break;

            case CrashType.Boat:
                if (vehicleRenderer != null)
                {
                    vehicleRenderer.material.color = boatCrashedColor;
                }
                //Debug.Log($"{gameObject.name} crashed and will disappear after 3 seconds...");
                StartCoroutine(DelayedDestroy(3f));  // 3s delay before being destroyed (after collision)
                break;
        }
    }

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
