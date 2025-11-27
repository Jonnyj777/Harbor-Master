using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class VehicleSpawnScript : MonoBehaviour
{
    [Header("Boat Prefabs")]
    public List<GameObject> allShipPrefabs;
    private List<GameObject> unlockedShipPrefabs = new List<GameObject>();

    [Header("Spawn Location")]
    public float spawnMargin = 0f;
    public float cornerMargin = 10f;
    public float maxSpawnAngle = 15f;
    public float spawnHeightOffset = 5f;

    [Header("Spawn Rate")]
    public float spawnRate = 10f;
    public float difficultyIncreaseRate = 60f;
    private SpawnSchedulerLogic spawnScheduler;
    private readonly IRandomProvider randomProvider = new UnityRandomProvider();

    [Header("World")]
    [SerializeField] private IslandPlacer islandPlacer;
    private Vector3 gridCenter;
    private float waterLevel = 1.27f;   // Hard-coded value (Potentially Dangerous!)
    private float minX, maxX, minZ, maxZ;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        //Debug.Log("MinX: " + minX + "; " + "MaxX: " + maxX + "; " + "MinZ: " + minZ + "; " + "MaxZ: " + maxZ + "; ");

        spawnScheduler = new SpawnSchedulerLogic(spawnRate, difficultyIncreaseRate, randomProvider);
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnScheduler != null && spawnScheduler.Tick(Time.deltaTime))
        {
            spawnVehicle();
        }

        if (spawnScheduler != null)
        {
            spawnRate = spawnScheduler.CurrentSpawnRate;
        }
    }

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

        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-maxSpawnAngle, maxSpawnAngle), 0);

        Instantiate(unlockedShipPrefabs[Random.Range(0, unlockedShipPrefabs.Count)], spawnPos, rotation);
    }

    public void UnlockShip(GameObject ship)
    {
        if (!unlockedShipPrefabs.Contains(ship))
        {
            unlockedShipPrefabs.Add(ship);
        }
    }
}
