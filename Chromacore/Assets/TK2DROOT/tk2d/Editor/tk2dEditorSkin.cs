using UnityEngine;
using UnityEditor;
using System.Collections;

public class tk2dEditorSkin
{
	static bool isProSkin;
	
	// Sprite collection editor styles
	public static void Init()
	{
		if (isProSkin != EditorGUIUtility.isProSkin)
		{
			tk2dExternal.Skin.Done();
			isProSkin = EditorGUIUtility.isProSkin;
		}
	}

	public static Texture2D GetTexture(string name) {
		return tk2dExternal.Skin.Inst.GetTexture(name);
	}

	public static GUIStyle GetStyle(string name) {
		return tk2dExternal.Skin.Inst.GetStyle(name);
	}

	public static GUIStyle SimpleButton(string textureInactive) {
		return SimpleButton(textureInactive, "");
	}

	public static GUIStyle SimpleButton(string textureInactive, string textureActive) {
		GUIStyle style = GetStyle("SimpleButtonTemplate");
		style.normal.background = GetTexture(textureInactive);
		style.active.background = string.IsNullOrEmpty(textureActive) ? null : GetTexture(textureActive);
		return style;
	}

	public static GUIStyle SimpleCheckbox(string textureInactive, string textureActive) {
		GUIStyle style = GetStyle("SimpleButtonTemplate");
		style.normal.background = GetTexture(textureInactive);
		style.onNormal.background = string.IsNullOrEmpty(textureActive) ? null : GetTexture(textureActive);
		return style;
	}

	public static void Done() {
		tk2dExternal.Skin.Done();
	}
	
	public static GUIStyle SC_InspectorBG { get { Init(); return GetStyle("InspectorBG"); } }
	public static GUIStyle SC_InspectorHeaderBG { get { Init(); return GetStyle("InspectorHeaderBG"); } }
	public static GUIStyle SC_ListBoxBG { get { Init(); return GetStyle("ListBoxBG"); } }
	public static GUIStyle SC_ListBoxItem { get { Init(); return GetStyle("ListBoxItem"); } }
	public static GUIStyle SC_ListBoxSectionHeader { get { Init(); return GetStyle("ListBoxSectionHeader"); } }	
	public static GUIStyle SC_BodyBackground { get { Init(); return GetStyle("BodyBackground"); } }	
	public static GUIStyle SC_DropBox { get { Init(); return GetStyle("DropBox"); } }	
	
	public static GUIStyle ToolbarSearch { get { Init(); return GetStyle("ToolbarSearch"); } }
	public static GUIStyle ToolbarSearchClear { get { Init(); return GetStyle("ToolbarSearchClear"); } }
	public static GUIStyle ToolbarSearchRightCap { get { Init(); return GetStyle("ToolbarSearchRightCap"); } }

	public static GUIStyle Anim_BG { get { Init(); return GetStyle("AnimBG"); } }
	public static GUIStyle Anim_Trigger { get { Init(); return GetStyle("AnimTrigger"); } }
	public static GUIStyle Anim_TriggerSelected { get { Init(); return GetStyle("AnimTriggerDown"); } }

	public static GUIStyle MoveHandle { get { Init(); return GetStyle("MoveHandle"); } }
	public static GUIStyle RotateHandle { get { Init(); return GetStyle("RotateHandle"); } }
	
	public static GUIStyle WhiteBox { get { Init(); return GetStyle("WhiteBox"); } }
	public static GUIStyle Selection { get { Init(); return GetStyle("Selection"); } }
}
