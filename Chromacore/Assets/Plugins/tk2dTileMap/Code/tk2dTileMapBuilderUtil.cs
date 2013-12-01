using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || !UNITY_FLASH

namespace tk2dRuntime.TileMap
{
	public static class BuilderUtil
	{
		/// Syncs layer data and makes sure data is valid
		public static bool InitDataStore(tk2dTileMap tileMap)
		{
			bool dataChanged = false;
			int numLayers = tileMap.data.NumLayers;
			if (tileMap.Layers == null)
			{
				tileMap.Layers = new Layer[numLayers];
				for (int i = 0; i < numLayers; ++i)
					tileMap.Layers[i] = new Layer(tileMap.data.Layers[i].hash, tileMap.width, tileMap.height, tileMap.partitionSizeX, tileMap.partitionSizeY);
				dataChanged = true;
			}
			else
			{
				// link up layer hashes
				Layer[] newLayers = new Layer[numLayers];
				
				for (int i = 0; i < numLayers; ++i)
				{
					var layerInfo = tileMap.data.Layers[i];
					bool found = false;
	
					// Find an existing layer with this hash
					for (int j = 0; j < tileMap.Layers.Length; ++j)
					{
						if (tileMap.Layers[j].hash == layerInfo.hash)
						{
							newLayers[i] = tileMap.Layers[j];
							found = true;
							break;
						}
					}
					
					if (!found)
						newLayers[i] = new Layer(layerInfo.hash, tileMap.width, tileMap.height, tileMap.partitionSizeX, tileMap.partitionSizeY);
				}
					
				// Identify if it has changed
				int numActiveLayers = 0;
				foreach (var layer in newLayers)
					if (!layer.IsEmpty)
						numActiveLayers++;
				
				int numPreviousActiveLayers = 0;
				foreach (var layer in tileMap.Layers)
				{
					if (!layer.IsEmpty)
						numPreviousActiveLayers++;
				}
				
				if (numActiveLayers != numPreviousActiveLayers)
					dataChanged = true;
				
				tileMap.Layers = newLayers;
			}
			
			if (tileMap.ColorChannel == null)
			{
				tileMap.ColorChannel = new ColorChannel(tileMap.width, tileMap.height, tileMap.partitionSizeX, tileMap.partitionSizeY);
			}
			
			return dataChanged;
		}		

		static List<int> TilePrefabsX;
		static List<int> TilePrefabsY;
		static List<int> TilePrefabsLayer;
		static List<GameObject> TilePrefabsInstance;

		static GameObject GetExistingTilePrefabInstance(tk2dTileMap tileMap, int tileX, int tileY, int tileLayer) {
			int n = tileMap.GetTilePrefabsListCount();
			for (int i = 0; i < n; ++i) {
				int x, y, layer;
				GameObject instance;
				tileMap.GetTilePrefabsListItem(i, out x, out y, out layer, out instance);
				if (x == tileX && y == tileY && layer == tileLayer)
					return instance;
			}
			return null;
		}
		
