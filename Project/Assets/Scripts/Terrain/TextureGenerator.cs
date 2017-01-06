using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	public static Texture2D TextureFromColor(Color[] colors, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colors);
		texture.Apply ();
		return texture;
	} 

	public static Texture2D TextureFromHeight(float[,] heights) {

		int width = heights.GetLength(0);
		int height = heights.GetLength(1);

		Color[] colors = new Color[width * height];

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colors [y * width + x] = Color.Lerp (Color.black, Color.white, heights [x, y]);
			}
		}

		return TextureFromColor (colors, width, height);
	}
}
