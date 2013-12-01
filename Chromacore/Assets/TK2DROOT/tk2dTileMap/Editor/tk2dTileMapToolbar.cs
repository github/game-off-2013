using UnityEngine;
using UnityEditor;

public static class tk2dTileMapToolbar
{
	// ---- Shared

	public static string tooltip = "";

	static GUIStyle toolbarButtonStyle {
		get {
			return tk2dEditorSkin.GetStyle("TilemapToolbarButton");
		}
	}

	static GUIStyle toolbarButtonLeftStyle {
		get {
			return tk2dEditorSkin.GetStyle("TilemapToolbarButtonLeft");
		}
	}

	static GUIStyle toolbarButtonRightStyle {
		get {
			return tk2dEditorSkin.GetStyle("TilemapToolbarButtonRight");
		}
	}

	static bool HiliteBtn(Texture2D texture, bool hilite, Color hiliteColor, string tooltip) {
		hiliteColor.a = 1.0f;
		if (hilite) { 
			GUI.contentColor = (hiliteColor + new Color(0.3f,0.3f,0.3f, 0));
			GUI.backgroundColor = (hiliteColor + new Color(0.3f,0.3f,0.3f, 0));
		}
		else {
			GUI.contentColor = Color.white;
			GUI.backgroundColor = Color.gray;
		}
		bool pressed = GUILayout.Button(new GUIContent(texture, tooltip), toolbarButtonStyle);

		GUI.backgroundColor = Color.white;
		GUI.contentColor = Color.white;

		return pressed;
	}

	static bool ToggleBtn(bool toggled, Texture2D texture, string tooltip, GUIStyle style) {
		if (toggled) { 
			GUI.contentColor = Color.white;
			GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
		}
		else {
			GUI.contentColor = Color.white;
			GUI.backgroundColor = Color.gray;
		}
		toggled = GUILayout.Toggle(toggled, new GUIContent(texture, tooltip), style);
		GUI.backgroundColor = Color.white;
		GUI.contentColor = Color.white;
		return toggled;
	}

	// ---- Paint

	public enum MainMode {
		Brush,
		BrushRandom,
		Erase,
		Eyedropper,
		Cut
	}

	public static MainMode mainMode = MainMode.Brush;
	public static bool workBrushFlipX = false;
	public static bool workBrushFlipY = false;
	public static bool workBrushRot90 = false;

	public static bool scratchpadOpen = false;

	public static float workBrushOpacity = 0.8f;

	public static void ToolbarWindow() {
		string pickupTooltipStr = (Application.platform == RuntimePlatform.OSXEditor) ? "Ctrl" : "Alt";
		string eraseTooltipStr = (Application.platform == RuntimePlatform.OSXEditor) ? "Command" : "Ctrl";

		Color lastColor = GUI.contentColor;

		GUILayout.BeginHorizontal();
		
		// Main modes

		if (HiliteBtn(tk2dEditorSkin.GetTexture("icon_pencil"), (mainMode == MainMode.Brush), tk2dPreferences.inst.tileMapToolColor_brush, "Draw")) mainMode = MainMode.Brush;
		GUILayout.Space (10);

		if (HiliteBtn(tk2dEditorSkin.GetTexture("icon_dice"), (mainMode == MainMode.BrushRandom), tk2dPreferences.inst.tileMapToolColor_brushRandom, "Draw Random")) mainMode = MainMode.BrushRandom;
		GUILayout.Space (10);
		
		if (HiliteBtn(tk2dEditorSkin.GetTexture("icon_eraser"), (mainMode == MainMode.Erase), tk2dPreferences.inst.tileMapToolColor_erase, "Erase (" + eraseTooltipStr + ")")) mainMode = MainMode.Erase;
		GUILayout.Space (10);
		
		if (HiliteBtn(tk2dEditorSkin.GetTexture("icon_eyedropper"), (mainMode == MainMode.Eyedropper), tk2dPreferences.inst.tileMapToolColor_eyedropper, "Eyedropper (" + pickupTooltipStr + ")")) mainMode = MainMode.Eyedropper;
		GUILayout.Space (10);
		
		if (HiliteBtn(tk2dEditorSkin.GetTexture("icon_scissors"), (mainMode == MainMode.Cut), tk2dPreferences.inst.tileMapToolColor_cut, "Cut (" + pickupTooltipStr + "+" + eraseTooltipStr + ")")) mainMode = MainMode.Cut;
		GUILayout.Space (20);

		// Flip workbrush

		GUI.contentColor = Color.white;
		workBrushFlipX = ToggleBtn(workBrushFlipX, tk2dEditorSkin.GetTexture("icon_fliph"), "Flip Horizontal (H)", toolbarButtonStyle);
		workBrushFlipY = ToggleBtn(workBrushFlipY, tk2dEditorSkin.GetTexture("icon_flipv"), "Flip Vertical (J)", toolbarButtonStyle);
		workBrushRot90 = ToggleBtn(workBrushRot90, tk2dEditorSkin.GetTexture("icon_pencil"), "Rotate 90", toolbarButtonStyle);
		GUILayout.Space (20);

		// Scratchpad

		if (scratchpadOpen) GUI.contentColor = Color.white;
		else GUI.contentColor = new Color(0.8f, 0.8f, 0.8f);
		scratchpadOpen = ToggleBtn(scratchpadOpen, tk2dEditorSkin.GetTexture("icon_scratchpad"), "Scratchpad (Tab)", toolbarButtonStyle);

		tooltip = GUI.tooltip;
		
		GUILayout.EndHorizontal();

		GUI.contentColor = lastColor;
	}

