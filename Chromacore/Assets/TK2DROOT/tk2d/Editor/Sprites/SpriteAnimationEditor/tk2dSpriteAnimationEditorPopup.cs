using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class tk2dSpriteAnimationEditorPopup : EditorWindow 
{
	tk2dSpriteAnimation anim;
	tk2dEditor.SpriteAnimationEditor.ClipEditor clipEditor = null;

	tk2dEditor.SpriteAnimationEditor.AnimOperator[] animOps = new tk2dEditor.SpriteAnimationEditor.AnimOperator[0];

	// "Create" menu
	GUIContent[] menuItems = new GUIContent[0];
	tk2dEditor.SpriteAnimationEditor.AnimOperator[] menuTargets = new tk2dEditor.SpriteAnimationEditor.AnimOperator[0];

	void OnEnable()
	{
		// Detect animOps
		List<tk2dEditor.SpriteAnimationEditor.AnimOperator> animOpList = new List<tk2dEditor.SpriteAnimationEditor.AnimOperator>();
		foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				System.Type[] types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type.BaseType == typeof(tk2dEditor.SpriteAnimationEditor.AnimOperator))
					{
						tk2dEditor.SpriteAnimationEditor.AnimOperator inst = (tk2dEditor.SpriteAnimationEditor.AnimOperator)System.Activator.CreateInstance(type);
						if (inst != null)
							animOpList.Add(inst);
					}
				}
			}
			catch { }
		}
		animOpList.Sort((a, b) => a.SortId.CompareTo(b.SortId));
		animOps = animOpList.ToArray();

		// Create menu items
		List<GUIContent> menuItems = new List<GUIContent>();
		List<tk2dEditor.SpriteAnimationEditor.AnimOperator> menuTargets = new List<tk2dEditor.SpriteAnimationEditor.AnimOperator>();
		menuItems.Add(new GUIContent("Clip")); menuTargets.Add(null);

		foreach (tk2dEditor.SpriteAnimationEditor.AnimOperator animOp in animOps)
		{
			for (int i = 0; i < animOp.AnimToolsMenu.Length; ++i)
			{
				menuItems.Add(new GUIContent(animOp.AnimToolsMenu[i]));
				menuTargets.Add(animOp);				
			}
		}

		this.menuItems = menuItems.ToArray();
		this.menuTargets = menuTargets.ToArray();

		// Create clip editor
		if (clipEditor == null)
		{
			clipEditor = new tk2dEditor.SpriteAnimationEditor.ClipEditor();		
			clipEditor.clipNameChangedEvent += ClipNameChanged;
			clipEditor.clipDeletedEvent += ClipDeleted;
			clipEditor.clipSelectionChangedEvent += ClipSelectionChanged;
			clipEditor.hostEditorWindow = this;
		}
		clipEditor.animOps = animOps;
		
		FilterClips();
		if (selectedClipId != -1 && selectedClipId < allClips.Count)
		{
			selectedClip = allClips[selectedClipId];
		}
		else
		{
			selectedClip = null;
		}
	}

	void OnDisable()
	{
		OnDestroy();
	}

	void OnDestroy()
	{
		if (clipEditor != null)
			clipEditor.Destroy();

		tk2dEditorSkin.Done();
	}

	public void ClipNameChanged(tk2dSpriteAnimationClip clip, int param)
	{
		FilterClips();
		Repaint();
	}

	public void ClipDeleted(tk2dSpriteAnimationClip clip, int param)
	{
		clip.Clear();
		selectedClip = null;
		FilterClips();
		Repaint();
	}

	public void ClipSelectionChanged(tk2dSpriteAnimationClip clip, int direction)
	{
		int selectedId = -1;
		for (int i = 0; i < filteredClips.Count; ++i)
		{
			if (filteredClips[i] == clip)
			{
				selectedId = i;
				break;
			}
		}
		if (selectedId != -1)
		{
			int newSelectedId = selectedId + direction;
			if (newSelectedId >= 0 && newSelectedId < filteredClips.Count)
			{
				selectedClip = filteredClips[newSelectedId];	
			}
		}
		Repaint();
	}

	public void SetSpriteAnimation(tk2dSpriteAnimation anim)
	{
		if (anim != this.anim)
		{
			searchFilter = "";
			this.anim = anim;
			this.name = anim.name;
		}

		selectedClip = null;
		MirrorClips();
		FilterClips();

		Repaint();
	}

	void MirrorClips()
	{
		if (anim == null) return;

		allClips.Clear();
		if (anim.clips != null)
		{
			foreach (tk2dSpriteAnimationClip clip in anim.clips)
			{
				tk2dSpriteAnimationClip c = new tk2dSpriteAnimationClip();
				c.CopyFrom(clip);
				allClips.Add(c);
			}
		}
	}

	int minLeftBarWidth = 150;
	int leftBarWidth { get { return tk2dPreferences.inst.animListWidth; } set { tk2dPreferences.inst.animListWidth = Mathf.Max(value, minLeftBarWidth); } }

	string searchFilter = "";
	List<tk2dSpriteAnimationClip> allClips = new List<tk2dSpriteAnimationClip>();
	List<tk2dSpriteAnimationClip> filteredClips = new List<tk2dSpriteAnimationClip>();
	tk2dSpriteAnimationClip _selectedClip;
	int selectedClipId = -1;

	tk2dSpriteAnimationClip selectedClip
	{
		get { return _selectedClip; }
		set 
		{
			selectedClipId = -1;
			if (value != null)
			{
				for (int i = 0; i < allClips.Count; ++i)
				{
					if (allClips[i] == value)
					{
						_selectedClip = value;
						selectedClipId = i;
						break;
					}
				}
			}
			if (selectedClipId == -1)
			{
				if (value != null) Debug.LogError("Unable to find clip");
				_selectedClip = null;
			}
		}
	}

	public static bool Contains(string s, string text) { return s.ToLower().IndexOf(text.ToLower()) != -1; }
	
	void FilterClips()
	{
		filteredClips = new List<tk2dSpriteAnimationClip>(allClips.Count);
		if (searchFilter.Length == 0) {
			filteredClips = (from clip in allClips where !clip.Empty select clip)
							.OrderBy( a => a.name, new tk2dEditor.Shared.NaturalComparer() )
							.ToList();
		}
		else {
			filteredClips = (from clip in allClips where !clip.Empty && Contains(clip.name, searchFilter) select clip)
							.OrderBy( a => a.name, new tk2dEditor.Shared.NaturalComparer() )
							.ToList();
		}
	}

	void Commit()
	{
		if (anim == null) return;

		// Handle duplicate names
		string dupNameString = "";
		HashSet<string> duplicateNames = new HashSet<string>();
		HashSet<string> names = new HashSet<string>();
		foreach (tk2dSpriteAnimationClip clip in allClips)
		{
			if (clip.Empty) continue;
			if (names.Contains(clip.name)) { duplicateNames.Add(clip.name); dupNameString += clip.name + " "; continue; }
			names.Add(clip.name);
		}
		if (duplicateNames.Count > 0)
		{
			int res = EditorUtility.DisplayDialogComplex("Commit",
				"Duplicate names found in animation library. You won't be able to select duplicates in the interface.\n" +
				"Duplicates: " + dupNameString, 
				"Auto-rename",
				"Cancel",
				"Force commit");

			if (res == 1) return; // cancel
			if (res == 0)
			{
				// auto rename
				HashSet<string> firstOccurances = new HashSet<string>();
				foreach (tk2dSpriteAnimationClip clip in allClips)
				{
					if (clip.Empty) continue;
					string name = clip.name;
					if (duplicateNames.Contains(name))
					{
						if (!firstOccurances.Contains(name))
						{
							firstOccurances.Add(name);
						}
						else
						{
							// find suitable name
							int i = 1;
							string n = "";
							do 
							{
								n = string.Format("{0} {1}", name, i++);
							} while (names.Contains(n));
							name = n;

							names.Add(name);
							clip.name = name;
						}
					}
				}
				FilterClips();
				Repaint();
			}
		}

		anim.clips = new tk2dSpriteAnimationClip[allClips.Count];
		for (int i = 0; i < allClips.Count; ++i)
		{
			anim.clips[i] = new tk2dSpriteAnimationClip();
			anim.clips[i].CopyFrom(allClips[i]);
		}
		EditorUtility.SetDirty(anim);
		AssetDatabase.SaveAssets();
	}

	void Revert()
	{
		MirrorClips();
		searchFilter = "";
		FilterClips();

		selectedClip = null;
	}

	Vector2 listScroll = Vector2.zero;
	void DrawList()
	{
		listScroll = GUILayout.BeginScrollView(listScroll, GUILayout.Width(leftBarWidth));
		GUILayout.BeginVertical(tk2dEditorSkin.SC_ListBoxBG, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

		foreach (tk2dSpriteAnimationClip clip in filteredClips)
		{
			// 0 length name signifies inactive clip
			if (clip.name.Length == 0) continue;

			bool selected = selectedClip == clip;
			bool newSelected = GUILayout.Toggle(selected, clip.name, tk2dEditorSkin.SC_ListBoxItem, GUILayout.ExpandWidth(true));
			if (newSelected != selected && newSelected == true)
			{
				selectedClip = clip;
				GUIUtility.keyboardControl = 0;
				Repaint();
			}
		}

		GUILayout.EndVertical();
		GUILayout.EndScrollView();

		Rect viewRect = GUILayoutUtility.GetLastRect();
		leftBarWidth = (int)tk2dGuiUtility.DragableHandle(4819283, 
			viewRect, leftBarWidth, 
			tk2dGuiUtility.DragDirection.Horizontal);
	}

	void TrimClips()
	{
		if (allClips.Count < 1)
			return;

		int validCount = allClips.Count;
		while (validCount > 0 && !allClips[validCount - 1].Empty)
			--validCount;
		allClips.RemoveRange(validCount, allClips.Count - validCount);

		if (allClips.Count == 0)
		{
			allClips.Add(CreateNewClip());
			FilterClips();
		}
	}

	tk2dSpriteAnimationClip FindValidClip()
	{
		if (selectedClip != null && !selectedClip.Empty)
			return selectedClip;
		foreach (tk2dSpriteAnimationClip c in allClips)
			if (!c.Empty) return c;
		return null;
	}

	tk2dSpriteAnimationClip CreateNewClip()
	{
		// Find a unique name
		string uniqueName = tk2dEditor.SpriteAnimationEditor.AnimOperatorUtil.UniqueClipName(allClips, "New Clip");
		
		tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip();
		clip.name = uniqueName;

		tk2dSpriteAnimationClip source = FindValidClip();
		clip.frames = new tk2dSpriteAnimationFrame[1];
		clip.frames[0] = new tk2dSpriteAnimationFrame();
		if (source != null)
		{
			clip.frames[0].CopyFrom(source.frames[0]);
		}
		else
		{
			clip.frames[0].spriteCollection = tk2dSpriteGuiUtility.GetDefaultSpriteCollection();
			clip.frames[0].spriteId = clip.frames[0].spriteCollection.FirstValidDefinitionIndex;
		}

		bool inserted = false;
		for (int i = 0; i < allClips.Count; ++i)
		{
			if (allClips[i].Empty) 
			{
				allClips[i] = clip;
				inserted = true;
				break;
			}
		}
		if (!inserted)
			allClips.Add(clip);

		searchFilter = "";
		FilterClips();
		return clip;
	}

	void DrawToolbar()
	{
		GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
		
		// LHS
		GUILayout.BeginHorizontal(GUILayout.Width(leftBarWidth - 6));
		
		// Create Button
		GUIContent createButton = new GUIContent("Create");
		Rect createButtonRect = GUILayoutUtility.GetRect(createButton, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
		if (GUI.Button(createButtonRect, createButton, EditorStyles.toolbarDropDown) && anim != null)
		{
			GUIUtility.hotControl = 0;
			EditorUtility.DisplayCustomMenu(createButtonRect, menuItems, -1, 
				delegate(object userData, string[] options, int selected) {
					if (selected == 0)
					{
							selectedClip = CreateNewClip();
							clipEditor.Clip = selectedClip;
							clipEditor.InitForNewClip();
							Repaint();
					}
					else if (menuTargets[selected] != null)
					{
						tk2dEditor.SpriteAnimationEditor.AnimOperator animOp = menuTargets[selected];
						tk2dSpriteAnimationClip newSelectedClip = animOp.OnAnimMenu(options[selected], allClips, selectedClip);
						if (selectedClip != newSelectedClip)
						{
							selectedClip = newSelectedClip;
							clipEditor.Clip = selectedClip;
						}

						if ((animOp.AnimEditOperations & tk2dEditor.SpriteAnimationEditor.AnimEditOperations.AllClipsChanged) != tk2dEditor.SpriteAnimationEditor.AnimEditOperations.None)
						{
							FilterClips();
							Repaint();
						}
						if ((animOp.AnimEditOperations & tk2dEditor.SpriteAnimationEditor.AnimEditOperations.NewClipCreated) != tk2dEditor.SpriteAnimationEditor.AnimEditOperations.None)
						{
							clipEditor.InitForNewClip();
							Repaint();
						}
					}
				}
				, null);
		}
		
		// Filter box
		if (anim != null)
		{
			GUILayout.Space(8);
			string newSearchFilter = GUILayout.TextField(searchFilter, tk2dEditorSkin.ToolbarSearch, GUILayout.ExpandWidth(true));
			if (newSearchFilter != searchFilter)
			{
				searchFilter = newSearchFilter;
				FilterClips();
			}
			if (searchFilter.Length > 0)
			{
				if (GUILayout.Button("", tk2dEditorSkin.ToolbarSearchClear, GUILayout.ExpandWidth(false)))
				{
					searchFilter = "";
					FilterClips();
				}
			}
			else
			{
				GUILayout.Label("", tk2dEditorSkin.ToolbarSearchRightCap);
			}
		}
	
		GUILayout.EndHorizontal();
		
		// Label
		if (anim != null)
		{
			if (GUILayout.Button(anim.name, EditorStyles.label))
				EditorGUIUtility.PingObject(anim);
		}
		
		// RHS
		GUILayout.FlexibleSpace();
		
		if (anim != null && GUILayout.Button("Revert", EditorStyles.toolbarButton))
			Revert();
		
		if (anim != null && GUILayout.Button("Commit", EditorStyles.toolbarButton))
			Commit();
		
		GUILayout.EndHorizontal();
	}

	void OnGUI()
	{
		DrawToolbar();
		
		if (anim != null)
		{
			GUILayout.BeginHorizontal();
			DrawList();
	
			clipEditor.Clip = selectedClip;
			clipEditor.Draw(Screen.width - leftBarWidth - 2);
	
			GUILayout.EndHorizontal();
		}
	}
}
