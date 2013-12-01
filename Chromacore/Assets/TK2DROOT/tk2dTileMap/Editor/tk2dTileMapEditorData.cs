using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class tk2dSparseTile
{
	// NOTE: THESE VARIABLES NEED TO BE CONSIDERED IN THE HASH CALCULATION
	public int x, y, layer;
	public int spriteId;
	public float blendAmount;
	// NOTE: THESE VARIABLES NEED TO BE CONSIDERED IN THE HASH CALCULATION
	
	public tk2dSparseTile()
	{
		x = 0;
		y = 0;
		layer = 0;
		spriteId = -1;
		blendAmount = 0.0f;
	}

	public tk2dSparseTile(tk2dSparseTile source)
	{
		this.x = source.x;
		this.y = source.y;
		this.layer = source.layer;
		this.spriteId = source.spriteId;
		this.blendAmount = source.blendAmount;
	}
	
	public tk2dSparseTile(int x, int y, int layer, int spriteId)
	{
		this.x = x;
		this.y = y;
		this.layer = layer;
		this.spriteId = spriteId;
	}
	
	public int TileHash
	{
		get
		{
			int hash = this.GetHashCode();
			hash ^= x.GetHashCode();
			hash ^= y.GetHashCode();
			hash ^= layer.GetHashCode();
			hash ^= spriteId.GetHashCode();
			hash ^= blendAmount.GetHashCode();
			return hash;
		}
	}
};

[System.Serializable]
public class tk2dTileMapEditorBrush
{
	public enum Type
	{
		Single,
		Rectangle,
		MultiSelect,
		Custom
	}
	
	public enum PaintMode
	{
		Brush,
		Random,
		Edged,
	}
	
	public enum EdgeMode
	{
		None,
		Horizontal,
		Vertical,
		Square
	}
	
	//
	// NOTE: Make sure the hash calculation is up to date
	//
	public string name;
	public Type type = Type.Single;
	public PaintMode paintMode = PaintMode.Brush;
	public tk2dSparseTile[] tiles;
	public int[] multiSelectTiles;
	public EdgeMode edgeMode = EdgeMode.None;
	public bool multiLayer = false;
	public bool overrideWithSpriteBounds = false;
	
	public tk2dTileMapEditorBrush()
	{
		tiles = new tk2dSparseTile[] { 
			new tk2dSparseTile(0, 0, 0, 0)
		};
		UpdateBrushHash();
	}

	public tk2dTileMapEditorBrush(tk2dTileMapEditorBrush source)
	{
		this.name = source.name;
		this.type = source.type;
		this.paintMode = source.paintMode;
		
		tiles = new tk2dSparseTile[source.tiles.Length];		
		for (int i = 0; i < source.tiles.Length; ++i)
			tiles[i] = new tk2dSparseTile(source.tiles[i]);
		
		multiSelectTiles = new int[source.multiSelectTiles.Length];
		for (int i = 0; i < source.multiSelectTiles.Length; ++i)
			multiSelectTiles[i] = source.multiSelectTiles[i];
	
		edgeMode = source.edgeMode;
		multiLayer = source.multiLayer;
		overrideWithSpriteBounds = source.overrideWithSpriteBounds;
	}
	
	public bool Empty
	{
		get 
		{
			if (type == Type.MultiSelect)
				return multiSelectTiles == null || multiSelectTiles.Length == 0;
			else
				return tiles == null || tiles.Length == 0;
		}
	}
	
	public int brushHash;
	
	public int UpdateBrushHash()
	{
		brushHash = this.GetHashCode();
		brushHash ^= type.GetHashCode();
		brushHash ^= paintMode.GetHashCode();
		brushHash ^= edgeMode.GetHashCode();
		brushHash ^= multiLayer.GetHashCode();
		if (tiles != null)
		{
			foreach (var tile in tiles)
			{
				brushHash ^= tile.TileHash;
			}
		}
		if (multiSelectTiles != null)
		{
			foreach (var tile in multiSelectTiles)
			{
				brushHash ^= tile.GetHashCode();
			}
		}
		return brushHash;
	}

	public void ClipTiles(int xMin, int yMin, int xMax, int yMax) {
		int n = tiles.Length;
		for (int i = 0; i < n;) {
			if (tiles[i].x < xMin || tiles[i].y < yMin || tiles[i].x > xMax || tiles[i].y > yMax)
				tiles[i] = tiles[--n];
			else
				++i;
		}
		if (n < tiles.Length) System.Array.Resize(ref tiles, n);
	}

