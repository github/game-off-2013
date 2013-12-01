using UnityEngine;

public static class tk2dTextGeomGen
{
	public class GeomData
	{
		internal tk2dTextMeshData textMeshData = null;
		internal tk2dFontData fontInst = null;
		internal string formattedText = "";
	}

	// Use this to get a correctly set up textgeomdata object
	// This uses a static global tmpData object and is not thread safe
	// Fortunately for us, neither is the rest of Unity.
	public static GeomData Data(tk2dTextMeshData textMeshData, tk2dFontData fontData, string formattedText) {
		tmpData.textMeshData = textMeshData;
		tmpData.fontInst = fontData;
		tmpData.formattedText = formattedText;
		return tmpData;
	}
	private static GeomData tmpData = new GeomData();

	/// <summary>
	/// Calculates the mesh dimensions for the given string
	/// and returns a width and height.
	/// </summary>
	public static Vector2 GetMeshDimensionsForString(string str, GeomData geomData)
	{
		tk2dTextMeshData data = geomData.textMeshData;
		tk2dFontData _fontInst = geomData.fontInst;

		float maxWidth = 0.0f;
		
		float cursorX = 0.0f;
		float cursorY = 0.0f;

		bool ignoreNextCharacter = false;
		int target = 0;
		for (int i = 0; i < str.Length && target < data.maxChars; ++i)
		{
			if (ignoreNextCharacter) {
				ignoreNextCharacter = false;
				continue;
			}

			int idx = str[i];
			if (idx == '\n')
			{
				maxWidth = Mathf.Max(cursorX, maxWidth);
				cursorX = 0.0f;
				cursorY -= (_fontInst.lineHeight + data.lineSpacing) * data.scale.y;
				continue;
			}
			else if (data.inlineStyling)
			{
				if (idx == '^' && i + 1 < str.Length)
				{
					if (str[i + 1] == '^') {
						ignoreNextCharacter = true;
					} else {
						int cmdLength = 0;
						switch (str[i + 1]) {
							case 'c': cmdLength = 5; break;
							case 'C': cmdLength = 9; break;
							case 'g': cmdLength = 9; break;
							case 'G': cmdLength = 17; break;
						}
						i += cmdLength;
						continue;
					}
				}
			}

			bool inlineHatChar = (idx == '^');
			
			// Get the character from dictionary / array
			tk2dFontChar chr;
			if (_fontInst.useDictionary)
			{
				if (!_fontInst.charDict.ContainsKey(idx)) idx = 0;
				chr = _fontInst.charDict[idx];
			}
			else
			{
				if (idx >= _fontInst.chars.Length) idx = 0; // should be space
				chr = _fontInst.chars[idx];
			}

			if (inlineHatChar) idx = '^';
			
			cursorX += (chr.advance + data.spacing) * data.scale.x;
			if (data.kerning && i < str.Length - 1)
			{
				foreach (var k in _fontInst.kerning)
				{
					if (k.c0 == str[i] && k.c1 == str[i+1])
					{
						cursorX += k.amount * data.scale.x;
						break;
					}
				}
			}				
			
			++target;
		}
		
		maxWidth = Mathf.Max(cursorX, maxWidth);
		cursorY -= (_fontInst.lineHeight + data.lineSpacing) * data.scale.y;
		
		return new Vector2(maxWidth, cursorY);
	}

	public static float GetYAnchorForHeight(float textHeight, GeomData geomData)
	{
		tk2dTextMeshData data = geomData.textMeshData;
		tk2dFontData _fontInst = geomData.fontInst;

		int heightAnchor = (int)data.anchor / 3;
		float lineHeight = (_fontInst.lineHeight + data.lineSpacing) * data.scale.y;
		switch (heightAnchor)
		{
		case 0: return -lineHeight;
		case 1:
		{
			float y = -textHeight / 2.0f - lineHeight;
			if (_fontInst.version >= 2) 
			{
				float ty = _fontInst.texelSize.y * data.scale.y;
				return Mathf.Floor(y / ty) * ty;
			}
			else return y;
		}
		case 2: return -textHeight - lineHeight;
		}
		return -lineHeight;
	}
	
