using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class tk2dUIMethodBindingHelper {
    private Dictionary<GameObject, Dictionary<System.Type, List<string>>> cache = new Dictionary<GameObject, Dictionary<System.Type, List<string>>>();

    private void CacheGameObject(GameObject go) {
        cache.Add( go, new Dictionary<System.Type, List<string>>() );
    }

    static readonly string[] ignoredMethodNames = new string[] {
        "Start", "Awake", "OnEnable", "OnDisable",
        "Update", "LateUpdate", "FixedUpdate"
    };

    private void CacheMethodsForGameObject(GameObject go, System.Type parameterType) {
        List<string> cachedMethods = new List<string>();
        cache[go].Add( parameterType, cachedMethods );

        List<System.Type> addedTypes = new List<System.Type>();
        MonoBehaviour[] behaviours = go.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour beh in behaviours) {
            System.Type type = beh.GetType();
            if (addedTypes.IndexOf(type) == -1) {
                System.Reflection.MethodInfo[] methods = type.GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                foreach (System.Reflection.MethodInfo method in methods) {
                    // Only add variables added by user, i.e. we don't want functions from the base UnityEngine baseclasses or lower
                    string moduleName = method.DeclaringType.Assembly.ManifestModule.Name;
                    if (!moduleName.Contains("UnityEngine") && !moduleName.Contains("mscorlib") &&
                        !method.ContainsGenericParameters && 
                        System.Array.IndexOf(ignoredMethodNames, method.Name) == -1) {
                        System.Reflection.ParameterInfo[] paramInfo = method.GetParameters();
                        if (paramInfo.Length == 0) {
                            cachedMethods.Add(method.Name);
                        }
                        else if (paramInfo.Length == 1 && paramInfo[0].ParameterType == parameterType) {
                            cachedMethods.Add(method.Name);
                        }
                    }
                }
            }
        }
    }

    public void MethodBinding( string name, System.Type supportedOptionalParameterType, GameObject target, ref string methodName ) {
        if (target == null) {
            bool oge = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.Popup( name, -1, new string[0] );
            GUI.enabled = oge;
            return;
        }

        if (displayHelp) {
            GUILayout.BeginVertical("box");
        }

        if (!cache.ContainsKey(target)) {
            CacheGameObject(target);
        }

        if (!cache[target].ContainsKey(supportedOptionalParameterType)) {
            CacheMethodsForGameObject(target, supportedOptionalParameterType);
        }

        List<string> cachedMethods = cache[target][supportedOptionalParameterType];
        int idx = cachedMethods.IndexOf(methodName);
        GUILayout.BeginHorizontal();
        int nidx = EditorGUILayout.Popup( name, idx, cachedMethods.ToArray() );
        if (nidx != idx) {
            methodName = cachedMethods[nidx];
        }
        if (methodName.Length != 0) {
            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.ExpandWidth(false))) {
                methodName = "";
                HandleUtility.Repaint();
            }
        }

        GUILayout.EndHorizontal();

        if (displayHelp) {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            GUIStyle helpStyle = EditorGUIUtility.isProSkin ? EditorStyles.whiteMiniLabel : EditorStyles.miniLabel;
            GUILayout.Label("Optional parameter: " + supportedOptionalParameterType.ToString(), helpStyle);
            GUILayout.EndHorizontal();
        }

        if (displayHelp) {
            GUILayout.EndVertical();
        }
    }

    static bool displayHelp = false;

    public GameObject BeginMessageGUI(GameObject target) {
        EditorGUIUtility.LookLikeControls();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Send Message", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        displayHelp = GUILayout.Toggle(displayHelp, "?", EditorStyles.miniButton);
        GUILayout.EndHorizontal();
        EditorGUI.indentLevel++;
        GameObject newSendMessageTarget = EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true, null) as GameObject;
        if (newSendMessageTarget != target) {
            target = newSendMessageTarget;
            GUI.changed = true;
        }
        EditorGUI.indentLevel++;
        return target;
    }

    public void EndMessageGUI() {
        EditorGUI.indentLevel--;
        EditorGUI.indentLevel--;
    }
}