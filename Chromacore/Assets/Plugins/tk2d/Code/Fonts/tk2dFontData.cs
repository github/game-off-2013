using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
/// <summary>
/// Defines one character in a font
/// </summary>
public class tk2dFontChar
{
	/// <summary>
	/// End points forming a quad
	/// </summary>
    public Vector3 p0, p1;
	/// <summary>
	/// Uv for quad end points
	/// </summary>
    public Vector3 uv0, uv1;
	
	public bool flipped = false;
	/// <summary>
	/// Gradient Uvs for quad end points
	/// </summary>
	public Vector2[] gradientUv;
	/// <summary>
	/// Spacing required for current character, mix with <see cref="tk2dFontKerning"/> to get true advance value
	/// </summary>
    public float advance;

    public int channel = 0; // channel data for multi channel fonts. Lookup into an array (R=0, G=1, B=2, A=3)
}

[System.Serializable]
/// <summary>
/// Defines kerning within a font
/// </summary>
public class tk2dFontKerning
{
	/// <summary>
	/// First character to match
	/// </summary>
	public int c0;
	
	/// <summary>
	/// Second character to match
	/// </summary>
	public int c1;
	
	/// <summary>
	/// Kern amount.
	/// </summary>
	public float amount;
}

[AddComponentMenu("2D Toolkit/Backend/tk2dFontData")]
/// <summary>
/// Stores data to draw and display a font
/// </summary>
public class tk2dFontData : MonoBehaviour
{
	public const int CURRENT_VERSION = 2;
	
	[HideInInspector]
	public int version = 0;
	
	/// <summary>
	/// The height of the line in local units.
	/// </summary>
    public float lineHeight;
	
	/// <summary>
	/// Array of <see cref="tk2dFontChar"/>.
	/// If this.useDictionary is true, charDict will be used instead.
	/// </summary>
	public tk2dFontChar[] chars;
	
	[SerializeField]
	List<int> charDictKeys;
	[SerializeField]
	List<tk2dFontChar> charDictValues;
	
	/// <summary>
	/// Returns an array of platform names.
	/// </summary>
    public string[] fontPlatforms = null;

	/// <summary>
	/// Returns an array of GUIDs, each referring to an actual tk2dFontData object
	/// This object contains the actual font for the platform.
	/// </summary>
    public string[] fontPlatformGUIDs = null;	

	tk2dFontData platformSpecificData = null;
	public bool hasPlatformData = false;
    public bool managedFont = false;
    public bool needMaterialInstance = false;
    public bool isPacked = false;
    public bool premultipliedAlpha = false;

    public tk2dSpriteCollectionData spriteCollection = null;

	// Returns the active instance
	public tk2dFontData inst
	{
		get 
		{
			if (platformSpecificData == null || platformSpecificData.materialInst == null)
			{
				if (hasPlatformData)
				{
					string systemPlatform = tk2dSystem.CurrentPlatform;
					string guid = "";

					for (int i = 0; i < fontPlatforms.Length; ++i)
					{
						if (fontPlatforms[i] == systemPlatform)
						{
							guid = fontPlatformGUIDs[i];
							break;							
						}
					}
					if (guid.Length == 0)
						guid = fontPlatformGUIDs[0]; // failed to find platform, pick the first one

					platformSpecificData = tk2dSystem.LoadResourceByGUID<tk2dFontData>(guid);
				}
				else
				{
					platformSpecificData = this;
				}
				platformSpecificData.Init(); // awake is never called, so we initialize explicitly
			}
			return platformSpecificData;
		}
	}

	void Init()
	{
		if (needMaterialInstance)
		{
			if (spriteCollection)
			{
				tk2dSpriteCollectionData spriteCollectionInst = spriteCollection.inst;
				for (int i = 0; i < spriteCollectionInst.materials.Length; ++i)
				{
					if (spriteCollectionInst.materials[i] == material)
					{
						materialInst = spriteCollectionInst.materialInsts[i];
						break;
					}
				}
				if (materialInst == null)
					Debug.LogError("Fatal error - font from sprite collection is has an invalid material");
			}
			else
			{
				materialInst = Instantiate(material) as Material;
				materialInst.hideFlags = HideFlags.DontSave;
			}
		}
		else
		{
			materialInst = material;
		}
	}

	public void ResetPlatformData()
	{
		if (hasPlatformData && platformSpecificData)
		{
			platformSpecificData = null;
		}
		
		materialInst = null;		
	}

	void OnDestroy()
	{
		// if material is from a sprite collection
		if (needMaterialInstance && spriteCollection == null)
			DestroyImmediate(materialInst);
	}

	/// <summary>
	/// Dictionary of characters. This is used when chars is null. Chars is preferred when number of characters is low (< 2048).
	/// 
	/// </summary>
	public Dictionary<int, tk2dFontChar> charDict;
	
	/// <summary>
	/// Whether this font uses the dictionary or an array for character lookup.
	/// </summary>
	public bool useDictionary = false;
	
	/// <summary>
	/// Array of <see cref="tk2dFontKerning"/>
	/// </summary>
	public tk2dFontKerning[] kerning;
	
	/// <summary>
	/// Width of the largest character
	/// </summary>
	public float largestWidth;
	
	/// <summary>
	/// Material used by this font
	/// </summary>
	public Material material;

	[System.NonSerialized]
	public Material materialInst;
	
	// Gradients
	
	/// <summary>
	/// Reference to gradient texture
	/// </summary>
	public Texture2D gradientTexture;
	/// <summary>
	/// Does this font have gradients? Used to determine if second uv channel is necessary.
	/// </summary>
	public bool textureGradients;
	/// <summary>
	/// Number of gradients in list. 
	/// Used to determine how large the gradient uvs are and the offsets into the gradient lookup texture.
	/// </summary>
	public int gradientCount = 1;
	
	public Vector2 texelSize;
	
	[HideInInspector]
	/// <summary>
	/// The size of the inv ortho size used to generate the sprite collection.
	/// </summary>
	public float invOrthoSize = 1.0f;
	
	[HideInInspector]
	/// <summary>
	/// Half of the target height used to generate the sprite collection.
	/// </summary>
	public float halfTargetHeight = 1.0f;	
	
	/// <summary>
	/// Initializes the dictionary, if it is required
	/// </summary>
	public void InitDictionary()
	{
		if (useDictionary && charDict == null)
		{
			charDict = new Dictionary<int, tk2dFontChar>(charDictKeys.Count);
			for (int i = 0; i < charDictKeys.Count; ++i)
			{
				charDict[charDictKeys[i]] = charDictValues[i];
			}
		}
	}
	
	/// <summary>
	/// Internal function to set up the dictionary
	/// </summary>
	public void SetDictionary(Dictionary<int, tk2dFontChar> dict)
	{
		charDictKeys = new List<int>(dict.Keys);
		charDictValues = new List<tk2dFontChar>();
		for (int i = 0; i < charDictKeys.Count; ++i)
		{
			charDictValues.Add(dict[charDictKeys[i]]);
		}
	}
}
