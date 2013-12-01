using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tk2dRuntime
{
	static class SpriteCollectionGenerator 
	{
		public static tk2dSpriteCollectionData CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor)
		{
			return CreateFromTexture(texture, size, new string[] { "Unnamed" }, new Rect[] { region },  new Vector2[] { anchor } );
		}
		
		public static tk2dSpriteCollectionData CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, string[] names, Rect[] regions, Vector2[] anchors) {
			Vector2 textureDimensions = new Vector2( texture.width, texture.height );
			return CreateFromTexture( texture, size, textureDimensions, names, regions, null, anchors, null );
		}

		public static tk2dSpriteCollectionData CreateFromTexture(
			Texture texture,
			tk2dSpriteCollectionSize size,
			Vector2 textureDimensions,
			string[] names,
			Rect[] regions,
			Rect[] trimRects, Vector2[] anchors,
			bool[] rotated)
		{
			return CreateFromTexture(null, texture, size, textureDimensions, names, regions, trimRects, anchors, rotated);
		}

		public static tk2dSpriteCollectionData CreateFromTexture(
			GameObject parentObject,
			Texture texture,
			tk2dSpriteCollectionSize size,
			Vector2 textureDimensions,
			string[] names,
			Rect[] regions,
			Rect[] trimRects, Vector2[] anchors,
			bool[] rotated)
		{
			GameObject go = ( parentObject != null ) ? parentObject : new GameObject("SpriteCollection");
			tk2dSpriteCollectionData sc = go.AddComponent<tk2dSpriteCollectionData>();
			sc.Transient = true;
			sc.version = tk2dSpriteCollectionData.CURRENT_VERSION;
			
			sc.invOrthoSize = 1.0f / size.OrthoSize;
			sc.halfTargetHeight = size.TargetHeight * 0.5f;
			sc.premultipliedAlpha = false;
			
			string shaderName = "tk2d/BlendVertexColor";

#if UNITY_EDITOR
			{
				Shader ts = Shader.Find(shaderName);
				string assetPath = UnityEditor.AssetDatabase.GetAssetPath(ts);
				if (assetPath.ToLower().IndexOf("/resources/") == -1) {
					UnityEditor.EditorUtility.DisplayDialog("tk2dRuntimeSpriteCollection Error",
						"The tk2d/BlendVertexColor shader needs to be in a resources folder for this to work.\n\n" +
						"Create a subdirectory named 'resources' where the shaders are, and move the BlendVertexColor shader into this directory.\n\n"+
						"eg. TK2DROOT/tk2d/Shaders/Resources/BlendVertexColor\n\n" +
						"Be sure to do this from within Unity and not from Explorer/Finder.",
						"Ok");
					return null;
				}
			}
#endif

			sc.material = new Material(Shader.Find(shaderName));
			sc.material.mainTexture = texture;
			sc.materials = new Material[1] { sc.material };
			sc.textures = new Texture[1] { texture };
			sc.buildKey = UnityEngine.Random.Range(0, Int32.MaxValue);
			
			float scale = 2.0f * size.OrthoSize / size.TargetHeight;
			Rect trimRect = new Rect(0, 0, 0, 0);
			
			// Generate geometry
			sc.spriteDefinitions = new tk2dSpriteDefinition[regions.Length];
			for (int i = 0; i < regions.Length; ++i) {
				bool defRotated = (rotated != null) ? rotated[i] : false;
				if (trimRects != null) {
					trimRect = trimRects[i];
				}
				else {
					if (defRotated) trimRect.Set( 0, 0, regions[i].height, regions[i].width );
					else trimRect.Set( 0, 0, regions[i].width, regions[i].height );
				}
				sc.spriteDefinitions[i] = CreateDefinitionForRegionInTexture(names[i], textureDimensions, scale, regions[i], trimRect, anchors[i], defRotated);
			}
			
			foreach (var def in sc.spriteDefinitions) {
				def.material = sc.material;
			}
			
			return sc;
		}

		static tk2dSpriteDefinition CreateDefinitionForRegionInTexture(string name, Vector2 textureDimensions, float scale, Rect uvRegion, Rect trimRect, Vector2 anchor, bool rotated)
		{
			float h = uvRegion.height;
			float w = uvRegion.width;
			float fwidth = textureDimensions.x;
			float fheight = textureDimensions.y;

			var def = new tk2dSpriteDefinition();
			def.flipped = rotated ? tk2dSpriteDefinition.FlipMode.TPackerCW : tk2dSpriteDefinition.FlipMode.None;
			def.extractRegion = false;
			def.name = name;
			def.colliderType = tk2dSpriteDefinition.ColliderType.Unset;
			
			Vector2 uvOffset = new Vector2(0.001f, 0.001f);
			Vector2 v0 = new Vector2((uvRegion.x + uvOffset.x) / fwidth, 1.0f - (uvRegion.y + uvRegion.height + uvOffset.y) / fheight);
			Vector2 v1 = new Vector2((uvRegion.x + uvRegion.width - uvOffset.x) / fwidth, 1.0f - (uvRegion.y - uvOffset.y) / fheight);
			
			// Correction offset to make the sprite correctly centered at 0, 0
			Vector2 offset = new Vector2( trimRect.x - anchor.x, -trimRect.y + anchor.y );
			if (rotated) {
				offset.y -= w;
			}
			offset *= scale;

			Vector3 b0 = new Vector3( -anchor.x * scale, anchor.y * scale, 0 );
			Vector3 b1 = b0 + new Vector3( trimRect.width * scale, -trimRect.height * scale, 0 );
			
			Vector3 pos0 = new Vector3(0, -h * scale, 0.0f);
			Vector3 pos1 = pos0 + new Vector3(w * scale, h * scale, 0.0f);

			if (rotated) {
				def.positions = new Vector3[] {
					new Vector3(-pos1.y + offset.x, pos0.x + offset.y, 0),
					new Vector3(-pos0.y + offset.x, pos0.x + offset.y, 0),
					new Vector3(-pos1.y + offset.x, pos1.x + offset.y, 0),
					new Vector3(-pos0.y + offset.x, pos1.x + offset.y, 0),
				};
				def.uvs = new Vector2[] {
					new Vector2(v0.x,v1.y),
					new Vector2(v0.x,v0.y),
					new Vector2(v1.x,v1.y),
					new Vector2(v1.x,v0.y),
				};
			}
			else 
			{
				def.positions = new Vector3[] {
					new Vector3(pos0.x + offset.x, pos0.y + offset.y, 0),
					new Vector3(pos1.x + offset.x, pos0.y + offset.y, 0),
					new Vector3(pos0.x + offset.x, pos1.y + offset.y, 0),
					new Vector3(pos1.x + offset.x, pos1.y + offset.y, 0)
				};
				def.uvs = new Vector2[] {
					new Vector2(v0.x,v0.y),
					new Vector2(v1.x,v0.y),
					new Vector2(v0.x,v1.y),
					new Vector2(v1.x,v1.y)
				};
			}
			
			def.normals = new Vector3[0];
			def.tangents = new Vector4[0];
			
			def.indices = new int[] {
				0, 3, 1, 2, 3, 0
			};
			
			Vector3 boundsMin = new Vector3(b0.x, b1.y, 0);
			Vector3 boundsMax = new Vector3(b1.x, b0.y, 0);

			// todo - calc trimmed bounds properly
			def.boundsData = new Vector3[2] {
				(boundsMax + boundsMin) / 2.0f,
				(boundsMax - boundsMin)
			};
			def.untrimmedBoundsData = new Vector3[2] {
				(boundsMax + boundsMin) / 2.0f,
				(boundsMax - boundsMin)
			};

			def.texelSize = new Vector2(scale, scale);			
							
			return def;
		}

		// Texture packer import
		public static tk2dSpriteCollectionData CreateFromTexturePacker( tk2dSpriteCollectionSize spriteCollectionSize, string texturePackerFileContents, Texture texture ) {
#if !UNITY_FLASH
			List<string> names = new List<string>();
			List<Rect> rects = new List<Rect>();
			List<Rect> trimRects = new List<Rect>();
			List<Vector2> anchors = new List<Vector2>();
			List<bool> rotated = new List<bool>();

			int state = 0;
			System.IO.TextReader tr = new System.IO.StringReader(texturePackerFileContents);

			// tmp state		
			bool entryRotated = false;
			bool entryTrimmed = false;
			string entryName = "";
			Rect entryRect = new Rect();
			Rect entryTrimData = new Rect();
			Vector2 textureDimensions = Vector2.zero;
			Vector2 anchor = Vector2.zero;

			// gonna write a non-allocating parser for this one day.
			// all these substrings & splits can't be good
			// but should be a tiny bit better than an xml / json parser...
			string line = tr.ReadLine();
			while (line != null) {
				if (line.Length > 0) {
					char cmd = line[0];
					switch (state) {
						case 0: {
							switch (cmd) {
								case 'i': break; // ignore version field for now
								case 'w': textureDimensions.x = Int32.Parse(line.Substring(2)); break;
								case 'h': textureDimensions.y = Int32.Parse(line.Substring(2)); break;
								// total number of sprites would be ideal to statically allocate
								case '~': state++; break;
							}
						}
						break;

						case 1: {
							switch (cmd) {
								case 'n': entryName = line.Substring(2); break;
								case 'r': entryRotated = Int32.Parse(line.Substring(2)) == 1; break;
								case 's': { // sprite
									string[] tokens = line.Split();
									entryRect.Set( Int32.Parse(tokens[1]), Int32.Parse(tokens[2]), Int32.Parse(tokens[3]), Int32.Parse(tokens[4]) );
								}
								break;
								case 'o': { // origin
									string[] tokens = line.Split();
									entryTrimData.Set( Int32.Parse(tokens[1]), Int32.Parse(tokens[2]), Int32.Parse(tokens[3]), Int32.Parse(tokens[4]) );
									entryTrimmed = true;
								}
								break;
								case '~': {
									names.Add(entryName);
									rotated.Add(entryRotated);
									rects.Add(entryRect);
									if (!entryTrimmed) {
										// The entryRect dimensions will be the wrong way around if the sprite is rotated
										if (entryRotated) entryTrimData.Set(0, 0, entryRect.height, entryRect.width);
										else entryTrimData.Set(0, 0, entryRect.width, entryRect.height);
									}
									trimRects.Add(entryTrimData);
									anchor.Set( (int)(entryTrimData.width / 2), (int)(entryTrimData.height / 2) );
									anchors.Add( anchor );
									entryName = "";
									entryTrimmed = false;
									entryRotated = false;
								}
								break;
							}
						}
						break;
					}
				}
				line = tr.ReadLine();
			}

			return CreateFromTexture( 
				texture, 
				spriteCollectionSize,
				textureDimensions,
				names.ToArray(),
				rects.ToArray(),
				trimRects.ToArray(),
				anchors.ToArray(),
				rotated.ToArray() );
#else
			return null;
#endif
		}		
	}
}
