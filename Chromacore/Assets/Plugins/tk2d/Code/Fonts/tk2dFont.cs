using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Backend/tk2dFont")]
public class tk2dFont : MonoBehaviour 
{
	public Object bmFont;
	public Material material;
	public Texture texture;
	public Texture2D gradientTexture;
    public bool dupeCaps = false; // duplicate lowercase into uc, or vice-versa, depending on which exists
	public bool flipTextureY = false;
	
	[HideInInspector]
	public bool proxyFont = false;

	[HideInInspector]
	private bool useTk2dCamera = false;
	[HideInInspector]
	private int targetHeight = 640;
	[HideInInspector]
	private float targetOrthoSize = 1.0f;

	public tk2dSpriteCollectionSize sizeDef = tk2dSpriteCollectionSize.Default();
	
	public int gradientCount = 1;
	
	public bool manageMaterial = false;
	
	[HideInInspector]
	public bool loadable = false;
	
	public int charPadX = 0;
	
	public tk2dFontData data;

	public static int CURRENT_VERSION = 1;
	public int version = 0;

	public void Upgrade() {
		if (version >= CURRENT_VERSION) {
			return;
		}
		Debug.Log("Font '" + this.name + "' - Upgraded from version " + version.ToString());

		if (version == 0) {
			sizeDef.CopyFromLegacy( useTk2dCamera, targetOrthoSize, targetHeight );
		}

		version = CURRENT_VERSION;
	}
}