	public static float GetXAnchorForWidth(float lineWidth, GeomData geomData)
	{
		tk2dTextMeshData data = geomData.textMeshData;
		tk2dFontData _fontInst = geomData.fontInst;

		int widthAnchor = (int)data.anchor % 3;
		switch (widthAnchor)
		{
		case 0: return 0.0f; // left
		case 1: // center
		{
			float x = -lineWidth / 2.0f;
			if (_fontInst.version >= 2) 
			{
				float tx = _fontInst.texelSize.x * data.scale.x;
				return Mathf.Floor(x / tx) * tx;
			}
			return x;
		}
		case 2: return -lineWidth; // right
		}
		return 0.0f;
	}

	static void PostAlignTextData(Vector3[] pos, int offset, int targetStart, int targetEnd, float offsetX)
	{
		for (int i = targetStart * 4; i < targetEnd * 4; ++i)
		{
			Vector3 v = pos[offset + i];
			v.x += offsetX;
			pos[offset + i] = v;
		}
	}

	// Channel select color constants
	static readonly Color32[] channelSelectColors = new Color32[] { new Color32(0,0,255,0), new Color(0,255,0,0), new Color(255,0,0,0), new Color(0,0,0,255) };

	// Inline styling
	static Color32 meshTopColor = new Color32(255, 255, 255, 255);
	static Color32 meshBottomColor = new Color32(255, 255, 255, 255);
	static float meshGradientTexU = 0.0f;
	static int curGradientCount = 1;

	static Color32 errorColor = new Color32(255, 0, 255, 255);

	static int GetFullHexColorComponent(int c1, int c2) {
		int result = 0;
		if (c1 >= '0' && c1 <= '9') result += (c1 - '0') * 16;
		else if (c1 >= 'a' && c1 <= 'f') result += (10 + c1 - 'a') * 16;
		else if (c1 >= 'A' && c1 <= 'F') result += (10 + c1 - 'A') * 16;
		else return -1;
		if (c2 >= '0' && c2 <= '9') result += (c2 - '0');
		else if (c2 >= 'a' && c2 <= 'f') result += (10 + c2 - 'a');
		else if (c2 >= 'A' && c2 <= 'F') result += (10 + c2 - 'A');
		else return -1;
		return result;
	}

	static int GetCompactHexColorComponent(int c) {
		if (c >= '0' && c <= '9') return (c - '0') * 17;
		if (c >= 'a' && c <= 'f') return (10 + c - 'a') * 17;
		if (c >= 'A' && c <= 'F') return (10 + c - 'A') * 17;
		return -1;
	}

	static int GetStyleHexColor(string str, bool fullHex, ref Color32 color) {
		int r, g, b, a;
		if (fullHex) {
			if (str.Length < 8) return 1;
			r = GetFullHexColorComponent(str[0], str[1]);
			g = GetFullHexColorComponent(str[2], str[3]);
			b = GetFullHexColorComponent(str[4], str[5]);
			a = GetFullHexColorComponent(str[6], str[7]);
		} else {
			if (str.Length < 4) return 1;
			r = GetCompactHexColorComponent(str[0]);
			g = GetCompactHexColorComponent(str[1]);
			b = GetCompactHexColorComponent(str[2]);
			a = GetCompactHexColorComponent(str[3]);
		}
		if (r == -1 || g == -1 || b == -1 || a == -1) {
			return 1;
		}
		color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
		return 0;
	}

	static int SetColorsFromStyleCommand(string args, bool twoColors, bool fullHex) {
		int argLength = (twoColors ? 2 : 1) * (fullHex ? 8 : 4);
		bool error = false;
		if (args.Length >= argLength) {
			if (GetStyleHexColor(args, fullHex, ref meshTopColor) != 0) {
				error = true;
			}
			if (twoColors) {
				string color2 = args.Substring (fullHex ? 8 : 4);
				if (GetStyleHexColor(color2, fullHex, ref meshBottomColor) != 0) {
					error = true;
				}
			}
			else {
				meshBottomColor = meshTopColor;
			}
		}
		else {
			error = true;
		}
		if (error) {
			meshTopColor = meshBottomColor = errorColor;
		}
		return argLength;
	}

	static void SetGradientTexUFromStyleCommand(int arg) {
		meshGradientTexU = (float)(arg - '0') / (float)((curGradientCount > 0) ? curGradientCount : 1);
	}

