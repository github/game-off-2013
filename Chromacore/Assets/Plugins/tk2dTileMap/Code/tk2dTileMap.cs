using UnityEngine;
using System.Collections.Generic;

using tk2dRuntime.TileMap;

[System.Flags]
public enum tk2dTileFlags {
	None = 0x00000000,
	FlipX = 0x01000000,
	FlipY = 0x02000000,
	Rot90 = 0x04000000,
}

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/TileMap/TileMap")]
/// <summary>
/// Tile Map
/// </summary>
public class tk2dTileMap : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	/// <summary>
	/// This is a link to the editor data object (tk2dTileMapEditorData).
	/// It contains presets, and other data which isn't really relevant in game.
	/// </summary>
	public string editorDataGUID = "";
	
	/// <summary>
	/// Tile map data, stores shared parameters for tilemaps
	/// </summary>
	public tk2dTileMapData data;
	
	/// <summary>
	/// Tile map render and collider object
	/// </summary>
	public GameObject renderData;
	
	/// <summary>
	/// The sprite collection used by the tilemap
	/// </summary>
	[SerializeField]
	private tk2dSpriteCollectionData spriteCollection = null;
	public tk2dSpriteCollectionData Editor__SpriteCollection 
	{ 
		get 
		{ 
			return spriteCollection; 
		} 
		set
		{
			_spriteCollectionInst = null;
			spriteCollection = value;
			if (spriteCollection != null)
				_spriteCollectionInst = spriteCollection.inst;
		}
	}
	
	tk2dSpriteCollectionData _spriteCollectionInst = null;
	public tk2dSpriteCollectionData SpriteCollectionInst
	{
		get 
		{
			if (_spriteCollectionInst == null && spriteCollection != null)
				_spriteCollectionInst = spriteCollection.inst;
			return _spriteCollectionInst;
		}
	}
	
	[SerializeField]
	int spriteCollectionKey;
	

	/// <summary>Width of the tilemap</summary>
	public int width = 128;
	/// <summary>Height of the tilemap</summary>
	public int height = 128;
	
	/// <summary>X axis partition size for this tilemap</summary>
	public int partitionSizeX = 32;
	/// <summary>Y axis partition size for this tilemap</summary>
	public int partitionSizeY = 32;

	[SerializeField]
	Layer[] layers;
	
	[SerializeField]
	ColorChannel colorChannel;

	[SerializeField]
	GameObject prefabsRoot;

	[System.Serializable]
	public class TilemapPrefabInstance {
		public int x, y, layer;
		public GameObject instance;
	}

	[SerializeField]
	List<TilemapPrefabInstance> tilePrefabsList = new List<TilemapPrefabInstance>();
	
	[SerializeField]
	bool _inEditMode = false;
	public bool AllowEdit { get { return _inEditMode; } }
	
	// holds a path to a serialized mesh, uses this to work out dump directory for meshes
	public string serializedMeshPath;
	
	void Awake()
	{
		if (spriteCollection != null)
			_spriteCollectionInst = spriteCollection.inst;
		
		bool spriteCollectionKeyMatch = true;
		if (SpriteCollectionInst && SpriteCollectionInst.buildKey != spriteCollectionKey) spriteCollectionKeyMatch = false;

		if (Application.platform == RuntimePlatform.WindowsEditor ||
			Application.platform == RuntimePlatform.OSXEditor)
		{
			if ((Application.isPlaying && _inEditMode == true) || !spriteCollectionKeyMatch)
			{
				// Switched to edit mode while still in edit mode, rebuild
				EndEditMode();
			}
		}
		else
		{
			if (_inEditMode == true)
			{
				Debug.LogError("Tilemap " + name + " is still in edit mode. Please fix." +
					"Building overhead will be significant.");
				EndEditMode();
			}
			else if (!spriteCollectionKeyMatch)
			{
				Build(BuildFlags.ForceBuild);
			}
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (data != null) {
			Vector3 p0 = data.tileOrigin;
			Vector3 p1 = new Vector3(p0.x + data.tileSize.x * width, p0.y + data.tileSize.y * height, 0.0f);

			Gizmos.color = Color.clear;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube((p0 + p1) * 0.5f, (p1 - p0));
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}
	}
#endif
	
	[System.Flags]
	public enum BuildFlags {
		Default = 0,
		EditMode = 1,
		ForceBuild = 2
	};
	

	/// <summary>
	/// Builds the tilemap. Call this after using the SetTile functions to
	/// rebuild the affected partitions. Build only rebuilds affected partitions
	/// and is efficent enough to use at runtime if you don't use Unity colliders.
	/// Avoid building tilemaps every frame if you use Unity colliders as it will 
	/// likely be too slow for runtime use.
	/// </summary>
	public void Build() { Build(BuildFlags.Default); }

	/// <summary>
	/// Like <see cref="T:Build"/> above, but forces a build of all partitions.
	/// </summary>
	public void ForceBuild() { Build(BuildFlags.ForceBuild); }
	
	// Clears all spawned instances, but retains the renderData object
	void ClearSpawnedInstances()
	{
		if (layers == null)
			return;

		for (int layerIdx = 0; layerIdx < layers.Length; ++layerIdx)
		{
			Layer layer = layers[layerIdx];
			for (int chunkIdx = 0; chunkIdx < layer.spriteChannel.chunks.Length; ++chunkIdx)
			{
				var chunk = layer.spriteChannel.chunks[chunkIdx];

				if (chunk.gameObject == null)
					continue;
				
				var transform = chunk.gameObject.transform;
				List<Transform> children = new List<Transform>();
				for (int i = 0; i < transform.childCount; ++i)
					children.Add(transform.GetChild(i));
				for (int i = 0; i < children.Count; ++i)
					DestroyImmediate(children[i].gameObject);
			}
		}
	}

	void SetPrefabsRootActive(bool active) {
		if (prefabsRoot != null)
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
			prefabsRoot.SetActiveRecursively(active);
#else
			prefabsRoot.SetActive(active);
#endif
	}

	public void Build(BuildFlags buildFlags)
	{
		if (spriteCollection != null)
			_spriteCollectionInst = spriteCollection.inst;
		
		
#if UNITY_EDITOR || !UNITY_FLASH
		// Sanitize tilePrefabs input, to avoid branches later
		if (data != null && spriteCollection != null)
		{
			if (data.tilePrefabs == null)
				data.tilePrefabs = new Object[SpriteCollectionInst.Count];
			else if (data.tilePrefabs.Length != SpriteCollectionInst.Count)
				System.Array.Resize(ref data.tilePrefabs, SpriteCollectionInst.Count);
			
			// Fix up data if necessary
			BuilderUtil.InitDataStore(this);
		}
		else
		{
			return;
		}

		// Sanitize sprite collection material ids
		if (SpriteCollectionInst)
			SpriteCollectionInst.InitMaterialIds();
			
		bool forceBuild = (buildFlags & BuildFlags.ForceBuild) != 0;

		// When invalid, everything needs to be rebuilt
		if (SpriteCollectionInst && SpriteCollectionInst.buildKey != spriteCollectionKey)
			forceBuild = true;

		if (forceBuild)
			ClearSpawnedInstances();

		BuilderUtil.CreateRenderData(this, _inEditMode);
		
		RenderMeshBuilder.Build(this, _inEditMode, forceBuild);
		
		if (!_inEditMode)
		{
			ColliderBuilder.Build(this, forceBuild);
			BuilderUtil.SpawnPrefabs(this, forceBuild);
		}
		
		// Clear dirty flag on everything
		foreach (var layer in layers)
			layer.ClearDirtyFlag();
		if (colorChannel != null)
			colorChannel.ClearDirtyFlag();
	
		// Update sprite collection key
		if (SpriteCollectionInst)
			spriteCollectionKey = SpriteCollectionInst.buildKey;
#endif
	}
	
	/// <summary>
	/// Gets the tile coordinate at position. This can be used to obtain tile or color data explicitly from layers
	/// Returns true if the position is within the tilemap bounds
	/// </summary>
	public bool GetTileAtPosition(Vector3 position, out int x, out int y)
	{
		float ox, oy;
		bool b = GetTileFracAtPosition(position, out ox, out oy);
		x = (int)ox;
		y = (int)oy;
		return b;
	}
	
	/// <summary>
	/// Gets the tile coordinate at position. This can be used to obtain tile or color data explicitly from layers
	/// The fractional value returned is the fraction into the current tile
	/// Returns true if the position is within the tilemap bounds
	/// </summary>
	public bool GetTileFracAtPosition(Vector3 position, out float x, out float y)
	{
		switch (data.tileType)
		{
		case tk2dTileMapData.TileType.Rectangular:
		{
			Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(position);
			x = (localPosition.x - data.tileOrigin.x) / data.tileSize.x;
			y = (localPosition.y - data.tileOrigin.y) / data.tileSize.y;
			return (x >= 0 && x <= width && y >= 0 && y <= height);
		}
		case tk2dTileMapData.TileType.Isometric:
		{
			if (data.tileSize.x == 0.0f)
				break;

			float tileAngle = Mathf.Atan2(data.tileSize.y, data.tileSize.x / 2.0f);
			
			Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(position);
			x = (localPosition.x - data.tileOrigin.x) / data.tileSize.x;
			y = ((localPosition.y - data.tileOrigin.y) / (data.tileSize.y));
			
			float fy = y * 0.5f;
			int iy = (int)fy;
			
			float fry = fy - iy;
			float frx = x % 1.0f;
			
			x = (int)x;
			y = iy * 2;
			
			if (frx > 0.5f)
			{
				if (fry > 0.5f && Mathf.Atan2(1.0f - fry, (frx - 0.5f) * 2) < tileAngle)
					y += 1;
				else if (fry < 0.5f && Mathf.Atan2(fry, (frx - 0.5f) * 2) < tileAngle)
					y -= 1;
			}
			else if (frx < 0.5f)
			{
				if (fry > 0.5f && Mathf.Atan2(fry - 0.5f, frx * 2) > tileAngle)
				{
					y += 1;
					x -= 1;
				}
				
				if (fry < 0.5f && Mathf.Atan2(fry, (0.5f - frx) * 2) < tileAngle)
				{
					y -= 1;
					x -= 1;
				}
			}
			
			return (x >= 0 && x <= width && y >= 0 && y <= height);
		}
		}
		
		x = 0.0f;
		y = 0.0f;
		return false;
	}
	
	/// <summary>
	/// Returns the tile position in world space
	/// </summary>
	public Vector3 GetTilePosition(int x, int y)
	{
		switch (data.tileType)
		{
		case tk2dTileMapData.TileType.Rectangular:
		default:
			{
				Vector3 localPosition = new Vector3(
				x * data.tileSize.x + data.tileOrigin.x,
				y * data.tileSize.y + data.tileOrigin.y,
				0);
				return transform.localToWorldMatrix.MultiplyPoint(localPosition);
			}
		case tk2dTileMapData.TileType.Isometric:
			{
				Vector3 localPosition = new Vector3(
				((float)x + (((y & 1) == 0) ? 0.0f : 0.5f)) * data.tileSize.x + data.tileOrigin.x,
				y * data.tileSize.y + data.tileOrigin.y,
				0);
				return transform.localToWorldMatrix.MultiplyPoint(localPosition);
			}
		}
	}
	
	/// <summary>
	/// Gets the tile at position. This can be used to obtain tile data, etc
	/// -1 = no data or empty tile
	/// </summary>
	public int GetTileIdAtPosition(Vector3 position, int layer)
	{
		if (layer < 0 || layer >= layers.Length)
			return -1;
		
		int x, y;
		if (!GetTileAtPosition(position, out x, out y))
			return -1;
		
		return layers[layer].GetTile(x, y);
	}
	
	/// <summary>
	/// Returns the tile info chunk for the tile. Use this to store additional metadata
	/// </summary>
	public tk2dRuntime.TileMap.TileInfo GetTileInfoForTileId(int tileId)
	{
		return data.GetTileInfoForSprite(tileId);
	}
	
	/// <summary>
	/// Gets the tile at position. This can be used to obtain tile data, etc
	/// -1 = no data or empty tile
	/// </summary>
	public Color GetInterpolatedColorAtPosition(Vector3 position)
	{
		Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint(position);
		int x = (int)((localPosition.x - data.tileOrigin.x) / data.tileSize.x);
		int y = (int)((localPosition.y - data.tileOrigin.y) / data.tileSize.y);
	
		if (colorChannel == null || colorChannel.IsEmpty)
			return Color.white;
		
		if (x < 0 || x >= width ||
			y < 0 || y >= height)
		{
			return colorChannel.clearColor;
		}
		
		int offset;
		ColorChunk colorChunk = colorChannel.FindChunkAndCoordinate(x, y, out offset);
		
		if (colorChunk.Empty)
		{
			return colorChannel.clearColor;
		}
		else
		{
			int colorChunkRowOffset = partitionSizeX + 1;
			Color tileColorx0y0 = colorChunk.colors[offset];
			Color tileColorx1y0 = colorChunk.colors[offset + 1];
			Color tileColorx0y1 = colorChunk.colors[offset + colorChunkRowOffset];
			Color tileColorx1y1 = colorChunk.colors[offset + colorChunkRowOffset + 1];
			
			float wx = x * data.tileSize.x + data.tileOrigin.x;
			float wy = y * data.tileSize.y + data.tileOrigin.y;
			
			float ix = (localPosition.x - wx) / data.tileSize.x;
			float iy = (localPosition.y - wy) / data.tileSize.y;
			
			Color cy0 = Color.Lerp(tileColorx0y0, tileColorx1y0, ix);
			Color cy1 = Color.Lerp(tileColorx0y1, tileColorx1y1, ix);
			return Color.Lerp(cy0, cy1, iy);
		}
	}
	
	// ISpriteCollectionBuilder
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return spriteCollection == this.spriteCollection || _spriteCollectionInst == spriteCollection;
	}

	// We might need to end edit mode when running in game
	public void EndEditMode()
	{
		_inEditMode = false;
		SetPrefabsRootActive(true);
		Build(BuildFlags.ForceBuild);

		if (prefabsRoot != null) {
			GameObject.DestroyImmediate(prefabsRoot);
			prefabsRoot = null;
		}
	}
	
