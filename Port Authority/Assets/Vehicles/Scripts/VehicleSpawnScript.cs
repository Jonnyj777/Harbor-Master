using System.Collections.Generic;
using UnityEngine;


public class VehicleSpawnScript : MonoBehaviour
{   
    public GameObject ship;
    public float spawnRate = 5f;
    public float spawnMargin = 0f;
    public float cornerMargin = 10f;
    public float maxSpawnAngle = 15f;
    public float spawnHeightOffset = 5f;
    private float timer = 0f;

    // World Positions
    private Vector3 terrainCenter;
    private float waterLevel;

    // Terrain vertices where terrainHeight <= waterLevel
    private List<Vector3> validSpawnLocations;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

        //Debug.Log("MinX: " + minX + "; " + "MaxX: " + maxX + "; " + "MinZ: " + minZ + "; " + "MaxZ: " + maxZ + "; ");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            spawnVehicle();
            timer = 0f;
        }
    }

    void spawnVehicle()
    {
        Vector3 spawnPos = validSpawnLocations[Random.Range(0, validSpawnLocations.Count)];
        spawnPos.y = waterLevel + spawnHeightOffset;

        Vector3 shipDirection = (terrainCenter - spawnPos).normalized;

        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-maxSpawnAngle, maxSpawnAngle), 0);

        Instantiate(ship, spawnPos, rotation);

        // TODO: Delete old logic once new logic has been reviewed.
        //Vector3 spawnPos = Vector3.zero;
        //Vector3 shipDirection = Vector3.zero;

        //// Spawn from a random side of the screen
        //int side = Random.Range(0, 4);

        //switch (side)
        //{
        //    case 0: // Left
        //        spawnPos = new Vector3(minX - spawnMargin, waterLevel + spawnHeightOffset, Random.Range(minZ + cornerMargin, maxZ - cornerMargin));
        //        shipDirection = Vector3.right;
        //        break;
        //    case 1: // Right
        //        spawnPos = new Vector3(maxX + spawnMargin, waterLevel + spawnHeightOffset, Random.Range(minZ + cornerMargin, maxZ - cornerMargin));
        //        shipDirection = Vector3.left;
        //        break;
        //    case 2: // Bottom
        //        spawnPos = new Vector3(Random.Range(minX + cornerMargin, maxX - cornerMargin), waterLevel + spawnHeightOffset, minZ - spawnMargin);
        //        shipDirection = Vector3.forward;
        //        break;
        //    case 3: // Top
        //        spawnPos = new Vector3(Random.Range(minX + cornerMargin, maxX - cornerMargin), waterLevel + spawnHeightOffset, maxZ + spawnMargin);
        //        shipDirection = Vector3.back;
        //        break;
        //}
    }
}
