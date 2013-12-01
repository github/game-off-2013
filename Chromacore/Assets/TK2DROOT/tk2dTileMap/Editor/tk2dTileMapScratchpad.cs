using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
class tk2dScratchpadLayer {
	public int width, height;
	public int[] tiles;

	public tk2dScratchpadLayer() {
		width = 0;
		height = 0;
		tiles = new int[0];
	}

	public void SetDimensions(int _width, int _height) {
		int[] newTiles = new int[_width * _height];
		for (int y = 0; y < _height; ++y) {
			for (int x = 0; x < _width; ++x) {
				newTiles[y * _width + x] = (x < width && y < height) ? tiles[y * width + x] : -1;
			}
		}
		tiles = newTiles;
		width = _width;
		height = _height;
	}

	public void SplatTile(int x, int y, int rawTile) {
		if (x >= 0 && x < width && y >= 0 && y < height) {
			tiles[y * width + x] = rawTile;
		}
	}

	public void EraseTiles(int x1, int y1, int x2, int y2) {
		for (int y = y1; y <= y2; ++y) {
			for (int x = x1; x <= x2; ++x) {
				if (x >= 0 && x < width && y >= 0 && y < height) {
					tiles[y * width + x] = -1;
				}
			}
		}
	}

	public int GetTile(int x, int y) {
		if (x >= 0 && x < width && y >= 0 && y < height) {
			return tiles[y * width + x];
		}
		return -1;
	}
}

[System.Serializable]
public class tk2dTileMapScratchpad
{
	[SerializeField] int width;
	[SerializeField] int height;
	[SerializeField] tk2dScratchpadLayer[] layers;
	tk2dTileMapEditorBrush canvas;
	public string name = "New Scratchpad";
	bool tileSortLeftToRight = true;
	bool tileSortBottomToTop = true;

	public tk2dTileMapScratchpad() {
		width = 0;
		height = 0;
		layers = new tk2dScratchpadLayer[0];
		canvas = new tk2dTileMapEditorBrush();
	}

	public void SetDimensions(int _width, int _height) {
		width = _width;
		height = _height;
		foreach (tk2dScratchpadLayer layer in layers) {
			layer.SetDimensions(width, height);
		}
	}

	public void SetNumLayers(int n) {
		tk2dScratchpadLayer[] newLayers = new tk2dScratchpadLayer[n];
		for (int i = 0; i < n; ++i) {
			newLayers[i] = (i < layers.Length) ? layers[i] : new tk2dScratchpadLayer();
		}
		layers = newLayers;
		SetDimensions(width, height);
	}

	public void UpdateCanvas() {
        List<tk2dSparseTile> newTiles = new List<tk2dSparseTile>();
        for (int iLayer = 0; iLayer < layers.Length; ++iLayer) {
            tk2dScratchpadLayer layer = layers[iLayer];
            for (int y = 0; y < layer.height; ++y) {
                for (int x = 0; x < layer.width; ++x) {
					int k = y * layer.width + x;
					if (layer.tiles[k] != -1) {
						newTiles.Add(new tk2dSparseTile(x, y, iLayer, layer.tiles[k]));
					}
                }
            }
        }
		canvas.tiles = newTiles.ToArray();
		canvas.SortTiles(tileSortLeftToRight, tileSortBottomToTop);
		canvas.UpdateBrushHash();
	}

	public void SplatTile(int x, int y, int layer, int rawTile) {
		if (layer >= 0 && layer < layers.Length) {
			layers[layer].SplatTile(x, y, rawTile);
		}
	}

	public void EraseTiles(int x1, int y1, int x2, int y2, int layer) {
		if (layer >= 0 && layer < layers.Length) {
			layers[layer].EraseTiles(x1, y1, x2, y2);
		}
	}

	public int GetTile(int x, int y, int layer) {
		if (layer >= 0 && layer < layers.Length) {
			return layers[layer].GetTile(x, y);
		}
		return -1;
	}

	public void GetDimensions(out int _width, out int _height) {
		_width = width;
		_height = height;
	}

	public int GetNumLayers() {
		return layers.Length;
	}

	public void SetTileSort(bool leftToRight, bool bottomToTop) {
		tileSortLeftToRight = leftToRight;
		tileSortBottomToTop = bottomToTop;
	}

	public tk2dTileMapEditorBrush CanvasBrush {
		get {return canvas;}
	}
}