		/// Spawns all prefabs for a given chunk
		/// Expects the chunk to have a valid GameObject
		public static void SpawnPrefabsForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, int layer, int[] prefabCounts)
		{
			var chunkData = chunk.spriteIds;
			var tilePrefabs = tileMap.data.tilePrefabs;
			Vector3 tileSize = tileMap.data.tileSize;
			var parent = chunk.gameObject.transform;
			
			float xOffsetMult = 0.0f, yOffsetMult = 0.0f;
			tileMap.data.GetTileOffset(out xOffsetMult, out yOffsetMult);

			for (int y = 0; y < tileMap.partitionSizeY; ++y)
			{
				float xOffset = ((baseY + y) & 1) * xOffsetMult;
				for (int x = 0; x < tileMap.partitionSizeX; ++x)
				{
					int tile = GetTileFromRawTile(chunkData[y * tileMap.partitionSizeX + x]);
					if (tile < 0 || tile >= tilePrefabs.Length)
						continue;

					Object prefab = tilePrefabs[tile];
					if (prefab != null)
					{
						prefabCounts[tile]++;

						GameObject instance = GetExistingTilePrefabInstance(tileMap, baseX + x, baseY + y, layer);
						bool foundExisting = (instance != null);

						if (instance == null) {
					#if UNITY_EDITOR && !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
							instance = UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject;
					#else
							instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
					#endif
						}

						if (instance != null) {
							GameObject prefabGameObject = prefab as GameObject;

							Vector3 pos = new Vector3(tileSize.x * (x + xOffset), tileSize.y * y, 0);
							bool enablePrefabOffset = false;
							var tileInfo = tileMap.data.GetTileInfoForSprite(tile);
							if (tileInfo != null)
								enablePrefabOffset = tileInfo.enablePrefabOffset;
							if (enablePrefabOffset && prefabGameObject != null)
								pos += prefabGameObject.transform.position;

							if (!foundExisting)
								instance.name = prefab.name + " " + prefabCounts[tile].ToString();

							instance.transform.parent = parent;
							instance.transform.localPosition = pos;

							// Add to tilePrefabs list
							TilePrefabsX.Add(baseX + x);
							TilePrefabsY.Add(baseY + y);
							TilePrefabsLayer.Add(layer);
							TilePrefabsInstance.Add(instance);
						}
					}
				}
			}
		}
		
		/// Spawns all prefabs for a given tilemap
		/// Expects populated chunks to have valid GameObjects
		public static void SpawnPrefabs(tk2dTileMap tileMap, bool forceBuild) 
		{
			// Restart these lists that will be stored in the tileMap tilePrefabsList
			TilePrefabsX = new List<int>();
			TilePrefabsY = new List<int>();
			TilePrefabsLayer = new List<int>();
			TilePrefabsInstance = new List<GameObject>();

			int[] prefabCounts = new int[tileMap.data.tilePrefabs.Length];

			int numLayers = tileMap.Layers.Length;
			for (int layerId = 0; layerId < numLayers; ++layerId)
			{
				var layer = tileMap.Layers[layerId];
				var layerData = tileMap.data.Layers[layerId];

				// We skip offsetting the first one
				if (layer.IsEmpty || layerData.skipMeshGeneration)
					continue;

				for (int cellY = 0; cellY < layer.numRows; ++cellY)
				{
					int baseY = cellY * layer.divY;
					for (int cellX = 0; cellX < layer.numColumns; ++cellX)
					{
						int baseX = cellX * layer.divX;
						var chunk = layer.GetChunk(cellX, cellY);
						if (chunk.IsEmpty)
							continue;
						if (!forceBuild && !chunk.Dirty)
							continue;
						
						SpawnPrefabsForChunk(tileMap, chunk, baseX, baseY, layerId, prefabCounts);
					}
				}
			}

			tileMap.SetTilePrefabsList(TilePrefabsX, TilePrefabsY, TilePrefabsLayer, TilePrefabsInstance);
		}

		/// <summary>
		/// Moves the chunk's gameobject's children to the prefab root
		/// </summary>
		public static void HideTileMapPrefabs(tk2dTileMap tileMap) {
			if (tileMap.renderData == null) {
				// No Render Data to parent Prefab Root to
				return;
			} else {
				if (tileMap.PrefabsRoot == null) {
					var go = tileMap.PrefabsRoot = new GameObject("Prefabs");
					go.transform.parent = tileMap.renderData.transform;
					go.transform.localPosition = Vector3.zero;
					go.transform.localRotation = Quaternion.identity;
					go.transform.localScale = Vector3.one;
				}
			}

			if (tileMap.Layers == null)
				return;

			int instListCount = tileMap.GetTilePrefabsListCount();
			bool[] instExists = new bool[instListCount];

			for (int i = 0; i < tileMap.Layers.Length; ++i) {
				var layer = tileMap.Layers[i];
				for (int j = 0; j < layer.spriteChannel.chunks.Length; ++j) {
					var chunk = layer.spriteChannel.chunks[j];
					if (chunk.gameObject == null)
						continue;

					var t = chunk.gameObject.transform;
					int childCount = t.childCount;
					for (int k = 0; k < childCount; ++k) {
						GameObject go = t.GetChild(k).gameObject;
						for (int q = 0; q < instListCount; ++q) {
							int x, y, layerIdx;
							GameObject instance;
							tileMap.GetTilePrefabsListItem(q, out x, out y, out layerIdx, out instance);
							if (instance == go) {
								instExists[q] = true;
								break;
							}
						}
					}
				}
			}

			List<int> tileX = new List<int>();
			List<int> tileY = new List<int>();
			List<int> tileLayer = new List<int>();
			List<GameObject> tileInst = new List<GameObject>();
			for (int i = 0; i < instListCount; ++i) {
				if (instExists[i]) {
					int x, y, layerIdx;
					GameObject instance;
					tileMap.GetTilePrefabsListItem(i, out x, out y, out layerIdx, out instance);
					tileX.Add(x);
					tileY.Add(y);
					tileLayer.Add(layerIdx);
					tileInst.Add(instance);

					instance.transform.parent = tileMap.PrefabsRoot.transform;
				}
			}
			tileMap.SetTilePrefabsList(tileX, tileY, tileLayer, tileInst);
		}
		
