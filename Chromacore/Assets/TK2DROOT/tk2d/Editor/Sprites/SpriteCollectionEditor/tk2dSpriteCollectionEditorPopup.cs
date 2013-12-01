using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using tk2dEditor.SpriteCollectionEditor;

namespace tk2dEditor.SpriteCollectionEditor
{
	public interface IEditorHost
	{
		void OnSpriteCollectionChanged(bool retainSelection);
		void OnSpriteCollectionSortChanged();
		
		Texture2D GetTextureForSprite(int spriteId);
		
		SpriteCollectionProxy SpriteCollection { get; }

		int InspectorWidth { get; }
		SpriteView SpriteView { get; }
		void SelectSpritesFromList(int[] indices);
		void SelectSpritesInSpriteSheet(int spriteSheetId, int[] spriteIds);

		void Commit();
	}
	
	public class SpriteCollectionEditorEntry
	{
		public enum Type
		{
			None,
			Sprite,
			SpriteSheet,
			Font,
			
			MaxValue
		}
		
		public string name;
		public int index;
		public Type type;
		public bool selected = false;
		
		// list management
		public int listIndex; // index into the currently active list
		public int selectionKey; // a timestamp of when the entry was selected, to decide the last selected one
	}
}

public class tk2dSpriteCollectionEditorPopup : EditorWindow, IEditorHost
{
	tk2dSpriteCollection _spriteCollection; // internal tmp var
	SpriteView spriteView;
	SettingsView settingsView;
	FontView fontView;
	SpriteSheetView spriteSheetView;
	
	// sprite collection we're editing
	SpriteCollectionProxy spriteCollectionProxy = null;
	public SpriteCollectionProxy SpriteCollection { get { return spriteCollectionProxy; } }
	public SpriteView SpriteView { get { return spriteView; } }

	// This lists all entries
	List<SpriteCollectionEditorEntry> entries = new List<SpriteCollectionEditorEntry>();
	// This lists all selected entries
	List<SpriteCollectionEditorEntry> selectedEntries = new List<SpriteCollectionEditorEntry>();
	
	// Callback when a sprite collection is changed and the selection needs to be refreshed
	public void OnSpriteCollectionChanged(bool retainSelection)
	{
		var oldSelection = selectedEntries.ToArray();
		
		PopulateEntries();
		
		if (retainSelection)
		{
			searchFilter = ""; // name may have changed
			foreach (var selection in oldSelection)
			{
				foreach (var entry in entries)
				{
					if (entry.type == selection.type && entry.index == selection.index)
					{
						entry.selected = true;
						break;
					}
				}
			}
			UpdateSelection();
		}
	}
	
	public void SelectSpritesFromList(int[] indices)
	{
		OnSpriteCollectionChanged(true); // clear filter
		selectedEntries = new List<SpriteCollectionEditorEntry>();
		// Clear selection
		foreach (var entry in entries)
			entry.selected = false;
		// Create new selection
		foreach (var index in indices)
		{
			foreach (var entry in entries)
			{
				if (entry.type == SpriteCollectionEditorEntry.Type.Sprite && entry.index == index)
				{
					entry.selected = true;
					selectedEntries.Add(entry);
					break;				
				}
			}
		}
	}
	
	public void SelectSpritesInSpriteSheet(int spriteSheetId, int[] spriteIds)
	{
		OnSpriteCollectionChanged(true); // clear filter
		selectedEntries = new List<SpriteCollectionEditorEntry>();
		foreach (var entry in entries)
		{
			entry.selected = (entry.type == SpriteCollectionEditorEntry.Type.SpriteSheet && entry.index == spriteSheetId);
			if (entry.selected)
			{
				spriteSheetView.Select(spriteCollectionProxy.spriteSheets[spriteSheetId], spriteIds);
			}
		}
		UpdateSelection();
	}
	
	void UpdateSelection()
	{
		// clear settings view if its selected
		settingsView.show = false;
		
		selectedEntries = (from entry in entries where entry.selected == true orderby entry.selectionKey select entry).ToList();
	}
	
