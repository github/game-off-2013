using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteFromTexture")]
[ExecuteInEditMode]
public class tk2dSpriteFromTexture : MonoBehaviour {

	public Texture texture = null;
	public tk2dSpriteCollectionSize spriteCollectionSize = new tk2dSpriteCollectionSize();
	public tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.MiddleCenter;
	tk2dSpriteCollectionData spriteCollection;

	tk2dBaseSprite _sprite;
	tk2dBaseSprite Sprite {
		get {
			if (_sprite == null) {
				_sprite = GetComponent<tk2dBaseSprite>();
				if (_sprite == null) {
					Debug.Log("tk2dSpriteFromTexture - Missing sprite object. Creating.");
					_sprite = gameObject.AddComponent<tk2dSprite>();
				}
			}
			return _sprite;
		}
	}

	void Awake() {
		Create( spriteCollectionSize, texture, anchor );
	}

	public bool HasSpriteCollection {
		get { return spriteCollection != null; }
	}

	void OnDestroy() {
		DestroyInternal();
		if (renderer != null) {
			renderer.material = null;
		}
	}

	public void Create( tk2dSpriteCollectionSize spriteCollectionSize, Texture texture, tk2dBaseSprite.Anchor anchor ) {
		DestroyInternal();
		if (texture != null) {
			// Copy values
			this.spriteCollectionSize.CopyFrom( spriteCollectionSize );
			this.texture = texture;
			this.anchor = anchor;

			GameObject go = new GameObject("tk2dSpriteFromTexture - " + texture.name);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			go.hideFlags = HideFlags.DontSave;
			
			Vector2 anchorPos = tk2dSpriteGeomGen.GetAnchorOffset( anchor, texture.width, texture.height );
			spriteCollection = tk2dRuntime.SpriteCollectionGenerator.CreateFromTexture(
				go, 
				texture, 
				spriteCollectionSize,
				new Vector2(texture.width, texture.height),
				new string[] { "unnamed" } ,
				new Rect[] { new Rect(0, 0, texture.width, texture.height) },
				null,
				new Vector2[] { anchorPos },
				new bool[] { false } );

			string objName = "SpriteFromTexture " + texture.name;
			spriteCollection.spriteCollectionName = objName;
			spriteCollection.spriteDefinitions[0].material.name = objName;
			spriteCollection.spriteDefinitions[0].material.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;

			Sprite.SetSprite( spriteCollection, 0 );
		}
	}

	public void Clear() {
		DestroyInternal();
	}

	public void ForceBuild() {
		DestroyInternal();
		Create( spriteCollectionSize, texture, anchor );
	}

	void DestroyInternal() {
		if (spriteCollection != null) {
			if (spriteCollection.spriteDefinitions[0].material != null) {
				DestroyImmediate( spriteCollection.spriteDefinitions[0].material );
			}
			DestroyImmediate( spriteCollection.gameObject );
			spriteCollection = null;
		}
	}
}
