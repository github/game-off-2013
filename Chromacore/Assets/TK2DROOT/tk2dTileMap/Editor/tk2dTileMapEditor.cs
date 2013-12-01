using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public interface ITileMapEditorHost
{
	void BuildIncremental();
	void Build(bool force);
}

[CustomEditor(typeof(tk2dTileMap))]
public class tk2dTileMapEditor : Editor, ITileMapEditorHost
{
	tk2dTileMap tileMap { get { return (tk2dTileMap)target; } }
	tk2dTileMapEditorData editorData;

	tk2dTileMapSceneGUI sceneGUI;
	tk2dEditor.BrushRenderer _brushRenderer;
	tk2dEditor.BrushRenderer brushRenderer
	{
		get {
			if (_brushRenderer == null) _brushRenderer = new tk2dEditor.BrushRenderer(tileMap);
			return _brushRenderer;
		}
		set {
			if (value != null) { Debug.LogError("Only alloyed to set to null"); return; }
			if (_brushRenderer != null)
			{
				_brushRenderer.Destroy();
				_brushRenderer = null;
			}
		}
	}
	tk2dEditor.BrushBuilder _guiBrushBuilder;
	tk2dEditor.BrushBuilder guiBrushBuilder
	{
		get {
			if (_guiBrushBuilder == null) _guiBrushBuilder = new tk2dEditor.BrushBuilder();
			return _guiBrushBuilder;
		}
		set {
			if (value != null) { Debug.LogError("Only allowed to set to null"); return; }
			if (_guiBrushBuilder != null)
			{
				_guiBrushBuilder = null;
			}
		}
	}

	int width, height;
	int partitionSizeX, partitionSizeY;

	// Sprite collection accessor, cleanup when changed
	tk2dSpriteCollectionData _spriteCollection = null;
	tk2dSpriteCollectionData SpriteCollection
	{
		get
		{
			if (_spriteCollection != tileMap.SpriteCollectionInst)
			{
				_spriteCollection = tileMap.SpriteCollectionInst;
			}
			
			return _spriteCollection;
		}
	}
	
	
	void OnEnable()
	{
		if (Application.isPlaying || !tileMap.AllowEdit)
			return;
		
		LoadTileMapData();
	}

	void OnDestroy() {
		tk2dGrid.Done();
		tk2dEditorSkin.Done();
		tk2dPreferences.inst.Save();
		tk2dSpriteThumbnailCache.Done();
	}
	
	void InitEditor()
	{
		// Initialize editor
		LoadTileMapData();
	}
	
	void OnDisable()
	{
		brushRenderer = null;
		guiBrushBuilder = null;
		
		if (sceneGUI != null)
		{
			sceneGUI.Destroy();
			sceneGUI = null;
		}
		
		if (editorData)
		{
			EditorUtility.SetDirty(editorData);
		}
		
		if (tileMap && tileMap.data)
		{
			EditorUtility.SetDirty(tileMap.data);
		}
	}
	
	void LoadTileMapData()
	{
		width = tileMap.width;
		height = tileMap.height;
		partitionSizeX = tileMap.partitionSizeX;
		partitionSizeY = tileMap.partitionSizeY;
	
		GetEditorData();

		if (tileMap.data && editorData && tileMap.Editor__SpriteCollection != null)
		{
			// Rebuild the palette
			editorData.CreateDefaultPalette(tileMap.SpriteCollectionInst, editorData.paletteBrush, editorData.paletteTilesPerRow);
		}
		
		// Rebuild the render utility
		if (sceneGUI != null)
		{
			sceneGUI.Destroy();
		}
		sceneGUI = new tk2dTileMapSceneGUI(this, tileMap, editorData);
		
		// Rebuild the brush renderer
		brushRenderer = null;
	}
	
	public void Build(bool force, bool incremental)
	{
		if (force)
		{
			//if (buildKey != tileMap.buildKey)
				//tk2dEditor.TileMap.TileMapUtility.CleanRenderData(tileMap);
			
			tk2dTileMap.BuildFlags buildFlags = tk2dTileMap.BuildFlags.EditMode;
			if (!incremental) buildFlags |= tk2dTileMap.BuildFlags.ForceBuild;
			tileMap.Build(buildFlags);
		}
	}
	
	public void Build(bool force) { Build(force, false); }
	public void BuildIncremental() { Build(true, true); }
	
	bool Ready
	{
		get
		{
			return (tileMap != null && tileMap.data != null && editorData != null & tileMap.Editor__SpriteCollection != null && tileMap.SpriteCollectionInst != null);
		}
	}
	
	void HighlightTile(Rect rect, Rect tileSize, int tilesPerRow, int x, int y, Color fillColor, Color outlineColor)
	{
		Rect highlightRect = new Rect(rect.x + x * tileSize.width, 
									  rect.y + y * tileSize.height, 
									  tileSize.width, 
									  tileSize.height);
		Vector3[] rectVerts = { new Vector3(highlightRect.x, highlightRect.y, 0), 
								new Vector3(highlightRect.x + highlightRect.width, highlightRect.y, 0), 
								new Vector3(highlightRect.x + highlightRect.width, highlightRect.y + highlightRect.height, 0), 
								new Vector3(highlightRect.x, highlightRect.y + highlightRect.height, 0) };
		Handles.DrawSolidRectangleWithOutline(rectVerts, fillColor, outlineColor);
	}

	Vector2 tiledataScrollPos = Vector2.zero;
	
