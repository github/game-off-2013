using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteAttachPoint")]
/// <summary>
/// Sprite Attach Point reference implementation
/// Creates and manages a list of child gameObjects, with data for these sourced from
/// the SpriteDefinition.AttachPoint. Position and rotation are supported.
/// </summary>
public class tk2dSpriteAttachPoint : MonoBehaviour {

	private tk2dBaseSprite sprite;

	/// <summary>
	/// A list of live attach points.
	/// </summary>
	public List<Transform> attachPoints = new List<Transform>();

	// A list of attach points updated this frame - this is static as its only used for the lifetime
	// of the HandleSpriteChanged function
	static bool[] attachPointUpdated = new bool[32];

	/// <summary>
	/// When set, all inactive attach points (attach points that don't exist on a particular frame / sprite)
	/// will be disabled.
	/// </summary>
	public bool deactivateUnusedAttachPoints = false;

	void Awake() {
		if (sprite == null) {
			sprite = GetComponent<tk2dBaseSprite>();
			if (sprite != null) {
				HandleSpriteChanged( sprite );
			}
		}
	}

	void OnEnable() {
		if (sprite != null) {
			sprite.SpriteChanged += HandleSpriteChanged;
		}
	}

	void OnDisable() {
		if (sprite != null) {
			sprite.SpriteChanged -= HandleSpriteChanged;
		}
	}

	void UpdateAttachPointTransform( tk2dSpriteDefinition.AttachPoint attachPoint, Transform t ) {
		t.localPosition = Vector3.Scale( attachPoint.position, sprite.scale );
		t.localScale = sprite.scale;

		float scl = Mathf.Sign(sprite.scale.x) * Mathf.Sign(sprite.scale.y);

		t.localEulerAngles = new Vector3(0, 0, attachPoint.angle * scl); // handle angle fixup
	}

	void HandleSpriteChanged(tk2dBaseSprite spr) {
		tk2dSpriteDefinition def = spr.CurrentSprite;

		int maxAttachPoints = Mathf.Max( def.attachPoints.Length, attachPoints.Count );
		if (maxAttachPoints > attachPointUpdated.Length) {
			// resize to accomodate. no more bounds tests required below
			attachPointUpdated = new bool[maxAttachPoints];
		}

		foreach (tk2dSpriteDefinition.AttachPoint ap in def.attachPoints) {
			bool found = false;
			int currAttachPointId = 0;
			foreach (Transform inst in attachPoints ) {
				// A dictionary would be ideal here, but could end up in an indeterminate state due to
				// user deleting things at runtime. Hopefully the user won't have that many attach points
				// that a linear search becomes an issue
				if (inst != null && inst.name == ap.name) {
					attachPointUpdated[currAttachPointId] = true;
					UpdateAttachPointTransform( ap, inst );
					found = true;
				}
				currAttachPointId++;
			}
			if (!found) {
				GameObject go = new GameObject(ap.name);
				Transform t = go.transform;
				t.parent = transform;
				UpdateAttachPointTransform( ap, t );
				attachPointUpdated[attachPoints.Count] = true;
				attachPoints.Add(t);
			}
		}

		if (deactivateUnusedAttachPoints) {
			for (int i = 0; i < attachPoints.Count; ++i) {
				if (attachPoints[i] != null) {
					GameObject go = attachPoints[i].gameObject;
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
					if (attachPointUpdated[i] && !go.active) {
						go.SetActiveRecursively(true);
					}
					else if (!attachPointUpdated[i] && go.active) {
						go.SetActiveRecursively(false);
					}
#else
					if (attachPointUpdated[i] && !go.activeSelf) {
						go.SetActive(true);
					}
					else if (!attachPointUpdated[i] && go.activeSelf) {
						go.SetActive(false);
					}

#endif
				}
				attachPointUpdated[i] = false; // always reset to false to avoid a second pass update next time
			}
		}
	}
}