#if UNITY_EDITOR
	public void BeginEditMode()
	{
		if (layers == null) {
			_inEditMode = true;
			return;
		}

		if (!_inEditMode) {
			_inEditMode = true;

			// Destroy all children
			// Only necessary when switching INTO edit mode
			BuilderUtil.HideTileMapPrefabs(this);
			SetPrefabsRootActive(false);
		}
		
		Build(BuildFlags.ForceBuild);
	}

	public bool AreSpritesInitialized()
	{
		return layers != null;
	}
	
	public bool HasColorChannel()
	{
		return (colorChannel != null && !colorChannel.IsEmpty);
	}
	
	public void CreateColorChannel()
	{
		colorChannel = new ColorChannel(width, height, partitionSizeX, partitionSizeY);
		colorChannel.Create();
	}
	
	public void DeleteColorChannel()
	{
		colorChannel.Delete();
	}
	
	public void DeleteSprites(int layerId, int x0, int y0, int x1, int y1)
	{
		x0 = Mathf.Clamp(x0, 0, width - 1);
		y0 = Mathf.Clamp(y0, 0, height - 1);
		x1 = Mathf.Clamp(x1, 0, width - 1);
		y1 = Mathf.Clamp(y1, 0, height - 1);
		int numTilesX = x1 - x0 + 1;
		int numTilesY = y1 - y0 + 1;
		var layer = layers[layerId];
		for (int y = 0; y < numTilesY; ++y)
		{
			for (int x = 0; x < numTilesX; ++x)
			{
				layer.SetTile(x0 + x, y0 + y, -1);
			}
		}
		
		layer.OptimizeIncremental();
	}
