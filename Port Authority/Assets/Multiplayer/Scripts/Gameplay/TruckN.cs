using Mirror;
using System.Collections; 
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TruckN : NetworkBehaviour
{
    public List<Cargo> cargo = new List<Cargo>();
    private PortN port;
    public List<GameObject> cargoBoxes;
    private bool stallPortMovement = false;
    private bool stallBuildingMovement = false;

    LineFollowN vehicle;
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
        if(isServer)
        {
            vehicle = GetComponent<LineFollowN>();
            vehicleRenderer = GetComponent<Renderer>();
        }
    }

    [ServerCallback]
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
            port = other.GetComponent<PortN>();
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

    [Server]
    private void StopMovement()
    {
        LineFollowN vehicle = GetComponent<LineFollowN>();
        vehicle.SetAtPort(true);
        vehicle.DeleteLine();
    }

    [ServerCallback]
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Port"))
        {
            port = other.GetComponent<PortN>();
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

    [ServerCallback]
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

    [Server]
    private IEnumerator PortMovementDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        stallPortMovement = false;
        
    }

    [Server]
    private IEnumerator BuildingMovementDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        stallBuildingMovement = false;
    }

    [Server]
    private void DeliverCargo()
    {
        if (cargo.Count > 0)
        {
            int scoreUpdate = 0;
            bool bonus = false;

            for (int i = 0; i < cargoBoxes.Count; i++)
            {
                RpcDeactivateCargo(i);
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

            ScoreManagerN.Instance.AddScore(scoreUpdate, bonus);
            cargo.Clear();
        }
    }

    [Server]
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
                //GameObject box = cargoBoxes[boxIndex];
                Color color = new Color(tempCargo[i].colorData.x, tempCargo[i].colorData.y, tempCargo[i].colorData.z);
                RpcActivateCargo(boxIndex, color);
            }
        }
    }

    [ClientRpc]
    private void RpcActivateCargo(int cargoIndex, Color color)
    {
        cargoBoxes[cargoIndex].SetActive(true);

        if (cargoBoxes[cargoIndex].TryGetComponent<Renderer>(out var r))
        {
            r.material.color = color;
        }
    }

    [ClientRpc]
    private void RpcDeactivateCargo(int cargoIndex)
    {
        cargoBoxes[cargoIndex].SetActive(false);
    }

    [Server]
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
        if (ScoreManagerN.Instance.GetSpendableScore() >= repairCost)
        {
            StartCoroutine(RestoreMaterialAfterDelay());
            ScoreManagerN.Instance.UpdateSpendableScore(-repairCost);
            Destroy(repairButtonInstance);
        }
    }

    [Server]
    private IEnumerator RestoreMaterialAfterDelay()
    {
        yield return new WaitForSeconds(globalRestartDelay);

        vehicleRenderer.material = originalMaterial;
        vehicle.SetIsCrashed(false);
    }
}
