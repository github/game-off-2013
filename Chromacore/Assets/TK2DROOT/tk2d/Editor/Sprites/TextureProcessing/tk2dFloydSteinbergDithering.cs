using UnityEngine;

namespace tk2dEditor.TextureProcessing
{
	public static class FloydSteinbergDithering
	{
		/// <summary>
		/// Destructive dithering of texture.
		/// Texture is 8888, will be written out as 8888 too
		/// </summary>
		public static void DitherTexture(Texture2D texture, TextureFormat targetTextureFormat, int x0, int y0, int w, int h)	
		{
			int quantShiftR = 0, quantShiftG = 0, quantShiftB = 0, quantShiftA = 0;
			switch (targetTextureFormat)
			{
			case TextureFormat.ARGB4444:
				quantShiftR = quantShiftG = quantShiftB = quantShiftA = 4;
				break;
			case TextureFormat.RGB565:
				quantShiftR = 5;
				quantShiftB = 6;
				quantShiftG = 5;
				quantShiftA = 0;
				break;
			}
			
			int x1 = x0 + w;
			int y1 = y0 + h;
			
			for (int y = y0; y < y1; ++y)
			{
				for (int x = x0; x < x1; ++x)
				{
					Color oldPixel = texture.GetPixel(x, y);
					
					Color newPixel = new Color(  (((int)(oldPixel.r * 255.0f + 0.5f) >> quantShiftR) << quantShiftR) / 255.0f,
												 (((int)(oldPixel.g * 255.0f + 0.5f) >> quantShiftG) << quantShiftG) / 255.0f,
												 (((int)(oldPixel.b * 255.0f + 0.5f) >> quantShiftB) << quantShiftB) / 255.0f,
												 (((int)(oldPixel.a * 255.0f + 0.5f) >> quantShiftA) << quantShiftA) / 255.0f );
					Color quantizationError = oldPixel - newPixel;
					
					// write out color, but "fix up" whites
					Color targetColor = new Color((oldPixel.r == 1.0f)?1.0f:newPixel.r,
												  (oldPixel.g == 1.0f)?1.0f:newPixel.g,
												  (oldPixel.b == 1.0f)?1.0f:newPixel.b,
												  (oldPixel.a == 1.0f)?1.0f:newPixel.a);
					texture.SetPixel(x, y, targetColor);
			
					if (x < x1 - 1) texture.SetPixel(x + 1, y, texture.GetPixel(x + 1, y) + (quantizationError * 7.0f / 16.0f));
					if (y < y1 - 1)
					{
						if (x > x0) texture.SetPixel(x - 1, y + 1, texture.GetPixel(x - 1, y + 1) + (quantizationError * 3.0f / 16.0f));
						if (x < x1 - 1) texture.SetPixel(x + 1, y + 1, texture.GetPixel(x + 1, y + 1) + (quantizationError / 16.0f));
						texture.SetPixel(x, y + 1, texture.GetPixel(x, y + 1) + (quantizationError * 5.0f / 16.0f));
					}
				}
			}
		}
		
		
	}

} // namespace
