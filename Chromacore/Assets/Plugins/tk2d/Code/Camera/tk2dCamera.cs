using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("2D Toolkit/Camera/tk2dCamera")]
[ExecuteInEditMode]
/// <summary>
/// Maintains a screen resolution camera. 
/// Whole number increments seen through this camera represent one pixel.
/// For example, setting an object to 300, 300 will position it at exactly that pixel position.
/// </summary>
public class tk2dCamera : MonoBehaviour 
{
	static int CURRENT_VERSION = 1;
	public int version = 0;

	[SerializeField] private tk2dCameraSettings cameraSettings = new tk2dCameraSettings();

	/// <summary>
	/// The unity camera settings. 
	/// Use this instead of camera.XXX to change parameters.
	/// </summary>
	public tk2dCameraSettings CameraSettings {
		get {
			return cameraSettings;
		}
	}

	/// <summary>
	/// Resolution overrides, if necessary. See <see cref="tk2dCameraResolutionOverride"/>
	/// </summary>
	public tk2dCameraResolutionOverride[] resolutionOverride = new tk2dCameraResolutionOverride[1] {
		tk2dCameraResolutionOverride.DefaultOverride
	};

	/// <summary>
	/// The currently used override
	/// </summary>
	public tk2dCameraResolutionOverride CurrentResolutionOverride {
		get { 
			tk2dCamera settings = SettingsRoot;
			Camera cam = ScreenCamera;

			float pixelWidth = cam.pixelWidth;
			float pixelHeight = cam.pixelHeight;
#if UNITY_EDITOR
			if (settings.useGameWindowResolutionInEditor) {
				pixelWidth = settings.gameWindowResolution.x;
				pixelHeight = settings.gameWindowResolution.y;
			}
			else if (settings.forceResolutionInEditor)
			{
				pixelWidth = settings.forceResolution.x;
				pixelHeight = settings.forceResolution.y;
			}
#endif

			tk2dCameraResolutionOverride currentResolutionOverride = null;

			if ((currentResolutionOverride == null ||
				(currentResolutionOverride != null && (currentResolutionOverride.width != pixelWidth || currentResolutionOverride.height != pixelHeight))
				))
			{
				currentResolutionOverride = null;

				// find one if it matches the current resolution
				if (settings.resolutionOverride != null)
				{
					foreach (var ovr in settings.resolutionOverride)
					{
						if (ovr.Match((int)pixelWidth, (int)pixelHeight))
						{
							currentResolutionOverride = ovr;
							break;
						}
					}
				}
			}

			return currentResolutionOverride;
		}
	}

	/// <summary>
	/// A tk2dCamera to inherit configuration from. 
	/// All resolution and override settings will be pulled from the root inherited camera.
	/// This allows you to create a tk2dCamera prefab in your project or a master camera
	/// in the scene and guarantee that multiple instances of tk2dCameras referencing this
	/// will use exactly the same paramaters. 
	/// </summary>
	public tk2dCamera InheritConfig {
		get { return inheritSettings; }
		set {
			if (inheritSettings != value) {
				inheritSettings = value;
				_settingsRoot = null;
			}
		}
	}

	[SerializeField]
	private tk2dCamera inheritSettings = null;
	
	/// <summary>
	/// Native resolution width of the camera. Override this in the inspector.
	/// Don't change this at runtime unless you understand the implications.
	/// </summary>
	public int nativeResolutionWidth = 960;

	/// <summary>
	/// Native resolution height of the camera. Override this in the inspector.
	/// Don't change this at runtime unless you understand the implications.
	/// </summary>
	public int nativeResolutionHeight = 640;
	
	[SerializeField]
	private Camera _unityCamera;
	private Camera UnityCamera {
		get {
			if (_unityCamera == null) {
				_unityCamera = camera;
				if (_unityCamera == null) {
					Debug.LogError("A unity camera must be attached to the tk2dCamera script");
				}
			}
			return _unityCamera;
		}
	}


