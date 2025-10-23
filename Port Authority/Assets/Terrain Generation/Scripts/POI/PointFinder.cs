using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PointFinder : MonoBehaviour
{
    [SerializeField] private float raycastHeight = 100f;
    [SerializeField] private int radialSamples = 16;
    [SerializeField] private float shoreOrientationRadius = 15f;
    [SerializeField, Range(8, 360)] private int shoreOrientationSamples = 72;
    [SerializeField] private int shoreOrientationMinSequence = 2;
    private int shoreMaxAttempts;
    private float shoreDistance;
    private float shoreBuildingRadius;
    
    private float landBuildingRadius;
    private float landShoreClearance;
    private int landMaxAttempts;
    
    private float cubeSize = 10f;
    private string terrainTag = "Terrain";
    private string waterTag = "Water";
    private LayerMask raycastMask = ~0;

    private readonly List<Vector3> landPoints = new List<Vector3>();
    private readonly List<Vector3> shorePoints = new List<Vector3>();
    public Vector3 FindShorePoint(float shoreDistance, int maxAttempts, float shoreBuildingRadius, Vector2 areaCenter, Vector2 areaSize)
    {
        this.shoreDistance = Mathf.Max(0f, shoreDistance);
        this.shoreMaxAttempts = Mathf.Max(1, maxAttempts);
        this.shoreBuildingRadius = Mathf.Max(0f, shoreBuildingRadius);

        Vector3 point = FindShorePoint( areaCenter, areaSize);

        if (point == Vector3.zero)
            Debug.LogWarning("Failed to find a valid shore point within the provided parameters.");

        return point;
    }
    
    public Quaternion GetShoreFacingRotation(Vector3 shorePoint)
    {
        Vector3 facing = ComputeShoreFacingDirection(shorePoint, shoreOrientationSamples, shoreOrientationRadius, shoreOrientationMinSequence);
        if (facing.sqrMagnitude <= Mathf.Epsilon)
            return Quaternion.identity;

        return Quaternion.LookRotation(facing, Vector3.up);
    }
    public Vector3 FindLandPoint(float landBuildingRadius, float landShoreClearance, int landMaxAttempts, Vector2 areaCenter, Vector2 areaSize)
    {
        this.landBuildingRadius = Mathf.Max(0f, landBuildingRadius);
        this.landShoreClearance = Mathf.Max(0f, landShoreClearance);
        this.landMaxAttempts = Mathf.Max(1, landMaxAttempts);
        
        Vector3 point = FindLandPointInternal( areaCenter, areaSize);

        if (point == Vector3.zero)
            Debug.LogWarning("Failed to find a valid land point within the specified area and constraints.");

        return point;
    }

    private void OnEnable()
    {
        landPoints.Clear();
        shorePoints.Clear();
    }

    private Vector3 FindLandPointInternal(Vector2 areaCenter, Vector2 areaSize)
    {
        int totalAttempts = 0;

        while (totalAttempts < landMaxAttempts)
        {
            totalAttempts++;

            if (!TryFindValidPoint(out Vector3 surfacePoint, areaCenter, areaSize))
                continue;

            if (!IsFarFromPoints(surfacePoint, landPoints, landBuildingRadius))
                continue;

            if (HasWaterWithinRadius(surfacePoint, landShoreClearance))
                continue;

            landPoints.Add(surfacePoint);
            return surfacePoint;
        }

        return Vector3.zero;
    }

    private Vector3 FindShorePoint(Vector2 areaCenter, Vector2 areaSize)
    {
        int totalAttempts = 0;

        while (totalAttempts < shoreMaxAttempts)
        {
            totalAttempts++;

            if (!TryFindValidPoint(out Vector3 surfacePoint, areaCenter, areaSize))
                continue;

            if (!IsFarFromPoints(surfacePoint, shorePoints, shoreBuildingRadius))
                continue;

            if (!HasWaterWithinRadius(surfacePoint, shoreDistance))
                continue;

            shorePoints.Add(surfacePoint);
            return surfacePoint;
        }

        return Vector3.zero;
    }

    private bool TryFindValidPoint(out Vector3 point, Vector2 areaCenter, Vector2 areaSize)
    {
        point = default;

        Vector2 sampledPoint = GetRandomXZWithinArea( areaCenter, areaSize);
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
    
    private Vector3 ComputeShoreFacingDirection(Vector3 shorePoint, int sampleCount, float surveyRadius, int minimumSequenceLength)
    {
        // Sample the area around the candidate point, cluster contiguous water samples, and
        // average the large water segments to infer a stable outward-facing direction.
        int samples = Mathf.Clamp(sampleCount, 8, 360);
        float radius = Mathf.Max(1f, surveyRadius);
        int minSequence = Mathf.Max(1, minimumSequenceLength);

        if (samples <= 0)
            return Vector3.zero;

        float step = Mathf.PI * 2f / samples;

        var segments = new List<ShoreSegment>();
        bool hasInitialSegment = false;
        ShoreSegment currentSegment = default;

        for (int i = 0; i < samples; i++)
        {
            float angle = step * i;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 samplePosition = shorePoint + direction * radius;

            SurfaceType surface = GetSurfaceTypeAtPosition(samplePosition);
            bool isWater = surface == SurfaceType.Water;

            if (!hasInitialSegment)
            {
                currentSegment = new ShoreSegment
                {
                    IsWater = isWater,
                    Sum = direction,
                    Length = 1
                };
                hasInitialSegment = true;
                continue;
            }

            if (currentSegment.IsWater == isWater)
            {
                currentSegment.Sum += direction;
                currentSegment.Length += 1;
            }
            else
            {
                segments.Add(currentSegment);
                currentSegment = new ShoreSegment
                {
                    IsWater = isWater,
                    Sum = direction,
                    Length = 1
                };
            }
        }

        if (hasInitialSegment)
            segments.Add(currentSegment);

        if (segments.Count == 0)
            return Vector3.zero;

        if (segments.Count > 1 && segments[0].IsWater == segments[segments.Count - 1].IsWater)
        {
            ShoreSegment merged = segments[0];
            merged.Sum += segments[segments.Count - 1].Sum;
            merged.Length += segments[segments.Count - 1].Length;
            segments[0] = merged;
            segments.RemoveAt(segments.Count - 1);
        }

        Vector3 waterAccumulator = Vector3.zero;
        int waterSamples = 0;

        for (int i = 0; i < segments.Count; i++)
        {
            ShoreSegment segment = segments[i];
            if (!segment.IsWater)
                continue;

            if (segment.Length < minSequence)
                continue;

            Vector3 flattenedDirection = segment.Sum;
            flattenedDirection.y = 0f;

            if (flattenedDirection.sqrMagnitude <= Mathf.Epsilon)
                continue;

            waterAccumulator += flattenedDirection.normalized * segment.Length;
            waterSamples += segment.Length;
        }

        if (waterSamples == 0 || waterAccumulator.sqrMagnitude <= Mathf.Epsilon)
            return Vector3.zero;

        return waterAccumulator.normalized;
    }

    private SurfaceType GetSurfaceTypeAtPosition(Vector3 position)
    {
        Vector3 origin = new Vector3(position.x, raycastHeight, position.z);
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, Mathf.Infinity, EffectiveRaycastMask);

        if (!ValidateRaycasts(hits, out RaycastHit terrainHit, out RaycastHit waterHit))
            return SurfaceType.Unknown;

        if (waterHit.collider != null && waterHit.distance <= terrainHit.distance)
            return SurfaceType.Water;

        return SurfaceType.Terrain;
    }
    private Vector2 GetRandomXZWithinArea(Vector2 areaCenter, Vector2 areaSize)
    {
        Vector2 halfSize = new Vector2(Mathf.Max(0.1f, areaSize.x) * 0.5f, Mathf.Max(0.1f, areaSize.y) * 0.5f);
        float x = Random.Range(areaCenter.x - halfSize.x, areaCenter.x + halfSize.x);
        float z = Random.Range(areaCenter.y - halfSize.y, areaCenter.y + halfSize.y);
        return new Vector2(x, z);
    }

    private int EffectiveRaycastMask => raycastMask.value == 0 ? Physics.DefaultRaycastLayers : raycastMask.value;

    private enum SurfaceType
    {
        Unknown,
        Terrain,
        Water
    }

    private struct ShoreSegment
    {
        public bool IsWater;
        public Vector3 Sum;
        public int Length;
    }
}
