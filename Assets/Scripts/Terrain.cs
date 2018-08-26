using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour {
	
	private float[,] heightMap;
    private int[,] textureMap;

	private int mapTileWidth, mapTileHeight;
	private float tileWidth, tileHeight;

    // Terrain objects
    public GameObject treePrefab;

	void Start(){
		textureMap = TileMap.instance.GetTextureMap();
		heightMap = TileMap.instance.GetHeightMap();
		mapTileHeight = TileMap.instance.GetMapTileHeight();
		mapTileWidth = TileMap.instance.GetMapTileWidth();
		tileWidth = TileMap.instance.GetTileWidth();
		tileHeight = TileMap.instance.GetTileHeight();
	}

	void GenerateTerrain()
    {
        // Loop through all tiles
        for (int tileIndex_z = 0; tileIndex_z < mapTileHeight; tileIndex_z++)
        {
            for (int tileIndex_x = 0; tileIndex_x < mapTileWidth; tileIndex_x++)
            {
                if (textureMap[tileIndex_z, tileIndex_x] != 4)
                    continue;

                int random = Random.Range(0, 10);
                if (random < 9)
                    continue;
                float tileCentreHeight = GetTileHeight(tileIndex_z, tileIndex_x);
                Vector3 tileCentre = new Vector3((float)(tileIndex_x + 0.5) * tileWidth, tileCentreHeight, (float)(tileIndex_z + 0.5) * tileHeight);
                Instantiate(treePrefab, tileCentre, treePrefab.transform.rotation);
            }
        }

    }

    float GetTileHeight(int tileIndex_z, int tileIndex_x)
    {
        float[] tileVertices = {heightMap[tileIndex_z, tileIndex_x],
            heightMap[tileIndex_z + 1, tileIndex_x],
            heightMap[tileIndex_z, tileIndex_x + 1],
            heightMap[tileIndex_z + 1, tileIndex_x + 1]};
        return Utilities.instance.ArraySum(tileVertices) / 4;
    }
}
