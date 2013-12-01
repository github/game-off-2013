using UnityEngine;

public static class tk2dSpriteGeomGen
{
	// Common
	public static void SetSpriteColors(Color32[] dest, int offset, int numVertices, Color c, bool premulAlpha)
	{
		if (premulAlpha) { c.r *= c.a; c.g *= c.a; c.b *= c.a; }
		Color32 c32 = c;
		
		for (int i = 0; i < numVertices; ++i)
			dest[offset + i] = c32;
	}

	public static Vector2 GetAnchorOffset( tk2dBaseSprite.Anchor anchor, float width, float height ) {
		Vector2 anchorOffset = Vector2.zero;

		switch (anchor) {
		case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.UpperLeft: 
			break;
		case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.UpperCenter: 
			anchorOffset.x = (int)(width / 2.0f); break;
		case tk2dBaseSprite.Anchor.LowerRight: case tk2dBaseSprite.Anchor.MiddleRight: case tk2dBaseSprite.Anchor.UpperRight: 
			anchorOffset.x = (int)(width); break;
		}
		switch (anchor) {
		case tk2dBaseSprite.Anchor.UpperLeft: case tk2dBaseSprite.Anchor.UpperCenter: case tk2dBaseSprite.Anchor.UpperRight:
			break;
		case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.MiddleRight:
			anchorOffset.y = (int)(height / 2.0f); break;
		case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.LowerRight:
			anchorOffset.y = (int)height; break;
		}

		return anchorOffset;
	}

