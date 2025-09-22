using UnityEngine;

public class TestController : MonoBehaviour
{
    [Header("Assign Vehicles Here")]
    public VehicleMovement landVehicle1;
    public VehicleMovement landVehicle2;

    [Header("Testing Key")]
    public KeyCode testKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            if (landVehicle1 != null && landVehicle2 != null)
            {
                // set each vehicle's target to the other vehicle's current position
                landVehicle1.SetTarget(landVehicle2.transform.position);
                landVehicle2.SetTarget(landVehicle1.transform.position);
            }
        }

    }
}
