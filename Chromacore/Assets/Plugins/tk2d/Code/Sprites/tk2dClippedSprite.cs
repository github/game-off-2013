using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dClippedSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation that clips the sprite using normalized clip coordinates.
/// </summary>
public class tk2dClippedSprite : tk2dBaseSprite
{
	Mesh mesh;
	Vector2[] meshUvs;
	Vector3[] meshVertices;
	Color32[] meshColors;
	int[] meshIndices;
	
	public Vector2 _clipBottomLeft = new Vector2(0, 0);
	public Vector2 _clipTopRight = new Vector2(1, 1);

	// Temp cached variables
	Rect _clipRect = new Rect(0, 0, 0, 0);

	/// <summary>
	/// Sets the clip rectangle
	/// 0, 0, 1, 1 = display the entire sprite
	/// </summary>
	public Rect ClipRect {
		get {
			_clipRect.Set( _clipBottomLeft.x, _clipBottomLeft.y, _clipTopRight.x - _clipBottomLeft.x, _clipTopRight.y - _clipBottomLeft.y );
			return _clipRect;
		}
		set {
			Vector2 v = new Vector2( value.x, value.y );
			clipBottomLeft = v;
			v.x += value.width;
			v.y += value.height;
			clipTopRight = v;
		}
	}
	
	
	/// <summary>
	/// Sets the bottom left clip area.
	/// 0, 0 = display full sprite
	/// </summary>
	public Vector2 clipBottomLeft
	{
		get { return _clipBottomLeft; }
		set 
		{ 
			if (value != _clipBottomLeft) 
			{
				_clipBottomLeft = new Vector2(value.x, value.y);
				Build();
				UpdateCollider();
			}
		}
	}

	/// <summary>
	/// Sets the top right clip area
	/// 1, 1 = display full sprite
	/// </summary>
	public Vector2 clipTopRight
	{
		get { return _clipTopRight; }
		set 
		{ 
			if (value != _clipTopRight) 
			{
				_clipTopRight = new Vector2(value.x, value.y);
				Build();
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
		if (CurrentSprite.positions.Length == 4)
		{
			tk2dSpriteGeomGen.SetSpriteColors (dest, 0, 4, _color, collectionInst.premultipliedAlpha);
		}
	}	
	
	// Calculated center and extents
	Vector3 boundsCenter = Vector3.zero, boundsExtents = Vector3.zero;

	protected void SetGeometry(Vector3[] vertices, Vector2[] uvs)
	{
		var sprite = CurrentSprite;

		float colliderOffsetZ = ( boxCollider != null ) ? ( boxCollider.center.z ) : 0.0f;
		float colliderExtentZ = ( boxCollider != null ) ? ( boxCollider.size.z * 0.5f ) : 0.5f;
		tk2dSpriteGeomGen.SetClippedSpriteGeom( meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, sprite, _scale, _clipBottomLeft, _clipTopRight, colliderOffsetZ, colliderExtentZ );
		
		// Only do this when there are exactly 4 polys to a sprite (i.e. the sprite isn't diced, and isnt a more complex mesh)
		if (sprite.positions.Length != 4)
		{
			// Only supports normal sprites
			for (int i = 0; i < vertices.Length; ++i)
				vertices[i] = Vector3.zero;
		}
	}

	public override void Build()
	{
		meshUvs = new Vector2[4];
		meshVertices = new Vector3[4];
		meshColors = new Color32[4];
		
		SetGeometry(meshVertices, meshUvs);
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

		int[] indices = new int[6];
		tk2dSpriteGeomGen.SetClippedSpriteIndices(indices, 0, 0, CurrentSprite);
		mesh.triangles = indices;

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
		if (meshVertices == null || meshVertices.Length == 0) {
			Build();
		}
		else {
			SetGeometry(meshVertices, meshUvs);
			mesh.vertices = meshVertices;
			mesh.uv = meshUvs;
			mesh.RecalculateBounds();
			mesh.bounds = AdjustedMeshBounds( mesh.bounds, renderLayer );
		}
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
		return 4;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax) {
		var sprite = CurrentSprite;
		Vector3 oldMin = Vector3.Scale(sprite.untrimmedBoundsData[0] - 0.5f * sprite.untrimmedBoundsData[1], _scale);
		Vector3 oldSize = Vector3.Scale(sprite.untrimmedBoundsData[1], _scale);
		Vector3 newScale = oldSize + dMax - dMin;
		newScale.x /= sprite.untrimmedBoundsData[1].x;
		newScale.y /= sprite.untrimmedBoundsData[1].y;
		Vector3 scaledMin = new Vector3(Mathf.Approximately(_scale.x, 0) ? 0 : (oldMin.x * newScale.x / _scale.x),
			Mathf.Approximately(_scale.y, 0) ? 0 : (oldMin.y * newScale.y / _scale.y));
		Vector3 offset = oldMin + dMin - scaledMin;
		offset.z = 0;
		transform.position = transform.TransformPoint(offset);
		scale = new Vector3(newScale.x, newScale.y, _scale.z);
	}
}
