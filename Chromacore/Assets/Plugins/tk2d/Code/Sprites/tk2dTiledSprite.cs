using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dTiledSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation that tiles a sprite to fill given dimensions.
/// </summary>
public class tk2dTiledSprite : tk2dBaseSprite
{
	Mesh mesh;
	Vector2[] meshUvs;
	Vector3[] meshVertices;
	Color32[] meshColors;
	int[] meshIndices;
	
	[SerializeField]
	Vector2 _dimensions = new Vector2(50.0f, 50.0f);
	[SerializeField]
	Anchor _anchor = Anchor.LowerLeft;
	
	/// <summary>
	/// Gets or sets the dimensions.
	/// </summary>
	/// <value>
	/// Use this to change the dimensions of the sliced sprite in pixel units
	/// </value>
	public Vector2 dimensions
	{ 
		get { return _dimensions; } 
		set
		{
			if (value != _dimensions)
			{
				_dimensions = value;
				UpdateVertices();
#if UNITY_EDITOR
				EditMode__CreateCollider();
#endif
				UpdateCollider();
			}
		}
	}
	
	/// <summary>
	/// The anchor position for this tiled sprite
	/// </summary>
	public Anchor anchor
	{
		get { return _anchor; }
		set
		{
			if (value != _anchor)
			{
				_anchor = value;
				UpdateVertices();
#if UNITY_EDITOR
				EditMode__CreateCollider();
#endif
				UpdateCollider();
			}
		}
	}

	[SerializeField]
	protected bool _createBoxCollider = false;

	/// <summary>
	/// Create a trimmed box collider for this sprite
	/// </summary>
	public bool CreateBoxCollider {
		get { return _createBoxCollider; }
		set {
			if (_createBoxCollider != value) {
				_createBoxCollider = value;
				UpdateCollider();
			}
		}
	}
	
	new void Awake()
	{
		base.Awake();
		
		// Create mesh, independently to everything else
		mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		GetComponent<MeshFilter>().mesh = mesh;
		
		// This will not be set when instantiating in code
		// In that case, Build will need to be called
		if (Collection)
		{
			// reset spriteId if outside bounds
			// this is when the sprite collection data is corrupt
			if (_spriteId < 0 || _spriteId >= Collection.Count)
				_spriteId = 0;
			
			Build();
			
			if (boxCollider == null)
				boxCollider = GetComponent<BoxCollider>();
		}
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
	}
	
	new protected void SetColors(Color32[] dest)
	{
		int numVertices;
		int numIndices;
		tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out numVertices, out numIndices, CurrentSprite, dimensions);
		tk2dSpriteGeomGen.SetSpriteColors (dest, 0, numVertices, _color, collectionInst.premultipliedAlpha);
	}
	
	// Calculated center and extents
	Vector3 boundsCenter = Vector3.zero, boundsExtents = Vector3.zero;


	
	public override void Build()
	{
		var spriteDef = CurrentSprite;
		int numVertices;
		int numIndices;
		tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out numVertices, out numIndices, spriteDef, dimensions);

		if (meshUvs == null || meshUvs.Length != numVertices) {
			meshUvs = new Vector2[numVertices];
			meshVertices = new Vector3[numVertices];
			meshColors = new Color32[numVertices];
		}
		if (meshIndices == null || meshIndices.Length != numIndices) {
			meshIndices = new int[numIndices];
		}

		float colliderOffsetZ = ( boxCollider != null ) ? ( boxCollider.center.z ) : 0.0f;
		float colliderExtentZ = ( boxCollider != null ) ? ( boxCollider.size.z * 0.5f ) : 0.5f;
		tk2dSpriteGeomGen.SetTiledSpriteGeom(meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, spriteDef, _scale, dimensions, anchor, colliderOffsetZ, colliderExtentZ);
		tk2dSpriteGeomGen.SetTiledSpriteIndices(meshIndices, 0, 0, spriteDef, dimensions);
		
		SetColors(meshColors);
		
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = meshVertices;
		mesh.colors32 = meshColors;
		mesh.uv = meshUvs;
		mesh.triangles = meshIndices;
		mesh.RecalculateBounds();
		mesh.bounds = AdjustedMeshBounds( mesh.bounds, renderLayer );
		
		GetComponent<MeshFilter>().mesh = mesh;
		
		UpdateCollider();
		UpdateMaterial();
	}
	
	protected override void UpdateGeometry() { UpdateGeometryImpl(); }
	protected override void UpdateColors() { UpdateColorsImpl(); }
	protected override void UpdateVertices() { UpdateGeometryImpl(); }
	
	
	protected void UpdateColorsImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (meshColors == null || meshColors.Length == 0)
			return;
