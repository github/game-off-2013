using UnityEngine;
using UnityEditor;
using System.Collections;

public class tk2dUIControlsHelperEditor : Editor
{
    public static float DrawLengthHandles(string labelText, float currentLength, Vector3 startPos, Vector3 dir, Color handleColor, float smallBarLength, float offset, float textOffset)
    {
        float newLength = currentLength;
        Vector3 right = Vector3.Cross(Vector3.forward, dir);
        Vector3 centerPosTop = startPos - right * (smallBarLength + offset);
        Vector3 centerPosBottom = centerPosTop + dir * currentLength;

        Color transparentHandleColor = handleColor;
        transparentHandleColor.a = 170 / 255f;

        bool oldChanged = GUI.changed;
        GUI.changed = false;

        Handles.color = handleColor;
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = handleColor;
        Handles.Label(centerPosTop - dir * textOffset - right * .1f, labelText, labelStyle);
        Handles.DrawLine(centerPosTop - right * (smallBarLength / 2), centerPosTop + right * (smallBarLength / 2));
        Handles.color = transparentHandleColor;
        Handles.DrawLine(centerPosTop, centerPosTop + dir * currentLength);
        Handles.color = handleColor;
        Handles.DrawLine(centerPosBottom - right * (smallBarLength / 2), centerPosBottom + right * (smallBarLength / 2));

        string controlName = labelText;
        GUI.SetNextControlName(controlName);
        Vector3 resultSliderPos = Handles.Slider(centerPosBottom, dir);

        if (GUI.GetNameOfFocusedControl() == controlName) {
            // Draw extended lines
            Color faintHandleColor = handleColor;
            faintHandleColor.a = 90 / 255.0f;
            Handles.color = faintHandleColor;
            float longBarLength = 1000.0f;
            Handles.DrawLine(centerPosTop - right * longBarLength, centerPosTop + right * longBarLength);
            Handles.DrawLine(centerPosBottom - right * longBarLength, centerPosBottom + right * longBarLength);
        }

        Handles.color = Color.white;

        if (GUI.changed) {
            newLength = (centerPosTop - resultSliderPos).magnitude / dir.magnitude;
        }
        GUI.changed |= oldChanged;

        return newLength;
    }

}
