using UnityEngine;
using System.Collections;
using Pathfinding;
using System;
using System.Collections.Generic;
using System.IO;

/** Deserializer for 3.07 Graphs.
  * This class will override deserialization functions which changed with the next version */
public class AstarSerializer3_07 : AstarSerializer {
	
	public AstarSerializer3_07 (AstarPath script) : base (script){}

	/** Serializes a Unity Reference. Serializer references such as Transform, GameObject, Texture or other unity objects */
	public override void AddUnityReferenceValue (string key, UnityEngine.Object value) {
		//Segment --- Should be identical to a segment in AddUnityReferenceValue/AddValue
		BinaryWriter stream = writerStream;
		
		AddVariableAnchor (key);
		
		if (value == null) {
			stream.Write ((byte)0);//Magic number indicating a null reference
			return;
		}
		
		//Magic number indicating that the data is written and not null
		stream.Write ((byte)1);
		//Segment --- End
		
		if (value == active.gameObject) {
			stream.Write (-128);//Magic number (random) indicates that the reference is the A* object
		} else if (value == active.transform) {
			stream.Write (-129);
		} else {
			stream.Write (value.GetInstanceID ());
		}
		stream.Write (value.name);
		
		//Write scene path if the object is a Component or GameObject
		Component component = value as Component;
		GameObject go = value as GameObject;
		
		if (component == null && go == null) {
			stream.Write ("");
		} else {
			if (go == null) {
				go = component.gameObject;
			}
			string path = go.name;
			
			while (go.transform.parent != null) {
				go = go.transform.parent.gameObject;
				path = go.name+"/" +path;
			}
			stream.Write (path);
		}
		
		if (writeUnityReference_Editor != null) {
			stream.Write (true);
			writeUnityReference_Editor (this,value);
		} else {
			stream.Write (false);
		}
	}
	
	/** Deserializes a Unity Reference. Deserializes references such as Transform, GameObject, Texture or other unity objects */
	public override UnityEngine.Object GetUnityReferenceValue (string key, Type type, UnityEngine.Object defaultValue = null) {
		//Segment --- Should be (except for the defaultValue cast) identical to a segment in GetUnityReferenceValue/GetValue
		if (!MoveToVariableAnchor (key)) {
			Debug.Log ("Couldn't find key '"+key+"' in the data, returning default");
			return (defaultValue == null ? GetDefaultValue (type) : defaultValue) as UnityEngine.Object;
		}
		
		BinaryReader stream = readerStream;
		
		int magicNumber = (int)stream.ReadByte ();
		
		if (magicNumber == 0) {
			return (defaultValue == null ? GetDefaultValue (type) : defaultValue) as UnityEngine.Object;//Null reference
		} else if (magicNumber == 2) {
			Debug.Log ("The variable '"+key+"' was not serialized correctly and can therefore not be deserialized");
			return (defaultValue == null ? GetDefaultValue (type) : defaultValue) as UnityEngine.Object;
		}
		//Else - magic number is 1 - indicating correctly serialized data
		//Segment --- End
		
		int instanceID = stream.ReadInt32 ();
		string obName = stream.ReadString ();
						
		if (instanceID == -128) {//Magic number
			return active.gameObject;
		} else if (instanceID == -129) { //Magic number
			return active.transform;
		}
		
		//Load scene path, will be "" if it is irrelevant (when not a GameObject or Component)
		string scenePath = stream.ReadString ();
		
		UnityEngine.Object ob3 = null;
		
		//Debug.Log ("Scene Path: "+scenePath);
		if (scenePath != "") {
			GameObject go = GameObject.Find (scenePath);
			if (go != null) {
				if (type == typeof (GameObject)) {
					return go;
				}
				ob3 = go.GetComponent (type);
				
				if (ob3 != null && ob3.name == obName) {
					return ob3;
				}
			}
		}
		
		//If we are in the editor, more sophisticated searching can be performed
		bool didSaveFromEditor = stream.ReadBoolean ();
		if (readUnityReference_Editor != null && didSaveFromEditor) {
			UnityEngine.Object eob = readUnityReference_Editor (this,obName,instanceID,type);
			if (eob != null && eob.name == obName) {
				return eob;
			} else if (ob3 != null) {
				return ob3;
			} else {
				return eob;
			}
		}
		
		//If the editor deserialization didn't come up with a better answer, return ob3 if it isn't null
		if (ob3 != null) {
			return ob3;
		}
		
		//Last resort, find all objects of type and check them for the instance ID
		UnityEngine.Object[] ueObs = Resources.FindObjectsOfTypeAll (type);
			//UnityEngine.Object.FindObjectsOfType (type);
		
		UnityEngine.Object ob1 = null;
		
		for (int i=0;i<ueObs.Length;i++) {
			if (ueObs[i].GetInstanceID () == instanceID) {
				ob1 = ueObs[i];
				break;
			}
			
			//Connecting it based on name is a bit too vague
			/*if (ueObs[i].name == obName) {
				ob2 = ueObs[i];
			}*/
		}
		
		if (ob1 != null) {
			return ob1;
		}
		
		//Try to load from resources
		UnityEngine.Object ob4 = Resources.Load (obName);
		
		return ob4;
		/*if (ob1 != null) {
			return ob1;
		} else {
			return ob2;
		}*/
	}
}
