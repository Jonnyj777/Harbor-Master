using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class NoiseToTerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private int zSize, xSize;

    [SerializeField]
    private Vector3[] vertices;

    [SerializeField]
    private int[] triangles;

    private Mesh mesh;

    [SerializeField]
    private Texture2D texture;

    [SerializeField]
    private float maxHeight;

    [SerializeField]
    private float waterLevel;

    [SerializeField]
    private Transform waterPlaneTransform;

    [SerializeField]
    private Vector3 meshScale = new Vector3(2.0f, 1.0f, 2.0f);

    [SerializeField]
    private Gradient meshGradient;

    [SerializeField]
    private List<Vector3> edgeVertices = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMeshStructure();
        CreateMeshTriangles();
        UpdateToMesh();
        SetVectorColors();

        waterPlaneTransform.position = new Vector3(waterPlaneTransform.position.x, waterLevel, waterPlaneTransform.position.z);
    }

    private void Update()
    {
        
    }



    private void CreateMeshStructure()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        edgeVertices.Clear();

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                int xNormalized = Mathf.FloorToInt(x / ((float)xSize+1) * texture.width);
                int yNormalized = Mathf.FloorToInt(z / ((float)zSize+1) * texture.height);

                float height = texture.GetPixel(xNormalized, yNormalized).r * maxHeight;
                Vector3 vertex = new Vector3(x, height, z);
                vertices[i] = vertex;

                // Check if this vertex is on the edge
                if ((x == 0 || z == 0 || x == xSize || z == zSize) && height <= waterLevel)
                {
                    edgeVertices.Add(transform.TransformPoint(vertex));
                }

                i++;
            }
        }
    }

    private void CreateMeshTriangles()
    {
        triangles = new int[xSize * zSize * 6];
        int vertexOffset = 0;
        int triangleOffset = 0;

        for(int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                triangles[triangleOffset + 0] = vertexOffset;
                triangles[triangleOffset + 1] = vertexOffset + xSize + 1;
                triangles[triangleOffset + 2] = vertexOffset + 1;
                triangles[triangleOffset + 3] = vertexOffset + 1;
                triangles[triangleOffset + 4] = vertexOffset + + xSize + 1;
                triangles[triangleOffset + 5] = vertexOffset + + xSize + 2;
                vertexOffset++;
                triangleOffset += 6;
            }
            vertexOffset++;
        }
    }

    private void SetVectorColors()
    {
        int numVerts = vertices.Length;
        Color[] vertexColors = new Color[numVerts];
        for(int i = 0; i < numVerts; i++)
        {
            float normalizedHeight = vertices[i].y / maxHeight;
            vertexColors[i] = meshGradient.Evaluate(normalizedHeight);
        }


        mesh.colors = vertexColors;
    }

    private void UpdateToMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        //mesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mesh;

        gameObject.transform.localScale = meshScale;

    }

    public List<Vector3> GetOceanEdgeVertices() => edgeVertices;

    public float GetWaterLevel() => waterLevel;
}
