using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PointFinder : MonoBehaviour
{
    public static PointFinder Instance { get; private set; }

    [SerializeField] private float shoreDistance = 10f;
    [SerializeField] private float shoreBuildingRadius = 25f;
    [SerializeField] private float landBuildingRadius = 25f;
    [SerializeField] private float landShoreClearance = 40f;

    [SerializeField] private int maxAttemptsPerPoint = 50;
    [SerializeField] private int shoreMaxAttempts = 500;
    [SerializeField] private float raycastHeight = 100f;
    [SerializeField] private Vector2 areaCenter = Vector2.zero;
    [SerializeField] private Vector2 areaSize = new Vector2(500f, 500f);
    [SerializeField] private int radialSamples = 16;
    [SerializeField] private float cubeSize = 2f;
    [SerializeField] private string terrainTag = "Terrain";
    [SerializeField] private string waterTag = "Water";
    [SerializeField] private LayerMask raycastMask = ~0;

    private readonly List<Vector3> landPoints = new List<Vector3>();
    private readonly List<Vector3> shorePoints = new List<Vector3>();
    private readonly List<GameObject> landMarkers = new List<GameObject>();
    private readonly List<GameObject> shoreMarkers = new List<GameObject>();

    public Vector3 SetShorePoint(float distance, int maxAttempts)
    {
        shoreDistance = Mathf.Max(0f, distance);
        shoreMaxAttempts = Mathf.Max(1, maxAttempts);

        Vector3 point = FindShorePoint();

        if (point == Vector3.zero)
            Debug.LogWarning("Failed to find a valid shore point within the provided parameters.");

        return point;
    }

    public Vector3 FindLandPoint()
    {
        Vector3 point = FindLandPointInternal();

        if (point == Vector3.zero)
            Debug.LogWarning("Failed to find a valid land point within the specified area and constraints.");

        return point;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private Vector3 FindLandPointInternal()
    {
        int totalAttempts = 0;

        while (totalAttempts < maxAttemptsPerPoint)
        {
            totalAttempts++;

            if (!TryFindValidPoint(out Vector3 surfacePoint))
                continue;

            if (!IsFarFromPoints(surfacePoint, landPoints, landBuildingRadius))
                continue;

            if (HasWaterWithinRadius(surfacePoint, landShoreClearance))
                continue;

            landPoints.Add(surfacePoint);
            landMarkers.Add(SpawnMarker(surfacePoint));
            return surfacePoint;
        }

        return Vector3.zero;
    }

    private Vector3 FindShorePoint()
    {
        int totalAttempts = 0;

        while (totalAttempts < shoreMaxAttempts)
        {
            totalAttempts++;

            if (!TryFindValidPoint(out Vector3 surfacePoint))
                continue;

            if (!IsFarFromPoints(surfacePoint, shorePoints, shoreBuildingRadius))
                continue;

            if (!HasWaterWithinRadius(surfacePoint, shoreDistance))
                continue;

            shorePoints.Add(surfacePoint);
            shoreMarkers.Add(SpawnMarker(surfacePoint));
            return surfacePoint;
        }

        return Vector3.zero;
    }

    private bool TryFindValidPoint(out Vector3 point)
    {
        point = default;

        Vector2 sampledPoint = GetRandomXZWithinArea();
        Vector3 rayOrigin = new Vector3(sampledPoint.x, raycastHeight, sampledPoint.y);
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, Mathf.Infinity, EffectiveRaycastMask);

        if (!ValidateRaycasts(hits, out RaycastHit terrainHit, out RaycastHit waterHit))
            return false;

        if (waterHit.collider == null)
            return false;

        if (waterHit.distance <= terrainHit.distance)
            return false;

        point = terrainHit.point;
        return true;
    }

    private bool HasWaterWithinRadius(Vector3 center, float radius)
    {
        if (radius <= 0f || radialSamples <= 0)
            return false;

        float step = Mathf.PI * 2f / radialSamples;

        for (int i = 0; i < radialSamples; i++)
        {
            float angle = step * i;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            Vector3 origin = new Vector3(center.x + offset.x, raycastHeight, center.z + offset.z);
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, Mathf.Infinity, EffectiveRaycastMask);

            if (!ValidateRaycasts(hits, out RaycastHit terrainHit, out RaycastHit waterHit))
                continue;

            if (waterHit.collider != null && waterHit.distance <= terrainHit.distance)
                return true;
        }

        return false;
    }

    private bool ValidateRaycasts(RaycastHit[] hits, out RaycastHit terrainHit, out RaycastHit waterHit)
    {
        terrainHit = default;
        waterHit = default;

        if (hits == null || hits.Length == 0)
            return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit? firstTerrain = null;
        RaycastHit? firstWater = null;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider collider = hits[i].collider;

            if (collider == null)
                continue;

            if (firstTerrain == null && collider.CompareTag(terrainTag))
                firstTerrain = hits[i];

            if (firstWater == null && collider.CompareTag(waterTag))
                firstWater = hits[i];
        }

        if (firstTerrain == null)
            return false;

        terrainHit = firstTerrain.Value;
        waterHit = firstWater ?? default;
        return true;
    }

    private bool IsFarFromPoints(Vector3 candidate, List<Vector3> points, float radius)
    {
        if (radius <= 0f)
            return true;

        float minDistanceSqr = radius * radius;

        for (int i = 0; i < points.Count; i++)
        {
            if ((points[i] - candidate).sqrMagnitude < minDistanceSqr)
                return false;
        }

        return true;
    }

    private GameObject SpawnMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.localScale = Vector3.one * cubeSize;
        marker.transform.position = position + Vector3.up * (cubeSize * 0.5f);
        return marker;
    }

    private Vector2 GetRandomXZWithinArea()
    {
        Vector2 halfSize = new Vector2(Mathf.Max(0.1f, areaSize.x) * 0.5f, Mathf.Max(0.1f, areaSize.y) * 0.5f);
        float x = Random.Range(areaCenter.x - halfSize.x, areaCenter.x + halfSize.x);
        float z = Random.Range(areaCenter.y - halfSize.y, areaCenter.y + halfSize.y);
        return new Vector2(x, z);
    }

    private int EffectiveRaycastMask => raycastMask.value == 0 ? Physics.DefaultRaycastLayers : raycastMask.value;
}
