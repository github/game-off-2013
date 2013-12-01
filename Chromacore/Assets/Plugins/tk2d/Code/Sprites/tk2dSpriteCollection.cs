using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class tk2dSpriteColliderIsland
{
	public bool connected = true;
	public Vector2[] points;
	
	public bool IsValid()
	{
		if (connected)
		{
			return points.Length >= 3;
		}
		else
		{
			return points.Length >= 2;
		}
	}
	
	public void CopyFrom(tk2dSpriteColliderIsland src)
	{
		connected = src.connected;
		
		points = new Vector2[src.points.Length];
		for (int i = 0; i < points.Length; ++i)
			points[i] = src.points[i];		
	}
	
	public bool CompareTo(tk2dSpriteColliderIsland src)
	{
		if (connected != src.connected) return false;
		if (points.Length != src.points.Length) return false;
		for (int i = 0; i < points.Length; ++i)
			if (points[i] != src.points[i]) return false;
		return true;
	}
}

[System.Serializable]
public class tk2dSpriteCollectionDefinition
{
    public enum Anchor
    {
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight,
		Custom
    }
	
	public enum Pad
	{
		Default,
		BlackZeroAlpha,
		Extend,
		TileXY,
	}
	
	public enum ColliderType
	{
		UserDefined,		// don't try to create or destroy anything
		ForceNone,			// nothing will be created, if something exists, it will be destroyed
		BoxTrimmed, 		// box, trimmed to cover visible region
		BoxCustom, 			// box, with custom values provided by user
		Polygon, 			// polygon, can be concave
	}
	
	public enum PolygonColliderCap
	{
		None,
		FrontAndBack,
		Front,
		Back,
	}
	
	public enum ColliderColor
	{
		Default, // default unity color scheme
		Red,
		White,
		Black
	}
	
	public enum Source
	{
		Sprite,
		SpriteSheet,
		Font
	}

	public enum DiceFilter
	{
		Complete,
		SolidOnly,
		TransparentOnly,
	}
	
	public string name = "";
	
	public bool disableTrimming = false;
    public bool additive = false;
    public Vector3 scale = new Vector3(1,1,1);
    
    public Texture2D texture = null;
	
	[System.NonSerialized]
	public Texture2D thumbnailTexture;
	
	public int materialId = 0;
	
	public Anchor anchor = Anchor.MiddleCenter;
	public float anchorX, anchorY;
    public Object overrideMesh;

    public bool doubleSidedSprite = false;
	public bool customSpriteGeometry = false;
	public tk2dSpriteColliderIsland[] geometryIslands = new tk2dSpriteColliderIsland[0];
	
	public bool dice = false;
	public int diceUnitX = 64;
	public int diceUnitY = 64;
	public DiceFilter diceFilter = DiceFilter.Complete;

	public Pad pad = Pad.Default;
	public int extraPadding = 0; // default
	
	public Source source = Source.Sprite;
	public bool fromSpriteSheet = false;
	public bool hasSpriteSheetId = false;
	public int spriteSheetId = 0;
	public int spriteSheetX = 0, spriteSheetY = 0;
	public bool extractRegion = false;
	public int regionX, regionY, regionW, regionH;
	public int regionId;
	
	public ColliderType colliderType = ColliderType.UserDefined;
	public Vector2 boxColliderMin, boxColliderMax;
	public tk2dSpriteColliderIsland[] polyColliderIslands;
	public PolygonColliderCap polyColliderCap = PolygonColliderCap.FrontAndBack;
	public bool colliderConvex = false;
	public bool colliderSmoothSphereCollisions = false;
	public ColliderColor colliderColor = ColliderColor.Default;

	public List<tk2dSpriteDefinition.AttachPoint> attachPoints = new List<tk2dSpriteDefinition.AttachPoint>();