	static tk2dCamera inst;

	/// <summary>
	/// Global instance, used by sprite and textmesh class to quickly find the tk2dCamera instance.
	/// </summary>
	public  static tk2dCamera Instance {
		get {
			return inst;
		}
	}

	// Global instance of active tk2dCameras, used to quickly find cameras matching a particular layer.
	private static List<tk2dCamera> allCameras = new List<tk2dCamera>();

	/// <summary>
	/// Returns the first camera in the list that can "see" this layer, or null if none can be found
	/// </summary>
	public static tk2dCamera CameraForLayer( int layer ) {
		int layerMask = 1 << layer;
		int cameraCount = allCameras.Count;
		for (int i = 0; i < cameraCount; ++i) {
			tk2dCamera cam = allCameras[i];
			if ((cam.UnityCamera.cullingMask & layerMask) == layerMask) {
				return cam;
			}
		}
		return null;
	}

	/// <summary>
	/// Returns screen extents - top, bottom, left and right will be the extent of the physical screen
	/// Regardless of resolution or override
	/// </summary>
	public Rect ScreenExtents { get { return _screenExtents; } }

	/// <summary>
	/// Returns screen extents - top, bottom, left and right will be the extent of the native screen
	/// before it gets scaled and processed by overrides
	/// </summary>
	public Rect NativeScreenExtents { get { return _nativeScreenExtents; } }

	/// <summary>
	/// Enable/disable viewport clipping.
	/// ScreenCamera must be valid for it to be actually enabled when rendering.
	/// </summary>
	public bool viewportClippingEnabled = false;

	/// <summary>
	/// Viewport clipping region.
	/// </summary>
	public Vector4 viewportRegion = new Vector4(0, 0, 100, 100);

	/// <summary>
	/// Target resolution
	/// The target resolution currently being used.
	/// If displaying on a 960x640 display, this will be the number returned here, regardless of scale, etc.
	/// If the editor resolution is forced, the returned value will be the forced resolution.
	/// </summary>
	public Vector2 TargetResolution { get { return _targetResolution; } }
	Vector2 _targetResolution = Vector2.zero;

	/// <summary>
	/// Native resolution
	/// The native resolution of this camera.
	/// This is the native resolution of the camera before any scaling is performed.
	/// The resolution the game is set up to run at initially.
	/// </summary>
	public Vector2 NativeResolution { get { return new Vector2(nativeResolutionWidth, nativeResolutionHeight); } }

	// Some obselete functions, use ScreenExtents instead
	[System.Obsolete] public Vector2 ScreenOffset { get { return new Vector2(ScreenExtents.xMin - NativeScreenExtents.xMin, ScreenExtents.yMin - NativeScreenExtents.yMin); } }
	[System.Obsolete] public Vector2 resolution { get { return new Vector2( ScreenExtents.xMax, ScreenExtents.yMax ); } }
	[System.Obsolete] public Vector2 ScreenResolution { get { return new Vector2( ScreenExtents.xMax, ScreenExtents.yMax ); } }
	[System.Obsolete] public Vector2 ScaledResolution { get { return new Vector2( ScreenExtents.width, ScreenExtents.height ); } }

	/// <summary>
	/// Zooms the current display
	/// A zoom factor of 2 will zoom in 2x, i.e. the object on screen will be twice as large
	/// Anchors will still be anchored, but will be scaled with the zoomScale.
	/// It is recommended to use a second camera for HUDs if necessary to avoid this behaviour.
	/// </summary>
	public float ZoomFactor {
		get { return zoomFactor; }
		set { zoomFactor = Mathf.Max(0.01f, value); }
	}

	// Fallback obselete interface
	[System.Obsolete]
	public float zoomScale {
		get { return 1.0f / Mathf.Max(0.01f, zoomFactor); }
	}

	[SerializeField] float zoomFactor = 1.0f;


