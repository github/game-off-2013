using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUICamera")]
public class tk2dUICamera : MonoBehaviour {

	// This is multiplied with the cameras layermask
	[SerializeField]
	private LayerMask raycastLayerMask = -1;

	// This is used for backwards compatiblity only
	public void AssignRaycastLayerMask( LayerMask mask ) {
		raycastLayerMask = mask;
	}

	// The actual layermask, i.e. allowedMasks & layerMask
	public LayerMask FilteredMask {
		get {
			return raycastLayerMask & camera.cullingMask;
		}
	}

	public Camera HostCamera {
		get {
			return camera;
		}
	}

	void OnEnable() {
		if (camera == null) {
			Debug.LogError("tk2dUICamera should only be attached to a camera.");
			enabled = false;
			return;
		}

		tk2dUIManager.RegisterCamera( this );
	}

	void OnDisable() {
		tk2dUIManager.UnregisterCamera( this );
	}
}
