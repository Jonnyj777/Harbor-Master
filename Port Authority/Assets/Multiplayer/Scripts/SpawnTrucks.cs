using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class SpawnTrucks : NetworkBehaviour
{
    [SerializeField]
    private GameObject truck;

    [SerializeField]
    private Transform[] spawnLocations;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(Transform t in spawnLocations)
        {
            SpawnTruck(t);
        }
    }

    [Server]
    void SpawnTruck(Transform t)
    {
        GameObject newTruck = Instantiate(truck, t.position, t.rotation);
        newTruck.AddComponent<AuthorityCheckerDebug>();
        NetworkServer.Spawn(newTruck);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
