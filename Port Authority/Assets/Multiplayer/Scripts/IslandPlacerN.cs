using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class IslandPlacerN : NetworkBehaviour
{
    [SerializeField] private List<GameObject> islandPrefabs = new List<GameObject>();
    [SerializeField] private Vector2 gridSize = new Vector2(2f, 2f);
    [SerializeField] private Vector2 tileSize = new Vector2(500f, 500f);

    private readonly HashSet<int> availableIslandIndices = new HashSet<int>();

    [ServerCallback]
    private void Start()
    {
        if (islandPrefabs == null || islandPrefabs.Count == 0)
        {
            Debug.LogWarning("IslandPlacer requires at least one island prefab assigned.", this);
            return;
        }

        InitializeIndices();
        SpawnIslands();
        GetGridBounds();
    }

    [Server]
    private void SpawnIslands()
    {
        int gridWidth = Mathf.Max(0, Mathf.RoundToInt(gridSize.x));
        int gridHeight = Mathf.Max(0, Mathf.RoundToInt(gridSize.y));

        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                GameObject prefab = GetIslandPrefab();
                if (prefab == null)
                {
                    Debug.LogWarning("IslandPlacer could not retrieve an island prefab.", this);
                    return;
                }

                GameObject island = Instantiate(prefab);
                island.transform.position = new Vector3(x * tileSize.x, 0f, z * tileSize.y);
                NetworkServer.Spawn(island);
                Debug.Log("Island PositionX: " + island.transform.position.x + " Island PositionZ: " + island.transform.position.z);
            }
        }
    }


    private void InitializeIndices()
    {
        availableIslandIndices.Clear();

        for (int i = 0; i < islandPrefabs.Count; i++)
            availableIslandIndices.Add(i);
    }

    private GameObject GetIslandPrefab()
    {
        if (availableIslandIndices.Count == 0)
        {
            InitializeIndices();
        }

        if (availableIslandIndices.Count == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, availableIslandIndices.Count);
        int selectedIndex = -1;
        int currentIndex = 0;

        foreach (int index in availableIslandIndices)
        {
            if (currentIndex == randomIndex)
            {
                selectedIndex = index;
                break;
            }

            currentIndex++;
        }

        if (selectedIndex == -1)
        {
            return null;
        }

        availableIslandIndices.Remove(selectedIndex);
        return islandPrefabs[selectedIndex];
    }

    public Bounds GetGridBounds()
    {
        Vector3 center = new Vector3((gridSize.x / 2) * tileSize.x, 0, (gridSize.y / 2) * tileSize.y); // Example: centered at the origin
        Vector3 size = new Vector3(gridSize.x * tileSize.x, 0, gridSize.y * tileSize.y);   // Example: total size of 1 unit on X, 2 on Y, 1 on Z
        Bounds gridBounds = new Bounds(center, size);

        Debug.Log($"GridBounds - Center: {gridBounds.center}, Size: {gridBounds.size}");

        return gridBounds;
    }
}
