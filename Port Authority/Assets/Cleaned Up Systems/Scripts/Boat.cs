using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boat : NetworkBehaviour
{
    [Header("Cargo Prefabs")]
    public List<GameObject> cargoBoxes;

    private List<Cargo> unlockedCargo = new List<Cargo>();
    private Port port;

    [Header("Boat Collisions Settings")]
    public Color crashedColor = Color.cyan;  // Color to when boat vehicles crash

    private bool hasCrashed = false;
    private float sinkDelay = 2f;
    private float sinkLength = 10f;  // distance the boat sinks down
    private float sinkDuration = 2f;  // time it takes to sink down to the desired length
    //private float fadeDelay = 1f;  // time to wait before fading starts
    //private float fadeDuration = 5f;  // how long to fully fade out

    [Header("Instance Settings")]
    private LineFollow vehicle;
    private List<Renderer> vehiclePartRenderers = new List<Renderer>();
    private float minX, maxX, minZ, maxZ;   // World bounds

    private void Start()
    {
        if (isServer)
        {
            //AssignCargo();
            //vehicle = GetComponent<LineFollow>();
            //vehicleRenderer = GetComponent<Renderer>();
            AssignCargo();

            vehicle = GetComponent<LineFollow>();
            //vehicleRenderer = GetComponent<Renderer>();
            foreach (Renderer vehiclePartRenderer in GetComponentsInChildren<Renderer>())
            {
                if (vehiclePartRenderer.CompareTag("Boat"))
                {
                    vehiclePartRenderers.Add(vehiclePartRenderer);
                }
            }

            // Teach the boat the world bounds so it can destroy itself
            GameObject terrain = GameObject.Find("TerrainGenerator");
            MeshFilter terrainMeshFilter = terrain.GetComponent<MeshFilter>();
            Bounds terrainMeshBounds = terrainMeshFilter.mesh.bounds;
            Vector3 terrainUnscaledSize = terrainMeshBounds.size;
            Vector3 terrainScaledSize = Vector3.Scale(terrainUnscaledSize, terrain.transform.localScale);

            minX = terrain.transform.position.x;
            maxX = minX + terrainScaledSize.x;
            minZ = terrain.transform.position.z;
            maxZ = minZ + terrainScaledSize.z;
        }
    }

    private void Update()
    {
        if(isServer)
        {
            CheckBounds();
        }
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Terrain") || other.CompareTag("Boat"))
        {
            EnterCrashState();
        }
        if (other.CompareTag("Port"))
        {
            vehicle.SetAtPort(true);
            vehicle.DeleteLine();
            port = other.GetComponent<Port>();
            DeliverCargo();
            transform.Rotate(0f, 180f, 0f);
        }
    }

    [Server]
    public void AssignCargo()
    {
        List<CargoType> unlockedCargoTypes = CargoManager.Instance.GetUnlockedCargo();
        if (unlockedCargoTypes.Count == 0)
        {
            return;
        }

        int cargoAmount = Random.Range(1, cargoBoxes.Count + 1);

        for (int i = 0; i < cargoBoxes.Count; i++)
        {
            if (i < cargoAmount)
            {
                cargoBoxes[i].SetActive(true);

                // Pick a random unlocked cargo type
                CargoType selectedCargoType = unlockedCargoTypes[Random.Range(0, unlockedCargoTypes.Count)];

                // Create a new Cargo instance
                RpcActivateCargo(i, selectedCargoType.color);
            }
            else
            {
                RpcDeactivateCargo(i);
            }
        }
    }

    [ClientRpc]
    private void RpcActivateCargo(int cargoIndex, Color randomColor)
    {
        cargoBoxes[cargoIndex].SetActive(true);
        Renderer rend = cargoBoxes[cargoIndex].GetComponent<Renderer>();
        rend.material.color = randomColor;

        Cargo c = cargoBoxes[cargoIndex].AddComponent<Cargo>();
        c.type = "Coffee";
        c.amount = 1;
        c.colorData = new Vector3(randomColor.r, randomColor.g, randomColor.b);
        c.spawnTime = Time.time;
        c.price = 20;
    }


    [ClientRpc]
    private void RpcDeactivateCargo(int cargoIndex)
    {
        cargoBoxes[cargoIndex].SetActive(false);
    }



    [Server]
    void DeliverCargo()
    {
        List<Cargo> cargo = new List<Cargo>();

        foreach(GameObject gameObject in cargoBoxes)
        {
            if (!gameObject.activeSelf) continue;
            if(gameObject.TryGetComponent<Cargo>(out Cargo c))
            {
                cargo.Add(c);
            }
        }

        if(cargo.Count > 0) { 
            port.ReceiveCargo(cargo);

            for(int i = 0; i < cargoBoxes.Count; i++)
            {
                RpcDeactivateCargo(i);
            }
        }
    }

    [Server]
    public void EnterCrashState()
    {
        // Prevent multiple triggers
        if (hasCrashed)
        {
            return;
        }
        hasCrashed = true;

        LivesManager.Instance.LoseLife();

        vehicle.SetIsCrashed(true);

        RpcEnterClientCrashState();

        StartCoroutine(SinkFadeOut());
    }

    [ClientRpc]
    void RpcEnterClientCrashState()
    {
        if (vehiclePartRenderers.Count != 0)
        {
            foreach (Renderer vehiclePartRenderer in vehiclePartRenderers)
            {
                foreach (Material mat in vehiclePartRenderer.materials)
                {
                    mat.color = crashedColor;
                }
            }
        }
    }


    [Server]
    // function to make boats sink, fade, then destroyed after crashing into another boat vehicle
    private IEnumerator SinkFadeOut()
    {
        // Wait before sinking
        yield return new WaitForSeconds(sinkDelay);

        // Sinking logic
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(startPos.x, startPos.y - sinkLength, startPos.z);
        float time = 0f;
        while (time < sinkDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, time / sinkDuration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        Destroy(gameObject);
    }

    private void CheckBounds()
    {
        Vector3 pos = transform.position;

        // Small buffer to prevent boats from being deleted too early if their model origin isn’t centered
        float buffer = 5f;

        if (pos.x < minX - buffer || pos.x > maxX + buffer ||
            pos.z < minZ - buffer || pos.z > maxZ + buffer)
        {
            DestroyBoatOutOfBounds();
        }
    }

    private void DestroyBoatOutOfBounds()
    {
        // Avoid duplicate calls if already crashing/sinking
        if (vehicle != null && hasCrashed)
        {
            return;
        }

        //Debug.Log($"Boat {name} went out of bounds and was destroyed.");
        Destroy(gameObject);
    }
}
