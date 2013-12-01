using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Pathfinding.Util;
using Pathfinding.Serialization.JsonFx;

namespace Pathfinding {
	
	/** A class for holding a user placed connection */
	public class UserConnection {
		
		public Vector3 p1;
		public Vector3 p2;
		public ConnectionType type;
		
		//Connection
		[JsonName("doOverCost")]
		public bool doOverrideCost = false;
		[JsonName("overCost")]
		public int overrideCost = 0;
		
		public bool oneWay = false;
		public bool enable = true;
		public float width = 0;
		
		//Modify Node
		[JsonName("doOverWalkable")]
		public bool doOverrideWalkability = true;
		
		[JsonName("doOverCost")]
		public bool doOverridePenalty = false;
		[JsonName("overPenalty")]
		public uint overridePenalty = 0;
		
	}
	
	[System.Serializable]
	/** Stores editor colors */
	public class AstarColor {
		
		public Color _NodeConnection;
		public Color _UnwalkableNode;
		public Color _BoundsHandles;
	
		public Color _ConnectionLowLerp;
		public Color _ConnectionHighLerp;
		
		public Color _MeshEdgeColor;
		public Color _MeshColor;
		
		/** Holds user set area colors.
		 * Use GetAreaColor to get an area color */
		public Color[] _AreaColors;
		
		public static Color NodeConnection = new Color (1,1,1,0.9F);
		public static Color UnwalkableNode = new Color (1,0,0,0.5F);
		public static Color BoundsHandles = new Color (0.29F,0.454F,0.741F,0.9F);
		
		public static Color ConnectionLowLerp = new Color (0,1,0,0.5F);
		public static Color ConnectionHighLerp = new Color (1,0,0,0.5F);
		
		public static Color MeshEdgeColor = new Color (0,0,0,0.5F);
		public static Color MeshColor = new Color (0,0,0,0.5F);
		
		/** Holds user set area colors.
		 * Use GetAreaColor to get an area color */
		private static Color[] AreaColors;
		
		/** Returns an color for an area, uses both user set ones and calculated.
		 * If the user has set a color for the area, it is used, but otherwise the color is calculated using Mathfx.IntToColor
		 * \see #AreaColors */
		public static Color GetAreaColor (int area) {
			if (AreaColors == null || area >= AreaColors.Length) {
				return Mathfx.IntToColor (area,1F);
			}
			return AreaColors[area];
		}
		
		/** Pushes all local variables out to static ones */
		public void OnEnable () {
			
			NodeConnection = _NodeConnection;
			UnwalkableNode = _UnwalkableNode;
			BoundsHandles = _BoundsHandles;
			
			ConnectionLowLerp = _ConnectionLowLerp;
			ConnectionHighLerp = _ConnectionHighLerp;
			
			MeshEdgeColor = _MeshEdgeColor;
			MeshColor = _MeshColor;
			
			AreaColors = _AreaColors;
		}
		
		public AstarColor () {
			
			_NodeConnection = new Color (1,1,1,0.9F);
			_UnwalkableNode = new Color (1,0,0,0.5F);
			_BoundsHandles = new Color (0.29F,0.454F,0.741F,0.9F);
			
			_ConnectionLowLerp = new Color (0,1,0,0.5F);
			_ConnectionHighLerp = new Color (1,0,0,0.5F);
			
			_MeshEdgeColor = new Color (0,0,0,0.5F);
			_MeshColor = new Color (0.125F, 0.686F, 0, 0.19F);
		}
		
		//new Color (0.909F,0.937F,0.243F,0.6F);
	}
	
	
	/** Returned by graph ray- or linecasts containing info about the hit. This will only be set up if something was hit. */
	public struct GraphHitInfo {
		public Vector3 origin;
		public Vector3 point;
		public Node node;
		public Vector3 tangentOrigin;
		public Vector3 tangent;
		public bool success;
		
		public float distance {
			get {
				return (point-origin).magnitude;
			}
		}
		
		public GraphHitInfo (Vector3 point) {
			success = false;
			tangentOrigin  = Vector3.zero;
			origin = Vector3.zero;
			this.point = point;
			node = null;
			tangent = Vector3.zero;
			//this.distance = distance;
		}
	}
	
