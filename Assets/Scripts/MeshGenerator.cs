using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    MeshRenderer meshRenderer;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    // Grid settings
    public int xSize = 100;
    public int zSize = 100;

    public float scale = 2f;
    public float waveHeight = 4f;

    public Gradient gradient;
    float minTerrainHeight;
    float maxTerrainHeight;

    [Header("Water Settings")]
    public float waterLevel = 0.4f; 
    public Material waterMaterial;  
    GameObject waterMesh;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        meshRenderer = GetComponent<MeshRenderer>();

        CreateShape();
        UpdateMesh();
    }

    // created this to see real time updates
    void Update()
    {
        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        // creating the Vertices
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        uvs = new Vector2[vertices.Length];

        minTerrainHeight = float.MaxValue;
        maxTerrainHeight = float.MinValue;

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float xCoord = (float)x / xSize * scale;
                float zCoord = (float)z / zSize * scale;

                // PerlinNoise returns 0.0 to 1.0 so waveHeight is mutliplies to make it taller

                float y = Mathf.PerlinNoise(xCoord, zCoord) * waveHeight;
                vertices[i] = new Vector3(x, y, z);

                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }

                if (y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }
                uvs[i] = new Vector2((float)x / xSize, (float)z / zSize);
                i++;
            }
        }

        // creating the triangles
        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                // Triangle 1
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                
                // Triangle 2
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals(); 

        ApplyColor();
        CreateWater();

        MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
    }

    void CreateWater()
    {
        // checking if water already exists, if not, create it
        if (waterMesh == null)
        {
            waterMesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterMesh.name = "Water";
            waterMesh.transform.parent = this.transform;
            
            // remove the collider so the player can swim/walk through it
            Destroy(waterMesh.GetComponent<Collider>()); 
            
            waterMesh.GetComponent<Renderer>().material = waterMaterial;
        }

        float xCenter = xSize / 2f;
        float zCenter = zSize / 2f;
        
        float waterY = Mathf.Lerp(minTerrainHeight, maxTerrainHeight, waterLevel);

        waterMesh.transform.position = new Vector3(xCenter, waterY, zCenter);

        // since a plane is 10 units big, we divide our size by 10.
        waterMesh.transform.localScale = new Vector3(xSize / 10f, 1f, zSize / 10f);
    }

    void ApplyColor()
    {
        Texture2D texture = new Texture2D(xSize + 1, zSize + 1);
        Color[] colorMap = new Color[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, vertices[i].y);
                colorMap[i] = gradient.Evaluate(height);
                i++;
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();
        GetComponent<MeshRenderer>().material.mainTexture = texture;
    }
}