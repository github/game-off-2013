using UnityEngine;
using System.Collections.Generic;

namespace tk2dRuntime.TileMap
{
	[System.Serializable]
	public class LayerInfo
	{
		public string name;
		public int hash;
		public bool useColor;
		public bool generateCollider;
		public float z = 0.1f;
		public int unityLayer = 0;
		public bool skipMeshGeneration = false;
		public PhysicMaterial physicMaterial = null;
		
		public LayerInfo()
		{
			unityLayer = 0;
			useColor = true;
			generateCollider = true;
			skipMeshGeneration = false;
		}
	}
	
	[System.Serializable]
	public class TileInfo
	{
		public string stringVal = "";
		public int intVal;
		public float floatVal;
		public bool enablePrefabOffset;
	}
}

public class tk2dTileMapData : ScriptableObject 
{
	// Start at this point
	public enum SortMethod
	{
		BottomLeft,
		TopLeft,
		BottomRight,
		TopRight,
	}
	
	// Tile type
	public enum TileType
	{
		Rectangular,
		Isometric,		// isometric tiles, offset in horizontal axis
	}
	
	public Vector3 tileSize;
	public Vector3 tileOrigin;
	
	public TileType tileType = TileType.Rectangular;

	public SortMethod sortMethod = SortMethod.BottomLeft;

	public bool layersFixedZ = false;
	
	public Object[] tilePrefabs = new Object[0];
	[SerializeField]
	tk2dRuntime.TileMap.TileInfo[] tileInfo = new tk2dRuntime.TileMap.TileInfo[0];

	[SerializeField]
	public List<tk2dRuntime.TileMap.LayerInfo> tileMapLayers = new List<tk2dRuntime.TileMap.LayerInfo>();
	
	public int NumLayers
	{
		get 
		{
			if (tileMapLayers == null || tileMapLayers.Count == 0)
				InitLayers();
			return tileMapLayers.Count;
		}
	}
	
	public tk2dRuntime.TileMap.LayerInfo[] Layers
	{
		get 
		{
			if (tileMapLayers == null || tileMapLayers.Count == 0)
				InitLayers();
			return tileMapLayers.ToArray();
		}
	}
	
	public tk2dRuntime.TileMap.TileInfo GetTileInfoForSprite(int tileId)
	{
		if (tileInfo == null || tileId < 0 || tileId >= tileInfo.Length)
			return null;
		
		return tileInfo[tileId];
	}
	
	public tk2dRuntime.TileMap.TileInfo[] GetOrCreateTileInfo(int numTiles)
	{
#if UNITY_EDITOR || !UNITY_FLASH
		bool needInit = false;
		if (tileInfo == null)
		{
			tileInfo = new tk2dRuntime.TileMap.TileInfo[numTiles];
			needInit = true;
		}
		else if (tileInfo.Length != numTiles)
		{
			System.Array.Resize(ref tileInfo, numTiles);
			needInit = true;
		}
		
		if (needInit)
		{
			for (int i = 0; i < tileInfo.Length; ++i)
			{
				if (tileInfo[i] == null)
					tileInfo[i] = new tk2dRuntime.TileMap.TileInfo();
			}
		}
#endif
		
		return tileInfo;
	}
	
	public void GetTileOffset(out float x, out float y)
	{
		switch (tileType)
		{
		case TileType.Isometric: x = 0.5f; y = 0.0f; break;
//		case TileType.HexHoritonal: x = 0.5f; y = 0.0f; break;
		
		case TileType.Rectangular: 
		default:
			x = 0.0f; y = 0.0f; break;
		}
	}
	
	void InitLayers()
	{
		tileMapLayers = new List<tk2dRuntime.TileMap.LayerInfo>();
		var newLayer = new tk2dRuntime.TileMap.LayerInfo();
		newLayer = new tk2dRuntime.TileMap.LayerInfo();
		newLayer.name = "Layer 0";
		newLayer.hash = 0x70d32b98;
		newLayer.z = 0.0f;
		tileMapLayers.Add(newLayer);
	}
}
