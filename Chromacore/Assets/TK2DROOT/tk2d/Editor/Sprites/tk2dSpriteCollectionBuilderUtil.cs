using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class tk2dSpriteCollectionBuilderUtil
{
	public static int NiceRescaleK( float scale ) {
		if (scale > 0.499f && scale < 0.501f) {
			return 2;
		}
		else if (scale > 0.249f && scale < 0.251f) {
			return 4;
		}
		return 0;
	}

	// Rescale a texture
	// Only supports
	public static Texture2D RescaleTexture(Texture2D texture, float scale) {
		// If globalTextureRescale is 0.5 or 0.25, average pixels from the larger image. Otherwise just pick one pixel, and look really bad
		int niceRescaleK = NiceRescaleK( scale );
		bool niceRescale = niceRescaleK != 0;
		if (texture != null) {
			int k = niceRescaleK;
			int srcW = texture.width, srcH = texture.height;
			int dstW = niceRescale ? ((srcW + k - 1) / k) : (int)(srcW * scale);
			int dstH = niceRescale ? ((srcH + k - 1) / k) : (int)(srcH * scale);
			Texture2D dstTex = new Texture2D(dstW, dstH);
			for (int dstY = 0; dstY < dstH; ++dstY) {
				for (int dstX = 0; dstX < dstW; ++dstX) {
					if (niceRescale) {
						Color sumColor = new Color(0, 0, 0, 0);
						float w = 0.0f;
						for (int dy = 0; dy < k; ++dy) {
							int srcY = dstY * k + dy;
							if (srcY >= srcH) continue;
							for (int dx = 0; dx < k; ++dx) {
								int srcX = dstX * k + dx;
								if (srcX >= srcW) continue;
								w += 1.0f;
								Color srcColor = texture.GetPixel(srcX, srcY);
								sumColor += srcColor;
							}
						}
						dstTex.SetPixel(dstX, dstY, (w > 0.0f) ? (sumColor * (1.0f / w)) : Color.black);
					} else {
						dstTex.SetPixel(dstX, dstY, texture.GetPixelBilinear((float)dstX / (float)dstW, (float)dstY / (float)dstH));
					}
				}
			}
			dstTex.Apply();
			return dstTex;
		}
		else {
			return null;
		}
	}

}