	/** Nearest node constraint. Constrains which nodes will be returned by the GetNearest function */
	public class NNConstraint {
		
		/** Graphs treated as valid to search on.
		 * This is a bitmask meaning that bit 0 specifies whether or not the first graph in the graphs list should be able to be included in the search,
		 * bit 1 specifies whether or not the second graph should be included and so on.
		 * \code
		 * //Enables the first and third graphs to be included, but not the rest
		 * myNNConstraint.graphMask = (1 << 0) | (1 << 2);
		 * \endcode
		 * \note This does only affect which nodes are returned from a GetNearest call, if an invalid graph is linked to from a valid graph, it might be searched anyway.
		 * 
		 * \see AstarPath.GetNearest */
		public int graphMask = -1;
		
		public bool constrainArea = false; /**< Only treat nodes in the area #area as suitable. Does not affect anything if #area is less than 0 (zero) */ 
		public int area = -1; /**< Area ID to constrain to. Will not affect anything if less than 0 (zero) or if #constrainArea is false */
		
		public bool constrainWalkability = true; /**< Only treat nodes with the walkable flag set to the same as #walkable as suitable */
		public bool walkable = true; /**< What must the walkable flag on a node be for it to be suitable. Does not affect anything if #constrainWalkability if false */
		
		public bool constrainTags = true; /**< Sets if tags should be constrained */
		public int tags = -1; /**< Nodes which have any of these tags set are suitable. This is a bitmask, i.e bit 0 indicates that tag 0 is good, bit 3 indicates tag 3 is good etc. */
		
		/** Constrain distance to node.
		 * Uses distance from AstarPath.maxNearestNodeDistance.
		 * If this is false, it will completely ignore the distance limit.
		 * \note This value is not used in this class, it is used by the AstarPath.GetNearest function.
		 */
		public bool constrainDistance = true;
		
		/** Returns whether or not the graph conforms to this NNConstraint's rules.
		  */
		public virtual bool SuitableGraph (int graphIndex, NavGraph graph) {
			return ((graphMask >> graphIndex) & 1) != 0;
		}
		
		/** Returns whether or not the node conforms to this NNConstraint's rules */
		public virtual bool Suitable (Node node) {
			if (constrainWalkability && node.walkable != walkable) return false;
			
			if (constrainArea && area >= 0 && node.area != area) return false;
			
			if (constrainTags && (tags >> node.tags & 0x1) == 0) return false;
			
			return true;
		}
		
		/** The default NNConstraint.
		  * Equivalent to new NNConstraint ().
		  * This NNConstraint has settings which works for most, it only finds walkable nodes
		  * and it constrains distance set by A* Inspector -> Settings -> Max Nearest Node Distance */
		public static NNConstraint Default {
			get {
				return new NNConstraint ();
			}
		}
		
		/** Returns a constraint which will not filter the results */
		public static NNConstraint None {
			get {
				NNConstraint n = new NNConstraint ();
				n.constrainWalkability = false;
				n.constrainArea = false;
				n.constrainTags = false;
				n.constrainDistance = false;
				n.graphMask = -1;
				return n;
			}
		}
		
		/** Default constructor. Equals to the property #Default */
		public NNConstraint () {
		}
	}
	
	/** A special NNConstraint which can use different logic for the start node and end node in a path.
	 * A PathNNConstraint can be assigned to the Path.nnConstraint field, the path will first search for the start node, then it will call #SetStart and proceed with searching for the end node (nodes in the case of a MultiTargetPath).\n
	 * The default PathNNConstraint will constrain the end point to lie inside the same area as the start point.
	 */
	public class PathNNConstraint : NNConstraint {
		
		public static new PathNNConstraint Default {
			get {
				PathNNConstraint n = new PathNNConstraint ();
				n.constrainArea = true;
				return n;
			}
		}
		
		/** Called after the start node has been found. This is used to get different search logic for the start and end nodes in a path */
		public virtual void SetStart (Node node) {
			if (node != null) {
				area = node.area;
			} else {
				constrainArea = false;
			}
		}
	}
	
	public struct NNInfo {
		/** Closest node found.
		 * This node is not necessarily accepted by any NNConstraint passed.
		 * \see constrainedNode
		 */
		public Node node;
		
