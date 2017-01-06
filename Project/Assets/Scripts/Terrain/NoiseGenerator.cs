using UnityEngine;
using System.Collections;

public static class NoiseGenerator {

	public enum Normalize {Local, Global};

	public static float[,] GenerateNoise(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 off, Normalize normalize) {
		
		float[,] noise = new float[width, height];

		System.Random prng = new System.Random (seed);
		Vector2[] offsets = new Vector2[octaves];

		float maxPossibleHeight = 0;
		float amplitude = 1;
		float frequency = 1;

		for (int i = 0; i < octaves; i++) {
			float offX = prng.Next (-100000, 100000) + off.x;
			float offY = prng.Next (-100000, 100000) - off.y;
			offsets [i] = new Vector2 (offX, offY);

			maxPossibleHeight += amplitude;
			amplitude *= persistance;
		}

		if (scale <= 0) {
			scale = 0.0001f;
		}

		float maxLocalHeight = float.MinValue;
		float minLocalHeight = float.MaxValue;

		float halfW = width / 2f;
		float halfH = height / 2f;

		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {

				amplitude = 1;
				frequency = 1;
				float noiseHeight = 0;

				for(int i = 0; i < octaves; i++) {
					
					float sampleX = (x-halfW + offsets[i].x) / scale * frequency;
					float sampleY = (y-halfH + offsets[i].y) / scale * frequency;

					float perlin = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;

					noiseHeight += perlin * amplitude;

					amplitude *= persistance;
					frequency *= lacunarity;
				}

				if (noiseHeight > maxLocalHeight) {
					maxLocalHeight = noiseHeight;
				} else if (noiseHeight < minLocalHeight) {
					minLocalHeight = noiseHeight;
				}

				noise [x, y] = noiseHeight;
			}
		}

		if (normalize == Normalize.Local) {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					noise [x, y] = Mathf.InverseLerp (minLocalHeight, maxLocalHeight, noise [x, y]);
				}
			}
		} else {
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					noise [x, y] = Mathf.Clamp((noise [x, y] + 1) / (maxPossibleHeight), 0, int.MaxValue);
				}
			}
		}
		return noise;
	}
}
