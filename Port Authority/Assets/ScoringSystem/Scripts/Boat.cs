using UnityEngine;
using System.Collections.Generic;

public class Boat : MonoBehaviour
{
    public List<Cargo> cargo = new List<Cargo>();
    private Port port;
    public GameObject cargoObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Port")) {
            LineFollower vehicle = GetComponent<LineFollower>();
            vehicle.atPort = true;
            vehicle.drawControl.DeleteLine();
            port = other.GetComponent<Port>();
            DeliverCargo();
            vehicle.pathFinding = false;
            transform.Rotate(0f, 180f, 0f);
        }
    }

    void DeliverCargo()
    {
        if (cargo.Count > 0)
        {
            port.ReceiveCargo(cargo);
            cargo.Clear();
            cargoObject.SetActive(false);
        }
    }
}