	public void CopyFrom(tk2dSpriteCollectionDefinition src)
	{
		name = src.name;
		
		disableTrimming = src.disableTrimming;
		additive = src.additive;
		scale = src.scale;
		texture = src.texture;
		materialId = src.materialId;
		anchor = src.anchor;
		anchorX = src.anchorX;
		anchorY = src.anchorY;
		overrideMesh = src.overrideMesh;
		
		doubleSidedSprite = src.doubleSidedSprite;
		customSpriteGeometry = src.customSpriteGeometry;
		geometryIslands = src.geometryIslands;
		
		dice = src.dice;
		diceUnitX = src.diceUnitX;
		diceUnitY = src.diceUnitY;
		diceFilter = src.diceFilter;
		pad = src.pad;
		
		source = src.source;
		fromSpriteSheet = src.fromSpriteSheet;
		hasSpriteSheetId = src.hasSpriteSheetId;
		spriteSheetX = src.spriteSheetX;
		spriteSheetY = src.spriteSheetY;
		spriteSheetId = src.spriteSheetId;
		extractRegion = src.extractRegion;
		regionX = src.regionX;
		regionY = src.regionY;
		regionW = src.regionW;
		regionH = src.regionH;
		regionId = src.regionId;
		
		colliderType = src.colliderType;
		boxColliderMin = src.boxColliderMin;
		boxColliderMax = src.boxColliderMax;
		polyColliderCap = src.polyColliderCap;
		
		colliderColor = src.colliderColor;
		colliderConvex = src.colliderConvex;
		colliderSmoothSphereCollisions = src.colliderSmoothSphereCollisions;
		
		extraPadding = src.extraPadding;
		
		if (src.polyColliderIslands != null)
		{
			polyColliderIslands = new tk2dSpriteColliderIsland[src.polyColliderIslands.Length];
			for (int i = 0; i < polyColliderIslands.Length; ++i)
			{
				polyColliderIslands[i] = new tk2dSpriteColliderIsland();
				polyColliderIslands[i].CopyFrom(src.polyColliderIslands[i]);
			}
		}
		else
		{
			polyColliderIslands = new tk2dSpriteColliderIsland[0];
		}
		
		if (src.geometryIslands != null)
		{
			geometryIslands = new tk2dSpriteColliderIsland[src.geometryIslands.Length];
			for (int i = 0; i < geometryIslands.Length; ++i)
			{
				geometryIslands[i] = new tk2dSpriteColliderIsland();
				geometryIslands[i].CopyFrom(src.geometryIslands[i]);
			}
		}
		else
		{
			geometryIslands = new tk2dSpriteColliderIsland[0];
		}

		attachPoints = new List<tk2dSpriteDefinition.AttachPoint>(src.attachPoints.Count);
		foreach (tk2dSpriteDefinition.AttachPoint srcAp in src.attachPoints) {
			tk2dSpriteDefinition.AttachPoint ap = new tk2dSpriteDefinition.AttachPoint();
			ap.CopyFrom(srcAp);
			attachPoints.Add(ap);
		}
	}
	
	public void Clear()
	{
		// Reinitialize
		var tmpVar = new tk2dSpriteCollectionDefinition();
		CopyFrom(tmpVar);
	}
	
