using UnityEngine;
using System.Collections.Generic;

// Hideous überclass to work around Unity not supporting inheritence
[System.Serializable]
public class tk2dBatchedSprite
{
	public enum Type {
		EmptyGameObject,
		Sprite,
		TiledSprite,
		SlicedSprite,
		ClippedSprite,
		TextMesh
	}

	[System.Flags] 
	public enum Flags {
		None = 0,
		Sprite_CreateBoxCollider = 1,
		SlicedSprite_BorderOnly = 2,
	}
	
	public Type type = Type.Sprite;
	public string name = ""; // for editing
	public int parentId = -1;
	public int spriteId = 0;
	public int xRefId = -1; // index into cross referenced array. what this is depends on type.
	public tk2dSpriteCollectionData spriteCollection = null;
	public Quaternion rotation = Quaternion.identity;
	public Vector3 position = Vector3.zero;
	public Vector3 localScale = Vector3.one;
	public Color color = Color.white;
	public Vector3 baseScale = Vector3.one; // sprite/textMesh scale
	public int renderLayer = 0;
	
	[SerializeField]
	Vector2 internalData0; // Used for clipped region or sliced border
	[SerializeField]
	Vector2 internalData1; // Used for clipped region or sliced border
	[SerializeField]
	Vector2 internalData2; // Used for dimensions
	[SerializeField]
	Vector2 colliderData = new Vector2(0, 1); // collider offset z, collider extent z in x and y respectively
	[SerializeField]
	string formattedText = ""; // Formatted text cached for text mesh
	
	[SerializeField]
	Flags flags = Flags.None;
	public tk2dBaseSprite.Anchor anchor = tk2dBaseSprite.Anchor.LowerLeft;

	// Used to create batched mesh
	public Matrix4x4 relativeMatrix = Matrix4x4.identity;

	public float BoxColliderOffsetZ {
		get { return colliderData.x; }
		set { colliderData.x = value; }
	}
	public float BoxColliderExtentZ {
		get { return colliderData.y; }
		set { colliderData.y = value; }
	}
	public string FormattedText {
		get {return formattedText;}
		set {formattedText = value;}
	}
	public Vector2 ClippedSpriteRegionBottomLeft {
		get { return internalData0; }
		set { internalData0 = value; }
	}
	public Vector2 ClippedSpriteRegionTopRight {
		get { return internalData1; }
		set { internalData1 = value; }
	}
	public Vector2 SlicedSpriteBorderBottomLeft {
		get { return internalData0; }
		set { internalData0 = value; }
	}
	public Vector2 SlicedSpriteBorderTopRight {
		get { return internalData1; }
		set { internalData1 = value; }
	}
	public Vector2 Dimensions {
		get { return internalData2; }
		set { internalData2 = value; }
	}

	public bool IsDrawn { get { return type != Type.EmptyGameObject; } }
	public bool CheckFlag(Flags mask) { return (flags & mask) != Flags.None; }
	public void SetFlag(Flags mask, bool value) { if (value) flags |= mask;	else flags &= ~mask; }

	// Bounds - not serialized, but retrieved in BuildRenderMesh (used in BuildPhysicsMesh)
	Vector3 cachedBoundsCenter = Vector3.zero;
	Vector3 cachedBoundsExtents = Vector3.zero;
	public Vector3 CachedBoundsCenter {
		get {return cachedBoundsCenter;}
		set {cachedBoundsCenter = value;}
	}
	public Vector3 CachedBoundsExtents {
		get {return cachedBoundsExtents;}
		set {cachedBoundsExtents = value;}
	}
	
	public tk2dSpriteDefinition GetSpriteDefinition() {
		if (spriteCollection != null && spriteId != -1)
		{
			return spriteCollection.inst.spriteDefinitions[spriteId];
		}
		else {
			return null;
		}
	}

	public tk2dBatchedSprite()
	{
		parentId = -1;
	}
}

