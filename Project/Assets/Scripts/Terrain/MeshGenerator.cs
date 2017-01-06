using UnityEngine;
using System.Collections;

public static class MeshGenerator {

	public static MeshData GenerateTerrainMesh(float[,] heights, float heightMult, AnimationCurve heightCurve, int levelOfDetail) {
		AnimationCurve hCurve = new AnimationCurve (heightCurve.keys);
		int width = heights.GetLength (0);
		int height = heights.GetLength (1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		int increment = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
		int vertices = (width - 1) / increment + 1;

		MeshData meshData = new MeshData (vertices, vertices);
		int index = 0;

		for (int y = 0; y < height; y += increment) {
			for (int x = 0; x < width; x += increment) {

				meshData.vertices [index] = new Vector3 (topLeftX + x, hCurve.Evaluate(heights [x, y]) * heightMult, topLeftZ - y);
				meshData.uvs [index] = new Vector2 (x / (float)width, y / (float)height);

				if (x < width - 1 && y < height - 1) {
					meshData.AddTriangle (index, index + vertices + 1, index + vertices);
					meshData.AddTriangle (index + vertices + 1, index, index + 1);
				}

				index++;
			}
		}
		return meshData;
	}
}

public class MeshData {
	
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int width, int height) {
		vertices = new Vector3[width * height];
		uvs = new Vector2[width * height];
		triangles = new int[(width - 1) * (height - 1) * 6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

	public Mesh GetMesh() {
		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals ();
		return mesh;
	}
}