	void ClearSelection()
	{
		entries.ForEach((a) => a.selected = false);
		UpdateSelection();
	}
	
	// Callback when a sprite collection needs resorting
	public static bool Contains(string s, string text)
	{
		return s.ToLower().IndexOf(text.ToLower()) != -1;
	}
	
	// Callback when a sort criteria is changed
	public void OnSpriteCollectionSortChanged()
	{
		if (searchFilter.Length > 0)
		{
			// re-sort list
			entries = (from entry in entries where Contains(entry.name, searchFilter) select entry)
						.OrderBy( e => e.type )
						.ThenBy( e => e.name, new tk2dEditor.Shared.NaturalComparer() )
						.ToList();
		}
		else
		{
			// re-sort list
			entries = (from entry in entries select entry)
						.OrderBy( e => e.type )
						.ThenBy( e => e.name, new tk2dEditor.Shared.NaturalComparer() )
						.ToList();
		}
		for (int i = 0; i < entries.Count; ++i)
			entries[i].listIndex = i;
	}
	
	public int InspectorWidth { get { return tk2dPreferences.inst.spriteCollectionInspectorWidth; } }
	
	// populate the entries struct for display in the listbox
	void PopulateEntries()
	{
		entries = new List<SpriteCollectionEditorEntry>();
		selectedEntries = new List<SpriteCollectionEditorEntry>();
		if (spriteCollectionProxy == null)
			return;

		for (int spriteIndex = 0; spriteIndex < spriteCollectionProxy.textureParams.Count; ++spriteIndex)
		{
			var sprite = spriteCollectionProxy.textureParams[spriteIndex];
			var spriteSourceTexture = sprite.texture;
			if (spriteSourceTexture == null && sprite.name.Length == 0) continue;
			
			var newEntry = new SpriteCollectionEditorEntry();
			newEntry.name = sprite.name;

			if (sprite.texture == null) {
				newEntry.name += " (missing)";
			}

			newEntry.index = spriteIndex;
			newEntry.type = SpriteCollectionEditorEntry.Type.Sprite;
			entries.Add(newEntry);
		}
		
		for (int i = 0; i < spriteCollectionProxy.spriteSheets.Count; ++i)
		{
			var spriteSheet = spriteCollectionProxy.spriteSheets[i];
			if (!spriteSheet.active) continue;
			
			var newEntry = new SpriteCollectionEditorEntry();
			newEntry.name = spriteSheet.Name;
			newEntry.index = i;
			newEntry.type = SpriteCollectionEditorEntry.Type.SpriteSheet;
			entries.Add(newEntry);
		}
		
		for (int i = 0; i < spriteCollectionProxy.fonts.Count; ++i)
		{
			var font = spriteCollectionProxy.fonts[i];
			if (!font.active) continue;
			
			var newEntry = new SpriteCollectionEditorEntry();
			newEntry.name = font.Name;
			newEntry.index = i;
			newEntry.type = SpriteCollectionEditorEntry.Type.Font;
			entries.Add(newEntry);
		}
		
		OnSpriteCollectionSortChanged();
		selectedEntries = new List<SpriteCollectionEditorEntry>();
	}
	
	public void SetGenerator(tk2dSpriteCollection spriteCollection)
	{
		this._spriteCollection = spriteCollection;
		this.firstRun = true;
		spriteCollectionProxy = new SpriteCollectionProxy(spriteCollection);
		PopulateEntries();
	}
	
	public void SetGeneratorAndSelectedSprite(tk2dSpriteCollection spriteCollection, int selectedSprite)
	{
		searchFilter = "";
		SetGenerator(spriteCollection);
		foreach (var entry in entries)
		{
			if (entry.type == SpriteCollectionEditorEntry.Type.Sprite && entry.index == selectedSprite)
			{
				entry.selected = true;
				break;
			}
		}
		UpdateSelection();
	}
	
	int cachedSpriteId = -1;
	Texture2D cachedSpriteTexture = null;
	
