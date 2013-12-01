using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR || !UNITY_FLASH

namespace tk2dRuntime.TileMap
{
	public static class ColliderBuilder
	{
		public static void Build(tk2dTileMap tileMap, bool forceBuild)
		{
			bool incremental = !forceBuild;
			int numLayers = tileMap.Layers.Length;
			for (int layerId = 0; layerId < numLayers; ++layerId)
			{
				var layer = tileMap.Layers[layerId];
				if (layer.IsEmpty || !tileMap.data.Layers[layerId].generateCollider)
					continue;
	
				for (int cellY = 0; cellY < layer.numRows; ++cellY)
				{
					int baseY = cellY * layer.divY;
					for (int cellX = 0; cellX < layer.numColumns; ++cellX)
					{
						int baseX = cellX * layer.divX;
						var chunk = layer.GetChunk(cellX, cellY);
						
						if (incremental && !chunk.Dirty)
							continue;
						
						if (chunk.IsEmpty)
							continue;
						
						BuildForChunk(tileMap, chunk, baseX, baseY);

						PhysicMaterial material = tileMap.data.Layers[layerId].physicMaterial;
						if (material != null)
							chunk.meshCollider.sharedMaterial = material;
					}
				}
			}
		}
		
		public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY)
		{
			// Build local mesh
			Vector3[] localMeshVertices = new Vector3[0];
			int[] localMeshIndices = new int[0];
			BuildLocalMeshForChunk(tileMap, chunk, baseX, baseY, ref localMeshVertices, ref localMeshIndices);
			
			// only process when there are more than two triangles
			// avoids a lot of branches later
			if (localMeshIndices.Length > 6) 
			{
				// Remove duplicate verts
				localMeshVertices = WeldVertices(localMeshVertices, ref localMeshIndices);
				
				// Remove duplicate and back-to-back faces
				// Removes inside faces
				localMeshIndices = RemoveDuplicateFaces(localMeshIndices);
	
				// Merge coplanar faces
				// Optimize (remove unused vertices, reindex)
			}
	
			if (localMeshVertices.Length > 0)
			{
				if (chunk.colliderMesh != null)
				{
					GameObject.DestroyImmediate(chunk.colliderMesh);
					chunk.colliderMesh = null;
				}
				
				if (chunk.meshCollider == null)
				{
					chunk.meshCollider = chunk.gameObject.GetComponent<MeshCollider>();
					if (chunk.meshCollider == null)
						chunk.meshCollider = chunk.gameObject.AddComponent<MeshCollider>();
				}
				
				chunk.colliderMesh = new Mesh();
				chunk.colliderMesh.vertices = localMeshVertices;
				chunk.colliderMesh.triangles = localMeshIndices;

				chunk.colliderMesh.RecalculateBounds();
				
				chunk.meshCollider.sharedMesh = chunk.colliderMesh;
			}
			else
			{
				chunk.DestroyColliderData(tileMap);
			}
		}		
		
