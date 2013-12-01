using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSprite")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
/// <summary>
/// Sprite implementation which maintains its own Unity Mesh. Leverages dynamic batching.
/// </summary>
public class tk2dSprite : tk2dBaseSprite
{
	Mesh mesh;
	Vector3[] meshVertices;
	Vector3[] meshNormals = null;
	Vector4[] meshTangents = null;
	Color32[] meshColors;
	
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
		
		if (meshColliderMesh)
		{
#if UNITY_EDITOR
			DestroyImmediate(meshColliderMesh);
#else
			Destroy(meshColliderMesh);
#endif
		}
	}
	
	public override void Build()
	{
		var sprite = collectionInst.spriteDefinitions[spriteId];

		meshVertices = new Vector3[sprite.positions.Length];
        meshColors = new Color32[sprite.positions.Length];
		
		meshNormals = new Vector3[0];
		meshTangents = new Vector4[0];
		
		if (sprite.normals != null && sprite.normals.Length > 0)
		{
			meshNormals = new Vector3[sprite.normals.Length];
		}
		if (sprite.tangents != null && sprite.tangents.Length > 0)
		{
			meshTangents = new Vector4[sprite.tangents.Length];
		}
		
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);
		
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			GetComponent<MeshFilter>().mesh = mesh;
		}
		
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors32 = meshColors;
		mesh.uv = sprite.uvs;
		mesh.triangles = sprite.indices;
		mesh.bounds = AdjustedMeshBounds( GetBounds(), renderLayer );
		
		UpdateMaterial();
		CreateCollider();
	}
	
	/// <summary>
	/// Adds a tk2dSprite as a component to the gameObject passed in, setting up necessary parameters and building geometry.
	/// Convenience alias of tk2dBaseSprite.AddComponent<tk2dSprite>(...).
	/// </summary>
	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteId);
	}
	
	/// <summary>
	/// Adds a tk2dSprite as a component to the gameObject passed in, setting up necessary parameters and building geometry.
	/// Convenience alias of tk2dBaseSprite.AddComponent<tk2dSprite>(...).
	/// </summary>
	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, string spriteName)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteName);
	}
	
	/// <summary>
	/// Create a sprite (and gameObject) displaying the region of the texture specified.
	/// Use <see cref="tk2dSpriteCollectionData.CreateFromTexture"/> if you need to create a sprite collection
	/// with multiple sprites.
	/// Convenience alias of tk2dBaseSprite.CreateFromTexture<tk2dSprite>(...)
	/// </summary>
	public static GameObject CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor)
	{
		return tk2dBaseSprite.CreateFromTexture<tk2dSprite>(texture, size, region, anchor);
	}

	protected override void UpdateGeometry() { UpdateGeometryImpl(); }
	protected override void UpdateColors() { UpdateColorsImpl(); }
	protected override void UpdateVertices() { UpdateVerticesImpl(); }
	
	
	protected void UpdateColorsImpl()
	{
		// This can happen with prefabs in the inspector
		if (mesh == null || meshColors == null || meshColors.Length == 0)
			return;

		SetColors(meshColors);
		mesh.colors32 = meshColors;
	}
	
	protected void UpdateVerticesImpl()
	{
		var sprite = collectionInst.spriteDefinitions[spriteId];
		
		// This can happen with prefabs in the inspector
		if (mesh == null || meshVertices == null || meshVertices.Length == 0)
			return;
		
		// Clear out normals and tangents when switching from a sprite with them to one without
		if (sprite.normals.Length != meshNormals.Length)
		{
			meshNormals = (sprite.normals != null && sprite.normals.Length > 0)?(new Vector3[sprite.normals.Length]):(new Vector3[0]);
		}
		if (sprite.tangents.Length != meshTangents.Length)
		{
			meshTangents = (sprite.tangents != null && sprite.tangents.Length > 0)?(new Vector4[sprite.tangents.Length]):(new Vector4[0]);
		}
		
		SetPositions(meshVertices, meshNormals, meshTangents);
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.uv = sprite.uvs;
		mesh.bounds = AdjustedMeshBounds( GetBounds(), renderLayer );
	}

	protected void UpdateGeometryImpl()
	{
		// This can happen with prefabs in the inspector
		if (mesh == null)
			return;
		
		var sprite = collectionInst.spriteDefinitions[spriteId];
		if (meshVertices == null || meshVertices.Length != sprite.positions.Length)
		{
			meshVertices = new Vector3[sprite.positions.Length];
			meshNormals = (sprite.normals != null && sprite.normals.Length > 0)?(new Vector3[sprite.normals.Length]):(new Vector3[0]);
			meshTangents = (sprite.tangents != null && sprite.tangents.Length > 0)?(new Vector4[sprite.tangents.Length]):(new Vector4[0]);
			meshColors = new Color32[sprite.positions.Length];
		}
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);

		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors32 = meshColors;
		mesh.uv = sprite.uvs;
		mesh.bounds = AdjustedMeshBounds( GetBounds(), renderLayer );
        mesh.triangles = sprite.indices;
	}
	
	protected override void UpdateMaterial()
	{
		if (renderer.sharedMaterial != collectionInst.spriteDefinitions[spriteId].materialInst)
			renderer.material = collectionInst.spriteDefinitions[spriteId].materialInst;
	}
	
	protected override int GetCurrentVertexCount()
	{
		if (meshVertices == null)
			return 0;
		// Really nasty bug here found by Andrew Welch.
		return meshVertices.Length;
	}

#if UNITY_EDITOR
	void OnDrawGizmos() {
		if (collectionInst != null && spriteId >= 0 && spriteId < collectionInst.Count) {
			var sprite = collectionInst.spriteDefinitions[spriteId];
			Gizmos.color = Color.clear;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.DrawCube(Vector3.Scale(sprite.untrimmedBoundsData[0], _scale), Vector3.Scale(sprite.untrimmedBoundsData[1], _scale));
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = Color.white;
		}
	}
#endif
	
	public override void ForceBuild()
	{
		base.ForceBuild();
		GetComponent<MeshFilter>().mesh = mesh;
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
