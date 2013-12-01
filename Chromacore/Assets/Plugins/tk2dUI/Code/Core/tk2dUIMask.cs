using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUIMask")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class tk2dUIMask : MonoBehaviour {

	public tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleCenter;
	public Vector2 size = new Vector2(1, 1);
	public float depth = 1.0f;
	public bool createBoxCollider = true;

	MeshFilter _thisMeshFilter = null;
	MeshFilter ThisMeshFilter {
		get {
			if (_thisMeshFilter == null) {
				_thisMeshFilter = GetComponent<MeshFilter>();
			}
			return _thisMeshFilter;
		}
	}

	BoxCollider _thisBoxCollider = null;
	BoxCollider ThisBoxCollider {
		get {
			if (_thisBoxCollider == null) {
				_thisBoxCollider = GetComponent<BoxCollider>();
			}
			return _thisBoxCollider;
		}
	}

	// Use this for initialization
	void Awake() {
		Build();
	}

	void OnDestroy() {
		if (ThisMeshFilter.sharedMesh != null) {
#if UNITY_EDITOR
			Object.DestroyImmediate(ThisMeshFilter.sharedMesh);
#else
			Object.Destroy(ThisMeshFilter.sharedMesh);
#endif
		}
	}

	static readonly Vector2[] uv = new Vector2[] {
		new Vector2(0, 0),
		new Vector2(1, 0),
		new Vector2(0, 1),
		new Vector2(1, 1)
	};
	static readonly int[] indices = new int[6] { 
		0, 3, 1, 2, 3, 0
	};

	
	Mesh FillMesh(Mesh mesh) {
		Vector3 anchorOffset = Vector3.zero;
		switch (anchor) {
			case tk2dBaseSprite.Anchor.UpperLeft: 		anchorOffset = new Vector3(0, -size.y, 0); break;
			case tk2dBaseSprite.Anchor.UpperCenter: 	anchorOffset = new Vector3(-size.x / 2, -size.y, 0); break;
			case tk2dBaseSprite.Anchor.UpperRight: 		anchorOffset = new Vector3(-size.x, -size.y, 0); break;
			case tk2dBaseSprite.Anchor.MiddleLeft: 		anchorOffset = new Vector3(0, -size.y / 2, 0); break;
			case tk2dBaseSprite.Anchor.MiddleCenter: 	anchorOffset = new Vector3(-size.x / 2, -size.y / 2, 0); break;
			case tk2dBaseSprite.Anchor.MiddleRight: 	anchorOffset = new Vector3(-size.x, -size.y / 2, 0); break;
			case tk2dBaseSprite.Anchor.LowerLeft: 		anchorOffset = new Vector3(0, 0, 0); break;
			case tk2dBaseSprite.Anchor.LowerCenter: 	anchorOffset = new Vector3(-size.x / 2, 0, 0); break;
			case tk2dBaseSprite.Anchor.LowerRight: 		anchorOffset = new Vector3(-size.x, 0, 0); break;
		}

		Vector3[] positions = new Vector3[4] {
			anchorOffset + new Vector3( 0,  0, -depth),
			anchorOffset + new Vector3( size.x,  0, -depth),
			anchorOffset + new Vector3( 0,  size.y, -depth),
			anchorOffset + new Vector3( size.x,  size.y, -depth)
		};

		mesh.vertices = positions;
		mesh.uv = uv;
		mesh.triangles = indices;
		
		// Recalculate bounds, and reset bounds center, i.e. determining draw order for
		// transparent sprites
		Bounds bounds = new Bounds();
		bounds.SetMinMax(anchorOffset, anchorOffset + new Vector3(size.x, size.y, 0));
		mesh.bounds = bounds;

		return mesh;
	}

	void OnDrawGizmosSelected() {
		Mesh mesh = ThisMeshFilter.sharedMesh;
		if (mesh != null) {
			Gizmos.matrix = transform.localToWorldMatrix;
			Bounds bounds = mesh.bounds;
			Gizmos.color = new Color32(56, 146, 227, 96);
			float d = -depth * 1.001f;
			Vector3 center = new Vector3(bounds.center.x, bounds.center.y, d * 0.5f);
			Vector3 size = new Vector3(bounds.extents.x * 2, bounds.extents.y * 2, Mathf.Abs(d));
			Gizmos.DrawCube(center, size);

			Gizmos.color = new Color32(22, 145, 255, 255);
			Gizmos.DrawWireCube(center, size);
		}
	}

	public void Build() {
		if (ThisMeshFilter.sharedMesh == null) {
			Mesh mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			ThisMeshFilter.mesh = FillMesh(mesh);
		}
		else {
			FillMesh(ThisMeshFilter.sharedMesh);
		}

		if (createBoxCollider) {
			if (ThisBoxCollider == null) {
				_thisBoxCollider = gameObject.AddComponent<BoxCollider>();
			}
			Bounds bounds = ThisMeshFilter.sharedMesh.bounds;
			ThisBoxCollider.center = new Vector3( bounds.center.x, bounds.center.y, -depth );
			ThisBoxCollider.size = new Vector3( bounds.size.x, bounds.size.y, 0.0002f );
		}
		else if (ThisBoxCollider != null) {
#if UNITY_EDITOR
			Object.DestroyImmediate(ThisBoxCollider);
#else
			Object.Destroy(ThisBoxCollider);
#endif
		}
	}

	public void ReshapeBounds(Vector3 dMin, Vector3 dMax) {
		Vector3 oldSize = new Vector3(size.x, size.y);
		Vector3 oldMin = Vector3.zero;
		switch (anchor) {
			case tk2dBaseSprite.Anchor.LowerLeft: oldMin.Set(0,0,0); break;
			case tk2dBaseSprite.Anchor.LowerCenter: oldMin.Set(0.5f,0,0); break;
			case tk2dBaseSprite.Anchor.LowerRight: oldMin.Set(1,0,0); break;
			case tk2dBaseSprite.Anchor.MiddleLeft: oldMin.Set(0,0.5f,0); break;
			case tk2dBaseSprite.Anchor.MiddleCenter: oldMin.Set(0.5f,0.5f,0); break;
			case tk2dBaseSprite.Anchor.MiddleRight: oldMin.Set(1,0.5f,0); break;
			case tk2dBaseSprite.Anchor.UpperLeft: oldMin.Set(0,1,0); break;
			case tk2dBaseSprite.Anchor.UpperCenter: oldMin.Set(0.5f,1,0); break;
			case tk2dBaseSprite.Anchor.UpperRight: oldMin.Set(1,1,0); break;
		}
		oldMin = Vector3.Scale(oldMin, oldSize) * -1;
		Vector3 newSize = oldSize + dMax - dMin;
		Vector3 scaledMin = new Vector3(Mathf.Approximately(oldSize.x, 0) ? 0 : (oldMin.x * newSize.x / oldSize.x),
			Mathf.Approximately(oldSize.y, 0) ? 0 : (oldMin.y * newSize.y / oldSize.y));
		Vector3 offset = oldMin + dMin - scaledMin;
		offset.z = 0;
		transform.position = transform.TransformPoint(offset);
		size = new Vector2(newSize.x, newSize.y);

		Build();
	}
}
