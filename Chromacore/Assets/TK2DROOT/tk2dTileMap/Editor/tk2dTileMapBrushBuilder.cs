using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor 
{

	// Tile selection control used in the GUI
	public class BrushBuilder 
	{
		// tile selector
		int tileSelection_x0 = -1, tileSelection_y0 = -1;
		int tileSelection_x1 = -1, tileSelection_y1 = -1;
		List<int> tileSelection = new List<int>();
		bool multiSelect = false;
		bool rectSelect = false;
		
		public void Reset()
		{
			tileSelection_x0 = -1;
			tileSelection_x1 = -1;
			tileSelection_y0 = -1;
			tileSelection_y1 = -1;
			multiSelect = false;
			rectSelect = false;
			tileSelection.Clear();
		}
		
		void DrawSelectedTiles(Rect rect, Rect tileSize, int tilesPerRow)
		{
			if (!multiSelect && (tileSelection_x0 != -1 && tileSelection_y0 != -1))
			{
				int tx0 = (tileSelection_x0 < tileSelection_x1)?tileSelection_x0:tileSelection_x1;
				int tx1 = (tileSelection_x0 < tileSelection_x1)?tileSelection_x1:tileSelection_x0;
				int ty0 = (tileSelection_y0 < tileSelection_y1)?tileSelection_y0:tileSelection_y1;
				int ty1 = (tileSelection_y0 < tileSelection_y1)?tileSelection_y1:tileSelection_y0;
				
				Rect highlightRect = new Rect(rect.x + tx0 * tileSize.width, 
											  rect.y + ty0 * tileSize.height, 
											  (tx1 - tx0 + 1) * tileSize.width, 
											  (ty1 - ty0 + 1) * tileSize.height);
				Vector3[] rectVerts = { new Vector3(highlightRect.x, highlightRect.y, 0), 
										new Vector3(highlightRect.x + highlightRect.width, highlightRect.y, 0), 
										new Vector3(highlightRect.x + highlightRect.width, highlightRect.y + highlightRect.height, 0), 
										new Vector3(highlightRect.x, highlightRect.y + highlightRect.height, 0) };
				Handles.DrawSolidRectangleWithOutline(rectVerts, new Color(0,0,0,0.2f), new Color(0,0,0,1));
			}
			if (multiSelect && tileSelection.Count > 0)
			{
				foreach (int selectedTile in tileSelection)
				{
					int tx0 = selectedTile % tilesPerRow;
					int ty0 = selectedTile / tilesPerRow;
					
					Rect highlightRect = new Rect(rect.x + tx0 * tileSize.width, 
												  rect.y + ty0 * tileSize.height, 
												  tileSize.width, 
												  tileSize.height);
					Vector3[] rectVerts = { new Vector3(highlightRect.x, highlightRect.y, 0), 
											new Vector3(highlightRect.x + highlightRect.width, highlightRect.y, 0), 
											new Vector3(highlightRect.x + highlightRect.width, highlightRect.y + highlightRect.height, 0), 
											new Vector3(highlightRect.x, highlightRect.y + highlightRect.height, 0) };
					Handles.DrawSolidRectangleWithOutline(rectVerts, new Color(0,0,0,0.2f), new Color(0,0,0,1));
				}
			}
		}
		
		bool IsValidSprite(tk2dSpriteCollectionData spriteCollection, int spriteId)
		{
			return (spriteId >= 0 && spriteId < spriteCollection.Count 
				&& spriteCollection.spriteDefinitions[spriteId] != null
				&& spriteCollection.spriteDefinitions[spriteId].Valid);
		}
		
		void BuildBrush(tk2dSpriteCollectionData spriteCollection, tk2dTileMapEditorBrush brush, int tilesPerRow)
		{
			brush.name = "";
			brush.multiLayer = false;
			
			if (multiSelect)
			{
				List<int> filteredTileSelection = new List<int>();
				foreach (var v in tileSelection)
				{
					if (IsValidSprite(spriteCollection, v))
						filteredTileSelection.Add(v);
				}
				
				brush.type = tk2dTileMapEditorBrush.Type.MultiSelect;
				brush.multiSelectTiles = filteredTileSelection.ToArray();
				brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.None;
				brush.tiles = new tk2dSparseTile[0];
			}
			else
			{
				int tx0 = (tileSelection_x0 < tileSelection_x1)?tileSelection_x0:tileSelection_x1;
				int tx1 = (tileSelection_x0 < tileSelection_x1)?tileSelection_x1:tileSelection_x0;
				int ty0 = (tileSelection_y0 < tileSelection_y1)?tileSelection_y0:tileSelection_y1;
				int ty1 = (tileSelection_y0 < tileSelection_y1)?tileSelection_y1:tileSelection_y0;
				
				int numTilesX = tx1 - tx0 + 1;
				int numTilesY = ty1 - ty0 + 1;
				int numValidTiles = 0;
				
				tileSelection.Clear();
				List<tk2dSparseTile> tiles = new List<tk2dSparseTile>();
				for (int y = 0; y < numTilesY; ++y)
				{
					for (int x = 0; x < numTilesX; ++x)
					{
						ushort spriteId = (ushort)((y + ty0) * tilesPerRow + (x + tx0));
						if (IsValidSprite(spriteCollection, spriteId))
						{
							tiles.Add(new tk2dSparseTile(x, numTilesY - 1 - y, 0, spriteId));
							
							if (tileSelection.IndexOf(spriteId) == -1)
								tileSelection.Add(spriteId);
							
							numValidTiles++;
						}
					}
				}
			
				if (numTilesX == 1 && numTilesY == 3 && numValidTiles == 3) brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.Vertical;
				else if (numTilesX == 3 && numTilesY == 1 && numValidTiles == 3) brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.Horizontal;
				else if (numTilesX == 3 && numTilesY == 3 && numValidTiles == 9) brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.Square;
				else brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.None;
				
				brush.type = (rectSelect)?tk2dTileMapEditorBrush.Type.Rectangle:tk2dTileMapEditorBrush.Type.Single;
				brush.multiSelectTiles = tileSelection.ToArray();
				brush.tiles = tiles.ToArray();
			}

			brush.UpdateBrushHash();
		}
		
		void AddToSelection(int tilesPerRow)
		{
			int tx0 = (tileSelection_x0 < tileSelection_x1)?tileSelection_x0:tileSelection_x1;
			int tx1 = (tileSelection_x0 < tileSelection_x1)?tileSelection_x1:tileSelection_x0;
			int ty0 = (tileSelection_y0 < tileSelection_y1)?tileSelection_y0:tileSelection_y1;
			int ty1 = (tileSelection_y0 < tileSelection_y1)?tileSelection_y1:tileSelection_y0;
			for (int ty = ty0; ty < ty1 + 1; ty++)
			{
				for (int tx = tx0; tx < tx1 + 1; tx++)
				{
					ushort tileId = (ushort)(ty * tilesPerRow + tx);
					if (tileSelection.IndexOf(tileId) == -1)
						tileSelection.Add(tileId);
					else
						tileSelection.Remove(tileId);
				}
			}
		}
		
		public void HandleGUI(Rect rect, Rect tileSize, int tilesPerRow, tk2dSpriteCollectionData spriteCollection, tk2dTileMapEditorBrush brush)
		{
			DrawSelectedTiles(rect, tileSize, tilesPerRow);
			
			int controlID = GUIUtility.GetControlID(FocusType.Passive, rect);
			if (!rect.Contains(Event.current.mousePosition))
			{
				return;
			}
			
			Vector2 localClickPosition = Event.current.mousePosition - new Vector2(rect.x, rect.y);
			Vector2 tileLocalPosition = new Vector2(localClickPosition.x / tileSize.width, localClickPosition.y / tileSize.height);
			int tx = (int)tileLocalPosition.x;
			int ty = (int)tileLocalPosition.y;
			
			switch (Event.current.GetTypeForControl(controlID))
			{
			case EventType.MouseDown:
				bool multiSelectKeyDown = (Application.platform == RuntimePlatform.OSXEditor)?Event.current.command:Event.current.control;
				if (multiSelectKeyDown)
				{
					multiSelect = true;
					rectSelect = false;
					
					// Only add to selection when changing from single selection to multiselect
					if (tileSelection.Count == 0)
					{
						AddToSelection(tilesPerRow);
					}
				}
				else
				{
					Reset();
				}
				
				tileSelection_x0 = tx;
				tileSelection_y0 = ty;
				tileSelection_x1 = tx;
				tileSelection_y1 = ty;
				HandleUtility.Repaint();
				
				GUIUtility.hotControl = controlID;
				break;
			
			case EventType.MouseDrag:
				if (GUIUtility.hotControl == controlID)
				{
					tileSelection_x1 = tx;
					tileSelection_y1 = ty;
					HandleUtility.Repaint();
				}
				break;
				
			case EventType.MouseUp:
				if (GUIUtility.hotControl == controlID)
				{
					if (multiSelect)
					{
						AddToSelection(tilesPerRow);
					}
					
					// Build brush
					GUIUtility.hotControl = 0;
					BuildBrush(spriteCollection, brush, tilesPerRow);

					HandleUtility.Repaint();
				}
				break;
			}
		}
	}

}