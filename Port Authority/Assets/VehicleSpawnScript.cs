using UnityEngine;


public class VehicleSpawnScript : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject ship;
    public float spawnRate = 5f;
    public float spawnMargin = 2f;
    private float timer = 0f;

    // World Bounds
    private float minX, maxX, minZ, maxZ;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Calculate the world bounds after the camera exists
        Vector3 worldBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.transform.position.y));
        Vector3 worldTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.transform.position.y));

        minX = worldBottomLeft.x;
        maxX = worldTopRight.x;
        minZ = worldBottomLeft.z;
        maxZ = worldTopRight.z;
        Debug.Log("MinX: " + minX + "; " + "MaxX: " + maxX + "; " + "MinZ: " + minZ + "; " + "MaxZ: " + maxZ + "; ");

        // Spawn a boat immediately
        spawnVehicle();
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
        Vector3 spawnPos = Vector3.zero;
        Vector3 shipDirection = Vector3.zero;

        // Spawn from a random side of the screen
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // Left
                spawnPos = new Vector3(minX - spawnMargin, 0, Random.Range(minZ, maxZ));
                shipDirection = Vector3.right;
                break;
            case 1: // Right
                spawnPos = new Vector3(maxX + spawnMargin, 0, Random.Range(minZ, maxZ));
                shipDirection = Vector3.left;
                break;
            case 2: // Bottom
                spawnPos = new Vector3(Random.Range(minX, maxX), 0, minZ - spawnMargin);
                shipDirection = Vector3.forward;
                break;
            case 3: // Top
                spawnPos = new Vector3(Random.Range(minX, maxX), 0, maxZ + spawnMargin);
                shipDirection = Vector3.back;
                break;
        }

        Quaternion rotation = Quaternion.LookRotation(shipDirection);
        rotation *= Quaternion.Euler(0, Random.Range(-3f, 3f), 0);

        Instantiate(ship, spawnPos, rotation);
    }
}