	public bool CompareTo(tk2dSpriteCollectionDefinition src)
	{
		if (name != src.name) return false;
		
		if (additive != src.additive) return false;
		if (scale != src.scale) return false;
		if (texture != src.texture) return false;
		if (materialId != src.materialId) return false;
		if (anchor != src.anchor) return false;
		if (anchorX != src.anchorX) return false;
		if (anchorY != src.anchorY) return false;
		if (overrideMesh != src.overrideMesh) return false;
		if (dice != src.dice) return false;
		if (diceUnitX != src.diceUnitX) return false;
		if (diceUnitY != src.diceUnitY) return false;
		if (diceFilter != src.diceFilter) return false;
		if (pad != src.pad) return false;
		if (extraPadding != src.extraPadding) return false;

		if (doubleSidedSprite != src.doubleSidedSprite) return false;

		if (customSpriteGeometry != src.customSpriteGeometry) return false;
		if (geometryIslands != src.geometryIslands) return false;
		if (geometryIslands != null && src.geometryIslands != null)
		{
			if (geometryIslands.Length != src.geometryIslands.Length) return false;
			for (int i = 0; i < geometryIslands.Length; ++i)
				if (!geometryIslands[i].CompareTo(src.geometryIslands[i])) return false;
		}

		if (source != src.source) return false;
		if (fromSpriteSheet != src.fromSpriteSheet) return false;
		if (hasSpriteSheetId != src.hasSpriteSheetId) return false;
		if (spriteSheetId != src.spriteSheetId) return false;
		if (spriteSheetX != src.spriteSheetX) return false;
		if (spriteSheetY != src.spriteSheetY) return false;
		if (extractRegion != src.extractRegion) return false;
		if (regionX != src.regionX) return false;
		if (regionY != src.regionY) return false;
		if (regionW != src.regionW) return false;
		if (regionH != src.regionH) return false;
		if (regionId != src.regionId) return false;
		
		if (colliderType != src.colliderType) return false;
		if (boxColliderMin != src.boxColliderMin) return false;
		if (boxColliderMax != src.boxColliderMax) return false;
		
		if (polyColliderIslands != src.polyColliderIslands) return false;
		if (polyColliderIslands != null && src.polyColliderIslands != null)
		{
			if (polyColliderIslands.Length != src.polyColliderIslands.Length) return false;
			for (int i = 0; i < polyColliderIslands.Length; ++i)
				if (!polyColliderIslands[i].CompareTo(src.polyColliderIslands[i])) return false;
		}
		
		if (polyColliderCap != src.polyColliderCap) return false;
		
		if (colliderColor != src.colliderColor) return false;
		if (colliderSmoothSphereCollisions != src.colliderSmoothSphereCollisions) return false;
		if (colliderConvex != src.colliderConvex) return false;

		if (attachPoints.Count != src.attachPoints.Count) return false;
		for (int i = 0; i < attachPoints.Count; ++i) {
			if (!attachPoints[i].CompareTo(src.attachPoints[i])) return false;
		}
		
		return true;
	}	
}

[System.Serializable]
public class tk2dSpriteCollectionDefault
{
    public bool additive = false;
    public Vector3 scale = new Vector3(1,1,1);
	public tk2dSpriteCollectionDefinition.Anchor anchor = tk2dSpriteCollectionDefinition.Anchor.MiddleCenter;
	public tk2dSpriteCollectionDefinition.Pad pad = tk2dSpriteCollectionDefinition.Pad.Default;	
	
	public tk2dSpriteCollectionDefinition.ColliderType colliderType = tk2dSpriteCollectionDefinition.ColliderType.UserDefined;
}

[System.Serializable]
public class tk2dSpriteSheetSource
{
    public enum Anchor
    {
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight,
    }
	
	public enum SplitMethod
	{
		UniformDivision,
	}
	
	public Texture2D texture;
	public int tilesX, tilesY;
	public int numTiles = 0;
	public Anchor anchor = Anchor.MiddleCenter;
	public tk2dSpriteCollectionDefinition.Pad pad = tk2dSpriteCollectionDefinition.Pad.Default;
	public Vector3 scale = new Vector3(1,1,1);
	public bool additive = false;
	
	// version 1
	public bool active = false;
	public int tileWidth, tileHeight;
	public int tileMarginX, tileMarginY;
	public int tileSpacingX, tileSpacingY;
	public SplitMethod splitMethod = SplitMethod.UniformDivision;
	
	public int version = 0;
	public const int CURRENT_VERSION = 1;

	public tk2dSpriteCollectionDefinition.ColliderType colliderType = tk2dSpriteCollectionDefinition.ColliderType.UserDefined;
	
	public void CopyFrom(tk2dSpriteSheetSource src)
	{
		texture = src.texture;
		tilesX = src.tilesX;
		tilesY = src.tilesY;
		numTiles = src.numTiles;
		anchor = src.anchor;
		pad = src.pad;
		scale = src.scale;
		colliderType = src.colliderType;
		version = src.version;
		
		active = src.active;
		tileWidth = src.tileWidth;
		tileHeight = src.tileHeight;
		tileSpacingX = src.tileSpacingX;
		tileSpacingY = src.tileSpacingY;
		tileMarginX = src.tileMarginX;
		tileMarginY = src.tileMarginY;
		splitMethod = src.splitMethod;
	}
	
