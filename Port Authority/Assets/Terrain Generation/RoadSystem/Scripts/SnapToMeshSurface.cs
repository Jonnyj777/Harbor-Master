using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SnapToMeshSurface : MonoBehaviour
{
    private SplineContainer splineContainer;

    [SerializeField]
    private NoiseToTerrainGenerator terrainGenerator;

    [SerializeField]
    private float heightOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        splineContainer = GetComponent<SplineContainer>();
        SnapToMesh();
    }

    private void SnapToMesh()
    {
        for(int vI = 0; vI < terrainGenerator.mesh.vertexCount; vI++)
        {
            for(int sI = 0; sI < splineContainer.Spline.Count; sI++)
            {
                if (splineContainer.Spline[sI].Position.x == terrainGenerator.mesh.vertices[vI].x && splineContainer.Spline[sI].Position.z == terrainGenerator.mesh.vertices[vI].z)
                {
                    Unity.Mathematics.float3 newPos = new Unity.Mathematics.float3(splineContainer.Spline[sI].Position.x, terrainGenerator.mesh.vertices[vI].y + heightOffset, splineContainer.Spline[sI].Position.z);
                    splineContainer.Spline[sI] = new BezierKnot(newPos);
                }
            }
        }
    }
}
