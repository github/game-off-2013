using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(tk2dCamera))]
public class tk2dCameraEditor : Editor 
{
	struct Preset
	{
		public string name;
		public int width;
		public int height;
		public float aspect;
		public Preset(string name, int width, int height) { this.name = name; this.width = width; this.height = height; this.aspect = (float)this.width / (float)this.height; }
		public Preset(string name, int width, int height, float aspect) { this.name = name; this.width = width; this.height = height; this.aspect = aspect; }
		public bool MatchAspect( float aspect ) {
			return (width == -1 && height == -1) || Mathf.Abs(aspect - this.aspect) < 0.01f;
		}
	}

	Preset[] presets = new Preset[] {
		new Preset("iOS/iPhone 3G Tall", 320, 480),
		new Preset("iOS/iPhone 3G Wide", 480, 320),
		new Preset("iOS/iPhone 5 Tall", 640, 1136, 0.563380282f),
		new Preset("iOS/iPhone 5 Wide", 1136, 640, 1.777777778f),
		new Preset("iOS/iPhone 4 Tall", 640, 960),
		new Preset("iOS/iPhone 4 Wide", 960, 640),
		new Preset("iOS/iPad Tall", 768, 1024),
		new Preset("iOS/iPad Wide", 1024, 768),
		new Preset("iOS/iPad 3 Tall", 1536, 2048),
		new Preset("iOS/iPad 3 Wide", 2048, 1536),

		new Preset("Android/HTC Legend Tall", 480, 320),
		new Preset("Android/HTC Legend Wide", 320, 480),
		new Preset("Android/Nexus One Tall", 480, 800),
		new Preset("Android/Nexus One Wide", 800, 480),
		new Preset("Android/MotorolaDroidX Tall", 480, 854),
		new Preset("Android/MotorolaDroidX Wide", 854, 480),
		new Preset("Android/MotorolaDroidX2 Tall", 540, 960),
		new Preset("Android/MotorolaDroidX2 Wide", 960, 540),
		new Preset("Android/Tegra Tablet Tall", 600, 1024),
		new Preset("Android/Tegra Tablet Wide", 1024, 600),
		new Preset("Android/Nexus7 Tall", 800, 1280),
		new Preset("Android/Nexus7 Wide", 1280, 800),

		new Preset("TV/720p", 1280, 720),
		new Preset("TV/1080p", 1920, 1080),

		new Preset("PC/4:3", 640, 480),
		new Preset("PC/4:3", 800, 600),
		new Preset("PC/4:3", 1024, 768),

		new Preset("Custom", -1, -1),
	};

	static int toolbarSelection = 0;
	int refreshCount = 0;

	Vector2 ResolutionControl(string label, Vector2 resolution) {
		GUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(label);
		resolution.x = EditorGUILayout.IntField((int)resolution.x, GUILayout.Width(60));
		GUILayout.Label("x", GUILayout.ExpandWidth(false));
		resolution.y = EditorGUILayout.IntField((int)resolution.y, GUILayout.Width(60));
		GUILayout.EndHorizontal();
		return resolution;
	}

	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector();
		tk2dCamera _target = (tk2dCamera)target;
	
		// sanity
		if (_target.resolutionOverride == null)
		{
			_target.resolutionOverride = new tk2dCameraResolutionOverride[0];
			GUI.changed = true;
		}

