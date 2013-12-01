using UnityEngine;
using System.Collections;

[System.Serializable]
public class tk2dTextMeshData
{
	public int version = 0;

	public tk2dFontData font;
	public string text = ""; 
	public Color color = Color.white; 
	public Color color2 = Color.white; 
	public bool useGradient = false; 
	public int textureGradient = 0;
	public TextAnchor anchor = TextAnchor.LowerLeft; 
	public int renderLayer = 0;
	public Vector3 scale = Vector3.one; 
	public bool kerning = false; 
	public int maxChars = 16; 
	public bool inlineStyling = false;

	public bool formatting = false; 
	public int wordWrapWidth = 0; 

	public float spacing = 0.0f;
	public float lineSpacing = 0.0f;
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("2D Toolkit/Text/tk2dTextMesh")]
/// <summary>
/// Text mesh
/// </summary>
public class tk2dTextMesh : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	tk2dFontData _fontInst;
	string _formattedText = "";

	// This stuff now kept in tk2dTextMeshData. Remove in future version.
	[SerializeField] tk2dFontData _font = null;
	[SerializeField] string _text = ""; 
	[SerializeField] Color _color = Color.white; 
	[SerializeField] Color _color2 = Color.white; 
	[SerializeField] bool _useGradient = false; 
	[SerializeField] int _textureGradient = 0;
	[SerializeField] TextAnchor _anchor = TextAnchor.LowerLeft; 
	[SerializeField] Vector3 _scale = new Vector3(1.0f, 1.0f, 1.0f); 
	[SerializeField] bool _kerning = false; 
	[SerializeField] int _maxChars = 16; 
	[SerializeField] bool _inlineStyling = false;
	
	[SerializeField] bool _formatting = false; 
	[SerializeField] int _wordWrapWidth = 0; 

	[SerializeField] float spacing = 0.0f;
	[SerializeField] float lineSpacing = 0.0f;

	// Holding the data in this struct for the next version
	[SerializeField] tk2dTextMeshData data = new tk2dTextMeshData();

	// Batcher needs to grab this
	public string FormattedText {
		get {return _formattedText;}
	}

	void UpgradeData()
	{
		if (data.version != 1)
		{
			data.font = _font;
			data.text = _text;
			data.color = _color;
			data.color2 = _color2;
			data.useGradient = _useGradient;
			data.textureGradient = _textureGradient;
			data.anchor = _anchor;
			data.scale = _scale;
			data.kerning = _kerning;
			data.maxChars = _maxChars;
			data.inlineStyling = _inlineStyling;
			data.formatting = _formatting;
			data.wordWrapWidth = _wordWrapWidth;
			data.spacing = spacing;
			data.lineSpacing = lineSpacing;
		}
		data.version = 1;
	}
	
	Vector3[] vertices;
	Vector2[] uvs;
	Vector2[] uv2;
	Color32[] colors;
	Color32[] untintedColors;

	static int GetInlineStyleCommandLength(int cmdSymbol) {
		int val = 0;
		switch (cmdSymbol) {
			case 'c': val = 5; break; // cRGBA
			case 'C': val = 9; break; // CRRGGBBAA
			case 'g': val = 9; break; // gRGBARGBA
			case 'G': val = 17; break; // GRRGGBBAARRGGBBAA
		}
		return val;
	}
	
	/// <summary>
	/// Formats the string using the current settings, and returns the formatted string.
	/// You can use this if you need to calculate how many lines your string is going to be wrapped to.
	/// </summary>
	public string FormatText(string unformattedString) {
		string returnValue = "";
		FormatText(ref returnValue, unformattedString);
		return returnValue;
	}

	void FormatText() {
		FormatText(ref _formattedText, data.text);
	}

	void FormatText(ref string _targetString, string _source)
	{
		if (formatting == false || wordWrapWidth == 0 || _fontInst.texelSize == Vector2.zero)
		{
			_targetString = _source;
			return;
		}

		float lineWidth = _fontInst.texelSize.x * wordWrapWidth;

		System.Text.StringBuilder target = new System.Text.StringBuilder(_source.Length);
		float widthSoFar = 0.0f;
		float wordStart = 0.0f;
		int targetWordStartIndex = -1;
		int fmtWordStartIndex = -1;
		bool ignoreNextCharacter = false;
		for (int i = 0; i < _source.Length; ++i)
		{
			char idx = _source[i];
			tk2dFontChar chr;

			bool inlineHatChar = (idx == '^');
			
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = (char)0;
				chr = _fontInst.charDict[idx];
			}
			else
			{
				if (idx >= _fontInst.chars.Length) idx = (char)0; // should be space
				chr = _fontInst.chars[idx];
			}

			if (inlineHatChar) idx = '^';

			if (ignoreNextCharacter) {
				ignoreNextCharacter = false;
				continue;
			}

			if (data.inlineStyling && idx == '^' && i + 1 < _source.Length) {
				if (_source[i + 1] == '^') {
					ignoreNextCharacter = true;
					target.Append('^'); // add the second hat that we'll skip
				} else {
					int cmdLength = GetInlineStyleCommandLength(_source[i + 1]);
					int skipLength = 1 + cmdLength; // The ^ plus the command
					for (int j = 0; j < skipLength; ++j) {
						if (i + j < _source.Length) {
							target.Append(_source[i + j]);
						}
					}
					i += skipLength - 1;
					continue;
				}
			}

			if (idx == '\n') 
			{
				widthSoFar = 0.0f;
				wordStart = 0.0f;
				targetWordStartIndex = target.Length;
				fmtWordStartIndex = i;
			}
			else if (idx == ' '/* || idx == '.' || idx == ',' || idx == ':' || idx == ';' || idx == '!'*/)
			{
				/*if ((widthSoFar + chr.p1.x * data.scale.x) > lineWidth)
				{
					target.Append('\n');
					widthSoFar = chr.advance * data.scale.x;
				}
				else
				{*/
					widthSoFar += (chr.advance + data.spacing) * data.scale.x;
				//}

				wordStart = widthSoFar;
				targetWordStartIndex = target.Length;
				fmtWordStartIndex = i;
			}
			else
			{
				if ((widthSoFar + chr.p1.x * data.scale.x) > lineWidth)
				{
					// If the last word started after the start of the line
					if (wordStart > 0.0f)
					{
						wordStart = 0.0f;
						widthSoFar = 0.0f;
						// rewind
						target.Remove(targetWordStartIndex + 1, target.Length - targetWordStartIndex - 1);
						target.Append('\n');
						i = fmtWordStartIndex;
						continue; // don't add this character
					}
					else
					{
						target.Append('\n');
						widthSoFar = (chr.advance + data.spacing) * data.scale.x;
					}
				}
				else
				{
					widthSoFar += (chr.advance + data.spacing) * data.scale.x;
				}
			}
			
			target.Append(idx);
		}
		_targetString = target.ToString();
	}

	[System.FlagsAttribute]
	enum UpdateFlags
	{
		UpdateNone		= 0,
		UpdateText		= 1,	// update text vertices & uvs
		UpdateColors	= 2,	// only colors have changed
		UpdateBuffers	= 4,	// update buffers (maxchars has changed)
	};
	UpdateFlags updateFlags = UpdateFlags.UpdateBuffers;

	Mesh mesh;
	MeshFilter meshFilter;

	void SetNeedUpdate(UpdateFlags uf) {
		if (updateFlags == UpdateFlags.UpdateNone) {
			updateFlags |= uf;
			tk2dUpdateManager.QueueCommit(this);
		}
		else {
			// Already queued
			updateFlags |= uf;
		}
	}

	// accessors
	/// <summary>Gets or sets the font. Call <see cref="Commit"/> to commit changes.</summary>
	public tk2dFontData font 
	{ 
		get { UpgradeData(); return data.font; } 
		set 
		{ 
			UpgradeData();
			data.font = value; 
			_fontInst = data.font.inst;
			SetNeedUpdate( UpdateFlags.UpdateText );

			UpdateMaterial();
		} 
	}

	/// <summary>Enables or disables formatting. Call <see cref="Commit"/> to commit changes.</summary>
	public bool formatting
	{
		get { UpgradeData(); return data.formatting; }
		set
		{
			UpgradeData();
			if (data.formatting != value)
			{
				data.formatting = value;
				SetNeedUpdate( UpdateFlags.UpdateText );
			}
		}
	}

	/// <summary>Change word wrap width. This only works when formatting is enabled. 
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public int wordWrapWidth
	{
		get { UpgradeData(); return data.wordWrapWidth; }
		set { UpgradeData(); if (data.wordWrapWidth != value) { data.wordWrapWidth = value; SetNeedUpdate(UpdateFlags.UpdateText); } }
	}

	/// <summary>Gets or sets the text. Call <see cref="Commit"/> to commit changes.</summary>
	public string text 
	{ 
		get { UpgradeData(); return data.text; } 
		set 
		{
			UpgradeData();
			data.text = value;
			SetNeedUpdate(UpdateFlags.UpdateText);
		}
	}

	/// <summary>Gets or sets the color. Call <see cref="Commit"/> to commit changes.</summary>
	public Color color { get { UpgradeData(); return data.color; } set { UpgradeData(); data.color = value; SetNeedUpdate(UpdateFlags.UpdateColors); } }
	/// <summary>Gets or sets the secondary color (used in the gradient). Call <see cref="Commit"/> to commit changes.</summary>
	public Color color2 { get { UpgradeData(); return data.color2; } set { UpgradeData(); data.color2 = value; SetNeedUpdate(UpdateFlags.UpdateColors); } }
	/// <summary>Use vertex vertical gradient. Call <see cref="Commit"/> to commit changes.</summary>
	public bool useGradient { get { UpgradeData(); return data.useGradient; } set { UpgradeData(); data.useGradient = value; SetNeedUpdate(UpdateFlags.UpdateColors); } }
	/// <summary>Gets or sets the text anchor. Call <see cref="Commit"/> to commit changes.</summary>
	public TextAnchor anchor { get { UpgradeData(); return data.anchor; } set { UpgradeData(); data.anchor = value; SetNeedUpdate(UpdateFlags.UpdateText); } }
	/// <summary>Gets or sets the scale. Call <see cref="Commit"/> to commit changes.</summary>
	public Vector3 scale { get { UpgradeData(); return data.scale; } set { UpgradeData(); data.scale = value; SetNeedUpdate(UpdateFlags.UpdateText); } }
	/// <summary>Gets or sets kerning state. Call <see cref="Commit"/> to commit changes.</summary>
	public bool kerning { get { UpgradeData(); return data.kerning; } set { UpgradeData(); data.kerning = value; SetNeedUpdate(UpdateFlags.UpdateText); } }
	/// <summary>Gets or sets maxChars. Call <see cref="Commit"/> to commit changes.
	/// NOTE: This will free & allocate memory, avoid using at runtime.
	/// </summary>
	public int maxChars { get { UpgradeData(); return data.maxChars; } set { UpgradeData(); data.maxChars = value; SetNeedUpdate(UpdateFlags.UpdateBuffers); } }
	/// <summary>Gets or sets the default texture gradient. 
	/// You can also change texture gradient inline by using ^1 - ^9 sequences within your text.
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public int textureGradient { get { UpgradeData(); return data.textureGradient; } set { UpgradeData(); data.textureGradient = value % font.gradientCount; SetNeedUpdate(UpdateFlags.UpdateText); } }
	/// <summary>Enables or disables inline styling (texture gradient). Call <see cref="Commit"/> to commit changes.</summary>
	public bool inlineStyling { get { UpgradeData(); return data.inlineStyling; } set { UpgradeData(); data.inlineStyling = value; SetNeedUpdate(UpdateFlags.UpdateText); } }
	/// <summary>Additional spacing between characters. 
	/// This can be negative to bring characters closer together.
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public float Spacing { get { UpgradeData(); return data.spacing; } set { UpgradeData(); if (data.spacing != value) { data.spacing = value; SetNeedUpdate(UpdateFlags.UpdateText); } } }
	/// <summary>Additional line spacing for multieline text. 
	/// This can be negative to bring lines closer together.
	/// Call <see cref="Commit"/> to commit changes.</summary>
	public float LineSpacing { get { UpgradeData(); return data.lineSpacing; } set { UpgradeData(); if (data.lineSpacing != value) { data.lineSpacing = value; SetNeedUpdate(UpdateFlags.UpdateText); } } }

	/// <summary>
	/// Gets or sets the sorting order
	/// The sorting order lets you override draw order for sprites which are at the same z position
	/// It is similar to offsetting in z - the sprite stays at the original position
	/// This corresponds to the renderer.sortingOrder property in Unity 4.3
	/// </summary>
	public int SortingOrder { get { return data.renderLayer; } set { if (data.renderLayer != value) { data.renderLayer = value; SetNeedUpdate(UpdateFlags.UpdateText); } } }

	void InitInstance()
	{
		if (_fontInst == null && data.font != null)
			_fontInst = data.font.inst;
	}

	// Use this for initialization
	void Awake() 
	{
		UpgradeData();
		if (data.font != null)
			_fontInst = data.font.inst;

		// force rebuild when awakened, for when the object has been pooled, etc
		// this is probably not the best way to do it
		updateFlags = UpdateFlags.UpdateBuffers;
		
		if (data.font != null)
		{
			Init();
			UpdateMaterial();
		}

		// Sensibly reset, so tk2dUpdateManager can deal with this properly
		updateFlags = UpdateFlags.UpdateNone;
	}

	protected void OnDestroy()
	{
		if (meshFilter == null)
		{
			meshFilter = GetComponent<MeshFilter>();
		}
		if (meshFilter != null)
		{
			mesh = meshFilter.sharedMesh;
		}
		
		if (mesh)
		{
			DestroyImmediate(mesh, true);
			meshFilter.mesh = null;
		}
	}
	
	bool useInlineStyling { get { return inlineStyling && _fontInst.textureGradients; } }

	/// <summary>
	/// Returns the number of characters drawn for the currently active string.
	/// This may be less than string.Length - some characters are used as escape codes for switching texture gradient ^0-^9
	/// Also, there might be more characters in the string than have been allocated for the textmesh, in which case
	/// the string will be truncated.
	/// </summary>
	public int NumDrawnCharacters()
	{
		int charsDrawn = NumTotalCharacters();
		if (charsDrawn > data.maxChars) charsDrawn = data.maxChars;
		return charsDrawn;
	}
	
	/// <summary>
	/// Returns the number of characters excluding texture gradient escape codes.
	/// </summary>
	public int NumTotalCharacters()
	{
		InitInstance();

		if ((updateFlags & (UpdateFlags.UpdateText | UpdateFlags.UpdateBuffers)) != 0)
			FormatText();

		int numChars = 0;
		for (int i = 0; i < _formattedText.Length; ++i)
		{
			int idx = _formattedText[i];

			bool inlineHatChar = (idx == '^');

			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = 0;
			}
			else
			{
				if (idx >= _fontInst.chars.Length) idx = 0; // should be space
			}

			if (inlineHatChar) idx = '^';

			if (idx == '\n')
			{
				continue;
			}
			else if (data.inlineStyling)
			{
				if (idx == '^' && i + 1 < _formattedText.Length)
				{
					if (_formattedText[i + 1] == '^') {
						++i;
					} else {
						i += GetInlineStyleCommandLength(_formattedText[i + 1]);
						continue;
					}
				}
			}
			
			++numChars;
		}
		return numChars;
	}

	[System.Obsolete]
	public Vector2 GetMeshDimensionsForString(string str) {
		return tk2dTextGeomGen.GetMeshDimensionsForString(str, tk2dTextGeomGen.Data( data, _fontInst, _formattedText ));
	}

	/// <summary>
	/// Calculates an estimated bounds for the given string if it were rendered
	/// using the current settings.
	/// This expects an unformatted string and will wrap the string if required.
	/// </summary>
	public Bounds GetEstimatedMeshBoundsForString( string str ) {
		tk2dTextGeomGen.GeomData geomData = tk2dTextGeomGen.Data( data, _fontInst, _formattedText );
		Vector2 dims = tk2dTextGeomGen.GetMeshDimensionsForString( FormatText( str ), geomData);
		float offsetY = tk2dTextGeomGen.GetYAnchorForHeight(dims.y, geomData);
		float offsetX = tk2dTextGeomGen.GetXAnchorForWidth(dims.x, geomData);
		float lineHeight = (_fontInst.lineHeight + data.lineSpacing) * data.scale.y;
		return new Bounds( new Vector3(offsetX + dims.x * 0.5f, offsetY + dims.y * 0.5f + lineHeight, 0), Vector3.Scale(dims, new Vector3(1, -1, 1)) );
	}
	
	public void Init(bool force)
	{
		if (force)
		{
			SetNeedUpdate(UpdateFlags.UpdateBuffers);
		}
		Init();
	}
	
	public void Init()
	{
		if (_fontInst && ((updateFlags & UpdateFlags.UpdateBuffers) != 0 || mesh == null))
		{
			_fontInst.InitDictionary();
			FormatText();

			var geomData = tk2dTextGeomGen.Data( data, _fontInst, _formattedText );

			// volatile data
			int numVertices;
			int numIndices;
			tk2dTextGeomGen.GetTextMeshGeomDesc(out numVertices, out numIndices, geomData);
			vertices = new Vector3[numVertices];
			uvs = new Vector2[numVertices];
			colors = new Color32[numVertices];
			untintedColors = new Color32[numVertices];
			if (_fontInst.textureGradients)
			{
				uv2 = new Vector2[numVertices];
			}
			int[] triangles = new int[numIndices];


			int target = tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData);

			if (!_fontInst.isPacked) {
				Color32 topColor = data.color;
				Color32 bottomColor = data.useGradient ? data.color2 : data.color;
				for (int i = 0; i < numVertices; ++i) {
					Color32 c = ((i % 4) < 2) ? topColor : bottomColor;
					byte red = (byte)(((int)untintedColors[i].r * (int)c.r) / 255);
					byte green = (byte)(((int)untintedColors[i].g * (int)c.g) / 255);
					byte blue = (byte)(((int)untintedColors[i].b * (int)c.b) / 255);
					byte alpha = (byte)(((int)untintedColors[i].a * (int)c.a) / 255);
					if (_fontInst.premultipliedAlpha) {
						red = (byte)(((int)red * (int)alpha) / 255);
						green = (byte)(((int)green * (int)alpha) / 255);
						blue = (byte)(((int)blue * (int)alpha) / 255);
					}
					colors[i] = new Color32(red, green, blue, alpha);
				}
			}
			else {
				colors = untintedColors;
			}

			tk2dTextGeomGen.SetTextMeshIndices(triangles, 0, 0, geomData, target);
			


			if (mesh == null)
			{
				if (meshFilter == null)
					meshFilter = GetComponent<MeshFilter>();
				
				mesh = new Mesh();
				mesh.hideFlags = HideFlags.DontSave;
				meshFilter.mesh = mesh;
			}
			else
			{
				mesh.Clear();
			}
			mesh.vertices = vertices;
			mesh.uv = uvs;
			if (font.textureGradients)
			{
				mesh.uv2 = uv2;
			}
			mesh.triangles = triangles;
			mesh.colors32 = colors;
			mesh.RecalculateBounds();
			mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds( mesh.bounds, data.renderLayer );

			updateFlags = UpdateFlags.UpdateNone;
		}
	}
	
	/// <summary>
	/// Calling commit is no longer required on text meshes.
	/// You can still call commit to manually commit all changes so far in the frame.
	/// </summary>
	public void Commit() {
		tk2dUpdateManager.FlushQueues();
	}

	// Do not call this, its meant fo internal use
	public void DoNotUse__CommitInternal()
	{
		// Make sure instance is set up, might not be when calling from Awake.
		InitInstance();

		// make sure fonts dictionary is initialized properly before proceeding
		if (_fontInst == null) {
			return;
		}
		_fontInst.InitDictionary();
		
		// Can come in here without anything initalized when
		// instantiated in code
		if ((updateFlags & UpdateFlags.UpdateBuffers) != 0 || mesh == null)
		{
			Init();
		}
		else 
		{
			if ((updateFlags & UpdateFlags.UpdateText) != 0)
			{
				FormatText();

				var geomData = tk2dTextGeomGen.Data( data, _fontInst, _formattedText );
				int target = tk2dTextGeomGen.SetTextMeshGeom(vertices, uvs, uv2, untintedColors, 0, geomData);

				for (int i = target; i < data.maxChars; ++i)
				{
					// was/is unnecessary to fill anything else
					vertices[i * 4 + 0] = vertices[i * 4 + 1] = vertices[i * 4 + 2] = vertices[i * 4 + 3] = Vector3.zero;
				}
	
				mesh.vertices = vertices;
				mesh.uv = uvs;
				if (_fontInst.textureGradients)
				{
					mesh.uv2 = uv2;
				}
				if (_fontInst.isPacked) {
					colors = untintedColors;
					mesh.colors32 = colors;
				}
				if (data.inlineStyling) {
					SetNeedUpdate(UpdateFlags.UpdateColors);
				}

				mesh.RecalculateBounds();
				mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds( mesh.bounds, data.renderLayer );
			}
	
			if (!font.isPacked && (updateFlags & UpdateFlags.UpdateColors) != 0) // packed fonts don't support tinting
			{
				Color32 topColor = data.color;
				Color32 bottomColor = data.useGradient ? data.color2 : data.color;
				for (int i = 0; i < colors.Length; ++i) {
					Color32 c = ((i % 4) < 2) ? topColor : bottomColor;
					byte red = (byte)(((int)untintedColors[i].r * (int)c.r) / 255);
					byte green = (byte)(((int)untintedColors[i].g * (int)c.g) / 255);
					byte blue = (byte)(((int)untintedColors[i].b * (int)c.b) / 255);
					byte alpha = (byte)(((int)untintedColors[i].a * (int)c.a) / 255);
					if (_fontInst.premultipliedAlpha) {
						red = (byte)(((int)red * (int)alpha) / 255);
						green = (byte)(((int)green * (int)alpha) / 255);
						blue = (byte)(((int)blue * (int)alpha) / 255);
					}
					colors[i] = new Color32(red, green, blue, alpha);
				}

				mesh.colors32 = colors;
			}
		}
		
		updateFlags = UpdateFlags.UpdateNone;
	}

	/// <summary>
	/// Makes the text mesh pixel perfect to the active camera.
	/// Automatically detects <see cref="tk2dCamera"/> if present
	/// Otherwise uses Camera.main
	/// </summary>
	public void MakePixelPerfect()
	{
		float s = 1.0f;
		tk2dCamera cam = tk2dCamera.CameraForLayer(gameObject.layer);
		if (cam != null)
		{
			if (_fontInst.version < 1)
			{
				Debug.LogError("Need to rebuild font.");
			}

			float zdist = (transform.position.z - cam.transform.position.z);
			float textMeshSize = (_fontInst.invOrthoSize * _fontInst.halfTargetHeight);
			s = cam.GetSizeAtDistance(zdist) * textMeshSize;
		}
		else if (Camera.main)
		{
			if (Camera.main.isOrthoGraphic)
			{
				s = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = (transform.position.z - Camera.main.transform.position.z);
				s = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
			s *= _fontInst.invOrthoSize;
		}
		scale = new Vector3(Mathf.Sign(scale.x) * s, Mathf.Sign(scale.y) * s, Mathf.Sign(scale.z) * s);
	}	
	
	// tk2dRuntime.ISpriteCollectionEditor
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		if (data.font != null && data.font.spriteCollection != null)
			return data.font.spriteCollection == spriteCollection;
		
		// No easy way to identify this at this stage
		return true;
	}
	
	void UpdateMaterial()
	{
		if (renderer.sharedMaterial != _fontInst.materialInst)
			renderer.material = _fontInst.materialInst;
	}
	
	public void ForceBuild()
	{
		if (data.font != null)
		{
			_fontInst = data.font.inst;
			UpdateMaterial();
		}
		Init(true);
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (mesh != null) {
			Bounds b = mesh.bounds;
			Gizmos.color = Color.clear;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(b.center, b.extents * 2);
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}
	}
#endif
}
