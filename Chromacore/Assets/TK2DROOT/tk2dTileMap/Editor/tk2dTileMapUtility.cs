using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using tk2dRuntime.TileMap;

namespace tk2dEditor.TileMap
{
	public static class TileMapUtility
	{
		public static int MaxWidth = 1024;
		public static int MaxHeight = 1024;
		public static int MaxLayers = 32;
		
		public static void ResizeTileMap(tk2dTileMap tileMap, int width, int height, int partitionSizeX, int partitionSizeY)
		{
			int w = Mathf.Clamp(width, 1, MaxWidth);
			int h = Mathf.Clamp(height, 1, MaxHeight);

			Undo.RegisterSceneUndo("Resize tile map");

			// Since this only works in edit mode, prefabs can be assumed to be saved here

			// Delete old layer render data
			foreach (Layer layer in tileMap.Layers) {
				if (layer.gameObject != null) {
					GameObject.DestroyImmediate(layer.gameObject);
				}				
			}

			// copy into new tilemap
			Layer[] layers = new Layer[tileMap.Layers.Length];
			for (int layerId = 0; layerId < tileMap.Layers.Length; ++layerId)
			{
				Layer srcLayer = tileMap.Layers[layerId];
				layers[layerId] = new Layer(srcLayer.hash, width, height, partitionSizeX, partitionSizeY);
				Layer destLayer = layers[layerId];
				
				if (srcLayer.IsEmpty)
					continue;
				
				int hcopy = Mathf.Min(tileMap.height, h);
				int wcopy = Mathf.Min(tileMap.width, w);
				
				for (int y = 0; y < hcopy; ++y)
				{
					for (int x = 0; x < wcopy; ++x)
					{
						destLayer.SetRawTile(x, y, srcLayer.GetRawTile(x, y));
					}
				}
				
				destLayer.Optimize();
			}
			
			// copy new colors
			bool copyColors = (tileMap.ColorChannel != null && !tileMap.ColorChannel.IsEmpty);
			ColorChannel targetColors = new ColorChannel(width, height, partitionSizeX, partitionSizeY);
			if (copyColors)
			{
				int hcopy = Mathf.Min(tileMap.height, h) + 1;
				int wcopy = Mathf.Min(tileMap.width, w) + 1;
				for (int y = 0; y < hcopy; ++y)
				{
					for (int x = 0; x < wcopy; ++x)
					{
						targetColors.SetColor(x, y, tileMap.ColorChannel.GetColor(x, y));
					}
				}
				
				targetColors.Optimize();
			}
		
			tileMap.ColorChannel = targetColors;
			tileMap.Layers = layers;
			tileMap.width = w;
			tileMap.height = h;
			tileMap.partitionSizeX = partitionSizeX;
			tileMap.partitionSizeY = partitionSizeY;
			
			tileMap.ForceBuild();
		}
		
		// Returns index of newly added layer
		public static int AddNewLayer(tk2dTileMap tileMap)
		{
			var existingLayers = tileMap.data.Layers;
			// find a unique hash
			bool duplicateHash = false;
			int hash;
			do
			{
				duplicateHash = false;
				hash = Random.Range(0, int.MaxValue);
				foreach (var layer in existingLayers) 
					if (layer.hash == hash) 
						duplicateHash = true;
			} while (duplicateHash == true);

			List<Object> objectsToUndo = new List<Object>();
			objectsToUndo.Add(tileMap);
			objectsToUndo.Add(tileMap.data);
			Undo.RegisterUndo(objectsToUndo.ToArray(), "Add layer");
			
			var newLayer = new tk2dRuntime.TileMap.LayerInfo();
			newLayer.name = "New Layer";
			newLayer.hash = hash;
			newLayer.z = 0.1f;
			tileMap.data.tileMapLayers.Add(newLayer);
			
			// remap tilemap
			tk2dRuntime.TileMap.BuilderUtil.InitDataStore(tileMap);

			GameObject layerGameObject = new GameObject(newLayer.name);
			layerGameObject.transform.parent = tileMap.renderData.transform;
			layerGameObject.transform.localPosition = Vector3.zero;
			layerGameObject.transform.localScale = Vector3.one;
			layerGameObject.transform.localRotation = Quaternion.identity;
			tileMap.Layers[tileMap.Layers.Length - 1].gameObject = layerGameObject;

			Undo.RegisterCreatedObjectUndo(layerGameObject, "Add layer");
			
			return tileMap.data.NumLayers - 1;
		}
		