#endif
		if (meshColors == null || meshColors.Length == 0) {
			Build();
		}
		else {
			SetColors(meshColors);
			mesh.colors32 = meshColors;
		}
	}

	protected void UpdateGeometryImpl()
	{
#if UNITY_EDITOR
		// This can happen with prefabs in the inspector
		if (mesh == null)
			return;
#endif
		Build();
	}
	
#region Collider
	protected override void UpdateCollider()
	{
		if (CreateBoxCollider) {
			if (boxCollider == null) {
				boxCollider = GetComponent<BoxCollider>();
				if (boxCollider == null) {
					boxCollider = gameObject.AddComponent<BoxCollider>();
				}
			}
			boxCollider.size = 2 * boundsExtents;
			boxCollider.center = boundsCenter;
		} else {
#if UNITY_EDITOR
			boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null) {
				DestroyImmediate(boxCollider);
			}
#else
			if (boxCollider != null) {
				Destroy(boxCollider);
			}
#endif
		}
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (mesh != null) {
			Bounds b = mesh.bounds;
			Gizmos.color = Color.clear;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(b.center, b.extents * 2);
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}
	}
#endif

	protected override void CreateCollider() {
		UpdateCollider();
	}

#if UNITY_EDITOR
	public override void EditMode__CreateCollider() {
		UpdateCollider();
	}
#endif
#endregion	
	
	protected override void UpdateMaterial()
	{
		if (renderer.sharedMaterial != collectionInst.spriteDefinitions[spriteId].materialInst)
			renderer.material = collectionInst.spriteDefinitions[spriteId].materialInst;
	}
	
	protected override int GetCurrentVertexCount()
	{
#if UNITY_EDITOR
		if (meshVertices == null)
			return 0;
#endif
		return 16;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax) {
		var sprite = CurrentSprite;
		Vector3 oldSize = new Vector3(_dimensions.x * sprite.texelSize.x * _scale.x, _dimensions.y * sprite.texelSize.y * _scale.y);
		Vector3 oldMin = Vector3.zero;
		switch (_anchor) {
			case Anchor.LowerLeft: oldMin.Set(0,0,0); break;
			case Anchor.LowerCenter: oldMin.Set(0.5f,0,0); break;
			case Anchor.LowerRight: oldMin.Set(1,0,0); break;
			case Anchor.MiddleLeft: oldMin.Set(0,0.5f,0); break;
			case Anchor.MiddleCenter: oldMin.Set(0.5f,0.5f,0); break;
			case Anchor.MiddleRight: oldMin.Set(1,0.5f,0); break;
			case Anchor.UpperLeft: oldMin.Set(0,1,0); break;
			case Anchor.UpperCenter: oldMin.Set(0.5f,1,0); break;
			case Anchor.UpperRight: oldMin.Set(1,1,0); break;
		}
		oldMin = Vector3.Scale(oldMin, oldSize) * -1;
		Vector3 newDimensions = oldSize + dMax - dMin;
		newDimensions.x /= sprite.texelSize.x * _scale.x;
		newDimensions.y /= sprite.texelSize.y * _scale.y;
		Vector3 scaledMin = new Vector3(Mathf.Approximately(_dimensions.x, 0) ? 0 : (oldMin.x * newDimensions.x / _dimensions.x),
			Mathf.Approximately(_dimensions.y, 0) ? 0 : (oldMin.y * newDimensions.y / _dimensions.y));
		Vector3 offset = oldMin + dMin - scaledMin;
		offset.z = 0;
		transform.position = transform.TransformPoint(offset);
		dimensions = new Vector2(newDimensions.x, newDimensions.y);
	}
}