	[HideInInspector]
	/// <summary>
	/// Forces the resolution in the editor. This option is only used when tk2dCamera can't detect the game window resolution.
	/// </summary>
	public bool forceResolutionInEditor = false;

	// When true, overrides the "forceResolutionInEditor" flag above
	bool useGameWindowResolutionInEditor = false;
	
	[HideInInspector]
	/// <summary>
	/// The resolution to force the game window to when <see cref="forceResolutionInEditor"/> is enabled.
	/// </summary>
	public Vector2 forceResolution = new Vector2(960, 640);
	
	// Usred when useGameWindowResolutionInEditor == true
	Vector2 gameWindowResolution = new Vector2(960, 640);

	/// <summary>
	/// The camera that sees the screen - i.e. if viewport clipping is enabled, its the camera that sees the entire screen
	/// </summary>
	public Camera ScreenCamera {
		get {
			bool viewportClippingEnabled = this.viewportClippingEnabled && this.inheritSettings != null && this.inheritSettings.UnityCamera.rect == unitRect;
			return viewportClippingEnabled ? this.inheritSettings.UnityCamera : UnityCamera;
		}
	}

	// Use this for initialization
	void Awake () {
		Upgrade();
		if (allCameras.IndexOf(this) == -1) {
			allCameras.Add(this);
		}
	}

	void OnEnable() {
		if (UnityCamera != null) {
			UpdateCameraMatrix();
		}
		else {
			this.camera.enabled = false;
		}
		
		if (!viewportClippingEnabled) // the main camera can't display rect
			inst = this;

		if (allCameras.IndexOf(this) == -1) {
			allCameras.Add(this);
		}
	}

	void OnDestroy() {
		int idx = allCameras.IndexOf(this);
		if (idx != -1) {
			allCameras.RemoveAt( idx );
		}
	}
	
	void OnPreCull() {
		// Commit all pending changes - this more or less guarantees
		// everything is committed before drawing this camera.
		tk2dUpdateManager.FlushQueues();
		UpdateCameraMatrix();
	}

#if UNITY_EDITOR
	void LateUpdate() {
		if (!Application.isPlaying) {
			UpdateCameraMatrix();
		}
	}
#endif

	Rect _screenExtents;
	Rect _nativeScreenExtents;
	Rect unitRect = new Rect(0, 0, 1, 1);

	// Gives you the size of one pixel in world units at the native resolution
	// For perspective cameras, it is dependent on the distance to the camera.
	public float GetSizeAtDistance(float distance) {
		tk2dCameraSettings cameraSettings = SettingsRoot.CameraSettings;
		switch (cameraSettings.projection) {
			case tk2dCameraSettings.ProjectionType.Orthographic:
				if (cameraSettings.orthographicType == tk2dCameraSettings.OrthographicType.PixelsPerMeter) {
					return 1.0f / cameraSettings.orthographicPixelsPerMeter;
				}
				else {
					return 2.0f * cameraSettings.orthographicSize / SettingsRoot.nativeResolutionHeight;
				}
			case tk2dCameraSettings.ProjectionType.Perspective:
				return Mathf.Tan(CameraSettings.fieldOfView * Mathf.Deg2Rad * 0.5f) * distance * 2.0f / SettingsRoot.nativeResolutionHeight;
		}
		return 1;
	}

	
	// This returns the tk2dCamera object which has the settings stored on it
	// Trace back to the source, however far up the hierarchy that may be
	// You can't change this at runtime
	tk2dCamera _settingsRoot;
	public tk2dCamera SettingsRoot {
		get { 
			if (_settingsRoot == null) {
				_settingsRoot = (inheritSettings == null || inheritSettings == this) ? this : inheritSettings.SettingsRoot;	
			}
			return _settingsRoot;
		}
	}

#if UNITY_EDITOR
	public static tk2dCamera Editor__Inst {
		get {
			if (inst != null) {
				return inst;
			}
			return GameObject.FindObjectOfType(typeof(tk2dCamera)) as tk2dCamera;
		}
	}
#endif
	
#if UNITY_EDITOR
	static bool Editor__getGameViewSizeError = false;
	public static bool Editor__gameViewReflectionError = false;

