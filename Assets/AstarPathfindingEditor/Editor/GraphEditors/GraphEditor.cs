using Pathfinding;
using UnityEditor;
using UnityEngine;

public class GraphEditor : GraphEditorBase {
	
	public AstarPathEditor editor;
	
	public virtual void OnEnable () {
	}
	
	public virtual void OnDisable () {
	}
	
	public virtual void OnDestroy () {
	}
	
	public Object ObjectField (string label, Object obj, System.Type objType, bool allowSceneObjects) {
		return ObjectField (new GUIContent (label),obj,objType,allowSceneObjects);
	}
	
	public Object ObjectField (GUIContent label, Object obj, System.Type objType, bool allowSceneObjects) {
		
#if UNITY_3_3
		allowSceneObjects = true;
#endif
		
#if UNITY_3_3
		obj = EditorGUILayout.ObjectField (label, obj, objType);
#else
		obj = EditorGUILayout.ObjectField (label, obj, objType, allowSceneObjects);
#endif
		
		if (obj != null) {
			if (allowSceneObjects && !EditorUtility.IsPersistent (obj)) {
				//Object is in the scene
				Component com = obj as Component;
				GameObject go = obj as GameObject;
				if (com != null) {
					go = com.gameObject;
				}
				if (go != null) {
					UnityReferenceHelper urh = go.GetComponent<UnityReferenceHelper> ();
					if (urh == null) {
						
						if (FixLabel ("Object's GameObject must have a UnityReferenceHelper component attached")) {
							go.AddComponent<UnityReferenceHelper>();
						}	
					}
				}
				
			} else if (EditorUtility.IsPersistent (obj)) {
				
				string path = AssetDatabase.GetAssetPath (obj);
				
				System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"Resources[/|\\][^/]*$");
				
				//if (!path.Contains ("Resources/")) {
				Debug.Log (path + "  " + rg.Match(path).Value);
				
				if (!rg.IsMatch(path)) {
					if (FixLabel ("Object must be in the 'Resources' folder, top level")) {
						if (!System.IO.Directory.Exists (Application.dataPath+"/Resources")) {
							System.IO.Directory.CreateDirectory (Application.dataPath+"/Resources");
							AssetDatabase.Refresh ();
						}
						string ext = System.IO.Path.GetExtension(path);
						
						string error = AssetDatabase.MoveAsset	(path,"Assets/Resources/"+obj.name+ext);
						
						if (error == "") {
							//Debug.Log ("Successful move");
							path = AssetDatabase.GetAssetPath (obj);
						} else {
							Debug.LogError ("Couldn't move asset - "+error);
						}
					}
				}
				
				if (!AssetDatabase.IsMainAsset (obj) && obj.name != AssetDatabase.LoadMainAssetAtPath (path).name) {
					if (FixLabel ("Due to technical reasons, the main asset must\nhave the same name as the referenced asset")) {
						string error = AssetDatabase.RenameAsset (path,obj.name);
						if (error == "") {
							//Debug.Log ("Successful");
						} else {
							Debug.LogError ("Couldn't rename asset - "+error);
						}
					}
				}
			}
		}
		