		string[] toolbarButtons = new string[] { "General", "Camera", "Overrides", "Advanced" };
		toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarButtons);
		GUILayout.Space(16);
		
		if (toolbarSelection == 0) {
			GUILayout.BeginHorizontal();
			tk2dCamera newInherit = EditorGUILayout.ObjectField("Inherit config", _target.InheritConfig, typeof(tk2dCamera), true) as tk2dCamera;
			if (newInherit != _target.InheritConfig) {
				if (newInherit != _target) {
					_target.InheritConfig = newInherit;
				}
				else {
					EditorUtility.DisplayDialog("Error", "Can't inherit from self", "Ok");
				}
			}
			if (_target.InheritConfig != null && GUILayout.Button("Clear", GUILayout.ExpandWidth(false))) {
				_target.InheritConfig = null;
				GUI.changed = true;
			}
			GUILayout.EndHorizontal();

			GUI.enabled = _target.SettingsRoot == _target;
			DrawConfigGUI( _target.SettingsRoot );
			GUI.enabled = true;

			GUILayout.Space(16);
			_target.ZoomFactor = EditorGUILayout.FloatField("Zoom factor", _target.ZoomFactor);
		}

		if (toolbarSelection == 1) {
			DrawCameraGUI(_target, true);
		}

		if (toolbarSelection == 2) {
			// Overrides
			DrawOverrideGUI(_target);
		}

		if (toolbarSelection == 3) {

			bool isPerspective = _target.SettingsRoot.CameraSettings.projection == tk2dCameraSettings.ProjectionType.Perspective;

			EditorGUILayout.LabelField("Anchored Viewport Clipping", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			if (_target.InheritConfig == null || isPerspective) {
				_target.viewportClippingEnabled = false;
				GUI.enabled = false;
				EditorGUILayout.Toggle("Enable", false);
				GUI.enabled = true;
				if (isPerspective) {
					tk2dGuiUtility.InfoBox("Anchored viewport clipping not allowed on perspective cameras.\n", tk2dGuiUtility.WarningLevel.Error);
				}
				else {
					tk2dGuiUtility.InfoBox("Anchored viewport clipping not allowed on this camera.\nAttach a link to a camera which displays the entire screen to enable viewport clipping.", tk2dGuiUtility.WarningLevel.Error);	
				}
			}
			else {
				_target.viewportClippingEnabled = EditorGUILayout.Toggle("Enable", _target.viewportClippingEnabled);
				if (_target.viewportClippingEnabled) {
					EditorGUILayout.LabelField("Region");
					EditorGUI.indentLevel++;
					_target.viewportRegion.x = EditorGUILayout.IntField("X", (int)_target.viewportRegion.x);
					_target.viewportRegion.y = EditorGUILayout.IntField("Y", (int)_target.viewportRegion.y);
					_target.viewportRegion.z = EditorGUILayout.IntField("Width", (int)_target.viewportRegion.z);
					_target.viewportRegion.w = EditorGUILayout.IntField("Height", (int)_target.viewportRegion.w);
					EditorGUI.indentLevel--;
				}
			}
			EditorGUI.indentLevel--;
		}

		if (GUI.changed)
		{
			_target.UpdateCameraMatrix();
			EditorUtility.SetDirty(target);
			tk2dCameraAnchor[] allAlignmentObjects = GameObject.FindObjectsOfType(typeof(tk2dCameraAnchor)) as tk2dCameraAnchor[];
			foreach (var v in allAlignmentObjects)
			{
				EditorUtility.SetDirty(v);
			}
		}
		
		GUILayout.Space(16.0f);
	}

	void DrawConfigGUI(tk2dCamera _target) {
		bool cameraOverrideChanged = false;
		GUILayout.Space(8);

		// Game view stuff		
		float gameViewPixelWidth = 0, gameViewPixelHeight = 0;
		float gameViewAspect = 0;
		bool gameViewFound = tk2dCamera.Editor__GetGameViewSize( out gameViewPixelWidth, out gameViewPixelHeight, out gameViewAspect);
		bool gameViewReflectionError = tk2dCamera.Editor__gameViewReflectionError;
		bool gameViewResolutionSet = (gameViewFound && gameViewPixelWidth != 0 && gameViewPixelHeight != 0);
		if (!gameViewFound && ++refreshCount < 3) { 
			HandleUtility.Repaint();
		}

		// Native resolution
		Vector2 nativeRes = new Vector2(_target.nativeResolutionWidth, _target.nativeResolutionHeight);
		EditorGUI.BeginChangeCheck();
		nativeRes = ResolutionControl("Native Resolution", nativeRes);
		if (EditorGUI.EndChangeCheck()) {
			_target.nativeResolutionWidth = (int)nativeRes.x;
			_target.nativeResolutionHeight = (int)nativeRes.y;
		}

		// Preview resolution
		if (gameViewFound && gameViewResolutionSet) {
			if (_target.forceResolutionInEditor == false || _target.forceResolution.x != gameViewPixelWidth || _target.forceResolution.y != gameViewPixelHeight) {
				_target.forceResolutionInEditor = true;
				_target.forceResolution.Set( gameViewPixelWidth, gameViewPixelHeight );
				GUI.changed = true;
			}

			ResolutionControl("Preview Resolution", _target.forceResolution);
		}
		else {
			EditorGUILayout.LabelField("Preview Resolution");
			EditorGUI.indentLevel++;

			GUIContent toggleLabel = new GUIContent("Force Resolution", 
				"When enabled, forces the resolution in the editor regardless of the size of the game window.");

			if (gameViewReflectionError) {
				EditorGUILayout.HelpBox("Game window resolution can't be detected.\n\n" + 
					"tk2dCamera can't detect the game view resolution, possibly because of a Unity upgrade. You might need a 2D Toolkit update, or alternatively pick a resolution below.\n", MessageType.Error);
			}
			else if (gameViewFound) {
				EditorGUILayout.HelpBox("Game window has an aspect ratio selected.\n\n" + 
					"tk2dCamera doesn't know your preview resolution, select from the list below, or pick a resolution in the game view instead.\n", MessageType.Info);
			}
			else {
				EditorGUILayout.HelpBox("Unable to detect game window resolution.\n\n" + 
					"Ensure that the game window resolution is set, instead of selecting Free Aspect.\n", MessageType.Error);
			}

			tk2dGuiUtility.BeginChangeCheck();
			_target.forceResolutionInEditor = EditorGUILayout.Toggle(toggleLabel, _target.forceResolutionInEditor);
			if (tk2dGuiUtility.EndChangeCheck()) cameraOverrideChanged = true;

			if (_target.forceResolutionInEditor)
			{
				tk2dGuiUtility.BeginChangeCheck();

				List<Preset> presetList = null;
				if (gameViewFound) {
					presetList = new List<Preset>(from t in presets where t.MatchAspect( gameViewAspect ) select t);
				}
				else {
					presetList = new List<Preset>( presets );
				}

				int currentSelectedResolution = presetList.FindIndex( x => x.width == _target.forceResolution.x && x.height == _target.forceResolution.y );
				if (currentSelectedResolution == -1) {
					currentSelectedResolution = presetList.Count - 1;
				}

				string[] presetNames = (from t in presetList select t.name).ToArray();
				int selectedResolution = EditorGUILayout.Popup("Preset", currentSelectedResolution, presetNames);
				if (selectedResolution != presetList.Count && selectedResolution != currentSelectedResolution)
				{
					var preset = presetList[selectedResolution];
					_target.forceResolution.x = preset.width;
					_target.forceResolution.y = preset.height;
					GUI.changed = true;
				}

				if (gameViewFound // we only want to display warning when a resolution hasn't been selected from the list of known res / aspects
					&& currentSelectedResolution == presetList.Count - 1) {
					float targetAspect = _target.forceResolution.y / Mathf.Max(_target.forceResolution.x, 0.01f);
					if (Mathf.Abs(targetAspect - gameViewAspect) > 0.01f) {
						EditorGUILayout.HelpBox("The preview resolution looks incorrect.\n\n" + 
												"It looks like you've selected a preview resolution that doesn't match the game view aspect ratio.\n", MessageType.Error);
					}
				}

				_target.forceResolution.x = EditorGUILayout.IntField("Width", (int)_target.forceResolution.x);
				_target.forceResolution.y = EditorGUILayout.IntField("Height", (int)_target.forceResolution.y);

				// clamp to a sensible value
				_target.forceResolution.x = Mathf.Max(_target.forceResolution.x, 50);
				_target.forceResolution.y = Mathf.Max(_target.forceResolution.y, 50);

				if (tk2dGuiUtility.EndChangeCheck())
					cameraOverrideChanged = true;
			}
			else
			{
				EditorGUILayout.FloatField("Width", _target.TargetResolution.x);
				EditorGUILayout.FloatField("Height", _target.TargetResolution.y);
			}
			EditorGUI.indentLevel--;

		}

		// Camera GUI is not available when inheriting configuration
		GUILayout.Space(16);
		DrawCameraGUI(_target, false);


		if (cameraOverrideChanged) {
			// Propagate values to all tk2dCameras in scene
			tk2dCamera[] otherCameras = Resources.FindObjectsOfTypeAll(typeof(tk2dCamera)) as tk2dCamera[];
			foreach (tk2dCamera thisCamera in otherCameras)
			{
				thisCamera.forceResolutionInEditor = _target.forceResolutionInEditor;
				thisCamera.forceResolution = _target.forceResolution;
				thisCamera.UpdateCameraMatrix();
			}

			// Update all anchors after that
			tk2dCameraAnchor[] anchors = Resources.FindObjectsOfTypeAll(typeof(tk2dCameraAnchor)) as tk2dCameraAnchor[];
			foreach (var anchor in anchors)
				anchor.ForceUpdateTransform();					
		}
	}

	void DrawCameraGUI(tk2dCamera target, bool complete) {
		bool allowProjectionParameters = target.SettingsRoot == target;
		bool oldGuiEnabled = GUI.enabled;

		SerializedObject so = this.serializedObject;
		SerializedObject cam = new SerializedObject( target.camera );

		SerializedProperty m_ClearFlags = cam.FindProperty("m_ClearFlags");
		SerializedProperty m_BackGroundColor = cam.FindProperty("m_BackGroundColor");
		SerializedProperty m_CullingMask = cam.FindProperty("m_CullingMask");
		SerializedProperty m_TargetTexture = cam.FindProperty("m_TargetTexture");
		SerializedProperty m_Near = cam.FindProperty("near clip plane");
		SerializedProperty m_Far = cam.FindProperty("far clip plane");
		SerializedProperty m_Depth = cam.FindProperty("m_Depth");
		SerializedProperty m_RenderingPath = cam.FindProperty("m_RenderingPath");
		SerializedProperty m_HDR = cam.FindProperty("m_HDR");
		TransparencySortMode transparencySortMode = target.camera.transparencySortMode;

		if (complete) {
			EditorGUILayout.PropertyField( m_ClearFlags );
			EditorGUILayout.PropertyField( m_BackGroundColor );
			EditorGUILayout.PropertyField( m_CullingMask );
			EditorGUILayout.Space();
		}

		tk2dCameraSettings cameraSettings = target.CameraSettings;
		tk2dCameraSettings inheritedSettings = target.SettingsRoot.CameraSettings;

		GUI.enabled &= allowProjectionParameters;
		inheritedSettings.projection = (tk2dCameraSettings.ProjectionType)EditorGUILayout.EnumPopup("Projection", inheritedSettings.projection);
		EditorGUI.indentLevel++;
		if (inheritedSettings.projection == tk2dCameraSettings.ProjectionType.Orthographic) {
			inheritedSettings.orthographicType = (tk2dCameraSettings.OrthographicType)EditorGUILayout.EnumPopup("Type", inheritedSettings.orthographicType);
			switch (inheritedSettings.orthographicType) {
				case tk2dCameraSettings.OrthographicType.OrthographicSize:
					inheritedSettings.orthographicSize = Mathf.Max( 0.001f, EditorGUILayout.FloatField("Orthographic Size", inheritedSettings.orthographicSize) );
					break;
				case tk2dCameraSettings.OrthographicType.PixelsPerMeter:
					inheritedSettings.orthographicPixelsPerMeter = Mathf.Max( 0.001f, EditorGUILayout.FloatField("Pixels per Meter", inheritedSettings.orthographicPixelsPerMeter) );
					break;
			}
			inheritedSettings.orthographicOrigin = (tk2dCameraSettings.OrthographicOrigin)EditorGUILayout.EnumPopup("Origin", inheritedSettings.orthographicOrigin);
		}
		else if (inheritedSettings.projection == tk2dCameraSettings.ProjectionType.Perspective) {
			inheritedSettings.fieldOfView = EditorGUILayout.Slider("Field of View", inheritedSettings.fieldOfView, 1, 179);
			transparencySortMode = (TransparencySortMode)EditorGUILayout.EnumPopup("Sort mode", transparencySortMode);
		}
		EditorGUI.indentLevel--;
		GUI.enabled = oldGuiEnabled;

		if (complete) {
			EditorGUILayout.Space();
			GUILayout.Label("Clipping Planes");
			GUILayout.BeginHorizontal();
			GUILayout.Space(14);
			GUILayout.Label("Near");
			if (m_Near != null) EditorGUILayout.PropertyField(m_Near, GUIContent.none, GUILayout.Width(60) );
			GUILayout.Label("Far");
			if (m_Far != null) EditorGUILayout.PropertyField(m_Far, GUIContent.none, GUILayout.Width(60) );
			GUILayout.EndHorizontal();
			cameraSettings.rect = EditorGUILayout.RectField("Normalized View Port Rect", cameraSettings.rect);

			EditorGUILayout.Space();
			if (m_Depth != null) EditorGUILayout.PropertyField(m_Depth);
			if (m_RenderingPath != null) EditorGUILayout.PropertyField(m_RenderingPath);
			if (m_TargetTexture != null) EditorGUILayout.PropertyField(m_TargetTexture);
			if (m_HDR != null) EditorGUILayout.PropertyField(m_HDR);
		}

		cam.ApplyModifiedProperties();
		so.ApplyModifiedProperties();

		if (transparencySortMode != target.camera.transparencySortMode) {
			target.camera.transparencySortMode = transparencySortMode;
			EditorUtility.SetDirty(target.camera);
		}
	}

	void DrawToolsGUI() {
		EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		if (GUILayout.Button("Create Anchor", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
		{
			tk2dCamera cam = (tk2dCamera)target;
			
			GameObject go = new GameObject("Anchor");
			go.transform.parent = cam.transform;
			tk2dCameraAnchor cameraAnchor = go.AddComponent<tk2dCameraAnchor>();
			cameraAnchor.AnchorCamera = cam.camera;
			tk2dCameraAnchorEditor.UpdateAnchorName( cameraAnchor );
			
			EditorGUIUtility.PingObject(go);
		}
		EditorGUILayout.EndHorizontal();
	}

	void DrawOverrideGUI(tk2dCamera _camera) {
		var frameBorderStyle = EditorStyles.textField;

		EditorGUIUtility.LookLikeControls(64);

		tk2dCamera _target = _camera.SettingsRoot;
		if (_target.CameraSettings.projection == tk2dCameraSettings.ProjectionType.Perspective) {
			tk2dGuiUtility.InfoBox("Overrides not supported with perspective camera.", tk2dGuiUtility.WarningLevel.Info);
		}
		else {
			GUI.enabled = _target == _camera;

			tk2dCameraResolutionOverride usedOverride = _target.CurrentResolutionOverride;

			if (_target.resolutionOverride.Length == 0) {
				EditorGUILayout.HelpBox("There are no overrides on this tk2dCamera.\n\nThe camera will always scale itself to be pixel perfect at any resolution. " +
					"Add an override if you wish to change this behaviour.", MessageType.Warning);
			}
			else {
				EditorGUILayout.HelpBox("Matching is performed from top to bottom. The first override matching the current resolution will be used.", MessageType.Info);
			}

			System.Action<int> deferredAction = null;
			for (int i = 0; i < _target.resolutionOverride.Length; ++i)
			{
				tk2dCameraResolutionOverride ovr = _target.resolutionOverride[i];

				EditorGUILayout.BeginVertical(frameBorderStyle);
				GUILayout.Space(8);
				GUILayout.BeginHorizontal();
				ovr.name = EditorGUILayout.TextField("Name", ovr.name);

				GUI.enabled = (i != _target.resolutionOverride.Length - 1);
				if (GUILayout.Button("", tk2dEditorSkin.SimpleButton("btn_down")))
				{
					int idx = i;
					deferredAction = delegate(int q) {
						_target.resolutionOverride[idx] = _target.resolutionOverride[idx+1];
						_target.resolutionOverride[idx+1] = ovr;
					};
				}
				
				GUI.enabled = (i != 0);
				if (GUILayout.Button("", tk2dEditorSkin.SimpleButton("btn_up")))
				{
					int idx = i;
					deferredAction = delegate(int q) {
						_target.resolutionOverride[idx] = _target.resolutionOverride[idx-1];
						_target.resolutionOverride[idx-1] = ovr;
					};
				}

				GUI.enabled = true;
				if (GUILayout.Button("", tk2dEditorSkin.GetStyle("TilemapDeleteItem"))) {
					int idx = i;
					deferredAction = delegate(int q) {
						List<tk2dCameraResolutionOverride> list = new List<tk2dCameraResolutionOverride>(_target.resolutionOverride);
						list.RemoveAt(idx);
						_target.resolutionOverride = list.ToArray();
					};
				}

				GUILayout.EndHorizontal();

				ovr.matchBy = (tk2dCameraResolutionOverride.MatchByType)EditorGUILayout.EnumPopup("Match By", ovr.matchBy);

				int tmpIndent = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				switch (ovr.matchBy) {
					case tk2dCameraResolutionOverride.MatchByType.Wildcard:
						break;
					case tk2dCameraResolutionOverride.MatchByType.Resolution:
						Vector2 res = new Vector2(ovr.width, ovr.height);
						res = ResolutionControl(" ", res);
						ovr.width = (int)res.x;
						ovr.height = (int)res.y;
						break;
					case tk2dCameraResolutionOverride.MatchByType.AspectRatio:
						GUILayout.BeginHorizontal();
						EditorGUILayout.PrefixLabel(" ");
						ovr.aspectRatioNumerator = EditorGUILayout.FloatField(ovr.aspectRatioNumerator, GUILayout.Width(40));
						GUILayout.Label(":", GUILayout.ExpandWidth(false));
						ovr.aspectRatioDenominator = EditorGUILayout.FloatField(ovr.aspectRatioDenominator, GUILayout.Width(40));
						GUILayout.EndHorizontal();
						break;
				}
				EditorGUI.indentLevel = tmpIndent;

				ovr.autoScaleMode = (tk2dCameraResolutionOverride.AutoScaleMode)EditorGUILayout.EnumPopup("Auto Scale", ovr.autoScaleMode);
				if (ovr.autoScaleMode == tk2dCameraResolutionOverride.AutoScaleMode.None)
				{
					EditorGUI.indentLevel++;
					ovr.scale = EditorGUILayout.FloatField("Scale", ovr.scale);
					EditorGUI.indentLevel--;
				}
				if (ovr.autoScaleMode == tk2dCameraResolutionOverride.AutoScaleMode.StretchToFit)
				{
					string msg = "The native resolution image will be stretched to fit the target display. " +
					"Image quality will suffer if non-uniform scaling occurs.";
					tk2dGuiUtility.InfoBox(msg, tk2dGuiUtility.WarningLevel.Info);
				}
				else
				{
					ovr.fitMode = (tk2dCameraResolutionOverride.FitMode)EditorGUILayout.EnumPopup("Fit Mode", ovr.fitMode);
					if (ovr.fitMode == tk2dCameraResolutionOverride.FitMode.Constant)
					{
						EditorGUI.indentLevel++;
						ovr.offsetPixels.x = EditorGUILayout.FloatField("X", ovr.offsetPixels.x);
						ovr.offsetPixels.y = EditorGUILayout.FloatField("Y", ovr.offsetPixels.y);
						EditorGUI.indentLevel--;
					}
				}
				GUILayout.Space(4);

				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (ovr == usedOverride) {
					GUI.color = Color.green;
					GUIContent content = new GUIContent("ACTIVE", "The active override is the one that matches the current resolution, and is being used in the tk2dCamera game window.");
					GUILayout.Label(content, EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(false));
					GUI.color = Color.white;
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
			}

			if (deferredAction != null)
			{
				deferredAction(0);
				GUI.changed = true;
				Repaint();
			}
			
			EditorGUILayout.BeginVertical(frameBorderStyle);
			GUILayout.Space(32);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Add override", GUILayout.ExpandWidth(false)))
			{
				tk2dCameraResolutionOverride ovr = new tk2dCameraResolutionOverride();
				ovr.name = "New override";
				ovr.matchBy = tk2dCameraResolutionOverride.MatchByType.Wildcard;
				ovr.autoScaleMode = tk2dCameraResolutionOverride.AutoScaleMode.FitVisible;
				ovr.fitMode = tk2dCameraResolutionOverride.FitMode.Center;
				System.Array.Resize(ref _target.resolutionOverride, _target.resolutionOverride.Length + 1);
				_target.resolutionOverride[_target.resolutionOverride.Length - 1] = ovr;
				GUI.changed = true;
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(32);
			EditorGUILayout.EndVertical();

			GUI.enabled = true;
		}
	}

	// Scene GUI handler - draws custom preview window, working around Unity bug
	tk2dEditor.tk2dCameraSceneGUI sceneGUIHandler = null;

	void OnDisable()
	{
		if (sceneGUIHandler != null)
		{
			sceneGUIHandler.Destroy();
			sceneGUIHandler = null;
		}
	}

	static Vector3[] viewportBoxPoints = new Vector3[] {
		new Vector3(-1, -1, -1), new Vector3( 1, -1, -1), new Vector3( 1,  1, -1), new Vector3(-1,  1, -1), new Vector3(-1, -1,  1), new Vector3( 1, -1,  1), new Vector3( 1,  1,  1), new Vector3(-1,  1,  1),
	};
	static int[] viewportBoxIndices = new int[] {
		0, 1, 1, 2, 2, 3, 3, 0, 4, 5, 5, 6, 6, 7, 7, 4, 0, 4, 1, 5, 3, 7, 2, 6,
	};
	static Vector3[] transformedViewportBoxPoints = new Vector3[8];

	static void DrawCameraBounds( Matrix4x4 worldToCamera, Matrix4x4 projectionMatrix ) {
		Matrix4x4 m = worldToCamera.inverse * projectionMatrix.inverse;
		for (int i = 0; i < viewportBoxPoints.Length; ++i) {
			transformedViewportBoxPoints[i] =  m.MultiplyPoint( viewportBoxPoints[i] );
		}
		for (int i = 0; i < viewportBoxIndices.Length; i += 2) {
			Handles.DrawLine(transformedViewportBoxPoints[viewportBoxIndices[i]], transformedViewportBoxPoints[viewportBoxIndices[i + 1]]);
		}
	}

	void OnSceneGUI()
	{
		tk2dCamera target = this.target as tk2dCamera;
		Handles.color = new Color32(255,255,255,255);
		DrawCameraBounds( target.camera.worldToCameraMatrix, target.Editor__GetFinalProjectionMatrix() );
		Handles.color = new Color32(55,203,105,102);
		DrawCameraBounds( target.camera.worldToCameraMatrix, target.Editor__GetNativeProjectionMatrix() );


		Handles.color = Color.white;

		// Draw scene gui
		if (sceneGUIHandler == null)
			sceneGUIHandler = new tk2dEditor.tk2dCameraSceneGUI();
		sceneGUIHandler.OnSceneGUI(target);
	}

	[MenuItem("CONTEXT/tk2dCamera/Toggle unity camera")]
	static void ToggleUnityCamera() {
		if (Selection.gameObjects.Length == 1) {
			Camera c = Selection.activeGameObject.GetComponent<Camera>();
			if ((c.hideFlags & HideFlags.HideInInspector) != 0) {
				c.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
			}
			else {
				c.hideFlags |= HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			}
		}
	}


	// Create tk2dCamera menu item
    [MenuItem("GameObject/Create Other/tk2d/Camera", false, 14905)]
    static void DoCreateCameraObject()
	{
		bool setAsMain = (Camera.main == null);

		Camera[] allCameras = Object.FindObjectsOfType(typeof(Camera)) as Camera[];
		foreach (Camera cam in allCameras) {
			if (cam.cullingMask == -1) {
				Debug.LogError(string.Format("Camera: {0} has Culling Mask set to Everything. This will cause the scene to be drawn multiple times. Did you mean to do this?", cam.name ));
			}
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("tk2dCamera");
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		go.active = false;
#else
		go.SetActive(false);
#endif		
		go.transform.position = new Vector3(0, 0, -10.0f);
		Camera camera = go.AddComponent<Camera>();
		camera.orthographic = true;
		camera.orthographicSize = 480.0f; // arbitrary large number
		camera.farClipPlane = 1000.0f;
		camera.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		tk2dCamera newCamera = go.AddComponent<tk2dCamera>();
		newCamera.version = 1;
		go.AddComponent("FlareLayer");
		go.AddComponent("GUILayer");
		if (Object.FindObjectsOfType(typeof(AudioListener)).Length == 0) {
			go.AddComponent<AudioListener>();
		}

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		go.active = true;
#else
		go.SetActive(true);
#endif		


		// Set as main camera if Camera.main is not set
		if (setAsMain)
			go.tag = "MainCamera";

		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create tk2dCamera");
	}
}


// tk2dCameraSceneGUI - Enacapsulates the scene GUI implementation
// This is a workaround while Unity fixes the bug in the tk2dCamera code
// This is also the reason its in the same file - it will simply be defined
// when unity fix the bug and not leave an extra file in the file system
namespace tk2dEditor
{
	public class tk2dCameraSceneGUI
	{
		public void Destroy()
		{
			if (previewCamera != null)
			{
				Object.DestroyImmediate(previewCamera.gameObject);
				previewCamera = null;
			}
		}

		void PreviewWindowFunc(int windowId) 
		{
			GUILayout.BeginVertical();
			Rect rs = GUILayoutUtility.GetRect(1.0f, 1.0f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			switch (Event.current.type)
			{
				case EventType.Repaint:
				{
					int heightTweak = 19;
					Rect r = new Rect(previewWindowRect.x + rs.x, Camera.current.pixelHeight - (previewWindowRect.y + rs.y), rs.width, rs.height);
					Vector2 v = new Vector2(previewWindowRect.x + rs.x, (Camera.current.pixelHeight - previewWindowRect.y - rs.height - heightTweak) + rs.y);
					previewCamera.CopyFrom(target.camera);
					previewCamera.projectionMatrix = target.Editor__GetFinalProjectionMatrix(); // Work around a Unity bug
					previewCamera.pixelRect = new Rect(v.x, v.y, r.width, r.height);
					previewCamera.Render();
					break;
				}
			}

			GUILayout.EndVertical();
		}

		public void OnSceneGUI(tk2dCamera target)
		{
			this.target = target;

			if (previewCamera == null)
			{
				GameObject go = EditorUtility.CreateGameObjectWithHideFlags("@tk2dCamera_ScenePreview", UnityEngine.HideFlags.HideAndDontSave, new System.Type[] { typeof(Camera) } );
				previewCamera = go.camera;
				previewCamera.enabled = false;
			}

			Vector2 resolution = target.TargetResolution;

			float maxHeight = Screen.height / 5;
			float fWidth, fHeight;
			fHeight = maxHeight;
			fWidth = resolution.x / resolution.y * maxHeight;
			
			int windowDecorationWidth = 11;
			int windowDecorationHeight = 24;
			int width = (int)fWidth + windowDecorationWidth;
			int height = (int)fHeight + windowDecorationHeight;

			string windowCaption = "tk2dCamera";
			if (width > 200)
				windowCaption += string.Format(" ({0:0} x {1:0})", resolution.x, resolution.y);

			int viewportOffsetLeft = 10;
			int viewportOffsetBottom = -8;
			previewWindowRect = new Rect(viewportOffsetLeft, Camera.current.pixelHeight - height - viewportOffsetBottom, width, height);
			Handles.BeginGUI();
			GUI.Window("tk2dCamera Preview".GetHashCode(), previewWindowRect, PreviewWindowFunc, windowCaption);
			Handles.EndGUI();
		}

		tk2dCamera target;

		Camera previewCamera = null;
		Rect previewWindowRect;
	}	
}
