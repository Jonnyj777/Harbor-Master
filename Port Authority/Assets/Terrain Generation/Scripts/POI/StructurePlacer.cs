using System;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StructurePlacer : MonoBehaviour
{
    //Prefabs for different building types
    [SerializeField] private List<GameObject> storePrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> dockPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> factoriesPrefabs = new List<GameObject>();
    
    [SerializeField] private int commercialCount = 4;
    [SerializeField] private int factoryCount = 4;
    [SerializeField] private int dockCount = 4;
    
    // Shore placement settingsq
    [SerializeField] private float shoreDistance = 10f;
    [SerializeField] private float shoreBuildingRadius = 25f;
    [SerializeField] private float shoreSearchDistance = 10f;
    [SerializeField] private int shoreSearchMaxAttempts = 1000;
    [SerializeField] private int shoreMaxAttempts = 500;

    // Land placement settings
    [SerializeField] private float landBuildingRadius = 25f;
    [SerializeField] private float landShoreClearance = 40f;
    [SerializeField] private int maxAttemptsPerPoint = 50;

    // Sampling and raycasting
    [SerializeField] private float raycastHeight = 100f;
    [SerializeField] private int radialSamples = 16;

    // Area settings
    [SerializeField] private Vector2 areaCenter = Vector2.zero;
    [SerializeField] private Vector2 areaSize = new Vector2(500f, 500f);
    
    HashSet<int> availableStoreIndices = new HashSet<int>();
    HashSet<int> availableDockIndices = new HashSet<int>();
    HashSet<int> availableFactoryIndices = new HashSet<int>();
    
    private List<GameObject> stores = new List<GameObject>();
    private List<GameObject> factories = new List<GameObject>();
    private List<GameObject> docks = new List<GameObject>();

    private void Start()
    {
        InitializeIndices();
        StoreLoop();
        DockLoop();
        FactoryLoop();
    }
    
    private void InitializeIndices()
    {
        for (int i = 0; i < storePrefabs.Count; i++)
            availableStoreIndices.Add(i);
        
        for (int i = 0; i < dockPrefabs.Count; i++)
            availableDockIndices.Add(i);
        
        for (int i = 0; i < factoriesPrefabs.Count; i++)
            availableFactoryIndices.Add(i);
    }

    private void StoreLoop()
    {
        for (int i = 0; i < commercialCount; i++)
        {
            Vector3 pos = PointFinder.Instance.FindLandPoint(landBuildingRadius, landShoreClearance, maxAttemptsPerPoint);
            if (pos != Vector3.zero)
                Instantiate(GetStorePrefab(), pos, Quaternion.identity);
            else 
                Debug.LogWarning("Store Prefab Not Found or Invalid Position");

        }
    }

    private void DockLoop()
    {
        for (int i = 0; i < dockCount; i++)
        {
            Vector3 pos = PointFinder.Instance.FindShorePoint(shoreDistance, shoreMaxAttempts, shoreBuildingRadius);
            if (pos != Vector3.zero)
            {
                Quaternion dockRotation = GetClosestDockNodeOrientation(pos);
                Instantiate(GetDockPrefab(), pos, dockRotation);
            }
            else 
                Debug.LogWarning("Dock Prefab Not Found or Invalid Position");
        }
    }
    
    private void FactoryLoop()
    {
        for (int i = 0; i < factoryCount; i++)
        {
            Vector3 pos = PointFinder.Instance.FindLandPoint(landBuildingRadius, landShoreClearance, maxAttemptsPerPoint);
            if (pos != Vector3.zero)
                Instantiate(GetFactoryPrefab(), pos, Quaternion.identity);
            else 
                Debug.LogWarning("Factory Prefab Not Found or Invalid Position");
        }
    }

    private Quaternion GetClosestDockNodeOrientation(Vector3 spawnPosition)
    {
        GameObject[] dockNodes = GameObject.FindGameObjectsWithTag("DockNode");
        if (dockNodes == null || dockNodes.Length == 0)
            return Quaternion.identity;

        GameObject closestNode = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < dockNodes.Length; i++)
        {
            if (dockNodes[i] == null)
                continue;

            float distanceSqr = (dockNodes[i].transform.position - spawnPosition).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestNode = dockNodes[i];
            }
        }

        if (closestNode == null)
            return Quaternion.identity;

        Vector3 directionToNode = closestNode.transform.position - spawnPosition;
        directionToNode.y = 0f;

        if (directionToNode.sqrMagnitude <= Mathf.Epsilon)
            return Quaternion.identity;

        return Quaternion.LookRotation(directionToNode.normalized, Vector3.up);
    }

    private GameObject GetStorePrefab()
{
    if (availableStoreIndices.Count == 0)
    {
        for (int i = 0; i < storePrefabs.Count; i++)
            availableStoreIndices.Add(i);
    }

    int randomIndex = UnityEngine.Random.Range(0, availableStoreIndices.Count);
    int selectedIndex = -1;
    int currentIndex = 0;

    foreach (int index in availableStoreIndices)
    {
        if (currentIndex == randomIndex)
        {
            selectedIndex = index;
            break;
        }
        currentIndex++;
    }

    if (selectedIndex != -1)
    {
        availableStoreIndices.Remove(selectedIndex);
        return storePrefabs[selectedIndex];
    }

    return null;
}

    private GameObject GetFactoryPrefab()
{
    if (availableFactoryIndices.Count == 0)
    {
        for (int i = 0; i < factoriesPrefabs.Count; i++)
            availableFactoryIndices.Add(i);
    }

    int randomIndex = UnityEngine.Random.Range(0, availableFactoryIndices.Count);
    int selectedIndex = -1;
    int currentIndex = 0;

    foreach (int index in availableFactoryIndices)
    {
        if (currentIndex == randomIndex)
        {
            selectedIndex = index;
            break;
        }
        currentIndex++;
    }

    if (selectedIndex != -1)
    {
        availableFactoryIndices.Remove(selectedIndex);
        return factoriesPrefabs[selectedIndex];
    }

    return null;
}

    private GameObject GetDockPrefab()
{
    if (availableDockIndices.Count == 0)
    {
        for (int i = 0; i < dockPrefabs.Count; i++)
            availableDockIndices.Add(i);
    }

    int randomIndex = UnityEngine.Random.Range(0, availableDockIndices.Count);
    int selectedIndex = -1;
    int currentIndex = 0;

    foreach (int index in availableDockIndices)
    {
        if (currentIndex == randomIndex)
        {
            selectedIndex = index;
            break;
        }
        currentIndex++;
    }

    if (selectedIndex != -1)
    {
        availableDockIndices.Remove(selectedIndex);
        return dockPrefabs[selectedIndex];
    }

    return null;
}

}
