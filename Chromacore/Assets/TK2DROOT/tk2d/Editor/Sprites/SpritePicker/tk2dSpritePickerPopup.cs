using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class tk2dSpritePickerPopup  : EditorWindow
{
	class PresentationParams
	{
		public int border = 12;
		public int tileSep = 8;
		public int labelHeight = 15;
		public int labelOffset = 4;
		public int scrollbarWidth = 16;
		public int tileSizeMin = 32;
		public int tileSizeMax = 512;
		public int numTilesPerRow = 0;
		public int tileHeight = 0;
		public int tileWidth = 0;
		public int numRows = 0;
		int tileSizeInt = 128;

		public Vector2 rectDims;

		public void Update(int numTiles) {
			tileSizeInt = TileSize;
			int w = Screen.width - scrollbarWidth - border * 2;
			numTilesPerRow = Mathf.Max((w + tileSep) / (tileSizeInt + tileSep), 1);
			numRows = (numTiles + numTilesPerRow - 1) / numTilesPerRow;
			tileWidth = tileSizeInt + tileSep;
			tileHeight = tileSizeInt + tileSep + labelOffset + labelHeight;
			rectDims = new Vector2(w, numRows * tileHeight + border * 2);
		}

		public int TileSize
		{
			get { return tk2dPreferences.inst.spriteThumbnailSize; }
			set { tk2dPreferences.inst.spriteThumbnailSize = Mathf.Clamp(value, tileSizeMin, tileSizeMax); }
		}

		public int GetYForIndex(int index)
		{
			int tileSep = 8;
			int labelHeight = 15;
			int tileHeight = tileSizeInt + tileSep + labelOffset + labelHeight;
			return tileHeight * (index / numTilesPerRow);			
		}
	}
	PresentationParams presentParams = new PresentationParams();

	tk2dSpriteGuiUtility.SpriteChangedCallback callback = null;
	object callbackData = null;

	tk2dSpriteCollectionData spriteCollection = null;
	List<tk2dSpriteDefinition> selectedDefinitions = new List<tk2dSpriteDefinition>();
	string searchFilter = "";
	int selectedIndex = -1;
	bool makeSelectionVisible = false;
	Vector2 scroll = Vector2.zero;
	tk2dSpriteDefinition SelectedDefinition 
	{
		get {
			for (int i = 0; i < selectedDefinitions.Count; ++i)
				if (i == selectedIndex) return selectedDefinitions[i];
			return null;
		}
	}

	void OnEnable()
	{
		UpdateFilter();
	}

	void OnDestroy()
	{
		tk2dSpriteThumbnailCache.Done();
		tk2dEditorSkin.Done();
		tk2dGrid.Done();
	}

	void PerformSelection()
	{
		tk2dSpriteDefinition def = SelectedDefinition;
		if (def != null)
		{
			int spriteIndex = -1;
			for (int i = 0; i < spriteCollection.inst.spriteDefinitions.Length; ++i)
				if (spriteCollection.inst.spriteDefinitions[i] == def)
					spriteIndex = i;
			if (spriteIndex != -1 && callback != null)
				callback( spriteCollection, spriteIndex, callbackData );
		}
	}

	void UpdateFilter()
	{
		if (spriteCollection == null)
		{
			selectedDefinitions.Clear();
		}
		else
		{
			tk2dSpriteDefinition selectedSprite = SelectedDefinition;

			string s = searchFilter.ToLower();
			if (s != "") {
				selectedDefinitions = (from d in spriteCollection.inst.spriteDefinitions where d.Valid && d.name.ToLower().IndexOf(s) != -1 select d)
				.OrderBy( d => d.name, new tk2dEditor.Shared.NaturalComparer() )
				.ToList();				
			}
			else {
				selectedDefinitions = (from d in spriteCollection.inst.spriteDefinitions where d.Valid select d)
				.OrderBy( d => d.name, new tk2dEditor.Shared.NaturalComparer() )
				.ToList();
			}

			selectedIndex = -1;
			for (int i = 0; i < selectedDefinitions.Count; ++i)
			{
				if (selectedSprite == selectedDefinitions[i])
				{
					selectedIndex = i;
					break;
				}
			}
		}
	}

	void HandleKeyboardShortcuts() {
		Event ev = Event.current;

		int numTilesPerRow = presentParams.numTilesPerRow;
		int newSelectedIndex = selectedIndex;
		if (ev.type == EventType.KeyDown)
		{
			switch (ev.keyCode)
			{
				case KeyCode.Escape:
					if (searchFilter.Length > 0)
					{
						searchFilter = "";
						UpdateFilter();
						newSelectedIndex = selectedIndex; // avoid it doing any processing later
					}
					else
					{
						Close();
					}
					ev.Use();
					break;
				case KeyCode.Return:
					{
						PerformSelection();
						ev.Use();
						Close();
						break;
					}			
				case KeyCode.RightArrow: newSelectedIndex++;break;
				case KeyCode.LeftArrow: newSelectedIndex--; break;
				case KeyCode.DownArrow: if (newSelectedIndex + numTilesPerRow < selectedDefinitions.Count) newSelectedIndex += numTilesPerRow; break;
				case KeyCode.UpArrow: if (newSelectedIndex - numTilesPerRow >= 0) newSelectedIndex -= numTilesPerRow; break;
				case KeyCode.Home: newSelectedIndex = 0; break;
				case KeyCode.End: newSelectedIndex = selectedDefinitions.Count - 1; break;

				default:
					return; // unused key
			}

			newSelectedIndex = Mathf.Clamp(newSelectedIndex, 0, selectedDefinitions.Count - 1);
			if (newSelectedIndex != selectedIndex)
			{
				selectedIndex = newSelectedIndex;
				ev.Use();
				makeSelectionVisible = true;
			}
		}
	}

	void OnGUI()
	{
		HandleKeyboardShortcuts();
		presentParams.Update(selectedDefinitions.Count);

		if (makeSelectionVisible) {
			scroll.y = presentParams.GetYForIndex(selectedIndex);
			makeSelectionVisible = false;
		}

		GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

		GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

		GUILayout.Label("Collection: ");
		tk2dSpriteCollectionData newSpriteCollection = tk2dSpriteGuiUtility.SpriteCollectionList(spriteCollection);
		if (newSpriteCollection != spriteCollection)
		{
			spriteCollection = newSpriteCollection;
			searchFilter = "";
			UpdateFilter();
		}
		GUILayout.Space(8);


		string searchControlName = "tk2dSpritePickerSearch";
		GUI.SetNextControlName(searchControlName);
		string newSearchFilter = GUILayout.TextField(searchFilter, tk2dEditorSkin.ToolbarSearch, GUILayout.Width(140));
		if (GUIUtility.keyboardControl == 0) {
			GUI.FocusControl(searchControlName);
		}
		if (newSearchFilter != searchFilter)
		{
			searchFilter = newSearchFilter;
			UpdateFilter();
		}
		if (searchFilter.Length > 0)
		{
			if (GUILayout.Button("", tk2dEditorSkin.ToolbarSearchClear, GUILayout.ExpandWidth(false)))
			{
				searchFilter = "";
				UpdateFilter();
			}
		}
		else
		{
			GUILayout.Label("", tk2dEditorSkin.ToolbarSearchRightCap);
		}

		GUILayout.FlexibleSpace();
		presentParams.TileSize = (int)EditorGUILayout.Slider(presentParams.TileSize, presentParams.tileSizeMin, presentParams.tileSizeMax, GUILayout.Width(120));
		GUILayout.EndHorizontal();

		if (selectedIndex < 0 || selectedIndex >= selectedDefinitions.Count)
		{
			if (selectedDefinitions.Count == 0) selectedIndex = -1;
			else selectedIndex = 0;
		}

		if (spriteCollection != null)
			DrawSpriteCollection(spriteCollection);

		GUILayout.EndArea();
	}

	void DrawSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		Event ev = Event.current;
		int tileSize = presentParams.TileSize;

		scroll = GUILayout.BeginScrollView(scroll);
		Rect r = GUILayoutUtility.GetRect(presentParams.rectDims.x, presentParams.rectDims.y);

		if (ev.type == EventType.MouseDown && ev.button == 0 && r.Contains(ev.mousePosition))
		{
			int selX = ((int)ev.mousePosition.x - presentParams.border) / presentParams.tileWidth;
			int selY = ((int)ev.mousePosition.y - presentParams.border) / presentParams.tileHeight;
			int selId = selY * presentParams.numTilesPerRow + selX;
			if (selX < presentParams.numTilesPerRow && selY < presentParams.numRows) {
				selectedIndex = Mathf.Clamp(selId, 0, selectedDefinitions.Count - 1);
				Repaint();
			}

			if (ev.clickCount == 2)
			{
				PerformSelection();
				Close();
			}

			ev.Use();
		}

		r.x += presentParams.border;
		r.y += presentParams.border;

		int ix = 0;
		float x = r.x;
		float y = r.y;
		int index = 0;
		foreach (tk2dSpriteDefinition def in selectedDefinitions)
		{
			Rect spriteRect = new Rect(x, y, tileSize, tileSize);
			tk2dGrid.Draw(spriteRect, Vector2.zero);
			tk2dSpriteThumbnailCache.DrawSpriteTextureInRect(spriteRect, def, Color.white);

			Rect labelRect = new Rect(x, y + tileSize + presentParams.labelOffset, tileSize, presentParams.labelHeight);
			if (selectedIndex == index)
			{
				GUI.Label(labelRect, "", tk2dEditorSkin.Selection);
			}
			GUI.Label(labelRect, def.name, EditorStyles.miniLabel);

			if (++ix >= presentParams.numTilesPerRow)
			{
				ix = 0;
				x = r.x;
				y += presentParams.tileHeight;
			}
			else
			{
				x += presentParams.tileWidth;
			}
			index++;
		}

		GUILayout.EndScrollView();
	}

	void InitForSpriteInCollection(tk2dSpriteCollectionData sc, int spriteId)
	{
		spriteCollection = sc;
		searchFilter = "";
		UpdateFilter();
		
		tk2dSpriteDefinition def = (spriteId >= 0 && spriteId < sc.Count) ? sc.inst.spriteDefinitions[spriteId] : null;
		selectedIndex = -1;
		for (int i = 0; i < selectedDefinitions.Count; ++i) {
			if (selectedDefinitions[i] == def) {
				selectedIndex = i;
				break;
			}
		}
		if (selectedIndex != -1) {
			makeSelectionVisible = true;
		}
	}

	public static void DoPickSprite(tk2dSpriteCollectionData spriteCollection, int spriteId, string title, tk2dSpriteGuiUtility.SpriteChangedCallback callback, object callbackData)
	{
		tk2dSpritePickerPopup popup = EditorWindow.GetWindow(typeof(tk2dSpritePickerPopup), true, title, true) as tk2dSpritePickerPopup;
		popup.InitForSpriteInCollection(spriteCollection, spriteId);
		popup.callback = callback;
		popup.callbackData = callbackData;
	}
}

