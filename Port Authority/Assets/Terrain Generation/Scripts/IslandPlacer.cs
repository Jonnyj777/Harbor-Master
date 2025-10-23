using System.Collections.Generic;
using UnityEngine;

public class IslandPlacer : MonoBehaviour
{
    [SerializeField] List<GameObject> islandPrefabs;
    [SerializeField] Vector2 gridSize = new Vector2(2, 2);
    [SerializeField] Vector2 tileSize = new Vector2(500, 500);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                GameObject island = Instantiate(islandPrefabs[Random.Range(0, islandPrefabs.Count)]);
                island.transform.position = new Vector3(x * tileSize.x, 0, z * tileSize.y);
            }
        }
        /*
        GameObject island = Instantiate(islandPrefabs[0]);
        island.transform.position = new Vector3(0, 0, 0);
        
        GameObject island2 = Instantiate(islandPrefabs[0]);
        island2.transform.position = new Vector3(500, 0, 0);
        
        GameObject island3 = Instantiate(islandPrefabs[0]);
        island3.transform.position = new Vector3(0 ,0, 500);
        
        GameObject island4 = Instantiate(islandPrefabs[0]);
        island4.transform.position = new Vector3(500, 0, 500);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
