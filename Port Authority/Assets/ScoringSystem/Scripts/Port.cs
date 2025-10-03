using UnityEngine;
using System.Collections.Generic;

public class Port : MonoBehaviour
{
    public List<Cargo> portCargo = new List<Cargo>();
    public GameObject cargoObject;

    public void ReceiveCargo(List<Cargo> cargo)
    {
        foreach(var c in cargo)
        {
            portCargo.Add(c);
            cargoObject.SetActive(true);
        }
    }
}
