using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dSlicedSprite))]
class tk2dSlicedSpriteEditor : tk2dSpriteEditor
{
	tk2dSlicedSprite[] targetSlicedSprites = new tk2dSlicedSprite[0];

	new void OnEnable() {
		base.OnEnable();
		targetSlicedSprites = GetTargetsOfType<tk2dSlicedSprite>( targets );
	}

	public override void OnInspectorGUI()
	{
		tk2dSlicedSprite sprite = (tk2dSlicedSprite)target;
		base.OnInspectorGUI();
		
		if (sprite.Collection == null)
			return;

		
		var spriteData = sprite.GetCurrentSpriteDef();
		if (spriteData == null) {
			return;
		}
		
		EditorGUILayout.BeginVertical();
		
		// need raw extents (excluding scale)
		Vector3 extents = spriteData.boundsData[1];
		
		// this is the size of one texel
		Vector3 spritePixelMultiplier = new Vector3(0, 0);

		bool newCreateBoxCollider = EditorGUILayout.Toggle("Create Box Collider", sprite.CreateBoxCollider);
		if (newCreateBoxCollider != sprite.CreateBoxCollider) {
			Undo.RegisterUndo(targetSlicedSprites, "Create Box Collider");
			sprite.CreateBoxCollider = newCreateBoxCollider;
		}
		
		// if either of these are zero, the division to rescale to pixels will result in a
		// div0, so display the data in fractions to avoid this situation
		bool editBorderInFractions = true;
		if (spriteData.texelSize.x != 0.0f && spriteData.texelSize.y != 0.0f && extents.x != 0.0f && extents.y != 0.0f)
		{
			spritePixelMultiplier = new Vector3(extents.x / spriteData.texelSize.x, extents.y / spriteData.texelSize.y, 1);
			editBorderInFractions = false;
		}
		
		if (!editBorderInFractions)
		{
			Vector2 newDimensions = EditorGUILayout.Vector2Field("Dimensions (Pixel Units)", sprite.dimensions);
			if (newDimensions != sprite.dimensions) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite Dimensions");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) {
					spr.dimensions = newDimensions;
				}
			}
			