public class tk2dScratchpadGUI {
	tk2dTileMapSceneGUI parent = null;
	List<tk2dTileMapScratchpad> activeScratchpads = null;
	List<tk2dTileMapScratchpad> filteredScratchpads = null;
	tk2dTileMapScratchpad currentScratchpad = null;
	int currentLayer = 0;

	tk2dEditor.BrushRenderer brushRenderer = null;
	tk2dTileMapEditorBrush workingBrush = null;

	public bool workingHere = false;
	public bool doMouseDown = false;
	public bool doMouseDrag = false;
	public bool doMouseUp = false;
	public bool doMouseMove = false;
	public Vector2 paintMousePosition = Vector2.zero;
	public Rect padAreaRect = new Rect(0, 0, 0, 0);
	public bool requestSelectAllTiles = false;
	public bool requestClose = false;
	public string tooltip = "";
	public float scratchZoom = 1.0f;

	Vector3 tileSize = Vector3.zero;
	Vector2 texelSize = Vector2.one;

	public tk2dScratchpadGUI(tk2dTileMapSceneGUI _parent, tk2dEditor.BrushRenderer _brushRenderer, tk2dTileMapEditorBrush _workingBrush) {
		parent = _parent;
		brushRenderer = _brushRenderer;
		workingBrush = _workingBrush;
	}

	public void SetActiveScratchpads(List<tk2dTileMapScratchpad> scratchpads) {
		activeScratchpads = scratchpads;
		currentScratchpad = null;
		UpdateFilteredScratchpads();
	}

	public void SetTileSizes(Vector3 _tileSize, Vector2 _texelSize) {
		tileSize = _tileSize;
		texelSize = _texelSize;
	}

	public void SetTileSort(bool leftToRight, bool bottomToTop) {
		if (currentScratchpad != null)
			currentScratchpad.SetTileSort(leftToRight, bottomToTop);
	}

	Vector2 padsScrollPos = Vector2.zero;
	Vector2 canvasScrollPos = Vector2.zero;
	int padWidthField = 15;
	int padHeightField = 15;
	string searchFilter = "";
	bool focusName = false;
	bool focusSearchFilter = false;
	bool focusSearchFilterOnKeyUp = false;
	System.Action<int> pendingAction = null;

	void SelectScratchpad(tk2dTileMapScratchpad pad) {
		currentScratchpad = pad;
		if (currentScratchpad != null)
			currentScratchpad.GetDimensions(out padWidthField, out padHeightField);
	}

