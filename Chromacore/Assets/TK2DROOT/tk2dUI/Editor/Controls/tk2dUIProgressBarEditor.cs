using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dUIProgressBar))]
public class tk2dUIProgressBarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUIUtility.LookLikeInspector();
        bool markAsDirty = false;
        tk2dUIProgressBar progressBar = (tk2dUIProgressBar)target;

        if (progressBar.clippedSpriteBar != null) //can only be one
        {
            progressBar.scalableBar = null;
            progressBar.slicedSpriteBar = null;
        }

        if (progressBar.slicedSpriteBar != null) 
        {
            progressBar.clippedSpriteBar = null;
            progressBar.scalableBar = null;
        }

        tk2dClippedSprite tempClippedSpriteBar = tk2dUICustomEditorGUILayout.SceneObjectField("Clipped Sprite Bar", progressBar.clippedSpriteBar, target);
        if (tempClippedSpriteBar != progressBar.clippedSpriteBar)
        {
            markAsDirty = true;
            progressBar.clippedSpriteBar = tempClippedSpriteBar;
            progressBar.scalableBar = null; //can only be one
            progressBar.slicedSpriteBar = null;
        }

        tk2dSlicedSprite tempSlicedSpriteBar = tk2dUICustomEditorGUILayout.SceneObjectField("Sliced Sprite Bar", progressBar.slicedSpriteBar, target);
        if (tempSlicedSpriteBar != progressBar.slicedSpriteBar)
        {
            markAsDirty = true;
            progressBar.slicedSpriteBar = tempSlicedSpriteBar;
            progressBar.scalableBar = null; //can only be one
            progressBar.clippedSpriteBar = null;
        }

        Transform tempScalableBar = tk2dUICustomEditorGUILayout.SceneObjectField("Scalable Bar", progressBar.scalableBar,target);
        if (tempScalableBar != progressBar.scalableBar)
        {
            markAsDirty = true;
            progressBar.scalableBar = tempScalableBar;
            progressBar.clippedSpriteBar = null; //can only be one
            progressBar.slicedSpriteBar = null;
        }

        float tempPercent = EditorGUILayout.FloatField("Value", progressBar.Value);
        if (tempPercent != progressBar.Value)
        {
            markAsDirty = true;
            progressBar.Value = tempPercent;
        }

        tk2dUIMethodBindingHelper methodBindingUtil = new tk2dUIMethodBindingHelper();
        progressBar.sendMessageTarget = methodBindingUtil.BeginMessageGUI(progressBar.sendMessageTarget);
        methodBindingUtil.MethodBinding( "On Progress Complete", typeof(tk2dUIProgressBar), progressBar.sendMessageTarget, ref progressBar.SendMessageOnProgressCompleteMethodName );
        methodBindingUtil.EndMessageGUI();

        if (markAsDirty || GUI.changed)
        {
            EditorUtility.SetDirty(progressBar);
        }
    }

}
