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
    [SerializeField, Min(1)] private int buildingCount = 1;
    [SerializeField, Min(0)] private int trimFront = 0;
    [SerializeField, Min(0)] private int trimBack = 0;
    [SerializeField] private List<GameObject> buildingPrefabs = new List<GameObject>();

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private readonly List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
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
            CollectSpawnPoints(position, tangent, up);
        }

        ApplyTrimSettings();

        if (isDebug)
            SpawnDebugCubesFromPoints();
        else
            SpawnBuildingsFromPoints();
    }

    [ContextMenu("Clear Spawned Cubes")]
    public void ClearSpawnedCubes()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            Destroy(spawnedObjects[i]);

        spawnedObjects.Clear();
        spawnPoints.Clear();
    }

    private void CollectSpawnPoints(float3 position, float3 tangent, float3 up)
    {
        Vector3 tangentVector = ((Vector3)tangent).normalized;
        Vector3 upVector = ((Vector3)up).sqrMagnitude > 0f ? ((Vector3)up).normalized : Vector3.up;
        Vector3 rightVector = Vector3.Cross(upVector, tangentVector);
        if (rightVector.sqrMagnitude <= math.EPSILON)
            rightVector = transform.right;
        else
            rightVector.Normalize();
        
        AddSpawnPoint(position, tangentVector, upVector, rightVector); // Right side
        AddSpawnPoint(position, tangentVector, upVector, -rightVector); // Left side
    }

    private void AddSpawnPoint(float3 splinePosition, Vector3 tangentVector, Vector3 upVector, Vector3 lateralDirection)
    {
        Vector3 spawnPosition = (Vector3)splinePosition + lateralDirection * lateralOffset;
        
        Quaternion spawnRotation = tangentVector.sqrMagnitude > 0f ? Quaternion.LookRotation(tangentVector, upVector) : Quaternion.identity;

        Vector3 normalizedLateral = lateralDirection;
        if (normalizedLateral.sqrMagnitude <= math.EPSILON)
            normalizedLateral = transform.right;
        else
            normalizedLateral.Normalize();

        spawnPoints.Add(new SpawnPoint(spawnPosition, spawnRotation, upVector, normalizedLateral));
    }

    private void SpawnDebugCubesFromPoints()
    {
        foreach (var spawnPoint in spawnPoints)
        {
            GameObject spawnedObject = SpawnDebugCube(spawnPoint.Position, spawnPoint.AlongSplineRotation);
            spawnedObjects.Add(spawnedObject);
        }
    }

    private void SpawnBuildingsFromPoints()
    {
        if (spawnPoints.Count == 0)
            return;

        int maxBuildings = Mathf.Clamp(buildingCount, 1, spawnPoints.Count);
        var availableIndices = new List<int>(spawnPoints.Count);
        for (int i = 0; i < spawnPoints.Count; i++)
            availableIndices.Add(i);

        while (spawnedObjects.Count < maxBuildings && availableIndices.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableIndices.Count);
            int spawnPointIndex = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);

            SpawnPoint chosenPoint = spawnPoints[spawnPointIndex];

            GameObject spawnedObject = SpawnBuilding(chosenPoint);
            if (spawnedObject == null)
                continue;

            spawnedObjects.Add(spawnedObject);
        }
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

    private GameObject SpawnBuilding(SpawnPoint spawnPoint)
    {
        if (buildingPrefabs == null || buildingPrefabs.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, buildingPrefabs.Count);
        GameObject prefab = buildingPrefabs[randomIndex];
        if (prefab == null)
            return null;
        
        Quaternion prefabRotation = prefab.transform.rotation;
        Quaternion streetRotation = spawnPoint.GetStreetFacingRotation();
        Quaternion finalRotation = streetRotation * prefabRotation;
        
        GameObject building = Instantiate(prefab, spawnPoint.Position, finalRotation);
        Building validator = building.GetComponent<Building>();
        if (validator == null)
            validator = building.AddComponent<Building>();

        validator.Validate();

        return building;
    }
    
    private readonly struct SpawnPoint
    {
        public readonly Vector3 Position;
        public readonly Quaternion AlongSplineRotation;
        public readonly Vector3 UpDirection;
        public readonly Vector3 LateralDirection;

        public SpawnPoint(Vector3 position, Quaternion alongSplineRotation, Vector3 upDirection, Vector3 lateralDirection)
        {
            Position = position;
            AlongSplineRotation = alongSplineRotation;
            UpDirection = upDirection;
            LateralDirection = lateralDirection;
        }

        public Quaternion GetStreetFacingRotation()
        {
            Vector3 forward = -LateralDirection;
            if (forward.sqrMagnitude <= math.EPSILON)
                forward = AlongSplineRotation * Vector3.forward;
            return Quaternion.LookRotation(forward, UpDirection);
        }
    }

    private void ApplyTrimSettings()
    {
        if (spawnPoints.Count == 0)
            return;

        TrimFromFront();
        TrimFromBack();
    }

    private void TrimFromFront()
    {
        if (trimFront <= 0)
            return;

        int pointsToRemove = Mathf.Min(spawnPoints.Count, trimFront * 2);
        if (pointsToRemove > 0)
            spawnPoints.RemoveRange(0, pointsToRemove);
    }

    private void TrimFromBack()
    {
        if (trimBack <= 0 || spawnPoints.Count == 0)
            return;

        int pointsToRemove = Mathf.Min(spawnPoints.Count, trimBack * 2);
        if (pointsToRemove <= 0)
            return;

        int startIndex = spawnPoints.Count - pointsToRemove;
        spawnPoints.RemoveRange(startIndex, pointsToRemove);
    }
}
