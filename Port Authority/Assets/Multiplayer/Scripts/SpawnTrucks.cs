using Mirror;
using UnityEngine;

public class SpawnTrucks : MonoBehaviour
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
            GameObject newTruck = Instantiate(truck, t.position, t.rotation);
            NetworkServer.Spawn(newTruck);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
