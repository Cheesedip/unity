using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class TileMap : MonoBehaviour
{
	private Utilities utilities;
	public static TileMap instance;

	private TileData tileData;
	public Texture2D tileMapTexture;
	private List<List<Vector2>> uvTileRefs;

	private float[,] heightMap;
	private int[,] textureMap;
	private Vector3[,] normalMap;

	public int mapTileWidth, mapTileHeight; // width = x, height = z
	private float planeOffsetX = 0f, planeOffsetZ = 0f;
	private float tileWidth = 1.0f, tileHeight = 1.0f;
	private float maxHeight, minHeight;

	// perlin noise
	public float heightScale, noiseScale;
	private float noiseOffsetX, noiseOffsetZ;

	void Awake()
	{
		// Set static MainGameManager instance
		utilities = new Utilities();
		instance = this;
		BuildHeightMap();
		GenerateSmoothedNormalMap();
		BuildTexture();
		BuildMesh();
	}

	// Use this for initialization
	void Start()
	{
	}

	void BuildHeightMap()
	{
		maxHeight = 0f;
		minHeight = heightScale;

		noiseOffsetX = Random.Range(0f, 99999f);
		noiseOffsetZ = Random.Range(0f, 99999f);

		heightMap = new float[mapTileHeight + 1, mapTileWidth + 1];

		for (int verticeIndex_z = 0; verticeIndex_z < mapTileHeight + 1; verticeIndex_z++)
		{
			for (int verticeIndex_x = 0; verticeIndex_x < mapTileWidth + 1; verticeIndex_x++)
			{
				float height = GenerateHeight(verticeIndex_z, verticeIndex_x);
				heightMap[verticeIndex_z, verticeIndex_x] = height;

				if (height < minHeight)
				{
					minHeight = height;
				}
				if (height > maxHeight)
				{
					maxHeight = height;
				}
			}
		}
		//Debug.Log("Max Height: " + maxHeight + ", Min Height: " + minHeight);
	}

	void GenerateSmoothedNormalMap(){
		normalMap = new Vector3[mapTileHeight + 1, mapTileWidth + 1];
		for (int verticeIndex_z = 0; verticeIndex_z < mapTileHeight + 1; verticeIndex_z++)
        {
            for (int verticeIndex_x = 0; verticeIndex_x < mapTileWidth + 1; verticeIndex_x++)
            {
				normalMap[verticeIndex_z, verticeIndex_x] = CalculateSmoothedNormal(verticeIndex_z, verticeIndex_x);
            }
        }
	}

    void BuildTexture()
	{
		tileData = new TileData();
		uvTileRefs = tileData.GetUVTileRefs();
		//tileMapTexture = tileData.GetTileMapTexture();
	}


	void BuildMesh()
	{
		// Initialize data storage
		textureMap = new int[mapTileHeight, mapTileWidth];
		int numVertices = (mapTileWidth) * (mapTileHeight) * 6;
		Vector3[] vertices = new Vector3[numVertices];

		int numTriangles = mapTileWidth * mapTileHeight * 2;
		int[] triangles = new int[numTriangles * 3];

		Vector3[] normals = new Vector3[numVertices];
		Vector2[] uvs = new Vector2[numVertices];

		// Generate Mesh Data

		// Run through all vertices
		for (int tileIndex_z = 0; tileIndex_z < mapTileHeight; tileIndex_z++)
		{
			for (int tileIndex_x = 0; tileIndex_x < mapTileWidth; tileIndex_x++)
			{
				float baseX = planeOffsetX + tileIndex_x * tileWidth;
				float baseZ = planeOffsetZ + tileIndex_z * tileHeight;

				int baseIndex = (tileIndex_z * mapTileHeight + tileIndex_x) * 6;

				float[] heights = {GetHeight(tileIndex_z, tileIndex_x, 0),
					GetHeight(tileIndex_z, tileIndex_x, 1),
					GetHeight(tileIndex_z, tileIndex_x, 2),
					GetHeight(tileIndex_z, tileIndex_x, 3)};

				// Set vertices  
				vertices[baseIndex] = new Vector3(baseX, heights[0], baseZ);
				vertices[baseIndex + 1] = new Vector3(baseX, heights[1], baseZ + tileHeight);
				vertices[baseIndex + 2] = new Vector3(baseX + tileWidth, heights[2], baseZ + tileHeight);
				vertices[baseIndex + 3] = new Vector3(baseX, heights[0], baseZ);
				vertices[baseIndex + 4] = new Vector3(baseX + tileWidth, heights[2], baseZ + tileHeight);
				vertices[baseIndex + 5] = new Vector3(baseX + tileWidth, heights[3], baseZ);

				// Set normals
				normals[baseIndex] = normalMap[tileIndex_z, tileIndex_x];
				normals[baseIndex + 1] = normalMap[tileIndex_z + 1, tileIndex_x];
				normals[baseIndex + 2] = normalMap[tileIndex_z + 1, tileIndex_x + 1];
				normals[baseIndex + 3] = normalMap[tileIndex_z, tileIndex_x];;
				normals[baseIndex + 4] = normalMap[tileIndex_z + 1, tileIndex_x + 1];
				normals[baseIndex + 5] = normalMap[tileIndex_z, tileIndex_x + 1];


				// Select Texture for both triangles (Now implemented that texture is selected per tile, not per triangle)
				float[] heights1 = { heights[0], heights[1], heights[2] };
				float[] heights2 = { heights[0], heights[2], heights[3] };
				int textureID1 = GetTextureID(heights1);
				int textureID2 = textureID1;
				List<Vector2> UVs1 = uvTileRefs[textureID1];
				List<Vector2> UVs2 = uvTileRefs[textureID2];

				// Set tileID in tileMap
				textureMap[tileIndex_z, tileIndex_x] = textureID1;

				// Set uvs
				uvs[baseIndex] = UVs1[0];
				uvs[baseIndex + 1] = UVs1[1];
				uvs[baseIndex + 2] = UVs1[2];
				uvs[baseIndex + 3] = UVs2[0];
				uvs[baseIndex + 4] = UVs2[2];
				uvs[baseIndex + 5] = UVs2[3];

				// Set triangles
				int triangleBaseIndex = (tileIndex_z * mapTileHeight + tileIndex_x) * 2 * 3;
				triangles[triangleBaseIndex] = baseIndex;
				triangles[triangleBaseIndex + 1] = baseIndex + 1;
				triangles[triangleBaseIndex + 2] = baseIndex + 2;
				triangles[triangleBaseIndex + 3] = baseIndex + 3;
				triangles[triangleBaseIndex + 4] = baseIndex + 4;
				triangles[triangleBaseIndex + 5] = baseIndex + 5;

			}
		}

		// Create a new Mesh
		Mesh mesh = new Mesh();

		// Create a new mesh and populate with data
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uvs;

		// Assign our mesh to our filter/renderer/collider
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		meshFilter.mesh = mesh;
		meshRenderer.material.mainTexture = tileMapTexture;
		meshRenderer.material.SetFloat("_Metallic", 0f);
		meshRenderer.material.SetFloat("_Glossiness", 0f);
	}

	Vector3 CalculateSmoothedNormal(int verticeIndex_z, int verticeIndex_x){
		// Calculate the normals vertice using heightmap
		Vector3 left = new Vector3(Mathf.Max(verticeIndex_x - 1, 0), heightMap[verticeIndex_z, Mathf.Max(verticeIndex_x - 1, 0)], verticeIndex_z);
		Vector3 right = new Vector3(Mathf.Min(verticeIndex_x + 1, mapTileWidth), heightMap[verticeIndex_z, Mathf.Min(verticeIndex_x + 1, mapTileWidth)], verticeIndex_z);
		Vector3 up = new Vector3(verticeIndex_x, heightMap[Mathf.Min(verticeIndex_z + 1, mapTileHeight), verticeIndex_x] , Mathf.Min(verticeIndex_z + 1, mapTileHeight));
        Vector3 down = new Vector3(verticeIndex_x, heightMap[Mathf.Max(verticeIndex_z - 1, 0), verticeIndex_x], Mathf.Max(verticeIndex_z - 1, 0));

		Vector3 normal = Vector3.Cross(right - left, up - down);
		return normal;
  }

	float GenerateHeight(int verticeIndex_z, int verticeIndex_x)
    {
        float perlin_z = (float)verticeIndex_z / (mapTileHeight + 1) * noiseScale + noiseOffsetZ;
        float perlin_x = (float)verticeIndex_x / (mapTileWidth + 1) * noiseScale + noiseOffsetX;
        float height = Mathf.PerlinNoise(perlin_z, perlin_x);
        height = height * heightScale;
        //Debug.Log("height: " + height);
        return height;
    }

    float GetHeight(int tileIndex_z, int tileIndex_x, uint verticeID)
    {
        switch (verticeID)
        {
            case 0:
                return heightMap[tileIndex_z, tileIndex_x];
            case 1:
                return heightMap[tileIndex_z + 1, tileIndex_x];
            case 2:
                return heightMap[tileIndex_z + 1, tileIndex_x + 1];
            case 3:
                return heightMap[tileIndex_z, tileIndex_x + 1];
        }
        Debug.Log("Error, this is not supposed to happen!, see GetHeight function");
        return 0f; // to shut up c# compiler
    }


	int GetTextureID(float [] heights){
		
		float averageHeight = utilities.ArraySum(heights) / heights.Length;
		float range = maxHeight - minHeight;
		if ((averageHeight - minHeight) < range * 0.1){
			return 0;
		}
		if ((averageHeight - minHeight) < range * 0.15)
        {
            return 1;
        }
		if ((averageHeight - minHeight) < range * 0.35)
        {
            return 2;
        }
		if ((averageHeight - minHeight) < range * 0.45)
        {
            return 3;
        }
		if ((averageHeight - minHeight) < range * 0.65)
        {
            return 4;
        }
		if ((averageHeight - minHeight) < range * 0.75)
        {
            return 5;
        }
		if ((averageHeight - minHeight) < range * 0.85)
        {
            return 6;
        }
		return 7;
	}
    
	public float[,] GetHeightMap(){
		return heightMap;
	}
	public int[,] GetTextureMap(){
		return textureMap;
	}

	public int GetMapTileHeight(){
		return mapTileHeight;
	}

	public int GetMapTileWidth(){
		return mapTileWidth;
    }

	public float GetTileWidth(){
		return tileWidth;
	}

	public float GetTileHeight(){
		return tileHeight;
	}
}
