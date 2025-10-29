using System.Collections.Generic;
using UnityEngine;

public class IslandPlacer : MonoBehaviour
{
    [SerializeField] private List<GameObject> islandPrefabs = new List<GameObject>();
    [SerializeField] private Vector2 gridSize = new Vector2(2f, 2f);
    [SerializeField] private Vector2 tileSize = new Vector2(500f, 500f);

    private readonly HashSet<int> availableIslandIndices = new HashSet<int>();

    private void Start()
    {
        if (islandPrefabs == null || islandPrefabs.Count == 0)
        {
            Debug.LogWarning("IslandPlacer requires at least one island prefab assigned.", this);
            return;
        }

        InitializeIndices();
        SpawnIslands();
    }

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
}
