using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    public NoiseToTerrainGenerator terrain;
    public List<StreetGenerationManager> streetManagers = new List<StreetGenerationManager>();

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

    [Header("Obstacle Parents")]
    public Transform whirlpoolParent;
    public Transform treesParent;
    public Transform mudParent;

    [Header("UI References")]
    public GameObject treeCleanupButtonPrefab;
    public GameObject mudCleanupButtonPrefab;
    public Canvas treesUICanvas;
    public Canvas mudUICanvas;

    private List<GameObject> whirlpools = new List<GameObject>();
    private List<GameObject> trees = new List<GameObject>();
    private List<GameObject> mudPuddles = new List<GameObject>();

    private float waterLevel;

    private void Start()
    {
        StartCoroutine(WaitForStreetManagers());
    }

    private IEnumerator WaitForStreetManagers()
    {
        StreetGenerationManager[] managers = null;

        //Debug.Log("ObstacleSpawner.Start() has begun executing");
        while (managers == null || managers.Length == 0)
        {
            managers = FindObjectsByType<StreetGenerationManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            yield return new WaitForSeconds(0.1f);
        }

        streetManagers.AddRange(managers);
        Debug.Log("Found " + streetManagers.Count + " street managers.");

        bool ready = false;
        while (!ready)
        {
            ready = true;

            foreach (var mgr in managers)
            {
                if (mgr.GetActivatedChildren().Count == 0)
                {
                    ready = false;
                    break;
                }
            }
            if (!ready)
                yield return new WaitForSeconds(0.25f);
        }

        Debug.Log("All islands ready");

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
        StreetGenerationManager island = streetManagers[Random.Range(0, streetManagers.Count)];
        
        List<Vector3> islandEdges = terrain.GetOceanEdgeVerticesNearIsland(island.transform.position, 20f);
        
        if (islandEdges.Count == 0)
            return;

        // choose random water point
        Vector3 pos = islandEdges[Random.Range(0, islandEdges.Count)];

        // pushes whirlpool inward to avoid getting stuck at edge
        pos += new Vector3(
            Random.Range(-15f, 15f),
            0,
            Random.Range(-15f, 15f));

        pos.y = waterLevel + 1f;

        GameObject wp = Instantiate(whirlpoolPrefab, pos, Quaternion.identity, whirlpoolParent);
        whirlpools.Add(wp);

        // spawn sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWhirlpoolSpawn();
        }
    }

    // ROAD LOCATION
    private bool RoadSpawnPoint(out Vector3 result)
    {
        result = Vector3.zero;

        List<GameObject> allRoads = new List<GameObject>();

        foreach (var mgr in streetManagers)
        {
            allRoads.AddRange(mgr.GetActivatedChildren());
        }

        if (allRoads.Count == 0)
            return false;

        GameObject road = allRoads[Random.Range(0, allRoads.Count)];

        MeshRenderer rend = road.GetComponentInChildren<MeshRenderer>();

        if (!rend)
        {
            Debug.LogWarning("No MeshRenderer found inside: " + road.name);
            return false;
        }

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
        while (streetManagers == null || streetManagers.Count == 0)
        {
            yield return null;
        }

        bool anyReady = false;
        while (!anyReady)
        {
            foreach (var mgr in streetManagers)
            {
                if (mgr != null && mgr.GetActivatedChildren().Count > 0)
                {
                    anyReady = true;
                    break;
                }
            }

            if (!anyReady)
            {
                Debug.Log("Waiting for streets to finish activating...");
                yield return new WaitForSeconds(0.25f);
            }
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
        //Debug.Log("SpawnTree() called");

        if (treePrefab == null)
        {
            //Debug.LogError("TREE PREFAB IS NULL");
            return;
        }

        Vector3 pos;
        if(!RoadSpawnPoint(out pos))
        {
            return;
        }

        pos.y += 0.2f;

        GameObject t = Instantiate(treePrefab, pos, Quaternion.identity, treesParent);
        
        TreeObstacle obstacle = t.GetComponent<TreeObstacle>();
        // assign canvas
        if(obstacle != null)
        {
            obstacle.treesUICanvas = treesUICanvas;
            obstacle.cleanupButtonPrefab = treeCleanupButtonPrefab;
        }

        trees.Add(t);
        //Debug.Log("Tree spawned at: " + pos);
    }

    private void SpawnMud()
    {
        Vector3 pos;
        if (!RoadSpawnPoint(out pos))
        {
            return;
        }

        pos.y += 0.5f;

        GameObject m = Instantiate(mudPrefab, pos, Quaternion.identity, mudParent);

        MudPuddle puddle = m.GetComponent<MudPuddle>();
        if (puddle != null)
        {
            puddle.mudUICanvas = mudUICanvas;
            puddle.cleanupButtonPrefab = mudCleanupButtonPrefab;

            puddle.ShowCleanupButton();
        }

        mudPuddles.Add(m);
    }
}
