using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Demo/tk2dDemoRuntimeSpriteController")]
public class tk2dDemoRuntimeSpriteController : MonoBehaviour {

	// Texture for runtime sprite collection demo
	public Texture2D runtimeTexture;

	// Texture packer textures
	public Texture2D texturePackerTexture;
	public TextAsset texturePackerExportFile;

	// This object will be destroyed on startup
	public GameObject destroyOnStart;

	tk2dBaseSprite spriteInstance = null;
	tk2dSpriteCollectionData spriteCollectionInstance = null;

	// Use this for initialization
	void Start () {
		if (destroyOnStart != null) {
			Destroy(destroyOnStart);
		}	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void DestroyData() {
		if (spriteInstance != null) {
			Destroy(spriteInstance.gameObject);
		}
		if (spriteCollectionInstance != null) {
			Destroy(spriteCollectionInstance.gameObject);
		}
	}

	void DoDemoTexturePacker(tk2dSpriteCollectionSize spriteCollectionSize) {
		if (GUILayout.Button("Import")) {
			DestroyData();

			// Create atlas
			spriteCollectionInstance = tk2dSpriteCollectionData.CreateFromTexturePacker(spriteCollectionSize, texturePackerExportFile.text, texturePackerTexture );

			GameObject go = new GameObject("sprite");
			go.transform.localPosition = new Vector3(-1, 0, 0);
			spriteInstance = go.AddComponent<tk2dSprite>();
			spriteInstance.SetSprite(spriteCollectionInstance, "sun");

			go = new GameObject("sprite2");
			go.transform.parent = spriteInstance.transform;
			go.transform.localPosition = new Vector3(2, 0, 0);
			tk2dSprite sprite = go.AddComponent<tk2dSprite>();
			sprite.SetSprite(spriteCollectionInstance, "2dtoolkit_logo");

			go = new GameObject("sprite3");
			go.transform.parent = spriteInstance.transform;
			go.transform.localPosition = new Vector3(1, 1, 0);
			sprite = go.AddComponent<tk2dSprite>();
			sprite.SetSprite(spriteCollectionInstance, "button_up");

			go = new GameObject("sprite4");
			go.transform.parent = spriteInstance.transform;
			go.transform.localPosition = new Vector3(1, -1, 0);
			sprite = go.AddComponent<tk2dSprite>();
			sprite.SetSprite(spriteCollectionInstance, "Rock");
		}
	}

	void DoDemoRuntimeSpriteCollection(tk2dSpriteCollectionSize spriteCollectionSize) {
		if (GUILayout.Button("Use Full Texture")) {
			DestroyData();

			// Create a sprite, using the entire texture as the sprite
			Rect region = new Rect(0, 0, runtimeTexture.width, runtimeTexture.height);
			Vector2 anchor = new Vector2(region.width / 2, region.height / 2);
			GameObject go = tk2dSprite.CreateFromTexture(runtimeTexture, spriteCollectionSize, region, anchor);
			spriteInstance = go.GetComponent<tk2dSprite>();
			spriteCollectionInstance = spriteInstance.Collection;
		}

		if (GUILayout.Button("Extract Region)")) {
			DestroyData();

			// Create a sprite, using a region of the texture as the sprite
			Rect region = new Rect(79, 243, 215, 200);
			Vector2 anchor = new Vector2(region.width / 2, region.height / 2);
			GameObject go = tk2dSprite.CreateFromTexture(runtimeTexture, spriteCollectionSize, region, anchor);
			spriteInstance = go.GetComponent<tk2dSprite>();
			spriteCollectionInstance = spriteInstance.Collection;
		}

		if (GUILayout.Button("Extract multiple Sprites")) {
			DestroyData();

			string[] names = new string[] {
				"Extracted region",
				"Another region",
				"Full sprite",
			};
			Rect[] regions = new Rect[] {
				new Rect(79, 243, 215, 200), 
				new Rect(256, 0, 64, 64),
				new Rect(0, 0, runtimeTexture.width, runtimeTexture.height)
			};
			Vector2[] anchors = new Vector2[] {
				new Vector2(regions[0].width / 2, regions[0].height / 2),
				new Vector2(0, regions[1].height),
				new Vector2(0, regions[1].height)
			};

			// Create a sprite collection with multiple sprites, using regions of the texture
			spriteCollectionInstance = tk2dSpriteCollectionData.CreateFromTexture(runtimeTexture, spriteCollectionSize, names, regions, anchors);
			GameObject go = new GameObject("sprite");
			go.transform.localPosition = new Vector3(-1, 0, 0);
			spriteInstance = go.AddComponent<tk2dSprite>();
			spriteInstance.SetSprite(spriteCollectionInstance, 0);

			go = new GameObject("sprite2");
			go.transform.parent = spriteInstance.transform;
			go.transform.localPosition = new Vector3(2, 0, 0);
			tk2dSprite sprite = go.AddComponent<tk2dSprite>();
			sprite.SetSprite(spriteCollectionInstance, "Another region");
		}		
	}

	void OnGUI() {
		tk2dSpriteCollectionSize spriteCollectionSize = tk2dSpriteCollectionSize.Explicit(5, 640);
		// If using the tk2dCamera using pixels per meter, use this:
		//tk2dSpriteCollectionSize spriteCollectionSize = tk2dSpriteCollectionSize.PixelsPerMeter( 20 );

		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical("box");
		GUILayout.Label("Runtime Sprite Collection");
		DoDemoRuntimeSpriteCollection( spriteCollectionSize );
		GUILayout.EndVertical();

		GUILayout.BeginVertical("box");
		GUILayout.Label("Texture Packer Import");
		DoDemoTexturePacker( spriteCollectionSize );
		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
	}
}
