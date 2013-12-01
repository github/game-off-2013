using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace tk2dEditor.SpriteAnimationEditor
{
	public class TimelineEditor 
	{
		// State
		public class State
		{
			int _selectedFrame = -1, _selectedTrigger = -1;

			public enum Type
			{
				None, 
				// All pending actions
				MoveHandle,  

				// Actions
				Action,
				Move, Resize
			}

			public int selectedFrame { get { return _selectedFrame; } set { ResetSelection(); _selectedFrame = value; } }
			public Type type = Type.None;
			public int activeFrame = -1;
			public int insertMarker = -1;
			public List<tk2dSpriteAnimationFrame> backupFrames = new List<tk2dSpriteAnimationFrame>();

			public int selectedTrigger { get { return _selectedTrigger; } set { ResetSelection(); _selectedTrigger = value; } }
			public int movingTrigger = -1;

			public Vector2 frameSelectionOffset = Vector2.zero;
			
			public void Reset()
			{
				ResetSelection();
				ResetState();
			}

			public void ResetSelection()
			{
				_selectedFrame = -1;
				_selectedTrigger = -1;
			}

			public void ResetState()
			{
				activeFrame = -1;
				type = Type.None;
				backupFrames.Clear();

				movingTrigger = -1;
				insertMarker = -1;
				frameSelectionOffset.Set(0, 0);
			}
		}
		State state = new State();
		public State CurrentState { get { return state; } }

		void Repaint() { HandleUtility.Repaint(); }
		
		// Internal
		int clipLeftHeaderSpace = 16;
		int clipHeight = 80;
		int clipHeightScrollBar = 94;
		Vector2 clipScrollbar = Vector2.zero;

		// Trigger rects and selection utility
		Rect GetRectForTrigger(Rect triggerRect, int frame) { return new Rect(triggerRect.x + clipLeftHeaderSpace + frameWidth * frame - 3, triggerRect.y + 1, 15, 14); }
		int GetRoundedSelectedTrigger(Rect triggerRect, Vector2 mousePosition) { return (int)Mathf.Round((mousePosition.x - triggerRect.x - clipLeftHeaderSpace) / frameWidth); }
		int GetSelectedTrigger(Rect triggerRect, Vector2 mousePosition) {
			int rounded = GetRoundedSelectedTrigger(triggerRect, mousePosition);
			Rect r = GetRectForTrigger(triggerRect, rounded);
			return r.Contains(mousePosition) ? rounded : -1;
		}

		// Framegroup rect
		Vector2 GetInsertMarkerPositionForFrameGroup(Rect fgRect, int frameGroup, List<ClipEditor.FrameGroup> frameGroups)
		{
			int frame = (frameGroup >= frameGroups.Count) ? ( frameGroups[frameGroups.Count - 1].startFrame + frameGroups[frameGroups.Count - 1].frames.Count ) : frameGroups[frameGroup].startFrame;
			return new Vector2(fgRect.x + clipLeftHeaderSpace + frameWidth * frame, fgRect.y);
		}
		Rect GetRectForFrame(Rect fgRect, int frame, int numFrames) { return new Rect(fgRect.x + clipLeftHeaderSpace + frameWidth * frame, fgRect.y, numFrames * frameWidth, fgRect.height); }
		Rect GetRectForFrameGroup(Rect fgRect, ClipEditor.FrameGroup frameGroup) { return new Rect(fgRect.x + clipLeftHeaderSpace + frameWidth * frameGroup.startFrame, fgRect.y, frameGroup.frames.Count * frameWidth, fgRect.height); }
		int GetSelectedFrame(Rect fgRect, Vector2 mousePosition) 
		{ 
			return (int)Mathf.Floor((mousePosition.x - fgRect.x - clipLeftHeaderSpace) / frameWidth);
		}
		int GetSelectedFrameGroup(Rect fgRect, Vector2 mousePosition, List<ClipEditor.FrameGroup> frameGroups, bool insert)
		{
			int frame = GetSelectedFrame(fgRect, mousePosition);
			int currrentFrameGroup = 0;
			int newSel = insert ? frameGroups.Count : -1;
			foreach (ClipEditor.FrameGroup fg in frameGroups)
			{
				if (frame >= fg.startFrame && frame < fg.startFrame + fg.frames.Count)
					newSel = currrentFrameGroup;
				++currrentFrameGroup;
			}
			return newSel;
		}
		Rect GetResizeRectFromFrameRect(Rect r) 
		{ 
			int resizeHandleSize = (frameWidth < 15) ? 3 : 10;
			return new Rect(r.x + r.width - 1 - resizeHandleSize, r.y, resizeHandleSize, r.height);
		}

		// Frame width
		int frameWidth { get { if (tk2dPreferences.inst.animFrameWidth == -1) return 80; else return Mathf.Clamp(tk2dPreferences.inst.animFrameWidth, minFrameWidth, maxFrameWidth); } set { tk2dPreferences.inst.animFrameWidth = value; } }
		const int minFrameWidth = 10;
		const int maxFrameWidth = 100;

		public void Reset()
		{
			state.Reset();
		}

		public void Draw(int windowWidth, tk2dSpriteAnimationClip clip, List<ClipEditor.FrameGroup> frameGroups, float clipTimeMarker)
		{
			int space = clipLeftHeaderSpace;

			int requiredWidth = space + (clip.frames.Length + 1) * frameWidth;
			int clipHeightTotal = (requiredWidth > windowWidth) ? clipHeightScrollBar : clipHeight;

			clipScrollbar = GUILayout.BeginScrollView(clipScrollbar, GUILayout.Height(clipHeightTotal), GUILayout.ExpandWidth(true));
			GUILayout.BeginVertical();
		
			// Draw timeline axis
			GUILayout.Box("", EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			Rect timelineRect = GUILayoutUtility.GetLastRect();
			DrawAxis(clip, new Rect(timelineRect.x + space, timelineRect.y, timelineRect.width - space, timelineRect.height), frameWidth);

			// Draw background and derive trigger rect
			GUILayout.Box("", tk2dEditorSkin.Anim_BG, GUILayout.ExpandWidth(true), GUILayout.Height(16));
			Rect triggerRect = GUILayoutUtility.GetLastRect();

			// Trigger helpbox
			Rect triggerHelpBox = new Rect(triggerRect.x, triggerRect.y, triggerRect.height, triggerRect.height);
			if (GUIUtility.hotControl == 0 && triggerHelpBox.Contains(Event.current.mousePosition))
				GUI.Label(new Rect(triggerHelpBox.x, triggerHelpBox.y, 150, triggerHelpBox.height), "Double click to add triggers", EditorStyles.whiteMiniLabel);
			else
				GUI.Label(triggerHelpBox, "?", EditorStyles.whiteMiniLabel);

			// Control IDs
			int triggerControlId = "tk2d.DrawClip.Triggers".GetHashCode();
			int frameGroupControlId = "tk2d.DrawClip.FrameGroups".GetHashCode();

			// Draw triggers
			DrawTriggers(triggerControlId, triggerRect, clip);

			// Draw frames
			GUILayout.BeginHorizontal();

			int framesWidth = clipLeftHeaderSpace + (clip.frames.Length + 1) * frameWidth;
			Rect frameGroupRect = GUILayoutUtility.GetRect(framesWidth, 1, GUILayout.ExpandHeight(true));
			DrawFrameGroups(frameGroupControlId, frameGroupRect, clip, frameGroups, clipTimeMarker);

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			if (Event.current.type == EventType.ScrollWheel && (Event.current.alt || Event.current.control))
			{
				frameWidth =  Mathf.Clamp((int)(Event.current.delta.y + frameWidth), minFrameWidth, maxFrameWidth);
				Repaint();
			}
		
			GUILayout.EndScrollView();

			Rect scrollRect = GUILayoutUtility.GetLastRect();
			DrawFrameGroupsOverlay(frameGroupControlId, new Rect(scrollRect.x + frameGroupRect.x, scrollRect.y + frameGroupRect.y, frameGroupRect.width, frameGroupRect.height), clip, frameGroups, clipTimeMarker);
		}

		// Internal draw
		void DrawAxis(tk2dSpriteAnimationClip clip, Rect r, int widthPerTick)
		{
			if (Event.current.type == EventType.Repaint)
			{
				float minWidthPerTick = 50;
				int ticksPerStep = (int)Mathf.Ceil(minWidthPerTick / widthPerTick);

				float t = 0.0f;
				float x = r.x;
				while (x < r.x + r.width)
				{
					GUI.Label(new Rect(x, r.y, r.width, r.height), t.ToString("0.00"), EditorStyles.miniLabel);
					x += widthPerTick * ticksPerStep;
					t += ticksPerStep / clip.fps;
				}
			}
		}		

		void DrawFrameGroups(int controlId, Rect frameGroupRect, tk2dSpriteAnimationClip clip, List<ClipEditor.FrameGroup> frameGroups, float clipTimeMarker)
		{
			bool singleFrameMode = clip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single;

			// Initialize startframe in framegroups
			int numFrames = 0;
			foreach (ClipEditor.FrameGroup fg in frameGroups)
			{
				fg.startFrame = numFrames;
				numFrames += fg.frames.Count;
			}

			// Draw frames
			int currrentFrameGroup = 0;
			foreach (ClipEditor.FrameGroup fg in frameGroups)
			{
				Rect r = GetRectForFrameGroup(frameGroupRect, fg);
				DrawFrameGroupEx(r, clip, fg, 
					/* highlighted: */	 currrentFrameGroup == state.selectedFrame,
					/* showTime: */		 currrentFrameGroup == state.selectedFrame,
					/* playHighlight: */ clipTimeMarker >= fg.startFrame && clipTimeMarker < fg.startFrame + fg.frames.Count);
				if (!singleFrameMode)
					EditorGUIUtility.AddCursorRect(GetResizeRectFromFrameRect(r), MouseCursor.ResizeHorizontal);

				currrentFrameGroup++;
			}

			// Add frame button
			if ((int)state.type < (int)State.Type.Action)
			{
				Rect addFrameButonRect = GetRectForFrame(frameGroupRect, clip.frames.Length, 1);
				addFrameButonRect = new Rect(addFrameButonRect.x + addFrameButonRect.width * 0.25f, addFrameButonRect.y + addFrameButonRect.height * 0.25f,
											 addFrameButonRect.height * 0.5f, addFrameButonRect.height * 0.5f);
				if (!singleFrameMode &&
				 	GUI.Button(addFrameButonRect, "+"))
				{
					frameGroups.Add(AnimOperatorUtil.NewFrameGroup(frameGroups, frameGroups.Count - 1));
					ClipEditor.RecalculateFrames(clip, frameGroups);
					state.selectedFrame = frameGroups.Count - 1;
					Repaint();
				}
			}

			// Draw insert marker
			if (GUIUtility.hotControl == controlId && state.type == State.Type.Move && state.activeFrame != -1 && state.insertMarker != -1)
			{
				Vector2 v = GetInsertMarkerPositionForFrameGroup(frameGroupRect, state.insertMarker, frameGroups);
				GUI.color = Color.green;
				GUI.Box(new Rect(v.x, v.y, 2, frameGroupRect.height), "", tk2dEditorSkin.WhiteBox);
				GUI.color = Color.white;
			}

			// Keyboard shortcuts
			Event ev = Event.current;
			if (ev.type == EventType.KeyDown && GUIUtility.keyboardControl == 0	
				&& state.type == State.Type.None && state.selectedFrame != -1)
			{
				int newFrame = state.selectedFrame;
				switch (ev.keyCode)
				{
					case KeyCode.LeftArrow: case KeyCode.Comma: newFrame--; break;
					case KeyCode.RightArrow: case KeyCode.Period: newFrame++; break;
					case KeyCode.Home: newFrame = 0; break;
					case KeyCode.End: newFrame = frameGroups.Count - 1; break;
					case KeyCode.Escape: state.selectedFrame = -1; Repaint(); ev.Use(); break;
				}
				
				if (ev.type != EventType.Used && frameGroups.Count > 0)
				{
					newFrame = Mathf.Clamp(newFrame, 0, frameGroups.Count - 1);
					if (newFrame != state.selectedFrame)
					{
						state.selectedFrame = newFrame;
						Repaint();
						ev.Use();
					}
				}
			}
			if (state.selectedFrame != -1 && (GUIUtility.hotControl == controlId || (GUIUtility.keyboardControl == 0 && state.type == State.Type.None)))
			{
				if (ev.type == EventType.KeyDown && (ev.keyCode == KeyCode.Delete || ev.keyCode == KeyCode.Backspace) && frameGroups.Count > 1)
				{
					frameGroups.RemoveAt(state.selectedFrame);
					ClipEditor.RecalculateFrames(clip, frameGroups);
					GUIUtility.hotControl = 0;
					state.Reset();
					Repaint();
					ev.Use();
				}
			}

			if (ev.type == EventType.MouseDown || GUIUtility.hotControl == controlId)
			{
				switch (ev.GetTypeForControl(controlId))
				{
					case EventType.MouseDown:
						if (frameGroupRect.Contains(ev.mousePosition))
						{ 
							int frameGroup = GetSelectedFrameGroup(frameGroupRect, ev.mousePosition, frameGroups, false);
							if (frameGroup != state.selectedFrame)
							{
								Repaint();
								state.selectedFrame = frameGroup;
							}
							if (frameGroup != -1)
							{
								Rect r = GetRectForFrameGroup(frameGroupRect, frameGroups[frameGroup]);
								Rect resizeRect = GetResizeRectFromFrameRect(r);
								state.frameSelectionOffset = ev.mousePosition - new Vector2(r.x, 0);
								state.type = resizeRect.Contains(ev.mousePosition) ? State.Type.Resize : State.Type.MoveHandle;
								if (state.type == State.Type.Resize)
								{
									if (singleFrameMode)
									{
										state.ResetState(); // disallow resize in single frame mode
									}
									else
									{
										state.backupFrames = new List<tk2dSpriteAnimationFrame>(frameGroups[frameGroup].frames); // make a backup of frames for triggers
										state.activeFrame = frameGroup; 
										state.insertMarker = state.activeFrame;
									}
								}
								else
								{
									state.activeFrame = frameGroup; 
									state.insertMarker = state.activeFrame;
								}
							}
							GUIUtility.hotControl = controlId;
						}
						GUIUtility.keyboardControl = 0;
						break;
					case EventType.MouseDrag:
						{
							switch (state.type)
							{
								case State.Type.MoveHandle: 
								case State.Type.Move:
									{
										state.type = State.Type.Move;
										state.insertMarker = GetSelectedFrameGroup(frameGroupRect, ev.mousePosition, frameGroups, true);
									}
									break;
								case State.Type.Resize: 
									{
										int frame = GetSelectedFrame(frameGroupRect, ev.mousePosition + new Vector2(frameWidth * 0.5f, 0.0f));
										ClipEditor.FrameGroup fg = frameGroups[state.activeFrame];
										int frameCount = Mathf.Max(1, frame - fg.startFrame);
										bool changed = frameCount != fg.frames.Count;
										if (changed)
										{
											fg.frames = new List<tk2dSpriteAnimationFrame>(state.backupFrames);
											fg.SetFrameCount(frameCount);
											Repaint();
											ClipEditor.RecalculateFrames(clip, frameGroups);
										}
									}
									break;
							}
						}
						break;
					case EventType.MouseUp:
						switch (state.type)
						{
							case State.Type.Move:
							{
								int finalInsertMarker = (state.insertMarker > state.activeFrame) ? (state.insertMarker - 1) : state.insertMarker;
								if (state.activeFrame != finalInsertMarker)
								{
									ClipEditor.FrameGroup tmpFrameGroup = frameGroups[state.activeFrame];
									frameGroups.RemoveAt(state.activeFrame);
									frameGroups.Insert(finalInsertMarker, tmpFrameGroup);
									state.selectedFrame = finalInsertMarker;
									ClipEditor.RecalculateFrames(clip, frameGroups);
									Repaint();
								}
							}
							break;
						}
						if (state.type != State.Type.None) Repaint();
						state.ResetState();
						GUIUtility.keyboardControl = 0;
						GUIUtility.hotControl = 0;
						break;
				}
			}

			if (clipTimeMarker >= 0.0f)
			{
				float x = clipLeftHeaderSpace + frameWidth * clipTimeMarker;
				GUI.color = Color.red;
				GUI.Box(new Rect(frameGroupRect.x + x, frameGroupRect.y, 2, frameGroupRect.height), "", tk2dEditorSkin.WhiteBox);
				GUI.color = Color.white;
			}
		}

		void DrawFrameGroupsOverlay(int controlId, Rect frameGroupRect, tk2dSpriteAnimationClip clip, List<ClipEditor.FrameGroup> frameGroups, float clipTimeMarker)
		{
			// Draw moving frame if active
			if (GUIUtility.hotControl == controlId && state.type == State.Type.Move && state.activeFrame != -1)
			{
				GUI.color = new Color(0.8f,0.8f,0.8f,0.9f);
				ClipEditor.FrameGroup fg = frameGroups[state.activeFrame];
				DrawFrameGroup(new Rect(Event.current.mousePosition.x - state.frameSelectionOffset.x, frameGroupRect.y - frameGroupRect.height, frameWidth * fg.frames.Count, frameGroupRect.height), clip, fg);
				GUI.color = Color.white;
				Repaint();
			}
		}

		void DrawTriggers(int controlId, Rect triggerRect, tk2dSpriteAnimationClip clip)
		{
			// Draw triggers
			GUI.color = (state.movingTrigger != -1) ? new Color(1,1,1,0.25f) : Color.white;
			for (int i = 0; i < clip.frames.Length; ++i)
			{
				Rect r = GetRectForTrigger(triggerRect, i);
				if (clip.frames[i].triggerEvent)
				{
					if (state.selectedTrigger == i) 
						GUI.Box(r, " ", tk2dEditorSkin.Anim_TriggerSelected);
					else
						GUI.Box(r, " ", tk2dEditorSkin.Anim_Trigger);
				}
			}
			if (state.movingTrigger != -1)
			{
				GUI.color = Color.white;
				Rect r = GetRectForTrigger(triggerRect, state.movingTrigger);
				GUI.Box(r, " ", tk2dEditorSkin.Anim_TriggerSelected);
			}

			Event ev = Event.current;

			// Keyboard
			if (state.selectedTrigger != -1 && (GUIUtility.hotControl == controlId || GUIUtility.keyboardControl == 0) && ev.type == EventType.KeyDown)
			{
				switch (ev.keyCode)
				{
					case KeyCode.Escape:
						GUIUtility.hotControl = 0;
						state.Reset();
						Repaint();
						ev.Use();
						break;
					case KeyCode.Delete:
					case KeyCode.Backspace:
						clip.frames[state.selectedTrigger].ClearTrigger();
						GUIUtility.hotControl = 0;
						state.Reset();
						Repaint();
						ev.Use();
						break;
				}
			}

			// Process trigger input
			if (ev.type == EventType.MouseDown || GUIUtility.hotControl == controlId)
			{
				switch (ev.GetTypeForControl(controlId))
				{
					case EventType.MouseDown:
						if (triggerRect.Contains(ev.mousePosition) && ev.button == 0)
						{
							int selectedTrigger = GetSelectedTrigger(triggerRect, ev.mousePosition);
							int selectedTriggerRegion = GetRoundedSelectedTrigger(triggerRect, ev.mousePosition);
							bool startDrag = state.selectedTrigger == selectedTriggerRegion;
							if (ev.clickCount == 1)
							{
								if (startDrag && selectedTriggerRegion == state.selectedTrigger)
								{
									GUIUtility.hotControl = controlId;
								}
								else if (selectedTrigger >= 0 && selectedTrigger < clip.frames.Length && clip.frames[selectedTrigger].triggerEvent)
								{
									state.selectedTrigger = selectedTrigger;
									Repaint();
									GUIUtility.hotControl = controlId;
								}
							}
							// Double click on an empty area
							if (GUIUtility.hotControl == 0 && ev.clickCount == 2 && selectedTriggerRegion >= 0 && selectedTriggerRegion < clip.frames.Length && !clip.frames[selectedTriggerRegion].triggerEvent)
							{
								clip.frames[selectedTriggerRegion].triggerEvent = true;
								state.selectedTrigger = selectedTriggerRegion;
								Repaint();
							}

							GUIUtility.keyboardControl = 0;
						}
						break;
					case EventType.MouseDrag:
						{
							int selectedTrigger = Mathf.Clamp( GetRoundedSelectedTrigger(triggerRect, ev.mousePosition), 0, clip.frames.Length - 1);
							if (state.movingTrigger != selectedTrigger)
							{
								state.movingTrigger = selectedTrigger;
								Repaint();
							}
						}
						break;
					case EventType.MouseUp:
						if (state.movingTrigger != -1 && state.movingTrigger != state.selectedTrigger)
						{
							tk2dSpriteAnimationFrame source = clip.frames[state.selectedTrigger];
							tk2dSpriteAnimationFrame dest = clip.frames[state.movingTrigger];
							dest.CopyTriggerFrom(source);
							source.ClearTrigger();
							state.selectedTrigger = state.movingTrigger;
						}
						Repaint();
						state.ResetState();
						GUIUtility.hotControl = 0;
						break;
				}
			}			
		}

		void DrawFrameGroup(Rect r, tk2dSpriteAnimationClip clip, ClipEditor.FrameGroup fg)
		{
			DrawFrameGroupEx(r, clip, fg, false, false, false);
		}

		void DrawFrameGroupEx(Rect r, tk2dSpriteAnimationClip clip, ClipEditor.FrameGroup fg, bool highlighted, bool showTime, bool playHighlight)
		{
			if (highlighted && playHighlight) GUI.color = new Color(1.0f, 0.8f, 1.0f, 1.0f);
			else if (playHighlight) GUI.color = new Color(1.0f, 0.8f, 0.8f, 1.0f);
			else if (highlighted) GUI.color = new Color(0.8f, 0.8f, 1.0f, 1.0f);

			tk2dSpriteCollectionData sc = fg.spriteCollection;
			int spriteId = fg.spriteId;
			string name = sc.inst.spriteDefinitions[spriteId].name;
			string label = name;
			if (showTime)
			{
				string numFrames = (fg.frames.Count == 1) ? "1 frame" : (fg.frames.Count.ToString() + " frames");
				string time = (fg.frames.Count / clip.fps).ToString("0.000") + "s";
				label = label + "\n" + numFrames + "\n" + time;
			}
			GUI.Label(r, label, "button");

			if (highlighted || playHighlight) GUI.color = Color.white;
		}
	}
}