#endif
	
	public void TouchMesh(Mesh mesh)
	{
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(mesh);
#endif
	}
	
	public void DestroyMesh(Mesh mesh)
	{
#if UNITY_EDITOR
		if (UnityEditor.AssetDatabase.GetAssetPath(mesh).Length != 0)
		{
			mesh.Clear();
			UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(mesh));
		}
		else
		{
			DestroyImmediate(mesh);
		}
#else
		DestroyImmediate(mesh);
#endif
	}

	public int GetTilePrefabsListCount() {
		return tilePrefabsList.Count;
	}

	public List<TilemapPrefabInstance> TilePrefabsList {
		get {
			return tilePrefabsList;
		}
	}

	public void GetTilePrefabsListItem(int index, out int x, out int y, out int layer, out GameObject instance) {
		TilemapPrefabInstance item = tilePrefabsList[index];
		x = item.x;
		y = item.y;
		layer = item.layer;
		instance = item.instance;
	}

	public void SetTilePrefabsList(List<int> xs, List<int> ys, List<int> layers, List<GameObject> instances) {
		int n = instances.Count;
		tilePrefabsList = new List<TilemapPrefabInstance>(n);
		for (int i = 0; i < n; ++i) {
			TilemapPrefabInstance item = new TilemapPrefabInstance();
			item.x = xs[i];
			item.y = ys[i];
			item.layer = layers[i];
			item.instance = instances[i];
			tilePrefabsList.Add(item);
		}
	}

	/// <summary>
	/// Gets or sets the layers.
	/// </summary>
	public Layer[] Layers
	{
		get { return layers; }
		set { layers = value; }
	}

	/// <summary>
	/// Gets or sets the color channel.
	/// </summary>
	public ColorChannel ColorChannel
	{
		get { return colorChannel; }
		set { colorChannel = value; }
	}

	/// <summary>
	/// Gets or sets the prefabs root.
	/// </summary>
	public GameObject PrefabsRoot
	{
		get { return prefabsRoot; }
		set { prefabsRoot = value; }
	}

	/// <summary>Gets the tile on a layer at x, y</summary> 
	/// <returns>The tile - either a sprite Id or -1 if the tile is empty.</returns>
	public int GetTile(int x, int y, int layer) {
		if (layer < 0 || layer >= layers.Length)
			return -1;
		return layers[layer].GetTile(x, y);
	}

	/// <summary>Gets the tile flags on a layer at x, y</summary> 
	/// <returns>The tile flags - a combination of tk2dTileFlags</returns>
	public tk2dTileFlags GetTileFlags(int x, int y, int layer) {
		if (layer < 0 || layer >= layers.Length)
			return tk2dTileFlags.None;
		return layers[layer].GetTileFlags(x, y);
	}

	/// <summary>Sets the tile on a layer at x, y - either a sprite Id or -1 if the tile is empty.</summary> 
	public void SetTile(int x, int y, int layer, int tile) {
		if (layer < 0 || layer >= layers.Length)
			return;
		layers[layer].SetTile(x, y, tile);
	}

	/// <summary>Sets the tile flags on a layer at x, y - a combination of tk2dTileFlags</summary> 
	public void SetTileFlags(int x, int y, int layer, tk2dTileFlags flags) {
		if (layer < 0 || layer >= layers.Length)
			return;
		layers[layer].SetTileFlags(x, y, flags);
	}

	/// <summary>Clears the tile on a layer at x, y</summary> 
	public void ClearTile(int x, int y, int layer) {
		if (layer < 0 || layer >= layers.Length)
			return;
		layers[layer].ClearTile(x, y);
	}
}
