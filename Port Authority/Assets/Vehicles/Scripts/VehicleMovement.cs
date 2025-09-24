using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class VehicleMovement : MonoBehaviour
{
    [Header("Vehicle Settings")]  // Adds labeled header in Inspector for clarity regarding vehicle type and speed
    public VehicleType vehicleType = VehicleType.Boat;  // Default to Boat, can be changed in Inspector
    public float speed = 5f;  // Default, adjustable speed variable for different vehicles

    [Header("Crash Settings")]  // Adds labeled header in Inspector for clarity regarding vehicle crash states
    public bool isCrashed = false;  // Indicates if the vehicle is in a crashed state
    public CrashType crashType = CrashType.None;  // Type of crash, default to None
    public Color landCrashedColor = Color.red;  // Color to red when land vehicles crash
    public Color boatCrashedColor = Color.cyan;  // Color to when boat vehicles crash

    // Target position for the vehicle to move towards (moveTo function)
    private Vector3? targetPosition = null;
    private Renderer vehicleRenderer;
    private Color originalColor;  // used for a reset state later on (e.g., repairs on crashed vehicles)

    // enum for crash types
    public enum CrashType
    {
        None,
        Land,
        Boat
    }

    private void Awake()
    {
        vehicleRenderer = GetComponent<Renderer>();
        if (vehicleRenderer != null)
        {
            originalColor = vehicleRenderer.material.color;
        }
    }

    void Update()
    {
        // If the vehicle is crashed, do not process movement
        if (isCrashed)
        {
            return;
        }

        // Decide movement behavior based on vehicle type
        if (vehicleType == VehicleType.Boat)
        {
            if (targetPosition.HasValue)
            {
                MoveTo(targetPosition.Value);
            }
            //else
            //{
                //DefaultBoatMovement();
            //}
        }
        else if (vehicleType == VehicleType.Land)
        {
            if (targetPosition.HasValue)
            {
                MoveTo(targetPosition.Value);
            }
            else
            {
                DefaultLandMovement();
            }
        }

        // USED FOR TESTING PURPOSES ONLY - REMOVE LATER
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
            //SetTarget(new Vector3(10, 0, 10));
        //}

        // USED FOR TESTING PURPOSES ONLY - REMOVE LATER
        //if (Input.GetKeyDown(KeyCode.E))
        //{
            //SetTarget(new Vector3(70, 0, 20));
        //}
    }

    // Sets a new destination for this vehicle
    // Sade can call this function for path-drawing(?)
    public void SetTarget(Vector3 newTarget)
    {
        targetPosition = newTarget;
    }

    // Moves the vehicle towards the target position
    private void MoveTo(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            speed * Time.deltaTime
            );
    }

    // Default movement for boats (e.g., moving forward continuously)
    private void DefaultBoatMovement()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    // Default movement for land vehicles
    private void DefaultLandMovement()
    {
        // Discuss cycling behavior in next call
    }

    // Collision detection to set crash state
    private void OnCollisionEnter(Collision collision)
    {
        VehicleMovement other = collision.gameObject.GetComponent<VehicleMovement>();
        if (other == null) return;

        // land vehicle crash state
        // if land vehicle collides into another land vehicle, both land vehicles enter land crash state
        // land crash state == stuck in place, no fade out
        if (vehicleType == VehicleType.Land && other.vehicleType == VehicleType.Land)
        {
            EnterCrashState(CrashType.Land);
            other.EnterCrashState(CrashType.Land);
        }

        // boat vehicle crash state
        // if boat vehicle collides into another boat vehicle, both boat vehicles enter boat crash state
        // boat crash state = disappear off map after a few seconds (do NOT act as additional obstacles)
        if (vehicleType == VehicleType.Boat && other.vehicleType == VehicleType.Boat)
        {
            EnterCrashState(CrashType.Boat);
            other.EnterCrashState(CrashType.Boat);
        }
    }

    // Apply crash state behavior depending on vehicle type
    private void EnterCrashState(CrashType type)
    {
        isCrashed = true;  // switches the vehicle to a crashed state
        crashType = type;  // helps depict what class of vehicle is crashed

        // stop rigidbody immediately to have vehicles stay in place
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
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
                Debug.Log($"{gameObject.name} crashed and will disappear after 3 seconds...");
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

// REFERENCES USED FOR VEHICLE MOVEMENT SCRIPT
// https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
// https://docs.unity3d.com/ScriptReference/Transform.Translate.html
// https://docs.unity3d.com/ScriptReference/Input.GetKeyDown.html
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types
// https://docs.unity3d.com/Manual/Coroutines.html
// https://docs.unity3d.com/ScriptReference/WaitForSeconds.html
// https://docs.unity3d.com/ScriptReference/Renderer-material.html