using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor
{
	
	class BrushDictData
	{
		public tk2dTileMapEditorBrush brush;
		public int brushHash;
		public Mesh mesh;
		public Material[] materials;
		public Rect rect;
	}
	
	public class BrushRenderer
	{
		tk2dTileMap tileMap;
		tk2dSpriteCollectionData spriteCollection;
		Dictionary<tk2dTileMapEditorBrush, BrushDictData> brushLookupDict = new  Dictionary<tk2dTileMapEditorBrush, BrushDictData>();
		
		public BrushRenderer(tk2dTileMap tileMap)
		{
			this.tileMap = tileMap;
			this.spriteCollection = tileMap.SpriteCollectionInst;
		}
		
		public void Destroy()
		{
			foreach (var v in brushLookupDict.Values)
			{
				Mesh.DestroyImmediate(v.mesh);
				v.mesh = null;
			}
		}
		
		// Build a mesh for a list of given sprites
		void BuildMeshForBrush(tk2dTileMapEditorBrush brush, BrushDictData dictData, int tilesPerRow)
		{
			List<Vector3> vertices = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			Dictionary<Material, List<int>> triangles = new Dictionary<Material, List<int>>();
			
			// bounds of tile
			Vector3 spriteBounds = Vector3.zero;
			foreach (var spriteDef in spriteCollection.spriteDefinitions) {
				if (spriteDef.Valid)
					spriteBounds = Vector3.Max(spriteBounds, spriteDef.untrimmedBoundsData[1]);
			}
			Vector3 tileSize = brush.overrideWithSpriteBounds?
								spriteBounds:
								tileMap.data.tileSize;
			float layerOffset = 0.001f;
			
			Vector3 boundsMin = new Vector3(1.0e32f, 1.0e32f, 1.0e32f);
			Vector3 boundsMax = new Vector3(-1.0e32f, -1.0e32f, -1.0e32f);
		
			float tileOffsetX = 0, tileOffsetY = 0;
			if (!brush.overrideWithSpriteBounds)
				tileMap.data.GetTileOffset(out tileOffsetX, out tileOffsetY);
			
			if (brush.type == tk2dTileMapEditorBrush.Type.MultiSelect)
			{
				int tileX = 0;
				int tileY = brush.multiSelectTiles.Length / tilesPerRow;
				if ((brush.multiSelectTiles.Length % tilesPerRow) == 0) tileY -=1;
				foreach (var uncheckedSpriteId in brush.multiSelectTiles)
				{
					float xOffset = (tileY & 1) * tileOffsetX;
				
					// The origin of the tile in mesh space
					Vector3 tileOrigin = new Vector3((tileX + xOffset) * tileSize.x, tileY * tileSize.y, 0.0f);
					//if (brush.overrideWithSpriteBounds)
					{
						boundsMin = Vector3.Min(boundsMin, tileOrigin);
						boundsMax = Vector3.Max(boundsMax, tileOrigin + tileSize);
					}

					if (uncheckedSpriteId != -1)
					{
						int indexRoot = vertices.Count;
						int spriteId = Mathf.Clamp(uncheckedSpriteId, 0, spriteCollection.Count - 1);
						tk2dSpriteDefinition sprite = spriteCollection.spriteDefinitions[spriteId];

						for (int j = 0; j < sprite.positions.Length; ++j)
						{
							// Offset so origin is at bottom left
							Vector3 v = sprite.positions[j] - tileMap.data.tileOrigin;
							
							boundsMin = Vector3.Min(boundsMin, tileOrigin + v);
							boundsMax = Vector3.Max(boundsMax, tileOrigin + v);
							
							vertices.Add(tileOrigin + v);
							uvs.Add(sprite.uvs[j]);
						}
						
						if (!triangles.ContainsKey(sprite.material))
							triangles.Add(sprite.material, new List<int>());

						for (int j = 0; j < sprite.indices.Length; ++j)
						{
							triangles[sprite.material].Add(indexRoot + sprite.indices[j]);
						}
					}
					
					tileX += 1;
					if (tileX == tilesPerRow)
					{
						tileX = 0;
						tileY -= 1;
					}
				}				
			}
			else
			{
				// the brush is centered around origin, x to the right, y up
				foreach (var tile in brush.tiles)
				{
					float xOffset = (tile.y & 1) * tileOffsetX;
					
					// The origin of the tile in mesh space
					Vector3 tileOrigin = new Vector3((tile.x + xOffset) * tileSize.x, tile.y * tileSize.y, tile.layer * layerOffset);
					
					//if (brush.overrideWithSpriteBounds)
					{
						boundsMin = Vector3.Min(boundsMin, tileOrigin);
						boundsMax = Vector3.Max(boundsMax, tileOrigin + tileSize);
					}

					int spriteIdx = tk2dRuntime.TileMap.BuilderUtil.GetTileFromRawTile(tile.spriteId);
					bool flipH = tk2dRuntime.TileMap.BuilderUtil.IsRawTileFlagSet(tile.spriteId, tk2dTileFlags.FlipX);
					bool flipV = tk2dRuntime.TileMap.BuilderUtil.IsRawTileFlagSet(tile.spriteId, tk2dTileFlags.FlipY);
					bool rot90 = tk2dRuntime.TileMap.BuilderUtil.IsRawTileFlagSet(tile.spriteId, tk2dTileFlags.Rot90);

					if (spriteIdx < 0 || spriteIdx >= spriteCollection.Count)
						continue;
					
					int indexRoot = vertices.Count;
					var sprite = spriteCollection.spriteDefinitions[spriteIdx];

					if (brush.overrideWithSpriteBounds) {
						tileOrigin.x += spriteBounds.x * 0.5f - sprite.untrimmedBoundsData[0].x;
						tileOrigin.y += spriteBounds.y * 0.5f - sprite.untrimmedBoundsData[0].y;
					}
		
					for (int j = 0; j < sprite.positions.Length; ++j)
					{
						Vector3 flippedPos = tk2dRuntime.TileMap.BuilderUtil.ApplySpriteVertexTileFlags(tileMap, sprite, sprite.positions[j], flipH, flipV, rot90);
						
						// Offset so origin is at bottom left (if not using bounds)
						Vector3 v = flippedPos;
						if (!brush.overrideWithSpriteBounds)
							v -= tileMap.data.tileOrigin;

						boundsMin = Vector3.Min(boundsMin, tileOrigin + v);
						boundsMax = Vector3.Max(boundsMax, tileOrigin + v);
						
						vertices.Add(tileOrigin + v);
						uvs.Add(sprite.uvs[j]);
					}
					
					if (!triangles.ContainsKey(sprite.material))
						triangles.Add(sprite.material, new List<int>());

					for (int j = 0; j < sprite.indices.Length; ++j)
					{
						triangles[sprite.material].Add(indexRoot + sprite.indices[j]);
					}
				}
			}
			
			if (dictData.mesh == null)
			{
				dictData.mesh = new Mesh();
				dictData.mesh.hideFlags = HideFlags.DontSave;
			}
			
			Mesh mesh = dictData.mesh;
			mesh.Clear();
			mesh.vertices = vertices.ToArray(); 
			Color[] colors = new Color[vertices.Count];
			for (int i = 0; i < vertices.Count; ++i)
				colors[i] = Color.white;
			mesh.colors = colors;
			mesh.uv = uvs.ToArray();
			mesh.subMeshCount = triangles.Keys.Count;

			int subMeshId = 0;
			foreach (Material mtl in triangles.Keys)
			{
				mesh.SetTriangles(triangles[mtl].ToArray(), subMeshId);
				subMeshId++;
			}
			
			dictData.brush = brush;
			dictData.brushHash = brush.brushHash;
			dictData.mesh = mesh;
			dictData.materials = (new List<Material>(triangles.Keys)).ToArray();
			dictData.rect = new Rect(boundsMin.x, boundsMin.y, boundsMax.x - boundsMin.x, boundsMax.y - boundsMin.y);
		}
		
		BrushDictData GetDictDataForBrush(tk2dTileMapEditorBrush brush, int tilesPerRow)
		{
			BrushDictData dictEntry;
			if (brushLookupDict.TryGetValue(brush, out dictEntry))
			{
				if (brush.brushHash != dictEntry.brushHash)
				{
					BuildMeshForBrush(brush, dictEntry, tilesPerRow);
				}
				return dictEntry;
			}
			else
			{
				dictEntry = new BrushDictData();
				BuildMeshForBrush(brush, dictEntry, tilesPerRow);
				brushLookupDict[brush] = dictEntry;
				return dictEntry;
			}
		}
		
		float lastScale;
		public float LastScale { get { return lastScale; } }

		public Rect GetBrushViewRect(tk2dTileMapEditorBrush brush, int tilesPerRow) {
			var dictData = GetDictDataForBrush(brush, tilesPerRow);
			return BrushToScreenRect(dictData.rect);
		}
		
		public Rect DrawBrush(tk2dTileMap tileMap, tk2dTileMapEditorBrush brush, float scale, bool forceUnitSpacing, int tilesPerRow)
		{
			var dictData = GetDictDataForBrush(brush, tilesPerRow);
			Mesh atlasViewMesh = dictData.mesh;
			Rect atlasViewRect = BrushToScreenRect(dictData.rect);

			Rect visibleRect = tk2dSpriteThumbnailCache.VisibleRect;
			Vector4 clipRegion = new Vector4(visibleRect.x, visibleRect.y, visibleRect.x + visibleRect.width, visibleRect.y + visibleRect.height);

			Material customMaterial = tk2dSpriteThumbnailCache.GetMaterial();
			customMaterial.SetColor("_Tint", Color.white);
			customMaterial.SetVector("_Clip", clipRegion);
			
			float width = atlasViewRect.width * scale;
			float height = atlasViewRect.height * scale;
			
			Rect rect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
			scale = width / atlasViewRect.width;
			lastScale = scale;
			
			if (Event.current.type == EventType.Repaint)
			{
				Matrix4x4 mat = new Matrix4x4();
				var spriteDef = tileMap.SpriteCollectionInst.spriteDefinitions[0];
				mat.SetTRS(new Vector3(rect.x, 
									   rect.y + height, 0), Quaternion.identity, new Vector3(scale / spriteDef.texelSize.x, -scale / spriteDef.texelSize.y, 1));
					
				for (int i = 0; i < dictData.materials.Length; ++i)
				{
					customMaterial.mainTexture = dictData.materials[i].mainTexture;
					customMaterial.SetPass(0);
					Graphics.DrawMeshNow(atlasViewMesh, mat * GUI.matrix, i);
				}
			}
			
			return rect;
		}
		
		public Vector3 TexelSize
		{
			get
			{
				return spriteCollection.spriteDefinitions[0].texelSize;
			}
		}
		
		public Rect BrushToScreenRect(Rect rect)
		{
			Vector3 texelSize = TexelSize;
				
			int w = (int)(rect.width / texelSize.x);
			int h = (int)(rect.height / texelSize.y);
			
			return new Rect(0, 0, w, h);
		}
		
		public Rect TileSizePixels
		{
			get 
			{
				Vector3 texelSize = TexelSize;
				Vector3 tileSize = Vector3.zero;
				foreach (var spriteDef in spriteCollection.spriteDefinitions) {
					if (spriteDef.Valid)
						tileSize = Vector3.Max(tileSize, spriteDef.untrimmedBoundsData[1]);
				}
				return new Rect(0, 0, tileSize.x / texelSize.x, tileSize.y / texelSize.y);
			}
		}

		public void DrawBrushInScene(Matrix4x4 matrix, tk2dTileMapEditorBrush brush, int tilesPerRow) {
			var dictData = GetDictDataForBrush(brush, tilesPerRow);
			Mesh mesh = dictData.mesh;

			Vector4 clipRegion = new Vector4(-1.0e32f, -1.0e32f, 1.0e32f, 1.0e32f);
			Material customMaterial = tk2dSpriteThumbnailCache.GetMaterial();
			customMaterial.SetColor("_Tint", new Color(1,1,1,tk2dTileMapToolbar.workBrushOpacity));
			customMaterial.SetVector("_Clip", clipRegion);

			for (int i = 0; i < dictData.materials.Length; ++i)
			{
				customMaterial.mainTexture = dictData.materials[i].mainTexture;
				customMaterial.SetPass(0);
				Graphics.DrawMeshNow( mesh, matrix, i );
			}
		}

		public void DrawBrushInScratchpad(tk2dTileMapEditorBrush brush, Matrix4x4 matrix, bool setWorkbrushOpacity) {
			var dictData = GetDictDataForBrush(brush, 1000000);
			Mesh mesh = dictData.mesh;

			Rect visibleRect = tk2dSpriteThumbnailCache.VisibleRect;
			Vector4 clipRegion = new Vector4(visibleRect.x, visibleRect.y, visibleRect.x + visibleRect.width, visibleRect.y + visibleRect.height);

			Material customMaterial = tk2dSpriteThumbnailCache.GetMaterial();
			customMaterial.SetColor("_Tint", new Color(1,1,1,setWorkbrushOpacity?tk2dTileMapToolbar.workBrushOpacity:1));
			customMaterial.SetVector("_Clip", clipRegion);

			for (int i = 0; i < dictData.materials.Length; ++i) {
				customMaterial.mainTexture = dictData.materials[i].mainTexture;
				customMaterial.SetPass(0);
				Graphics.DrawMeshNow(mesh, matrix, i);
			}
		}
	}

}