[AddComponentMenu("2D Toolkit/Sprite/tk2dStaticSpriteBatcher")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Static sprite batcher builds a collection of sprites, textmeshes into one
/// static mesh for better performance.
/// </summary>
public class tk2dStaticSpriteBatcher : MonoBehaviour, tk2dRuntime.ISpriteCollectionForceBuild
{
	public static int CURRENT_VERSION = 3;
	
	public int version = 0;
	/// <summary>
	/// The list of batched sprites. Fill this and call Build to build your mesh.
	/// </summary>
	public tk2dBatchedSprite[] batchedSprites = null;
	public tk2dTextMeshData[] allTextMeshData = null;
	public tk2dSpriteCollectionData spriteCollection = null;

	// Flags
	public enum Flags {
		None = 0,
		GenerateCollider = 1,
		FlattenDepth = 2,
		SortToCamera = 4,
	}
	
	// default to keep backwards compatibility
	[SerializeField]
	Flags flags = Flags.GenerateCollider;

	public bool CheckFlag(Flags mask) { return (flags & mask) != Flags.None; }
	public void SetFlag(Flags mask, bool value) { 
		if (CheckFlag(mask) != value) {
			if (value) {
				flags |= mask; 
			}
			else {
				flags &= ~mask; 
			}
			Build();
		}
	}

	Mesh mesh = null;
	Mesh colliderMesh = null;
	
	[SerializeField] Vector3 _scale = new Vector3(1.0f, 1.0f, 1.0f);
	
#if UNITY_EDITOR
	// This is not exposed to game, as the cost of rebuilding this data is very high
	public Vector3 scale
	{
		get { UpgradeData(); return _scale; }
		set
		{
			bool needBuild = _scale != value;
			_scale = value;
			if (needBuild)
				Build();
		}
	}
#endif
	
	void Awake()
	{
		Build();
	}
	
	// Sanitize data, returns true if needs rebuild
	bool UpgradeData()
	{
		if (version == CURRENT_VERSION) {
			return false;
		}
		
		if (_scale == Vector3.zero) {
			_scale = Vector3.one;
		}
		
		if (version < 2)
		{
			if (batchedSprites != null)
			{
				// Parented to this object
				foreach (var sprite in batchedSprites)
					sprite.parentId = -1;
			}
		}

		if (version < 3)
		{
			if (batchedSprites != null)
			{
				foreach (var sprite in batchedSprites)
				{
					if (sprite.spriteId == -1)
					{
						sprite.type = tk2dBatchedSprite.Type.EmptyGameObject;
					}
					else {
						sprite.type = tk2dBatchedSprite.Type.Sprite;
						if (sprite.spriteCollection == null) { 
							sprite.spriteCollection = spriteCollection;
						}
					}
				}

				UpdateMatrices();
			}

			spriteCollection = null;
		}
		
		version = CURRENT_VERSION;

#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(this);
#endif
		
		return true;
	}
	
	protected void OnDestroy()
	{
		if (mesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(mesh);
#else
			Destroy(mesh);
#endif
		}
		
		if (colliderMesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(colliderMesh);
#else
			Destroy(colliderMesh);
#endif
		}
	}

	/// <summary>
	/// Update matrices, if the sprite batcher has been built using .position, etc.
	/// It is far more efficient to simply set the matrices when building at runtime
	/// so do that if possible.
	/// </summary>
	public void UpdateMatrices() {
		bool hasParentIds = false;
		foreach (var sprite in batchedSprites)
		{
			if (sprite.parentId != -1) {
				hasParentIds = true;
				break;
			}
		}

		if (hasParentIds) {
			// Reconstruct matrices from TRS, respecting hierarchy
			Matrix4x4 tmpMatrix = new Matrix4x4();
			List<tk2dBatchedSprite> parentSortedSprites = new List<tk2dBatchedSprite>( batchedSprites );
			parentSortedSprites.Sort((a, b) => a.parentId.CompareTo(b.parentId) );
			foreach (tk2dBatchedSprite sprite in parentSortedSprites) {
				tmpMatrix.SetTRS( sprite.position, sprite.rotation, sprite.localScale );
				sprite.relativeMatrix = ((sprite.parentId == -1) ? Matrix4x4.identity : batchedSprites[ sprite.parentId ].relativeMatrix) * tmpMatrix;
			}
		}
		else {
			foreach (tk2dBatchedSprite sprite in batchedSprites) {
				sprite.relativeMatrix.SetTRS( sprite.position, sprite.rotation, sprite.localScale );
			}
		}		
	}

	/// <summary>
	/// Build a static sprite batcher's geometry and collider
	/// </summary>
	public void Build()
	{
		UpgradeData();

		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			GetComponent<MeshFilter>().mesh = mesh;
		}
		else
		{
			// this happens when the sprite rebuilds
			mesh.Clear();
		}
		
		if (colliderMesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(colliderMesh);
#else
			Destroy(colliderMesh);
#endif
			colliderMesh = null;
		}
		
		if (batchedSprites == null || batchedSprites.Length == 0)
		{
		}
		else
		{
			SortBatchedSprites();
			BuildRenderMesh();
			BuildPhysicsMesh();
		}
	}
	
	void SortBatchedSprites()
	{
		List<tk2dBatchedSprite> solidBatches = new List<tk2dBatchedSprite>();
		List<tk2dBatchedSprite> otherBatches = new List<tk2dBatchedSprite>();
		List<tk2dBatchedSprite> undrawnBatches = new List<tk2dBatchedSprite>();
		foreach (tk2dBatchedSprite batchedSprite in batchedSprites)
		{
			if (!batchedSprite.IsDrawn)
			{
				undrawnBatches.Add(batchedSprite);
				continue;				
			}

			Material material = GetMaterial(batchedSprite);
			if (material != null) {
				if (material.renderQueue == 2000) {
					solidBatches.Add(batchedSprite);
				}
				else {
					otherBatches.Add(batchedSprite);
				}
			}
			else
			{
				solidBatches.Add(batchedSprite);
			}
		}
		
		List<tk2dBatchedSprite> allBatches = new List<tk2dBatchedSprite>(solidBatches.Count + otherBatches.Count + undrawnBatches.Count);
		allBatches.AddRange(solidBatches);
		allBatches.AddRange(otherBatches);
		allBatches.AddRange(undrawnBatches);
		
		// Re-index parents
		Dictionary<tk2dBatchedSprite, int> lookup = new Dictionary<tk2dBatchedSprite, int>();
		int index = 0;
		foreach (var v in allBatches)
			lookup[v] = index++;
		
		foreach (var v in allBatches)
		{
			if (v.parentId == -1)
				continue;
			v.parentId = lookup[ batchedSprites[v.parentId] ];
		}
		
		batchedSprites = allBatches.ToArray();
	}

	Material GetMaterial(tk2dBatchedSprite bs) {
		switch (bs.type) {
			case tk2dBatchedSprite.Type.EmptyGameObject: return null;
			case tk2dBatchedSprite.Type.TextMesh: return allTextMeshData[bs.xRefId].font.materialInst;
			default: return bs.GetSpriteDefinition().materialInst;
		}
	}

	void BuildRenderMesh()
	{
		List<Material> materials = new List<Material>();
		List<List<int>> indices = new List<List<int>>();
		
		bool needNormals = false;
		bool needTangents = false;
		bool needUV2 = false;
		bool flattenDepth = CheckFlag(Flags.FlattenDepth);

		foreach (var bs in batchedSprites)
		{
			var spriteDef = bs.GetSpriteDefinition();
			if (spriteDef != null)
			{
				needNormals |= (spriteDef.normals != null && spriteDef.normals.Length > 0); 
				needTangents |= (spriteDef.tangents != null && spriteDef.tangents.Length > 0);
			}
			if (bs.type == tk2dBatchedSprite.Type.TextMesh)
			{
				tk2dTextMeshData textMeshData = allTextMeshData[bs.xRefId];
				if ((textMeshData.font != null) && textMeshData.font.inst.textureGradients)
				{
					needUV2 = true;
				}
			}
		}

		// just helpful to have these here, stop code being more messy
		List<int> bsNVerts = new List<int>();
		List<int> bsNInds = new List<int>();
		
		int numVertices = 0;
		foreach (var bs in batchedSprites) 
		{
			if (!bs.IsDrawn) // when the first non-drawn child is found, it signals the end of the drawn list
				break;

			var spriteDef = bs.GetSpriteDefinition();
			int nVerts = 0;
			int nInds = 0;
			switch (bs.type)
			{
			case tk2dBatchedSprite.Type.EmptyGameObject:
				break;
			case tk2dBatchedSprite.Type.Sprite:
				if (spriteDef != null) tk2dSpriteGeomGen.GetSpriteGeomDesc(out nVerts, out nInds, spriteDef);
				break;
			case tk2dBatchedSprite.Type.TiledSprite:
				if (spriteDef != null) tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out nVerts, out nInds, spriteDef, bs.Dimensions);
				break;
			case tk2dBatchedSprite.Type.SlicedSprite:
				if (spriteDef != null) tk2dSpriteGeomGen.GetSlicedSpriteGeomDesc(out nVerts, out nInds, spriteDef, bs.CheckFlag(tk2dBatchedSprite.Flags.SlicedSprite_BorderOnly));
				break;
			case tk2dBatchedSprite.Type.ClippedSprite:
				if (spriteDef != null) tk2dSpriteGeomGen.GetClippedSpriteGeomDesc(out nVerts, out nInds, spriteDef);
				break;
			case tk2dBatchedSprite.Type.TextMesh:
				{
					tk2dTextMeshData textMeshData = allTextMeshData[bs.xRefId];
					tk2dTextGeomGen.GetTextMeshGeomDesc(out nVerts, out nInds, tk2dTextGeomGen.Data(textMeshData, textMeshData.font.inst, bs.FormattedText));
					break;
				}
			}
			numVertices += nVerts;

			bsNVerts.Add(nVerts);
			bsNInds.Add(nInds);
		}
		
		Vector3[] meshNormals = needNormals?new Vector3[numVertices]:null;
		Vector4[] meshTangents = needTangents?new Vector4[numVertices]:null;
		Vector3[] meshVertices = new Vector3[numVertices];
		Color32[] meshColors = new Color32[numVertices];
		Vector2[] meshUvs = new Vector2[numVertices];
		Vector2[] meshUv2s = needUV2 ? new Vector2[numVertices] : null;
		
		int currVertex = 0;

		Material currentMaterial = null;
		List<int> currentIndices = null;

		Matrix4x4 scaleMatrix = Matrix4x4.identity;
		scaleMatrix.m00 = _scale.x;
		scaleMatrix.m11 = _scale.y;
		scaleMatrix.m22 = _scale.z;

		int bsIndex = 0;
		foreach (var bs in batchedSprites)
		{
			if (!bs.IsDrawn) // when the first non-drawn child is found, it signals the end of the drawn list
				break;

			if (bs.type == tk2dBatchedSprite.Type.EmptyGameObject)
			{
				++bsIndex; // watch out for this
				continue;
			}

			var spriteDef = bs.GetSpriteDefinition();
			int nVerts = bsNVerts[bsIndex];
			int nInds = bsNInds[bsIndex];

			Material material = GetMaterial(bs);

			// should have a material at this point
			if (material != currentMaterial)
			{
				if (currentMaterial != null)
				{
					materials.Add(currentMaterial);
					indices.Add(currentIndices);
				}
				
				currentMaterial = material;
				currentIndices = new List<int>();
			}

			Vector3[] posData = new Vector3[nVerts];
			Vector2[] uvData = new Vector2[nVerts];
			Vector2[] uv2Data = needUV2 ? new Vector2[nVerts] : null;
			Color32[] colorData = new Color32[nVerts];
			Vector3[] normalData = needNormals ? new Vector3[nVerts] : null;
			Vector4[] tangentData = needTangents ? new Vector4[nVerts] : null;
			int[] indData = new int[nInds];

			Vector3 boundsCenter = Vector3.zero;
			Vector3 boundsExtents = Vector3.zero;

			switch (bs.type)
			{
			case tk2dBatchedSprite.Type.EmptyGameObject:
				break;
			case tk2dBatchedSprite.Type.Sprite:
				if (spriteDef != null) {
					tk2dSpriteGeomGen.SetSpriteGeom(posData, uvData, normalData, tangentData, 0, spriteDef, Vector3.one);
					tk2dSpriteGeomGen.SetSpriteIndices(indData, 0, currVertex, spriteDef);
				}
				break;
			case tk2dBatchedSprite.Type.TiledSprite:
				if (spriteDef != null) {
					tk2dSpriteGeomGen.SetTiledSpriteGeom(posData, uvData, 0, out boundsCenter, out boundsExtents, spriteDef, Vector3.one, bs.Dimensions, bs.anchor, bs.BoxColliderOffsetZ, bs.BoxColliderExtentZ);
					tk2dSpriteGeomGen.SetTiledSpriteIndices(indData, 0, currVertex, spriteDef, bs.Dimensions);
				}
				break;
			case tk2dBatchedSprite.Type.SlicedSprite:
				if (spriteDef != null) {
					tk2dSpriteGeomGen.SetSlicedSpriteGeom(posData, uvData, 0, out boundsCenter, out boundsExtents, spriteDef, Vector3.one, bs.Dimensions, bs.SlicedSpriteBorderBottomLeft, bs.SlicedSpriteBorderTopRight, bs.anchor, bs.BoxColliderOffsetZ, bs.BoxColliderExtentZ);
					tk2dSpriteGeomGen.SetSlicedSpriteIndices(indData, 0, currVertex, spriteDef, bs.CheckFlag(tk2dBatchedSprite.Flags.SlicedSprite_BorderOnly));
				}
				break;
			case tk2dBatchedSprite.Type.ClippedSprite:
				if (spriteDef != null) {
					tk2dSpriteGeomGen.SetClippedSpriteGeom(posData, uvData, 0, out boundsCenter, out boundsExtents, spriteDef, Vector3.one, bs.ClippedSpriteRegionBottomLeft, bs.ClippedSpriteRegionTopRight, bs.BoxColliderOffsetZ, bs.BoxColliderExtentZ);
					tk2dSpriteGeomGen.SetClippedSpriteIndices(indData, 0, currVertex, spriteDef);
				}
				break;
			case tk2dBatchedSprite.Type.TextMesh:
				{
					tk2dTextMeshData textMeshData = allTextMeshData[bs.xRefId];
					var geomData = tk2dTextGeomGen.Data(textMeshData, textMeshData.font.inst, bs.FormattedText);
					int target = tk2dTextGeomGen.SetTextMeshGeom(posData, uvData, uv2Data, colorData, 0, geomData);
					if (!geomData.fontInst.isPacked) {
						Color32 topColor = textMeshData.color;
						Color32 bottomColor = textMeshData.useGradient ? textMeshData.color2 : textMeshData.color;
						for (int i = 0; i < colorData.Length; ++i) {
							Color32 c = ((i % 4) < 2) ? topColor : bottomColor;
							byte red = (byte)(((int)colorData[i].r * (int)c.r) / 255);
							byte green = (byte)(((int)colorData[i].g * (int)c.g) / 255);
							byte blue = (byte)(((int)colorData[i].b * (int)c.b) / 255);
							byte alpha = (byte)(((int)colorData[i].a * (int)c.a) / 255);
							if (geomData.fontInst.premultipliedAlpha) {
								red = (byte)(((int)red * (int)alpha) / 255);
								green = (byte)(((int)green * (int)alpha) / 255);
								blue = (byte)(((int)blue * (int)alpha) / 255);
							}
							colorData[i] = new Color32(red, green, blue, alpha);
						}
					}
					tk2dTextGeomGen.SetTextMeshIndices(indData, 0, currVertex, geomData, target);
					break;
				}
			}
			
			bs.CachedBoundsCenter = boundsCenter;
			bs.CachedBoundsExtents = boundsExtents;

			if (nVerts > 0 && bs.type != tk2dBatchedSprite.Type.TextMesh)
			{
				bool premulAlpha = (bs.spriteCollection != null) ? bs.spriteCollection.premultipliedAlpha : false;
				tk2dSpriteGeomGen.SetSpriteColors(colorData, 0, nVerts, bs.color, premulAlpha);
			}

			Matrix4x4 mat = scaleMatrix * bs.relativeMatrix;
			for (int i = 0; i < nVerts; ++i)
			{
				Vector3 pos = Vector3.Scale(posData[i], bs.baseScale);
				pos = mat.MultiplyPoint(pos);
				if (flattenDepth) {
					pos.z = 0;
				}
				
				meshVertices[currVertex + i] = pos;

				meshUvs[currVertex + i] = uvData[i];
				if (needUV2) meshUv2s[currVertex + i] = uv2Data[i];
				meshColors[currVertex + i] = colorData[i];

				if (needNormals)
				{
					meshNormals[currVertex + i] = bs.rotation * normalData[i];
				}
				if (needTangents)
				{
					Vector3 tang = new Vector3(tangentData[i].x, tangentData[i].y, tangentData[i].z);
					tang = bs.rotation * tang;
					meshTangents[currVertex + i] = new Vector4(tang.x, tang.y, tang.z, tangentData[i].w);
				}
			}

			currentIndices.AddRange (indData);

			currVertex += nVerts;

			++bsIndex;
		}
		
		if (currentIndices != null)
		{
			materials.Add(currentMaterial);
			indices.Add(currentIndices);
		}
		
		if (mesh)
		{
			mesh.vertices = meshVertices;
	        mesh.uv = meshUvs;
			if (needUV2)
				mesh.uv2 = meshUv2s;
	        mesh.colors32 = meshColors;
			if (needNormals)
				mesh.normals = meshNormals;
			if (needTangents)
				mesh.tangents = meshTangents;
			
			mesh.subMeshCount = indices.Count;
			for (int i = 0; i < indices.Count; ++i)
				mesh.SetTriangles(indices[i].ToArray(), i);
			
			mesh.RecalculateBounds();
		}
		
		renderer.sharedMaterials = materials.ToArray();
	}
	
	void BuildPhysicsMesh()
	{
		MeshCollider meshCollider = GetComponent<MeshCollider>();
		if (meshCollider != null)
		{
			if (collider != meshCollider) {
				// Already has a collider
				return;
			}

			if (!CheckFlag(Flags.GenerateCollider)) {
#if UNITY_EDITOR
				DestroyImmediate(meshCollider);
#else
				Destroy(meshCollider);
#endif
			}
		}

		if (!CheckFlag(Flags.GenerateCollider)) {
			return;
		}

		bool flattenDepth = CheckFlag(Flags.FlattenDepth);
		int numIndices = 0;
		int numVertices = 0;
		
		// first pass, count required vertices and indices
		foreach (var bs in batchedSprites) 
		{
			if (!bs.IsDrawn) // when the first non-drawn child is found, it signals the end of the drawn list
				break;

			var spriteDef = bs.GetSpriteDefinition();

			bool buildSpriteDefinitionMesh = false;
			bool buildBox = false;
			switch (bs.type)
			{
			case tk2dBatchedSprite.Type.Sprite:
				if (spriteDef != null && spriteDef.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
				{
					buildSpriteDefinitionMesh = true;
				}
				if (spriteDef != null && spriteDef.colliderType == tk2dSpriteDefinition.ColliderType.Box)
				{
					buildBox = true;
				}
				break;
			case tk2dBatchedSprite.Type.ClippedSprite:
			case tk2dBatchedSprite.Type.SlicedSprite:
			case tk2dBatchedSprite.Type.TiledSprite:
				buildBox = bs.CheckFlag(tk2dBatchedSprite.Flags.Sprite_CreateBoxCollider);
				break;
			case tk2dBatchedSprite.Type.TextMesh:
				//...
				break;
			}

			// might want to return these counts from SpriteGeomGen? (tidier...?)
			if (buildSpriteDefinitionMesh)
			{
				numIndices += spriteDef.colliderIndicesFwd.Length;
				numVertices += spriteDef.colliderVertices.Length;
			}
			else if (buildBox)
			{
				numIndices += 6 * 6;
				numVertices += 8;
			}
		}
		
		if (numIndices == 0)
		{
			if (colliderMesh)
			{
#if UNITY_EDTIOR
				DestroyImmediate(colliderMesh);
#else
				Destroy(colliderMesh);
#endif
			}
			
			return;
		}
		
		if (meshCollider == null)
		{
			meshCollider = gameObject.AddComponent<MeshCollider>();
		}
	
		if (colliderMesh == null)
		{
			colliderMesh = new Mesh();
			colliderMesh.hideFlags = HideFlags.DontSave;
		}
		else
		{
			colliderMesh.Clear();
		}
		
		// second pass, build composite mesh
		int currVertex = 0;
		Vector3[] vertices = new Vector3[numVertices];
		int currIndex = 0;
		int[] indices = new int[numIndices];

		Matrix4x4 scaleMatrix = Matrix4x4.identity;
		scaleMatrix.m00 = _scale.x;
		scaleMatrix.m11 = _scale.y;
		scaleMatrix.m22 = _scale.z;

		foreach (var bs in batchedSprites) 
		{
			if (!bs.IsDrawn) // when the first non-drawn child is found, it signals the end of the drawn list
				break;

			var spriteDef = bs.GetSpriteDefinition();

			bool buildSpriteDefinitionMesh = false;
			bool buildBox = false;
			Vector3 boxOrigin = Vector3.zero;
			Vector3 boxExtents = Vector3.zero;
			switch (bs.type)
			{
			case tk2dBatchedSprite.Type.Sprite:
				if (spriteDef != null && spriteDef.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
				{
					buildSpriteDefinitionMesh = true;
				}
				if (spriteDef != null && spriteDef.colliderType == tk2dSpriteDefinition.ColliderType.Box)
				{
					buildBox = true;
					boxOrigin = spriteDef.colliderVertices[0];
					boxExtents = spriteDef.colliderVertices[1];
				}
				break;
			case tk2dBatchedSprite.Type.ClippedSprite:
			case tk2dBatchedSprite.Type.SlicedSprite:
			case tk2dBatchedSprite.Type.TiledSprite:
				buildBox = bs.CheckFlag(tk2dBatchedSprite.Flags.Sprite_CreateBoxCollider);
				if (buildBox)
				{
					boxOrigin = bs.CachedBoundsCenter;
					boxExtents = bs.CachedBoundsExtents;
				}
				break;
			case tk2dBatchedSprite.Type.TextMesh:
				break;
			}

			Matrix4x4 mat = scaleMatrix * bs.relativeMatrix;
			if (flattenDepth) {
				mat.m23 = 0;
			}
			if (buildSpriteDefinitionMesh)
			{
				tk2dSpriteGeomGen.SetSpriteDefinitionMeshData(vertices, indices, currVertex, currIndex, currVertex, spriteDef, mat, bs.baseScale);
				currVertex += spriteDef.colliderVertices.Length;
				currIndex += spriteDef.colliderIndicesFwd.Length;
			}
			else if (buildBox)
			{
				tk2dSpriteGeomGen.SetBoxMeshData(vertices, indices, currVertex, currIndex, currVertex, boxOrigin, boxExtents, mat, bs.baseScale);
				currVertex += 8;
				currIndex += 6 * 6;
			}
		}
		
		colliderMesh.vertices = vertices;
		colliderMesh.triangles = indices;
		
		meshCollider.sharedMesh = colliderMesh;
	}
	
	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return this.spriteCollection == spriteCollection;	
	}
	
	public void ForceBuild()
	{
		Build();
	}
}