	// Returns a texture for a given sprite, if the sprite is a region sprite, a new texture is returned
	public Texture2D GetTextureForSprite(int spriteId)
	{
		var param = spriteCollectionProxy.textureParams[spriteId];
		if (spriteId != cachedSpriteId)
		{
			ClearTextureCache();
			cachedSpriteId = spriteId;
		}
		
		if (param.extractRegion)		
		{
			if (cachedSpriteTexture == null)
			{
				var tex = param.texture;
				cachedSpriteTexture = new Texture2D(param.regionW, param.regionH);
				for (int y = 0; y < param.regionH; ++y)
				{
					for (int x = 0; x < param.regionW; ++x)
					{
						cachedSpriteTexture.SetPixel(x, y, tex.GetPixel(param.regionX + x, param.regionY + y));
					}
				}
				cachedSpriteTexture.Apply();
			}
			
			return cachedSpriteTexture;
		}
		else
		{
			return param.texture;
		}
	}
	
	void ClearTextureCache()
	{
		if (cachedSpriteId != -1)
			cachedSpriteId = -1;
		
		if (cachedSpriteTexture != null)
		{
			DestroyImmediate(cachedSpriteTexture);
			cachedSpriteTexture = null;
		}
	}
	
	void OnEnable()
	{
		if (_spriteCollection != null)
		{
			SetGenerator(_spriteCollection);
		}
		
		spriteView = new SpriteView(this);
		settingsView = new SettingsView(this);
		fontView = new FontView(this);
		spriteSheetView = new SpriteSheetView(this);
	}
	
	void OnDisable()
	{
		ClearTextureCache();
		_spriteCollection = null;
	}

	void OnDestroy() {
		tk2dSpriteThumbnailCache.Done();
		tk2dEditorSkin.Done();
	}
	
