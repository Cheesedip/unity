using UnityEngine;
using System.Collections.Generic;

public class TileData{

    private Texture2D tileMapTexture;
	private List<List<Vector2>> uvTileRefs;
	private int numTextures = 8;
    
	public TileData(){
		uvTileRefs = new List<List<Vector2>>();
		for (int i = 0; i < numTextures; i++){
			List<Vector2> textureRefs = new List<Vector2>();
			textureRefs.Add(new Vector2((float) i / numTextures, 1f)); // bottom left
			textureRefs.Add(new Vector2((float) i / numTextures, 0f)); // top left
			textureRefs.Add(new Vector2((float)(i+1) / numTextures, 0f)); // top right
			textureRefs.Add(new Vector2((float)(i+1) / numTextures, 1f)); // bottom right
			uvTileRefs.Add(textureRefs);
        }
	}

	public List<List<Vector2>> GetUVTileRefs(){
		return uvTileRefs;
	}

	public Texture2D GetTileMapTexture(){
		return tileMapTexture;
	}
}
