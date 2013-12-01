using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class tk2dTileMapSceneGUI
{
	ITileMapEditorHost host;
	tk2dTileMap tileMap;
	tk2dTileMapData tileMapData;
	tk2dTileMapEditorData editorData;
	tk2dScratchpadGUI scratchpadGUI;

	int cursorX0 = 0, cursorY0 = 0;
	int cursorX = 0, cursorY = 0;
	int vertexCursorX = 0, vertexCursorY = 0;
	
	Color tileSelectionFillColor = new Color32(0, 128, 255, 32);
	Color tileSelectionOutlineColor = new Color32(0,200,255,255);	
	int currentModeForBrushColor = -1;

	tk2dTileMapEditorBrush workingBrush = null;
	tk2dTileMapEditorBrush WorkingBrush {
		get {
			if (workingBrush == null) workingBrush = new tk2dTileMapEditorBrush();
			return workingBrush;
		}
	}

	tk2dEditor.BrushRenderer brushRenderer = null;
	tk2dEditor.BrushRenderer BrushRenderer {
		get {
			if (brushRenderer == null) brushRenderer = new tk2dEditor.BrushRenderer(tileMap);
			return brushRenderer;
		}
	}

	// Data from one sprite in the collection
	Vector2 curSpriteDefTexelSize = Vector2.one;
	
	public tk2dTileMapSceneGUI(ITileMapEditorHost host, tk2dTileMap tileMap, tk2dTileMapEditorData editorData)
	{
		this.host = host;
		this.tileMap = tileMap;
		this.editorData = editorData;
		this.tileMapData = tileMap.data;
		
		// create default brush
		if (tileMap.SpriteCollectionInst && this.editorData)
		{
			this.editorData.InitBrushes(tileMap.SpriteCollectionInst);
			EditorUtility.SetDirty(this.editorData);
		}

		scratchpadGUI = new tk2dScratchpadGUI(this, BrushRenderer, WorkingBrush);
		if (editorData != null) {
			scratchpadGUI.SetActiveScratchpads(editorData.scratchpads);
		}
	}
	
	public void Destroy()
	{
		if (brushRenderer != null) {
			brushRenderer.Destroy();
			brushRenderer = null;
		}
	}
	
	void DrawCursorAt(int x, int y)
	{
		switch (tileMap.data.tileType)
		{
		case tk2dTileMapData.TileType.Rectangular:
			{
				float xOffsetMult, yOffsetMult;
				tileMap.data.GetTileOffset(out xOffsetMult, out yOffsetMult);
				float xOffset = (y & 1) * xOffsetMult;
				Vector3 p0 = new Vector3(tileMapData.tileOrigin.x + (x + xOffset) * tileMapData.tileSize.x, tileMapData.tileOrigin.y + y * tileMapData.tileSize.y, 0);
				Vector3 p1 = new Vector3(p0.x + tileMapData.tileSize.x, p0.y + tileMapData.tileSize.y, 0);
				Vector3[] v = new Vector3[4];
				v[0] = new Vector3(p0.x, p0.y, 0);
				v[1] = new Vector3(p1.x, p0.y, 0);
				v[2] = new Vector3(p1.x, p1.y, 0);
				v[3] = new Vector3(p0.x, p1.y, 0);
				
				for (int i = 0; i < v.Length; ++i)
					v[i] = tileMap.transform.TransformPoint(v[i]);
				
				Handles.DrawSolidRectangleWithOutline(v, tileSelectionFillColor, tileSelectionOutlineColor);
			}	
			break;
		case tk2dTileMapData.TileType.Isometric:
			{
				float xOffsetMult, yOffsetMult;
				tileMap.data.GetTileOffset(out xOffsetMult, out yOffsetMult);
				float xOffset = (y & 1) * xOffsetMult;
				Vector3 p0 = new Vector3(tileMapData.tileOrigin.x + (x + xOffset) * tileMapData.tileSize.x, tileMapData.tileOrigin.y + y * tileMapData.tileSize.y, 0);
				Vector3 p1 = new Vector3(p0.x + tileMapData.tileSize.x, p0.y + tileMapData.tileSize.y * 2, 0);
				Vector3[] v = new Vector3[4];
				v[0] = new Vector3(p0.x + (p1.x-p0.x)*0.5f, p0.y, 0);
				v[1] = new Vector3(p1.x, p0.y + (p1.y-p0.y)*0.5f, 0);
				v[2] = new Vector3(p1.x - (p1.x-p0.x)*0.5f, p1.y, 0);
				v[3] = new Vector3(p0.x, p1.y - (p1.y-p0.y)*0.5f, 0);
				
				for (int i = 0; i < v.Length; ++i)
					v[i] = tileMap.transform.TransformPoint(v[i]);
				
				Handles.DrawSolidRectangleWithOutline(v, tileSelectionFillColor, tileSelectionOutlineColor);
			}	
			break;
		}
	}

	void DrawTileMapRectCursor(int x0, int y0, int x1, int y1)
	{
		Vector3 p0 = new Vector3(tileMapData.tileOrigin.x + x0 * tileMapData.tileSize.x, tileMapData.tileOrigin.y + y0 * tileMapData.tileSize.y, 0);
		Vector3 p1 = new Vector3(tileMapData.tileOrigin.x + (x1 + 1) * tileMapData.tileSize.x, tileMapData.tileOrigin.y + (y1 + 1) * tileMapData.tileSize.y, 0);

		float layerDepth = GetLayerDepth(editorData.layer);

		Vector3[] v = new Vector3[4];
		v[0] = new Vector3(p0.x, p0.y, layerDepth);
		v[1] = new Vector3(p1.x, p0.y, layerDepth);
		v[2] = new Vector3(p1.x, p1.y, layerDepth);
		v[3] = new Vector3(p0.x, p1.y, layerDepth);
		
		for (int i = 0; i < v.Length; ++i)
			v[i] = tileMap.transform.TransformPoint(v[i]);
		
		Handles.DrawSolidRectangleWithOutline(v, tileSelectionFillColor, tileSelectionOutlineColor);
	}

	void DrawScratchpadRectCursor(int x1, int y1, int x2, int y2) {
		if (scratchpadGUI.padAreaRect.width < 1.0f || scratchpadGUI.padAreaRect.height < 1.0f)
			return;

		float scale = scratchpadGUI.scratchZoom;

		Vector3 p0 = new Vector3(x1 * scale * (tileMapData.tileSize.x / curSpriteDefTexelSize.x), y1 * scale * (tileMapData.tileSize.y / curSpriteDefTexelSize.y), 0);
		Vector3 p1 = new Vector3((x2 + 1) * scale * (tileMapData.tileSize.x / curSpriteDefTexelSize.x), (y2 + 1) * scale * (tileMapData.tileSize.y / curSpriteDefTexelSize.y), 0);

		float temp = p0.y;
		p0.y = scratchpadGUI.padAreaRect.height - p1.y;
		p1.y = scratchpadGUI.padAreaRect.height - temp;

		Rect viewRect = tk2dSpriteThumbnailCache.VisibleRect;
		p0.x = Mathf.Max(p0.x, viewRect.xMin);
		p0.y = Mathf.Max(p0.y, viewRect.yMin);
		p1.x = Mathf.Min(p1.x, viewRect.xMax);
		p1.y = Mathf.Min(p1.y, viewRect.yMax);
		if (p0.x > p1.x || p0.y > p1.y)
			return;

		Vector3[] v = new Vector3[4];
		v[0] = new Vector3(p0.x, p0.y, 0);
		v[1] = new Vector3(p1.x, p0.y, 0);
		v[2] = new Vector3(p1.x, p1.y, 0);
		v[3] = new Vector3(p0.x, p1.y, 0);
		
		for (int i = 0; i < v.Length; ++i)
			v[i] = new Vector3(v[i].x, v[i].y, 0.0f);

		Handles.DrawSolidRectangleWithOutline(v, tileSelectionFillColor, tileSelectionOutlineColor);
	}
	
	public void DrawTileCursor()
	{	
		// Where to draw the cursor
		bool workTiles = false;
		bool mouseRect = false;

		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Brush || tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.BrushRandom) {
			workTiles = true;
		}
		else { // Erase, Eyedropper or Cut
			mouseRect = true;
		}

		if (!workTiles && !mouseRect) return;
		int x1 = cursorX;
		int y1 = cursorY;
		int x2 = cursorX;
		int y2 = cursorY;
		if (workTiles) {
			foreach (var tile in WorkingBrush.tiles) {
				x1 = Mathf.Min (x1, tile.x);
				y1 = Mathf.Min (y1, tile.y);
				x2 = Mathf.Max (x2, tile.x);
				y2 = Mathf.Max (y2, tile.y);
			}
		}
		else if (mouseRect) {
			x1 = Mathf.Min(cursorX, cursorX0);
			y1 = Mathf.Min(cursorY, cursorY0);
			x2 = Mathf.Max(cursorX, cursorX0);
			y2 = Mathf.Max(cursorY, cursorY0);
		}

		// Clip rect appropriately
		int clipW, clipH;
		if (scratchpadGUI.workingHere) {
			scratchpadGUI.GetScratchpadSize(out clipW, out clipH);
		} else {
			clipW = tileMap.width;
			clipH = tileMap.height;
		}
		x1 = Mathf.Max(0, x1);
		y1 = Mathf.Max(0, y1);
		x2 = Mathf.Min(clipW - 1, x2);
		y2 = Mathf.Min(clipH - 1, y2);
		if (x2 < x1 || y2 < y1)
			return;

		// Draw cursor
		if (tileMap.data.tileType == tk2dTileMapData.TileType.Rectangular) {
			if (scratchpadGUI.workingHere) {
				DrawScratchpadRectCursor(x1, y1, x2, y2);
			} else {
				DrawTileMapRectCursor(x1, y1, x2, y2);
			}
		} else {
			// would be nice to have an isometric outlined poly...
			if (scratchpadGUI.workingHere) {
				;
			} else {
				for (int y = y1; y <= y2; ++y) {
					for (int x = x1; x <= x2; ++x) {
						DrawCursorAt(x, y);
					}
				}
			}
		}
	}

	void DrawPaintCursor()
	{
		float layerZ = 0.0f;
		
		Vector3 p0 = new Vector3(tileMapData.tileOrigin.x + vertexCursorX * tileMapData.tileSize.x, tileMapData.tileOrigin.y + vertexCursorY * tileMapData.tileSize.y, layerZ);
		float radius = Mathf.Max(tileMapData.tileSize.x, tileMapData.tileSize.y) * tk2dTileMapToolbar.colorBrushRadius;

		// We get intensity, and tint the handle color by this.
		float t = tk2dTileMapToolbar.colorBrushIntensity;
		Color c = editorData.brushColor;

		Color oldColor = Handles.color;
		Handles.color = new Color( c.r, c.g, c.b, t * 0.7f + 0.3f );
		Handles.DrawWireDisc(tileMap.transform.TransformPoint(p0), tileMap.transform.TransformDirection(Vector3.forward), radius);
		Handles.color = oldColor;
	}
	
	void DrawOutline()
	{
		Vector3 p0 = tileMapData.tileOrigin;
		Vector3 p1 = new Vector3(p0.x + tileMapData.tileSize.x * tileMap.width, p0.y + tileMapData.tileSize.y * tileMap.height, 0);
		
		Vector3[] v = new Vector3[5];
		v[0] = new Vector3(p0.x, p0.y, 0);
		v[1] = new Vector3(p1.x, p0.y, 0);
		v[2] = new Vector3(p1.x, p1.y, 0);
		v[3] = new Vector3(p0.x, p1.y, 0);
		v[4] = new Vector3(p0.x, p0.y, 0);
		
		for (int i = 0; i < 5; ++i)
		{
			v[i] = tileMap.transform.TransformPoint(v[i]);
		}
		
		Handles.DrawPolyLine(v);
	}

	public static int tileMapHashCode = "TileMap".GetHashCode();
	
	bool UpdateCursorPosition()
	{
		if (scratchpadGUI.workingHere) {
			cursorX = (int)((scratchpadGUI.paintMousePosition.x / scratchpadGUI.scratchZoom) / (tileMapData.tileSize.x / curSpriteDefTexelSize.x));
			cursorY = (int)((scratchpadGUI.paintMousePosition.y / scratchpadGUI.scratchZoom) / (tileMapData.tileSize.y / curSpriteDefTexelSize.y));
			return true;
		}

		bool isInside = false;

		Vector3 layerDepthOffset = new Vector3(0, 0, GetLayerDepth(editorData.layer));
		layerDepthOffset = tileMap.transform.TransformDirection(layerDepthOffset);
		
		Plane p = new Plane(tileMap.transform.forward, tileMap.transform.position + layerDepthOffset);
		Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
		float hitD = 0.0f;
		
		if (p.Raycast(r, out hitD))
		{
			float fx, fy;
			if (tileMap.GetTileFracAtPosition(r.GetPoint(hitD), out fx, out fy))
			{
				isInside = true;
			}
			int x = (int)(fx);
			int y = (int)(fy);
				
			cursorX = x;
			cursorY = y;
			vertexCursorX = (int)Mathf.Round(fx);
			vertexCursorY = (int)Mathf.Round(fy);
				
			HandleUtility.Repaint();
				
		}
		
		return isInside;
	}
	
	bool IsCursorInside()
	{
		return UpdateCursorPosition();
	}

	void SplatWorkingBrush() {
		foreach (var tile in WorkingBrush.tiles) {
			if (tile.spriteId != -1) {
				SplatTile(tile.x, tile.y, tile.layer, tile.spriteId);
			}
		}
	}

	tk2dTileMapToolbar.MainMode pushedToolbarMainMode = tk2dTileMapToolbar.MainMode.Brush;
	bool pencilDragActive = false;
	tk2dTileMapToolbar.ColorBlendMode pushedToolbarColorBlendMode = tk2dTileMapToolbar.ColorBlendMode.Replace;
	bool hotkeyModeSwitchActive = false;

	void PencilDrag() {
		pencilDragActive = true;

		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Cut || 
			tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Eyedropper) {
			// fake hotkey so it will pop back to brush mode
			hotkeyModeSwitchActive = true;
			pushedToolbarMainMode = tk2dTileMapToolbar.MainMode.Brush;
		}

		host.BuildIncremental();
	}

	void RectangleDragBegin() {
	}

	void RectangleDragEnd() {
		if (!pencilDragActive)
			return;

		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Brush || tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.BrushRandom) {
			SplatWorkingBrush();
		}
		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Eyedropper ||
		    tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Cut)
		{
			PickUpBrush(editorData.activeBrush, false);
			tk2dTileMapToolbar.workBrushFlipX = false;
			tk2dTileMapToolbar.workBrushFlipY = false;
			tk2dTileMapToolbar.workBrushRot90 = false;
		}
		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Erase || tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Cut) {
			int x0 = Mathf.Min(cursorX, cursorX0);
			int x1 = Mathf.Max(cursorX, cursorX0);
			int y0 = Mathf.Min(cursorY, cursorY0);
			int y1 = Mathf.Max(cursorY, cursorY0);
			
			if (scratchpadGUI.workingHere) {
				scratchpadGUI.EraseTiles(x0, y0, x1, y1);
			} else {
				tileMap.DeleteSprites(editorData.layer, x0, y0, x1, y1);
			}
		}

		host.BuildIncremental();

		pencilDragActive = false;
	}
	
	void CheckVisible(int layer)
	{
		if (tileMap != null && 
			tileMap.Layers != null &&
			layer < tileMap.Layers.Length &&
			tileMap.Layers[layer].gameObject != null &&
			tk2dEditorUtility.IsGameObjectActive(tileMap.Layers[layer].gameObject) == false)
		{
			tk2dEditorUtility.SetGameObjectActive(tileMap.Layers[layer].gameObject, true);
		}
	}

	Vector2 tooltipPos = Vector2.zero;
	bool lastScratchpadOpen = false;
	bool openedScratchpadWithTab = false;

	float GetLayerDepth(int idx) {
		var layers = tileMapData.Layers;
		if (idx < 0 || idx >= layers.Length)
			return 0.0f;
		if (tileMapData.layersFixedZ) {
			return -(layers[idx].z);
		} else {
			float result = 0.0f;
			for (int i = 1; i <= idx; ++i) {
				result -= layers[idx].z;
			}
			return result;
		}
	}

	void UpdateScratchpadTileSizes() {
		if (scratchpadGUI != null && tileMap != null && tileMapData != null) {
			var spriteDef = tileMap.SpriteCollectionInst.FirstValidDefinition;
			if (spriteDef != null)
				scratchpadGUI.SetTileSizes(new Vector3(tileMapData.tileSize.x / spriteDef.texelSize.x, tileMapData.tileSize.y / spriteDef.texelSize.y, 0),
					spriteDef.texelSize);
		}
	}

	public void OnSceneGUI()
	{
		// Always draw the outline
		DrawOutline();
		
		if (Application.isPlaying || !tileMap.AllowEdit)
			return;
		
		if (editorData.editMode == tk2dTileMapEditorData.EditMode.Settings)
		{
			return;
		}
		
		if (editorData.editMode != tk2dTileMapEditorData.EditMode.Paint && 
			editorData.editMode != tk2dTileMapEditorData.EditMode.Color)
			return;

		if (editorData.editMode == tk2dTileMapEditorData.EditMode.Color &&
			!tileMap.HasColorChannel())
			return;

		if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
			tooltipPos = Event.current.mousePosition;

		UpdateScratchpadTileSizes();

		// Scratchpad tilesort
		scratchpadGUI.SetTileSort(tileMapData.sortMethod == tk2dTileMapData.SortMethod.BottomLeft || tileMapData.sortMethod == tk2dTileMapData.SortMethod.TopLeft,
			tileMapData.sortMethod == tk2dTileMapData.SortMethod.BottomLeft || tileMapData.sortMethod == tk2dTileMapData.SortMethod.BottomRight);

		// Spritedef vars
		if (tileMap != null) {
			var spriteDef = tileMap.SpriteCollectionInst.FirstValidDefinition;
			if (spriteDef != null) {
				curSpriteDefTexelSize = spriteDef.texelSize;
			}
		}

		// Working brush / tile or paint cursor (behind scratchpad)
		switch (editorData.editMode)
		{
		case tk2dTileMapEditorData.EditMode.Paint:
			if (!scratchpadGUI.workingHere) {
				if (Event.current.type == EventType.Repaint) {
					Matrix4x4 matrix = tileMap.transform.localToWorldMatrix;
					// Brush mesh is offset so origin is at bottom left. Do the reverse here...
					// Also add layer depth
					float layerDepth = GetLayerDepth(editorData.layer);
					matrix *= Matrix4x4.TRS (tileMapData.tileOrigin + new Vector3(0, 0, layerDepth), Quaternion.identity, Vector3.one);
					BrushRenderer.DrawBrushInScene(matrix, WorkingBrush, 1000000);
				}
				DrawTileCursor();
			}
			break;
		case tk2dTileMapEditorData.EditMode.Color:
			DrawPaintCursor();
			break;
		}

		// Toolbar and scratchpad
		if (editorData.editMode == tk2dTileMapEditorData.EditMode.Paint) {
			Handles.BeginGUI();
			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

			Event ev = Event.current;
			bool mouseOverToolbar = false;
			bool mouseOverScratchpad = false;

			GUILayout.BeginVertical(tk2dEditorSkin.GetStyle("TilemapToolbarBG"), GUILayout.Width(20), GUILayout.Height(34));
			tk2dTileMapToolbar.ToolbarWindow();
			GUILayout.EndVertical();
			mouseOverToolbar = GUILayoutUtility.GetLastRect().Contains(ev.mousePosition);

			if (tk2dTileMapToolbar.scratchpadOpen) {
				GUILayout.BeginVertical(tk2dEditorSkin.GetStyle("TilemapToolbarBG"), GUILayout.Width(20), GUILayout.Height(Screen.height - 74));
				scratchpadGUI.DrawGUI();
				GUILayout.EndVertical();
				mouseOverScratchpad = GUILayoutUtility.GetLastRect().Contains(ev.mousePosition);
			}

			if (ev.type == EventType.MouseMove) {
				scratchpadGUI.workingHere = mouseOverToolbar || mouseOverScratchpad;
				if (scratchpadGUI.workingHere) ev.Use();
			}

			GUILayout.EndArea();
			Handles.EndGUI();
		} else {
			scratchpadGUI.workingHere = false;
		}

		// Draw Tooltip
		string curTooltip = "";
		if (tk2dTileMapToolbar.tooltip.Length > 0) curTooltip = tk2dTileMapToolbar.tooltip;
		if (scratchpadGUI.tooltip.Length > 0) curTooltip = scratchpadGUI.tooltip;
		if (curTooltip.Length > 0 && scratchpadGUI.workingHere) {
			Handles.BeginGUI();
			GUI.contentColor = Color.white;
			GUIContent tooltipContent = new GUIContent(curTooltip);
			Vector2 size = GUI.skin.GetStyle("label").CalcSize(tooltipContent);
			GUI.Label(new Rect(tooltipPos.x, tooltipPos.y + 20, size.x + 10, size.y + 5), curTooltip, "textarea");
			Handles.EndGUI();
		}

		if (tk2dTileMapToolbar.scratchpadOpen && !lastScratchpadOpen) {
			scratchpadGUI.FocusOnSearchFilter(openedScratchpadWithTab);
		}
		if (!tk2dTileMapToolbar.scratchpadOpen && lastScratchpadOpen) {
			openedScratchpadWithTab = false;
		}
		lastScratchpadOpen = tk2dTileMapToolbar.scratchpadOpen;

		int controlID = tileMapHashCode;

		if (tk2dTileMapToolbar.scratchpadOpen && scratchpadGUI.workingHere) {
			if (scratchpadGUI.doMouseDown) {
				GUIUtility.hotControl = controlID;
				Undo.RegisterUndo(editorData, "Edit scratchpad");
				randomSeed = Random.Range(0, int.MaxValue);

				cursorX0 = cursorX;
				cursorY0 = cursorY;

				PencilDrag();
				RectangleDragBegin();
			}
			if (scratchpadGUI.doMouseDrag) {
				UpdateCursorPosition();
				UpdateWorkingBrush();

				PencilDrag();
			}
			if (scratchpadGUI.doMouseUp) {
				RectangleDragEnd();

				cursorX0 = cursorX;
				cursorY0 = cursorY;
				UpdateWorkingBrush();

				GUIUtility.hotControl = 0;
			}
			if (scratchpadGUI.doMouseMove) {
				UpdateCursorPosition();
				cursorX0 = cursorX;
				cursorY0 = cursorY;
				UpdateWorkingBrush();
			}
		}
		else
		{
			EventType controlEventType = Event.current.GetTypeForControl(controlID);
			switch (controlEventType)
			{
			case EventType.MouseDown:
			case EventType.MouseDrag:
				if ((controlEventType == EventType.MouseDrag && GUIUtility.hotControl != controlID) ||
					(Event.current.button != 0 && Event.current.button != 1))
				{
					return;
				}

				// make sure we don't use up reserved combinations
				bool inhibitMouseDown = false;
				if (Application.platform == RuntimePlatform.OSXEditor) {
					if (Event.current.command && Event.current.alt) { // pan combination on mac
						inhibitMouseDown = true;
					}
				}
				
				if (Event.current.type == EventType.MouseDown && !inhibitMouseDown)
				{
					CheckVisible(editorData.layer);
					
					if (IsCursorInside() && !Event.current.shift)
					{
						if (editorData.editMode == tk2dTileMapEditorData.EditMode.Paint)
						{
							GUIUtility.hotControl = controlID;
							Undo.RegisterUndo(tileMap, "Edit tile map");
							randomSeed = Random.Range(0, int.MaxValue);

							PencilDrag();
							RectangleDragBegin();
						}
						if (editorData.editMode == tk2dTileMapEditorData.EditMode.Color) {
							GUIUtility.hotControl = controlID;
							Undo.RegisterUndo(tileMap, "Paint tile map");
							if (tk2dTileMapToolbar.colorBlendMode != tk2dTileMapToolbar.ColorBlendMode.Eyedropper) {
								PaintColorBrush((float)vertexCursorX, (float)vertexCursorY);
								host.BuildIncremental();
							}
						}
					}
				}
			
				if (Event.current.type == EventType.MouseDrag && GUIUtility.hotControl == controlID)
				{
					UpdateCursorPosition();
					UpdateWorkingBrush();
					if (editorData.editMode == tk2dTileMapEditorData.EditMode.Paint) {
						PencilDrag();
					}
					if (editorData.editMode == tk2dTileMapEditorData.EditMode.Color) {
						if (tk2dTileMapToolbar.colorBlendMode != tk2dTileMapToolbar.ColorBlendMode.Eyedropper) {
							PaintColorBrush((float)vertexCursorX, (float)vertexCursorY);
							host.BuildIncremental();
						}
					}
				}
				
				break;
				
			case EventType.MouseUp:
				if ((Event.current.button == 0 || Event.current.button == 1) && GUIUtility.hotControl == controlID)
				{
					GUIUtility.hotControl = 0;

					if (editorData.editMode == tk2dTileMapEditorData.EditMode.Paint) {
						RectangleDragEnd();
					}
					if (editorData.editMode == tk2dTileMapEditorData.EditMode.Color) {
						if (tk2dTileMapToolbar.colorBlendMode == tk2dTileMapToolbar.ColorBlendMode.Eyedropper) {
							PickUpColor();
						}
					}

					cursorX0 = cursorX;
					cursorY0 = cursorY;
					UpdateWorkingBrush();
					
					HandleUtility.Repaint();
				}
				break;
				
			case EventType.Layout:
				//HandleUtility.AddDefaultControl(controlID);
				break;
				
			case EventType.MouseMove:
				UpdateCursorPosition();
				cursorX0 = cursorX;
				cursorY0 = cursorY;
				UpdateWorkingBrush();
				break;
			}
		}

		// Set cursor color based on toolbar main mode
		bool updateCursorColor = false;
		if (currentModeForBrushColor != (int)tk2dTileMapToolbar.mainMode) {
			currentModeForBrushColor = (int)tk2dTileMapToolbar.mainMode;
			updateCursorColor = true;
		}
		if (tk2dPreferencesEditor.CheckTilemapCursorColorUpdate()) {
			updateCursorColor = true;
		}
		if (updateCursorColor) {
			switch (tk2dTileMapToolbar.mainMode) {
			case tk2dTileMapToolbar.MainMode.Brush:
				tileSelectionFillColor = tk2dPreferences.inst.tileMapToolColor_brush;
				break;
			case tk2dTileMapToolbar.MainMode.BrushRandom:
				tileSelectionFillColor = tk2dPreferences.inst.tileMapToolColor_brushRandom;
				break;
			case tk2dTileMapToolbar.MainMode.Erase:
				tileSelectionFillColor = tk2dPreferences.inst.tileMapToolColor_erase;
				break;
			case tk2dTileMapToolbar.MainMode.Eyedropper:
				tileSelectionFillColor = tk2dPreferences.inst.tileMapToolColor_eyedropper;
				break;
			case tk2dTileMapToolbar.MainMode.Cut:
				tileSelectionFillColor = tk2dPreferences.inst.tileMapToolColor_cut;
				break;
			}
			tileSelectionOutlineColor = (tileSelectionFillColor + Color.white) * new Color(0.5f, 0.5f, 0.5f, 1.0f);
		}

		// Hotkeys switch the static toolbar mode
		{
			bool pickupKeyDown = (Application.platform == RuntimePlatform.OSXEditor)?Event.current.control:Event.current.alt;
			if (Event.current.button == 1) pickupKeyDown = true;
			bool eraseKeyDown = false;
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				if (Event.current.command && !Event.current.alt)
					eraseKeyDown = true;
			}
			else eraseKeyDown = Event.current.control;
			bool hotkeysPressed = pickupKeyDown || eraseKeyDown;

			if (editorData.editMode == tk2dTileMapEditorData.EditMode.Paint) {

				if (!pencilDragActive) {
					if (hotkeysPressed) {
						if (!hotkeyModeSwitchActive) {
							// Push mode
							pushedToolbarMainMode = tk2dTileMapToolbar.mainMode;
						}
						if (pickupKeyDown) {
							if (eraseKeyDown) {
								pendingModeChange = delegate(int i) {
									tk2dTileMapToolbar.mainMode = tk2dTileMapToolbar.MainMode.Cut;
									hotkeyModeSwitchActive = true;
								};
							}
							else {
								pendingModeChange = delegate(int i) {
									tk2dTileMapToolbar.mainMode = tk2dTileMapToolbar.MainMode.Eyedropper;
									hotkeyModeSwitchActive = true;
								};
							}
						}
						else if (eraseKeyDown) {
							pendingModeChange = delegate(int i) {
								tk2dTileMapToolbar.mainMode = tk2dTileMapToolbar.MainMode.Erase;
								hotkeyModeSwitchActive = true;
							};
						}
					} else {
						if (hotkeyModeSwitchActive) {
							// Pop mode
							pendingModeChange = delegate(int i) {
								tk2dTileMapToolbar.mainMode = pushedToolbarMainMode;
								hotkeyModeSwitchActive = false;
							};
						}
					}
				}

			}
			if (editorData.editMode == tk2dTileMapEditorData.EditMode.Color) {
				if (hotkeysPressed) {
					if (!hotkeyModeSwitchActive) {
						// Push mode
						pushedToolbarColorBlendMode = tk2dTileMapToolbar.colorBlendMode;

						if (pickupKeyDown) {
							tk2dTileMapToolbar.colorBlendMode = tk2dTileMapToolbar.ColorBlendMode.Eyedropper;
							hotkeyModeSwitchActive = true;
						} else if (eraseKeyDown) {
							switch (tk2dTileMapToolbar.colorBlendMode) {
								case tk2dTileMapToolbar.ColorBlendMode.Addition:
									tk2dTileMapToolbar.colorBlendMode = tk2dTileMapToolbar.ColorBlendMode.Subtraction;
									break;
								case tk2dTileMapToolbar.ColorBlendMode.Subtraction:
									tk2dTileMapToolbar.colorBlendMode = tk2dTileMapToolbar.ColorBlendMode.Addition;
									break;
							}
							hotkeyModeSwitchActive = true;
						}
					}
				} else {
					if (hotkeyModeSwitchActive) {
						tk2dTileMapToolbar.colorBlendMode = pushedToolbarColorBlendMode;
						hotkeyModeSwitchActive = false;
					}
				}
			}
		}
		// Hotkeys toggle flipping, scratchpad, paint
		{
			Event ev = Event.current;
			if (ev.type == EventType.KeyDown) {
				if (ev.keyCode == KeyCode.Tab) {
					if (!tk2dTileMapToolbar.scratchpadOpen) {
						pendingModeChange = delegate(int i) {
							tk2dTileMapToolbar.scratchpadOpen = !tk2dTileMapToolbar.scratchpadOpen;
							if (tk2dTileMapToolbar.scratchpadOpen)
								openedScratchpadWithTab = true;
						};
					}
				}
				switch (ev.character) {
				case 'h':
					ev.Use();
					tk2dTileMapToolbar.workBrushFlipX = !tk2dTileMapToolbar.workBrushFlipX;
					UpdateWorkingBrush();
					break;
				case 'j':
					ev.Use();
					tk2dTileMapToolbar.workBrushFlipY = !tk2dTileMapToolbar.workBrushFlipY;
					UpdateWorkingBrush();
					break;
				case '[':
					ev.Use();
					tk2dTileMapToolbar.colorBrushRadius -= 0.5f;
					if (tk2dTileMapToolbar.colorBrushRadius < 1.0f) tk2dTileMapToolbar.colorBrushRadius = 1.0f;
					break;
				case ']':
					ev.Use();
					tk2dTileMapToolbar.colorBrushRadius += 0.5f;
					break;
				case '-': case '_':
					ev.Use();
					tk2dTileMapToolbar.colorBrushIntensity -= 0.01f;
					break;
				case '=': case '+':
					ev.Use();
					tk2dTileMapToolbar.colorBrushIntensity += 0.01f;
					break;
				}
			}
			if (scratchpadGUI.requestClose) {
				scratchpadGUI.requestClose = false;
				pendingModeChange = delegate(int i) {
					tk2dTileMapToolbar.scratchpadOpen = false;
				};
			}
		}
		// Hotkey (enter) selects scratchpad tiles
		if (scratchpadGUI.requestSelectAllTiles) {
			// fake mouse coords
			scratchpadGUI.GetTilesCropRect(out cursorX, out cursorY, out cursorX0, out cursorY0);
			PickUpBrush(editorData.activeBrush, false);
			tk2dTileMapToolbar.workBrushFlipX = false;
			tk2dTileMapToolbar.workBrushFlipY = false;
			tk2dTileMapToolbar.workBrushRot90 = false;

			scratchpadGUI.requestSelectAllTiles = false;
		}

		if (pendingModeChange != null && Event.current.type == EventType.Repaint) {
			pendingModeChange(0);
			pendingModeChange = null;
			UpdateWorkingBrush();
			HandleUtility.Repaint();
		}
	}

	System.Action<int> pendingModeChange = null;
	
	void SplatTile(int x, int y, int layerId, int spriteId)
	{
		if (scratchpadGUI.workingHere) {
			scratchpadGUI.SplatTile(x, y, spriteId);
		} else {
			if (x >= 0 && x < tileMap.width &&
				y >= 0 && y < tileMap.height &&
				layerId >= 0 && layerId < tileMap.data.NumLayers)
			{
				var layer =	tileMap.Layers[layerId];
				layer.SetRawTile(x, y, spriteId);
			}
		}
	}

	int randomSeed = 0;

	public void UpdateWorkingBrush()
	{
		tk2dTileMapEditorBrush workBrush = WorkingBrush;
		tk2dTileMapEditorBrush activeBrush = editorData.activeBrush;

		int rectX1 = Mathf.Min(cursorX, cursorX0);
		int rectX2 = Mathf.Max(cursorX, cursorX0);
		int rectY1 = Mathf.Min(cursorY, cursorY0);
		int rectY2 = Mathf.Max(cursorY, cursorY0);

		int xoffset = 0;
		if (tileMap.data.tileType == tk2dTileMapData.TileType.Isometric && (cursorY & 1) == 1) 
			xoffset = 1;

		workBrush.tiles = new tk2dSparseTile[0]; 

		tk2dSparseTile[] srcTiles;
		if (activeBrush.type != tk2dTileMapEditorBrush.Type.MultiSelect) {
			srcTiles = activeBrush.tiles;
		} else {
			int n = activeBrush.multiSelectTiles.Length;
			srcTiles = new tk2dSparseTile[n];
			for (int i = 0; i < n; ++i) {
				srcTiles[i] = new tk2dSparseTile(i, 0, editorData.layer, activeBrush.multiSelectTiles[i]);
			}
		}

		if (srcTiles.Length == 0) {
			workBrush.UpdateBrushHash();
			return;
		}

		bool flipH = tk2dTileMapToolbar.workBrushFlipX;
		bool flipV = tk2dTileMapToolbar.workBrushFlipY;
		bool rot90 = tk2dTileMapToolbar.workBrushRot90;

		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.Brush) {
			if (rectX1 == rectX2 && rectY1 == rectY2) {
				int nTiles = srcTiles.Length;
				workBrush.tiles = new tk2dSparseTile[nTiles];
				
				for (int i = 0; i < nTiles; ++i) {
					int spriteId = srcTiles[i].spriteId;
					int tx = srcTiles[i].x;
					int ty = srcTiles[i].y;

					if (rot90) {
						int tmp = tx;
						tx = ty;
						ty = -tmp;
						tk2dRuntime.TileMap.BuilderUtil.SetRawTileFlag(ref spriteId, tk2dTileFlags.Rot90, true);
					}
					if (flipH) {
						tx = -tx;
						tk2dRuntime.TileMap.BuilderUtil.InvertRawTileFlag(ref spriteId, tk2dTileFlags.FlipX);
					}
					if (flipV) {
						ty = -ty;
						tk2dRuntime.TileMap.BuilderUtil.InvertRawTileFlag(ref spriteId, tk2dTileFlags.FlipY);
					}
					
					int thisRowXOffset = ((ty & 1) == 1) ? xoffset : 0;
					
					workBrush.tiles[i] = new tk2dSparseTile(
						cursorX + tx + thisRowXOffset,
						cursorY + ty,
						editorData.layer,
						spriteId	);
				}
			} else {
				int gridWidth = 1 + rectX2 - rectX1;
				int gridHeight = 1 + rectY2 - rectY1;
				workBrush.tiles = new tk2dSparseTile[gridWidth * gridHeight];

				// fill with tiles repeated pattern...
				int patternX1 = 0;
				int patternY1 = 0;
				int patternX2 = 0;
				int patternY2 = 0;
				foreach (var tile in srcTiles) {
					patternX1 = Mathf.Min (patternX1, tile.x);
					patternY1 = Mathf.Min (patternY1, tile.y);
					patternX2 = Mathf.Max (patternX2, tile.x);
					patternY2 = Mathf.Max (patternY2, tile.y);
				}
				int patternW = 1 + patternX2 - patternX1;
				int patternH = 1 + patternY2 - patternY1;

				int idx = 0;
				for (int y = 0; y < gridHeight; ++y) {
					int thisRowXOffset = ((y & 1) == 1) ? xoffset : 0;
					for (int x = 0; x < gridWidth; ++x) {
						int spriteId = srcTiles[0].spriteId;
						foreach (var tile in srcTiles) {
							if ((x % patternW) == (tile.x - patternX1) &&
							    (y % patternH) == (tile.y - patternY1))
							{
								spriteId = tile.spriteId;
								break;
							}
						}
						if (rot90)
							tk2dRuntime.TileMap.BuilderUtil.SetRawTileFlag(ref spriteId, tk2dTileFlags.Rot90, true);
						if (flipH)
							tk2dRuntime.TileMap.BuilderUtil.InvertRawTileFlag(ref spriteId, tk2dTileFlags.FlipX);
						if (flipV)
							tk2dRuntime.TileMap.BuilderUtil.InvertRawTileFlag(ref spriteId, tk2dTileFlags.FlipY);
						workBrush.tiles[idx++] = new tk2dSparseTile(
							rectX1 + x + thisRowXOffset, rectY1 + y, editorData.layer, spriteId);
					}
				}
			}
		}
		if (tk2dTileMapToolbar.mainMode == tk2dTileMapToolbar.MainMode.BrushRandom) {
			int gridWidth = 1 + rectX2 - rectX1;
			int gridHeight = 1 + rectY2 - rectY1;
			workBrush.tiles = new tk2dSparseTile[gridWidth * gridHeight];

			var rng = new System.Random(randomSeed + cursorY * tileMap.width + cursorX);

			int idx = 0;
			for (int y = 0; y < gridHeight; ++y) {
				int thisRowXOffset = ((y & 1) == 1) ? xoffset : 0;
				for (int x = 0; x < gridWidth; ++x) {
					int spriteId = srcTiles[rng.Next(srcTiles.Length)].spriteId;
					workBrush.tiles[idx++] = new tk2dSparseTile(
						rectX1 + x + thisRowXOffset, rectY1 + y, editorData.layer, spriteId);
				}
			}
		}

		if (scratchpadGUI.workingHere) {
			int scratchW, scratchH;
			scratchpadGUI.GetScratchpadSize(out scratchW, out scratchH);
			workBrush.ClipTiles(0, 0, scratchW - 1, scratchH - 1);
		} else {
			workBrush.ClipTiles(0, 0, tileMap.width - 1, tileMap.height - 1);
		}

		workBrush.SortTiles(tileMapData.sortMethod == tk2dTileMapData.SortMethod.BottomLeft || tileMapData.sortMethod == tk2dTileMapData.SortMethod.TopLeft,
			tileMapData.sortMethod == tk2dTileMapData.SortMethod.BottomLeft || tileMapData.sortMethod == tk2dTileMapData.SortMethod.BottomRight);

		workBrush.UpdateBrushHash();
	}
	
	void PickUpBrush(tk2dTileMapEditorBrush brush, bool allLayers)
	{
		bool pickFromScratchpad = (scratchpadGUI.workingHere || scratchpadGUI.requestSelectAllTiles);

		int x0 = Mathf.Min(cursorX, cursorX0);
		int x1 = Mathf.Max(cursorX, cursorX0);
		int y0 = Mathf.Min(cursorY, cursorY0);
		int y1 = Mathf.Max(cursorY, cursorY0);
		if (pickFromScratchpad) {
			// Clamp to scratchpad
			int scratchW, scratchH;
			scratchpadGUI.GetScratchpadSize(out scratchW, out scratchH);
			x0 = Mathf.Clamp(x0, 0, scratchW - 1);
			y0 = Mathf.Clamp(y0, 0, scratchH - 1);
			x1 = Mathf.Clamp(x1, 0, scratchW - 1);
			y1 = Mathf.Clamp(y1, 0, scratchH - 1);
		} else {
			// Clamp to tilemap
			x0 = Mathf.Clamp(x0, 0, tileMap.width - 1);
			y0 = Mathf.Clamp(y0, 0, tileMap.height - 1);
			x1 = Mathf.Clamp(x1, 0, tileMap.width - 1);
			y1 = Mathf.Clamp(y1, 0, tileMap.height - 1);
		}
		int numTilesX = x1 - x0 + 1;
		int numTilesY = y1 - y0 + 1;
		
		List<tk2dSparseTile> sparseTile = new List<tk2dSparseTile>();
		List<int> tiles = new List<int>();
		
		int numLayers = tileMap.data.NumLayers;
		int startLayer = 0;
		int endLayer = numLayers;
		
		if (allLayers)
		{
			brush.multiLayer = true;
		}
		else
		{
			brush.multiLayer = false;
			startLayer = editorData.layer;
			endLayer = startLayer + 1;
		}

		// Scratchpad only allows one layer for now
		if (pickFromScratchpad) {
			startLayer = 0;
			endLayer = 1;
		}
		
		if (tileMap.data.tileType == tk2dTileMapData.TileType.Rectangular)
		{
			for (int layer = startLayer; layer < endLayer; ++layer)
			{
				for (int y = numTilesY - 1; y >= 0; --y)
				{
					for (int x = 0; x < numTilesX; ++x)
					{
						int tile;
						if (pickFromScratchpad) {
							tile = scratchpadGUI.GetTile(x0 + x, y0 + y, layer);
						} else {
							tile = tileMap.Layers[layer].GetRawTile(x0 + x, y0 + y);
						}
						tiles.Add(tile);
						sparseTile.Add(new tk2dSparseTile(x, y, allLayers?layer:0, tile));
					}
				}
			}
		}
		else if (tileMap.data.tileType == tk2dTileMapData.TileType.Isometric)
		{
			int xOffset = 0;
			int yOffset = 0;
			if ((y0 & 1) != 0)
				yOffset -= 1;
			
			for (int layer = startLayer; layer < endLayer; ++layer)
			{
				for (int y = numTilesY - 1; y >= 0; --y)
				{
					for (int x = 0; x < numTilesX; ++x)
					{
						int tile;
						if (pickFromScratchpad) {
							tile = scratchpadGUI.GetTile(x0 + x, y0 + y, layer);
						} else {
							tile = tileMap.Layers[layer].GetRawTile(x0 + x, y0 + y);
						}
						tiles.Add(tile);
						sparseTile.Add(new tk2dSparseTile(x + xOffset, y + yOffset, allLayers?layer:0, tile));
					}
				}
			}
		}
		
		
		brush.type = tk2dTileMapEditorBrush.Type.Custom;
		
		if (numTilesX == 1 && numTilesY == 3) brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.Vertical;
		else if (numTilesX == 3 && numTilesY == 1) brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.Horizontal;
		else if (numTilesX == 3 && numTilesY == 3) brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.Square;
		else brush.edgeMode = tk2dTileMapEditorBrush.EdgeMode.None;
		
		brush.tiles = sparseTile.ToArray();
		brush.multiSelectTiles = tiles.ToArray();
		brush.UpdateBrushHash();
		
		// Make the inspector update
		EditorUtility.SetDirty(tileMap);
	}

	void PaintColorBrush(float x, float y) {
		float maskR = 1.0f;
		float maskG = 1.0f;
		float maskB = 1.0f;
		Color src = tk2dTileMapToolbar.colorBrushColor;
		switch (tk2dTileMapToolbar.colorChannelsMode) {
		case tk2dTileMapToolbar.ColorChannelsMode.Red:
			maskG = maskB = 0.0f;
			src.r = 1.0f;
			break;
		case tk2dTileMapToolbar.ColorChannelsMode.Green:
			maskR = maskB = 0.0f;
			src.g = 1.0f;
			break;
		case tk2dTileMapToolbar.ColorChannelsMode.Blue:
			maskR = maskG = 0.0f;
			src.b = 1.0f;
			break;
		}

		var colorGrid = tileMap.ColorChannel;

		float r = tk2dTileMapToolbar.colorBrushRadius;
		int x1 = Mathf.Max((int)Mathf.Floor(x - r), 0);
		int y1 = Mathf.Max((int)Mathf.Floor(y - r), 0);
		int x2 = Mathf.Min((int)Mathf.Ceil(x + r), tileMap.width);
		int y2 = Mathf.Min((int)Mathf.Ceil(y + r), tileMap.height);
		for (int py = y1; py <= y2; ++py) {
			for (int px = x1; px <= x2; ++px) {
				float dx = x - (float)px;
				float dy = y - (float)py;
				float a = 1.0f - Mathf.Sqrt(dx * dx + dy * dy) / r;
				if (a > 0.0f) {
					float alpha = tk2dTileMapToolbar.colorBrushCurve.Evaluate(a);
					alpha *= tk2dTileMapToolbar.colorBrushIntensity;

					float srcFactor = 0.0f;
					float dstFactor = 0.0f;
					switch (tk2dTileMapToolbar.colorBlendMode) {
					case tk2dTileMapToolbar.ColorBlendMode.Replace:
						srcFactor = alpha;
						dstFactor = 1.0f - alpha;
						break;
					case tk2dTileMapToolbar.ColorBlendMode.Addition:
						srcFactor = alpha;
						dstFactor = 1.0f;
						break;
					case tk2dTileMapToolbar.ColorBlendMode.Subtraction:
						srcFactor = -alpha;
						dstFactor = 1.0f;
						break;
					}

					Color dst = colorGrid.GetColor(px, py);
					float resultR = maskR * (src.r * srcFactor + dst.r * dstFactor) + (1.0f - maskR) * dst.r;
					float resultG = maskG * (src.g * srcFactor + dst.g * dstFactor) + (1.0f - maskG) * dst.g;
					float resultB = maskB * (src.b * srcFactor + dst.b * dstFactor) + (1.0f - maskB) * dst.b;
					colorGrid.SetColor(px, py, new Color(resultR, resultG, resultB));
				}
			}
		}
	}
	
	void PickUpColor()
	{
		vertexCursorX = Mathf.Clamp(vertexCursorX, 0, tileMap.width - 1);
		vertexCursorY = Mathf.Clamp(vertexCursorY, 0, tileMap.height - 1);
		tk2dTileMapToolbar.colorBrushColor = tileMap.ColorChannel.GetColor(vertexCursorX, vertexCursorY);
	}
}