		/** Optional to be filled in.
		 * If the search will be able to find the constrained node without any extra effort it can fill it in. */
		public Node constrainedNode;
		
		/** The position clamped to the closest point on the #node.
		 */
		public Vector3 clampedPosition;
		/** Clamped position for the optional constrainedNode */
		public Vector3 constClampedPosition;
		
		public NNInfo (Node node) {
			this.node = node;
			constrainedNode = null;
			constClampedPosition = Vector3.zero;
			
			if (node != null) {
				clampedPosition = (Vector3)node.position;
			} else {
				clampedPosition = Vector3.zero;
			}
		}
		
		/** Sets the constrained node */
		public void SetConstrained (Node constrainedNode, Vector3 clampedPosition) {
			this.constrainedNode = constrainedNode;
			constClampedPosition = clampedPosition;
		}
		
		/** Updates #clampedPosition and #constClampedPosition from node positions */
		public void UpdateInfo () {
			if (node != null) {
				clampedPosition = (Vector3)node.position;
			} else {
				clampedPosition = Vector3.zero;
			}
			
			if (constrainedNode != null) {
				constClampedPosition = (Vector3)constrainedNode.position;
			} else {
				constClampedPosition = Vector3.zero;
			}
		}
		
		public static explicit operator Vector3 (NNInfo ob) {
			return ob.clampedPosition;
		}
		
		public static explicit operator Node (NNInfo ob) {
			return ob.node;
		}
		
		public static explicit operator NNInfo (Node ob) {
			return new NNInfo (ob);
		}
	}
	
	/** Progress info for e.g a progressbar.
	 * Used by the scan functions in the project
	 * \see AstarPath.ScanLoop
	 */
	public struct Progress {
		public float progress;
		public string description;
		
		public Progress (float p, string d) {
			progress = p;
			description = d;
		}
	}
	
	/** Graphs which can be updated during runtime */
	public interface IUpdatableGraph {
		
		/** Updates an area using the specified GraphUpdateObject.
		 * 
		 * Notes to implementators.
		 * This function should (in order):
		 * -# Call o.WillUpdateNode on the GUO for every node it will update, it is important that this is called BEFORE any changes are made to the nodes.
		 * -# Update walkabilty using special settings such as the usePhysics flag used with the GridGraph.
		 * -# Call Apply on the GUO for every node which should be updated with the GUO.
		 * -# Update eventual connectivity info if appropriate (GridGraphs updates connectivity, but most other graphs don't since then the connectivity cannot be recovered later).
		 */
		void UpdateArea (GraphUpdateObject o);
	}
	
	[System.Serializable]
	/** Holds a tagmask.
	 * This is used to store which tags to change and what to set them to in a Pathfinding.GraphUpdateObject.
	 * All variables are bitmasks.\n
	 * I wanted to make it a struct, but due to technical limitations when working with Unity's GenericMenu, I couldn't.
	 * So be wary of this when passing it as it will be passed by reference, not by value as e.g LayerMask.
	 */
	public class TagMask {
		public int tagsChange;
		public int tagsSet;
		
		public TagMask () {}
		
		public TagMask (int change, int set) {
			tagsChange = change;
			tagsSet = set;
		}
		
		public void SetValues (System.Object boxedTagMask) {
			TagMask o = (TagMask)boxedTagMask;
			tagsChange = o.tagsChange;
			tagsSet = o.tagsSet;
			//Debug.Log ("Set tags to "+tagsChange +" "+tagsSet+" "+someVar);
		}
		
		public override string ToString () {
			return ""+System.Convert.ToString (tagsChange,2)+"\n"+System.Convert.ToString (tagsSet,2);
		}
	}
	
	/** Represents a collection of settings used to update nodes in a specific area of a graph.
	 * \see AstarPath.UpdateGraphs
	 */
	public class GraphUpdateObject {
		
		/** The bounds to update nodes within */
		public Bounds bounds;
		
