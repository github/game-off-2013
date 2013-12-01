using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace tk2dEditor.SpriteAnimationEditor
{
	public enum AnimEditOperations
	{
		None = 0,
		AllClipsChanged = 1, // the allClips list has changed
		ClipContentChanged = 2, // the content of the clip has changed, only for the clipinspector & frame group inspector
		ClipNameChanged = 4, // clips name has changed
		NewClipCreated = 8, // the returned selectedClip is a newly created clip
	};

	// Inherit directly from this class
	public class AnimOperator
	{
		protected AnimEditOperations operations = AnimEditOperations.None;
		public AnimEditOperations AnimEditOperations { get { return operations; } }
		public int sortId = 0;

		// Sort id allows the operations to be sorted for draw = decide how it appears in the inspector
		// Negative numbers are reserved for the system
		public int SortId { get { return sortId; } }

		// Insert menu item into "Create" menu
		public virtual string[] AnimToolsMenu { get { return new string[0]; } }

		// Called by system when one of the anim tools menu above is selected
		// Return new selection if selection changed.
		public virtual tk2dSpriteAnimationClip OnAnimMenu(string menuEntry, List<tk2dSpriteAnimationClip> allClips, tk2dSpriteAnimationClip selectedClip) { return selectedClip; }

		// Drawn in the clip inspector GUI for the selected clip.
		// Return true when data has changed.
		public virtual bool OnClipInspectorGUI(tk2dSpriteAnimationClip selectedClip, List<ClipEditor.FrameGroup> frameGroups, TimelineEditor.State state) { return false; }

		// Drawn in the frame group inspector GUI for the selected clip.
		// Return true when data has changed.
		public virtual bool OnFrameGroupInspectorGUI(tk2dSpriteAnimationClip selectedClip, List<ClipEditor.FrameGroup> frameGroups, TimelineEditor.State state ) { return false; }
	}

	public static class AnimOperatorUtil
	{
		public static ClipEditor.FrameGroup NewFrameGroup(List<ClipEditor.FrameGroup> frameGroups, int selectedFrameGroup)
		{
			ClipEditor.FrameGroup src = frameGroups[selectedFrameGroup];
			ClipEditor.FrameGroup fg = new ClipEditor.FrameGroup();
			fg.spriteCollection = src.spriteCollection;
			fg.spriteId = src.spriteId;
			tk2dSpriteAnimationFrame f = new tk2dSpriteAnimationFrame();
			f.spriteCollection = fg.spriteCollection;
			f.spriteId = fg.spriteId;
			fg.frames.Add(f);
			return fg;
		}

		public static string UniqueClipName(List<tk2dSpriteAnimationClip> allClips, string baseName)
		{
			bool found = false;
			for (int i = 0; i < allClips.Count; ++i)
			{
				if (allClips[i].name == baseName) 
				{ 
					found = true; 
					break; 
				}
			}
			if (!found) return baseName;

			string uniqueName = baseName + " ";
			int uniqueId = 1;
			for (int i = 0; i < allClips.Count; ++i)
			{
				string uname = uniqueName + uniqueId.ToString();
				if (allClips[i].name == uname)
				{
					uniqueId++;
					i = -1;
					continue;
				}
			}
			uniqueName = uniqueName + uniqueId.ToString();
			return uniqueName;
		}
	}

	// Add a "Copy" option to the Animation menu
	public class CopyAnimation : AnimOperator
	{
		public override string[] AnimToolsMenu { get { return new string[] { "Copy" }; } }
		public override tk2dSpriteAnimationClip OnAnimMenu(string menuEntry, List<tk2dSpriteAnimationClip> allClips, tk2dSpriteAnimationClip selectedClip)
		{
			tk2dSpriteAnimationClip newClip = new tk2dSpriteAnimationClip();
			newClip.CopyFrom(selectedClip);
			newClip.name = AnimOperatorUtil.UniqueClipName( allClips, "Copy of " + selectedClip.name );
			allClips.Add(newClip);

			operations = AnimEditOperations.NewClipCreated | AnimEditOperations.AllClipsChanged;
			return newClip;	
		}
	} 

	// "Reverse frames"
	public class ClipTools : AnimOperator
	{
		public ClipTools()
		{
			sortId = -1000;
		}

		bool textToggle = false;
		string textNames = "";

		public override bool OnClipInspectorGUI(tk2dSpriteAnimationClip selectedClip, List<ClipEditor.FrameGroup> frameGroups, TimelineEditor.State state )
		{
			GUILayout.BeginHorizontal();

			bool changed = false;
			if (GUILayout.Button("Reverse", EditorStyles.miniButton))
			{
				frameGroups.Reverse();
				operations = AnimEditOperations.ClipContentChanged;
				state.selectedFrame = (state.selectedFrame == -1) ? state.selectedFrame : (frameGroups.Count - 1 - state.selectedFrame);
				changed = true;
			}
			GUIContent addTriggerContent = new GUIContent("Trigger", "You can also add a trigger by double clicking on the trigger area");
			if (GUILayout.Button(addTriggerContent, EditorStyles.miniButton))
			{
				for (int i = 0; i < selectedClip.frames.Length; ++i)
				{
					if (!selectedClip.frames[i].triggerEvent)
					{
						selectedClip.frames[i].triggerEvent = true;
						state.selectedTrigger = i;
						break;
					}
				}
				changed = true;
			}
			if (selectedClip.wrapMode != tk2dSpriteAnimationClip.WrapMode.Single)
			{
				bool newTextToggle = GUILayout.Toggle(textToggle, "Text", EditorStyles.miniButton);
				if (newTextToggle != textToggle)
				{
					if (newTextToggle == true)
					{
						textNames = BuildTextSpriteList(frameGroups);
						if (textNames.Length == 0) newTextToggle = false;
					}
					textToggle = newTextToggle;
				}
			}
			GUILayout.EndHorizontal();

			if (textToggle)
			{
				textNames = EditorGUILayout.TextArea(textNames, GUILayout.ExpandWidth(true));
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Process"))
				{
					if (ProcessSpriteImport(frameGroups, textNames))
					{
						textNames = "";
						textToggle = false;
						state.selectedFrame = -1;
						changed = true;
						GUIUtility.keyboardControl = 0;
					}
				}
				GUILayout.EndHorizontal();
			}

			return changed;
		}

		string BuildTextSpriteList(List<ClipEditor.FrameGroup> frameGroups)
		{
			bool fromSameCollection = true;
			bool areNamesValid = true;
			tk2dSpriteCollectionData coll = null;
			List<string> s = new List<string>();
			foreach (ClipEditor.FrameGroup frameGroup in frameGroups)
			{
				tk2dSpriteDefinition def = frameGroup.spriteCollection.spriteDefinitions[frameGroup.spriteId];
				if (coll == null) coll = frameGroup.spriteCollection;
				if (coll != frameGroup.spriteCollection) fromSameCollection = false;
				string spriteName = def.name;
				if (spriteName.IndexOf(";") != -1) areNamesValid = false;
				int frameCount = frameGroup.frames.Count;
				s.Add( (frameCount == 1) ? (spriteName) : (spriteName + ";" + frameCount.ToString()) );
			}
			if (!fromSameCollection)
			{
				EditorUtility.DisplayDialog("Text importer failed", "Current animation clip contains sprites from multiple collections", "Ok");
				return "";
			}
			if (!areNamesValid)
			{
				EditorUtility.DisplayDialog("Text importer failed", "Sprite names contain the ; character", "Ok");
				return "";
			}

			string spriteList = "";
			for (int i = 0; i < s.Count; ++i)
				spriteList += s[i] + "\n";
			return spriteList;
		}

		bool ProcessSpriteImport(List<ClipEditor.FrameGroup> frameGroups, string spriteNames)
		{
			tk2dSpriteCollectionData coll = frameGroups[0].spriteCollection;

			// make new list
			List<int> spriteIds = new List<int>();
			List<int> frameCounts = new List<int>();

			int lineNumber = 1;
			string[] lines = spriteNames.Split('\n');
			foreach (string line in lines)
			{
				if (line.Trim().Length != 0)
				{
					string spriteName = line;
					int frameCount = 1;
					int splitIndex = line.LastIndexOf(';');
					if (splitIndex != -1)
					{
						spriteName = line.Substring(0, splitIndex);
						string frameCountStr = line.Substring(splitIndex + 1, line.Length - 1 - splitIndex);
						if (!System.Int32.TryParse(frameCountStr, out frameCount))
						{
							Debug.LogError("Parse error in line " + lineNumber.ToString());
							return false;
						}
						frameCount = Mathf.Max(frameCount, 1);
					}
					int spriteId = coll.GetSpriteIdByName(spriteName, -1);
					if (spriteId == -1)
					{
						Debug.LogError(string.Format("Unable to find sprite '{0}' in sprite collection", spriteName));
						return false;
					}
					spriteIds.Add(spriteId);
					frameCounts.Add(frameCount);
				}
				lineNumber++;
			}

			List<ClipEditor.FrameGroup> newFrameGroups = new List<ClipEditor.FrameGroup>();
			for (int i = 0; i < spriteIds.Count; ++i)
			{
				if (i < frameGroups.Count && frameGroups[i].spriteId == spriteIds[i])
				{
					if (frameGroups[i].frames.Count != frameCounts[i])
						frameGroups[i].SetFrameCount(frameCounts[i]);
					newFrameGroups.Add(frameGroups[i]);
				}
				else
				{
					ClipEditor.FrameGroup fg = new ClipEditor.FrameGroup();
					fg.spriteCollection = coll;
					fg.spriteId = spriteIds[i];
					fg.SetFrameCount(frameCounts[i]);
					newFrameGroups.Add(fg);
				}	
			}
			frameGroups.Clear();
			foreach (ClipEditor.FrameGroup fg in newFrameGroups)
				frameGroups.Add(fg);

			operations = AnimEditOperations.ClipContentChanged;
			return true;
		}
	}

	// "Delete frames"
	public class DeleteFrames : AnimOperator
	{
		public DeleteFrames()
		{
			sortId = -50;
		}

		public override bool OnFrameGroupInspectorGUI(tk2dSpriteAnimationClip selectedClip, List<ClipEditor.FrameGroup> frameGroups, TimelineEditor.State state )
		{
			bool changed = false;
			if (frameGroups.Count > 1)
			{
				GUILayout.Space(16);
				if (GUILayout.Button("Delete", EditorStyles.miniButton))
				{
					frameGroups.RemoveAt(state.selectedFrame);
					state.selectedFrame = -1;
					changed = true;
				}
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Delete <", EditorStyles.miniButton)) { frameGroups.RemoveRange(0, state.selectedFrame); changed = true; state.selectedFrame = 0; }
				if (GUILayout.Button("Delete >", EditorStyles.miniButton)) { frameGroups.RemoveRange(state.selectedFrame + 1, frameGroups.Count - 1 - state.selectedFrame); changed = true; state.selectedFrame = frameGroups.Count - 1; }
				GUILayout.EndHorizontal();
			}
			operations = changed ? AnimEditOperations.ClipContentChanged : AnimEditOperations.None;
			return changed;
		}		
	}

	// "Insert frames"
	public class InsertFrames : AnimOperator
	{
		public InsertFrames()
		{
			sortId = -100;
		}

		public override bool OnFrameGroupInspectorGUI(tk2dSpriteAnimationClip selectedClip, List<ClipEditor.FrameGroup> frameGroups, TimelineEditor.State state )
		{
			if (selectedClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single)
				return false;

			bool changed = false;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Insert <", EditorStyles.miniButton)) 
			{ 
				frameGroups.Insert(state.selectedFrame, AnimOperatorUtil.NewFrameGroup(frameGroups, state.selectedFrame)); 
				state.selectedFrame++; 
				changed = true;
			}
			if (GUILayout.Button("Insert >", EditorStyles.miniButton)) 
			{ 
				frameGroups.Insert(state.selectedFrame + 1, AnimOperatorUtil.NewFrameGroup(frameGroups, state.selectedFrame));
				changed = true;
			}
			GUILayout.EndHorizontal();

			operations = changed ? AnimEditOperations.ClipContentChanged : AnimEditOperations.None;
			return changed;
		}		
	}

	// "AutoFill frames"
	public class AutoFillFrames : AnimOperator
	{
		public AutoFillFrames()
		{
			sortId = -110;
		}

		// Finds a sprite with the name and id
		// matches "baseName" [ 0..9 ]* as id
		// todo rewrite with regex
		int GetFrameIndex(tk2dSpriteDefinition def, string baseName)
		{
			if (System.String.Compare(baseName, 0, def.name, 0, baseName.Length, true) == 0)
			{
				int thisFrameId = 0;
				if (System.Int32.TryParse( def.name.Substring(baseName.Length), out thisFrameId ))
				{
					return thisFrameId;
				}
			}
			return -1;
		}
		
		int FindFrameIndex(tk2dSpriteDefinition[] spriteDefs, string baseName, int frameId)
		{
			for (int j = 0; j < spriteDefs.Length; ++j)
			{
				if (GetFrameIndex(spriteDefs[j], baseName) == frameId)
					return j;
			}
			return -1;
		}

		bool AutoFill(List<ClipEditor.FrameGroup> frameGroups, int selectedFrame, bool reverse)
		{
			ClipEditor.FrameGroup selectedFrameGroup = frameGroups[selectedFrame];
			if (selectedFrameGroup.spriteCollection != null && selectedFrameGroup.spriteId >= 0 && selectedFrameGroup.spriteId < selectedFrameGroup.spriteCollection.inst.Count)
			{
				string na = selectedFrameGroup.spriteCollection.inst.spriteDefinitions[selectedFrameGroup.spriteId].name;
				
				int numStartA = na.Length - 1;
				if (na[numStartA] >= '0' && na[numStartA] <= '9')
				{
					while (numStartA > 0 && na[numStartA - 1] >= '0' && na[numStartA - 1] <= '9')
						numStartA--;
					
					string baseName = na.Substring(0, numStartA).ToLower();
					int baseNo = System.Convert.ToInt32(na.Substring(numStartA));
					
					int maxAllowedMissing = 10;
					int allowedMissing = maxAllowedMissing;
					List<int> pendingFrames = new List<int>();
					int startOffset = reverse ? -1 : 1;
					int frameInc = reverse ? -1 : 1;
					for (int frameNo = baseNo + startOffset; frameNo >= 0 ; frameNo += frameInc)
					{
						int frameIdx = FindFrameIndex(selectedFrameGroup.spriteCollection.inst.spriteDefinitions, baseName, frameNo);
						if (frameIdx == -1)
						{
							if (--allowedMissing <= 0)
								break;
						}
						else
						{
							pendingFrames.Add(frameIdx);
							allowedMissing = maxAllowedMissing; // reset
						}
					}
					
					int numInserted = 0;
					int insertIndex = selectedFrame + 1;
					ClipEditor.FrameGroup nextFrameGroup = (insertIndex >= frameGroups.Count) ? null : frameGroups[insertIndex];
					while (pendingFrames.Count > 0)
					{
						int frameToInsert = pendingFrames[0];
						pendingFrames.RemoveAt(0);
						
						if (nextFrameGroup != null && 
							nextFrameGroup.spriteCollection == selectedFrameGroup.spriteCollection && 
							nextFrameGroup.spriteId == frameToInsert)
							break;
						
						ClipEditor.FrameGroup fg = AnimOperatorUtil.NewFrameGroup(frameGroups, selectedFrame);
						fg.spriteId = frameToInsert;
						fg.Update();
						frameGroups.Insert(insertIndex++, fg);
						numInserted++;
					}
					
					return numInserted > 0;
				}
			}
			
			return false;
		}		

		public override bool OnFrameGroupInspectorGUI(tk2dSpriteAnimationClip selectedClip, List<ClipEditor.FrameGroup> frameGroups, TimelineEditor.State state)
		{
			if (selectedClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single)
				return false;

			bool changed = false;
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Autofill 9..1", EditorStyles.miniButton) && AutoFill(frameGroups, state.selectedFrame, true)) { changed = true; }
			if (GUILayout.Button("Autofill 1..9", EditorStyles.miniButton) && AutoFill(frameGroups, state.selectedFrame, false)) { changed = true; }
			GUILayout.EndHorizontal();

			operations = changed ? AnimEditOperations.ClipContentChanged : AnimEditOperations.None;
			return changed;
		}		
	}
}