	public bool CompareTo(tk2dSpriteSheetSource src)
	{
		if (texture != src.texture) return false;
		if (tilesX != src.tilesX) return false;
		if (tilesY != src.tilesY) return false;
		if (numTiles != src.numTiles) return false;
		if (anchor != src.anchor) return false;
		if (pad != src.pad) return false;
		if (scale != src.scale) return false;
		if (colliderType != src.colliderType) return false;
		if (version != src.version) return false;
	
		if (active != src.active) return false;
		if (tileWidth != src.tileWidth) return false;
		if (tileHeight != src.tileHeight) return false;
		if (tileSpacingX != src.tileSpacingX) return false;
		if (tileSpacingY != src.tileSpacingY) return false;
		if (tileMarginX != src.tileMarginX) return false;
		if (tileMarginY != src.tileMarginY) return false;
		if (splitMethod != src.splitMethod) return false;
		
		return true;
	}
	
	public string Name { get { return texture != null?texture.name:"New Sprite Sheet"; } }
}

[System.Serializable]
public class tk2dSpriteCollectionFont
{
	public bool active = false;
	public Object bmFont;
	public Texture2D texture;
    public bool dupeCaps = false; // duplicate lowercase into uc, or vice-versa, depending on which exists
	public bool flipTextureY = false;
	public int charPadX = 0;
	public tk2dFontData data;
	public tk2dFont editorData;
	public int materialId;

	public bool useGradient = false;
	public Texture2D gradientTexture = null;
	public int gradientCount = 1;
	
	public void CopyFrom(tk2dSpriteCollectionFont src)
	{
		active = src.active;
		bmFont = src.bmFont;
		texture = src.texture;
		dupeCaps = src.dupeCaps;
		flipTextureY = src.flipTextureY;
		charPadX = src.charPadX;
		data = src.data;
		editorData = src.editorData;
		materialId = src.materialId;
		gradientCount = src.gradientCount;
		gradientTexture = src.gradientTexture;
		useGradient = src.useGradient;
	}
	
	public string Name
	{
		get
		{
			if (bmFont == null || texture == null)
				return "Empty";
			else
			{
				if (data == null)
					return bmFont.name + " (Inactive)";
				else
					return bmFont.name;
			}
		}
	}
	
	public bool InUse
	{
		get { return active && bmFont != null && texture != null && data != null && editorData != null; }
	}
}

[System.Serializable]
public class tk2dSpriteCollectionPlatform
{
	public string name = "";
	public tk2dSpriteCollection spriteCollection = null;
	public bool Valid { get { return name.Length > 0 && spriteCollection != null; } }
	public void CopyFrom(tk2dSpriteCollectionPlatform source)
	{
		name = source.name;
		spriteCollection = source.spriteCollection;
	}
}

[AddComponentMenu("2D Toolkit/Backend/tk2dSpriteCollection")]
public class tk2dSpriteCollection : MonoBehaviour 
{
	public const int CURRENT_VERSION = 4;

	public enum NormalGenerationMode
	{
		None,
		NormalsOnly,
		NormalsAndTangents,
	};
	
    // Deprecated fields
    [SerializeField] private tk2dSpriteCollectionDefinition[] textures; 
    [SerializeField] private Texture2D[] textureRefs;

    public Texture2D[] DoNotUse__TextureRefs { get { return textureRefs; } set { textureRefs = value; } } // Don't use this for anything. Except maybe in tk2dSpriteCollectionBuilderDeprecated...

    // new method
	public tk2dSpriteSheetSource[] spriteSheets;

	public tk2dSpriteCollectionFont[] fonts;
	public tk2dSpriteCollectionDefault defaults;

