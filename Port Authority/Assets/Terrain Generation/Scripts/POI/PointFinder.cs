using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PointFinder : MonoBehaviour
{
    public static PointFinder Instance { get; private set; }
    
    [SerializeField] private int maxAttemptsPerPoint = 50;
    private float raycastHeight = 100f;
    private Vector2 areaCenter = Vector2.zero;
    [SerializeField] private Vector2 areaSize = new Vector2(500f, 500f);
    [SerializeField] private float pointSpacing = 25f;
    [SerializeField] private float shoreSpacing = 25f;
    [SerializeField] private int radialSamples = 16;
    [SerializeField] private float cubeSize = 2f;
    [SerializeField] private string terrainTag = "Terrain";
    [SerializeField] private string waterTag = "Water";
    [SerializeField] private LayerMask raycastMask = ~0;

    private readonly List<Vector3> foundPoints = new List<Vector3>();
    private readonly List<GameObject> spawnedMarkers = new List<GameObject>();
    
    public Vector3 FindPoint(bool shorePadding, bool shoreRequired)
    {
        Vector3 point = FindPointsAndSpawnMarkers(shorePadding, shoreRequired);  
        if(point == Vector3.zero)
            Debug.LogWarning("Failed to find a valid point within the specified area and constraints.");
        
        return point;
    }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public Vector3 FindPointsAndSpawnMarkers(bool shorePadding, bool shoreRequired)
    {
        foundPoints.Clear();
        ClearExistingMarkers();

        int totalAttempts = 0;
        Vector3 point = Vector3.zero;

        while (totalAttempts < maxAttemptsPerPoint)
        {
            totalAttempts++;

            if (!TryFindValidPoint(out Vector3 surfacePoint))
                continue;
            

            if (!IsFarFromExistingPoints(surfacePoint))
                continue;
            

            if (!IsAreaSafe(surfacePoint, shorePadding, shoreRequired))
                continue;
            
            foundPoints.Add(surfacePoint);
            SpawnMarker(surfacePoint);
            return surfacePoint;
        }
        
        return Vector3.zero;
    }

    private void ClearExistingMarkers()
    {
        for (int i = spawnedMarkers.Count - 1; i >= 0; i--)
        {
            GameObject marker = spawnedMarkers[i];

            if (marker == null)
                continue;
            
            Destroy(marker);
        }

        spawnedMarkers.Clear();
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

    private bool IsAreaSafe(Vector3 center, bool shorePadding, bool shoreRequired)
    {
        //if (shoreSpacing <= 0f || radialSamples <= 0)
            //return true;

        float step = Mathf.PI * 2f / radialSamples;

        for (int i = 0; i < radialSamples; i++)
        {
            float angle = step * i;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * shoreSpacing, 0f, Mathf.Sin(angle) * shoreSpacing);
            Vector3 origin = new Vector3(center.x + offset.x, raycastHeight, center.z + offset.z);
            RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, Mathf.Infinity, EffectiveRaycastMask);

            if (!ValidateRaycasts(hits, out RaycastHit terrainHit, out RaycastHit waterHit))
                return false;

            if (shorePadding && waterHit.collider != null && waterHit.distance <= terrainHit.distance)
                return false;
        }

        return true;
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

    private bool IsFarFromExistingPoints(Vector3 candidate)
    {
        if (pointSpacing <= 0f)
            return true;

        float minDistanceSqr = pointSpacing * pointSpacing;

        for (int i = 0; i < foundPoints.Count; i++)
            if ((foundPoints[i] - candidate).sqrMagnitude < minDistanceSqr)
                return false;

        return true;
    }

    private void SpawnMarker(Vector3 position)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.transform.localScale = Vector3.one * cubeSize;
        marker.transform.position = position + Vector3.up * (cubeSize * 0.5f);
        spawnedMarkers.Add(marker);
        
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