	public void SortTiles(bool leftToRight, bool bottomToTop) {
		int n = tiles.Length;
		for (int i = 0; i < n; ++i) {
			for (int j = i + 1; j < n; ++j) {
				bool swap = false;
				if (tiles[i].y != tiles[j].y) {
					swap = ((tiles[i].y < tiles[j].y) != bottomToTop);
				} else {
					swap = ((tiles[i].x < tiles[j].x) != leftToRight);
				}
				if (swap) {
					tk2dSparseTile tmp = tiles[i];
					tiles[i] = tiles[j];
					tiles[j] = tmp;
				}
			}
		}
	}
}

public class tk2dTileMapEditorData : ScriptableObject 
{
	public enum EditMode
	{
		/// <summary>Paint mode</summary>
		Paint,
		/// <summary>Color paint mode</summary>
		Color,
		/// <summary>Tile data setup</summary>
		Data,
		/// <summary>Settings</summary>
		Settings,
	}
	public EditMode editMode = EditMode.Settings;
	
	
	public enum BlendMode
	{
		Blend,
		Add,
	}
	
	public enum SetupMode
	{
		None = 0,
		Dimensions = 1,
		TileProperties = 2,
		PaletteProperties = 4,
		AdvancedProperties = 8,
		Info = 16,
		Layers = 32,
		Import = 64,
	}
	
	public Color brushColor = Color.white;
	public float brushRadius = 1.0f;
	public float blendStrength = 1.0f;
	public BlendMode blendMode = BlendMode.Blend;
	public float brushDisplayScale = 1.0f;
	
	public tk2dTileMapEditorBrush defaultBrush;
	public tk2dTileMapEditorBrush activeBrush;
	public List<tk2dTileMapEditorBrush> brushes = new List<tk2dTileMapEditorBrush>();

	public tk2dTileMapEditorBrush paletteBrush;
	public int paletteTilesPerRow = 8;
	
	public SetupMode setupMode = 0;
	
	public bool showBrush = true;
	public bool showPalette = true;
	
	public int layer = 0;

	public List<tk2dTileMapScratchpad> scratchpads = new List<tk2dTileMapScratchpad>();
	
	public void InitBrushes(tk2dSpriteCollectionData spriteCollection)
	{
		if (defaultBrush == null || defaultBrush.Empty)
		{
			defaultBrush = new tk2dTileMapEditorBrush();
		}
		
		if (brushes == null)
		{
			brushes = new List<tk2dTileMapEditorBrush>();
		}
		
		if (paletteBrush == null || paletteBrush.Empty || !paletteBrush.overrideWithSpriteBounds)
		{
			paletteBrush = new tk2dTileMapEditorBrush();
			paletteBrush.overrideWithSpriteBounds = true;
			CreateDefaultPalette(spriteCollection, paletteBrush, paletteTilesPerRow);
		}
		
		int spriteCount = spriteCollection.spriteDefinitions.Length;
		foreach (var brush in brushes)
		{
			for (int j = 0; j < brush.tiles.Length; ++j)
			{
				brush.tiles[j].spriteId = (ushort)Mathf.Clamp(brush.tiles[j].spriteId, 0, spriteCount - 1);
			}
		}
		
		if (activeBrush == null || activeBrush.Empty)
		{
			activeBrush = defaultBrush;
		}
	}
	
	public void CreateDefaultPalette(tk2dSpriteCollectionData spriteCollection, tk2dTileMapEditorBrush brush, int numTilesX)
	{
		List<tk2dSparseTile> tiles = new List<tk2dSparseTile>();
		
		var spriteDefinitions = spriteCollection.spriteDefinitions;
		int numTilesY = spriteDefinitions.Length / numTilesX;
		if (numTilesY * numTilesX < spriteDefinitions.Length)
			numTilesY++;
		
		for (ushort spriteIndex = 0; spriteIndex < spriteDefinitions.Length; ++spriteIndex)
		{
			if (spriteDefinitions[spriteIndex].Valid)
				tiles.Add(new tk2dSparseTile(spriteIndex % numTilesX, numTilesY - 1 - spriteIndex / numTilesX, 0, spriteIndex));
		}
		brush.tiles = tiles.ToArray();
		brush.UpdateBrushHash();
	}
}