		/** Performance boost.
		 * This controlls if a flood fill will be carried out after this GUO has been applied.\n
		 * If you are sure that a GUO will not modify walkability or connections. You can set this to false.
		 * For example when only updating penalty values it can save processing power when setting this to false. Especially on large graphs.
		 * \note If you set this to false, even though it does change e.g walkability, it can lead to paths returning that they failed even though there is a path,
		 * or the try to search the whole graph for a path even though there is none, and will in the processes use wast amounts of processing power.
		 *
		 * If using the basic GraphUpdateObject (not a derived class), a quick way to check if it is going to need a flood fill is to check if #modifyWalkability is true or #updatePhysics is true.
		 *
		 */
		public bool requiresFloodFill = true;
		
		/** Use physics checks to update nodes.
		 * When updating a grid graph and this is true, the nodes' position and walkability will be updated using physics checks
		 * with settings from "Collision Testing" and "Height Testing".
		 * Also when updating a PointGraph, setting this to true will make it re-evaluate all connections in the graph which passes through the #bounds.
		 * This has no effect when updating GridGraphs if #modifyWalkability is turned on */
		public bool updatePhysics = true;
		
		/** When #updatePhysics is true, GridGraphs will normally reset penalties, with this option you can override it.
		 * Good to use when you want to keep old penalties even when you update the graph.
		 * 
		 * The images below shows two overlapping graph update objects, the right one happened to be applied before the left one. They both have updatePhysics = true and are
		 * set to increase the penalty of the nodes by some amount.
		 * 
		 * The first image shows the result when resetPenaltyOnPhysics is false. Both penalties are added correctly.
		 * \shadowimage{resetPenaltyOnPhysics_False.png}
		 * 
		 * This second image shows when resetPenaltyOnPhysics is set to true. The first GUO is applied correctly, but then the second one (the left one) is applied
		 * and during its updating, it resets the penalties first and then adds penalty to the nodes. The result is that the penalties from both GUOs are not added together.
		 * The green patch in at the border is there because physics recalculation (recalculation of the position of the node, checking for obstacles etc.) affects a slightly larger
		 * area than the original GUO bounds because of the Grid Graph -> Collision Testing -> Diameter setting (it is enlarged by that value). So some extra nodes have their penalties reset.
		 * 
		 * \shadowimage{resetPenaltyOnPhysics_True.png}
		 */
		public bool resetPenaltyOnPhysics = true;
		/** Update Erosion for GridGraphs.
		 * When enabled, erosion will be recalculated for grid graphs
		 * after the GUO has been applied.
		 * 
		 * In the below image you can see the different effects you can get with the different values.\n
		 * The first image shows the graph when no GUO has been applied. The blue box is not identified as an obstacle by the graph, the reason
		 * there are unwalkable nodes around it is because there is a height difference (nodes are placed on top of the box) so erosion will be applied (an erosion value of 2 is used in this graph).
		 * The orange box is identified as an obstacle, so the area of unwalkable nodes around it is a bit larger since both erosion and collision has made
		 * nodes unwalkable.\n
		 * The GUO used simply sets walkability to true, i.e making all nodes walkable.
		 * 
		 * \shadowimage{updateErosion.png}
		 * 
		 * When updateErosion=True, the reason the blue box still has unwalkable nodes around it is because there is still a height difference
		 * so erosion will still be applied. The orange box on the other hand has no height difference and all nodes are set to walkable.\n
		 * \n
		 * When updateErosion=False, all nodes walkability are simply set to be walkable in this example.
		 * 
		 * \see Pathfinding.GridGraph
		 */
		public bool updateErosion = true;
		
		/** NNConstraint to use.
		 * The Pathfinding.NNConstraint.SuitableGraph function will be called on the NNConstraint to enable filtering of which graphs to update.\n
		 * \note As the Pathfinding.NNConstraint.SuitableGraph function is A* Pathfinding Project Pro only, this variable doesn't really affect anything in the free version.
		 * 
		 * 
		 * \astarpro */
		public NNConstraint nnConstraint = NNConstraint.None;
		
		/** Penalty to add to the nodes */
		public int addPenalty = 0;
		
		public bool modifyWalkability = false; /**< If true, all nodes \a walkable variables will be set to #setWalkability */
		public bool setWalkability = false; /**< If #modifyWalkability is true, the nodes' \a walkable variable will be set to this */
		
		public bool modifyTag = false;
		public int setTag = 0;
		
