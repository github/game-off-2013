using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class tk2dGuiUtility  
{
	public static bool HasActivePositionHandle { get { return activePositionHandleId != 0; } }
	public static Vector2 ActiveHandlePosition { get { return activePositionHandlePosition; } }
	
	static int activePositionHandleId = 0;
	static Vector2 activePositionHandlePosition = Vector2.zero;
	static Vector2 positionHandleOffset = Vector2.zero;
	
	public static void SetPositionHandleValue(int id, Vector2 val)
	{
		if (id == activePositionHandleId)
			activePositionHandlePosition = val;
	}
	
	public static Vector2 PositionHandle(int id, Vector2 position)
	{
		return Handle(tk2dEditorSkin.MoveHandle, id, position, false);
	}
	
	public static Vector2 Handle(GUIStyle style, int id, Vector2 position, bool allowKeyboardFocus)
	{
		int handleSize = (int)style.fixedWidth;
		Rect rect = new Rect(position.x - handleSize / 2, position.y - handleSize / 2, handleSize, handleSize);
		int controlID = id;
		
		switch (Event.current.GetTypeForControl(controlID))
		{
			case EventType.MouseDown:
			{
				if (rect.Contains(Event.current.mousePosition))
				{
					activePositionHandleId = id;
					if (allowKeyboardFocus) {
						GUIUtility.keyboardControl = controlID;
					}
					positionHandleOffset = Event.current.mousePosition - position;
					GUIUtility.hotControl = controlID;
					Event.current.Use();
				}
				break;
			}
			
			case EventType.MouseDrag:
			{
				if (GUIUtility.hotControl == controlID)				
				{
					position = Event.current.mousePosition - positionHandleOffset;
					Event.current.Use();					
				}
				break;
			}
			
			case EventType.MouseUp:
			{
				if (GUIUtility.hotControl == controlID)
				{
					activePositionHandleId = 0;
					position = Event.current.mousePosition - positionHandleOffset;
					GUIUtility.hotControl = 0;
					Event.current.Use();
				}
				break;
			}
			
			case EventType.Repaint:
			{
				bool selected = (GUIUtility.keyboardControl == controlID ||
								 GUIUtility.hotControl == controlID);
				style.Draw(rect, selected, false, false, false);
				break;
			}
		}
		
		return position;
	}
	
	public enum WarningLevel
	{
		Info,
		Warning,
		Error
	}
	
	/// <summary>
	/// Display a warning box in the current GUI layout.
	/// This is expanded to fit the current GUILayout rect.
	/// </summary>
	public static void InfoBox(string message, WarningLevel warningLevel)
	{
		MessageType messageType = MessageType.None;
		switch (warningLevel)
		{
			case WarningLevel.Info: messageType = MessageType.Info; break;
			case WarningLevel.Warning: messageType = MessageType.Warning; break;
			case WarningLevel.Error: messageType = MessageType.Error; break;
		}

		EditorGUILayout.HelpBox(message, messageType);
	}
	
	/// <summary>
	/// Displays a warning box in the current GUI layout, with buttons.
	/// Returns the index of button pressed, or -1 otherwise.
	/// </summary>
	public static int InfoBoxWithButtons(string message, WarningLevel warningLevel, params string[] buttons)
	{
		InfoBox(message, warningLevel);

		Color oldBackgroundColor = GUI.backgroundColor;
		switch (warningLevel)
		{
		case WarningLevel.Info: GUI.backgroundColor = new Color32(154, 176, 203, 255); break;
		case WarningLevel.Warning: GUI.backgroundColor = new Color32(255, 255, 0, 255); break;
		case WarningLevel.Error: GUI.backgroundColor = new Color32(255, 0, 0, 255); break;
		}

		int buttonPressed = -1;
		if (buttons != null)
		{
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			for (int i = 0; i < buttons.Length; ++i)
			{
				if (GUILayout.Button(buttons[i], EditorStyles.miniButton))
					buttonPressed = i;
			}
			GUILayout.EndHorizontal();
		}
		GUI.backgroundColor = oldBackgroundColor;
		return buttonPressed;
	}

	public enum DragDirection
	{
		Horizontal,
	}
	// Size is the offset into the rect to draw the DragableHandle
	const float resizeBarHotSpotSize = 2.0f;
	public static float DragableHandle(int id, Rect windowRect, float offset, DragDirection direction)
	{
		int controlID = GUIUtility.GetControlID(id, FocusType.Passive);

		Vector2 positionFilter = Vector2.zero;
		Rect controlRect = windowRect;
		switch (direction)
		{
			case DragDirection.Horizontal: 
				controlRect = new Rect(controlRect.x + offset - resizeBarHotSpotSize, 
									   controlRect.y, 
									   resizeBarHotSpotSize * 2 + 1.0f, 
									   controlRect.height); 
				positionFilter.x = 1.0f;
				break;
		}
		EditorGUIUtility.AddCursorRect(controlRect, MouseCursor.ResizeHorizontal);

		if (GUIUtility.hotControl == 0)
		{
			if (Event.current.type == EventType.MouseDown && controlRect.Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl = controlID;
				Event.current.Use();
			}
		}
		else if (GUIUtility.hotControl == controlID)
		{
			if (Event.current.type == EventType.MouseDrag)
			{
				Vector2 mousePosition = Event.current.mousePosition;
				Vector2 handleOffset = new Vector2((mousePosition.x - windowRect.x) * positionFilter.x, 
												   (mousePosition.y - windowRect.y) * positionFilter.y);
				offset = handleOffset.x + handleOffset.y;
				HandleUtility.Repaint();
			}
			else if (Event.current.type == EventType.MouseUp)
			{
				GUIUtility.hotControl = 0;
			}
		}

		// Debug draw
		// GUI.Box(controlRect, "");

		return offset;
	}
	
	private static bool backupGuiChangedValue = false;
	public static void BeginChangeCheck()
	{
		backupGuiChangedValue = GUI.changed;
		GUI.changed = false;
	}
	
	public static bool EndChangeCheck()
	{
		bool hasChanged = GUI.changed;
		GUI.changed |= backupGuiChangedValue;
		return hasChanged;
	}

	public static void SpriteCollectionSize( tk2dSpriteCollectionSize scs ) {
		GUILayout.BeginHorizontal();
		scs.type = (tk2dSpriteCollectionSize.Type)EditorGUILayout.EnumPopup("Size", scs.type);
		tk2dCamera cam = tk2dCamera.Editor__Inst;
		GUI.enabled = cam != null;
		if (GUILayout.Button(new GUIContent("g", "Grab from tk2dCamera"), EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
			scs.CopyFrom( tk2dSpriteCollectionSize.ForTk2dCamera(cam) );
			GUI.changed = true;
		}
		GUI.enabled = true;
		GUILayout.EndHorizontal();
		EditorGUI.indentLevel++;
		switch (scs.type) {
			case tk2dSpriteCollectionSize.Type.Explicit:
				scs.orthoSize = EditorGUILayout.FloatField("Ortho Size", scs.orthoSize);
				scs.height = EditorGUILayout.FloatField("Target Height", scs.height);
				break;
			case tk2dSpriteCollectionSize.Type.PixelsPerMeter:
				scs.pixelsPerMeter = EditorGUILayout.FloatField("Pixels Per Meter", scs.pixelsPerMeter);
				break;
		}
		EditorGUI.indentLevel--;
	}

	public static string PlatformPopup(tk2dSystem system, string label, string platform)
	{
		if (system == null)
			return label;

		int selectedIndex = -1;
		string[] platformNames = new string[system.assetPlatforms.Length];

		for (int i = 0; i < system.assetPlatforms.Length; ++i)
		{
			platformNames[i] = system.assetPlatforms[i].name;
			if (platformNames[i] == platform) selectedIndex = i;
		}

		selectedIndex = EditorGUILayout.Popup(label, selectedIndex, platformNames);
		if (selectedIndex == -1) return "";
		else return platformNames[selectedIndex];
	}

	public static string SaveFileInProject(string title, string directory, string filename, string ext)
	{
		string path = EditorUtility.SaveFilePanel(title, directory, filename, ext);
		if (path.Length == 0) // cancelled
			return "";
		string cwd = System.IO.Directory.GetCurrentDirectory().Replace("\\","/") + "/assets/";
		if (path.ToLower().IndexOf(cwd.ToLower()) != 0)
		{
			path = "";
			EditorUtility.DisplayDialog(title, "Assets must be saved inside the Assets folder", "Ok");
		}
		else 
		{
			path = path.Substring(cwd.Length - "/assets".Length);
		}
		return path;
	}
}