	int selectedDataTile = -1;
	void DrawTileDataSetupPanel()
	{
		// Sanitize prefabs
		if (tileMap.data.tilePrefabs == null)
			tileMap.data.tilePrefabs = new Object[0];
		
		if (tileMap.data.tilePrefabs.Length != SpriteCollection.Count)
		{
			System.Array.Resize(ref tileMap.data.tilePrefabs, SpriteCollection.Count);
		}

		Rect innerRect = brushRenderer.GetBrushViewRect(editorData.paletteBrush, editorData.paletteTilesPerRow);
		tiledataScrollPos = BeginHScrollView(tiledataScrollPos, GUILayout.MinHeight(innerRect.height * editorData.brushDisplayScale + 32.0f));
		innerRect.width *= editorData.brushDisplayScale;
		innerRect.height *= editorData.brushDisplayScale;
		tk2dGrid.Draw(innerRect);

		Rect rect = brushRenderer.DrawBrush(tileMap, editorData.paletteBrush, editorData.brushDisplayScale, true, editorData.paletteTilesPerRow);
		float displayScale = brushRenderer.LastScale;
		Rect tileSize = new Rect(0, 0, brushRenderer.TileSizePixels.width * displayScale, brushRenderer.TileSizePixels.height * displayScale);
		int tilesPerRow = editorData.paletteTilesPerRow;
		int newSelectedPrefab = selectedDataTile;
		
		if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition))
		{
			Vector2 localClickPosition = Event.current.mousePosition - new Vector2(rect.x, rect.y);
			Vector2 tileLocalPosition = new Vector2(localClickPosition.x / tileSize.width, localClickPosition.y / tileSize.height);
			int tx = (int)tileLocalPosition.x;
			int ty = (int)tileLocalPosition.y;
			newSelectedPrefab = ty * tilesPerRow + tx;
		}
		
		if (Event.current.type == EventType.Repaint)
		{
			for (int tileId = 0; tileId < SpriteCollection.Count; ++tileId)
			{
				Color noDataFillColor = new Color(0, 0, 0, 0.2f);
				Color noDataOutlineColor = Color.clear;
				Color selectedFillColor = new Color(1,0,0,0.05f);
				Color selectedOutlineColor = Color.red;
				
				if (tileMap.data.tilePrefabs[tileId] == null || tileId == selectedDataTile)
				{
					Color fillColor = (selectedDataTile == tileId)?selectedFillColor:noDataFillColor;
					Color outlineColor = (selectedDataTile == tileId)?selectedOutlineColor:noDataOutlineColor;
					HighlightTile(rect, tileSize, editorData.paletteTilesPerRow, tileId % tilesPerRow, tileId / tilesPerRow, fillColor, outlineColor);
				}
			}
		}
		EndHScrollView();
		
		if (selectedDataTile >= 0 && selectedDataTile < tileMap.data.tilePrefabs.Length)
		{
			tileMap.data.tilePrefabs[selectedDataTile] = EditorGUILayout.ObjectField("Prefab", tileMap.data.tilePrefabs[selectedDataTile], typeof(Object), false);
		}
		
		// Add all additional tilemap data
		var allTileInfos = tileMap.data.GetOrCreateTileInfo(SpriteCollection.Count);
		if (selectedDataTile >= 0 && selectedDataTile < allTileInfos.Length)
		{
			var tileInfo = allTileInfos[selectedDataTile];
			GUILayout.Space(16.0f);
			tileInfo.stringVal = (tileInfo.stringVal==null)?"":tileInfo.stringVal;
			tileInfo.stringVal = EditorGUILayout.TextField("String", tileInfo.stringVal);
			tileInfo.intVal = EditorGUILayout.IntField("Int", tileInfo.intVal);
			tileInfo.floatVal = EditorGUILayout.FloatField("Float", tileInfo.floatVal);
			tileInfo.enablePrefabOffset = EditorGUILayout.Toggle("Enable Prefab Offset", tileInfo.enablePrefabOffset);
		}

		if (newSelectedPrefab != selectedDataTile)		
		{
			selectedDataTile = newSelectedPrefab;
			Repaint();
		}
	}
	
	void DrawLayersPanel(bool allowEditing)
	{
		GUILayout.BeginVertical();
		
		// constrain selected layer
		editorData.layer = Mathf.Clamp(editorData.layer, 0, tileMap.data.NumLayers - 1);
		
		if (allowEditing)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			tileMap.data.layersFixedZ = GUILayout.Toggle(tileMap.data.layersFixedZ, "Fixed Z", EditorStyles.miniButton, GUILayout.ExpandWidth(false));
			if (GUILayout.Button("Add Layer", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
			{
				editorData.layer = tk2dEditor.TileMap.TileMapUtility.AddNewLayer(tileMap);
			}
			GUILayout.EndHorizontal();
		}

		string zValueLabel = tileMap.data.layersFixedZ ? "Z Value" : "Z Offset";
		int numLayers = tileMap.data.NumLayers;
		int deleteLayer = -1;
		int moveUp = -1;
		int moveDown = -1;
		for (int layer = numLayers - 1; layer >= 0; --layer)
		{
			GUILayout.Space(4.0f);
			if (allowEditing) {
				GUILayout.BeginVertical(tk2dEditorSkin.SC_InspectorHeaderBG);
				
				GUILayout.BeginHorizontal();
				if (editorData.layer == layer) {
					string newName = GUILayout.TextField(tileMap.data.Layers[layer].name, EditorStyles.textField, GUILayout.MinWidth(120), GUILayout.ExpandWidth(true));
					tileMap.data.Layers[layer].name = newName;
				} else {
					if (GUILayout.Button(tileMap.data.Layers[layer].name, EditorStyles.textField, GUILayout.MinWidth(120), GUILayout.ExpandWidth(true))) {
						editorData.layer = layer;
						Repaint();
					}
				}

				GUI.enabled = (layer != 0);
				if (GUILayout.Button("", tk2dEditorSkin.SimpleButton("btn_down")))
				{
					moveUp = layer;
					Repaint();
				}
				
				GUI.enabled = (layer != numLayers - 1);
				if (GUILayout.Button("", tk2dEditorSkin.SimpleButton("btn_up")))
				{
					moveDown = layer;
					Repaint();
				}

				GUI.enabled = numLayers > 1;
				if (GUILayout.Button("", tk2dEditorSkin.GetStyle("TilemapDeleteItem")))
				{
					deleteLayer = layer;
					Repaint();
				}

				GUI.enabled = true;
				GUILayout.EndHorizontal();
				
				
				// Row 2
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				tk2dGuiUtility.BeginChangeCheck();
				tileMap.data.Layers[layer].skipMeshGeneration = !GUILayout.Toggle(!tileMap.data.Layers[layer].skipMeshGeneration, "Render Mesh", EditorStyles.miniButton, GUILayout.ExpandWidth(false));
				tileMap.data.Layers[layer].useColor = GUILayout.Toggle(tileMap.data.Layers[layer].useColor, "Color", EditorStyles.miniButton, GUILayout.ExpandWidth(false));				
				tileMap.data.Layers[layer].generateCollider = GUILayout.Toggle(tileMap.data.Layers[layer].generateCollider, "Collider", EditorStyles.miniButton, GUILayout.ExpandWidth(false));

				if (tk2dGuiUtility.EndChangeCheck())
					Build(true);
				
				GUILayout.EndHorizontal();

				// Row 3
				tk2dGuiUtility.BeginChangeCheck();

				if (layer == 0 && !tileMap.data.layersFixedZ) {
					GUI.enabled = false;
					EditorGUILayout.FloatField(zValueLabel, 0.0f);
					GUI.enabled = true;
				}
				else {
					tileMap.data.Layers[layer].z = EditorGUILayout.FloatField(zValueLabel, tileMap.data.Layers[layer].z);
				}

				if (!tileMap.data.layersFixedZ)
					tileMap.data.Layers[layer].z = Mathf.Max(0, tileMap.data.Layers[layer].z);
				
				tileMap.data.Layers[layer].unityLayer = EditorGUILayout.LayerField("Layer", tileMap.data.Layers[layer].unityLayer);
				
				tileMap.data.Layers[layer].physicMaterial = (PhysicMaterial)EditorGUILayout.ObjectField("Physic Material", tileMap.data.Layers[layer].physicMaterial, typeof(PhysicMaterial), false);

				if (tk2dGuiUtility.EndChangeCheck())
					Build(true);

				GUILayout.EndVertical();
			} else {
				GUILayout.BeginHorizontal(tk2dEditorSkin.SC_InspectorHeaderBG);

				bool layerSelVal = editorData.layer == layer;
				bool newLayerSelVal = GUILayout.Toggle(layerSelVal, tileMap.data.Layers[layer].name,  EditorStyles.toggle, GUILayout.ExpandWidth(true));
				if (newLayerSelVal != layerSelVal)
				{
					editorData.layer = layer;
					Repaint();
				}
				GUILayout.FlexibleSpace();
				
				var layerGameObject = tileMap.Layers[layer].gameObject;
				if (layerGameObject)
				{
					bool b = GUILayout.Toggle(tk2dEditorUtility.IsGameObjectActive(layerGameObject), "", tk2dEditorSkin.SimpleCheckbox("icon_eye_inactive", "icon_eye"));
					if (b != tk2dEditorUtility.IsGameObjectActive(layerGameObject))
						tk2dEditorUtility.SetGameObjectActive(layerGameObject, b);
				}

				GUILayout.EndHorizontal();
			}
		}
		
		if (deleteLayer != -1)
		{
			//Undo.RegisterUndo(new Object[] { tileMap, tileMap.data }, "Deleted layer");
			tk2dEditor.TileMap.TileMapUtility.DeleteLayer(tileMap, deleteLayer);
		}
		
		if (moveUp != -1)
		{
			//Undo.RegisterUndo(new Object[] { tileMap, tileMap.data }, "Moved layer");
			tk2dEditor.TileMap.TileMapUtility.MoveLayer(tileMap, moveUp, -1);
		}
		
		if (moveDown != -1)
		{
			//Undo.RegisterUndo(new Object[] { tileMap, tileMap.data }, "Moved layer");
			tk2dEditor.TileMap.TileMapUtility.MoveLayer(tileMap, moveDown, 1);
		}
		
		GUILayout.EndVertical();
	}
	
	bool Foldout(ref tk2dTileMapEditorData.SetupMode val, tk2dTileMapEditorData.SetupMode ident, string name)
	{
		bool selected = false;
		if ((val & ident) != 0)
			selected = true;
		
		//GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
		bool newSelected = GUILayout.Toggle(selected, name, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(true));
		if (newSelected != selected)
		{
			if (selected == false)
				val = ident;
			else
				val = 0;
		}
		return newSelected;
	}

	int tilePropertiesPreviewIdx = 0;
	Vector2 paletteSettingsScrollPos = Vector2.zero;
	
	void DrawSettingsPanel()
	{
		GUILayout.Space(8);

		// Sprite collection
		GUILayout.BeginHorizontal();
		tk2dSpriteCollectionData newSpriteCollection = tk2dSpriteGuiUtility.SpriteCollectionList("Sprite Collection", tileMap.Editor__SpriteCollection);
		if (newSpriteCollection != tileMap.Editor__SpriteCollection) {
			Undo.RegisterSceneUndo("Set TileMap Sprite Collection");

			tileMap.Editor__SpriteCollection = newSpriteCollection;
			newSpriteCollection.InitMaterialIds();
			LoadTileMapData();
			
			EditorUtility.SetDirty(tileMap);
			
			if (Ready)
			{
				Init(tileMap.data);
				tileMap.ForceBuild();
			}
		}
		if (tileMap.Editor__SpriteCollection != null && GUILayout.Button(">", EditorStyles.miniButton, GUILayout.Width(19))) {
			tk2dSpriteCollectionEditorPopup v = EditorWindow.GetWindow( typeof(tk2dSpriteCollectionEditorPopup), false, "Sprite Collection Editor" ) as tk2dSpriteCollectionEditorPopup;
			string assetPath = AssetDatabase.GUIDToAssetPath(tileMap.Editor__SpriteCollection.spriteCollectionGUID);
			var spriteCollection = AssetDatabase.LoadAssetAtPath(assetPath, typeof(tk2dSpriteCollection)) as tk2dSpriteCollection;
			v.SetGeneratorAndSelectedSprite(spriteCollection, tileMap.Editor__SpriteCollection.FirstValidDefinitionIndex);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(8);

		// Tilemap data
		tk2dTileMapData newData = (tk2dTileMapData)EditorGUILayout.ObjectField("Tile Map Data", tileMap.data, typeof(tk2dTileMapData), false);
		if (newData != tileMap.data)
		{
			Undo.RegisterSceneUndo("Assign TileMap Data");
			tileMap.data = newData;
			LoadTileMapData();
		}
		if (tileMap.data == null)
		{
			if (tk2dGuiUtility.InfoBoxWithButtons(
				"TileMap needs an data object to proceed. " +
				"Please create one or drag an existing data object into the inspector slot.\n",
				tk2dGuiUtility.WarningLevel.Info, 
				"Create") != -1)
			{
				string assetPath = EditorUtility.SaveFilePanelInProject("Save Tile Map Data", "tileMapData", "asset", "");
				if (assetPath.Length > 0)
				{
					Undo.RegisterSceneUndo("Create TileMap Data");
					tk2dTileMapData tileMapData = ScriptableObject.CreateInstance<tk2dTileMapData>();
					AssetDatabase.CreateAsset(tileMapData, assetPath);
					tileMap.data = tileMapData;
					EditorUtility.SetDirty(tileMap);
					
					Init(tileMapData);
					LoadTileMapData();
				}
			}
		}
		
		// Editor data
		tk2dTileMapEditorData newEditorData = (tk2dTileMapEditorData)EditorGUILayout.ObjectField("Editor Data", editorData, typeof(tk2dTileMapEditorData), false);
		if (newEditorData != editorData)
		{
			Undo.RegisterSceneUndo("Assign TileMap Editor Data");
			string assetPath = AssetDatabase.GetAssetPath(newEditorData);
			if (assetPath.Length > 0)
			{
				tileMap.editorDataGUID = AssetDatabase.AssetPathToGUID(assetPath);
				EditorUtility.SetDirty(tileMap);
				LoadTileMapData();
			}
		}
		if (editorData == null)
		{
			if (tk2dGuiUtility.InfoBoxWithButtons(
				"TileMap needs an editor data object to proceed. " +
				"Please create one or drag an existing data object into the inspector slot.\n",
				tk2dGuiUtility.WarningLevel.Info, 
				"Create") != -1)
			{
				string assetPath = EditorUtility.SaveFilePanelInProject("Save Tile Map Editor Data", "tileMapEditorData", "asset", "");
				if (assetPath.Length > 0)
				{
					Undo.RegisterSceneUndo("Create TileMap Editor Data");
					tk2dTileMapEditorData tileMapEditorData = ScriptableObject.CreateInstance<tk2dTileMapEditorData>();
					AssetDatabase.CreateAsset(tileMapEditorData, assetPath);
					tileMap.editorDataGUID = AssetDatabase.AssetPathToGUID(assetPath);
					EditorUtility.SetDirty(tileMap);
					LoadTileMapData();
				}
			}
		}
		
		// If not set up, don't bother drawing anything else
		if (!Ready)
			return;
		
		// this is intentionally read only
		GUILayout.Space(8);
		GUILayout.BeginHorizontal();
		GUI.enabled = false;
		EditorGUILayout.ObjectField("Render Data", tileMap.renderData, typeof(GameObject), false);
		GUI.enabled = true;
		if (tileMap.renderData != null && GUILayout.Button("Unlink", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
			tk2dEditor.TileMap.TileMapUtility.MakeUnique(tileMap);
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(8);
		
		// tile map size
		
		if (Foldout(ref editorData.setupMode, tk2dTileMapEditorData.SetupMode.Dimensions, "Dimensions"))
		{
			EditorGUI.indentLevel++;
			
			width = Mathf.Clamp(EditorGUILayout.IntField("Width", width), 1, tk2dEditor.TileMap.TileMapUtility.MaxWidth);
			height = Mathf.Clamp(EditorGUILayout.IntField("Height", height), 1, tk2dEditor.TileMap.TileMapUtility.MaxHeight);
			partitionSizeX = Mathf.Clamp(EditorGUILayout.IntField("PartitionSizeX", partitionSizeX), 4, 32);
			partitionSizeY = Mathf.Clamp(EditorGUILayout.IntField("PartitionSizeY", partitionSizeY), 4, 32);
			
			// Create a default tilemap with given dimensions
			if (!tileMap.AreSpritesInitialized())
			{
				tk2dRuntime.TileMap.BuilderUtil.InitDataStore(tileMap);
				tk2dEditor.TileMap.TileMapUtility.ResizeTileMap(tileMap, width, height, tileMap.partitionSizeX, tileMap.partitionSizeY);	
			}
			
			if (width != tileMap.width || height != tileMap.height || partitionSizeX != tileMap.partitionSizeX || partitionSizeY != tileMap.partitionSizeY)
			{
				if ((width < tileMap.width || height < tileMap.height))
				{
					tk2dGuiUtility.InfoBox("The new size of the tile map is smaller than the current size." +
						"Some clipping will occur.", tk2dGuiUtility.WarningLevel.Warning);
				}
				
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Apply", EditorStyles.miniButton))
				{
					tk2dEditor.TileMap.TileMapUtility.ResizeTileMap(tileMap, width, height, partitionSizeX, partitionSizeY);
				}
				GUILayout.EndHorizontal();
			}

			EditorGUI.indentLevel--;
		}
		
		if (Foldout(ref editorData.setupMode, tk2dTileMapEditorData.SetupMode.Layers, "Layers"))
		{
			EditorGUI.indentLevel++;
			
			DrawLayersPanel(true);
			
			EditorGUI.indentLevel--;
		}
		
		// tilemap info
		if (Foldout(ref editorData.setupMode, tk2dTileMapEditorData.SetupMode.Info, "Info"))
		{
			EditorGUI.indentLevel++;
			
			int numActiveChunks = 0;
			if (tileMap.Layers != null)
			{
				foreach (var layer in tileMap.Layers)
					numActiveChunks += layer.NumActiveChunks;
			}
			EditorGUILayout.LabelField("Active chunks", numActiveChunks.ToString());
			int partitionMemSize = partitionSizeX * partitionSizeY * 4;
			EditorGUILayout.LabelField("Memory", ((numActiveChunks * partitionMemSize) / 1024).ToString() + "kB" );
			
			int numActiveColorChunks = 0;
			if (tileMap.ColorChannel != null)
				numActiveColorChunks += tileMap.ColorChannel.NumActiveChunks;
			EditorGUILayout.LabelField("Active color chunks", numActiveColorChunks.ToString());
			int colorMemSize = (partitionSizeX + 1) * (partitionSizeY + 1) * 4;
			EditorGUILayout.LabelField("Memory", ((numActiveColorChunks * colorMemSize) / 1024).ToString() + "kB" );
			
			EditorGUI.indentLevel--;
		}
		
		// tile properties
		if (Foldout(ref editorData.setupMode, tk2dTileMapEditorData.SetupMode.TileProperties, "Tile Properties"))
		{
			EditorGUI.indentLevel++;

			// sort method
			tk2dGuiUtility.BeginChangeCheck();
			tileMap.data.tileType = (tk2dTileMapData.TileType)EditorGUILayout.EnumPopup("Tile Type", tileMap.data.tileType);
			if (tileMap.data.tileType != tk2dTileMapData.TileType.Rectangular) {
				tk2dGuiUtility.InfoBox("Non-rectangular tile types are still in beta testing.", tk2dGuiUtility.WarningLevel.Info);
			}

			tileMap.data.sortMethod = (tk2dTileMapData.SortMethod)EditorGUILayout.EnumPopup("Sort Method", tileMap.data.sortMethod);
			
			if (tk2dGuiUtility.EndChangeCheck())
			{
				tileMap.BeginEditMode();
			}
			

			// reset sizes			
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Reset sizes");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset", EditorStyles.miniButtonRight))
			{
				Init(tileMap.data);
				Build(true);
			}
			GUILayout.EndHorizontal();

			// convert these to pixel units
			Vector3 texelSize = SpriteCollection.spriteDefinitions[0].texelSize;
			Vector3 tileOriginPixels = new Vector3(tileMap.data.tileOrigin.x / texelSize.x, tileMap.data.tileOrigin.y / texelSize.y, tileMap.data.tileOrigin.z);
			Vector3 tileSizePixels = new Vector3(tileMap.data.tileSize.x / texelSize.x, tileMap.data.tileSize.y / texelSize.y, tileMap.data.tileSize.z);
			
			Vector3 newTileOriginPixels = EditorGUILayout.Vector3Field("Origin", tileOriginPixels);
			Vector3 newTileSizePixels = EditorGUILayout.Vector3Field("Size", tileSizePixels);
			
			if (newTileOriginPixels != tileOriginPixels ||
				newTileSizePixels != tileSizePixels)
			{
				tileMap.data.tileOrigin = new Vector3(newTileOriginPixels.x * texelSize.x, newTileOriginPixels.y * texelSize.y, newTileOriginPixels.z);
				tileMap.data.tileSize = new Vector3(newTileSizePixels.x * texelSize.x, newTileSizePixels.y * texelSize.y, newTileSizePixels.z);
				Build(true);
			}

			// preview tile origin and size setting
			Vector2 spritePixelOrigin = Vector2.zero;
			Vector2 spritePixelSize = Vector2.one;
			tk2dSpriteDefinition[] spriteDefs = tileMap.SpriteCollectionInst.spriteDefinitions;
			tk2dSpriteDefinition spriteDef = (tilePropertiesPreviewIdx < spriteDefs.Length) ? spriteDefs[tilePropertiesPreviewIdx] : null;
			if (!spriteDef.Valid) spriteDef = null;
			if (spriteDef != null) {
				spritePixelOrigin = new Vector2(spriteDef.untrimmedBoundsData[0].x / spriteDef.texelSize.x, spriteDef.untrimmedBoundsData[0].y / spriteDef.texelSize.y);
				spritePixelSize = new Vector2(spriteDef.untrimmedBoundsData[1].x / spriteDef.texelSize.x, spriteDef.untrimmedBoundsData[1].y / spriteDef.texelSize.y);
			}
			float zoomFactor = (Screen.width - 32.0f) / (spritePixelSize.x * 2.0f);
			EditorGUILayout.BeginScrollView(Vector2.zero, GUILayout.Height(spritePixelSize.y * 2.0f * zoomFactor + 32.0f));
			Rect innerRect = new Rect(0, 0, spritePixelSize.x * 2.0f * zoomFactor, spritePixelSize.y * 2.0f * zoomFactor);
			tk2dGrid.Draw(innerRect);
			if (spriteDef != null) {
				// Preview tiles
				tk2dSpriteThumbnailCache.DrawSpriteTexture(new Rect(spritePixelSize.x * 0.5f * zoomFactor, spritePixelSize.y * 0.5f * zoomFactor, spritePixelSize.x * zoomFactor, spritePixelSize.y * zoomFactor), spriteDef);
				// Preview cursor
				Vector2 cursorOffset = (spritePixelSize * 0.5f - spritePixelOrigin) * zoomFactor;
				Vector2 cursorSize = new Vector2(tileSizePixels.x * zoomFactor, tileSizePixels.y * zoomFactor);
				cursorOffset.x += tileOriginPixels.x * zoomFactor;
				cursorOffset.y += tileOriginPixels.y * zoomFactor;
				cursorOffset.x += spritePixelSize.x * 0.5f * zoomFactor;
				cursorOffset.y += spritePixelSize.y * 0.5f * zoomFactor;
				float top = spritePixelSize.y * 2.0f * zoomFactor;
				Vector3[] cursorVerts = new Vector3[] {
					new Vector3(cursorOffset.x, top - cursorOffset.y, 0),
					new Vector3(cursorOffset.x + cursorSize.x, top - cursorOffset.y, 0),
					new Vector3(cursorOffset.x + cursorSize.x, top - (cursorOffset.y + cursorSize.y), 0),
					new Vector3(cursorOffset.x, top - (cursorOffset.y + cursorSize.y), 0)
				};
				Handles.DrawSolidRectangleWithOutline(cursorVerts, new Color(1.0f, 1.0f, 1.0f, 0.2f), Color.white);
			}
			if (GUILayout.Button(new GUIContent("", "Click - preview using different tile"), "label", GUILayout.Width(innerRect.width), GUILayout.Height(innerRect.height))) {
				int n = spriteDefs.Length;
				for (int i = 0; i < n; ++i) {
					if (++tilePropertiesPreviewIdx >= n)
						tilePropertiesPreviewIdx = 0;
					if (spriteDefs[tilePropertiesPreviewIdx].Valid)
						break;
				}
			}
			EditorGUILayout.EndScrollView();

			EditorGUI.indentLevel--;
		}
		
		if (Foldout(ref editorData.setupMode, tk2dTileMapEditorData.SetupMode.PaletteProperties, "Palette Properties"))
		{
			EditorGUI.indentLevel++;
			int newTilesPerRow = Mathf.Clamp(EditorGUILayout.IntField("Tiles Per Row", editorData.paletteTilesPerRow),
											1, SpriteCollection.Count);
			if (newTilesPerRow != editorData.paletteTilesPerRow)
			{
				guiBrushBuilder.Reset();
				
				editorData.paletteTilesPerRow = newTilesPerRow;
				editorData.CreateDefaultPalette(tileMap.SpriteCollectionInst, editorData.paletteBrush, editorData.paletteTilesPerRow);
			}
			
			GUILayout.BeginHorizontal();
			editorData.brushDisplayScale = EditorGUILayout.FloatField("Display Scale", editorData.brushDisplayScale);
			editorData.brushDisplayScale = Mathf.Clamp(editorData.brushDisplayScale, 1.0f / 16.0f, 4.0f);
			if (GUILayout.Button("Reset", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
			{
				editorData.brushDisplayScale = 1.0f;
				Repaint();
			}
			GUILayout.EndHorizontal();

			EditorGUILayout.PrefixLabel("Preview");
			Rect innerRect = brushRenderer.GetBrushViewRect(editorData.paletteBrush, editorData.paletteTilesPerRow);
			paletteSettingsScrollPos = BeginHScrollView(paletteSettingsScrollPos, GUILayout.MinHeight(innerRect.height * editorData.brushDisplayScale + 32.0f));
			innerRect.width *= editorData.brushDisplayScale;
			innerRect.height *= editorData.brushDisplayScale;
			tk2dGrid.Draw(innerRect);
			brushRenderer.DrawBrush(tileMap, editorData.paletteBrush, editorData.brushDisplayScale, true, editorData.paletteTilesPerRow);
			EndHScrollView();

			EditorGUI.indentLevel--;
		}

		if (Foldout(ref editorData.setupMode, tk2dTileMapEditorData.SetupMode.Import, "Import"))
		{
			EditorGUI.indentLevel++;
			
			if (GUILayout.Button("Import TMX"))
			{
				if (tk2dEditor.TileMap.Importer.Import(tileMap, tk2dEditor.TileMap.Importer.Format.TMX)) 
				{
					Build(true);	
					width = tileMap.width;
					height = tileMap.height;
					partitionSizeX = tileMap.partitionSizeX;
					partitionSizeY = tileMap.partitionSizeY;
				}
			}
			
			EditorGUI.indentLevel--;
		}
	}

	// Little hack to allow nested scrollviews to behave properly
	Vector2 hScrollDelta = Vector2.zero;
	Vector2 BeginHScrollView(Vector2 pos, params GUILayoutOption[] options) {
		hScrollDelta = Vector2.zero;
		if (Event.current.type == EventType.ScrollWheel) {
			hScrollDelta.y = Event.current.delta.y;
		}
		return EditorGUILayout.BeginScrollView(pos, options);
	}
	void EndHScrollView() {
		EditorGUILayout.EndScrollView();
		if (hScrollDelta != Vector2.zero) {
			Event.current.type = EventType.ScrollWheel;
			Event.current.delta = hScrollDelta;
		}
	}

	void DrawColorPaintPanel()
	{
		if (!tileMap.HasColorChannel())
		{
			if (GUILayout.Button("Create Color Channel"))
			{
				Undo.RegisterUndo(tileMap, "Created Color Channel");
				tileMap.CreateColorChannel();
				tileMap.BeginEditMode();
			}
			
			Repaint();
			return;
		}

		tk2dTileMapToolbar.ColorToolsWindow();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel("Clear to Color");
		if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
		{
			tileMap.ColorChannel.Clear(tk2dTileMapToolbar.colorBrushColor);
			Build(true);
		}
		EditorGUILayout.EndHorizontal();
		
		if (tileMap.HasColorChannel())
		{
			EditorGUILayout.Separator();
			if (GUILayout.Button("Delete Color Channel"))
			{
				Undo.RegisterUndo(tileMap, "Deleted Color Channel");
				
				tileMap.DeleteColorChannel();
				tileMap.BeginEditMode();

				Repaint();
				return;
			}
		}
	}

	int InlineToolbar(string name, int val, string[] names)
	{
		int selectedIndex = val;
		GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
		GUILayout.Label(name, EditorStyles.toolbarButton);
		GUILayout.FlexibleSpace();
		for (int i = 0; i < names.Length; ++i)
		{
			bool selected = (i == selectedIndex);
			bool toggled = GUILayout.Toggle(selected, names[i], EditorStyles.toolbarButton);
			if (toggled == true)
			{
				selectedIndex = i;
			}
		}
		
		GUILayout.EndHorizontal();
		return selectedIndex;
	}
	
	bool showSaveSection = false;
	bool showLoadSection = false;

	void DrawLoadSaveBrushSection(tk2dTileMapEditorBrush activeBrush)
	{
		// Brush load & save handling
		bool startedSave = false;
		bool prevGuiEnabled = GUI.enabled;
		GUILayout.BeginHorizontal();
		if (showLoadSection) GUI.enabled = false;
		if (GUILayout.Button(showSaveSection?"Cancel":"Save"))
		{
			if (showSaveSection == false) startedSave = true;
			showSaveSection = !showSaveSection;
			if (showSaveSection) showLoadSection = false;
			Repaint();
		}
		GUI.enabled = prevGuiEnabled;

		if (showSaveSection) GUI.enabled = false;
		if (GUILayout.Button(showLoadSection?"Cancel":"Load"))
		{
			showLoadSection = !showLoadSection;
			if (showLoadSection) showSaveSection = false;
		}
		GUI.enabled = prevGuiEnabled;
		GUILayout.EndHorizontal();

		if (showSaveSection)
		{
			GUI.SetNextControlName("BrushNameEntry");
			activeBrush.name = EditorGUILayout.TextField("Name", activeBrush.name);
			if (startedSave)
				GUI.FocusControl("BrushNameEntry");

			if (GUILayout.Button("Save"))
			{
				if (activeBrush.name.Length == 0)
				{
					Debug.LogError("Active brush needs a name");
				}
				else
				{
					bool replaced = false;
					for (int i = 0; i < editorData.brushes.Count; ++i)
					{
						if (editorData.brushes[i].name == activeBrush.name)
						{
							editorData.brushes[i] = new tk2dTileMapEditorBrush(activeBrush);
							replaced = true;
						}
					}
					if (!replaced)
						editorData.brushes.Add(new tk2dTileMapEditorBrush(activeBrush));
					showSaveSection = false;
				}
			}
		}

		if (showLoadSection)
		{
			GUILayout.Space(8);

			if (editorData.brushes.Count == 0)
				GUILayout.Label("No saved brushes.");

			GUILayout.BeginVertical();
			int deleteBrushId = -1;
			for (int i = 0; i < editorData.brushes.Count; ++i)
			{
				var v = editorData.brushes[i];
				GUILayout.BeginHorizontal();
				if (GUILayout.Button(v.name, EditorStyles.miniButton))
				{
					showLoadSection = false;
					editorData.activeBrush = new tk2dTileMapEditorBrush(v);
				}
				if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(16)))
				{
					deleteBrushId = i;
				}
				GUILayout.EndHorizontal();
			}
			if (deleteBrushId != -1)
			{
				editorData.brushes.RemoveAt(deleteBrushId);
				Repaint();
			}
			GUILayout.EndVertical();
		}
	}

	Vector2 paletteScrollPos = Vector2.zero;
	Vector2 activeBrushScrollPos = Vector2.zero;

	void DrawPaintPanel()
	{
		var activeBrush = editorData.activeBrush;
		
		if (Ready && (activeBrush == null || activeBrush.Empty))
		{
			editorData.InitBrushes(tileMap.SpriteCollectionInst);
		}
		
		// Draw layer selector
		if (tileMap.data.NumLayers > 1)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			GUILayout.Label("Layers", EditorStyles.toolbarButton);	GUILayout.FlexibleSpace();	
			GUILayout.EndHorizontal();
			DrawLayersPanel(false);
			EditorGUILayout.Space();
			GUILayout.EndVertical();
		}

#if TK2D_TILEMAP_EXPERIMENTAL
		DrawLoadSaveBrushSection(activeBrush);
#endif

		// Draw palette
		if (!showLoadSection && !showSaveSection)
		{
			editorData.showPalette = EditorGUILayout.Foldout(editorData.showPalette, "Palette");
			if (editorData.showPalette)
			{
				// brush name
				string selectionDesc = "";
				if (activeBrush.tiles.Length == 1) {
					int tile = tk2dRuntime.TileMap.BuilderUtil.GetTileFromRawTile(activeBrush.tiles[0].spriteId);
					if (tile >= 0 && tile < SpriteCollection.spriteDefinitions.Length)
						selectionDesc = SpriteCollection.spriteDefinitions[tile].name;
				}
				GUILayout.Label(selectionDesc);
			
				
				Rect innerRect = brushRenderer.GetBrushViewRect(editorData.paletteBrush, editorData.paletteTilesPerRow);
				paletteScrollPos = BeginHScrollView(paletteScrollPos, GUILayout.MinHeight(innerRect.height * editorData.brushDisplayScale + 32.0f));
				innerRect.width *= editorData.brushDisplayScale;
				innerRect.height *= editorData.brushDisplayScale;
				tk2dGrid.Draw(innerRect);

				// palette
				Rect rect = brushRenderer.DrawBrush(tileMap, editorData.paletteBrush, editorData.brushDisplayScale, true, editorData.paletteTilesPerRow);
				float displayScale = brushRenderer.LastScale;
				
				Rect tileSize = new Rect(0, 0, brushRenderer.TileSizePixels.width * displayScale, brushRenderer.TileSizePixels.height * displayScale);
				guiBrushBuilder.HandleGUI(rect, tileSize, editorData.paletteTilesPerRow, tileMap.SpriteCollectionInst, activeBrush);
				EditorGUILayout.Separator();

				EndHScrollView();
			}
			EditorGUILayout.Separator();
		}

		// Draw brush
		if (!showLoadSection)
		{
			editorData.showBrush = EditorGUILayout.Foldout(editorData.showBrush, "Brush");
			if (editorData.showBrush)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Cursor Tile Opacity");
				tk2dTileMapToolbar.workBrushOpacity = EditorGUILayout.Slider(tk2dTileMapToolbar.workBrushOpacity, 0.0f, 1.0f);
				GUILayout.EndHorizontal();

				Rect innerRect = brushRenderer.GetBrushViewRect(editorData.activeBrush, editorData.paletteTilesPerRow);
				activeBrushScrollPos = BeginHScrollView(activeBrushScrollPos, GUILayout.MinHeight(innerRect.height * editorData.brushDisplayScale + 32.0f));
				innerRect.width *= editorData.brushDisplayScale;
				innerRect.height *= editorData.brushDisplayScale;
				tk2dGrid.Draw(innerRect);
				brushRenderer.DrawBrush(tileMap, editorData.activeBrush, editorData.brushDisplayScale, false, editorData.paletteTilesPerRow);
				EndHScrollView();
				EditorGUILayout.Separator();
			}
		}
	}
	
	/// <summary>
	/// Initialize tilemap data to sensible values.
	/// Mainly, tileSize and tileOffset
	/// </summary>
	void Init(tk2dTileMapData tileMapData)
	{
		if (tileMap.SpriteCollectionInst != null) {
			tileMapData.tileSize = tileMap.SpriteCollectionInst.spriteDefinitions[0].untrimmedBoundsData[1];
			tileMapData.tileOrigin = this.tileMap.SpriteCollectionInst.spriteDefinitions[0].untrimmedBoundsData[0] - tileMap.SpriteCollectionInst.spriteDefinitions[0].untrimmedBoundsData[1] * 0.5f;
		}
	}

	void GetEditorData() {
		// Don't guess, load editor data every frame		
		string editorDataPath = AssetDatabase.GUIDToAssetPath(tileMap.editorDataGUID);
		editorData = Resources.LoadAssetAtPath(editorDataPath, typeof(tk2dTileMapEditorData)) as tk2dTileMapEditorData;
	}
	
	public override void OnInspectorGUI()
	{
		if (tk2dEditorUtility.IsPrefab(target))
		{
			tk2dGuiUtility.InfoBox("Editor disabled on prefabs.", tk2dGuiUtility.WarningLevel.Error);
			return;
		}
		
		if (Application.isPlaying)
		{
			tk2dGuiUtility.InfoBox("Editor disabled while game is running.", tk2dGuiUtility.WarningLevel.Error);
			return;
		}

		GetEditorData();

		if (tileMap.data == null || editorData == null || tileMap.Editor__SpriteCollection == null) {
			DrawSettingsPanel();
			return;
		}

		if (tileMap.renderData != null)
		{
			if (tileMap.renderData.transform.position != tileMap.transform.position) {
				tileMap.renderData.transform.position = tileMap.transform.position;
			}
			if (tileMap.renderData.transform.rotation != tileMap.transform.rotation) {
				tileMap.renderData.transform.rotation = tileMap.transform.rotation;
			}
			if (tileMap.renderData.transform.localScale != tileMap.transform.localScale) {
				tileMap.renderData.transform.localScale = tileMap.transform.localScale;
			}
		}
	
		if (!tileMap.AllowEdit)
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Edit"))
			{
				Undo.RegisterSceneUndo("Tilemap Enter Edit Mode");
				tileMap.BeginEditMode();
				InitEditor();
				Repaint();
			}
			if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
			{
				tk2dTileMap[] allTileMaps = Resources.FindObjectsOfTypeAll(typeof(tk2dTileMap)) as tk2dTileMap[];
				foreach (var tm in allTileMaps)
				{
					if (!EditorUtility.IsPersistent(tm) && !tm.AllowEdit)
					{
						tm.BeginEditMode();
						EditorUtility.SetDirty(tm);
					}
				}
				InitEditor();
			}
			GUILayout.EndHorizontal();
			return;
		}
		
		// Commit
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Commit"))
		{
			Undo.RegisterSceneUndo("Tilemap Leave Edit Mode");
			tileMap.EndEditMode();
			Repaint();
		}
		if (GUILayout.Button("All", GUILayout.ExpandWidth(false)))
		{
			tk2dTileMap[] allTileMaps = Resources.FindObjectsOfTypeAll(typeof(tk2dTileMap)) as tk2dTileMap[];
			foreach (var tm in allTileMaps)
			{
				if (!EditorUtility.IsPersistent(tm) && tm.AllowEdit)
				{
					tm.EndEditMode();
					EditorUtility.SetDirty(tm);
				}
			}
		}
		GUILayout.EndHorizontal();
		EditorGUILayout.Separator();

		
		if (tileMap.editorDataGUID.Length > 0 && editorData == null)
		{
			// try to load it in
			LoadTileMapData();
			// failed, so the asset is lost
			if (editorData == null)
			{
				tileMap.editorDataGUID = "";
			}
		}
		
		if (editorData == null || tileMap.data == null || tileMap.Editor__SpriteCollection == null || !tileMap.AreSpritesInitialized())
		{
			DrawSettingsPanel();
		}
		else
		{
			// In case things have changed
			if (tk2dRuntime.TileMap.BuilderUtil.InitDataStore(tileMap))
				Build(true);
			
			string[] toolBarButtonNames = System.Enum.GetNames(typeof(tk2dTileMapEditorData.EditMode));
			
			tk2dTileMapEditorData.EditMode newEditMode = (tk2dTileMapEditorData.EditMode)GUILayout.Toolbar((int)editorData.editMode, toolBarButtonNames );
			if (newEditMode != editorData.editMode) {
				// Force updating the scene view when mode changes
				EditorUtility.SetDirty(target);
				editorData.editMode = newEditMode;
			}
			switch (editorData.editMode)
			{
			case tk2dTileMapEditorData.EditMode.Paint: DrawPaintPanel(); break;
			case tk2dTileMapEditorData.EditMode.Color: DrawColorPaintPanel(); break;
			case tk2dTileMapEditorData.EditMode.Settings: DrawSettingsPanel(); break;
			case tk2dTileMapEditorData.EditMode.Data: DrawTileDataSetupPanel(); break;
			}
		}
	}
	
	void OnSceneGUI()
	{
		if (!Ready)
		{
			return;
		}

		if (sceneGUI != null)
		{
			sceneGUI.OnSceneGUI();
		}
		
		if (!Application.isPlaying && tileMap.AllowEdit)
		{
			// build if necessary
			if (tk2dRuntime.TileMap.BuilderUtil.InitDataStore(tileMap))
				Build(true);
			else		
				Build(false);
		}
	}
	
    [MenuItem("GameObject/Create Other/tk2d/TileMap", false, 13850)]
	static void Create()
	{
		tk2dSpriteCollectionData sprColl = null;
		if (sprColl == null)
		{
			// try to inherit from other TileMaps in scene
			tk2dTileMap sceneTileMaps = GameObject.FindObjectOfType(typeof(tk2dTileMap)) as tk2dTileMap;
			if (sceneTileMaps)
			{
				sprColl = sceneTileMaps.Editor__SpriteCollection;
			}
		}

		if (sprColl == null)
		{
			tk2dSpriteCollectionIndex[] spriteCollections = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
			foreach (var v in spriteCollections)
			{
				if (v.managedSpriteCollection) continue; // don't wanna pick a managed one
				
				GameObject scgo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(v.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
				var sc = scgo.GetComponent<tk2dSpriteCollectionData>();
				if (sc != null && sc.spriteDefinitions != null && sc.spriteDefinitions.Length > 0 && sc.allowMultipleAtlases == false)
				{
					sprColl = sc;
					break;
				}
			}

			if (sprColl == null)
			{
				EditorUtility.DisplayDialog("Create TileMap", "Unable to create sprite as no SpriteCollections have been found.", "Ok");
				return;
			}
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("TileMap");
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
		tk2dTileMap tileMap = go.AddComponent<tk2dTileMap>();
		tileMap.BeginEditMode();
	
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create TileMap");
	}
}
