using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace tk2dEditor.SpriteCollectionBuilder
{
	public static class Deprecated
	{
		public static bool CheckAndFixUpParams(tk2dSpriteCollection gen)
		{
			if (gen.DoNotUse__TextureRefs != null && gen.textureParams != null && gen.DoNotUse__TextureRefs.Length != gen.textureParams.Length)
	        {
				tk2dSpriteCollectionDefinition[] newDefs = new tk2dSpriteCollectionDefinition[gen.DoNotUse__TextureRefs.Length];
				int c = Mathf.Min( newDefs.Length, gen.textureParams.Length );

				if (gen.DoNotUse__TextureRefs.Length > gen.textureParams.Length)
				{
					Texture2D[] newTexRefs = new Texture2D[gen.DoNotUse__TextureRefs.Length - gen.textureParams.Length];
					System.Array.Copy(gen.DoNotUse__TextureRefs, gen.textureParams.Length, newTexRefs, 0, newTexRefs.Length);
					System.Array.Sort(newTexRefs, (Texture2D a, Texture2D b) => tk2dSpriteGuiUtility.NameCompare(a?a.name:"", b?b.name:""));
					System.Array.Copy(newTexRefs, 0, gen.DoNotUse__TextureRefs, gen.textureParams.Length, newTexRefs.Length);
				}

				for (int i = 0; i < c; ++i)
				{
					newDefs[i] = new tk2dSpriteCollectionDefinition();
					newDefs[i].CopyFrom( gen.textureParams[i] );
				}
				for (int i = c; i < newDefs.Length; ++i)
				{
					newDefs[i] = new tk2dSpriteCollectionDefinition();
					newDefs[i].pad = gen.defaults.pad;
					newDefs[i].additive = gen.defaults.additive;
					newDefs[i].anchor = gen.defaults.anchor;
					newDefs[i].scale = gen.defaults.scale;
					newDefs[i].colliderType = gen.defaults.colliderType;
				}
				gen.textureParams = newDefs;
	        }

			// clear thumbnails on build
			foreach (var param in gen.textureParams)
			{
				param.thumbnailTexture = null;
			}

			foreach (var param in gen.textureParams)
			{
				if (gen.allowMultipleAtlases && param.dice)
				{
					EditorUtility.DisplayDialog("Error",
					                            "Multiple atlas spanning is not allowed when there are textures with dicing enabled in the SpriteCollection.",
								                "Ok");

					gen.allowMultipleAtlases = false;

					return false;
				}
			}

			return true;
		}

		public static void TrimTextureList(tk2dSpriteCollection gen)
		{
			// trim textureRefs & textureParams
			int lastNonEmpty = -1;
			for (int i = 0; i < gen.DoNotUse__TextureRefs.Length; ++i)
			{
				if (gen.DoNotUse__TextureRefs[i] != null) lastNonEmpty = i;
			}
			Texture2D[] textureRefs = gen.DoNotUse__TextureRefs;
			System.Array.Resize(ref textureRefs, lastNonEmpty + 1);
			System.Array.Resize(ref gen.textureParams, lastNonEmpty + 1);
			gen.DoNotUse__TextureRefs = textureRefs;
		}
		
		public static bool SetUpSpriteSheets(tk2dSpriteCollection gen)
		{
			// delete textures which aren't in sprite sheets any more
			// and delete textures which are out of range of the spritesheet
			for (int i = 0; i < gen.DoNotUse__TextureRefs.Length; ++i)
			{
				if (gen.textureParams[i].fromSpriteSheet)
				{
					bool found = false;
					foreach (var ss in gen.spriteSheets)
					{
						if (gen.DoNotUse__TextureRefs[i] == ss.texture)
						{
							found = true;
							int numTiles = (ss.numTiles == 0)?(ss.tilesX * ss.tilesY):Mathf.Min(ss.numTiles, ss.tilesX * ss.tilesY);
							// delete textures which are out of range
							if (gen.textureParams[i].regionId >= numTiles)
							{
								gen.DoNotUse__TextureRefs[i] = null;
								gen.textureParams[i].fromSpriteSheet = false;
								gen.textureParams[i].extractRegion = false;
								gen.textureParams[i].colliderType = tk2dSpriteCollectionDefinition.ColliderType.UserDefined;
								gen.textureParams[i].boxColliderMin = Vector3.zero;
								gen.textureParams[i].boxColliderMax = Vector3.zero;
							}
						}
					}

					if (!found)
					{
						gen.DoNotUse__TextureRefs[i] = null;
						gen.textureParams[i].fromSpriteSheet = false;
						gen.textureParams[i].extractRegion = false;
						gen.textureParams[i].colliderType = tk2dSpriteCollectionDefinition.ColliderType.UserDefined;
						gen.textureParams[i].boxColliderMin = Vector3.zero;
						gen.textureParams[i].boxColliderMax = Vector3.zero;
					}
				}
			}

			if (gen.spriteSheets == null)
			{
				gen.spriteSheets = new tk2dSpriteSheetSource[0];
			}
			
			int spriteSheetId = 0;
			for (spriteSheetId = 0; spriteSheetId < gen.spriteSheets.Length; ++spriteSheetId)
			{
				var spriteSheet = gen.spriteSheets[spriteSheetId];
				
				// New mode sprite sheets have sprites already created
				if (spriteSheet.version > 0)
					continue;
				
				// Sanity check
				if (spriteSheet.texture == null)
				{
					continue; // deleted, safely ignore this
				}
				if (spriteSheet.tilesX * spriteSheet.tilesY == 0 ||
				    (spriteSheet.numTiles != 0 && spriteSheet.numTiles > spriteSheet.tilesX * spriteSheet.tilesY))
				{
					EditorUtility.DisplayDialog("Invalid sprite sheet",
					                            "Sprite sheet '" + spriteSheet.texture.name + "' has an invalid number of tiles",
					                            "Ok");
					return false;
				}
				if ((spriteSheet.texture.width % spriteSheet.tilesX) != 0 || (spriteSheet.texture.height % spriteSheet.tilesY) != 0)
				{
					EditorUtility.DisplayDialog("Invalid sprite sheet",
					                            "Sprite sheet '" + spriteSheet.texture.name + "' doesn't match tile count",
					                            "Ok");
					return false;
				}

				int numTiles = (spriteSheet.numTiles == 0)?(spriteSheet.tilesX * spriteSheet.tilesY):Mathf.Min(spriteSheet.numTiles, spriteSheet.tilesX * spriteSheet.tilesY);
				for (int y = 0; y < spriteSheet.tilesY; ++y)
				{
					for (int x = 0; x < spriteSheet.tilesX; ++x)
					{
						// limit to number of tiles, if told to
						int tileIdx = y * spriteSheet.tilesX + x;
						if (tileIdx >= numTiles)
							break;
						
						bool foundInCollection = false;
						
						// find texture in collection
						int textureIdx = -1;
						for (int i = 0; i < gen.textureParams.Length; ++i)
						{
							if (gen.textureParams[i].fromSpriteSheet
							    && gen.textureParams[i].regionId == tileIdx
							    && gen.DoNotUse__TextureRefs[i] == spriteSheet.texture)
							{
								textureIdx = i;
								foundInCollection = true;
								break;
							}
						}

						if (textureIdx == -1)
						{
							// find first empty texture slot
							for (int i = 0; i < gen.textureParams.Length; ++i)
							{
								if (gen.DoNotUse__TextureRefs[i] == null)
								{
									textureIdx = i;
									break;
								}
							}
						}

						if (textureIdx == -1)
						{
							// texture not found, so extend arrays
							Texture2D[] textureRefs = gen.DoNotUse__TextureRefs;
							System.Array.Resize(ref textureRefs, gen.DoNotUse__TextureRefs.Length + 1);
							System.Array.Resize(ref gen.textureParams, gen.textureParams.Length + 1);
							gen.DoNotUse__TextureRefs = textureRefs;
							textureIdx = gen.DoNotUse__TextureRefs.Length - 1;
						}
						
						gen.DoNotUse__TextureRefs[textureIdx] = spriteSheet.texture;
						var param = new tk2dSpriteCollectionDefinition();
						param.fromSpriteSheet = true;
						param.name = spriteSheet.texture.name + "/" + tileIdx;
						param.regionId = tileIdx;
						param.regionW = spriteSheet.texture.width / spriteSheet.tilesX;
						param.regionH = spriteSheet.texture.height / spriteSheet.tilesY;
						param.regionX = (tileIdx % spriteSheet.tilesX) * param.regionW;
						param.regionY = (spriteSheet.tilesY - 1 - (tileIdx / spriteSheet.tilesX)) * param.regionH;
						param.extractRegion = true;
						param.additive = spriteSheet.additive;

						param.pad = spriteSheet.pad;
						param.anchor = (tk2dSpriteCollectionDefinition.Anchor)spriteSheet.anchor;
						param.scale = (spriteSheet.scale.sqrMagnitude == 0.0f)?Vector3.one:spriteSheet.scale;
						
						// Let the user tweak individually
						if (foundInCollection)
						{
							param.colliderType = gen.textureParams[textureIdx].colliderType;
							param.boxColliderMin = gen.textureParams[textureIdx].boxColliderMin;
							param.boxColliderMax = gen.textureParams[textureIdx].boxColliderMax;
							param.polyColliderIslands = gen.textureParams[textureIdx].polyColliderIslands;
							param.colliderConvex = gen.textureParams[textureIdx].colliderConvex;
							param.colliderSmoothSphereCollisions = gen.textureParams[textureIdx].colliderSmoothSphereCollisions;
							param.colliderColor = gen.textureParams[textureIdx].colliderColor;
						}
						else
						{
							param.colliderType = spriteSheet.colliderType;
						}

						gen.textureParams[textureIdx] = param;
					}
				}
			}

			return true;
		}		
	}

}