		static Vector3 GetTilePosition(tk2dTileMap tileMap, int x, int y)
		{
			return new Vector3(tileMap.data.tileSize.x * x, tileMap.data.tileSize.y * y, 0.0f);	
		}

		/// Creates render data for given tilemap
		public static void CreateRenderData(tk2dTileMap tileMap, bool editMode)
		{
			// Create render data
			if (tileMap.renderData == null)
				tileMap.renderData = new GameObject(tileMap.name + " Render Data");
	
			tileMap.renderData.transform.position = tileMap.transform.position;
			
			float accumulatedLayerZ = 0.0f;
			
			// Create all objects
			int layerId = 0;
			foreach (var layer in tileMap.Layers)
			{
				// We skip offsetting the first one
				float layerInfoZ = tileMap.data.Layers[layerId].z;
				if (layerId != 0)
					accumulatedLayerZ -= layerInfoZ;
				
				if (layer.IsEmpty && layer.gameObject != null)
				{
					GameObject.DestroyImmediate(layer.gameObject);
					layer.gameObject = null;
				}
				else if (!layer.IsEmpty && layer.gameObject == null)
				{
					var go = layer.gameObject = new GameObject("");
					go.transform.parent = tileMap.renderData.transform;
				}
				
				int unityLayer = tileMap.data.Layers[layerId].unityLayer;
				
				if (layer.gameObject != null)
				{
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
					if (!editMode && layer.gameObject.active == false)
						layer.gameObject.SetActiveRecursively(true);
#else
					if (!editMode && layer.gameObject.activeSelf == false)
						layer.gameObject.SetActive(true);
#endif
					
					layer.gameObject.name = tileMap.data.Layers[layerId].name;
					layer.gameObject.transform.localPosition = new Vector3(0, 0, tileMap.data.layersFixedZ ? (-layerInfoZ) : accumulatedLayerZ);
					layer.gameObject.transform.localRotation = Quaternion.identity;
					layer.gameObject.transform.localScale = Vector3.one;
					layer.gameObject.layer = unityLayer;
				}
				
				int x0, x1, dx;
				int y0, y1, dy;
				BuilderUtil.GetLoopOrder(tileMap.data.sortMethod, 
					layer.numColumns, layer.numRows,
					out x0, out x1, out dx,
					out y0, out y1, out dy);
				
				float z = 0.0f;
				for (int y = y0; y != y1; y += dy)
				{
					for (int x = x0; x != x1; x += dx)
					{
						var chunk = layer.GetChunk(x, y);
						bool isEmpty = layer.IsEmpty || chunk.IsEmpty;
						if (editMode) {
							isEmpty = false;
						}
						
						if (isEmpty && chunk.HasGameData)
						{
							chunk.DestroyGameData(tileMap);
						}
						else if (!isEmpty && chunk.gameObject == null)
						{
							string chunkName = "Chunk " + y.ToString() + " " + x.ToString();
							var go = chunk.gameObject = new GameObject(chunkName);
							go.transform.parent = layer.gameObject.transform;
							
							// render mesh
							MeshFilter meshFilter = go.AddComponent<MeshFilter>();
							go.AddComponent<MeshRenderer>();
							chunk.mesh = new Mesh();
							meshFilter.mesh = chunk.mesh;
							
							// collider mesh
							chunk.meshCollider = go.AddComponent<MeshCollider>();
							chunk.meshCollider.sharedMesh = null;
							chunk.colliderMesh = null;
						}
						
						if (chunk.gameObject != null)
						{
							Vector3 tilePosition = GetTilePosition(tileMap, x * tileMap.partitionSizeX, y * tileMap.partitionSizeY);
							tilePosition.z += z;
							chunk.gameObject.transform.localPosition = tilePosition;
							chunk.gameObject.transform.localRotation = Quaternion.identity;
							chunk.gameObject.transform.localScale = Vector3.one;
							chunk.gameObject.layer = unityLayer;
							
							// We won't be generating collider data in edit mode, so clear everything
							if (editMode)
							{
								if (chunk.colliderMesh)
									chunk.DestroyColliderData(tileMap);
							}
						}
						
						z -= 0.000001f;
					}
				}
				
				++layerId;
			}
		}
		
