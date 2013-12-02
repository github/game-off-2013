//#define ASTAR_NoTagPenalty
using UnityEngine;
using Pathfinding;

namespace Pathfinding {
	public class MeshNode : Node {
		//Vertices
		public int v1;
		public int v2;
		public int v3;
		
		public int GetVertexIndex (int i) {
			if (i == 0) {
				return v1;
			} else if (i == 1) {
				return v2;
			} else if (i == 2) {
				return v3;
			} else {
				throw new System.ArgumentOutOfRangeException ("A MeshNode only contains 3 vertices");
			}
		}
		
		public int this[int i]
	    {
	        get
	        {
	            return GetVertexIndex (i);
	        }
	    }
	    
	    public Vector3 ClosestPoint (Vector3 p, Int3[] vertices) {
	    	return Polygon.ClosestPointOnTriangle ((Vector3)vertices[v1],(Vector3)vertices[v2],(Vector3)vertices[v3],p);
	    }
	}
}