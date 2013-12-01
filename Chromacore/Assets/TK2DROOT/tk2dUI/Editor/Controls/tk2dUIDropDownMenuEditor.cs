using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIDropDownMenu))]
public class tk2dUIDropDownMenuEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();
        base.OnInspectorGUI();

		tk2dUIDropDownMenu dropdownMenu = (tk2dUIDropDownMenu)target;
		dropdownMenu.MenuLayoutItem = EditorGUILayout.ObjectField("Menu LayoutItem", dropdownMenu.MenuLayoutItem, typeof(tk2dUILayout), true) as tk2dUILayout;
		dropdownMenu.TemplateLayoutItem = EditorGUILayout.ObjectField("Template LayoutItem", dropdownMenu.TemplateLayoutItem, typeof(tk2dUILayout), true) as tk2dUILayout;

		if (dropdownMenu.MenuLayoutItem == null)
			dropdownMenu.height = EditorGUILayout.FloatField("Height", dropdownMenu.height, GUILayout.ExpandWidth(false));

        tk2dUIMethodBindingHelper methodBindingUtil = new tk2dUIMethodBindingHelper();
        dropdownMenu.SendMessageTarget = methodBindingUtil.BeginMessageGUI(dropdownMenu.SendMessageTarget);
        methodBindingUtil.MethodBinding( "On Selected Item Change", typeof(tk2dUIDropDownMenu), dropdownMenu.SendMessageTarget, ref dropdownMenu.SendMessageOnSelectedItemChangeMethodName );
        methodBindingUtil.EndMessageGUI();

		if (GUI.changed) {
			EditorUtility.SetDirty(target);
		}
    }

    public void OnSceneGUI()
    {
        bool wasChange=false;
        tk2dUIDropDownMenu dropdownMenu = (tk2dUIDropDownMenu)target;
        tk2dUIDropDownItem dropdownItemTemplate = dropdownMenu.dropDownItemTemplate;

		// Get rescaled transforms
        Matrix4x4 m = dropdownMenu.transform.localToWorldMatrix;
		Vector3 up = m.MultiplyVector(Vector3.up);
		// Vector3 right = m.MultiplyVector(Vector3.right);

		if (dropdownMenu.MenuLayoutItem == null) {
			float newDropDownButtonHeight = tk2dUIControlsHelperEditor.DrawLengthHandles("Dropdown Button Height", dropdownMenu.height, dropdownMenu.transform.position+(up*(dropdownMenu.height/2)), -up, Color.red,.15f, .3f, .05f);
			if (newDropDownButtonHeight != dropdownMenu.height)
			{
				Undo.RegisterUndo(dropdownMenu, "Dropdown Button Height Changed");
				dropdownMenu.height = newDropDownButtonHeight;
				wasChange = true;
			}
		}

        if (dropdownItemTemplate != null)
        {
			float yPosDropdownItemTemplate = (dropdownMenu.MenuLayoutItem != null) ? dropdownMenu.MenuLayoutItem.bMin.y : (-dropdownMenu.height);

			if (dropdownItemTemplate.transform.localPosition.y != yPosDropdownItemTemplate)
			{
				dropdownItemTemplate.transform.localPosition = new Vector3(dropdownItemTemplate.transform.localPosition.x, yPosDropdownItemTemplate, dropdownItemTemplate.transform.localPosition.z);
				EditorUtility.SetDirty(dropdownItemTemplate.transform);
			}

			if (dropdownMenu.TemplateLayoutItem == null) {
				float newDropDownItemTemplateHeight = tk2dUIControlsHelperEditor.DrawLengthHandles("Dropdown Item Template Height", dropdownItemTemplate.height, dropdownMenu.transform.position - (up * (dropdownMenu.height/2)), -up, Color.blue, .15f, .4f, .05f);
				if (newDropDownItemTemplateHeight != dropdownItemTemplate.height)
				{
					Undo.RegisterUndo(dropdownItemTemplate, "Dropdown Template Height Changed");
					dropdownItemTemplate.height = newDropDownItemTemplateHeight;
					EditorUtility.SetDirty(dropdownItemTemplate);
				}
			}
        }

        if (wasChange)
        {
            EditorUtility.SetDirty(dropdownMenu);
        }
    }

}