	// ---- Color

	public enum ColorChannelsMode {
		AllChannels,
		Red,
		Green,
		Blue
	}

	public enum ColorBlendMode {
		Replace,
		Addition,
		Subtraction,
		Eyedropper
	}

	public static Color colorBrushColor = Color.white;
	public static ColorChannelsMode colorChannelsMode = ColorChannelsMode.AllChannels;
	public static ColorBlendMode colorBlendMode = ColorBlendMode.Replace;
	public static float colorBrushRadius = 1.0f;
	public static float colorBrushIntensity = 1.0f;
	public static AnimationCurve colorBrushCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

	static ColorChannelsMode newColorChannelsMode = ColorChannelsMode.AllChannels;

	public static void ColorToolsWindow() {
		Color lastColor = GUI.contentColor;

		GUILayout.BeginVertical();

		// Channels
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Channel Mask");
			GUIContent[] contents = new GUIContent[] {
				new GUIContent("All", "All Channels"),
				new GUIContent("R", "Red"),
				new GUIContent("G", "Green"),
				new GUIContent("B", "Blue")
			};
			ColorChannelsMode temp = (ColorChannelsMode)GUILayout.Toolbar((int)colorChannelsMode, contents, toolbarButtonStyle);
			if (temp != colorChannelsMode)
				newColorChannelsMode = temp;
			GUILayout.EndHorizontal();
		}

		// Color
		if (colorChannelsMode == ColorChannelsMode.AllChannels) {
			colorBrushColor = EditorGUILayout.ColorField("Color", colorBrushColor);
		}

		// Blend
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Mode");
			GUIContent[] contents = new GUIContent[] {
				new GUIContent(tk2dEditorSkin.GetTexture("icon_pencil"), "Replace"),
				new GUIContent("+", "Addition"),
				new GUIContent("-", "Subtraction"),
				new GUIContent(tk2dEditorSkin.GetTexture("icon_eyedropper"), "Eyedropper")
			};
			colorBlendMode = (ColorBlendMode)GUILayout.Toolbar((int)colorBlendMode, contents, toolbarButtonStyle);
			GUILayout.EndHorizontal();
		}

		// Radius, Intensity
		{
			GUIContent radiusLabel = new GUIContent("Radius", "Shortcut - [, ]");
			colorBrushRadius = EditorGUILayout.Slider(radiusLabel, colorBrushRadius, 1.0f, 64.0f);

			GUIContent intensityLabel = new GUIContent("Intensity", "Shortcut - Minus (-), Plus (+)");
			colorBrushIntensity = EditorGUILayout.Slider(intensityLabel, colorBrushIntensity * 100.0f, 0.0f, 100.0f) / 100.0f;
		}

		// Curve
		{
			colorBrushCurve = EditorGUILayout.CurveField("Brush shape", colorBrushCurve);
		}

		GUILayout.EndVertical();

		GUI.contentColor = lastColor;

		if (Event.current.type == EventType.Repaint) {
			colorChannelsMode = newColorChannelsMode;
			HandleUtility.Repaint();
		}
	}
}