using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mirror;

public class Port : NetworkBehaviour
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

        //NetworkClient.RegisterPrefab(cargoPrefab);
    }

    [Server]
    public void ReceiveCargo(List<Cargo> cargo)
    {
        foreach (var c in cargo)
        {
            portCargo.Add(c);
            SpawnCargoBox(c);
        }
    }

    [Server]
    private void SpawnCargoBox(Cargo cargo)
    {
        Vector3 spawnPos = new Vector3(Random.Range(minBounds.x + spawnOffset, maxBounds.x - spawnOffset), maxBounds.y + spawnOffset, Random.Range(minBounds.z + spawnOffset, maxBounds.z - spawnOffset));
        GameObject box = Instantiate(cargoPrefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(box);
        Color color = new Color(cargo.colorData.x, cargo.colorData.y, cargo.colorData.z);
        print("box: " + box + " : color: " + color);
        //box.GetComponent<Renderer>().material.color = color;
        RpcAddCargo(box, color);
        cargoBoxes.Add(box);
    }

    [Server]
    public void RemoveCargoBox(Cargo cargo)
    {
        int index = portCargo.IndexOf(cargo);

        if (index >= 0)
        {
            portCargo.RemoveAt(index);
            GameObject obj = cargoBoxes[index];
            cargoBoxes.RemoveAt(index);
            Destroy(obj);
            NetworkServer.Destroy(obj);
        }
    }

    [ClientRpc]
    private void RpcAddCargo(GameObject box, Color color)
    {
        
        if (box.TryGetComponent<Renderer>(out var rend))
        {
            rend.material.color = color;
        }

    }
}

