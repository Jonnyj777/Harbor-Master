using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Mirror;
using UnityEngine.UIElements;

public class PortN : NetworkBehaviour
{
    public List<CargoN> portCargo = new List<CargoN>();
    public List<GameObject> cargoBoxes = new List<GameObject>();

    public GameObject loadingArea;
    public GameObject cargoPrefab;

    public Transform startPoint;
    public Transform endPoint;

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

        NetworkClient.RegisterPrefab(cargoPrefab);
    }

    [Server]
    public void ReceiveCargoBox(CargoN c)
    {
        portCargo.Add(c);
        SpawnCargoBox(c);
    }

    [Server]
    private void SpawnCargoBox(CargoN cargo)
    {
        print("spawning cargo");
        Vector3 spawnPos = new Vector3(Random.Range(minBounds.x + spawnOffset, maxBounds.x - spawnOffset), maxBounds.y + spawnOffset, Random.Range(minBounds.z + spawnOffset, maxBounds.z - spawnOffset));
        GameObject box = Instantiate(cargoPrefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(box);
        //Color color = new Color(cargo.colorData.x, cargo.colorData.y, cargo.colorData.z);
        cargoBoxes.Add(box);

        if(box.TryGetComponent<CargoMaterialSetter>(out CargoMaterialSetter c))
        {
            c.colorData = cargo.colorData;
        }
    }


    [Server]
    private IEnumerator DelayColorRpc(GameObject box, Color color)
    {
        yield return new WaitForEndOfFrame();
        RpcAddCargoColor(box, color);
    }

    [Server]
    public void RemoveCargoBox(CargoN cargo)
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
    private void RpcAddCargoColor(GameObject box, Color color)
    {
        
        if (box.TryGetComponent<Renderer>(out var rend))
        {
            rend.material.color = color;
        }

    }
}

