using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	public Renderer rend;
	public MeshFilter meshFilter;
	public MeshRenderer meshRend;

	public void DrawTexture(Texture2D texture) {
		rend.sharedMaterial.mainTexture = texture;
		rend.transform.localScale = new Vector3 (texture.width, 1, texture.height);
	}

	public void DrawMesh(MeshData meshData, Texture2D texture) {
		meshFilter.sharedMesh = meshData.GetMesh ();
		meshRend.sharedMaterial.mainTexture = texture;
	}
}