		public static int FindOrCreateLayer(tk2dTileMap tileMap, string name)
		{
			int index = 0;
			foreach (var v in tileMap.data.Layers)
			{
				if (v.name == name)
					return index;
				++index;
			}
			index = AddNewLayer(tileMap);
			tileMap.data.Layers[index].name = name;
			return index;
		}
		
		public static void DeleteLayer(tk2dTileMap tileMap, int layerToDelete)
		{
			// Just in case
			if (tileMap.data.NumLayers <= 1)
				return;
	
			// Find all objects that will be affected by this operation			
			List<Object> objectsToUndo = new List<Object>();
			objectsToUndo.Add(tileMap);
			objectsToUndo.Add(tileMap.data);
			objectsToUndo.AddRange(CollectDeepHierarchy(tileMap.Layers[layerToDelete].gameObject));
			Undo.RegisterUndo(objectsToUndo.ToArray(), "Delete layer");

			tileMap.data.tileMapLayers.RemoveAt(layerToDelete);
			if (tileMap.Layers[layerToDelete].gameObject != null) {
				GameObject.DestroyImmediate( tileMap.Layers[layerToDelete].gameObject );
			}
			tk2dRuntime.TileMap.BuilderUtil.InitDataStore(tileMap);
			tileMap.ForceBuild();
		}

		static Object[] CollectDeepHierarchy( GameObject go ) {
			if (go == null) {
				return new Object[0];
			}
			else {
				return EditorUtility.CollectDeepHierarchy( new Object[] { go } );
			}
		}
		
		public static void MoveLayer(tk2dTileMap tileMap, int layer, int direction)
		{
			List<Object> objectsToUndo = new List<Object>();
			objectsToUndo.Add(tileMap);
			objectsToUndo.Add(tileMap.data);
			objectsToUndo.AddRange(CollectDeepHierarchy(tileMap.Layers[layer].gameObject));
			objectsToUndo.AddRange(CollectDeepHierarchy(tileMap.Layers[layer + direction].gameObject));
			Undo.RegisterUndo(objectsToUndo.ToArray(), "Move layer");

			// Move all prefabs to new layer
			int targetLayer = layer + direction;
			foreach (tk2dTileMap.TilemapPrefabInstance v in tileMap.TilePrefabsList) {
				if (v.layer == layer) {
					v.layer = targetLayer;
				}
				else if (v.layer == targetLayer) {
					v.layer = layer;
				}
			}

			LayerInfo tmp = tileMap.data.tileMapLayers[layer];
			tileMap.data.tileMapLayers[layer] = tileMap.data.tileMapLayers[targetLayer];
			tileMap.data.tileMapLayers[targetLayer] = tmp;
			tk2dRuntime.TileMap.BuilderUtil.InitDataStore(tileMap);
			tileMap.ForceBuild();
		}

		/// Deletes all generated instances
		public static void MakeUnique(tk2dTileMap tileMap)
		{
			if (tileMap.renderData == null)
				return;
			
			List<Object> objectsToUndo = new List<Object>();
			objectsToUndo.Add(tileMap);
			objectsToUndo.Add(tileMap.data);
			objectsToUndo.AddRange(CollectDeepHierarchy(tileMap.renderData));
			objectsToUndo.AddRange(CollectDeepHierarchy(tileMap.PrefabsRoot));
			Undo.RegisterUndo(objectsToUndo.ToArray(), "Make Unique");

			if (tileMap.renderData != null) {
				GameObject.DestroyImmediate(tileMap.renderData);
				tileMap.renderData = null;
			}
			if (tileMap.PrefabsRoot != null) {
				GameObject.DestroyImmediate(tileMap.PrefabsRoot);
				tileMap.PrefabsRoot = null;
			}

			tileMap.ForceBuild();
		}
	}
}
