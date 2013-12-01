using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(tk2dSpriteAnimator))]
public class tk2dSpriteAnimatorEditor : Editor
{
	tk2dGenericIndexItem[] animLibs = null;
	string[] animLibNames = null;
	bool initialized = false;

	tk2dSpriteAnimator[] targetAnimators = new tk2dSpriteAnimator[0];

    protected T[] GetTargetsOfType<T>( Object[] objects ) where T : UnityEngine.Object {
    	List<T> ts = new List<T>();
    	foreach (Object o in objects) {
    		T s = o as T;
    		if (s != null)
    			ts.Add(s);
    	}
    	return ts.ToArray();
    }
	
	void OnEnable() {
		targetAnimators = GetTargetsOfType<tk2dSpriteAnimator>( targets );
	}

	void Init()
	{
		if (!initialized)
		{
			animLibs = tk2dEditorUtility.GetOrCreateIndex().GetSpriteAnimations();
			if (animLibs != null)
			{
				animLibNames = new string[animLibs.Length];
				for (int i = 0; i < animLibs.Length; ++i)
				{
					animLibNames[i] = animLibs[i].AssetName;
				}
			}
			initialized = true;
		}
	}
	
    public override void OnInspectorGUI()
    {
		Init();
		if (animLibs == null)
		{
			GUILayout.Label("no libraries found");
			if (GUILayout.Button("Refresh"))
			{
				initialized = false;
				Init();
			}
		}
		else
		{
	        tk2dSpriteAnimator sprite = (tk2dSpriteAnimator)target;
			
			EditorGUIUtility.LookLikeInspector();
			EditorGUI.indentLevel = 1;

			if (sprite.Library == null)
			{
				sprite.Library = animLibs[0].GetAsset<tk2dSpriteAnimation>();
				GUI.changed = true;
			}
			
			// Display animation library
			int selAnimLib = 0;
			string selectedGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sprite.Library));
			for (int i = 0; i < animLibs.Length; ++i)
			{
				if (animLibs[i].assetGUID == selectedGUID)
				{
					selAnimLib = i;
					break;
				}
			}
		
			int newAnimLib = EditorGUILayout.Popup("Anim Lib", selAnimLib, animLibNames);
			if (newAnimLib != selAnimLib)
			{
				Undo.RegisterUndo(targetAnimators, "Sprite Anim Lib");
				foreach (tk2dSpriteAnimator animator in targetAnimators) {
					animator.Library = animLibs[newAnimLib].GetAsset<tk2dSpriteAnimation>();
					animator.DefaultClipId = 0;
					
					if (animator.Library.clips.Length > 0)
					{
						if (animator.Sprite != null) {
							// automatically switch to the first frame of the new clip
							animator.Sprite.SetSprite(animator.Library.clips[animator.DefaultClipId].frames[0].spriteCollection,
						                  			  animator.Library.clips[animator.DefaultClipId].frames[0].spriteId);
						}
					}
				}
			}
			
			// Everything else
			if (sprite.Library && sprite.Library.clips.Length > 0)
			{
				int clipId = sprite.DefaultClipId;

				// Sanity check clip id
				clipId = Mathf.Clamp(clipId, 0, sprite.Library.clips.Length - 1);
				if (clipId != sprite.DefaultClipId)
				{
					sprite.DefaultClipId = clipId;
					GUI.changed = true;
				}
				
				List<string> clipNames = new List<string>(sprite.Library.clips.Length);
				List<int> clipIds = new List<int>(sprite.Library.clips.Length);

				// fill names (with ids if necessary)
				for (int i = 0; i < sprite.Library.clips.Length; ++i)
				{
					if (sprite.Library.clips[i].name != null && sprite.Library.clips[i].name.Length > 0) {
						string name = sprite.Library.clips[i].name;
						if (tk2dPreferences.inst.showIds) {
							name += "\t[" + i.ToString() + "]";
						}
						clipNames.Add( name );
						clipIds.Add( i );
					}
				}
				
				int newClipId = EditorGUILayout.IntPopup("Clip", sprite.DefaultClipId, clipNames.ToArray(), clipIds.ToArray());
				if (newClipId != sprite.DefaultClipId)
				{
					Undo.RegisterUndo(targetAnimators, "Sprite Anim Clip");
					foreach (tk2dSpriteAnimator animator in targetAnimators) {
						animator.DefaultClipId = newClipId;

						if (animator.Sprite != null) {
							// automatically switch to the first frame of the new clip
							animator.Sprite.SetSprite(animator.Library.clips[animator.DefaultClipId].frames[0].spriteCollection,
													  animator.Library.clips[animator.DefaultClipId].frames[0].spriteId);
						}
					}
				}
			}

			// Play automatically
			bool newPlayAutomatically = EditorGUILayout.Toggle("Play automatically", sprite.playAutomatically);
			if (newPlayAutomatically != sprite.playAutomatically) {
				Undo.RegisterUndo(targetAnimators, "Sprite Anim Play Automatically");
				foreach (tk2dSpriteAnimator animator in targetAnimators) {
					animator.playAutomatically = newPlayAutomatically;
				}
			}

			if (GUI.changed)
			{
				foreach (tk2dSpriteAnimator spr in targetAnimators) {
					EditorUtility.SetDirty(spr);
				}
			}
		}
    }

    public static bool GetDefaultSpriteAnimation(out tk2dSpriteAnimation anim, out int clipId) {
		tk2dGenericIndexItem[] animIndex = tk2dEditorUtility.GetOrCreateIndex().GetSpriteAnimations();
    	anim = null;
    	clipId = -1;
		foreach (var animIndexItem in animIndex)
		{
			tk2dSpriteAnimation a = animIndexItem.GetAsset<tk2dSpriteAnimation>();
			if (a != null && a.clips != null && a.clips.Length > 0)
			{
				for (int i = 0; i < a.clips.Length; ++i) {
					if (!a.clips[i].Empty &&
						a.clips[i].frames[0].spriteCollection != null &&
						a.clips[i].frames[0].spriteId >= 0) {
						clipId = i;
						break;
					}
				}

				if (clipId != -1) {
					anim = a;
					break;
				}
			}
		}

		return anim != null && clipId != -1;
    }

    [MenuItem("GameObject/Create Other/tk2d/Sprite With Animator", false, 12951)]
    static void DoCreateSpriteObject()
    {
		tk2dSpriteAnimation anim = null;
		int clipId = -1;
		if (!GetDefaultSpriteAnimation(out anim, out clipId)) {
			EditorUtility.DisplayDialog("Create Sprite Animation", "Unable to create animated sprite as no SpriteAnimations have been found.", "Ok");
			return;
		}
		
		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("AnimatedSprite");

		tk2dSprite sprite = go.AddComponent<tk2dSprite>();
		sprite.SetSprite(anim.clips[clipId].frames[0].spriteCollection, anim.clips[clipId].frames[0].spriteId);

		tk2dSpriteAnimator animator = go.AddComponent<tk2dSpriteAnimator>();
		animator.Library = anim;
		animator.DefaultClipId = clipId;
		
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create Sprite With Animator");
    }
}

