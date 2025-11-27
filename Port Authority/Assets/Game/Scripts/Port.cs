using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Port : MonoBehaviour
{
    public List<Cargo> portCargo = new List<Cargo>();
    public List<GameObject> cargoBoxes = new List<GameObject>();

    public GameObject loadingArea;
    public GameObject cargoPrefab;

    public Transform startPoint;
    public Transform endPoint;

    private float spawnOffset = 3f;
    private Vector3 minBounds;
    private Vector3 maxBounds;
    private readonly PortCargoLogic cargoLogic = new PortCargoLogic();
    private readonly IRandomProvider randomProvider = new UnityRandomProvider();

    private void Start()
    {
        Renderer loadingAreaRend = loadingArea.GetComponent<Renderer>();
        minBounds = loadingAreaRend.bounds.min;
        maxBounds = loadingAreaRend.bounds.max;

        Renderer cargoRend = cargoPrefab.GetComponent<Renderer>();
        spawnOffset = cargoRend.bounds.size.y;
    }

    public void ReceiveCargoBox(Cargo cargo)
    {
        cargoLogic.ReceiveCargo(portCargo, cargo);
        SpawnCargoBox(cargo);
    }

    private void SpawnCargoBox(Cargo cargo)
    {
        Vector3 spawnPos = cargoLogic.ComputeSpawnPosition(minBounds, maxBounds, spawnOffset, randomProvider);

        GameObject box = Instantiate(cargoPrefab, spawnPos, Quaternion.identity);
        Renderer rend = box.GetComponent<Renderer>();

        if (rend != null)
        {
            rend.material.color = cargo.color;
        }

        cargoBoxes.Add(box);
    }

    public void RemoveCargoBox(Cargo cargo)
    {
        if (cargoLogic.TryRemoveCargo(portCargo, cargo, out int index))
        {
            GameObject obj = cargoBoxes[index];
            cargoBoxes.RemoveAt(index);
            Destroy(obj);
        }
    }
}