		public static void GetLoopOrder(tk2dTileMapData.SortMethod sortMethod, int w, int h, out int x0, out int x1, out int dx, out int y0, out int y1, out int dy)
		{
			switch (sortMethod)
			{
			case tk2dTileMapData.SortMethod.BottomLeft: 
				x0 = 0; x1 = w; dx = 1;
				y0 = 0; y1 = h; dy = 1;
				break;
			case tk2dTileMapData.SortMethod.BottomRight:
				x0 = w - 1; x1 = -1; dx = -1;
				y0 = 0; y1 = h; dy = 1;
				break;
			case tk2dTileMapData.SortMethod.TopLeft:
				x0 = 0; x1 = w; dx = 1;
				y0 = h - 1; y1 = -1; dy = -1;
				break;
			case tk2dTileMapData.SortMethod.TopRight:
				x0 = w - 1; x1 = -1; dx = -1;
				y0 = h - 1; y1 = -1; dy = -1;
				break;
			default:
				Debug.LogError("Unhandled sort method");
				goto case tk2dTileMapData.SortMethod.BottomLeft;
			}
		}

		const int tileMask = 0x00ffffff;

		public static int GetTileFromRawTile(int rawTile) {
			if (rawTile == -1) return -1;
			return rawTile & tileMask;
		}

		public static bool IsRawTileFlagSet(int rawTile, tk2dTileFlags flag) {
			if (rawTile == -1) return false;
			return (rawTile & (int)flag) != 0;
		}

		public static void SetRawTileFlag(ref int rawTile, tk2dTileFlags flag, bool setValue) {
			if (rawTile == -1) return;
			rawTile = setValue ? (rawTile | (int)flag) : (rawTile & (int)(~flag));
		}

		public static void InvertRawTileFlag(ref int rawTile, tk2dTileFlags flag) {
			if (rawTile == -1) return;
			bool setValue = (rawTile & (int)flag) == 0;
			rawTile = setValue ? (rawTile | (int)flag) : (rawTile & (int)(~flag));
		}

		public static Vector3 ApplySpriteVertexTileFlags(tk2dTileMap tileMap, tk2dSpriteDefinition spriteDef, Vector3 pos, bool flipH, bool flipV, bool rot90) {
			float cx = tileMap.data.tileOrigin.x + 0.5f * tileMap.data.tileSize.x;
			float cy = tileMap.data.tileOrigin.y + 0.5f * tileMap.data.tileSize.y;
			float dx = pos.x - cx;
			float dy = pos.y - cy;
			if (rot90) {
				float tmp = dx;
				dx = dy;
				dy = -tmp;
			}
			if (flipH) dx *= -1.0f;
			if (flipV) dy *= -1.0f;
			pos.x = cx + dx;
			pos.y = cy + dy;
			return pos;
		}
	}
}

#endif