using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dSprite))]
class tk2dSpriteEditor : Editor
{
	// Serialized properties are going to be far too much hassle
	private tk2dBaseSprite[] targetSprites = new tk2dBaseSprite[0];

    public override void OnInspectorGUI()
    {
		DrawSpriteEditorGUI();
    }

    public void OnSceneGUI()
    {
		if (tk2dPreferences.inst.enableSpriteHandles == false) return;

    	tk2dSprite spr = (tk2dSprite)target;
		var sprite = spr.CurrentSprite;

		if (sprite == null) {
			return;
		}

		Transform t = spr.transform;
		Bounds b = spr.GetUntrimmedBounds();
		Rect localRect = new Rect(b.min.x, b.min.y, b.size.x, b.size.y);

		// Draw rect outline
		Handles.color = new Color(1,1,1,0.5f);
		tk2dSceneHelper.DrawRect (localRect, t);

		Handles.BeginGUI ();
		// Resize handles
		if (tk2dSceneHelper.RectControlsToggle ()) {
			EditorGUI.BeginChangeCheck ();
			Rect resizeRect = tk2dSceneHelper.RectControl (999888, localRect, t);
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
			float theta = tk2dSceneHelper.RectRotateControl (888999, localRect, t, new List<int>());
			if (EditorGUI.EndChangeCheck()) {
				Undo.RegisterUndo (t, "Rotate");
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

    protected T[] GetTargetsOfType<T>( Object[] objects ) where T : UnityEngine.Object {
    	List<T> ts = new List<T>();
    	foreach (Object o in objects) {
    		T s = o as T;
    		if (s != null)
    			ts.Add(s);
    	}
    	return ts.ToArray();
    }

    protected void OnEnable()
    {
    	targetSprites = GetTargetsOfType<tk2dBaseSprite>( targets );
    }
	
	void OnDestroy()
	{
		targetSprites = new tk2dBaseSprite[0];

		tk2dSpriteThumbnailCache.Done();
		tk2dGrid.Done();
		tk2dEditorSkin.Done();
	}
	
	// Callback and delegate
	void SpriteChangedCallbackImpl(tk2dSpriteCollectionData spriteCollection, int spriteId, object data)
	{
		Undo.RegisterUndo(targetSprites, "Sprite Change");
		
		foreach (tk2dBaseSprite s in targetSprites) {
			s.SetSprite(spriteCollection, spriteId);
			s.EditMode__CreateCollider();
			EditorUtility.SetDirty(s);
		}
	}
	tk2dSpriteGuiUtility.SpriteChangedCallback _spriteChangedCallbackInstance = null;
	tk2dSpriteGuiUtility.SpriteChangedCallback spriteChangedCallbackInstance {
		get {
			if (_spriteChangedCallbackInstance == null) {
				_spriteChangedCallbackInstance = new tk2dSpriteGuiUtility.SpriteChangedCallback( SpriteChangedCallbackImpl );
			}
			return _spriteChangedCallbackInstance;
		}
	}

	protected void DrawSpriteEditorGUI()
	{
		Event ev = Event.current;
		tk2dSpriteGuiUtility.SpriteSelector( targetSprites[0].Collection, targetSprites[0].spriteId, spriteChangedCallbackInstance, null );

        if (targetSprites[0].Collection != null)
        {
        	if (tk2dPreferences.inst.displayTextureThumbs) {
        		tk2dBaseSprite sprite = targetSprites[0];
				tk2dSpriteDefinition def = sprite.GetCurrentSpriteDef();
				if (sprite.Collection.version < 1 || def.texelSize == Vector2.zero)
				{
					string message = "";
					
					message = "No thumbnail data.";
					if (sprite.Collection.version < 1)
						message += "\nPlease rebuild Sprite Collection.";
					
					tk2dGuiUtility.InfoBox(message, tk2dGuiUtility.WarningLevel.Info);
				}
				else
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel(" ");

					int tileSize = 128;
					Rect r = GUILayoutUtility.GetRect(tileSize, tileSize, GUILayout.ExpandWidth(false));
					tk2dGrid.Draw(r);
					tk2dSpriteThumbnailCache.DrawSpriteTextureInRect(r, def, Color.white);

					GUILayout.EndHorizontal();

					r = GUILayoutUtility.GetLastRect();
					if (ev.type == EventType.MouseDown && ev.button == 0 && r.Contains(ev.mousePosition)) {
						tk2dSpriteGuiUtility.SpriteSelectorPopup( sprite.Collection, sprite.spriteId, spriteChangedCallbackInstance, null );
					}
				}
			}

            Color newColor = EditorGUILayout.ColorField("Color", targetSprites[0].color);
            if (newColor != targetSprites[0].color) {
            	Undo.RegisterUndo(targetSprites, "Sprite Color");
            	foreach (tk2dBaseSprite s in targetSprites) {
            		s.color = newColor;
            	}
            }

			int sortingOrder = EditorGUILayout.IntField("Sorting Order In Layer", targetSprites[0].SortingOrder);
			if (sortingOrder != targetSprites[0].SortingOrder) {
            	Undo.RegisterUndo(targetSprites, "Sorting Order In Layer");
            	foreach (tk2dBaseSprite s in targetSprites) {
            		s.SortingOrder = sortingOrder;
            	}
			}

			Vector3 newScale = EditorGUILayout.Vector3Field("Scale", targetSprites[0].scale);
			if (newScale != targetSprites[0].scale)
			{
				Undo.RegisterUndo(targetSprites, "Sprite Scale");
				foreach (tk2dBaseSprite s in targetSprites) {
					s.scale = newScale;
					s.EditMode__CreateCollider();
				}
			}
			
			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button("HFlip", EditorStyles.miniButton))
			{
				Undo.RegisterUndo(targetSprites, "Sprite HFlip");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					sprite.EditMode__CreateCollider();
					Vector3 scale = sprite.scale;
					scale.x *= -1.0f;
					sprite.scale = scale;
				}
				GUI.changed = true;
			}
			if (GUILayout.Button("VFlip", EditorStyles.miniButton))
			{
				Undo.RegisterUndo(targetSprites, "Sprite VFlip");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					Vector3 s = sprite.scale;
					s.y *= -1.0f;
					sprite.scale = s;
					GUI.changed = true;
				}
			}
			
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			
			if (GUILayout.Button(new GUIContent("Reset Scale", "Set scale to 1"), EditorStyles.miniButton))
			{
				Undo.RegisterUndo(targetSprites, "Sprite Reset Scale");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					Vector3 s = sprite.scale;
					s.x = Mathf.Sign(s.x);
					s.y = Mathf.Sign(s.y);
					s.z = Mathf.Sign(s.z);
					sprite.scale = s;
					GUI.changed = true;
				}
			}
			
