using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Please don't add this component manually
[AddComponentMenu("")]
public class tk2dUpdateManager : MonoBehaviour {
	static tk2dUpdateManager inst;
	static tk2dUpdateManager Instance {
		get {
			if (inst == null) {
				inst = GameObject.FindObjectOfType(typeof(tk2dUpdateManager)) as tk2dUpdateManager;
				if (inst == null) {
					GameObject go = new GameObject("@tk2dUpdateManager");
					go.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
					inst = go.AddComponent<tk2dUpdateManager>();
					DontDestroyOnLoad(go);
				}
			}
			return inst;
		}
	}

	// Add textmeshes to the list here
	// Take care to not add twice
	// Never queue in edit mode
	public static void QueueCommit( tk2dTextMesh textMesh ) {
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			textMesh.DoNotUse__CommitInternal();
		}
		else 
#endif
		{
			Instance.QueueCommitInternal( textMesh );
		}
	}

	// This can be called more than once, and we do - to try and catch all possibilities
	public static void FlushQueues() {
#if UNITY_EDITOR
		if (Application.isPlaying) {
			Instance.FlushQueuesInternal();
		}
#else
		Instance.FlushQueuesInternal();
#endif
	}

	void OnEnable() {
		// for when the assembly is reloaded, coroutine is killed then
		StartCoroutine(coSuperLateUpdate());
	}

	// One in late update
	void LateUpdate() {
		FlushQueuesInternal();
	}

	IEnumerator coSuperLateUpdate() {
		FlushQueuesInternal();
		yield break;
	}

	void QueueCommitInternal( tk2dTextMesh textMesh ) {
		textMeshes.Add( textMesh );
	}

	void FlushQueuesInternal() {
		int count = textMeshes.Count;
		for (int i = 0; i < count; ++i) {
			tk2dTextMesh tm = textMeshes[i];
			if (tm != null) {
				tm.DoNotUse__CommitInternal();
			}
		}
		textMeshes.Clear();
	}

	// Preallocate these lists to avoid allocation later
	[SerializeField] List<tk2dTextMesh> textMeshes = new List<tk2dTextMesh>(64);
}
