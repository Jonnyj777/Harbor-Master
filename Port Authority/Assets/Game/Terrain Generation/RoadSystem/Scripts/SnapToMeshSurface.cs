using UnityEngine;
using UnityEngine.Splines;

//[ExecuteInEditMode]
public class SnapToMeshSurface : MonoBehaviour
{
    private SplineContainer splineContainer;

    [SerializeField]
    private GameObject splineObject;

    [SerializeField]
    private GameObject terrainObject;

    [SerializeField]
    private float heightOffset;

    [SerializeField]
    private float raycastHeight = 20f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()    
    {
        var target = splineObject != null ? splineObject : gameObject;
        splineContainer = target.GetComponent<SplineContainer>();
        if (splineContainer != null)
        {
            SnapToMesh();
        }
    }

    private void SnapToMesh()
    {
        var terrain = terrainObject != null ? terrainObject : GameObject.FindWithTag("Terrain");
        if (terrain == null)
        {
            return;
        }

        var meshFilter = terrain.GetComponent<MeshFilter>();
        var terrainCollider = terrain.GetComponent<Collider>();
        if ((meshFilter == null || meshFilter.sharedMesh == null) && terrainCollider == null)
        {
            return;
        }

        var splineTransform = splineContainer.transform;

        var spline = splineContainer.Spline;
        for(int sI = 0; sI < spline.Count; sI++)
        {
            var knot = spline[sI];
            var knotWorld = splineTransform.TransformPoint((Vector3)knot.Position);

            bool snapped = false;

            if (terrainCollider != null)
            {
                var origin = knotWorld + Vector3.up * raycastHeight;
                if (terrainCollider.Raycast(new Ray(origin, Vector3.down), out var hit, raycastHeight * 2f))
                {
                    var newWorldPos = hit.point + Vector3.up * heightOffset;
                    var newLocalPos = splineTransform.InverseTransformPoint(newWorldPos);
                    Unity.Mathematics.float3 newPos = new Unity.Mathematics.float3(newLocalPos.x, newLocalPos.y, newLocalPos.z);
                    spline[sI] = new BezierKnot(newPos, knot.TangentIn, knot.TangentOut);
                    snapped = true;
                }
            }

            if (!snapped && meshFilter != null && meshFilter.sharedMesh != null)
            {
                var terrainMesh = meshFilter.sharedMesh;
                var terrainTransform = meshFilter.transform;
                for(int vI = 0; vI < terrainMesh.vertexCount; vI++)
                {
                    var terrainVertexWorld = terrainTransform.TransformPoint(terrainMesh.vertices[vI]);
                    if (Mathf.Approximately(knotWorld.x, terrainVertexWorld.x) && Mathf.Approximately(knotWorld.z, terrainVertexWorld.z))
                    {
                        var newWorldPos = new Vector3(knotWorld.x, terrainVertexWorld.y + heightOffset, knotWorld.z);
                        var newLocalPos = splineTransform.InverseTransformPoint(newWorldPos);
                        Unity.Mathematics.float3 newPos = new Unity.Mathematics.float3(newLocalPos.x, newLocalPos.y, newLocalPos.z);
                        spline[sI] = new BezierKnot(newPos, knot.TangentIn, knot.TangentOut);
                        break;
                    }
                }
            }
        }
    }
}
