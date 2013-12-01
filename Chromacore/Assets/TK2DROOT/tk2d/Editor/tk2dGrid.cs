using UnityEngine;
using System.Collections;

public class tk2dGrid {

	static tk2dGrid inst = null;

	public enum Type
	{
		LightChecked,
		MediumChecked,
		DarkChecked,
		BlackChecked,
		LightSolid,
		MediumSolid,
		DarkSolid,
		BlackSolid,
		Custom
	}

	public static void Done() {
		if (inst != null) {
			inst.DestroyTexture();
			inst = null;
		}
	}

	const int textureSize = 16;

	public static void Draw(Rect rect) {
		Draw(rect, Vector2.zero);
	}

	public static void Draw(Rect rect, Vector2 offset) {
		if (inst == null) {
			inst = new tk2dGrid();
			inst.InitTexture();
		}
		GUI.DrawTextureWithTexCoords(rect, inst.gridTexture, new Rect(-offset.x / textureSize, (offset.y - rect.height) / textureSize, rect.width / textureSize, rect.height / textureSize), false);
	} 

	Texture2D gridTexture = null;

	void InitTexture() {
		if (gridTexture == null) {
			gridTexture = new Texture2D(textureSize, textureSize);
			Color c0 = Color.white;
			Color c1 = new Color(0.8f, 0.8f, 0.8f, 1.0f);

			Type gridType = tk2dPreferences.inst.gridType;
			switch (gridType)
			{
				case Type.LightChecked:  c0 = new Color32(255, 255, 255, 255); c1 = new Color32(217, 217, 217, 255); break;
				case Type.MediumChecked: c0 = new Color32(178, 178, 178, 255); c1 = new Color32(151, 151, 151, 255); break;
				case Type.DarkChecked:   c0 = new Color32( 37,  37,  37, 255); c1 = new Color32( 31,  31,  31, 255); break;
				case Type.BlackChecked:  c0 = new Color32( 14,  14,  14, 255); c1 = new Color32(  0,   0,   0, 255); break;
				case Type.LightSolid:    c0 = new Color32(255, 255, 255, 255); c1 = c0; break;
				case Type.MediumSolid:   c0 = new Color32(178, 178, 178, 255); c1 = c0; break;
				case Type.DarkSolid:     c0 = new Color32( 37,  37,  37, 255); c1 = c0; break;
				case Type.BlackSolid:    c0 = new Color32(  0,   0,   0, 255); c1 = c0; break;
				case Type.Custom:		 c0 = tk2dPreferences.inst.customGridColor0; c1 = tk2dPreferences.inst.customGridColor1; break;
			}

			for (int y = 0; y < gridTexture.height; ++y)
			{
				for (int x = 0; x < gridTexture.width; ++x)
				{
					bool xx = (x < gridTexture.width / 2);
					bool yy = (y < gridTexture.height / 2);
					gridTexture.SetPixel(x, y, (xx == yy)?c0:c1);
				}
			}
			gridTexture.Apply();
			gridTexture.filterMode = FilterMode.Point;
			gridTexture.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	void DestroyTexture() {
		if (gridTexture != null) {
			Object.DestroyImmediate(gridTexture);
			gridTexture = null;
		}
	}
}
