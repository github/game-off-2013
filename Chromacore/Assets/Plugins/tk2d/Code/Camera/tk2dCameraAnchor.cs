using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Camera/tk2dCameraAnchor")]
[ExecuteInEditMode]
/// <summary>
/// Anchors children to anchor position, offset by number of pixels
/// </summary>
public class tk2dCameraAnchor : MonoBehaviour 
{
	// Legacy anchor
	// Order: Upper [Left, Center, Right], Middle, Lower
	[SerializeField]
	int anchor = -1;

	// Backing variable for AnchorPoint accessor
	[SerializeField]
	tk2dBaseSprite.Anchor _anchorPoint = tk2dBaseSprite.Anchor.UpperLeft;

	[SerializeField]
	bool anchorToNativeBounds = false;

	/// <summary>
	/// Anchor point location
	/// </summary>
	public tk2dBaseSprite.Anchor AnchorPoint {
		get {
			if (anchor != -1) {
				if (anchor >= 0 && anchor <= 2) _anchorPoint = (tk2dBaseSprite.Anchor)( anchor + 6 );
				else if (anchor >= 6 && anchor <= 8) _anchorPoint = (tk2dBaseSprite.Anchor)( anchor - 6 );
				else _anchorPoint = (tk2dBaseSprite.Anchor)( anchor );
				anchor = -1;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			}
			return _anchorPoint;
		}
		set {
			_anchorPoint = value;
		}
	}

	[SerializeField]
	Vector2 offset = Vector2.zero;

	/// <summary>
	/// Offset in pixels from the anchor. 
	/// This is consistently in screen space, i.e. +y = top of screen, +x = right of screen
	/// Eg. If you need to inset 10 pixels from from top right anchor, you'd use (-10, -10)
	/// </summary>
	public Vector2 AnchorOffsetPixels {
		get {
			return offset;
		}
		set {
			offset = value;
		}
	}

	/// <summary>
	/// Anchor this to the tk2dCamera native bounds, instead of the screen bounds.
	/// </summary>
	public bool AnchorToNativeBounds {
		get {
			return anchorToNativeBounds;
		}
		set {
			anchorToNativeBounds = value;
		}
	}
	
	// Another backwards compatiblity only thing here
	[SerializeField]
	tk2dCamera tk2dCamera = null;

	// New field
	[SerializeField]
	Camera _anchorCamera = null;

	// Used to decide when to try to find the tk2dCamera component again
	Camera _anchorCameraCached = null;
	tk2dCamera _anchorTk2dCamera = null;

	/// <summary>
	/// Offset in pixels from the anchor. 
	/// This is consistently in screen space, i.e. +y = top of screen, +x = right of screen
	/// Eg. If you need to inset 10 pixels from from top right anchor, you'd use (-10, -10)
	/// </summary>
	public Camera AnchorCamera {
		get {
			if (tk2dCamera != null) {
				_anchorCamera = tk2dCamera.camera;
				tk2dCamera = null;
#if UNITY_EDITOR
				UnityEditor.EditorUtility.SetDirty(this);
#endif
			}
			return _anchorCamera;
		}
		set {
			_anchorCamera = value;
			_anchorCameraCached = null;
		}
	}

	tk2dCamera AnchorTk2dCamera {
		get {
			if (_anchorCameraCached != _anchorCamera) {
				_anchorTk2dCamera = _anchorCamera.GetComponent<tk2dCamera>();
				_anchorCameraCached = _anchorCamera;
			}
			return _anchorTk2dCamera;
		}
	}

	// cache transform locally
	Transform _myTransform;
	Transform myTransform {
		get {
			if (_myTransform == null) _myTransform = transform;
			return _myTransform;
		}
	}
	
	void Start()
	{
		UpdateTransform();
	}
	
	void UpdateTransform()
	{
		// Break out if anchor camera is not bound
		if (AnchorCamera == null) {
			return;
		}

		float pixelScale = 1; // size of one pixel
		Vector3 position = myTransform.localPosition;

		// we're ignoring perspective tk2dCameras for now
		tk2dCamera = (AnchorTk2dCamera != null && AnchorTk2dCamera.CameraSettings.projection != tk2dCameraSettings.ProjectionType.Perspective) ? AnchorTk2dCamera : null;

		Rect rect = new Rect();
		if (tk2dCamera != null) {
			rect = anchorToNativeBounds ? tk2dCamera.NativeScreenExtents : tk2dCamera.ScreenExtents;
			pixelScale = tk2dCamera.GetSizeAtDistance( 1 ); 
		}
		else {
			rect.Set(0, 0, AnchorCamera.pixelWidth, AnchorCamera.pixelHeight);
		}

		float y_bot = rect.yMin;
		float y_top = rect.yMax;
		float y_ctr = (y_bot + y_top) * 0.5f;

		float x_lhs = rect.xMin;
		float x_rhs = rect.xMax;
		float x_ctr = (x_lhs + x_rhs) * 0.5f;

		Vector3 anchoredPosition = Vector3.zero;

		switch (AnchorPoint)
		{
		case tk2dBaseSprite.Anchor.UpperLeft: 		anchoredPosition = new Vector3(x_lhs, y_top, position.z); break;
		case tk2dBaseSprite.Anchor.UpperCenter: 	anchoredPosition = new Vector3(x_ctr, y_top, position.z); break;
		case tk2dBaseSprite.Anchor.UpperRight: 		anchoredPosition = new Vector3(x_rhs, y_top, position.z); break;
		case tk2dBaseSprite.Anchor.MiddleLeft: 		anchoredPosition = new Vector3(x_lhs, y_ctr, position.z); break;
		case tk2dBaseSprite.Anchor.MiddleCenter:	anchoredPosition = new Vector3(x_ctr, y_ctr, position.z); break;
		case tk2dBaseSprite.Anchor.MiddleRight: 	anchoredPosition = new Vector3(x_rhs, y_ctr, position.z); break;
		case tk2dBaseSprite.Anchor.LowerLeft: 		anchoredPosition = new Vector3(x_lhs, y_bot, position.z); break;
		case tk2dBaseSprite.Anchor.LowerCenter: 	anchoredPosition = new Vector3(x_ctr, y_bot, position.z); break;
		case tk2dBaseSprite.Anchor.LowerRight: 		anchoredPosition = new Vector3(x_rhs, y_bot, position.z); break;
		}
		
		Vector3 screenAnchoredPosition = anchoredPosition + new Vector3(pixelScale * offset.x, pixelScale * offset.y, 0);
		if (tk2dCamera == null) { // not a tk2dCamera, we need to transform
			Vector3 worldAnchoredPosition = AnchorCamera.ScreenToWorldPoint( screenAnchoredPosition );
			if (myTransform.position != worldAnchoredPosition) {
				myTransform.position = worldAnchoredPosition;
			}
		}
		else {
			Vector3 oldPosition = myTransform.localPosition;
			if (oldPosition != screenAnchoredPosition) {
				myTransform.localPosition = screenAnchoredPosition;
			}
		}
	}

	public void ForceUpdateTransform()
	{
		UpdateTransform();
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		UpdateTransform();
	}
}