		// Builds an unoptimized mesh for this chunk
		static void BuildLocalMeshForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, ref Vector3[] vertices, ref int[] indices)
		{
			List<Vector3> vertexList = new List<Vector3>();
			List<int> indexList = new List<int>();
			
			int spriteCount = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
			Vector3 tileSize = tileMap.data.tileSize;
			
			var tilePrefabs = tileMap.data.tilePrefabs;
			
			float xOffsetMult = 0.0f, yOffsetMult = 0.0f;
			tileMap.data.GetTileOffset(out xOffsetMult, out yOffsetMult);

			var chunkData = chunk.spriteIds;
			for (int y = 0; y < tileMap.partitionSizeY; ++y)
			{
				float xOffset = ((baseY + y) & 1) * xOffsetMult;
				for (int x = 0; x < tileMap.partitionSizeX; ++x)
				{
					int spriteId = chunkData[y * tileMap.partitionSizeX + x];
					int spriteIdx = BuilderUtil.GetTileFromRawTile(spriteId);
					Vector3 currentPos = new Vector3(tileSize.x * (x + xOffset), tileSize.y * y, 0);
	
					if (spriteIdx < 0 || spriteIdx >= spriteCount) 
						continue;
					
					if (tilePrefabs[spriteIdx])
						continue;

					bool flipH = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.FlipX);
					bool flipV = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.FlipY);
					bool rot90 = BuilderUtil.IsRawTileFlagSet(spriteId, tk2dTileFlags.Rot90);

					bool reverseIndices = false;
					if (flipH) reverseIndices = !reverseIndices;
					if (flipV) reverseIndices = !reverseIndices;

					var spriteData = tileMap.SpriteCollectionInst.spriteDefinitions[spriteIdx];
					int baseVertexIndex = vertexList.Count;
					
					if (spriteData.colliderType == tk2dSpriteDefinition.ColliderType.Box)
					{
						Vector3 origin = spriteData.colliderVertices[0];
						Vector3 extents = spriteData.colliderVertices[1];
						Vector3 min = origin - extents;
						Vector3 max = origin + extents;

						Vector3[] pos = new Vector3[8];
						pos[0] = new Vector3(min.x, min.y, min.z);
						pos[1] = new Vector3(min.x, min.y, max.z);
						pos[2] = new Vector3(max.x, min.y, min.z);
						pos[3] = new Vector3(max.x, min.y, max.z);
						pos[4] = new Vector3(min.x, max.y, min.z);
						pos[5] = new Vector3(min.x, max.y, max.z);
						pos[6] = new Vector3(max.x, max.y, min.z);
						pos[7] = new Vector3(max.x, max.y, max.z);
						for (int i = 0; i < 8; ++i) {
							Vector3 flippedPos = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, spriteData, pos[i], flipH, flipV, rot90);
							vertexList.Add (flippedPos + currentPos);
						}
	
	//						int[] indicesBack = { 0, 1, 2, 2, 1, 3, 6, 5, 4, 7, 5, 6, 3, 7, 6, 2, 3, 6, 4, 5, 1, 4, 1, 0 };
						int[] indicesFwd = { 2, 1, 0, 3, 1, 2, 4, 5, 6, 6, 5, 7, 6, 7, 3, 6, 3, 2, 1, 5, 4, 0, 1, 4 };
						
						var srcIndices = indicesFwd;
						for (int i = 0; i < srcIndices.Length; ++i)
						{
							int j = reverseIndices ? (srcIndices.Length - 1 - i) : i;
							indexList.Add(baseVertexIndex + srcIndices[j]);
						}
					}
					else if (spriteData.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
					{
						for (int i = 0; i < spriteData.colliderVertices.Length; ++i)
						{
							Vector3 flippedPos = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, spriteData, spriteData.colliderVertices[i], flipH, flipV, rot90);
							vertexList.Add(flippedPos + currentPos);
						}
						
						var srcIndices = spriteData.colliderIndicesFwd;
						for (int i = 0; i < srcIndices.Length; ++i)
						{
							int j = reverseIndices ? (srcIndices.Length - 1 - i) : i;
							indexList.Add(baseVertexIndex + srcIndices[j]);
						}
					}
				}
			}
			
			vertices = vertexList.ToArray();
			indices = indexList.ToArray();
		}
		
		static int CompareWeldVertices(Vector3 a, Vector3 b)
		{
			// Compare one component at a time, using epsilon
			float epsilon = 0.01f;
			float dx = a.x - b.x;
			if (Mathf.Abs(dx) > epsilon) return (int)Mathf.Sign(dx);
			float dy = a.y - b.y;
			if (Mathf.Abs(dy) > epsilon) return (int)Mathf.Sign(dy);
			float dz = a.z - b.z;
			if (Mathf.Abs(dz) > epsilon) return (int)Mathf.Sign(dz);
			return 0;
		}
		
		static Vector3[] WeldVertices(Vector3[] vertices, ref int[] indices)
		{
			// Sort by x, y and z
			// Adjacent values could be the same after this sort
			int[] sortIndex = new int[vertices.Length];
			for (int i = 0; i < vertices.Length; ++i)
			{
				sortIndex[i] = i;
			}
			System.Array.Sort<int>(sortIndex, (a, b) => CompareWeldVertices(vertices[a], vertices[b]) );
			
			// Step through the list, comparing current with previous value
			// If they are the same, use the current index
			// Otherwise add a new vertex to the vertex list, and use this index
			// Welding all similar vertices
			List<Vector3> newVertices = new List<Vector3>();
			int[] vertexRemap = new int[vertices.Length];
			// prime first value
			Vector3 previousValue = vertices[sortIndex[0]];
			newVertices.Add(previousValue);
			vertexRemap[sortIndex[0]] = newVertices.Count - 1;
			for (int i = 1; i < sortIndex.Length; ++i)
			{
				Vector3 v = vertices[sortIndex[i]];
				if (CompareWeldVertices(v, previousValue) != 0)
				{
					// add new vertex
					previousValue = v;
					newVertices.Add(previousValue);
					vertexRemap[sortIndex[i]] = newVertices.Count - 1;
				}
				vertexRemap[sortIndex[i]] = newVertices.Count - 1;
			}
			
			// remap indices
			for (int i = 0; i < indices.Length; ++i)
			{
				indices[i] = vertexRemap[indices[i]];
			}
			
			return newVertices.ToArray();
		}
		
		static int CompareDuplicateFaces(int[] indices, int face0index, int face1index)
		{
			for (int i = 0; i < 3; ++i)
			{
				int d = indices[face0index + i] - indices[face1index + i];
				if (d != 0) return d;
			}
			return 0;
		}
		
		static int[] RemoveDuplicateFaces(int[] indices)
		{
			// Create an ascending sorted list of face indices
			// If 2 sets of indices are identical, then the faces share the same vertices, and either
			// is a duplicate, or back-to-back
			int[] sortedFaceIndices = new int[indices.Length];
			for (int i = 0; i < indices.Length; i += 3)
			{
				int[] faceIndices = { indices[i], indices[i + 1], indices[i + 2] };
				System.Array.Sort(faceIndices);
				sortedFaceIndices[i] = faceIndices[0];
				sortedFaceIndices[i+1] = faceIndices[1];
				sortedFaceIndices[i+2] = faceIndices[2];
			}
			
			// Sort by faces
			int[] sortIndex = new int[indices.Length / 3];
			for (int i = 0; i < indices.Length; i += 3)
			{
				sortIndex[i / 3] = i;
			}
			System.Array.Sort<int>(sortIndex, (a, b) => CompareDuplicateFaces(sortedFaceIndices, a, b));
			
			List<int> newIndices = new List<int>();
			for (int i = 0; i < sortIndex.Length; ++i)
			{
				if (i != sortIndex.Length - 1 && CompareDuplicateFaces(sortedFaceIndices, sortIndex[i], sortIndex[i+1]) == 0)
				{
					// skip both faces
					// this will fail in the case where there are 3 coplanar faces
					// but that is probably likely user error / intentional
					i++;
					continue;
				}
				
				for (int j = 0; j < 3; ++j)
					newIndices.Add(indices[sortIndex[i] + j]);
			}
			
			return newIndices.ToArray();
		}
	}
}

#endif