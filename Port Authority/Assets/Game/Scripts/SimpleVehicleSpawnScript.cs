using System.Collections.Generic;
using UnityEngine;

public class SimpleVehicleSpawnScript : MonoBehaviour
{
    [Header("Boat Prefabs")]
    public List<GameObject> allShipPrefabs;
    private List<GameObject> unlockedShipPrefabs = new List<GameObject>();

    [Header("Technicals")]
    public float spawnRate = 10f;
    public float difficultyIncreaseRate = 60f;
    public float spawnMargin = 0f;
    public float cornerMargin = 10f;
    public float maxSpawnAngle = 15f;
    public float spawnHeightOffset = 5f;
    private SpawnSchedulerLogic spawnScheduler;
    private readonly IRandomProvider randomProvider = new UnityRandomProvider();

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

        spawnScheduler = new SpawnSchedulerLogic(spawnRate, difficultyIncreaseRate, randomProvider);
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnScheduler != null && spawnScheduler.Tick(Time.deltaTime))
        {
            spawnVehicle();
        }

        // keep serialized values in sync with logic for debugging/inspector purposes
        if (spawnScheduler != null)
        {
            spawnRate = spawnScheduler.CurrentSpawnRate;
        }
    }

    void spawnVehicle()
    {
        Vector3 spawnPos = validSpawnLocations[Random.Range(0, validSpawnLocations.Count)];
        spawnPos.y = waterLevel + spawnHeightOffset;

        Vector3 shipDirection = (terrainCenter - spawnPos).normalized;
        shipDirection.y = 0f;
        shipDirection.Normalize();

        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-maxSpawnAngle, maxSpawnAngle), 0);

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
