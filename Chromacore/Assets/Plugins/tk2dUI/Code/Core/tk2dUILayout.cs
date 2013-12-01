using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class tk2dUILayoutItem {
	public tk2dBaseSprite sprite = null;
	public tk2dUIMask UIMask = null;
	public tk2dUILayout layout = null;
	public GameObject gameObj = null;

	public bool snapLeft = false;
	public bool snapRight = false;
	public bool snapTop = false;
	public bool snapBottom = false;

	// ContainerSizer
	public bool fixedSize = false;
	public float fillPercentage = -1;
	public float sizeProportion = 1;
	public static tk2dUILayoutItem FixedSizeLayoutItem() {
		tk2dUILayoutItem item = new tk2dUILayoutItem();
		item.fixedSize = true;
		return item;
	}

	// Internal
	public bool inLayoutList = false;
	public int childDepth = 0;
	public Vector3 oldPos = Vector3.zero;
}

/// <summary>
/// UI layout class.
/// </summary>
[AddComponentMenu("2D Toolkit/UI/Core/tk2dUILayout")]
public class tk2dUILayout : MonoBehaviour {
	public Vector3 bMin = new Vector3(0, -1, 0);
	public Vector3 bMax = new Vector3(1, 0, 0);

	public List<tk2dUILayoutItem> layoutItems = new List<tk2dUILayoutItem>();

	public int ItemCount {
		get {return layoutItems.Count;}
	}

	public bool autoResizeCollider = false;

	public event System.Action<Vector3, Vector3> OnReshape;

	void Reset() {
		if (collider != null) {
			BoxCollider box = collider as BoxCollider;
			if (box != null) {
				Bounds b = box.bounds;
				Matrix4x4 m = transform.worldToLocalMatrix;
				Vector3 oldWorldPos = transform.position;
				Reshape(m.MultiplyPoint(b.min) - bMin, m.MultiplyPoint(b.max) - bMax, true);
				Vector3 deltaLocalPos = m.MultiplyVector(transform.position - oldWorldPos);

				Transform t = transform;
				for (int i = 0; i < t.childCount; ++i) {
					Transform c = t.GetChild(i);
					Vector3 p = c.localPosition - deltaLocalPos;
					c.localPosition = p;
				}

				box.center = box.center - deltaLocalPos;

				autoResizeCollider = true;
			}
		}
	}

	public virtual void Reshape(Vector3 dMin, Vector3 dMax, bool updateChildren) {
		foreach (var item in layoutItems) {
			item.oldPos = item.gameObj.transform.position;
		}

		bMin += dMin;
		bMax += dMax;
		// Anchor top-left
		Vector3 origin = new Vector3(bMin.x, bMax.y);
		transform.position += transform.localToWorldMatrix.MultiplyVector(origin);
		bMin -= origin;
		bMax -= origin;

		if (autoResizeCollider) {
			var box = GetComponent<BoxCollider>();
			if (box != null) {
				box.center += (dMin + dMax) / 2.0f - origin;
				box.size += (dMax - dMin);
			}
		}

		foreach (var item in layoutItems) {
			var offset = transform.worldToLocalMatrix.MultiplyVector(item.gameObj.transform.position - item.oldPos);
			Vector3 qMin = -offset;
			Vector3 qMax = -offset;
			if (updateChildren) {
				qMin.x += item.snapLeft ? dMin.x : (item.snapRight ? dMax.x : 0);
				qMin.y += item.snapBottom ? dMin.y : (item.snapTop ? dMax.y : 0);
				qMax.x += item.snapRight ? dMax.x : (item.snapLeft ? dMin.x : 0);
				qMax.y += item.snapTop ? dMax.y : (item.snapBottom ? dMin.y : 0);
			}
			if (item.sprite != null || item.UIMask != null || item.layout != null) {
				// Transform from my space into item's space
				Matrix4x4 m = transform.localToWorldMatrix * item.gameObj.transform.worldToLocalMatrix;
				qMin = m.MultiplyVector(qMin);
				qMax = m.MultiplyVector(qMax);
			}
			if (item.sprite != null)
				item.sprite.ReshapeBounds(qMin, qMax);
			else if (item.UIMask != null)
				item.UIMask.ReshapeBounds(qMin, qMax);
			else if (item.layout != null)
				item.layout.Reshape(qMin, qMax, true);
			else {
				Vector3 s = qMin;
				if (item.snapLeft && item.snapRight)
					s.x = 0.5f * (qMin.x + qMax.x);
				if (item.snapTop && item.snapBottom)
					s.y = 0.5f * (qMin.y + qMax.y);
				item.gameObj.transform.position += s;
			}
		}

		if (OnReshape != null)
			OnReshape(dMin, dMax);
	}

	public void SetBounds(Vector3 pMin, Vector3 pMax) {
		Matrix4x4 m = transform.worldToLocalMatrix;
		Reshape(m.MultiplyPoint(pMin) - bMin, m.MultiplyPoint(pMax) - bMax, true);
	}

	public Vector3 GetMinBounds() {
		Matrix4x4 m = transform.localToWorldMatrix;
		return m.MultiplyPoint(bMin);
	}

	public Vector3 GetMaxBounds() {
		Matrix4x4 m = transform.localToWorldMatrix;
		return m.MultiplyPoint(bMax);
	}

	public void Refresh() {
		Reshape(Vector3.zero, Vector3.zero, true);
	}
}