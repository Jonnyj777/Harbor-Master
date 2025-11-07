using System.Collections; 
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Truck : MonoBehaviour
{
    public List<Cargo> cargo = new List<Cargo>();
    private Port port;
    public List<GameObject> cargoBoxes;
    private bool stallPortMovement = false;
    private bool stallBuildingMovement = false;

    LineFollow vehicle;
    public Material originalMaterial;
    public Material crashedMaterial;
    public Color crashedColor = Color.magenta;  // Color to when truck vehicles crash
    private Renderer vehicleRenderer;
    public static float baseRestartDelay = 15f;
    public static float globalRestartDelay; // The current effective value (changes when upgraded)

    public GameObject repairButtonPrefab;
    public Canvas trucksUICanvas;
    private GameObject repairButtonInstance;

    private void Awake()
    {
        if (globalRestartDelay == 0)
        {
            globalRestartDelay = baseRestartDelay;
        }
    }

    private void Start()
    {
        vehicle = GetComponent<LineFollow>();
        vehicleRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // land vehicle crash state
        // if land vehicle collides into another land vehicle, both land vehicles enter land crash state
        // land crash state == stuck in place, no fade out
        if (other.CompareTag("Truck"))
        {
            EnterCrashState();
        }
        if (other.CompareTag("Port") && cargo.Count <= 3)
        {
            port = other.GetComponent<Port>();
            if (!stallPortMovement)
            {
                StopMovement();
                stallPortMovement = true;
            }
            if (cargo.Count < cargoBoxes.Count && port.portCargo.Count > 0)
            {
                PickUpCargo();
            }
        }
        if (other.CompareTag("Building"))
        {
            if (!stallBuildingMovement)
            {
                StopMovement();
                stallBuildingMovement = true;
            }
            DeliverCargo();
        }
    }

    private void StopMovement()
    {
        LineFollow vehicle = GetComponent<LineFollow>();
        vehicle.SetAtPort(true);
        vehicle.DeleteLine();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Port"))
        {
            port = other.GetComponent<Port>();
            if (cargo.Count < cargoBoxes.Count && port.portCargo.Count > 0)
            {
                PickUpCargo();
            }
        }
        if (other.CompareTag("Building"))
        {
            DeliverCargo();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (stallPortMovement)
        {
            StartCoroutine(PortMovementDelay(2f));
        }
        if (stallBuildingMovement)
        {
            StartCoroutine(BuildingMovementDelay(2f));
        }
    }

    private IEnumerator PortMovementDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        stallPortMovement = false;
        
    }

    private IEnumerator BuildingMovementDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        stallBuildingMovement = false;
    }

    private void DeliverCargo()
    {
        if (cargo.Count > 0)
        {
            int scoreUpdate = 0;
            bool bonus = false;

            for (int i = 0; i < cargoBoxes.Count; i++)
            {
                cargoBoxes[i].SetActive(false);
            }

            foreach (Cargo c in cargo)
            {
                float elapsedTime = Time.time - c.spawnTime; 
                float bonusMultiplier = 1f;

                if (elapsedTime <= c.timeLimit)
                {
                    bonusMultiplier = 1.25f;
                    bonus = true;
                }

                scoreUpdate += Mathf.RoundToInt(c.price * c.amount * bonusMultiplier);
            }

            AudioManager.Instance.PlayTruckDelivery();
            ScoreManager.Instance.AddScore(scoreUpdate, bonus);
            cargo.Clear();
        }
    }

    private void PickUpCargo()
    {
        if (cargo.Count >= cargoBoxes.Count)
        {
            return;
        }

        if (port.portCargo.Count > 0)
        {
            List<Cargo> tempCargo = new List<Cargo>(port.portCargo);
            int slotsAvailable = cargoBoxes.Count - cargo.Count;
            int pickUpAmount = Mathf.Min(slotsAvailable, tempCargo.Count);

            for (int i = 0; i < pickUpAmount; i++)
            {

                int boxIndex = cargo.Count;
                cargo.Add(tempCargo[i]);
                port.RemoveCargoBox(tempCargo[i]);
                GameObject box = cargoBoxes[boxIndex];
                box.SetActive(true);

                Renderer r = box.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material.color = tempCargo[i].color;
                }
            }
        }
    }

    public void EnterCrashState()
    {
        vehicle.SetIsCrashed(true);
        ShowRepairButton();

        if (vehicleRenderer != null)
        {
            vehicleRenderer.material = crashedMaterial;
            vehicleRenderer.material.color = crashedColor;
        }
    }

    private void ShowRepairButton()
    {
        repairButtonInstance = Instantiate(repairButtonPrefab, trucksUICanvas.transform);

        // Position the repair button at the truck's position + offset
        Vector3 buttonPosition = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
        repairButtonInstance.transform.position = buttonPosition;

        // Add a listener to the button
        Button button = repairButtonInstance.GetComponent<Button>();
        button.onClick.AddListener(RepairTruck);
    }

    public void RepairTruck()
    {
        int repairCost = 50;
        if (ScoreManager.Instance.GetSpendableScore() >= repairCost)
        {
            StartCoroutine(RestoreMaterialAfterDelay());
            ScoreManager.Instance.UpdateSpendableScore(-repairCost);
            Destroy(repairButtonInstance);
        }
    }

    private IEnumerator RestoreMaterialAfterDelay()
    {
        yield return new WaitForSeconds(globalRestartDelay);

        vehicleRenderer.material = originalMaterial;
        vehicle.SetIsCrashed(false);
    }
}