	static int HandleStyleCommand(string cmd) {
		if (cmd.Length == 0) return 0;

		int cmdSymbol = cmd[0];
		string cmdArgs = cmd.Substring(1);
		int cmdLength = 0;

		switch (cmdSymbol) {
		case 'c': cmdLength = 1 + SetColorsFromStyleCommand(cmdArgs, false, false); break;
		case 'C': cmdLength = 1 + SetColorsFromStyleCommand(cmdArgs, false, true); break;
		case 'g': cmdLength = 1 + SetColorsFromStyleCommand(cmdArgs, true, false); break;
		case 'G': cmdLength = 1 + SetColorsFromStyleCommand(cmdArgs, true, true); break;
		}
		if (cmdSymbol >= '0' && cmdSymbol <= '9') {
			SetGradientTexUFromStyleCommand(cmdSymbol);
			cmdLength = 1;
		}

		return cmdLength;
	}




	public static void GetTextMeshGeomDesc(out int numVertices, out int numIndices, GeomData geomData)
	{
		tk2dTextMeshData data = geomData.textMeshData;

		numVertices = data.maxChars * 4;
		numIndices = data.maxChars * 6;
	}
	
	public static int SetTextMeshGeom(Vector3[] pos, Vector2[] uv, Vector2[] uv2, Color32[] color, int offset, GeomData geomData)
	{
		tk2dTextMeshData data = geomData.textMeshData;
		tk2dFontData fontInst = geomData.fontInst;
		string formattedText = geomData.formattedText;

		meshTopColor = new Color32(255, 255, 255, 255);
		meshBottomColor = new Color32(255, 255, 255, 255);
		meshGradientTexU = (float)data.textureGradient / (float)((fontInst.gradientCount > 0) ? fontInst.gradientCount : 1);
		curGradientCount = fontInst.gradientCount;
		
		Vector2 dims = GetMeshDimensionsForString(geomData.formattedText, geomData);
		float offsetY = GetYAnchorForHeight(dims.y, geomData);

		float cursorX = 0.0f;
		float cursorY = 0.0f;
		int target = 0;
		int alignStartTarget = 0;
		for (int i = 0; i < formattedText.Length && target < data.maxChars; ++i)
		{
			int idx = formattedText[i];
			tk2dFontChar chr;

			bool inlineHatChar = (idx == '^');
			
			if (fontInst.useDictionary)
			{
				if (!fontInst.charDict.ContainsKey(idx)) idx = 0;
				chr = fontInst.charDict[idx];
			}
			else
			{
				if (idx >= fontInst.chars.Length) idx = 0; // should be space
				chr = fontInst.chars[idx];
			}

			if (inlineHatChar) idx = '^';
			
			if (idx == '\n')
			{
				float lineWidth = cursorX;
				int alignEndTarget = target; // this is one after the last filled character
				if (alignStartTarget != target)
				{
					float xOffset = GetXAnchorForWidth(lineWidth, geomData);
					PostAlignTextData(pos, offset, alignStartTarget, alignEndTarget, xOffset);
				}
				
				
				alignStartTarget = target;
				cursorX = 0.0f;
				cursorY -= (fontInst.lineHeight + data.lineSpacing) * data.scale.y;
				continue;
			}
			else if (data.inlineStyling)
			{
				if (idx == '^')
				{
					if (i + 1 < formattedText.Length && formattedText[i + 1] == '^') {
						++i;
					} else {
						i += HandleStyleCommand(formattedText.Substring(i + 1));
						continue;
					}
				}
			}
			
			pos[offset + target * 4 + 0] = new Vector3(cursorX + chr.p0.x * data.scale.x, offsetY + cursorY + chr.p0.y * data.scale.y, 0);
			pos[offset + target * 4 + 1] = new Vector3(cursorX + chr.p1.x * data.scale.x, offsetY + cursorY + chr.p0.y * data.scale.y, 0);
			pos[offset + target * 4 + 2] = new Vector3(cursorX + chr.p0.x * data.scale.x, offsetY + cursorY + chr.p1.y * data.scale.y, 0);
			pos[offset + target * 4 + 3] = new Vector3(cursorX + chr.p1.x * data.scale.x, offsetY + cursorY + chr.p1.y * data.scale.y, 0);
			
			if (chr.flipped)
			{
				uv[offset + target * 4 + 0] = new Vector2(chr.uv1.x, chr.uv1.y);
				uv[offset + target * 4 + 1] = new Vector2(chr.uv1.x, chr.uv0.y);
				uv[offset + target * 4 + 2] = new Vector2(chr.uv0.x, chr.uv1.y);
				uv[offset + target * 4 + 3] = new Vector2(chr.uv0.x, chr.uv0.y);
			}
			else			
			{
				uv[offset + target * 4 + 0] = new Vector2(chr.uv0.x, chr.uv0.y);
				uv[offset + target * 4 + 1] = new Vector2(chr.uv1.x, chr.uv0.y);
				uv[offset + target * 4 + 2] = new Vector2(chr.uv0.x, chr.uv1.y);
				uv[offset + target * 4 + 3] = new Vector2(chr.uv1.x, chr.uv1.y);
			}
			
			if (fontInst.textureGradients)
			{
				uv2[offset + target * 4 + 0] = chr.gradientUv[0] + new Vector2(meshGradientTexU, 0);
				uv2[offset + target * 4 + 1] = chr.gradientUv[1] + new Vector2(meshGradientTexU, 0);
				uv2[offset + target * 4 + 2] = chr.gradientUv[2] + new Vector2(meshGradientTexU, 0);
				uv2[offset + target * 4 + 3] = chr.gradientUv[3] + new Vector2(meshGradientTexU, 0);
			}
			
			if (fontInst.isPacked)
			{
				Color32 c = channelSelectColors[chr.channel];
				color[offset + target * 4 + 0] = c;
				color[offset + target * 4 + 1] = c;
				color[offset + target * 4 + 2] = c;
				color[offset + target * 4 + 3] = c;
			}
			else {
				color[offset + target * 4 + 0] = meshTopColor;
				color[offset + target * 4 + 1] = meshTopColor;
				color[offset + target * 4 + 2] = meshBottomColor;
				color[offset + target * 4 + 3] = meshBottomColor;
			}
			
			cursorX += (chr.advance + data.spacing) * data.scale.x;
			
			if (data.kerning && i < formattedText.Length - 1)
			{
				foreach (var k in fontInst.kerning)
				{
					if (k.c0 == formattedText[i] && k.c1 == formattedText[i+1])
					{
						cursorX += k.amount * data.scale.x;
						break;
					}
				}
			}				
			
			++target;
		}
		
		if (alignStartTarget != target)
		{
			float lineWidth = cursorX;
			int alignEndTarget = target;
			float xOffset = GetXAnchorForWidth(lineWidth, geomData);
			PostAlignTextData(pos, offset, alignStartTarget, alignEndTarget, xOffset);
		}
		
		for (int i = target; i < data.maxChars; ++i)
		{
			pos[offset + i * 4 + 0] = pos[offset + i * 4 + 1] = pos[offset + i * 4 + 2] = pos[offset + i * 4 + 3] = Vector3.zero;
			uv[offset + i * 4 + 0] = uv[offset + i * 4 + 1] = uv[offset + i * 4 + 2] = uv[offset + i * 4 + 3] = Vector2.zero;
			if (fontInst.textureGradients) 
			{
				uv2[offset + i * 4 + 0] = uv2[offset + i * 4 + 1] = uv2[offset + i * 4 + 2] = uv2[offset + i * 4 + 3] = Vector2.zero;
			}				
			
			if (!fontInst.isPacked)
			{
				color[offset + i * 4 + 0] = color[offset + i * 4 + 1] = meshTopColor;
				color[offset + i * 4 + 2] = color[offset + i * 4 + 3] = meshBottomColor;
			}
			else
			{
				color[offset + i * 4 + 0] = color[offset + i * 4 + 1] = color[offset + i * 4 + 2] = color[offset + i * 4 + 3] = Color.clear;
			}
			

		}
		
		return target;	
	}
	
	public static void SetTextMeshIndices(int[] indices, int offset, int vStart, GeomData geomData, int target)
	{
		tk2dTextMeshData data = geomData.textMeshData;
		for (int i = 0; i < data.maxChars; ++i)
		{
			indices[offset + i * 6 + 0] = vStart + i * 4 + 0;
			indices[offset + i * 6 + 1] = vStart + i * 4 + 1;
			indices[offset + i * 6 + 2] = vStart + i * 4 + 3;
			indices[offset + i * 6 + 3] = vStart + i * 4 + 2;
			indices[offset + i * 6 + 4] = vStart + i * 4 + 0;
			indices[offset + i * 6 + 5] = vStart + i * 4 + 3;
		}
	}
}