	// platforms
	public List<tk2dSpriteCollectionPlatform> platforms = new List<tk2dSpriteCollectionPlatform>();
	public bool managedSpriteCollection = false; // true when generated and managed by system, eg. platform specific data
	public bool HasPlatformData { get { return platforms.Count > 1; } }
	public bool loadable = false;
	
	public int maxTextureSize = 2048;
	
	public bool forceTextureSize = false;
	public int forcedTextureWidth = 2048;
	public int forcedTextureHeight = 2048;
	
	public enum TextureCompression
	{
		Uncompressed,
		Reduced16Bit,
		Compressed,
		Dithered16Bit_Alpha,
		Dithered16Bit_NoAlpha,
	}

	public TextureCompression textureCompression = TextureCompression.Uncompressed;
	
	public int atlasWidth, atlasHeight;
	public bool forceSquareAtlas = false;
	public float atlasWastage;
	public bool allowMultipleAtlases = false;
	public bool removeDuplicates = true;
	
    public tk2dSpriteCollectionDefinition[] textureParams;
    
	public tk2dSpriteCollectionData spriteCollection;
    public bool premultipliedAlpha = false;
	
	public Material[] altMaterials;
	public Material[] atlasMaterials;
	public Texture2D[] atlasTextures;
	
	[SerializeField] private bool useTk2dCamera = false;
	[SerializeField] private int targetHeight = 640;
	[SerializeField] private float targetOrthoSize = 10.0f;
	
	// New method of storing sprite size
	public tk2dSpriteCollectionSize sizeDef = tk2dSpriteCollectionSize.Default();

	public float globalScale = 1.0f;
	public float globalTextureRescale = 1.0f;

	// Remember test data for attach points
	[System.Serializable]
	public class AttachPointTestSprite {
		public string attachPointName = "";
		public tk2dSpriteCollectionData spriteCollection = null;
		public int spriteId = -1;
		public bool CompareTo(AttachPointTestSprite src) {
			return src.attachPointName == attachPointName && src.spriteCollection == spriteCollection && src.spriteId == spriteId;
		}
		public void CopyFrom(AttachPointTestSprite src) {
			attachPointName = src.attachPointName;
			spriteCollection = src.spriteCollection;
			spriteId = src.spriteId;
		}
	}
	public List<AttachPointTestSprite> attachPointTestSprites = new List<AttachPointTestSprite>();
	
	// Texture settings
	[SerializeField]
	private bool pixelPerfectPointSampled = false; // obsolete
	public FilterMode filterMode = FilterMode.Bilinear;
	public TextureWrapMode wrapMode = TextureWrapMode.Clamp;
	public bool userDefinedTextureSettings = false;
	public bool mipmapEnabled = false;
	public int anisoLevel = 1;

	public float physicsDepth = 0.1f;
	
	public bool disableTrimming = false;
	
	public NormalGenerationMode normalGenerationMode = NormalGenerationMode.None;
	
	public int padAmount = -1; // default
	
	public bool autoUpdate = true;
	
	public float editorDisplayScale = 1.0f;

	public int version = 0;

	public string assetName = "";

	// Fix up upgraded data structures
	public void Upgrade()
	{
		if (version == CURRENT_VERSION)
			return;

		Debug.Log("SpriteCollection '" + this.name + "' - Upgraded from version " + version.ToString());

		if (version == 0)
		{
			if (pixelPerfectPointSampled)
				filterMode = FilterMode.Point;
			else
				filterMode = FilterMode.Bilinear;

			// don't bother messing about with user settings
			// on old atlases
			userDefinedTextureSettings = true; 
		}

		if (version < 3)
		{
			if (textureRefs != null && textureParams != null && textureRefs.Length == textureParams.Length)
			{
				for (int i = 0; i < textureRefs.Length; ++i)
					textureParams[i].texture = textureRefs[i];

				textureRefs = null;
			}
		}

		if (version < 4) {
			sizeDef.CopyFromLegacy( useTk2dCamera, targetOrthoSize, targetHeight );
		}

		version = CURRENT_VERSION;

#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
	}
}
