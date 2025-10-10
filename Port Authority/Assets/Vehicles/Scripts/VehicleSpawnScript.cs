using Mirror;
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

    // World Bounds
    private float minX, maxX, minZ, maxZ;
    private float waterLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Calculate the world bounds after the game bounds object exists
        GameObject terrain = GameObject.Find("TerrainGenerator");
        GameObject water = GameObject.Find("WaterPlane");
        Renderer terrainRenderer = terrain.GetComponent<Renderer>();
        waterLevel = water.transform.position.y;

        if (terrainRenderer != null)
        {
            Bounds bounds = terrainRenderer.bounds;

            minX = bounds.min.x;
            maxX = bounds.max.x;
            minZ = bounds.min.z;
            maxZ = bounds.max.z;
        }
        Debug.Log("MinX: " + minX + "; " + "MaxX: " + maxX + "; " + "MinZ: " + minZ + "; " + "MaxZ: " + maxZ + "; ");

        // Spawn a boat immediately
        //spawnVehicle();
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

    [Server]
    void spawnVehicle()
    {
        Vector3 spawnPos = Vector3.zero;
        Vector3 shipDirection = Vector3.zero;

        // Spawn from a random side of the screen
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // Left
                spawnPos = new Vector3(minX - spawnMargin, waterLevel + spawnHeightOffset, Random.Range(minZ + cornerMargin, maxZ - cornerMargin));
                shipDirection = Vector3.right;
                break;
            case 1: // Right
                spawnPos = new Vector3(maxX + spawnMargin, waterLevel + spawnHeightOffset, Random.Range(minZ + cornerMargin, maxZ - cornerMargin));
                shipDirection = Vector3.left;
                break;
            case 2: // Bottom
                spawnPos = new Vector3(Random.Range(minX + cornerMargin, maxX - cornerMargin), waterLevel + spawnHeightOffset, minZ - spawnMargin);
                shipDirection = Vector3.forward;
                break;
            case 3: // Top
                spawnPos = new Vector3(Random.Range(minX + cornerMargin, maxX - cornerMargin), waterLevel + spawnHeightOffset, maxZ + spawnMargin);
                shipDirection = Vector3.back;
                break;
        }

        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-maxSpawnAngle, maxSpawnAngle), 0);
        GameObject spawnedShip = Instantiate(ship, spawnPos, rotation);
        NetworkServer.Spawn(spawnedShip);
    }
}
