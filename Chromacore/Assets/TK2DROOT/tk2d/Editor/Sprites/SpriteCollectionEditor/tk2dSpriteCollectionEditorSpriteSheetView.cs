using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteCollectionEditor
{
	public class SpriteSheetView
	{
		IEditorHost host;
		SpriteCollectionProxy SpriteCollection { get { return host.SpriteCollection; } }
		
		public SpriteSheetView(IEditorHost host)
		{
			this.host = host;
		}
		
		int FindSpriteSlotForSpriteSheetCell(int spriteSheetId, int x, int y)
		{
			for (int id = 0; id < SpriteCollection.textureParams.Count; ++id)
			{
				var v = SpriteCollection.textureParams[id];
				if (v.hasSpriteSheetId 
					&& v.spriteSheetId == spriteSheetId
					&& v.spriteSheetX == x
					&& v.spriteSheetY == y)
				{
					return id;
				}
			}
			return -1;
		}
		
		int GetSpriteSlotForSpriteSheetCell(int spriteSheetId, int x, int y)
		{
			int foundId = FindSpriteSlotForSpriteSheetCell(spriteSheetId, x, y);
			if (foundId != -1) return foundId;
			// create a new sprite 
			return SpriteCollection.FindOrCreateEmptySpriteSlot();
		}
		
		int GetSpriteCoordinateHash(int x, int y)
		{
			return (y << 16) + x;
		}
		
		void AddSprites(tk2dSpriteSheetSource spriteSheet)
		{
			int spriteSheetId = SpriteCollection.GetSpriteSheetId(spriteSheet);
			List<int> usedSpriteCoordinates = new List<int>();
			
			var tex = spriteSheet.texture;
			int regionId = 0;

			int numTilesX, numTilesY;
			GetNumTilesForSpriteSheet(spriteSheet, out numTilesX, out numTilesY);
			for (int idY = 0; idY < numTilesY; ++idY)
			{
				for (int idX = 0; idX < numTilesX; ++idX)
				{
					int x, y;
					GetTileCoordinateForSpriteSheet(spriteSheet, idX, idY, out x, out y);
					
					int spriteSlot = GetSpriteSlotForSpriteSheetCell(spriteSheetId, idX, idY);
					var param = SpriteCollection.textureParams[spriteSlot];
					param.texture = spriteSheet.texture;
					param.hasSpriteSheetId = true;
					param.spriteSheetId = spriteSheetId;
					param.spriteSheetX = idX;
					param.spriteSheetY = idY;
					param.extractRegion = true;
					param.regionId = regionId;
					param.regionX = x;
					param.regionY = tex.height - spriteSheet.tileHeight - y;
					param.regionW = spriteSheet.tileWidth;
					param.regionH = spriteSheet.tileHeight;
					param.pad = spriteSheet.pad;
					int id = idY * numTilesX + idX;
					param.name = tex.name + "/" + id.ToString();
					usedSpriteCoordinates.Add(GetSpriteCoordinateHash(idX, idY));
					
					regionId++;
				}
			}
			
			// Delete sprites from sprite sheet which aren't required any more
			for (int i = 0; i < SpriteCollection.textureParams.Count; ++i)
			{
				var p = SpriteCollection.textureParams[i];
				if (p.hasSpriteSheetId && p.spriteSheetId == spriteSheetId)
				{
					int coordinateHash = GetSpriteCoordinateHash(p.spriteSheetX, p.spriteSheetY);
					if (usedSpriteCoordinates.IndexOf(coordinateHash) == -1)
					{
						SpriteCollection.textureParams[i].Clear();
					}
				}
			}
			
			host.OnSpriteCollectionChanged(true);
		}
		
		void GetNumTilesForSpriteSheet(tk2dSpriteSheetSource spriteSheet, out int numTilesX, out int numTilesY)
		{
			var tex = spriteSheet.texture;
			numTilesX = (tex.width - spriteSheet.tileMarginX + spriteSheet.tileSpacingX) / (spriteSheet.tileSpacingX + spriteSheet.tileWidth);
			numTilesY = (tex.height - spriteSheet.tileMarginY + spriteSheet.tileSpacingY) / (spriteSheet.tileSpacingY + spriteSheet.tileHeight);
		}
		
		void GetTileCoordinateForSpriteSheet(tk2dSpriteSheetSource spriteSheet, int tileX, int tileY, out int coordX, out int coordY)
		{
			coordX = spriteSheet.tileMarginX + (spriteSheet.tileSpacingX + spriteSheet.tileWidth) * tileX;
			coordY = spriteSheet.tileMarginY + (spriteSheet.tileSpacingY + spriteSheet.tileHeight) * tileY;
		}
		
		void DrawGridOverlay(tk2dSpriteSheetSource spriteSheet, Rect rect)
		{
			if (spriteSheet.tileWidth > 0 && spriteSheet.tileHeight > 0)
			{
				var tex = spriteSheet.texture;
				
				Color oldColor = Handles.color;
				Handles.color = Color.red;
				int numTilesX, numTilesY;
				GetNumTilesForSpriteSheet(spriteSheet, out numTilesX, out numTilesY);
				for (int tileY = 0; tileY < numTilesY; ++tileY)
				{
					int x, y;
					GetTileCoordinateForSpriteSheet(spriteSheet, 0, tileY, out x, out y);
					Handles.DrawLine(new Vector3(rect.x, rect.y + y * zoomAmount, 0),
									 new Vector3(rect.x + tex.width * zoomAmount, rect.y + y * zoomAmount, 0));
					Handles.DrawLine(new Vector3(rect.x, rect.y + (y + spriteSheet.tileHeight) * zoomAmount, 0),
									 new Vector3(rect.x + tex.width * zoomAmount, rect.y + (y + spriteSheet.tileHeight) * zoomAmount, 0));
				}
				for (int tileX = 0; tileX < numTilesX; ++tileX)
				{
					int x, y;
					GetTileCoordinateForSpriteSheet(spriteSheet, tileX, 0, out x, out y);
					Handles.DrawLine(new Vector3(rect.x + x * zoomAmount, rect.y, 0),
									 new Vector3(rect.x + x * zoomAmount, rect.y + tex.height * zoomAmount, 0));
					Handles.DrawLine(new Vector3(rect.x + (x + spriteSheet.tileWidth) * zoomAmount, rect.y, 0),
									 new Vector3(rect.x + (x + spriteSheet.tileWidth) * zoomAmount, rect.y + tex.height * zoomAmount, 0));
				}
				
				// Highlight ONE tile
				{
					int x, y;
					GetTileCoordinateForSpriteSheet(spriteSheet, 0, 0, out x, out y);
					Vector3 v0 = new Vector3(rect.x + x * zoomAmount, rect.x + y * zoomAmount, 0);
					Vector3 v1 = v0 + new Vector3(spriteSheet.tileWidth * zoomAmount, spriteSheet.tileHeight * zoomAmount, 0);
					Vector3[] rectVerts = { new Vector3(v0.x, v0.y, 0), new Vector3(v1.x, v0.y, 0), new Vector3(v1.x, v1.y, 0), new Vector3(v0.x, v1.y, 0) };
					Handles.DrawSolidRectangleWithOutline(rectVerts, new Color(1,1,1,0.2f), new Color(1,1,1,1));
				}
				Handles.color = oldColor;
			}			
		}
		
		void ProcessSpriteSelectionUI(tk2dSpriteSheetSource spriteSheet, Rect rect)
		{
			int spriteSheetId = SpriteCollection.GetSpriteSheetId(spriteSheet);
			if (rect.Contains(Event.current.mousePosition))
			{
				Vector2 localMousePos = (Event.current.mousePosition - new Vector2(rect.x, rect.y)) / zoomAmount;
				int tileX = ((int)localMousePos.x - spriteSheet.tileMarginX) / (spriteSheet.tileSpacingX + spriteSheet.tileWidth);
				int tileY = ((int)localMousePos.y - spriteSheet.tileMarginY) / (spriteSheet.tileSpacingY + spriteSheet.tileHeight);
				int numTilesX, numTilesY;
				GetNumTilesForSpriteSheet(spriteSheet, out numTilesX, out numTilesY);
				
				if (Event.current.type == EventType.MouseDown)
				{
					bool multiSelectKey = (Application.platform == RuntimePlatform.OSXEditor)?Event.current.command:Event.current.control;
					if (tileX >= 0 && tileX < numTilesX && tileY >= 0 && tileY < numTilesY)
					{
						if (!multiSelectKey)
							selectedSprites.Clear();
						
						int id = FindSpriteSlotForSpriteSheetCell(SpriteCollection.GetSpriteSheetId(spriteSheet), tileX, tileY);
						if (id != -1)
						{
							if (!multiSelectKey)
							{
								rectSelectX = tileX;
								rectSelectY = tileY;
							}
							
							bool found = false;
							foreach (var sel in selectedSprites)
							{
								if (sel.index == id)
								{
									found = true;
									selectedSprites.Remove(sel);
									break;
								}
							}
							if (!found)
							{
								var entry = new SpriteCollectionEditorEntry();
								entry.index = id;
								selectedSprites.Add(entry);
							}
							HandleUtility.Repaint();
						}
					}
				}
				else if (Event.current.type == EventType.MouseDrag)
				{
					if (rectSelectX != -1 && rectSelectY != -1)
					{
						int x0 = Mathf.Min(tileX, rectSelectX);
						int x1 = Mathf.Max(tileX, rectSelectX);
						int y0 = Mathf.Min(tileY, rectSelectY);
						int y1 = Mathf.Max(tileY, rectSelectY);
						selectedSprites.Clear();
						for (int y = y0; y <= y1; ++y)
						{
							for (int x = x0; x <= x1; ++x)
							{
								int id = FindSpriteSlotForSpriteSheetCell(spriteSheetId, x, y);
								if (id != -1)
								{
									var entry = new SpriteCollectionEditorEntry();
									entry.index = id;
									selectedSprites.Add(entry);
								}
							}
						}
						HandleUtility.Repaint();
					}
				}
				else if (Event.current.type == EventType.MouseUp)
				{
					rectSelectX = -1;
					rectSelectY = -1;
				}
			}			
		}
		
		float zoomAmount = 1.0f;
		void DrawTextureView(tk2dSpriteSheetSource spriteSheet)
		{
			int spriteSheetId = SpriteCollection.GetSpriteSheetId(spriteSheet);
			var tex = spriteSheet.texture;
			
			int border = 16;
			float width = tex.width * zoomAmount;
			float height = tex.height * zoomAmount;
			Rect baseRect = GUILayoutUtility.GetRect(border * 2 + width, border * 2 + height, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			tk2dGrid.Draw(baseRect);
			Rect rect = new Rect(baseRect.x + border, baseRect.y + border, width, height);
			
			if (Event.current.type == EventType.ScrollWheel)
			{
				zoomAmount -= Event.current.delta.y * 0.01f;
				Event.current.Use();
				HandleUtility.Repaint();
			}
			
			GUI.DrawTexture(rect, tex);
			
			// Draw handles
			if (selectedMode == EditMode.Config)
			{
				DrawGridOverlay(spriteSheet, rect);
			}
			
			if (selectedMode == EditMode.Edit)
			{
				ProcessSpriteSelectionUI(spriteSheet, rect);
				
				// Cope with deleted selections
				List<SpriteCollectionEditorEntry> ss = new List<SpriteCollectionEditorEntry>();
				foreach (var sel in selectedSprites)
				{
					if (sel.index >= SpriteCollection.textureParams.Count)
						continue;
					var sprite = SpriteCollection.textureParams[sel.index];
					if (sprite.hasSpriteSheetId && sprite.spriteSheetId == spriteSheetId)
						ss.Add(sel);
				}
				selectedSprites = ss;
				
				// Draw selection outlines
				foreach (var sel in selectedSprites)
				{
					var sprite = SpriteCollection.textureParams[sel.index];
					int tileX = sprite.spriteSheetX;
					int tileY = sprite.spriteSheetY;
					int x, y;
					GetTileCoordinateForSpriteSheet(spriteSheet, tileX, tileY, out x, out y);
					Vector3 v0 = new Vector3(rect.x + x * zoomAmount, rect.x + y * zoomAmount, 0);
					Vector3 v1 = v0 + new Vector3(spriteSheet.tileWidth * zoomAmount, spriteSheet.tileHeight * zoomAmount, 0);
					Vector3[] rectVerts = { new Vector3(v0.x, v0.y, 0), new Vector3(v1.x, v0.y, 0), new Vector3(v1.x, v1.y, 0), new Vector3(v0.x, v1.y, 0) };
					Handles.DrawSolidRectangleWithOutline(rectVerts, new Color(1,1,1,0.2f), new Color(1,1,1,1));
				}
			}
		}
		
		public void Select(tk2dSpriteSheetSource spriteSheet, int[] ids)
		{
			selectedSprites.Clear();
			activeSelectedSprites.Clear();
			rectSelectX = rectSelectY = -1;
			textureViewScrollBar = Vector2.zero;
			inspectorScrollBar = Vector2.zero;
			activeSpriteSheetSource = spriteSheet;
			selectedMode = EditMode.Edit;	
			
			foreach (int id in ids)
			{
				var v = new SpriteCollectionEditorEntry();
				v.index = id;
				selectedSprites.Add(v);
			}
		}
		
		enum EditMode
		{
			Edit,
			Config
		}
		
		List<SpriteCollectionEditorEntry> selectedSprites = new List<SpriteCollectionEditorEntry>();
		List<SpriteCollectionEditorEntry> activeSelectedSprites = new List<SpriteCollectionEditorEntry>();
		int rectSelectX = -1, rectSelectY = -1;
		
		tk2dSpriteSheetSource activeSpriteSheetSource = null;
		EditMode selectedMode = EditMode.Edit;
		Vector2 textureViewScrollBar;
		Vector2 inspectorScrollBar;
		public bool Draw(List<SpriteCollectionEditorEntry> selectedEntries)
		{
			if (selectedEntries.Count == 0 || selectedEntries[0].type != SpriteCollectionEditorEntry.Type.SpriteSheet)
				return false;
			
			var entry = selectedEntries[selectedEntries.Count - 1];
			var spriteSheet = SpriteCollection.spriteSheets[ entry.index ];

			if (activeSpriteSheetSource != spriteSheet)
			{
				// reset state data
				selectedSprites.Clear();
				activeSelectedSprites.Clear();
				rectSelectX = rectSelectY = -1;
				textureViewScrollBar = Vector2.zero;
				inspectorScrollBar = Vector2.zero;
				activeSpriteSheetSource = spriteSheet;
				selectedMode = EditMode.Edit;
			}

			if (spriteSheet.tileWidth == 0 || spriteSheet.tileHeight == 0)
				selectedMode = EditMode.Config;			
			
			bool doDelete = false;
			GUILayout.BeginHorizontal();
			
			// Texture View
			GUILayout.BeginVertical(tk2dEditorSkin.SC_BodyBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
			textureViewScrollBar = GUILayout.BeginScrollView(textureViewScrollBar);
			if (spriteSheet.texture != null)
			{
				spriteSheet.texture.filterMode = FilterMode.Point;
				DrawTextureView(spriteSheet);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();

			// Inspector
			EditorGUIUtility.LookLikeControls(100.0f, 100.0f);
			inspectorScrollBar = GUILayout.BeginScrollView(inspectorScrollBar, GUILayout.ExpandHeight(true), GUILayout.Width(host.InspectorWidth));

			// Header
			GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorHeaderBG, GUILayout.ExpandWidth(true));
			GUILayout.Label("Sprite Sheet");
			GUILayout.BeginHorizontal();
			Texture2D newTexture = EditorGUILayout.ObjectField("Texture", spriteSheet.texture, typeof(Texture2D), false) as Texture2D;
			if (newTexture != spriteSheet.texture)
			{
				bool needFullRebuild = false;
				if (spriteSheet.texture == null)
					needFullRebuild = true;
				
				spriteSheet.texture = newTexture;
				if (needFullRebuild)
					host.OnSpriteCollectionChanged(true);
				else
					host.OnSpriteCollectionSortChanged();
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Delete", EditorStyles.miniButton)) doDelete = true;
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			
			bool textureReady = false;
			
			if (spriteSheet.texture != null)
			{
				GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				string assetPath = AssetDatabase.GetAssetPath(spriteSheet.texture);
				if (assetPath.Length > 0)
				{
					// make sure the source texture is npot and readable, and uncompressed
					if (tk2dSpriteCollectionBuilder.IsTextureImporterSetUp(assetPath))
					{
						textureReady = true;
					}
					else
					{
						if (tk2dGuiUtility.InfoBoxWithButtons(
							"The texture importer needs to be reconfigured to be used as a sprite sheet source. " +
							"Please note that this will globally change this texture importer.",
							tk2dGuiUtility.WarningLevel.Info,
							"Set up") != -1)
						{
							tk2dSpriteCollectionBuilder.ConfigureSpriteTextureImporter(assetPath);
							AssetDatabase.ImportAsset(assetPath);
						}						
					}
				}			
				GUILayout.EndVertical();
			}
			
			// Body
			if (textureReady)
			{
				GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
				selectedMode = (EditMode)GUILayout.Toolbar((int)selectedMode, new string[] { "Edit", "Config" } );
				EditorGUILayout.Space();
				GUILayout.EndVertical();
				
				if (selectedMode == EditMode.Edit)
				{
					if (Event.current.type == EventType.Layout)
						activeSelectedSprites = new List<SpriteCollectionEditorEntry>(selectedSprites);
					
					if (activeSelectedSprites.Count > 0)
						host.SpriteView.DrawSpriteEditorInspector(activeSelectedSprites, true, true);
				}
				else
				{
					GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
					
					spriteSheet.tileWidth = EditorGUILayout.IntField("Tile Width", spriteSheet.tileWidth);
					spriteSheet.tileHeight = EditorGUILayout.IntField("Tile Height", spriteSheet.tileHeight);
					spriteSheet.tileMarginX = EditorGUILayout.IntField("Tile Margin X", spriteSheet.tileMarginX);
					spriteSheet.tileMarginY = EditorGUILayout.IntField("Tile Margin Y", spriteSheet.tileMarginY);
					spriteSheet.tileSpacingX = EditorGUILayout.IntField("Tile Spacing X", spriteSheet.tileSpacingX);
					spriteSheet.tileSpacingY = EditorGUILayout.IntField("Tile Spacing Y", spriteSheet.tileSpacingY);
					
					spriteSheet.pad = (tk2dSpriteCollectionDefinition.Pad)EditorGUILayout.EnumPopup("Pad", spriteSheet.pad);
					if (spriteSheet.pad == tk2dSpriteCollectionDefinition.Pad.Default)
					{
						tk2dGuiUtility.InfoBox("The sprite sheet is configured to use default padding mode. " +
							"It is advised to select an explicit padding mode depending on the usage of the " +
							"sprites within the sprite sheet.\n\n" +
							"BlackZeroAlpha - Recommended for animations\n" +
							"Extend - Recommended for tilemaps", tk2dGuiUtility.WarningLevel.Warning);
					}
					
					// Apply button
					GUILayout.Space(8);
					GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
					if (spriteSheet.texture != null &&
						spriteSheet.tileWidth > 0 && spriteSheet.tileWidth <= spriteSheet.texture.width &&
						spriteSheet.tileHeight > 0 && spriteSheet.tileHeight <= spriteSheet.texture.height &&
						GUILayout.Button("Apply", EditorStyles.miniButton))
					{
						AddSprites(spriteSheet);
						selectedMode = EditMode.Edit;
					}
					GUILayout.EndHorizontal();
					
					GUILayout.EndVertical();
				}
			}
			
			GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			GUILayout.EndVertical();

			
			// /Body
			GUILayout.EndScrollView();

			// make dragable
			tk2dPreferences.inst.spriteCollectionInspectorWidth -= (int)tk2dGuiUtility.DragableHandle(4819284, GUILayoutUtility.GetLastRect(), 0, tk2dGuiUtility.DragDirection.Horizontal);

			GUILayout.EndHorizontal();
			
			if (doDelete)
			{
				string message = "Deleting a sprite sheet will delete all sprites sourced from this sprite sheet. " +
					"Are you sure you want to do this?";
				if (EditorUtility.DisplayDialog("Delete sprite sheet", message, "Yes", "No"))
				{
					SpriteCollection.DeleteSpriteSheet(spriteSheet);
					host.OnSpriteCollectionChanged(false);
				}
			}
			
			return true;
		}
	}
}
