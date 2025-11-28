using System;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Mirror;

public class StructurePlacerN : MonoBehaviour
{
    //Prefabs for different building types
    [SerializeField] private List<GameObject> storePrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> dockPrefabs = new List<GameObject>();
    [SerializeField] private List<GameObject> factoriesPrefabs = new List<GameObject>();

    [SerializeField] private PointFinder pointFinder;
    
    [SerializeField] private int commercialCount = 4;
    [SerializeField] private int factoryCount = 4;
    [SerializeField] private int dockCount = 4;
    
    // Shore placement settingsq
    [SerializeField] private float shoreDistance = 10f;
    [SerializeField] private float shoreBuildingRadius = 25f;
    [SerializeField] private int shoreMaxAttempts = 500;

    // Land placement settings
    [SerializeField] private float landBuildingRadius = 25f;
    [SerializeField] private float landShoreClearance = 40f;
    [SerializeField] private int maxAttemptsPerPoint = 50;
    [SerializeField] private float roadClearance = 20f;
    [SerializeField] private float factoryRoadPadding = 10f;

    // Sampling and raycasting
    [SerializeField] private float raycastHeight = 100f;
    [SerializeField] private int radialSamples = 16;

    // Area settings
    HashSet<int> availableStoreIndices = new HashSet<int>();
    HashSet<int> availableDockIndices = new HashSet<int>();
    HashSet<int> availableFactoryIndices = new HashSet<int>();
    
    private List<GameObject> stores = new List<GameObject>();
    private List<GameObject> factories = new List<GameObject>();
    private List<GameObject> docks = new List<GameObject>();

    private const float MaxPlacementSize = 450f;
    private const float MinPlacementSize = 25f;
    private const float BoundsPadding = 8f;

    private Vector2 landAreaCenter;
    private Vector2 landAreaSize;
    private Vector2 dockAreaCenter;
    private Vector2 dockAreaSize;
    private bool boundsInitialized;

    [ServerCallback]
    private void Awake()
    {
        if (pointFinder == null)
        {
            pointFinder = GetComponent<PointFinder>();
        }

        if (pointFinder == null)
        {
            pointFinder = GetComponentInChildren<PointFinder>();
        }
    }

    [Server]
    private void OnValidate()
    {
        boundsInitialized = false;
    }

    [ServerCallback]
    private void Start()
    {
        if (pointFinder == null)
        {
            Debug.LogError("StructurePlacer requires a PointFinder reference on the same prefab.", this);
            enabled = false;
            return;
        }

        Debug.Log("Spawning Buildings");
        EnsurePlacementBounds();

        InitializeIndices();
        StoreLoop();
        DockLoop();
        FactoryLoop();
    }

    [Server]
    private void InitializeIndices()
    {
        for (int i = 0; i < storePrefabs.Count; i++)
            availableStoreIndices.Add(i);
        
        for (int i = 0; i < dockPrefabs.Count; i++)
            availableDockIndices.Add(i);
        
        for (int i = 0; i < factoriesPrefabs.Count; i++)
            availableFactoryIndices.Add(i);
    }

    [Server]
    private void EnsurePlacementBounds()
    {
        if (boundsInitialized)
            return;

        Vector2 tileCenter = CalculateTileCenter();
        landAreaCenter = tileCenter;
        dockAreaCenter = tileCenter;

        landAreaSize = new Vector2(MaxPlacementSize, MaxPlacementSize);
        dockAreaSize = landAreaSize;

        boundsInitialized = true;
    }


    private Vector2 CalculateTileCenter()
    {
        float tileMinX = Mathf.Floor(transform.position.x / MaxPlacementSize) * MaxPlacementSize;
        float tileMinZ = Mathf.Floor(transform.position.z / MaxPlacementSize) * MaxPlacementSize;
        return new Vector2(tileMinX + MaxPlacementSize * 0.5f, tileMinZ + MaxPlacementSize * 0.5f);
    }

    [Server]
    private void StoreLoop()
    {
        EnsurePlacementBounds();

        for (int i = 0; i < commercialCount; i++)
        {
            Vector3 pos = pointFinder.FindLandPoint(landBuildingRadius, landShoreClearance, maxAttemptsPerPoint, landAreaCenter, landAreaSize, roadClearance);
            if (pos != Vector3.zero)
            {
                GameObject store = Instantiate(GetStorePrefab(), pos, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0));
                NetworkServer.Spawn(store);
            }
            else
                Debug.LogWarning("Store Prefab Not Found or Invalid Position");

        }
    }

    [Server]
    private void DockLoop()
    {
        EnsurePlacementBounds();

        for (int i = 0; i < dockCount; i++)
        {
            Vector3 pos = pointFinder.FindShorePoint(shoreDistance, shoreMaxAttempts, shoreBuildingRadius, dockAreaCenter, dockAreaSize);
            if (pos != Vector3.zero)
            {
                pos.y = 5;
                Quaternion dockRotation = GetDockOrientation(pos);
                GameObject dock = Instantiate(GetDockPrefab(), pos, dockRotation);
                NetworkServer.Spawn(dock);
            }
            else 
                Debug.LogWarning("Dock Prefab Not Found or Invalid Position");
        }
    }

    [Server]
    private void FactoryLoop()
    {
        EnsurePlacementBounds();

        for (int i = 0; i < factoryCount; i++)
        {
            Vector3 pos = pointFinder.FindLandPoint(landBuildingRadius, landShoreClearance, maxAttemptsPerPoint, landAreaCenter, landAreaSize, roadClearance + factoryRoadPadding);
            if (pos != Vector3.zero)
            {
                GameObject factory = Instantiate(GetFactoryPrefab(), pos, Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0));
                NetworkServer.Spawn(factory);

            }
            else 
                Debug.LogWarning("Factory Prefab Not Found or Invalid Position");
        }
    }

    [Server]
    private Quaternion GetDockOrientation(Vector3 spawnPosition)
    {
        if (pointFinder == null)
            return Quaternion.identity;

        Quaternion orientation = pointFinder.GetShoreFacingRotation(spawnPosition);
        return orientation;
    }

    [Server]
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

    [Server]
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

    [Server]
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