		return obj;
	}
	
	/*public void OnDisableUndo () {
		return;
		if (!editor.enableUndo) {
			return;
		}
		
		if (undoState != null) {
			ScriptableObject.DestroyImmediate (undoState);
			undoState = null;
		}
	}
	
	private void ApplyUndo () {
		return;
		if (!editor.enableUndo) {
			return;
		}
		
		undoState.hasBeenReverted = false;
		
		if (AstarPath.active == null) {
			return;
		}
		
		byte[] bytes = GetSerializedBytes (target);
			
		bool isDifferent = false;
		
		//Check if the data is any different from the last saved data, if it isn't, don't load it
		if (undoState.data == null || bytes.Length != undoState.data.Length) {
			isDifferent = true;
		} else {
			for (int i=0;i<bytes.Length;i++) {
				if (bytes[i] != undoState.data[i]) {
					isDifferent = true;
					break;
				}
			}
		}
		
		if (isDifferent) {
			
			Debug.Log ("The undo is different "+ Event.current.type +" "+Event.current.commandName);
			//Event.current.Use ();
			AstarSerializer sz = new AstarSerializer (editor.script);
			sz.OpenDeserialize (undoState.data);
			sz.DeSerializeSettings (target,AstarPath.active);
			sz.Close ();
		}
	}
	
	public void ModifierKeysChanged () {
		return;
		if (!editor.enableUndo) {
			return;
		}
		
		if (undoState == null) {
			return;
		}
		//The user has tried to undo something, apply that
		if (undoState.hasBeenReverted) {
			ApplyUndo ();
			GUI.changed = true;
			editor.Repaint ();
		}
		
	}
	
	/** Handles undo operations for the graph *
	public void HandleUndo (NavGraph target) {
		return;
		if (!editor.enableUndo) {
			return;
		}
		
		if (undoState == null) {
			undoState = ScriptableObject.CreateInstance<GraphUndo>();
		}
		
		Event e = Event.current;
		
		//ModifierKeysChanged ();
		
		//To serialize settings for a grid graph takes from 0.00 ms to 7.8 ms (usually 0.0, but sometimes jumps up to 7.8 (no values in between)
		if ((e.button == 0 && (e.type == EventType.MouseDown || e.type == EventType.MouseUp)) || (e.isKey && (e.keyCode == KeyCode.Tab || e.keyCode == KeyCode.Return))) {
			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
				
			//Serialize the settings of the graph
			byte[] bytes = GetSerializedBytes (target);
			
			bool isDifferent = false;
			
			if (undoState.data == null) {
				Debug.LogError ("UndoState.data == null - This should not happen");
				return;
			}
			
			//Check if the data is any different from the last saved data, if it isn't, don't save it
			if (bytes.Length != undoState.data.Length) {
				isDifferent = true;
			} else {
				for (int i=0;i<bytes.Length;i++) {
					if (bytes[i] != undoState.data[i]) {
						isDifferent = true;
						break;
					}
				}
			}
			
			//Only save undo if the data was different from the last saved undo
			if (isDifferent) {
				//This flag is set to true so we can detect if the object has been reverted
				undoState.hasBeenReverted = true;
			
				Undo.RegisterUndo (undoState,"A* inspector");
				
				//Assign the new data
				undoState.data = bytes;
				
				//Undo.SetSnapshotTarget(undoState,"A* inspector");
				//Undo.CreateSnapshot ();
				//Undo.RegisterSnapshot();
				
				undoState.hasBeenReverted = false;
				Debug.Log ("Saved "+bytes.Length+" bytes");
				stopWatch.Stop();
				Debug.Log ("Adding Undo "+stopWatch.Elapsed.ToString ());
			}
			
			
		}
	}*/
	
	/** Returns a byte array with the settings of the graph. This function serializes the graph's settings and stores them in a byte array, used for undo operations. This will not save any additional metadata such as which A* version we are working on. */
	private byte[] GetSerializedBytes (NavGraph target) {
		//Serialize the settings of the graph
		AstarSerializer sz = new AstarSerializer (editor.script);
		sz.OpenSerialize ();
		sz.SerializeSettings (target,AstarPath.active);
		sz.Close ();
		byte[] bytes = (sz.writerStream.BaseStream as System.IO.MemoryStream).ToArray ();
		
		return bytes;
	}
	
	/** Draws common graph settings */
	public void OnBaseInspectorGUI (NavGraph target) {
		int penalty = EditorGUILayout.IntField (new GUIContent ("Initial Penalty","Initial Penalty for nodes in this graph. Set during Scan."),(int)target.initialPenalty);
		if (penalty < 0) penalty = 0;
		target.initialPenalty = (uint)penalty;
	}
	
	/** Override to implement graph inspectors */
	public virtual void OnInspectorGUI (NavGraph target) {
	}
	
	/** Override to implement scene GUI drawing for the graph */
	public virtual void OnSceneGUI (NavGraph target) {
	}
	
	/** Override to implement scene Gizmos drawing for the graph editor */
	public virtual void OnDrawGizmos () {
	}
	
	/** Draws a thin separator line */
	public void Separator () {
		GUIStyle separator = AstarPathEditor.astarSkin.FindStyle ("PixelBox3Separator");
		if (separator == null) {
			separator = new GUIStyle ();
		}
		
		Rect r = GUILayoutUtility.GetRect (new GUIContent (),separator);
		
		if (Event.current.type == EventType.Repaint) {
			separator.Draw (r,false,false,false,false);
		}
	}
	
	/** Draws a small help box with a 'Fix' button to the right. \returns Boolean - Returns true if the button was clicked */
	public bool FixLabel (string label, string buttonLabel = "Fix", int buttonWidth = 40) {
		bool returnValue = false;
		GUILayout.BeginHorizontal ();
		GUILayout.Space (14*EditorGUI.indentLevel);
		GUILayout.BeginHorizontal (AstarPathEditor.helpBox);
		GUILayout.Label (label, EditorStyles.miniLabel,GUILayout.ExpandWidth (true));
		if (GUILayout.Button (buttonLabel,EditorStyles.miniButton,GUILayout.Width (buttonWidth))) {
			returnValue = true;
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndHorizontal ();
		return returnValue;
	}
	
	/** Draws a small help box. Works with EditorGUI.indentLevel */
	public void HelpBox (string label) {
		GUILayout.BeginHorizontal ();
		GUILayout.Space (14*EditorGUI.indentLevel);
		GUILayout.Label (label, AstarPathEditor.helpBox);
		GUILayout.EndHorizontal ();
	}
	
	/** Draws a toggle with a bold label to the right. Does not enable or disable GUI */
	public bool ToggleGroup (string label, bool value) {
		return ToggleGroup (new GUIContent (label),value);
	}
	
	/** Draws a toggle with a bold label to the right. Does not enable or disable GUI */
	public bool ToggleGroup (GUIContent label, bool value) {
		GUILayout.BeginHorizontal ();
		GUILayout.Space (13*EditorGUI.indentLevel);
		value = GUILayout.Toggle (value,"",GUILayout.Width (10));
		GUIStyle boxHeader = AstarPathEditor.astarSkin.FindStyle ("CollisionHeader");
		if (GUILayout.Button (label,boxHeader, GUILayout.Width (100))) {
			value = !value;
		}
		
		GUILayout.EndHorizontal ();
		return value;
	}
	
	/** Draws the inspector for a \link Pathfinding.GraphCollision GraphCollision class \endlink */
	public void DrawCollisionEditor (GraphCollision collision) {
		
		if (collision == null) {
			collision = new GraphCollision ();
		}
		
		/*GUILayout.Space (5);
		Rect r = EditorGUILayout.BeginVertical (AstarPathEditor.graphBoxStyle);
		GUI.Box (r,"",AstarPathEditor.graphBoxStyle);
		GUILayout.Space (2);*/
		Separator ();
		
		/*GUILayout.BeginHorizontal ();
		GUIStyle boxHeader = AstarPathEditor.astarSkin.FindStyle ("CollisionHeader");
		GUILayout.Label ("Collision testing",boxHeader);
		collision.collisionCheck = GUILayout.Toggle (collision.collisionCheck,"");
		
		bool preEnabledRoot = GUI.enabled;
		GUI.enabled = collision.collisionCheck;
		GUILayout.EndHorizontal ();*/
		collision.collisionCheck = ToggleGroup ("Collision testing",collision.collisionCheck);
		bool preEnabledRoot = GUI.enabled;
		GUI.enabled = collision.collisionCheck;
		
		//GUILayout.BeginHorizontal ();
		collision.type = (ColliderType)EditorGUILayout.EnumPopup("Collider type",collision.type);
		//new string[3] {"Sphere","Capsule","Ray"}
		
		bool preEnabled = GUI.enabled;
		if (collision.type != ColliderType.Capsule && collision.type != ColliderType.Sphere) {
			GUI.enabled = false;
		}
		collision.diameter = EditorGUILayout.FloatField (new GUIContent ("Diameter","Diameter of the capsule of sphere where 1 equals one node"),collision.diameter);
		
		GUI.enabled = preEnabled;
		
		if (collision.type != ColliderType.Capsule && collision.type != ColliderType.Ray) {
			GUI.enabled = false;
		}
		collision.height = EditorGUILayout.FloatField (new GUIContent ("Height/Length","Height of cylinder of length of ray in world units"),collision.height);
		GUI.enabled = preEnabled;
		
		collision.collisionOffset = EditorGUILayout.FloatField (new GUIContent("Offset","Offset upwards from the node. Can be used so obstacles can be used as ground and as obstacles for lower nodes"),collision.collisionOffset);
		
		//collision.mask = 1 << EditorGUILayout.LayerField ("Mask",Mathf.Clamp ((int)Mathf.Log (collision.mask,2),0,31));
		
		collision.mask = EditorGUILayoutx.LayerMaskField ("Mask",collision.mask);
		
		GUILayout.Space (2);
		
		
		GUI.enabled = preEnabledRoot;
		
		collision.heightCheck = ToggleGroup ("Height testing",collision.heightCheck);
		GUI.enabled = collision.heightCheck && GUI.enabled;
		/*GUILayout.BeginHorizontal ();
		GUILayout.Label ("Height testing",boxHeader);
		collision.heightCheck = GUILayout.Toggle (collision.heightCheck,"");
		GUI.enabled = collision.heightCheck;
		GUILayout.EndHorizontal ();*/
		
		collision.fromHeight = EditorGUILayout.FloatField (new GUIContent ("Ray length","The height from which to check for ground"),collision.fromHeight);
		
		collision.heightMask = EditorGUILayoutx.LayerMaskField ("Mask",collision.heightMask);
		//collision.heightMask = 1 << EditorGUILayout.LayerField ("Mask",Mathf.Clamp ((int)Mathf.Log (collision.heightMask,2),0,31));
		
		collision.thickRaycast = EditorGUILayout.Toggle (new GUIContent ("Thick Raycast", "Use a thick line instead of a thin line"),collision.thickRaycast);
		
		editor.GUILayoutx.BeginFadeArea (collision.thickRaycast,"thickRaycastDiameter");
		
		if (editor.GUILayoutx.DrawID ("thickRaycastDiameter")) {
			EditorGUI.indentLevel++;
			collision.thickRaycastDiameter = EditorGUILayout.FloatField (new GUIContent ("Diameter","Diameter of the thick raycast"),collision.thickRaycastDiameter);
			EditorGUI.indentLevel--;
		}
		
		editor.GUILayoutx.EndFadeArea ();
		
		collision.unwalkableWhenNoGround = EditorGUILayout.Toggle (new GUIContent ("Unwalkable when no ground","Make nodes unwalkable when no ground was found with the height raycast. If height raycast is turned off, this doesn't affect anything"), collision.unwalkableWhenNoGround);
		
		GUI.enabled = preEnabledRoot;
		
		//GUILayout.Space (2);
		//EditorGUILayout.EndVertical ();
		//GUILayout.Space (5);
	}
	
	/** Draws a wire cube using handles */
	public static void DrawWireCube (Vector3 center, Vector3 size) {
		
		size *= 0.5F;
		
		Vector3 dx = new Vector3 (size.x,0,0);
		Vector3 dy = new Vector3 (0,size.y,0);
		Vector3 dz = new Vector3 (0,0,size.z);
		
		Vector3 p1 = center-dy-dz-dx;
		Vector3 p2 = center-dy-dz+dx;
		Vector3 p3 = center-dy+dz+dx;
		Vector3 p4 = center-dy+dz-dx;
		
		Vector3 p5 = center+dy-dz-dx;
		Vector3 p6 = center+dy-dz+dx;
		Vector3 p7 = center+dy+dz+dx;
		Vector3 p8 = center+dy+dz-dx;
		
		/*Handles.DrawAAPolyLine (new Vector3[4] {p1,p2,p3,p4});
		Handles.DrawAAPolyLine (new Vector3[4] {p5,p6,p7,p8});
		
		Handles.DrawAAPolyLine (new Vector3[2] {p1,p5});
		Handles.DrawAAPolyLine (new Vector3[2] {p2,p6});
		Handles.DrawAAPolyLine (new Vector3[2] {p3,p7});
		Handles.DrawAAPolyLine (new Vector3[2] {p4,p8});*/
		
		Handles.DrawLine (p1,p2);
		Handles.DrawLine (p2,p3);
		Handles.DrawLine (p3,p4);
		Handles.DrawLine (p4,p1);
		
		Handles.DrawLine (p5,p6);
		Handles.DrawLine (p6,p7);
		Handles.DrawLine (p7,p8);
		Handles.DrawLine (p8,p5);
		
		Handles.DrawLine (p1,p5);
		Handles.DrawLine (p2,p6);
		Handles.DrawLine (p3,p7);
		Handles.DrawLine (p4,p8);
	}
	
	/** \cond */
	public static Texture2D lineTex;
	
	/** \deprecated Test function, might not work. Uses undocumented editor features */
	public static void DrawAALine (Vector3 a, Vector3 b) {
		
		if (lineTex == null) {
			lineTex = new Texture2D (1,4);
			lineTex.SetPixels (new Color[4] {
				Color.clear,
				Color.black,
				Color.black,
				Color.clear,
			});
			lineTex.Apply ();
		}
		
		SceneView c = SceneView.lastActiveSceneView;
		
		Vector3 tangent1 = Vector3.Cross ((b-a).normalized, c.camera.transform.position-a).normalized;
		
		Handles.DrawAAPolyLine (lineTex,new Vector3[3] {a,b,b+tangent1*10});//,b+tangent1,a+tangent1});
	}
	/** \endcond */
}
