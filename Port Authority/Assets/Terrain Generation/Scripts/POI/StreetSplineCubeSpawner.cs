using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
public class StreetSplineCubeSpawner : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float spacing = 40f;
    [SerializeField] private float lateralOffset = 10f;
    [SerializeField] private Vector3 cubeScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private bool isDebug = true;
    [SerializeField] private List<GameObject> buildingPrefabs = new List<GameObject>();

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private SplineContainer splineContainer;
    
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }
    
    private void Start()
    {
        SpawnCubesAlongSpline();
    }

    [ContextMenu("Spawn Cubes Along Spline")]
    public void SpawnCubesAlongSpline()
    {
        ClearSpawnedCubes();

        var spline = splineContainer.Spline;

        float totalLength = spline.GetLength();

        for (float distance = 0f; distance <= totalLength + 0.01f; distance += spacing)
        {
            float normalizedT = math.clamp(distance / totalLength, 0f, 1f);
            splineContainer.Evaluate(normalizedT, out float3 position, out float3 tangent, out float3 up);
            SpawnCube(position, tangent, up);
        }
    }

    [ContextMenu("Clear Spawned Cubes")]
    public void ClearSpawnedCubes()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            Destroy(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
    }

    private void SpawnCube(float3 position, float3 tangent, float3 up)
    {
        Vector3 tangentVector = ((Vector3)tangent).normalized;
        Vector3 upVector = ((Vector3)up).sqrMagnitude > 0f ? ((Vector3)up).normalized : Vector3.up;
        Vector3 rightVector = Vector3.Cross(upVector, tangentVector);
        if (rightVector.sqrMagnitude <= math.EPSILON)
            rightVector = transform.right;
        else
            rightVector.Normalize();
        
        SpawnCubeAt(position, tangentVector, upVector, rightVector); // Right side
        SpawnCubeAt(position, tangentVector, upVector, -rightVector); // Left side
    }

    private void SpawnCubeAt(float3 splinePosition, Vector3 tangentVector, Vector3 upVector, Vector3 lateralDirection)
    {
        Vector3 spawnPosition = (Vector3)splinePosition + lateralDirection * lateralOffset;
        
        Quaternion spawnRotation = tangentVector.sqrMagnitude > 0f ? Quaternion.LookRotation(tangentVector, upVector) : Quaternion.identity;

        GameObject spawnedObject = isDebug ? SpawnDebugCube(spawnPosition, spawnRotation) : SpawnBuilding(spawnPosition, spawnRotation);
        
        spawnedObjects.Add(spawnedObject);
    }

    private GameObject SpawnDebugCube(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = $"Street Cube {spawnedObjects.Count:000}";
        cube.transform.SetParent(transform);
        cube.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        cube.transform.localScale = cubeScale;
        return cube;
    }

    private GameObject SpawnBuilding(Vector3 spawnPosition, Quaternion splineRotation)
    {
        if (buildingPrefabs == null || buildingPrefabs.Count == 0)
        {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, buildingPrefabs.Count);
        GameObject prefab = buildingPrefabs[randomIndex];
        if (prefab == null)
        {
            return null;
        }

        Quaternion prefabRotation = prefab.transform.rotation;
        Quaternion finalRotation = splineRotation * prefabRotation;
        
        GameObject building = Instantiate(prefab, spawnPosition, finalRotation, transform);
        return building;
    }
    
}