			if (GUILayout.Button(new GUIContent("Bake Scale", "Transfer scale from transform.scale -> sprite"), EditorStyles.miniButton))
			{
				Undo.RegisterSceneUndo("Bake Scale");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					tk2dScaleUtility.Bake(sprite.transform);
				}
				GUI.changed = true;
			}
			
			GUIContent pixelPerfectButton = new GUIContent("1:1", "Make Pixel Perfect for camera");
			if ( GUILayout.Button(pixelPerfectButton, EditorStyles.miniButton ))
			{
				if (tk2dPixelPerfectHelper.inst) tk2dPixelPerfectHelper.inst.Setup();
				Undo.RegisterUndo(targetSprites, "Sprite Pixel Perfect");
				foreach (tk2dBaseSprite sprite in targetSprites) {
					sprite.MakePixelPerfect();
				}
				GUI.changed = true;
			}
			
			EditorGUILayout.EndHorizontal();
        }
        else
        {
			tk2dGuiUtility.InfoBox("Please select a sprite collection.", tk2dGuiUtility.WarningLevel.Error);        
		}


		bool needUpdatePrefabs = false;
		if (GUI.changed)
		{
			foreach (tk2dBaseSprite sprite in targetSprites) {
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			if (PrefabUtility.GetPrefabType(sprite) == PrefabType.Prefab)
				needUpdatePrefabs = true;
#endif
				EditorUtility.SetDirty(sprite);
			}
		}
		
		// This is a prefab, and changes need to be propagated. This isn't supported in Unity 3.4
		if (needUpdatePrefabs)
		{
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			// Rebuild prefab instances
			tk2dBaseSprite[] allSprites = Resources.FindObjectsOfTypeAll(typeof(tk2dBaseSprite)) as tk2dBaseSprite[];
			foreach (var spr in allSprites)
			{
				if (PrefabUtility.GetPrefabType(spr) == PrefabType.PrefabInstance)
				{
					Object parent = PrefabUtility.GetPrefabParent(spr.gameObject);
					bool found = false;
					foreach (tk2dBaseSprite sprite in targetSprites) {
						if (sprite.gameObject == parent) {
							found = true;
							break;
						}
					}

					if (found) {
						// Reset all prefab states
						var propMod = PrefabUtility.GetPropertyModifications(spr);
						PrefabUtility.ResetToPrefabState(spr);
						PrefabUtility.SetPropertyModifications(spr, propMod);
						
						spr.ForceBuild();
					}
				}
			}
#endif
		}
	}

	void PerformActionOnSelection(string actionName, System.Action<GameObject> action) {
		Undo.RegisterSceneUndo(actionName);
		foreach (tk2dBaseSprite sprite in targetSprites) {
			action(sprite.gameObject);
		}
	}

	static void PerformActionOnGlobalSelection(string actionName, System.Action<GameObject> action) {
		Undo.RegisterSceneUndo(actionName);
		foreach (GameObject go in Selection.gameObjects) {
			if (go.GetComponent<tk2dBaseSprite>() != null) {
				action(go);
			}
		}
	}


	static void ConvertSpriteType(GameObject go, System.Type targetType) {
		tk2dBaseSprite spr = go.GetComponent<tk2dBaseSprite>();
		System.Type sourceType = spr.GetType();

		if (sourceType != targetType) {
			tk2dBatchedSprite batchedSprite = new tk2dBatchedSprite();
			tk2dStaticSpriteBatcherEditor.FillBatchedSprite(batchedSprite, go);
			if (targetType == typeof(tk2dSprite)) batchedSprite.type = tk2dBatchedSprite.Type.Sprite;
			else if (targetType == typeof(tk2dTiledSprite)) batchedSprite.type = tk2dBatchedSprite.Type.TiledSprite;
			else if (targetType == typeof(tk2dSlicedSprite)) batchedSprite.type = tk2dBatchedSprite.Type.SlicedSprite;
			else if (targetType == typeof(tk2dClippedSprite)) batchedSprite.type = tk2dBatchedSprite.Type.ClippedSprite;

			Object.DestroyImmediate(spr, true);

			bool sourceHasDimensions = sourceType == typeof(tk2dSlicedSprite) || sourceType == typeof(tk2dTiledSprite);
			bool targetHasDimensions = targetType == typeof(tk2dSlicedSprite) || targetType == typeof(tk2dTiledSprite);

			// Some minor fixups
			if (!sourceHasDimensions && targetHasDimensions) {
				batchedSprite.Dimensions = new Vector2(100, 100);
			}
			if (targetType == typeof(tk2dClippedSprite)) {
				batchedSprite.ClippedSpriteRegionBottomLeft = Vector2.zero;
				batchedSprite.ClippedSpriteRegionTopRight = Vector2.one;
			}
			if (targetType == typeof(tk2dSlicedSprite)) {
				batchedSprite.SlicedSpriteBorderBottomLeft = new Vector2(0.1f, 0.1f);
				batchedSprite.SlicedSpriteBorderTopRight = new Vector2(0.1f, 0.1f);
			}

			tk2dStaticSpriteBatcherEditor.RestoreBatchedSprite(go, batchedSprite);
		}
	}

	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Sprite")]
	static void DoConvertSprite() { PerformActionOnGlobalSelection( "Convert to Sprite", (go) => ConvertSpriteType(go, typeof(tk2dSprite)) ); }
	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Sliced Sprite")]
	static void DoConvertSlicedSprite() { PerformActionOnGlobalSelection( "Convert to Sliced Sprite", (go) => ConvertSpriteType(go, typeof(tk2dSlicedSprite)) ); }
	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Tiled Sprite")]
	static void DoConvertTiledSprite() { PerformActionOnGlobalSelection( "Convert to Tiled Sprite", (go) => ConvertSpriteType(go, typeof(tk2dTiledSprite)) ); }
	[MenuItem("CONTEXT/tk2dBaseSprite/Convert to Clipped Sprite")]
	static void DoConvertClippedSprite() { PerformActionOnGlobalSelection( "Convert to Clipped Sprite", (go) => ConvertSpriteType(go, typeof(tk2dClippedSprite)) ); }
	

	[MenuItem("CONTEXT/tk2dBaseSprite/Add animator", true, 10000)]
	static bool ValidateAddAnimator() {
		if (Selection.activeGameObject == null) return false;
		return Selection.activeGameObject.GetComponent<tk2dSpriteAnimator>() == null;
	}
	[MenuItem("CONTEXT/tk2dBaseSprite/Add animator", false, 10000)]
	static void DoAddAnimator() {
		tk2dSpriteAnimation anim = null;
		int clipId = -1;
		if (!tk2dSpriteAnimatorEditor.GetDefaultSpriteAnimation(out anim, out clipId)) {
			EditorUtility.DisplayDialog("Create Sprite Animation", "Unable to create animated sprite as no SpriteAnimations have been found.", "Ok");
			return;
		}
		else {
			PerformActionOnGlobalSelection("Add animator", delegate(GameObject go) {
				tk2dSpriteAnimator animator = go.GetComponent<tk2dSpriteAnimator>();
				if (animator == null) {
					animator = go.AddComponent<tk2dSpriteAnimator>();
					animator.Library = anim;
					animator.DefaultClipId = clipId;
					tk2dSpriteAnimationClip clip = anim.GetClipById(clipId);
					animator.SetSprite( clip.frames[0].spriteCollection, clip.frames[0].spriteId );
				}
			});
		}
	}

	[MenuItem("CONTEXT/tk2dBaseSprite/Add AttachPoint", false, 10002)]
	static void DoRemoveAnimator() {
		PerformActionOnGlobalSelection("Add AttachPoint", delegate(GameObject go) {
			tk2dSpriteAttachPoint ap = go.GetComponent<tk2dSpriteAttachPoint>();
			if (ap == null) {
				go.AddComponent<tk2dSpriteAttachPoint>();
			}
		});	
	}

    [MenuItem("GameObject/Create Other/tk2d/Sprite", false, 12900)]
    static void DoCreateSpriteObject()
    {
    	tk2dSpriteGuiUtility.GetSpriteCollectionAndCreate( (sprColl) => {
			GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Sprite");
			tk2dSprite sprite = go.AddComponent<tk2dSprite>();
			sprite.SetSprite(sprColl, sprColl.FirstValidDefinitionIndex);
			sprite.renderer.material = sprColl.FirstValidDefinition.material;
			sprite.Build();
			
			Selection.activeGameObject = go;
			Undo.RegisterCreatedObjectUndo(go, "Create Sprite");
		} );
    }
}

