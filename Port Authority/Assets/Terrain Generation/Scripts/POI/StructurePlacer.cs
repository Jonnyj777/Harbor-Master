using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class StructurePlacer : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> structures;

    [SerializeField]
    private List<Transform> spawnLocations;

    private List<GameObject> spawns;


    [SerializeField]
    private bool isActive = false;

    private void Start()
    {
        Vector3 point = PointFinder.Instance.FindPoint(false, true);
    }
    /*private void Update()
    {

        if(!isActive) return;
        print(spawns);
        foreach(GameObject spawn in spawns)
        {
            print(spawn);
            DestroyImmediate(spawn);
        }

        spawns.Clear();

        PlaceStructures();
    }
*/
    private void PlaceStructures()
    {
        int randomStructureIndex = (int)Mathf.Floor(Random.Range(0, structures.Count)); 

        for(int i = 0; i < spawnLocations.Count; i++)
        {
            GameObject newStructure = Instantiate(structures[randomStructureIndex]);
            newStructure.transform.position = spawnLocations[i].position;
            randomStructureIndex = (int)Mathf.Floor(Random.Range(0, structures.Count));

            spawns.Add(newStructure);
        }
    }
}
