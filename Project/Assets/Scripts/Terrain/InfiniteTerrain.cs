using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiniteTerrain : MonoBehaviour {

	const float scale = .5f;

	const float updateThresh = 25f;
	const float updateThreshSqr = updateThresh * updateThresh;

	public LODInfo[] detailLevels;
	public static float maxViewDist;

	public static Transform viewer;
	public Material material;

	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator map;
	int chunkSize;
	int chunksVisible;

	Dictionary<Vector2, TerrainChunk> chunkDict = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> lastChunks = new List<TerrainChunk>();

	void Start() {
		map = FindObjectOfType<MapGenerator> ();
		maxViewDist = detailLevels [detailLevels.Length - 1].distThresh;
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisible = Mathf.RoundToInt (maxViewDist / chunkSize);

		UpdateVisibleChunks ();
	}

	void Update() {
		if (viewer != null) {
			viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale;

			if ((viewerPositionOld - viewerPosition).sqrMagnitude > updateThreshSqr) {
				viewerPositionOld = viewerPosition;
				UpdateVisibleChunks ();
			}
		}
	}

	void UpdateVisibleChunks() {

		for (int i = 0; i < lastChunks.Count; i++) {
			lastChunks [i].SetVisible (false);
		}
		lastChunks.Clear ();

		int currentChunkX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOff = -chunksVisible; yOff <= chunksVisible; yOff++) {
			for(int xOff = -chunksVisible; xOff <= chunksVisible; xOff++) {
				
				Vector2 viewedChunk = new Vector2 (currentChunkX + xOff, currentChunkY + yOff);

				if (chunkDict.ContainsKey (viewedChunk)) {
					chunkDict [viewedChunk].UpdateChunk ();
				} else {
					chunkDict.Add (viewedChunk, new TerrainChunk (viewedChunk, chunkSize, detailLevels, transform, material));
				}
			}
		}
	}

	public class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;

		MeshRenderer rend;
		MeshFilter filter;
		MeshCollider collider;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool dataRecieved;

		int prevIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.detailLevels = detailLevels;

			position = coord * size;
			bounds = new Bounds(position, Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x, 0, position.y);

			meshObject = new GameObject("Chunk");
			rend = meshObject.AddComponent<MeshRenderer>();
			filter = meshObject.AddComponent<MeshFilter>();
			collider = meshObject.AddComponent<MeshCollider>();
			rend.material = material;

			meshObject.transform.position = positionV3 * scale;
			meshObject.transform.parent = parent;
			meshObject.transform.localScale = Vector3.one * scale;
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for(int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
			}

			map.RequestMapData(position, OnMapDataRecieved);
		}

		void OnMapDataRecieved(MapData mapData) {
			//map.RequestMeshData (mapData, OnMeshDataRecieved);
			this.mapData = mapData;
			dataRecieved = true;
			Texture2D texture = TextureGenerator.TextureFromColor (mapData.colors, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
			rend.material.mainTexture = texture;
			UpdateChunk ();
		}

		public void UpdateChunk() {
			if (dataRecieved) {
				float viewerDist = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
				bool visible = viewerDist <= maxViewDist;

				if (visible) {
					int index = 0;
					for (int i = 0; i < detailLevels.Length - 1; i++) {
						if (viewerDist > detailLevels [i].distThresh) {
							index = i + 1;
						} else {
							break;
						}
					}

					if (index != prevIndex) {
						LODMesh lodMesh = lodMeshes [index];
						if (lodMesh.meshRecieved) {
							prevIndex = index;
							collider.sharedMesh = lodMesh.mesh;
							filter.mesh = lodMesh.mesh;
						} else if (!lodMesh.meshRequested) {
							lodMesh.RequestMesh (mapData);
						}
					}
					lastChunks.Add (this);
				}

				SetVisible (visible);
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}
	}

	class LODMesh {
		public Mesh mesh;
		public bool meshRequested;
		public bool meshRecieved;
		int lod;
		System.Action updateCallback;

		public LODMesh(int levelOfDetail, System.Action updateCallback) {
			this.lod = levelOfDetail;
			this.updateCallback = updateCallback;
		}

		void OnMeshDataRecieved(MeshData meshData) {
			mesh = meshData.GetMesh ();
			this.meshRecieved = true;

			updateCallback ();
		}

		public void RequestMesh(MapData mapData) {
			this.meshRequested = true;
			map.RequestMeshData (mapData, lod, OnMeshDataRecieved);
		}
	}

	[System.Serializable]
	public struct LODInfo {
		public int lod;
		public float distThresh;
	}
}
