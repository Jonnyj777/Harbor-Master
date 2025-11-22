using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Mirror.BouncyCastle.Asn1.Cmp.Challenge;

public class BoatN : NetworkBehaviour
{
    [Header("Cargo Prefabs")]
    public List<GameObject> cargoBoxes;

    private List<CargoN> unlockedCargo = new List<CargoN>();
    private PortN port;

    [Header("Boat Collisions Settings")]
    public Color crashedColor = Color.cyan;  // Color to when boat vehicles crash

    private bool hasCrashed = false;
    private float sinkDelay = 2f;
    private float sinkLength = 10f;  // distance the boat sinks down
    private float sinkDuration = 2f;  // time it takes to sink down to the desired length
    //private float fadeDelay = 1f;  // time to wait before fading starts
    //private float fadeDuration = 5f;  // how long to fully fade out

    [Header("Instance Settings")]
    private LineFollowN vehicle;
    private List<Renderer> vehiclePartRenderers = new List<Renderer>();
    private float minX, maxX, minZ, maxZ;   // World bounds

    [Header("Boat Snapping")]
    public float dockingTime = 1.5f;
    public float rotationSmooth = 10f;
    public Renderer rend;
    private Transform dockEndPoint;
    private float t = 0f;
    private Vector3 p0, p1, p2;
    private bool isDelivering = false;
    private float boatLength;

    private void Start()
    {
        print("spawned");
        foreach (Renderer vehiclePartRenderer in GetComponentsInChildren<Renderer>())
        {
            if (vehiclePartRenderer.CompareTag("Boat"))
            {
                vehiclePartRenderers.Add(vehiclePartRenderer);
            }
        }

        if (isServer)
        {
            //AssignCargo();
            //vehicle = GetComponent<LineFollow>();
            //vehicleRenderer = GetComponent<Renderer>();
            AssignCargo();

            vehicle = GetComponent<LineFollowN>();
            //vehicleRenderer = GetComponent<Renderer>();
            /*
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
            */

            minX = 0;
            maxX = 1000;
            minZ = 0;
            maxZ = 1000;

            // Get boat size
            boatLength = rend.bounds.size.z;
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
        bool multipleCollisions = true;
        if (other.CompareTag("Terrain"))
        {
            multipleCollisions = false;
            EnterCrashState(multipleCollisions);
        }
        else if (other.CompareTag("Boat"))
        {
            multipleCollisions = true;
            if (GetInstanceID() < other.GetInstanceID())
            {
                multipleCollisions = false;
            }
            EnterCrashState(multipleCollisions);
        }
        else if (other.CompareTag("Port"))
        {
            vehicle.SetAtPort(true);
            vehicle.DeleteLine();
            port = other.GetComponent<PortN>();
            //DeliverCargo();
            StartCoroutine(ParkBoatAndDeliver());
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

        CargoN c = cargoBoxes[cargoIndex].AddComponent<CargoN>();
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

        StartCoroutine(DelayCargo());
        RpcPlayAudio("boat-delivery");
        
       
    }

    [Server]
    private IEnumerator DelayCargo()
    {
        List<CargoN> cargo = new List<CargoN>();

        foreach (GameObject gameObject in cargoBoxes)
        {
            if (!gameObject.activeSelf) continue;
            if (gameObject.TryGetComponent<CargoN>(out CargoN c))
            {
                cargo.Add(c);
            }
        }

        print("delivering cargo num: " + cargo.Count);
        for (int i = 0; i < cargo.Count; i++)
        {
            yield return new WaitForSeconds(vehicle.delayPerCargo);
            RpcDeactivateCargo(i);
            port.ReceiveCargoBox(cargo[i]);
            cargoBoxes[i].SetActive(false);
        }
    }

    [ClientRpc]
    void RpcPlayAudio(string audioType)
    {
        switch(audioType)
        {
            case "boat-delivery":
                AudioManager.Instance.PlayBoatDelivery();
                break;
            case "boat-collision":
                AudioManager.Instance.PlayBoatCollision();
                break;
        }
    }

    [Server]
    public void EnterCrashState(bool multipleCollisions)
    {
        // Prevent multiple triggers
        if (hasCrashed)
        {
            return;
        }

        if (!multipleCollisions)
        {
            // Only trigger the collision sound once
            RpcPlayAudio("boat-collision");
        }
        hasCrashed = true;

        LivesManagerN.Instance.LoseLife();

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

    [Server]
    private IEnumerator ParkBoatAndDeliver()
    {
        yield return ParkBoat();
        DeliverCargo();

    }

    [Server]
    private IEnumerator ParkBoat()
    {
        t = 0f;
        dockEndPoint = port.endPoint;

        Vector3 p0 = transform.position;
        Vector3 p2 = dockEndPoint.position - dockEndPoint.forward * (boatLength / 2f);
        p2.y = p0.y;

        while (true)
        {
            if (hasCrashed)
                yield break;

            t += Time.deltaTime / dockingTime;
            float easedT = Mathf.SmoothStep(0, 1, t);

            transform.position = Vector3.Lerp(p0, p2, easedT);

            Quaternion targetRot = Quaternion.LookRotation(dockEndPoint.forward);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSmooth * Time.deltaTime
            );

            if (t >= 1f)
            {
                transform.position = p2;
                transform.rotation = dockEndPoint.rotation;
                break;
            }

            yield return null;
        }

        transform.Rotate(0f, 180f, 0f);
    }
}