	string searchFilter = "";
	void DrawToolbar()
	{
		GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
		
		// LHS
		GUILayout.BeginHorizontal(GUILayout.Width(leftBarWidth - 6));
		
		// Create Button
		GUIContent createButton = new GUIContent("Create");
		Rect createButtonRect = GUILayoutUtility.GetRect(createButton, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
		if (GUI.Button(createButtonRect, createButton, EditorStyles.toolbarDropDown))
		{
			GUIUtility.hotControl = 0;
			GUIContent[] menuItems = new GUIContent[] {
				new GUIContent("Sprite Sheet"),
				new GUIContent("Font")
			};
			
			EditorUtility.DisplayCustomMenu(createButtonRect, menuItems, -1, 
				delegate(object userData, string[] options, int selected) {
					switch (selected)
					{
						case 0:
							int addedSpriteSheetIndex = spriteCollectionProxy.FindOrCreateEmptySpriteSheetSlot();
							searchFilter = "";
							PopulateEntries();
							foreach (var entry in entries)
							{
								if (entry.type == SpriteCollectionEditorEntry.Type.SpriteSheet && entry.index == addedSpriteSheetIndex)
									entry.selected = true;
							}
							UpdateSelection();
							break;
						case 1:
							if (SpriteCollection.allowMultipleAtlases)
							{
								EditorUtility.DisplayDialog("Create Font", 
											"Adding fonts to sprite collections isn't allowed when multi atlas spanning is enabled. " +
											"Please disable it and try again.", "Ok");
							}
							else 
							{
								int addedFontIndex = spriteCollectionProxy.FindOrCreateEmptyFontSlot();
								searchFilter = "";
								PopulateEntries();
								foreach (var entry in entries)
								{
									if (entry.type == SpriteCollectionEditorEntry.Type.Font && entry.index == addedFontIndex)
										entry.selected = true;
								}
								UpdateSelection();
							}
							break;
					}
				}
				, null);
		}
		
		// Filter box
		GUILayout.Space(8);
		string newSearchFilter = GUILayout.TextField(searchFilter, tk2dEditorSkin.ToolbarSearch, GUILayout.ExpandWidth(true));
		if (newSearchFilter != searchFilter)
		{
			searchFilter = newSearchFilter;
			PopulateEntries();
		}
		if (searchFilter.Length > 0)
		{
			if (GUILayout.Button("", tk2dEditorSkin.ToolbarSearchClear, GUILayout.ExpandWidth(false)))
			{
				searchFilter = "";
				PopulateEntries();
			}
		}
		else
		{
			GUILayout.Label("", tk2dEditorSkin.ToolbarSearchRightCap);
		}
		GUILayout.EndHorizontal();
		
		// Label
		if (_spriteCollection != null)
			GUILayout.Label(_spriteCollection.name);
		
		// RHS
		GUILayout.FlexibleSpace();
		
		// Always in settings view when empty
		if (spriteCollectionProxy != null && spriteCollectionProxy.Empty)
		{
			GUILayout.Toggle(true, "Settings", EditorStyles.toolbarButton);
		}
		else
		{
			bool newSettingsView = GUILayout.Toggle(settingsView.show, "Settings", EditorStyles.toolbarButton);
			if (newSettingsView != settingsView.show)
			{
				ClearSelection();
				settingsView.show = newSettingsView;
			}
		}
		
		if (GUILayout.Button("Revert", EditorStyles.toolbarButton) && spriteCollectionProxy != null)
		{
			spriteCollectionProxy.CopyFromSource();
			OnSpriteCollectionChanged(false);
		}
		
		if (GUILayout.Button("Commit", EditorStyles.toolbarButton) && spriteCollectionProxy != null)
			Commit();
		
		GUILayout.EndHorizontal();
	}

	public void Commit()
	{
		spriteCollectionProxy.DeleteUnusedData();
		spriteCollectionProxy.CopyToTarget();
		tk2dSpriteCollectionBuilder.ResetCurrentBuild();
		if (!tk2dSpriteCollectionBuilder.Rebuild(_spriteCollection)) {
			EditorUtility.DisplayDialog("Failed to commit sprite collection", 
				"Please check the console for more details.", "Ok");
		}
		spriteCollectionProxy.CopyFromSource();
	}
	
	void HandleListKeyboardShortcuts(int controlId)
	{
		Event ev = Event.current;
		if (ev.type == EventType.KeyDown 
			&& (GUIUtility.keyboardControl == controlId || GUIUtility.keyboardControl == 0)
			&& entries != null && entries.Count > 0)
		{
			int selectedIndex = 0;
			foreach (var e in entries)
			{
				if (e.selected) break;
				selectedIndex++;
			}
			int newSelectedIndex = selectedIndex;
			switch (ev.keyCode)
			{
				case KeyCode.Home: newSelectedIndex = 0; break;
				case KeyCode.End: newSelectedIndex = entries.Count - 1; break;
				case KeyCode.UpArrow: newSelectedIndex = Mathf.Max(selectedIndex - 1, 0); break;
				case KeyCode.DownArrow: newSelectedIndex = Mathf.Min(selectedIndex + 1, entries.Count - 1); break;
				case KeyCode.PageUp: newSelectedIndex = Mathf.Max(selectedIndex - 10, 0); break;
				case KeyCode.PageDown: newSelectedIndex = Mathf.Min(selectedIndex + 10, entries.Count - 1); break;
			}
			if (newSelectedIndex != selectedIndex)
			{
				for (int i = 0; i < entries.Count; ++i)
					entries[i].selected = (i == newSelectedIndex);
				UpdateSelection();
				Repaint();
				ev.Use();
			}
		}
	}

	Vector2 spriteListScroll = Vector2.zero;
	int spriteListSelectionKey = 0;
	void DrawSpriteList()
	{
		if (spriteCollectionProxy != null && spriteCollectionProxy.Empty)
		{
			DrawDropZone();
			return;
		}
		
		int spriteListControlId = GUIUtility.GetControlID("tk2d.SpriteList".GetHashCode(), FocusType.Keyboard);
		HandleListKeyboardShortcuts(spriteListControlId);

		spriteListScroll = GUILayout.BeginScrollView(spriteListScroll, GUILayout.Width(leftBarWidth));
		GUILayout.BeginVertical(tk2dEditorSkin.SC_ListBoxBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		
		bool multiSelectKey = (Application.platform == RuntimePlatform.OSXEditor)?Event.current.command:Event.current.control;
		bool shiftSelectKey = Event.current.shift;

		bool selectionChanged = false;
		SpriteCollectionEditorEntry.Type lastType = SpriteCollectionEditorEntry.Type.None;
		foreach (var entry in entries)
		{
			if (lastType != entry.type)
			{
				if (lastType != SpriteCollectionEditorEntry.Type.None)
					GUILayout.Space(8);
				else
					GUI.SetNextControlName("firstLabel");
				
				GUILayout.Label(GetEntryTypeString(entry.type), tk2dEditorSkin.SC_ListBoxSectionHeader, GUILayout.ExpandWidth(true));
				lastType = entry.type;
			}
			
			bool newSelected = GUILayout.Toggle(entry.selected, entry.name, tk2dEditorSkin.SC_ListBoxItem, GUILayout.ExpandWidth(true));
			if (newSelected != entry.selected)
			{
				GUI.FocusControl("firstLabel");
				
				entry.selectionKey = spriteListSelectionKey++;
				if (multiSelectKey)
				{
					// Only allow multiselection with sprites
					bool selectionAllowed = entry.type == SpriteCollectionEditorEntry.Type.Sprite;
					foreach (var e in entries)
					{
						if (e != entry && e.selected && e.type != entry.type)
						{
							selectionAllowed = false;
							break;
						}
					}
					
					if (selectionAllowed)
					{
						entry.selected = newSelected;
						selectionChanged = true;
					}
					else
					{
						foreach (var e in entries)
						{
							e.selected = false;
						}
						entry.selected = true;
						selectionChanged = true;
					}
				}
				else if (shiftSelectKey)
				{
					// find first selected entry in list
					int firstSelection = int.MaxValue;
					foreach (var e in entries)
					{
						if (e.selected && e.listIndex < firstSelection)
						{
							firstSelection = e.listIndex;
						}
					}
					int lastSelection = entry.listIndex;
					if (lastSelection < firstSelection)
					{
						lastSelection = firstSelection;
						firstSelection = entry.listIndex;
					}
					// Filter for multiselection
					if (entry.type == SpriteCollectionEditorEntry.Type.Sprite)
					{
						for (int i = firstSelection; i <= lastSelection; ++i)
						{
							if (entries[i].type != entry.type)
							{
								firstSelection = entry.listIndex;
								lastSelection = entry.listIndex;
							}
						}
					}
					else
					{
						firstSelection = lastSelection = entry.listIndex;
					}
					foreach (var e in entries)
					{
						e.selected = (e.listIndex >= firstSelection && e.listIndex <= lastSelection);
					}
					selectionChanged = true;
				}
				else
				{
					foreach (var e in entries)
					{
						e.selected = false;
					}
					entry.selected = true;
					selectionChanged = true;
				}
			}
		}
		
		if (selectionChanged)
		{
			GUIUtility.keyboardControl = spriteListControlId;
			UpdateSelection();
			Repaint();
		}
		
		GUILayout.EndVertical();
		GUILayout.EndScrollView();

		Rect viewRect = GUILayoutUtility.GetLastRect();
		tk2dPreferences.inst.spriteCollectionListWidth = (int)tk2dGuiUtility.DragableHandle(4819283, 
			viewRect, tk2dPreferences.inst.spriteCollectionListWidth, 
			tk2dGuiUtility.DragDirection.Horizontal);
	}
	
	bool IsValidDragPayload()
	{
		int idx = 0;
		foreach (var v in DragAndDrop.objectReferences)
		{
			var type = v.GetType();
			if (type == typeof(Texture2D))
				return true;
			else if (type == typeof(Object) && System.IO.Directory.Exists(DragAndDrop.paths[idx]))
				return true;
			++idx;
		}
		return false;
	}
	
	string GetEntryTypeString(SpriteCollectionEditorEntry.Type kind)
	{
		switch (kind)
		{
			case SpriteCollectionEditorEntry.Type.Sprite: return "Sprites";
			case SpriteCollectionEditorEntry.Type.SpriteSheet: return "Sprite Sheets";
			case SpriteCollectionEditorEntry.Type.Font: return "Fonts";
		}
		
		Debug.LogError("Unhandled type");
		return "";
	}

	bool PromptImportDuplicate(string title, string message) {
		return EditorUtility.DisplayDialog(title, message, "Ignore", "Create Copy");
	}
	
	void HandleDroppedPayload(Object[] objects)
	{
		bool hasDuplicates = false;
		foreach (var obj in objects)
		{
			Texture2D tex = obj as Texture2D;
			if (tex != null) {
				if (spriteCollectionProxy.FindSpriteBySource(tex) != -1) {
					hasDuplicates = true;
				}
			}
		}

		bool cloneDuplicates = false;
		if (hasDuplicates && EditorUtility.DisplayDialog("Duplicate textures detected.",
				"One or more textures is already in the collection. What do you want to do with the duplicates?", 
				"Clone", "Ignore")) {
			cloneDuplicates = true;
		}

		List<int> addedIndices = new List<int>();
		foreach (var obj in objects)
		{
			Texture2D tex = obj as Texture2D;
			if ((tex != null) && (cloneDuplicates || spriteCollectionProxy.FindSpriteBySource(tex) == -1)) {
				string name = spriteCollectionProxy.FindUniqueTextureName(tex.name);
				int slot = spriteCollectionProxy.FindOrCreateEmptySpriteSlot();
				spriteCollectionProxy.textureParams[slot].name = name;
				spriteCollectionProxy.textureParams[slot].colliderType = tk2dSpriteCollectionDefinition.ColliderType.UserDefined;
				spriteCollectionProxy.textureParams[slot].texture = (Texture2D)obj;
				addedIndices.Add(slot);
			}
		}
		// And now select them
		searchFilter = "";
		PopulateEntries();
		foreach (var entry in entries)
		{
			if (entry.type == SpriteCollectionEditorEntry.Type.Sprite &&
				addedIndices.IndexOf(entry.index) != -1)
				entry.selected = true;
		}

		UpdateSelection();
	}
	
	// recursively find textures in path
	List<Object> AddTexturesInPath(string path)
	{
		List<Object> localObjects = new List<Object>();
		foreach (var q in System.IO.Directory.GetFiles(path))
		{
			string f = q.Replace('\\', '/');
			System.IO.FileInfo fi = new System.IO.FileInfo(f);
			if (fi.Extension.ToLower() == ".meta")
				continue;
			
			Object obj = AssetDatabase.LoadAssetAtPath(f, typeof(Texture2D));
			if (obj != null) localObjects.Add(obj);
		}
		foreach (var q in System.IO.Directory.GetDirectories(path)) 
		{
			string d = q.Replace('\\', '/');
			localObjects.AddRange(AddTexturesInPath(d));
		}
		
		return localObjects;
	}
	
	int leftBarWidth { get { return tk2dPreferences.inst.spriteCollectionListWidth; } }

	Object[] deferredDroppedObjects;
	void DrawDropZone()
	{
		GUILayout.BeginVertical(tk2dEditorSkin.SC_ListBoxBG, GUILayout.Width(leftBarWidth), GUILayout.ExpandHeight(true));
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (DragAndDrop.objectReferences.Length == 0 && !SpriteCollection.Empty)
			GUILayout.Label("Drop sprite here", tk2dEditorSkin.SC_DropBox);
		else
			GUILayout.Label("Drop sprites here", tk2dEditorSkin.SC_DropBox);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();

		Rect rect = new Rect(0, 0, leftBarWidth, Screen.height);
		if (rect.Contains(Event.current.mousePosition))
		{
			switch (Event.current.type)
			{
			case EventType.DragUpdated:
				if (IsValidDragPayload())
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				else
					DragAndDrop.visualMode = DragAndDropVisualMode.None;
				break;
				
			case EventType.DragPerform:
				var droppedObjectsList = new List<Object>();
				for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
				{
					var type = DragAndDrop.objectReferences[i].GetType();
					if (type == typeof(Texture2D))
						droppedObjectsList.Add(DragAndDrop.objectReferences[i]);
					else if (type == typeof(Object) && System.IO.Directory.Exists(DragAndDrop.paths[i]))
						droppedObjectsList.AddRange(AddTexturesInPath(DragAndDrop.paths[i]));
				}
				deferredDroppedObjects = droppedObjectsList.ToArray();
				Repaint();
				break;
			}
		}
	}
	
	bool dragging = false;
	bool currentDraggingValue = false;

	bool firstRun = true;
	List<UnityEngine.Object> assetsInResources = new List<UnityEngine.Object>();
	
	bool InResources(UnityEngine.Object obj)
	{
		return AssetDatabase.GetAssetPath(obj).ToLower().IndexOf("/resources/") != -1;
	}

	void CheckForAssetsInResources()
	{
		assetsInResources.Clear();
		foreach (tk2dSpriteCollectionDefinition tex in SpriteCollection.textureParams)
		{
			if (tex.texture == null) continue;
			if (InResources(tex.texture) && assetsInResources.IndexOf(tex.texture) == -1) assetsInResources.Add(tex.texture);
		}
		foreach (tk2dSpriteCollectionFont font in SpriteCollection.fonts)
		{
			if (font.texture != null && InResources(font.texture) && assetsInResources.IndexOf(font.texture) == -1) assetsInResources.Add(font.texture);
			if (font.bmFont != null && InResources(font.bmFont) && assetsInResources.IndexOf(font.bmFont) == -1) assetsInResources.Add(font.bmFont);
		}
	}

	Vector2 assetWarningScroll = Vector2.zero;
	bool HandleAssetsInResources()
	{
		if (firstRun && SpriteCollection != null)
		{
			CheckForAssetsInResources();
			firstRun = false;
		}
		if (assetsInResources.Count > 0)
		{
			tk2dGuiUtility.InfoBox("Warning: The following assets are in one or more resources directories.\n" + 
				"These files will be included in the build.",
				tk2dGuiUtility.WarningLevel.Warning);
			assetWarningScroll = GUILayout.BeginScrollView(assetWarningScroll, GUILayout.ExpandWidth(true));
			foreach (UnityEngine.Object obj in assetsInResources)
			{
				EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), false);
			}
			GUILayout.EndScrollView();
			GUILayout.Space(8);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Ok", GUILayout.MinWidth(100)))
			{
				assetsInResources.Clear();
				Repaint();
			}
			GUILayout.EndHorizontal();
			return true;
		}
		return false;
	}

    void OnGUI() 
	{
		if (Event.current.type == EventType.DragUpdated)
		{
			if (IsValidDragPayload())
				dragging = true;
		}
		else if (Event.current.type == EventType.DragExited)
		{
			dragging = false;
			Repaint();
		}
		else
		{
			if (currentDraggingValue != dragging)
			{
				currentDraggingValue = dragging;
			}
		}
		
		if (Event.current.type == EventType.Layout && deferredDroppedObjects != null)
		{
			HandleDroppedPayload(deferredDroppedObjects);
			deferredDroppedObjects = null;
		}

		if (HandleAssetsInResources()) return;
		
		GUILayout.BeginVertical();

		DrawToolbar();
		
		GUILayout.BeginHorizontal();
		
		if (currentDraggingValue)
			DrawDropZone();
		else
			DrawSpriteList();

		if (settingsView.show || (spriteCollectionProxy != null && spriteCollectionProxy.Empty)) settingsView.Draw();
		else if (fontView.Draw(selectedEntries)) { }
		else if (spriteSheetView.Draw(selectedEntries)) { }
		else spriteView.Draw(selectedEntries);
		
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
    }
}
