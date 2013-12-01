using UnityEngine;

[AddComponentMenu("2D Toolkit/Deprecated/Extra/tk2dPixelPerfectHelper")]
/// <summary>
/// Allows remapping resolution and rescaling based on settings in this class. Deprecated and replaced by <see cref="tk2dCamera"/>.
/// </summary>
public class tk2dPixelPerfectHelper : MonoBehaviour
{
	// All access to this object should be performed through inst.
	static tk2dPixelPerfectHelper _inst = null;
	
	/// <summary>
	/// Global singleton instance.
	/// </summary>
	public static tk2dPixelPerfectHelper inst 
	{
		get
		{
			if (_inst == null)
			{
				_inst = GameObject.FindObjectOfType(typeof(tk2dPixelPerfectHelper)) as tk2dPixelPerfectHelper;
				if (_inst == null)
				{
					return null;
				}
				inst.Setup();
			}
			return _inst;
		}
	}
	
	void Awake()
	{
		Setup();
		_inst = this;
	}
	
	public virtual void Setup()
	{
		// Platform dependent initializion can occur by overriding this
		// You will need to call base class after setting up to finalize
		
		float resScale = collectionTargetHeight / targetResolutionHeight;

		if (camera != null) cam = camera;
		if (cam == null) cam = Camera.main;
		
		if (cam.isOrthoGraphic)
		{
			scaleK = resScale * cam.orthographicSize / collectionOrthoSize;
			scaleD = 0.0f;
		}
		else
		{
			float tk = resScale * Mathf.Tan(Mathf.Deg2Rad * cam.fieldOfView * 0.5f) / collectionOrthoSize;
			scaleK = tk * -cam.transform.position.z;
			scaleD = tk;
		}
	}
	
	/// <summary>
	/// Calculate scale to get 1:1 given fov in degress, and zdistance to camera.
	/// This assumes the screen resoulution hasn't changed.
	/// </summary>
	public static float CalculateScaleForPerspectiveCamera(float fov, float zdist)
	{
		return Mathf.Abs( Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f) * zdist );
	}
	
	/// <summary>
	/// Is the linked camera orthographic?
	/// </summary>
	public bool CameraIsOrtho
	{
		get { return cam.isOrthoGraphic; }
	}
	
	// camera
	[System.NonSerialized] public Camera cam;
	
	/// <summary>
	/// The height of the collection target as it was set up.
	/// </summary>
	public int collectionTargetHeight = 640;
	/// <summary>
	/// The ortho size parameter of the sprite collection, as it was set up.
	/// </summary>
	public float collectionOrthoSize = 1.0f;
	
	/// <summary>
	/// The height of the resolution to map to. (eg. 1024x768 = 768)
	/// </summary>
	public float targetResolutionHeight = 640.0f;
	
	// scales
	[System.NonSerialized] public float scaleD = 0.0f; // scaled by distance
	[System.NonSerialized] public float scaleK = 0.0f; // constant
}
