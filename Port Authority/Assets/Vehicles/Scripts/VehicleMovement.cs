using Unity.VisualScripting;
using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    [Header("Vehicle Settings")]  // Adds labeled header in Inspector for clarity
    public VehicleType vehicleType = VehicleType.Boat;  // Default to Boat, can be changed in Inspector
    public float speed = 5f;  // Default, adjustable speed variable for different vehicles

    // Target position for the vehicle to move towards (moveTo function)
    private Vector3? targetPosition = null;

    void Update()
    {
        // Decide movement behavior based on vehicle type
        if (vehicleType == VehicleType.Boat)
        {
            if (targetPosition.HasValue)
            {
                MoveTo(targetPosition.Value);
            }
            else
            {
                DefaultBoatMovement();
            }
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetTarget(new Vector3(10, 0, 10));
        }

        // USED FOR TESTING PURPOSES ONLY - REMOVE LATER
        if (Input.GetKeyDown(KeyCode.E))
        {
            SetTarget(new Vector3(70, 0, 20));
        }
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
}

// REFERENCES USED FOR VEHICLE MOVEMENT SCRIPT
// https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
// https://docs.unity3d.com/ScriptReference/Transform.Translate.html
// https://docs.unity3d.com/ScriptReference/Input.GetKeyDown.html
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/nullable-value-types