			tk2dSlicedSprite.Anchor newAnchor = (tk2dSlicedSprite.Anchor)EditorGUILayout.EnumPopup("Anchor", sprite.anchor);
			if (newAnchor != sprite.anchor) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite Anchor");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) {
					spr.anchor = newAnchor;
				}
			}
			
			EditorGUILayout.PrefixLabel("Border");
			EditorGUI.indentLevel++;

			float newBorderLeft = EditorGUILayout.FloatField("Left", sprite.borderLeft * spritePixelMultiplier.x) / spritePixelMultiplier.x;
			if (newBorderLeft != sprite.borderLeft) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderLeft");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderLeft = newBorderLeft;
			}
			float newBorderRight = EditorGUILayout.FloatField("Right", sprite.borderRight * spritePixelMultiplier.x) / spritePixelMultiplier.x;
			if (newBorderRight != sprite.borderRight) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderRight");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderRight = newBorderRight;
			}
			float newBorderTop = EditorGUILayout.FloatField("Top", sprite.borderTop * spritePixelMultiplier.y) / spritePixelMultiplier.y;
			if (newBorderTop != sprite.borderTop) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderTop");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderTop = newBorderTop;
			}
			float newBorderBottom = EditorGUILayout.FloatField("Bottom", sprite.borderBottom * spritePixelMultiplier.y) / spritePixelMultiplier.y;
			if (newBorderBottom != sprite.borderBottom) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderBottom");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderBottom = newBorderBottom;
			}

			bool newBorderOnly = EditorGUILayout.Toggle("Draw Border Only", sprite.BorderOnly);
			if (newBorderOnly != sprite.BorderOnly) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite Border Only");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.BorderOnly = newBorderOnly;
			}

			EditorGUI.indentLevel--;
		}
		else
		{
			GUILayout.Label("Border (Displayed as Fraction).\nSprite Collection needs to be rebuilt.", "textarea");
			EditorGUI.indentLevel++;

			float newBorderLeft = EditorGUILayout.FloatField("Left", sprite.borderLeft);
			if (newBorderLeft != sprite.borderLeft) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderLeft");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderLeft = newBorderLeft;
			}
			float newBorderRight = EditorGUILayout.FloatField("Right", sprite.borderRight);
			if (newBorderRight != sprite.borderRight) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderRight");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderRight = newBorderRight;
			}
			float newBorderTop = EditorGUILayout.FloatField("Top", sprite.borderTop);
			if (newBorderTop != sprite.borderTop) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderTop");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderTop = newBorderTop;
			}
			float newBorderBottom = EditorGUILayout.FloatField("Bottom", sprite.borderBottom);
			if (newBorderBottom != sprite.borderBottom) {
				Undo.RegisterUndo(targetSlicedSprites, "Sliced Sprite BorderBottom");
				foreach (tk2dSlicedSprite spr in targetSlicedSprites) spr.borderBottom = newBorderBottom;
			}

			EditorGUI.indentLevel--;
		}

		// One of the border valus has changed, so simply rebuild mesh data here		
		if (GUI.changed)
		{
			foreach (tk2dSlicedSprite spr in targetSlicedSprites) {
				spr.Build();
				EditorUtility.SetDirty(spr);
			}
		}

		EditorGUILayout.EndVertical();
	}

	public new void OnSceneGUI() {
		if (tk2dPreferences.inst.enableSpriteHandles == false) return;

		tk2dSlicedSprite spr = (tk2dSlicedSprite)target;
		var sprite = spr.CurrentSprite;
		if (sprite == null) {
			return;
		}
		
		Transform t = spr.transform;
		Vector2 meshSize = new Vector2(spr.dimensions.x * sprite.texelSize.x * spr.scale.x, spr.dimensions.y * sprite.texelSize.y * spr.scale.y);
		Vector2 localRectOrig = tk2dSceneHelper.GetAnchorOffset(meshSize, spr.anchor);
		Rect localRect = new Rect(localRectOrig.x, localRectOrig.y, meshSize.x, meshSize.y);

		// Draw rect outline
		Handles.color = new Color(1,1,1,0.5f);
		tk2dSceneHelper.DrawRect (localRect, t);

		Handles.BeginGUI ();

		// Resize handles
		if (tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck ();
			Rect resizeRect = tk2dSceneHelper.RectControl(123192, localRect, t);
			if (EditorGUI.EndChangeCheck ()) {
				Undo.RegisterUndo (new Object[] {t, spr}, "Resize");
				spr.ReshapeBounds(new Vector3(resizeRect.xMin, resizeRect.yMin) - new Vector3(localRect.xMin, localRect.yMin),
					new Vector3(resizeRect.xMax, resizeRect.yMax) - new Vector3(localRect.xMax, localRect.yMax));
				EditorUtility.SetDirty(spr);
			}
		}
		// Rotate handles
		if (!tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck();
			List<int> hidePts = tk2dSceneHelper.getAnchorHidePtList(spr.anchor, localRect, t);
			float theta = tk2dSceneHelper.RectRotateControl( 456384, localRect, t, hidePts );
			if (EditorGUI.EndChangeCheck()) {
				Undo.RegisterUndo(t, "Rotate");
				if (Mathf.Abs(theta) > Mathf.Epsilon) {
					t.Rotate(t.forward, theta, Space.World);
				}
			}
		}

		Handles.EndGUI ();

		// Sprite selecting
		tk2dSceneHelper.HandleSelectSprites();

		// Move targeted sprites
		tk2dSceneHelper.HandleMoveSprites(t, localRect);

    	if (GUI.changed) {
    		EditorUtility.SetDirty(target);
    	}
	}

	[MenuItem("GameObject/Create Other/tk2d/Sliced Sprite", false, 12901)]
	static void DoCreateSlicedSpriteObject()
	{
		tk2dSpriteGuiUtility.GetSpriteCollectionAndCreate( (sprColl) => {
			GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Sliced Sprite");
			tk2dSlicedSprite sprite = go.AddComponent<tk2dSlicedSprite>();
			sprite.SetSprite(sprColl, sprColl.FirstValidDefinitionIndex);
			sprite.Build();
			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Create Sliced Sprite");
		} );
	}
}