		/** Track which nodes are changed and save backup data.
		 * Used internally to revert changes if needed.
		 */
		public bool trackChangedNodes = false;
		
		private List<Node> changedNodes;
		private List<ulong> backupData;
		private List<Int3> backupPositionData;
		
		public GraphUpdateShape shape = null;
		
		/** Should be called on every node which is updated with this GUO before it is updated.
		  * \param node The node to save fields for. If null, nothing will be done
		  * \see #trackChangedNodes
		  */
		public virtual void WillUpdateNode (Node node) {
			if (trackChangedNodes && node != null) {
				if (changedNodes == null) { changedNodes = ListPool<Node>.Claim(); backupData = ListPool<ulong>.Claim(); backupPositionData = ListPool<Int3>.Claim(); }
				changedNodes.Add (node);
				backupPositionData.Add (node.position);
				backupData.Add ((ulong)node.penalty<<32 | (ulong)node.flags);
			}
		}
		
		/** Reverts penalties and flags (which includes walkability) on every node which was updated using this GUO.
		 * Data for reversion is only saved if #trackChangedNodes is true */
		public virtual void RevertFromBackup () {
			if (trackChangedNodes) {
				if (changedNodes == null) return;
				for (int i=0;i<changedNodes.Count;i++) {
					changedNodes[i].penalty = (uint)(backupData[i]>>32);
					changedNodes[i].flags = (int)(backupData[i] & 0xFFFFFFFF);
					changedNodes[i].position = backupPositionData[i];
					
					ListPool<Node>.Release (changedNodes);
					ListPool<ulong>.Release(backupData);
					ListPool<Int3>.Release(backupPositionData);
				}
			} else {
				throw new System.InvalidOperationException ("Changed nodes have not been tracked, cannot revert from backup");
			}
		}
		
		/** Updates the specified node using this GUO's settings */
		public virtual void Apply (Node node) {
			if (shape == null || shape.Contains	(node)) {
				
				//Update penalty and walkability
				node.penalty = (uint)(node.penalty+addPenalty);
				if (modifyWalkability) {
					node.walkable = setWalkability;
				}
				
				//Update tags
				if (modifyTag) node.tags = setTag;
			}
		}
		
		public GraphUpdateObject () {
		}
		
		/** Creates a new GUO with the specified bounds */
		public GraphUpdateObject (Bounds b) {
			bounds = b;
		}
	}
	
	public interface IRaycastableGraph {
		bool Linecast (Vector3 start, Vector3 end);
		bool Linecast (Vector3 start, Vector3 end, Node hint);
		bool Linecast (Vector3 start, Vector3 end, Node hint, out GraphHitInfo hit);
	}
	
	/** Holds info about one pathfinding thread.
	 * Mainly used to send information about how the thread should execute when starting it
	  */
	public struct PathThreadInfo {
		public int threadIndex;
		public AstarPath astar;
		public NodeRunData runData;
		
		private System.Object _lock;
		public System.Object Lock { get {return _lock; }}
		
		public PathThreadInfo (int index, AstarPath astar, NodeRunData runData) {
			this.threadIndex = index;
			this.astar = astar;
			this.runData = runData;
			_lock = new object();
		}
	}
	
	/** Integer Rectangle.
	 * Works almost like UnityEngine.Rect but with integer coordinates
	 */
	public struct IntRect {
		public int xmin, ymin, xmax, ymax;
		
		public IntRect (int xmin, int ymin, int xmax, int ymax) {
			this.xmin = xmin;
			this.xmax = xmax;
			this.ymin = ymin;
			this.ymax = ymax;
		}
		
		public bool Contains (int x, int y) {
			return !(x < xmin || y < ymin || x > xmax || y > ymax);
		}
		
		/** Returns if this rectangle is valid.
		 * An invalid rect could have e.g xmin > xmax
		 */
		public bool IsValid () {
			return xmin <= xmax && ymin <= ymax;
		}
		
