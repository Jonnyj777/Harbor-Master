using Mirror;
using System.Collections.Generic;
using UnityEngine;


public class VehicleSpawnScriptN : NetworkBehaviour
{

    [Header("Boat Prefabs")]
    public List<GameObject> allShipPrefabs;


    private SyncList<GameObject> unlockedShipPrefabs = new SyncList<GameObject>();

    [Header("Spawn Location")]
    public float spawnMargin = 0f;
    public float cornerMargin = 10f;
    public float maxSpawnAngle = 15f;
    public float spawnHeightOffset = 5f;

    [Header("Spawn Rate")]
    public float spawnRate = 10f;
    public float difficultyIncreaseRate = 60f;
    private float minSpawnRate;
    private float currentSpawnInterval;
    private float spawnTimer = 0f;
    private float gameTimer = 0f;

    [Header("World")]
    [SerializeField] private IslandPlacerN islandPlacer;
    private Vector3 gridCenter;
    private float waterLevel = 1.27f;   // Hard-coded value (Potentially Dangerous!)
    private float minX, maxX, minZ, maxZ;

    void Start()
    {
        // Unlock the first/default boat
        if (allShipPrefabs.Count > 0)
        {
            unlockedShipPrefabs.Add(allShipPrefabs[0]);
        }

        if (islandPlacer != null)
        {
            Bounds gridBounds = islandPlacer.GetGridBounds();
            gridCenter = gridBounds.center;

            minX = gridBounds.min.x;
            maxX = gridBounds.max.x;
            minZ = gridBounds.min.z;
            maxZ = gridBounds.max.z;
        }
        minSpawnRate = spawnRate - 2f;
        currentSpawnInterval = Random.Range(minSpawnRate, spawnRate);

        //Debug.Log("MinX: " + minX + "; " + "MaxX: " + maxX + "; " + "MinZ: " + minZ + "; " + "MaxZ: " + maxZ + "; ");
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        gameTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnVehicle();
            spawnTimer = 0f;

            // Pick a new random delay for the next spawn
            currentSpawnInterval = Random.Range(minSpawnRate, spawnRate);
        }

        // Increase game difficulty every minute
        if (gameTimer >= difficultyIncreaseRate)
        {
            gameTimer = 0;

            if (minSpawnRate > 1)
            {
                minSpawnRate--;
            }

            if (spawnRate > 1)
            {
                spawnRate--;
            }
        }

    }

    /*
    [Server]
    void spawnVehicle()
    {
        Vector3 spawnPos = validSpawnLocations[Random.Range(0, validSpawnLocations.Count)];
        spawnPos.y = waterLevel + spawnHeightOffset;

        Vector3 shipDirection = (terrainCenter - spawnPos).normalized;

        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-maxSpawnAngle, maxSpawnAngle), 0);

        GameObject spawnedShip = Instantiate(unlockedShipPrefabs[Random.Range(0, unlockedShipPrefabs.Count)], spawnPos, new Quaternion(0, rotation.y, rotation.z, rotation.w));
        NetworkServer.Spawn(spawnedShip);
    }
    */

    [Server]
    void spawnVehicle()
    {
        // Choose a random side of the screen to spawn
        int side = Random.Range(0, 4);
        float xPos = 0f, zPos = 0f;

        // Random positions within corner margins, before side margin adjustments
        float randXPos = Random.Range(minX + cornerMargin, maxX - cornerMargin);
        float randZPos = Random.Range(minZ + cornerMargin, maxZ - cornerMargin);

        switch (side)
        {
            case 0: // Left
                xPos = minX - spawnMargin;
                zPos = randZPos;
                break;
            case 1: // Right
                xPos = maxX + spawnMargin;
                zPos = randZPos;
                break;
            case 2: // Bottom
                xPos = randXPos;
                zPos = minZ - spawnMargin;
                break;
            case 3: // Top
                xPos = randXPos;
                zPos = maxZ + spawnMargin;
                break;
        }

        Vector3 spawnPos = new Vector3(xPos, waterLevel + spawnHeightOffset, zPos);
        Vector3 shipDirection = (gridCenter - spawnPos).normalized;
        shipDirection.y = 0f;

        //RpcPlayAudio();
        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-maxSpawnAngle, maxSpawnAngle), 0);

       GameObject ship = Instantiate(unlockedShipPrefabs[Random.Range(0, unlockedShipPrefabs.Count)], spawnPos, rotation);
        NetworkServer.Spawn(ship);
    }

    [ClientRpc]
    void RpcPlayAudio()
    {
        AudioManager.Instance.PlayBoatEntrance();
    }

    [Server]
    public void UnlockShip(GameObject ship)
    {
        if (!unlockedShipPrefabs.Contains(ship))
        {
            unlockedShipPrefabs.Add(ship);
        }
    }

}
