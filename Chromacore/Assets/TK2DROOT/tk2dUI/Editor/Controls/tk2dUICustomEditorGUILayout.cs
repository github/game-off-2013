using UnityEngine;
using UnityEditor;
using System.Collections;

public class tk2dUICustomEditorGUILayout : Editor
{
    /// <summary>
    /// Used to nicely get scence objects inspector without getting direct prefabs. Obj is what is being passed in, targetObj is what received it
    /// </summary>
    public static T SceneObjectField<T>(string label, T obj,Object target) where T : Object
    {
        obj = EditorGUILayout.ObjectField(label, obj, typeof(T), true, null) as T;
        //if obj exists and both aren't prefabs, or both aren't in scene
        if (obj != null && (EditorUtility.IsPersistent(obj)!=EditorUtility.IsPersistent(target)))
        {
            obj = default(T);
            GUI.changed = true;
        }

        return obj;
    }
}