	public void DrawGUI() {
		GUILayout.BeginHorizontal();

		GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
		centeredLabel.alignment = TextAnchor.MiddleCenter;
		GUIStyle centeredTextField = new GUIStyle(EditorStyles.textField);
		centeredTextField.alignment = TextAnchor.MiddleCenter;

		GUILayout.BeginVertical(GUILayout.Width(150.0f));

		GUILayout.BeginHorizontal();
		if (GUILayout.Button("New Scratchpad")) {
			if (activeScratchpads != null) {
				pendingAction = delegate(int i) {
					tk2dTileMapScratchpad newPad = new tk2dTileMapScratchpad();
					newPad.SetNumLayers(1);
					newPad.SetDimensions(15, 15);
					activeScratchpads.Add(newPad);
					SelectScratchpad(newPad);
					UpdateFilteredScratchpads();
					focusName = true;
				};
			}
		}
		GUILayout.EndHorizontal();

		bool pressedUp = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.UpArrow);
		bool pressedDown = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.DownArrow);
		bool pressedReturn = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return);
		bool pressedEscape = (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape);

		if (pressedUp || pressedDown) {
			Event.current.Use();
			if (filteredScratchpads != null) {
				int curIdx = 0;
				for (int i = 0; i < filteredScratchpads.Count(); ++i) {
					if (filteredScratchpads[i] == currentScratchpad)
						curIdx = i;
				}
				curIdx += pressedDown ? 1 : -1;
				curIdx = Mathf.Clamp(curIdx, 0, filteredScratchpads.Count() - 1);
				for (int i = 0; i < filteredScratchpads.Count(); ++i) {
					if (i == curIdx)
						SelectScratchpad(filteredScratchpads[i]);
				}
			}
		}

		GUILayout.BeginHorizontal();
		GUI.SetNextControlName("SearchFilter");
		string newSearchFilter = GUILayout.TextField(searchFilter, tk2dEditorSkin.ToolbarSearch);
		if (newSearchFilter != searchFilter) {
			searchFilter = newSearchFilter;
			UpdateFilteredScratchpads();
			if (searchFilter.Length > 0 && filteredScratchpads != null && filteredScratchpads.Count() > 0) {
				SelectScratchpad(filteredScratchpads[0]);
			}
		}
		GUILayout.Label("", tk2dEditorSkin.ToolbarSearchRightCap);
		GUI.SetNextControlName("dummy");
		GUILayout.Box("", GUIStyle.none, GUILayout.Width(0), GUILayout.Height(0));
		if (focusSearchFilter || (focusSearchFilterOnKeyUp && Event.current.type == EventType.KeyUp)) {
			GUI.FocusControl("dummy");
			GUI.FocusControl("SearchFilter");
			focusSearchFilter = false;
			focusSearchFilterOnKeyUp = false;
		}
		GUILayout.EndHorizontal();

		bool searchHasFocus = (GUI.GetNameOfFocusedControl() == "SearchFilter");
		if (pressedEscape) {
			if (searchHasFocus && searchFilter.Length > 0) {
				searchFilter = "";
				UpdateFilteredScratchpads();
			}
			else {
				requestClose = true;
			}
		}

		// Select All
		if (pressedReturn && GUI.GetNameOfFocusedControl() != "ScratchpadName") {
			requestSelectAllTiles = true;
			requestClose = true;
		}

		padsScrollPos = GUILayout.BeginScrollView(padsScrollPos);
		List<tk2dTileMapScratchpad> curList = null;
		if (filteredScratchpads != null)
			curList = filteredScratchpads;
		else if (activeScratchpads != null)
			curList = activeScratchpads;
		if (curList != null) {
			GUILayout.BeginVertical();
			foreach (var pad in curList) {
				bool selected = currentScratchpad == pad;
				if (selected) {
					GUILayout.BeginHorizontal();
				}
				if (GUILayout.Toggle(selected, pad.name, tk2dEditorSkin.SC_ListBoxItem)) {
					if (currentScratchpad != pad) {
						SelectScratchpad(pad);
					}
				}
				if (selected) {
					if (GUILayout.Button("", tk2dEditorSkin.GetStyle("TilemapDeleteItem"))) {
						pendingAction = delegate(int i) {
							if (EditorUtility.DisplayDialog("Delete Scratchpad \"" + currentScratchpad.name + "\" ?", " ", "Yes", "No"))
							{
								activeScratchpads.Remove(currentScratchpad);
								SelectScratchpad(null);
								UpdateFilteredScratchpads();
							}
						};
					}
					GUILayout.EndHorizontal();
				}
			}

			GUILayout.EndVertical();
		}
		GUILayout.EndScrollView();

		if (currentScratchpad != null) {
			GUILayout.Space(20.0f);

			GUILayout.BeginVertical(tk2dEditorSkin.SC_ListBoxBG);
			// Select All
			if (GUILayout.Button(new GUIContent("Select All", "(Enter)"))) {
				requestSelectAllTiles = true;
				requestClose = true;
			}

			// Name
			GUI.SetNextControlName("ScratchpadName");
			currentScratchpad.name = EditorGUILayout.TextField(currentScratchpad.name, centeredTextField);
			if (focusName) {
				GUI.FocusControl("ScratchpadName");
				focusName = false;
			}

			// Size
			int padWidth, padHeight;
			currentScratchpad.GetDimensions(out padWidth, out padHeight);
			GUILayout.BeginHorizontal();
			padWidthField = EditorGUILayout.IntField(padWidthField);
			padHeightField = EditorGUILayout.IntField(padHeightField);
			GUILayout.EndHorizontal();
			if (padWidthField != padWidth || padHeightField != padHeight) {
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Apply size change")) {
					currentScratchpad.SetDimensions(padWidthField, padHeightField);
				}
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}

		tooltip = GUI.tooltip;

		GUILayout.EndVertical();

		GUILayout.BeginVertical();

		// Painting area
		doMouseDown = false;
		doMouseDrag = false;
		doMouseUp = false;
		doMouseMove = false;
		if (currentScratchpad != null) {
			//temp
			currentScratchpad.UpdateCanvas();

			int scratchW, scratchH;
			currentScratchpad.GetDimensions(out scratchW, out scratchH);
			canvasScrollPos = EditorGUILayout.BeginScrollView(canvasScrollPos, GUILayout.Width(Mathf.Min(scratchW * tileSize.x * scratchZoom + 24.0f, Screen.width - 190.0f)));

			Rect padRect = GUILayoutUtility.GetRect(scratchW * tileSize.x * scratchZoom, scratchH * tileSize.y * scratchZoom, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
			tk2dGrid.Draw(padRect);
			padAreaRect = padRect;

			Matrix4x4 canvasMatrix = Matrix4x4.identity;
			SceneView sceneview = SceneView.lastActiveSceneView;
			if (sceneview != null) {
				Camera sceneCam = sceneview.camera;
				if (sceneCam != null) {
					canvasMatrix = sceneCam.cameraToWorldMatrix;
				}
			}
			canvasMatrix *= Matrix4x4.TRS(	new Vector3(padRect.x, padRect.y + padRect.height, 0.0f),
											Quaternion.identity,
											new Vector3(scratchZoom / texelSize.x, -scratchZoom / texelSize.y, 1.0f));

			if (Event.current.type == EventType.Repaint) {
				if (brushRenderer != null) {
					brushRenderer.DrawBrushInScratchpad(currentScratchpad.CanvasBrush, canvasMatrix, false);
					if (workingBrush != null && workingHere) {
						brushRenderer.DrawBrushInScratchpad(workingBrush, canvasMatrix, true);
					}
				}
				if (workingHere && parent != null) {
					parent.DrawTileCursor();
				}
			}

			Event ev = Event.current;
			if (ev.type == EventType.MouseMove || ev.type == EventType.MouseDrag) {
				paintMousePosition.x = ev.mousePosition.x - padRect.x;
				paintMousePosition.y = padRect.height - (ev.mousePosition.y - padRect.y);
				HandleUtility.Repaint();
			}
			if (ev.button == 0 || ev.button == 1) {
				doMouseDown = (ev.type == EventType.MouseDown);
				doMouseDrag = (ev.type == EventType.MouseDrag);
				doMouseUp = (ev.rawType == EventType.MouseUp);
			}
			doMouseMove = (ev.type == EventType.MouseMove);

			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("+", GUILayout.Width(20)))
				scratchZoom *= 1.5f;
			if (GUILayout.Button("-", GUILayout.Width(20)))
				scratchZoom /= 1.5f;
			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();

		GUILayout.EndHorizontal();

		if (pendingAction != null && Event.current.type == EventType.Repaint) {
			pendingAction(0);
			pendingAction = null;
			HandleUtility.Repaint();
		}
	}

	public void SplatTile(int x, int y, int rawTile) {
		if (currentScratchpad != null) {
			currentScratchpad.SplatTile(x, y, currentLayer, rawTile);
		}
	}

	public void EraseTiles(int x1, int y1, int x2, int y2) {
		if (currentScratchpad != null) {
			currentScratchpad.EraseTiles(x1, y1, x2, y2, currentLayer);
		}
	}

	public int GetTile(int x, int y, int layer) {
		if (currentScratchpad != null) {
			return currentScratchpad.GetTile(x, y, layer);
		}
		return -1;
	}

	public void GetScratchpadSize(out int x, out int y) {
		if (currentScratchpad != null) {
			currentScratchpad.GetDimensions(out x, out y);
		} else {
			x = 0;
			y = 0;
		}
	}

	public void FocusOnSearchFilter(bool onKeyUp) {
		if (onKeyUp) focusSearchFilterOnKeyUp = true;
		else focusSearchFilter = true;
	}

	public bool Contains(string s, string text) { return s.ToLower().IndexOf(text.ToLower()) != -1; }

	void UpdateFilteredScratchpads() {
		if (activeScratchpads != null) {
			if (searchFilter.Length == 0) {
				filteredScratchpads = activeScratchpads;
			}
			else {
				filteredScratchpads =	(from pad in activeScratchpads where Contains(pad.name, searchFilter) select pad)
										.OrderBy( a => a.name, new tk2dEditor.Shared.NaturalComparer() )
										.ToList();
			}
		}
		else {
			filteredScratchpads = null;
		}
	}

	public void GetTilesCropRect(out int x1, out int y1, out int x2, out int y2) {
		x1 = 0;
		y1 = 0;
		x2 = 0;
		y2 = 0;
		if (currentScratchpad != null) {
			int w, h;
			currentScratchpad.GetDimensions(out w, out h);
			x1 = w - 1;
			y1 = h - 1;
			x2 = 0;
			y2 = 0;
			for (int y = 0; y < h; ++y) {
				for (int x = 0; x < w; ++x) {
					if (currentScratchpad.GetTile(x, y, 0) != -1) {
						x1 = Mathf.Min(x1, x);
						y1 = Mathf.Min(y1, y);
						x2 = Mathf.Max(x2, x);
						y2 = Mathf.Max(y2, y);
					}
				}
			}
		}
	}
}