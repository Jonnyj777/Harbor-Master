using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class VehicleSpawnScript : MonoBehaviour
{
    [Header("Boat Prefabs")]
    public List<GameObject> allShipPrefabs;
    private List<GameObject> unlockedShipPrefabs = new List<GameObject>();

    [Header("Technicals")]
    public float spawnRate = 10f;
    private float minSpawnRate;
    private float currentSpawnInterval;
    public float difficultyIncreaseRate = 60f;
    public float spawnMargin = 0f;
    public float cornerMargin = 10f;
    public float maxSpawnAngle = 15f;
    public float spawnHeightOffset = 5f;
    private float spawnTimer = 0f;
    private float gameTimer = 0f;

    // World Positions
    private Vector3 terrainCenter;
    private float waterLevel;

    // Terrain vertices where terrainHeight <= waterLevel
    private List<Vector3> validSpawnLocations;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Unlock the first/default boat
        if (allShipPrefabs.Count > 0)
        {
            unlockedShipPrefabs.Add(allShipPrefabs[0]);
        }

        // Calculate the world bounds after the game bounds object exists
        GameObject terrain = GameObject.Find("TerrainGenerator");
        MeshFilter terrainMeshFilter = terrain.GetComponent<MeshFilter>();
        Bounds terrainMeshBounds = terrainMeshFilter.mesh.bounds;
        Vector3 terrainUnscaledSize = terrainMeshBounds.size;
        Vector3 terrainScaledSize = Vector3.Scale(terrainUnscaledSize, terrain.transform.localScale);

        float centerX = terrain.transform.position.x + terrainScaledSize.x / 2;
        float centerZ = terrain.transform.position.z + terrainScaledSize.z / 2;
        terrainCenter = new Vector3(centerX, 0, centerZ);

        // Retrieve valid spawn locations
        NoiseToTerrainGenerator terrainGenerator = terrain.GetComponent<NoiseToTerrainGenerator>();
        validSpawnLocations = terrainGenerator.GetOceanEdgeVertices();

        // Retrieve the water level for boats
        waterLevel = terrainGenerator.GetWaterLevel();

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

    void spawnVehicle()
    {
        Vector3 top = new Vector3(500f, 0f, 1000f);
        Vector3 right = new Vector3(1000f, 0f, 500f);
        Vector3 left = new Vector3(0f, 0f, 500f);
        Vector3 bottom = new Vector3(500f, 0f, 0);

        // Each spawn point uses a base rotation of 0 degrees.
        List<Vector3> spawnPoints = new()
        {
            right,
            left,
            bottom,
            top
        };
        List<int> rotations = new()
        {
            270, //right
            90, //left
            0, //bottom
            180 //top
        };
        int index = Random.Range(0, spawnPoints.Count);
        Vector3 spawnPos = spawnPoints[index];
        spawnPos.y = waterLevel + spawnHeightOffset;

        // Base rotation is 0 for all spawn points; only a small random offset is applied.
        Quaternion rotation = Quaternion.Euler(0f, rotations[index], 0f);

        AudioManager.Instance.PlayBoatEntrance();
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
