using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Trilinear;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colourMap);
		texture.Apply ();
		return texture;
	}


	public static Texture2D TextureFromHeightMap(float[,] heightMap) {
		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, heightMap [x, y]);
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

	// public static Texture2D TextureFromIslandHeights(int divisions, int meshRings, IslandMeshData meshData) {
	
	// 	Color[] colourMap = new Color[divisions * meshRings - (divisions - 1)];
	// 	for (int y = 0; y < divisions; y++) {
	// 		for (int x = 0; x < meshRings; x++) {
	// 			colourMap [y * meshRings + x] = Color.Lerp (Color.black, Color.white, meshData.vertices[y * meshRings + x].y);
	// 		}
	// 	}

	// 	return TextureFromColourMap (colourMap, width, height);
	// }

}
