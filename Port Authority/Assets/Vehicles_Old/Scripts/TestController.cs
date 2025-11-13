using UnityEngine;

public class TestController : MonoBehaviour
{
    [Header("Assign Vehicles Here")]
    public VehicleMovement landVehicle1;
    public VehicleMovement landVehicle2;
    public VehicleMovement landVehicle3;
    public VehicleMovement boatVehicle1;
    public VehicleMovement boatVehicle2;
    public VehicleMovement boatVehicle3;

    [Header("Testing Key")]
    public KeyCode landTestKey = KeyCode.L;
    public KeyCode land3TestKey = KeyCode.Alpha1;
    public KeyCode boatTestKey = KeyCode.B;
    public KeyCode boat3TestKey = KeyCode.Alpha2;

    void Update()
    {
        if (Input.GetKeyDown(landTestKey))
        {
            if (landVehicle1 != null && landVehicle2 != null)
            {
                // set each vehicle's target to the other vehicle's current position -> cause a crash
                landVehicle1.SetTarget(landVehicle2.transform.position);
                landVehicle2.SetTarget(landVehicle1.transform.position);
            }
        }

        if (Input.GetKeyDown(boatTestKey))
        {
            if (boatVehicle1 != null && boatVehicle2 != null)
            {
                // set each vehicle's target to the other vehicle's current position -> cause a crash
                boatVehicle1.SetTarget(boatVehicle2.transform.position);
                boatVehicle2.SetTarget(boatVehicle1.transform.position);
            }
        }

        if (Input.GetKeyDown(land3TestKey))
        {
            if (landVehicle3 != null)
            {
                // move land vehicle down to crash into crashed land vehicles
                landVehicle3.SetTarget(new Vector3(0, 1, 0));
            }
        }

        if (Input.GetKeyDown(boat3TestKey))
        {
            if (boatVehicle3 != null)
            {
                // move boat vehicle down to crash into crashed boat vehicles
                boatVehicle3.SetTarget(new Vector3(0, 0.5f, 0));
            }
        }

    }
}
