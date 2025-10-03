using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class Truck : MonoBehaviour
{
    public List<Cargo> cargo = new List<Cargo>();
    private Port port;
    public GameObject cargoObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Port"))
        {
            port = other.GetComponent<Port>();
            PickUpCargo();
        }
        if (other.CompareTag("Building"))
        {
            DeliverCargo();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Port"))
        {
            port = other.GetComponent<Port>();
            PickUpCargo();
        }
        if (other.CompareTag("Building"))
        {
            DeliverCargo();
        }
    }
    void DeliverCargo()
    {
        if (cargo.Count > 0)
        {
            int scoreUpdate = 0;
            bool bonus = false;

            foreach (Cargo c in cargo)
            {
                float elapsedTime = Time.time - c.spawnTime; 
                float bonusMultiplier = 1f;


                Debug.Log(elapsedTime + " " + c.timeLimit);

                if (elapsedTime <= c.timeLimit)
                {
                    bonusMultiplier = 1.25f;
                    bonus = true;
                }

                scoreUpdate += Mathf.RoundToInt(c.price * c.amount * bonusMultiplier);

                Debug.Log($"Delivered Cargo: {c.type}, Amount: {c.amount}, Base Price: {c.price}, " +
                      $"Elapsed Time: {elapsedTime:F2}s, Bonus Applied: {bonusMultiplier > 1}, " +
                      $"Score for this cargo: {scoreUpdate}");
            }



            ScoreManager.Instance.AddScore(scoreUpdate, bonus);
            cargo.Clear();

            cargoObject.SetActive(false);
        }
    }

    void PickUpCargo()
    {
        if (port.portCargo.Count > 0)
        {
            List<Cargo> cargoToTake = new List<Cargo>(port.portCargo);

            foreach (Cargo c in cargoToTake)
            {
                cargo.Add(c);
                port.portCargo.Remove(c);
                port.cargoObject.SetActive(false);
                cargoObject.SetActive(true);
            }
        }
    }
}
