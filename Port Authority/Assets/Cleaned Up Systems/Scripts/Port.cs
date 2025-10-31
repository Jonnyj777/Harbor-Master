using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Port : MonoBehaviour
{
    public List<Cargo> portCargo = new List<Cargo>();
    public List<GameObject> cargoBoxes = new List<GameObject>();

    public GameObject loadingArea;
    public GameObject cargoPrefab;

    private float spawnOffset = 3f;
    private Vector3 minBounds;
    private Vector3 maxBounds;

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
        portCargo.Add(cargo);
        SpawnCargoBox(cargo);
    }

    private void SpawnCargoBox(Cargo cargo)
    {
        Vector3 spawnPos = new Vector3(Random.Range(minBounds.x + spawnOffset, maxBounds.x - spawnOffset), maxBounds.y + spawnOffset, Random.Range(minBounds.z + spawnOffset, maxBounds.z - spawnOffset));

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
        int index = portCargo.IndexOf(cargo);

        if (index >= 0 )
        {
            portCargo.RemoveAt(index);
            GameObject obj = cargoBoxes[index];
            cargoBoxes.RemoveAt(index);
            Destroy(obj);
        }
    }
}
