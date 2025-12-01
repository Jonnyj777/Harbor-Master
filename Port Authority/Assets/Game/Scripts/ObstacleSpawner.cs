using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public NoiseToTerrainGenerator terrain;
    public StreetGenerationManager streetManager;

    [Header("Obstacles")]
    public GameObject whirlpoolPrefab;
    public GameObject treePrefab;
    public GameObject mudPrefab;

    [Header("Spawn Settings")]
    public int maxWhirlpools = 5;
    public int maxTrees = 12;
    public int maxMudPuddles = 8;

    public float whirlpoolSpawnInterval = 3f;
    public float landObstacleSpawnInterval = 20f;

    private List<GameObject> whirlpools = new List<GameObject>();
    private List<GameObject> trees = new List<GameObject>();
    private List<GameObject> mudPuddles = new List<GameObject>();

    private float waterLevel;

    private void Start()
    {
        StartCoroutine(WaitForStreetManager());
    }
    private IEnumerator WaitForStreetManager()
    {
        //Debug.Log("ObstacleSpawner.Start() has begun executing");
        while (streetManager == null)
        {
            streetManager = Object.FindFirstObjectByType<StreetGenerationManager>();
            
            if (streetManager != null)
            {
                Debug.Log("Street generator found: " + streetManager.name);
                break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        waterLevel = terrain.GetWaterLevel();
        StartCoroutine(WhirlpoolRoutine());
        StartCoroutine(LandObstaclesRoutine());
    }

    // WHIRLPOOL SPAWNING
    IEnumerator WhirlpoolRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(whirlpoolSpawnInterval);

            whirlpools.RemoveAll(x => x == null);  // removes null refs

            if (whirlpools.Count >= maxWhirlpools)
            {
                continue;
            }

            SpawnWhirlpools();
        }
    }

    private void SpawnWhirlpools()
    {
        List<Vector3> oceanEdges = terrain.GetOceanEdgeVertices();
        if (oceanEdges.Count == 0)
            return;

        // choose random water point
        Vector3 pos = oceanEdges[Random.Range(0, oceanEdges.Count)];

        // pushes whirlpool inward to avoid getting stuck at edge
        pos += new Vector3(
            Random.Range(-15f, 15f),
            0,
            Random.Range(-15f, 15f));

        pos.y = waterLevel + 1f;

        GameObject wp = Instantiate(whirlpoolPrefab, pos, Quaternion.identity);
        whirlpools.Add(wp);
    }

    // ROAD LOCATION
    private bool RoadSpawnPoint(out Vector3 result)
    {
        result = Vector3.zero;

        var roads = streetManager.GetActivatedChildren();
        if (roads == null || roads.Count == 0)
            return false;

        GameObject road = roads[Random.Range(0, roads.Count)];

        MeshRenderer rend = road.GetComponent<MeshRenderer>();
        if (!rend)
            return false;

        Bounds b = rend.bounds;

        Vector3 point = new Vector3(Random.Range(b.min.x, b.max.x), b.center.y + 0.3f, Random.Range(b.min.z, b.max.z));

        // offsets to avoid obstacle perfect pizel-aligned positions
        point += new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));

        result = point;
        return true;
    }

    // LAND OBSTACLE SPAWNING
    IEnumerator LandObstaclesRoutine()
    {
        while (streetManager == null)
        {
            yield return null;
        }

        while (streetManager.GetActivatedChildren().Count == 0)
        {
            Debug.Log("Waiting for streets to finish activating...");
            yield return new WaitForSeconds(0.25f);
        }

        Debug.Log("Streets activated. Starting land obstacle spawning.");
        while(true)
        {
            trees.RemoveAll(x => x == null);
            mudPuddles.RemoveAll(x => x == null);

            if (trees.Count < maxTrees)
            {
                SpawnTree();
            }

            if (mudPuddles.Count < maxMudPuddles)
            {
                SpawnMud();
            }

            yield return new WaitForSeconds(landObstacleSpawnInterval);
        }
    }

    private void SpawnTree()
    {
        Vector3 pos;
        if(!RoadSpawnPoint(out pos))
        {
            return;
        }

        GameObject t = Instantiate(treePrefab, pos, Quaternion.identity);
        trees.Add(t);
    }

    private void SpawnMud()
    {
        Vector3 pos;
        if (!RoadSpawnPoint(out pos))
        {
            return;
        }

        GameObject m = Instantiate(mudPrefab, pos, Quaternion.identity);
        mudPuddles.Add(m);
    }
}