	// Sprite
	public static void GetSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef)
	{
		numVertices = spriteDef.positions.Length;
		numIndices = spriteDef.indices.Length;
	}

	public static void SetSpriteGeom(Vector3[] pos, Vector2[] uv, Vector3[] norm, Vector4[] tang, int offset, tk2dSpriteDefinition spriteDef, Vector3 scale)
	{
		for (int i = 0; i < spriteDef.positions.Length; ++i)
		{
			pos[offset + i] = Vector3.Scale(spriteDef.positions[i], scale);
		}
		for (int i = 0; i < spriteDef.uvs.Length; ++i)
		{
			uv[offset + i] = spriteDef.uvs[i];
		}
		if (norm != null && spriteDef.normals != null)
		{
			for (int i = 0; i < spriteDef.normals.Length; ++i)
			{
				norm[offset + i] = spriteDef.normals[i];
			}
		}
		if (tang != null && spriteDef.tangents != null)
		{
			for (int i = 0; i < spriteDef.tangents.Length; ++i)
			{
				tang[offset + i] = spriteDef.tangents[i];
			}
		}
	}

	public static void SetSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef)
	{
		for (int i = 0; i < spriteDef.indices.Length; ++i)
		{
			indices[offset + i] = vStart + spriteDef.indices[i];
		}
	}

	// Clipped sprite

	public static void GetClippedSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef)
	{
		if (spriteDef.positions.Length == 4)
		{
			numVertices = 4;
			numIndices = 6;
		}
		else {
			numVertices = 0;
			numIndices = 0;
		}
	}

	public static void SetClippedSpriteGeom( Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 clipBottomLeft, Vector2 clipTopRight, float colliderOffsetZ, float colliderExtentZ )
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		if (spriteDef.positions.Length == 4)
		{
			// Transform clipped region from untrimmed -> trimmed region
			Vector3 untrimmedMin = spriteDef.untrimmedBoundsData[0] - spriteDef.untrimmedBoundsData[1] * 0.5f;
			Vector3 untrimmedMax = spriteDef.untrimmedBoundsData[0] + spriteDef.untrimmedBoundsData[1] * 0.5f;
			
			// clipBottomLeft is the fraction to start from the bottom left (0,0 - full sprite)
			// clipTopRight is the fraction to start from the top right (1,1 - full sprite)
			float left = Mathf.Lerp( untrimmedMin.x, untrimmedMax.x, clipBottomLeft.x );
			float right = Mathf.Lerp( untrimmedMin.x, untrimmedMax.x, clipTopRight.x );
			float bottom = Mathf.Lerp( untrimmedMin.y, untrimmedMax.y, clipBottomLeft.y );
			float top = Mathf.Lerp( untrimmedMin.y, untrimmedMax.y, clipTopRight.y );
			
			Vector3 trimmedBounds = spriteDef.boundsData[1];
			Vector3 trimmedOrigin = spriteDef.boundsData[0] - trimmedBounds * 0.5f;
			float clipLeft = (left - trimmedOrigin.x) / trimmedBounds.x; 
			float clipRight = (right - trimmedOrigin.x) / trimmedBounds.x;
			float clipBottom = (bottom - trimmedOrigin.y) / trimmedBounds.y;
			float clipTop = (top - trimmedOrigin.y) / trimmedBounds.y;

			// The fractional clip region relative to the trimmed region
			Vector2 fracBottomLeft = new Vector2( Mathf.Clamp01( clipLeft ), Mathf.Clamp01( clipBottom ) );
			Vector2 fracTopRight = new Vector2( Mathf.Clamp01( clipRight ), Mathf.Clamp01( clipTop ) );

			// Default quad has index 0 = bottomLeft,  index 3 = topRight
			Vector3 c0 = spriteDef.positions[0];
			Vector3 c1 = spriteDef.positions[3];
			
			// find the fraction of positions, but fold in the scale multiply as well
			Vector3 bottomLeft = new Vector3(Mathf.Lerp(c0.x, c1.x, fracBottomLeft.x) * scale.x,
			                                 Mathf.Lerp(c0.y, c1.y, fracBottomLeft.y) * scale.y,
			                                 c0.z * scale.z);
			Vector3 topRight = new Vector3(Mathf.Lerp(c0.x, c1.x, fracTopRight.x) * scale.x,
			                               Mathf.Lerp(c0.y, c1.y, fracTopRight.y) * scale.y,
			                               c0.z * scale.z);

			boundsCenter.Set( bottomLeft.x + (topRight.x - bottomLeft.x) * 0.5f, bottomLeft.y + (topRight.y - bottomLeft.y) * 0.5f, colliderOffsetZ );
			boundsExtents.Set( (topRight.x - bottomLeft.x) * 0.5f, (topRight.y - bottomLeft.y) * 0.5f, colliderExtentZ );
			
			// The z component only needs to be consistent
			pos[offset + 0] = new Vector3(bottomLeft.x, bottomLeft.y, bottomLeft.z);
			pos[offset + 1] = new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);
			pos[offset + 2] = new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);
			pos[offset + 3] = new Vector3(topRight.x, topRight.y, bottomLeft.z);
			
			// find the fraction of UV
			// This can be done without a branch, but will end up with loads of unnecessary interpolations
			if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
			{
				Vector2 v0 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracBottomLeft.y),
				                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracBottomLeft.x));
				Vector2 v1 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracTopRight.y),
				                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracTopRight.x));
				
				uv[offset + 0] = new Vector2(v0.x, v0.y);
				uv[offset + 1] = new Vector2(v0.x, v1.y);
				uv[offset + 2] = new Vector2(v1.x, v0.y);
				uv[offset + 3] = new Vector2(v1.x, v1.y);
			}
			else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
			{
				Vector2 v0 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracBottomLeft.y),
				                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracBottomLeft.x));
				Vector2 v1 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracTopRight.y),
				                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracTopRight.x));
				
				uv[offset + 0] = new Vector2(v0.x, v0.y);
				uv[offset + 2] = new Vector2(v1.x, v0.y);
				uv[offset + 1] = new Vector2(v0.x, v1.y);
				uv[offset + 3] = new Vector2(v1.x, v1.y);
			}
			else
			{
				Vector2 v0 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracBottomLeft.x),
				                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracBottomLeft.y));
				Vector2 v1 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracTopRight.x),
				                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracTopRight.y));
				
				uv[offset + 0] = new Vector2(v0.x, v0.y);
				uv[offset + 1] = new Vector2(v1.x, v0.y);
				uv[offset + 2] = new Vector2(v0.x, v1.y);
				uv[offset + 3] = new Vector2(v1.x, v1.y);
			}
		}
	}
	
	public static void SetClippedSpriteIndices( int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef)
	{
		if (spriteDef.positions.Length == 4)
		{
			indices[offset + 0] = vStart + 0;
			indices[offset + 1] = vStart + 3;
			indices[offset + 2] = vStart + 1;
			indices[offset + 3] = vStart + 2;
			indices[offset + 4] = vStart + 3;
			indices[offset + 5] = vStart + 0;
		}
	}

	// Sliced sprite

	public static void GetSlicedSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, bool borderOnly)
	{
		if (spriteDef.positions.Length == 4)
		{
			numVertices = 16;
			numIndices = borderOnly ? (8 * 6) : (9 * 6);
		} else {
			numVertices = 0;
			numIndices = 0;
		}
	}
	
	public static void SetSlicedSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, Vector2 borderBottomLeft, Vector2 borderTopRight, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		if (spriteDef.positions.Length == 4)
		{
			float sx = spriteDef.texelSize.x;
			float sy = spriteDef.texelSize.y;
			
			Vector3[] srcVert = spriteDef.positions;
			float dx = (srcVert[1].x - srcVert[0].x);
			float dy = (srcVert[2].y - srcVert[0].y);
			
			float borderTopPixels = borderTopRight.y * dy;
			float borderBottomPixels = borderBottomLeft.y * dy;
			float borderRightPixels = borderTopRight.x * dx;
			float borderLeftPixels = borderBottomLeft.x * dx;
			
			float dimXPixels = dimensions.x * sx;
			float dimYPixels = dimensions.y * sy;
			
			float anchorOffsetX = 0.0f;
			float anchorOffsetY = 0.0f;
			switch (anchor)
			{
			case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.UpperLeft: 
				break;
			case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.UpperCenter: 
				anchorOffsetX = -(int)(dimensions.x / 2.0f); break;
			case tk2dBaseSprite.Anchor.LowerRight: case tk2dBaseSprite.Anchor.MiddleRight: case tk2dBaseSprite.Anchor.UpperRight: 
				anchorOffsetX = -(int)(dimensions.x); break;
			}
			switch (anchor)
			{
			case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.LowerRight:
				break;
			case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.MiddleRight:
				anchorOffsetY = -(int)(dimensions.y / 2.0f); break;
			case tk2dBaseSprite.Anchor.UpperLeft: case tk2dBaseSprite.Anchor.UpperCenter: case tk2dBaseSprite.Anchor.UpperRight:
				anchorOffsetY = -(int)dimensions.y; break;
			}
			
			// scale back to sprite coordinates
			// do it after the cast above, as we're trying to align to pixel
			anchorOffsetX *= sx;
			anchorOffsetY *= sy;

			boundsCenter.Set(scale.x * (dimXPixels * 0.5f + anchorOffsetX), scale.y * (dimYPixels * 0.5f + anchorOffsetY), colliderOffsetZ);
			boundsExtents.Set(scale.x * (dimXPixels * 0.5f), scale.y * (dimYPixels * 0.5f), colliderExtentZ);
			
			Vector2[] srcUv = spriteDef.uvs;
			Vector2 duvx = srcUv[1] - srcUv[0];
			Vector2 duvy = srcUv[2] - srcUv[0];
			
			Vector3 origin = new Vector3(anchorOffsetX, anchorOffsetY, 0);
			
			Vector3[] originPoints = new Vector3[4] {
				origin,
				origin + new Vector3(0, borderBottomPixels, 0),
				origin + new Vector3(0, dimYPixels - borderTopPixels, 0),
				origin + new Vector3(0, dimYPixels, 0),
			};
			Vector2[] originUvs = new Vector2[4] {
				srcUv[0],
				srcUv[0] + duvy * borderBottomLeft.y,
				srcUv[0] + duvy * (1 - borderTopRight.y),
				srcUv[0] + duvy,
			};
			
			for (int i = 0; i < 4; ++i)
			{
				pos[offset + i * 4 + 0] = originPoints[i];
				pos[offset + i * 4 + 1] = originPoints[i] + new Vector3(borderLeftPixels, 0, 0);
				pos[offset + i * 4 + 2] = originPoints[i] + new Vector3(dimXPixels - borderRightPixels, 0, 0);
				pos[offset + i * 4 + 3] = originPoints[i] + new Vector3(dimXPixels, 0, 0);
				
				for (int j = 0; j < 4; ++j) {
					pos[offset + i * 4 + j] = Vector3.Scale(pos[offset + i * 4 + j], scale);
				}
				
				uv[offset + i * 4 + 0] = originUvs[i];
				uv[offset + i * 4 + 1] = originUvs[i] + duvx * borderBottomLeft.x;
				uv[offset + i * 4 + 2] = originUvs[i] + duvx * (1 - borderTopRight.x);
				uv[offset + i * 4 + 3] = originUvs[i] + duvx;
			}
		}
	}
	
	public static void SetSlicedSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, bool borderOnly)
	{
		if (spriteDef.positions.Length == 4)
		{
			int[] inds = new int[9 * 6] {
				0, 4, 1, 1, 4, 5,
				1, 5, 2, 2, 5, 6,
				2, 6, 3, 3, 6, 7,
				4, 8, 5, 5, 8, 9,
				6, 10, 7, 7, 10, 11,
				8, 12, 9, 9, 12, 13,
				9, 13, 10, 10, 13, 14,
				10, 14, 11, 11, 14, 15,
				5, 9, 6, 6, 9, 10 // middle bit
			};
			int n = inds.Length;
			if (borderOnly) n -= 6; // take out middle
			for (int i = 0; i < n; ++i) {
				indices[offset + i] = vStart + inds[i];
			}
		}
	}

	// Tiled sprite

	public static void GetTiledSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		int numTilesX = (int)Mathf.Ceil( (dimensions.x * spriteDef.texelSize.x) / spriteDef.untrimmedBoundsData[1].x );
		int numTilesY = (int)Mathf.Ceil( (dimensions.y * spriteDef.texelSize.y) / spriteDef.untrimmedBoundsData[1].y );
		numVertices = numTilesX * numTilesY * 4;
		numIndices = numTilesX * numTilesY * 6;
	}
	
	public static void SetTiledSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;

		int numTilesX = (int)Mathf.Ceil( (dimensions.x * spriteDef.texelSize.x) / spriteDef.untrimmedBoundsData[1].x );
		int numTilesY = (int)Mathf.Ceil( (dimensions.y * spriteDef.texelSize.y) / spriteDef.untrimmedBoundsData[1].y );
		Vector2 totalMeshSize = new Vector2( dimensions.x * spriteDef.texelSize.x * scale.x, dimensions.y * spriteDef.texelSize.y * scale.y );
		
		// Anchor tweaks
		Vector3 anchorOffset = Vector3.zero;
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.UpperLeft: 
			break;
		case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.UpperCenter: 
			anchorOffset.x = -(totalMeshSize.x / 2.0f); break;
		case tk2dBaseSprite.Anchor.LowerRight: case tk2dBaseSprite.Anchor.MiddleRight: case tk2dBaseSprite.Anchor.UpperRight: 
			anchorOffset.x = -(totalMeshSize.x); break;
		}
		switch (anchor)
		{
		case tk2dBaseSprite.Anchor.LowerLeft: case tk2dBaseSprite.Anchor.LowerCenter: case tk2dBaseSprite.Anchor.LowerRight:
			break;
		case tk2dBaseSprite.Anchor.MiddleLeft: case tk2dBaseSprite.Anchor.MiddleCenter: case tk2dBaseSprite.Anchor.MiddleRight:
			anchorOffset.y = -(totalMeshSize.y / 2.0f); break;
		case tk2dBaseSprite.Anchor.UpperLeft: case tk2dBaseSprite.Anchor.UpperCenter: case tk2dBaseSprite.Anchor.UpperRight:
			anchorOffset.y = -totalMeshSize.y; break;
		}
		Vector3 colliderAnchor = anchorOffset;
		anchorOffset -= Vector3.Scale( spriteDef.positions[0], scale );

		boundsCenter.Set(totalMeshSize.x * 0.5f + colliderAnchor.x, totalMeshSize.y * 0.5f + colliderAnchor.y, colliderOffsetZ );
		boundsExtents.Set(totalMeshSize.x * 0.5f, totalMeshSize.y * 0.5f, colliderExtentZ);
		
		int vert = 0;
		Vector3 bounds = Vector3.Scale(  spriteDef.untrimmedBoundsData[1], scale );
		Vector3 baseOffset = Vector3.zero;
		Vector3 p = baseOffset;
		for (int y = 0; y < numTilesY; ++y) {
			p.x = baseOffset.x;
			for (int x = 0; x < numTilesX; ++x) {
				float xClipFrac = 1;
				float yClipFrac = 1;
				if (Mathf.Abs(p.x + bounds.x) > Mathf.Abs(totalMeshSize.x) ) {
					xClipFrac = ((totalMeshSize.x % bounds.x) / bounds.x);
				}
				if (Mathf.Abs(p.y + bounds.y) > Mathf.Abs(totalMeshSize.y)) {
					yClipFrac = ((totalMeshSize.y % bounds.y) / bounds.y);
				}
				
				Vector3 geomOffset = p + anchorOffset;
				
				if (xClipFrac != 1 || yClipFrac != 1) {
					Vector2 fracBottomLeft = Vector2.zero;
					Vector2 fracTopRight = new Vector2(xClipFrac, yClipFrac);
					
					Vector3 bottomLeft = new Vector3(Mathf.Lerp(spriteDef.positions[0].x, spriteDef.positions[3].x, fracBottomLeft.x) * scale.x,
					                                 Mathf.Lerp(spriteDef.positions[0].y, spriteDef.positions[3].y, fracBottomLeft.y) * scale.y,
					                                 spriteDef.positions[0].z * scale.z);
					Vector3 topRight = new Vector3(Mathf.Lerp(spriteDef.positions[0].x, spriteDef.positions[3].x, fracTopRight.x) * scale.x,
					                               Mathf.Lerp(spriteDef.positions[0].y, spriteDef.positions[3].y, fracTopRight.y) * scale.y,
					                               spriteDef.positions[0].z * scale.z);
					
					pos[offset + vert + 0] = geomOffset + new Vector3(bottomLeft.x, bottomLeft.y, bottomLeft.z);
					pos[offset + vert + 1] = geomOffset + new Vector3(topRight.x, bottomLeft.y, bottomLeft.z);
					pos[offset + vert + 2] = geomOffset + new Vector3(bottomLeft.x, topRight.y, bottomLeft.z);
					pos[offset + vert + 3] = geomOffset + new Vector3(topRight.x, topRight.y, bottomLeft.z);
					
					// find the fraction of UV
					// This can be done without a branch, but will end up with loads of unnecessary interpolations
					if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
					{
						Vector2 v0 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracBottomLeft.y),
						                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracBottomLeft.x));
						Vector2 v1 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracTopRight.y),
						                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracTopRight.x));
						
						uv[offset + vert + 0] = new Vector2(v0.x, v0.y);
						uv[offset + vert + 1] = new Vector2(v0.x, v1.y);
						uv[offset + vert + 2] = new Vector2(v1.x, v0.y);
						uv[offset + vert + 3] = new Vector2(v1.x, v1.y);
					}
					else if (spriteDef.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
					{
						Vector2 v0 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracBottomLeft.y),
						                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracBottomLeft.x));
						Vector2 v1 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracTopRight.y),
						                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracTopRight.x));
						
						uv[offset + vert + 0] = new Vector2(v0.x, v0.y);
						uv[offset + vert + 2] = new Vector2(v1.x, v0.y);
						uv[offset + vert + 1] = new Vector2(v0.x, v1.y);
						uv[offset + vert + 3] = new Vector2(v1.x, v1.y);
					}
					else
					{
						Vector2 v0 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracBottomLeft.x),
						                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracBottomLeft.y));
						Vector2 v1 = new Vector2(Mathf.Lerp(spriteDef.uvs[0].x, spriteDef.uvs[3].x, fracTopRight.x),
						                         Mathf.Lerp(spriteDef.uvs[0].y, spriteDef.uvs[3].y, fracTopRight.y));
						
						uv[offset + vert + 0] = new Vector2(v0.x, v0.y);
						uv[offset + vert + 1] = new Vector2(v1.x, v0.y);
						uv[offset + vert + 2] = new Vector2(v0.x, v1.y);
						uv[offset + vert + 3] = new Vector2(v1.x, v1.y);
					}
				}
				else {
					pos[offset + vert + 0] = geomOffset + Vector3.Scale( spriteDef.positions[0], scale );
					pos[offset + vert + 1] = geomOffset + Vector3.Scale( spriteDef.positions[1], scale );
					pos[offset + vert + 2] = geomOffset + Vector3.Scale( spriteDef.positions[2], scale );
					pos[offset + vert + 3] = geomOffset + Vector3.Scale( spriteDef.positions[3], scale );
					uv[offset + vert + 0] = spriteDef.uvs[0];
					uv[offset + vert + 1] = spriteDef.uvs[1];
					uv[offset + vert + 2] = spriteDef.uvs[2];
					uv[offset + vert + 3] = spriteDef.uvs[3];
				}
				
				vert += 4;
				p.x += bounds.x;
			}
			p.y += bounds.y;
		}
	}
	
	public static void SetTiledSpriteIndices(int[] indices, int offset, int vStart, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		int numVertices;
		int numIndices;
		GetTiledSpriteGeomDesc(out numVertices, out numIndices, spriteDef, dimensions);
		
		int baseIndex = 0;
		for (int i = 0; i < numIndices; i += 6) {
			indices[offset + i + 0] = vStart + spriteDef.indices[0] + baseIndex;
			indices[offset + i + 1] = vStart + spriteDef.indices[1] + baseIndex;
			indices[offset + i + 2] = vStart + spriteDef.indices[2] + baseIndex;
			indices[offset + i + 3] = vStart + spriteDef.indices[3] + baseIndex;
			indices[offset + i + 4] = vStart + spriteDef.indices[4] + baseIndex;
			indices[offset + i + 5] = vStart + spriteDef.indices[5] + baseIndex;
			baseIndex += 4;
		}
	}

	// Composite mesh for batched mesh. Box, and SpriteDefinition meshes.
	// Does transform here too.
	static readonly int[] boxIndicesBack = { 0, 1, 2, 2, 1, 3, 6, 5, 4, 7, 5, 6, 3, 7, 6, 2, 3, 6, 4, 5, 1, 4, 1, 0, 6, 4, 0, 6, 0, 2, 1, 7, 3, 5, 7, 1 };
	static readonly int[] boxIndicesFwd  = { 2, 1, 0, 3, 1, 2, 4, 5, 6, 6, 5, 7, 6, 7, 3, 6, 3, 2, 1, 5, 4, 0, 1, 4, 0, 4, 6, 2, 0, 6, 3, 7, 1, 1, 7, 5 };
	static readonly Vector3[] boxUnitVertices = new Vector3[] { new Vector3(-1,-1,-1), new Vector3(-1,-1,1), new Vector3(1,-1,-1), new Vector3(1,-1,1),
	new Vector3(-1,1,-1), new Vector3(-1,1,1), new Vector3(1,1,-1), new Vector3(1,1,1) };
	static Matrix4x4 boxScaleMatrix = Matrix4x4.identity;

	public static void SetBoxMeshData(Vector3[] pos, int[] indices, int posOffset, int indicesOffset, int vStart, Vector3 origin, Vector3 extents, Matrix4x4 mat, Vector3 baseScale)
	{
		boxScaleMatrix.m03 = origin.x * baseScale.x;
		boxScaleMatrix.m13 = origin.y * baseScale.y;
		boxScaleMatrix.m23 = origin.z * baseScale.z;
		boxScaleMatrix.m00 = extents.x * baseScale.x;
		boxScaleMatrix.m11 = extents.y * baseScale.y;
		boxScaleMatrix.m22 = extents.z * baseScale.z;
		Matrix4x4 boxFinalMatrix = mat * boxScaleMatrix;
		for (int j = 0; j < 8; ++j) {
			pos[posOffset + j] = boxFinalMatrix.MultiplyPoint(boxUnitVertices[j]);
		}
		
		float scl = mat.m00 * mat.m11 * mat.m22 * baseScale.x * baseScale.y * baseScale.z;
		int[] srcIndices = ( scl >= 0 ) ? boxIndicesFwd : boxIndicesBack;
		
		for (int i = 0; i < srcIndices.Length; ++i)
			indices[indicesOffset + i] = vStart + srcIndices[i];
	}
	
	public static void SetSpriteDefinitionMeshData(Vector3[] pos, int[] indices, int posOffset, int indicesOffset, int vStart, tk2dSpriteDefinition spriteDef, Matrix4x4 mat, Vector3 baseScale)
	{
		for (int i = 0; i < spriteDef.colliderVertices.Length; ++i)
		{
			Vector3 p = Vector3.Scale (spriteDef.colliderVertices[i], baseScale);
			p = mat.MultiplyPoint (p);
			pos[posOffset + i] = p;
		}
		
		float scl = mat.m00 * mat.m11 * mat.m22;
		int[] srcIndices = (scl >= 0)?spriteDef.colliderIndicesFwd:spriteDef.colliderIndicesBack;
		
		for (int i = 0; i < srcIndices.Length; ++i)
			indices[indicesOffset + i] = vStart + srcIndices[i];
	}
}