	// Try and get game view size
	// Will return true if it is able to work this out
	// If width / height == 0, it means the user has selected an aspect ratio "Resolution"
	public static bool Editor__GetGameViewSize(out float width, out float height, out float aspect) {
		try {
			Editor__gameViewReflectionError = false;

			System.Type gameViewType = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			System.Reflection.MethodInfo GetMainGameView = gameViewType.GetMethod("GetMainGameView", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
			object mainGameViewInst = GetMainGameView.Invoke(null, null);
			if (mainGameViewInst == null) {
				width = height = aspect = 0;
				return false;
			}
			System.Reflection.FieldInfo s_viewModeResolutions = gameViewType.GetField("s_viewModeResolutions", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
			if (s_viewModeResolutions == null) {
				System.Reflection.PropertyInfo currentGameViewSize = gameViewType.GetProperty("currentGameViewSize", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				object gameViewSize = currentGameViewSize.GetValue(mainGameViewInst, null);
				System.Type gameViewSizeType = gameViewSize.GetType();
				int gvWidth = (int)gameViewSizeType.GetProperty("width").GetValue(gameViewSize, null);
				int gvHeight = (int)gameViewSizeType.GetProperty("height").GetValue(gameViewSize, null);
				int gvSizeType = (int)gameViewSizeType.GetProperty("sizeType").GetValue(gameViewSize, null);
				if (gvWidth == 0 || gvHeight == 0) {
					width = height = aspect = 0;
					return false;
				}
				else if (gvSizeType == 0) {
					width = height = 0;
					aspect = (float)gvWidth / (float)gvHeight;
					return true;
				}
				else {
					width = gvWidth; height = gvHeight;
					aspect = (float)gvWidth / (float)gvHeight;
					return true;
				}
			}
			else {
				Vector2[] viewModeResolutions = (Vector2[])s_viewModeResolutions.GetValue(null);
				float[] viewModeAspects = (float[])gameViewType.GetField("s_viewModeAspects", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).GetValue(null);
				string[] viewModeStrings = (string[])gameViewType.GetField("s_viewModeAspectStrings", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).GetValue(null);
				if (mainGameViewInst != null 
					&& viewModeStrings != null
					&& viewModeResolutions != null && viewModeAspects != null) {
					int aspectRatio = (int)gameViewType.GetField("m_AspectRatio", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).GetValue(mainGameViewInst);
					string thisViewModeString = viewModeStrings[aspectRatio];
					if (thisViewModeString.Contains("Standalone")) {
						width = UnityEditor.PlayerSettings.defaultScreenWidth; height = UnityEditor.PlayerSettings.defaultScreenHeight;
						aspect = width / height;
					}
					else if (thisViewModeString.Contains("Web")) {
						width = UnityEditor.PlayerSettings.defaultWebScreenWidth; height = UnityEditor.PlayerSettings.defaultWebScreenHeight;
						aspect = width / height;
					}
					else {
						width = viewModeResolutions[ aspectRatio ].x; height = viewModeResolutions[ aspectRatio ].y;
						aspect = viewModeAspects[ aspectRatio ];
						// this is an error state
						if (width == 0 && height == 0 && aspect == 0) {
							return false;
						}
					}
					return true;
				}
			}
		}
		catch (System.Exception e) {
			if (Editor__getGameViewSizeError == false) {
				Debug.LogError("tk2dCamera.GetGameViewSize - has a Unity update broken this?\nThis is not a fatal error, but a warning that you've probably not got the latest 2D Toolkit update.\n\n" + e.ToString());
				Editor__getGameViewSizeError = true;
			}
			Editor__gameViewReflectionError = true;
		}
		width = height = aspect = 0;
		return false;
	}
#endif

	public Matrix4x4 OrthoOffCenter(Vector2 scale, float left, float right, float bottom, float top, float near, float far) {
		// Additional half texel offset
		// Takes care of texture unit offset, if necessary.
		
		float x =  (2.0f) / (right - left) * scale.x;
		float y = (2.0f) / (top - bottom) * scale.y;
		float z = -2.0f / (far - near);

		float a = -(right + left) / (right - left);
		float b = -(bottom + top) / (top - bottom);
		float c = -(far + near) / (far - near);
		
		Matrix4x4 m = new Matrix4x4();
		m[0,0] = x;  m[0,1] = 0;  m[0,2] = 0;  m[0,3] = a;
		m[1,0] = 0;  m[1,1] = y;  m[1,2] = 0;  m[1,3] = b;
		m[2,0] = 0;  m[2,1] = 0;  m[2,2] = z;  m[2,3] = c;
		m[3,0] = 0;  m[3,1] = 0;  m[3,2] = 0;  m[3,3] = 1;

		return m;
	}

	Vector2 GetScaleForOverride(tk2dCamera settings, tk2dCameraResolutionOverride currentOverride, float width, float height) {
		Vector2 scale = Vector2.one;
		float s = 1.0f;

		if (currentOverride == null) {
			return scale;
		}

		switch (currentOverride.autoScaleMode)
		{
		case tk2dCameraResolutionOverride.AutoScaleMode.PixelPerfect:
			s = 1;
			scale.Set(s, s);
			break;

		case tk2dCameraResolutionOverride.AutoScaleMode.FitHeight: 
			s = height / settings.nativeResolutionHeight; 
			scale.Set(s, s);
			break;

		case tk2dCameraResolutionOverride.AutoScaleMode.FitWidth: 
			s = width / settings.nativeResolutionWidth; 
			scale.Set(s, s);
			break;

		case tk2dCameraResolutionOverride.AutoScaleMode.FitVisible:
		case tk2dCameraResolutionOverride.AutoScaleMode.ClosestMultipleOfTwo:
			float nativeAspect = (float)settings.nativeResolutionWidth / settings.nativeResolutionHeight;
			float currentAspect = width / height;
			if (currentAspect < nativeAspect)
				s = width / settings.nativeResolutionWidth;
			else
				s = height / settings.nativeResolutionHeight;
			
			if (currentOverride.autoScaleMode == tk2dCameraResolutionOverride.AutoScaleMode.ClosestMultipleOfTwo)
			{
				if (s > 1.0f)
					s = Mathf.Floor(s); // round number
				else
					s = Mathf.Pow(2, Mathf.Floor(Mathf.Log(s, 2))); // minimise only as power of two
			}
			
			scale.Set(s, s);
			break;

		case tk2dCameraResolutionOverride.AutoScaleMode.StretchToFit:
			scale.Set(width / settings.nativeResolutionWidth, height / settings.nativeResolutionHeight);
			break;

		default:
		case tk2dCameraResolutionOverride.AutoScaleMode.None: 
			s = currentOverride.scale;
			scale.Set(s, s);
			break;
		}

		return scale;
	}

	Vector2 GetOffsetForOverride(tk2dCamera settings, tk2dCameraResolutionOverride currentOverride, Vector2 scale, float width, float height) {
		Vector2 offset = Vector2.zero;
		if (currentOverride == null) {
			return offset;
		}

		switch (currentOverride.fitMode) {
			case tk2dCameraResolutionOverride.FitMode.Center:
				if (settings.cameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.BottomLeft) {
					offset = new Vector2(Mathf.Round((settings.nativeResolutionWidth  * scale.x - width ) / 2.0f), 
										 Mathf.Round((settings.nativeResolutionHeight * scale.y - height) / 2.0f));
				}
				break;
				
			default:
			case tk2dCameraResolutionOverride.FitMode.Constant: 
				offset = -currentOverride.offsetPixels; 
				break;
		}
		return offset;
	}

#if UNITY_EDITOR
	private Matrix4x4 Editor__GetPerspectiveMatrix() {
		float aspect = (float)nativeResolutionWidth / (float)nativeResolutionHeight;
		return Matrix4x4.Perspective(SettingsRoot.CameraSettings.fieldOfView, aspect, UnityCamera.nearClipPlane, UnityCamera.farClipPlane);
	}

	public Matrix4x4 Editor__GetNativeProjectionMatrix(  ) {
		tk2dCamera settings = SettingsRoot;
		if (settings.CameraSettings.projection == tk2dCameraSettings.ProjectionType.Perspective) {
			return Editor__GetPerspectiveMatrix();
		}
		Rect rect1 = new Rect(0, 0, 1, 1);
		Rect rect2 = new Rect(0, 0, 1, 1);
		return GetProjectionMatrixForOverride( settings, null, nativeResolutionWidth, nativeResolutionHeight, false, out rect1, out rect2 );
	}

	public Matrix4x4 Editor__GetFinalProjectionMatrix(  ) {
		tk2dCamera settings = SettingsRoot;
		if (settings.CameraSettings.projection == tk2dCameraSettings.ProjectionType.Perspective) {
			return Editor__GetPerspectiveMatrix();
		}
		Vector2 resolution = GetScreenPixelDimensions(settings);
		Rect rect1 = new Rect(0, 0, 1, 1);
		Rect rect2 = new Rect(0, 0, 1, 1);
		return GetProjectionMatrixForOverride( settings, settings.CurrentResolutionOverride, resolution.x, resolution.y, false, out rect1, out rect2 );
	}
#endif

	Matrix4x4 GetProjectionMatrixForOverride( tk2dCamera settings, tk2dCameraResolutionOverride currentOverride, float pixelWidth, float pixelHeight, bool halfTexelOffset, out Rect screenExtents, out Rect unscaledScreenExtents ) {
		Vector2 scale = GetScaleForOverride( settings, currentOverride, pixelWidth, pixelHeight );
		Vector2 offset = GetOffsetForOverride( settings, currentOverride, scale, pixelWidth, pixelHeight);
		
		float left = offset.x, bottom = offset.y;
		float right = pixelWidth + offset.x, top = pixelHeight + offset.y;
		Vector2 nativeResolutionOffset = Vector2.zero;

		// Correct for viewport clipping rendering
		// Coordinates in subrect are "native" pixels, but origin is from the extrema of screen
		if (this.viewportClippingEnabled && this.InheritConfig != null) {
			float vw = (right - left) / scale.x;
			float vh = (top - bottom) / scale.y;
			Vector4 sr = new Vector4((int)this.viewportRegion.x, (int)this.viewportRegion.y,
									 (int)this.viewportRegion.z, (int)this.viewportRegion.w);

			float viewportLeft = -offset.x / pixelWidth + sr.x / vw;
			float viewportBottom = -offset.y / pixelHeight + sr.y / vh;
			float viewportWidth = sr.z / vw;
			float viewportHeight = sr.w / vh;

			Rect r = new Rect( viewportLeft, viewportBottom, viewportWidth, viewportHeight );
			if (UnityCamera.rect.x != viewportLeft ||
				UnityCamera.rect.y != viewportBottom ||
				UnityCamera.rect.width != viewportWidth ||
				UnityCamera.rect.height != viewportHeight) {
				UnityCamera.rect = r;
			}

			float maxWidth = Mathf.Min( 1.0f - r.x, r.width );
			float maxHeight = Mathf.Min( 1.0f - r.y, r.height );

			float rectOffsetX = sr.x * scale.x - offset.x;
			float rectOffsetY = sr.y * scale.y - offset.y;

			if (r.x < 0.0f) {
				rectOffsetX += -r.x * pixelWidth;
				maxWidth = (r.x + r.width);
			}
			if (r.y < 0.0f) {
				rectOffsetY += -r.y * pixelHeight;
				maxHeight = (r.y + r.height);
			}

			left += rectOffsetX;
			bottom += rectOffsetY;
			right = pixelWidth * maxWidth + offset.x + rectOffsetX;
			top = pixelHeight * maxHeight + offset.y +  rectOffsetY;
		}
		else {
			if (UnityCamera.rect != CameraSettings.rect) {
				UnityCamera.rect = CameraSettings.rect;
			}
		}

		// By default the camera is orthographic, bottom left, 1 pixel per meter
		if (settings.cameraSettings.orthographicOrigin == tk2dCameraSettings.OrthographicOrigin.Center) {
			float w = (right - left) * 0.5f;
			left -= w; right -= w;
			float h = (top - bottom) * 0.5f;
			top -= h; bottom -= h;
			nativeResolutionOffset.Set(-nativeResolutionWidth / 2.0f, -nativeResolutionHeight / 2.0f);
		}

		float orthoSize = settings.cameraSettings.orthographicSize;
		switch (settings.cameraSettings.orthographicType) {
			case tk2dCameraSettings.OrthographicType.OrthographicSize:
				orthoSize = 2.0f * settings.cameraSettings.orthographicSize / settings.nativeResolutionHeight;
				break;
			case tk2dCameraSettings.OrthographicType.PixelsPerMeter:
				orthoSize = 1.0f / settings.cameraSettings.orthographicPixelsPerMeter;
				break;
		}

		float zoomScale = 1.0f / ZoomFactor;

		// Only need the half texel offset on PC/D3D
		bool needHalfTexelOffset = (Application.platform == RuntimePlatform.WindowsPlayer ||
						   			Application.platform == RuntimePlatform.WindowsWebPlayer ||
						   			Application.platform == RuntimePlatform.WindowsEditor);
		float halfTexel = (halfTexelOffset && needHalfTexelOffset) ? 0.5f : 0.0f;

		float s = orthoSize * zoomScale;
		screenExtents = new Rect(left * s / scale.x, bottom * s / scale.y, 
						   		 (right - left) * s / scale.x, (top - bottom) * s / scale.y);

		unscaledScreenExtents = new Rect(nativeResolutionOffset.x * s, nativeResolutionOffset.y * s,
										 nativeResolutionWidth * s, nativeResolutionHeight * s);

		// Near and far clip planes are tweakable per camera, so we pull from current camera instance regardless of inherited values
		return OrthoOffCenter(scale, orthoSize * (left + halfTexel) * zoomScale, orthoSize * (right + halfTexel) * zoomScale, 
									 orthoSize * (bottom - halfTexel) * zoomScale, orthoSize * (top - halfTexel) * zoomScale, 
									 UnityCamera.nearClipPlane, UnityCamera.farClipPlane);
	}

	Vector2 GetScreenPixelDimensions(tk2dCamera settings) {
		Vector2 dimensions = new Vector2(ScreenCamera.pixelWidth, ScreenCamera.pixelHeight);

#if UNITY_EDITOR
		// This bit here allocates memory, but only runs in the editor
		float gameViewPixelWidth = 0, gameViewPixelHeight = 0;
		float gameViewAspect = 0;
		settings.useGameWindowResolutionInEditor = false;
		if (Editor__GetGameViewSize( out gameViewPixelWidth, out gameViewPixelHeight, out gameViewAspect)) {
			if (gameViewPixelWidth != 0 && gameViewPixelHeight != 0) {
				if (!settings.useGameWindowResolutionInEditor ||
					settings.gameWindowResolution.x != gameViewPixelWidth ||
					settings.gameWindowResolution.y != gameViewPixelHeight) {
					settings.useGameWindowResolutionInEditor = true;
					settings.gameWindowResolution.x = gameViewPixelWidth;
					settings.gameWindowResolution.y = gameViewPixelHeight;
				}
				dimensions.x = settings.gameWindowResolution.x;
				dimensions.y = settings.gameWindowResolution.y;
			}
		}

		if (!settings.useGameWindowResolutionInEditor && settings.forceResolutionInEditor)
		{
			dimensions.x = settings.forceResolution.x;
			dimensions.y = settings.forceResolution.y;
		}
#endif
		
		return dimensions;
	}

	private void Upgrade() {
		if (version != CURRENT_VERSION) {
			if (version == 0) {
				// Backwards compatibility
				cameraSettings.orthographicPixelsPerMeter = 1;
				cameraSettings.orthographicType = tk2dCameraSettings.OrthographicType.PixelsPerMeter;
				cameraSettings.orthographicOrigin = tk2dCameraSettings.OrthographicOrigin.BottomLeft;
				cameraSettings.projection = tk2dCameraSettings.ProjectionType.Orthographic;

				foreach (tk2dCameraResolutionOverride ovr in resolutionOverride) {
					ovr.Upgrade( version );
				}

				// Mirror camera settings
				Camera unityCamera = camera;
				if (unityCamera != null) {
					cameraSettings.rect = unityCamera.rect;
					if (!unityCamera.isOrthoGraphic) {
						cameraSettings.projection = tk2dCameraSettings.ProjectionType.Perspective;
						cameraSettings.fieldOfView = unityCamera.fieldOfView * ZoomFactor;
					}

					unityCamera.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
				}
			}

			Debug.Log("tk2dCamera '" + this.name + "' - Upgraded from version " + version.ToString());
			version = CURRENT_VERSION;
		}
	}

	/// <summary>
	/// Updates the camera matrix to ensure 1:1 pixel mapping
	/// Or however the override is set up.
	/// </summary>
	public void UpdateCameraMatrix()
	{
		Upgrade();

		if (!this.viewportClippingEnabled)
			inst = this;

		Camera unityCamera = UnityCamera;
		tk2dCamera settings = SettingsRoot;
		tk2dCameraSettings inheritedCameraSettings = settings.CameraSettings;

		if (unityCamera.rect != cameraSettings.rect) unityCamera.rect = cameraSettings.rect;

		// Projection type is inherited from base camera
		_targetResolution = GetScreenPixelDimensions(settings);

		if (inheritedCameraSettings.projection == tk2dCameraSettings.ProjectionType.Perspective) {
			if (unityCamera.orthographic == true) unityCamera.orthographic = false;
			float fov = Mathf.Min(179.9f, inheritedCameraSettings.fieldOfView / Mathf.Max(0.001f, ZoomFactor));
			if (unityCamera.fieldOfView != fov) unityCamera.fieldOfView = fov;
			_screenExtents.Set( -unityCamera.aspect, -1, unityCamera.aspect * 2, 2 );
			_nativeScreenExtents = _screenExtents;
			unityCamera.ResetProjectionMatrix();
		}
		else {
			if (unityCamera.orthographic == false) unityCamera.orthographic = true;
			// Find an override if necessary
			Matrix4x4 m = GetProjectionMatrixForOverride( settings, settings.CurrentResolutionOverride, _targetResolution.x, _targetResolution.y, true, out _screenExtents, out _nativeScreenExtents );

#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_1)
			// Windows phone?
			if (Application.platform == RuntimePlatform.WP8Player &&
			    (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)) {
				float angle = (Screen.orientation == ScreenOrientation.LandscapeRight) ? 90.0f : -90.0f;
				Matrix4x4 m2 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);
				m = m2 * m;
			}			
#endif

			if (unityCamera.projectionMatrix != m) {
				unityCamera.projectionMatrix = m;
			}
		}
	}
}
