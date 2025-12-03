using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using UnityEditor.PackageManager.UI;

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
    public float treeObstacleSpawnInterval = 20f;
    public float mudObstacleSpawnInterval = 10f;

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
        //Debug.Log("Found " + streetManagers.Count + " street managers.");

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

        //Debug.Log("All islands ready");

        waterLevel = terrain.GetWaterLevel();

        // delayed spawn schedule
        // wait 10 seconds then start whirlpools
        yield return new WaitForSeconds(10f);
        StartCoroutine(WhirlpoolRoutine());

        // wait 15 seconds then start trees
        yield return new WaitForSeconds(15f);
        StartCoroutine(TreeRoutine());

        // wait 10 seconds to then start mud puddle spawns
        yield return new WaitForSeconds(10f);
        StartCoroutine(MudRoutine());
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
        int whirlpoolSpawnRange = Random.Range(1, 4);
        for (int i = 0; i < whirlpoolSpawnRange; i++)
        {
            StreetGenerationManager island = streetManagers[Random.Range(0, streetManagers.Count)];
            Vector3 islandPos = island.transform.position;

            // random angle and distance from island center
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(20f, 30f);

            float x = islandPos.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius; ;
            float z = islandPos.z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;

            // choose random water point
            Vector3 pos = new Vector3(x, waterLevel + 1f, z);

            // pushes whirlpool inward to avoid getting stuck at edge
            //pos += new Vector3(
            //Random.Range(-20f, 20f),
            //0,
            //Random.Range(-20f, 20f));

            GameObject wp = Instantiate(whirlpoolPrefab, pos, Quaternion.identity, whirlpoolParent);
            whirlpools.Add(wp);

            // spawn sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayWhirlpoolSpawn();
            }
        }
    }

    // ROAD LOCATION
    private bool RoadSpawnPoint(out Vector3 result, float minDistance = 10f, int maxAttempts = 30)
    {
        result = Vector3.zero;

        List<GameObject> allRoads = new List<GameObject>();

        foreach (var mgr in streetManagers)
        {
            allRoads.AddRange(mgr.GetActivatedChildren());
        }

        if (allRoads.Count == 0)
            return false;

        // shuffle roads so each attempt picks different one
        allRoads = allRoads.OrderBy(x => Random.value).ToList();

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            GameObject road = allRoads[attempt % allRoads.Count];

            MeshRenderer rend = road.GetComponentInChildren<MeshRenderer>();

            if (!rend)
            {
                //Debug.LogWarning("No MeshRenderer found inside: " + road.name);
                continue;
            }

            Bounds b = rend.bounds;

            for (int location = 0; location < 4; location++)
            {
                Vector3 point = new Vector3(Random.Range(b.min.x, b.max.x), b.center.y + 0.3f, Random.Range(b.min.z, b.max.z));

                // offsets to avoid obstacle perfect pizel-aligned positions
                point += new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));

                // ensures obstacles do not spawn too close to one another/on top of each other
                bool tooClose = false;

                foreach (var t in trees)
                {
                    if (t != null && Vector3.Distance(point, t.transform.position) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                foreach (var m in mudPuddles)
                {
                    if (m != null && Vector3.Distance(point, m.transform.position) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    result = point;
                    return true;
                }
            }
        }

        // no valid spot after max attempts 10
        return false;
    }

    // LAND OBSTACLE SPAWNING
    IEnumerator TreeRoutine()
    {
        while (true)
        {
            trees.RemoveAll(x => x == null);

            if (trees.Count < maxTrees)
                SpawnTree();

            yield return new WaitForSeconds(treeObstacleSpawnInterval);
        }
    }

    IEnumerator MudRoutine()
    {
        while (true)
        {
            mudPuddles.RemoveAll(x => x == null);

            if (mudPuddles.Count < maxMudPuddles)
                SpawnMud();

            yield return new WaitForSeconds(mudObstacleSpawnInterval);
        }
    }
    //IEnumerator LandObstaclesRoutine()
    //{
    //    while (streetManagers == null || streetManagers.Count == 0)
    //    {
    //        yield return null;
    //    }

    //    bool anyReady = false;
    //    while (!anyReady)
    //    {
    //        foreach (var mgr in streetManagers)
    //        {
    //            if (mgr != null && mgr.GetActivatedChildren().Count > 0)
    //            {
    //                anyReady = true;
    //                break;
    //            }
    //        }

    //        if (!anyReady)
    //        {
    //            Debug.Log("Waiting for streets to finish activating...");
    //            yield return new WaitForSeconds(0.25f);
    //        }
    //    }

    //    Debug.Log("Streets activated. Starting land obstacle spawning.");
        
    //    while(true)
    //    {
    //        trees.RemoveAll(x => x == null);
    //        mudPuddles.RemoveAll(x => x == null);

    //        if (trees.Count < maxTrees)
    //        {
    //            SpawnTree();
    //        }

    //        if (mudPuddles.Count < maxMudPuddles)
    //        {
    //            SpawnMud();
    //        }

    //        yield return new WaitForSeconds(landObstacleSpawnInterval);
    //    }
    //}

    private void SpawnTree()
    {
        //Debug.Log("SpawnTree() called");

        if (treePrefab == null)
        {
            //Debug.LogError("TREE PREFAB IS NULL");
            return;
        }

        Vector3 pos;

        // spawn bigger distance
        if (!RoadSpawnPoint(out pos, minDistance: 15f))
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

        // spawn bigger distance
        if (!RoadSpawnPoint(out pos, minDistance: 20f))
        {
            return;
        }

        pos.y += 1.5f;

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