		/** Returns the intersection rect between the two rects.
		 * The intersection rect is the area which is inside both rects.
		 * If the rects do not have an intersection, an invalid rect is returned.
		 * \see IsValid
		 */
		public static IntRect Intersection (IntRect a, IntRect b) {
			IntRect r = new IntRect(
			                        System.Math.Max(a.xmin,b.xmin),
			                        System.Math.Max(a.ymin,b.ymin),
			                        System.Math.Min(a.xmax,b.xmax),
			                        System.Math.Min(a.ymax,b.ymax)
			                        );
			
			return r;
		}
		
		/** Returns a new rect which contains both input rects.
		 * This rectangle may contain areas outside both input rects as well in some cases.
		 */
		public static IntRect Union (IntRect a, IntRect b) {
			IntRect r = new IntRect(
			                        System.Math.Min(a.xmin,b.xmin),
			                        System.Math.Min(a.ymin,b.ymin),
			                        System.Math.Max(a.xmax,b.xmax),
			                        System.Math.Max(a.ymax,b.ymax)
			                        );
			
			return r;
		}
		
		/** Returns a new rect which is expanded by \a range in all directions.
		 * \param range How far to expand. Negative values are permitted.
		 */
		public IntRect Expand (int range) {
			return new IntRect(xmin-range,
			                   ymin-range,
			                   xmax+range,
			                   ymax+range
			                   );
		}
		
		/** Draws some debug lines representing the rect */
		public void DebugDraw (Matrix4x4 matrix, Color col) {
			Vector3 p1 = matrix.MultiplyPoint3x4 (new Vector3(xmin,0,ymin));
			Vector3 p2 = matrix.MultiplyPoint3x4 (new Vector3(xmin,0,ymax));
			Vector3 p3 = matrix.MultiplyPoint3x4 (new Vector3(xmax,0,ymax));
			Vector3 p4 = matrix.MultiplyPoint3x4 (new Vector3(xmax,0,ymin));
			
			Debug.DrawLine (p1,p2,col);
			Debug.DrawLine (p2,p3,col);
			Debug.DrawLine (p3,p4,col);
			Debug.DrawLine (p4,p1,col);
		}
	}
}

#region Delegates

/* Delegate with on Path object as parameter.
 * This is used for callbacks when a path has finished calculation.\n
 * Example function:
 * \code
public void Start () {
	//Assumes a Seeker component is attached to the GameObject
	Seeker seeker = GetComponent<Seeker>();
	
	//seeker.pathCallback is a OnPathDelegate, we add the function OnPathComplete to it so it will be called whenever a path has finished calculating on that seeker
	seeker.pathCallback += OnPathComplete;
}

public void OnPathComplete (Path p) {
	Debug.Log ("This is called when a path is completed on the seeker attached to this GameObject");
}\endcode
  */
public delegate void OnPathDelegate (Path p);

public delegate Vector3[] GetNextTargetDelegate (Path p, Vector3 currentPosition);

public delegate void NodeDelegate (Node node);

public delegate void OnGraphDelegate (NavGraph graph);

public delegate void OnScanDelegate (AstarPath script);

public delegate void OnVoidDelegate ();

#endregion

#region Enums

/** How path results are logged by the system */
public enum PathLog {
	None,		/**< Does not log anything */
	Normal,		/**< Logs basic info about the paths */
	Heavy,		/**< Includes additional info */
	InGame,		/**< Same as heavy, but displays the info in-game using GUI */
	OnlyErrors	/**< Same as normal, but logs only paths which returned an error */
}

/** Heuristic to use. Heuristic is the estimated cost from the current node to the target */
public enum Heuristic {
	Manhattan,
	DiagonalManhattan,
	Euclidean,
	None
}

/** What data to draw the graph debugging with */
public enum GraphDebugMode {
	Areas,
	G,
	H,
	F,
	Penalty,
	Connections,
	Tags
}

/** Type of connection for a user placed link */
public enum ConnectionType {
	Connection,
	ModifyNode
}

public enum ThreadCount {
	Automatic = -1,
	None = 0,
	One = 1,
	Two,
	Three,
	Four,
	Five,
	Six,
	Seven,
	Eight
}

public enum PathState {
	Created = 0,
	PathQueue = 1,
	Processing = 2,
	ReturnQueue = 3,
	Returned = 4
}

public enum PathCompleteState {
	NotCalculated = 0,
	Error = 1,
	Complete = 2,
	Partial = 3
}

#endregion