//#define ASTAR_EucledianHeuristic	//"Heuristic"[ASTAR_NoHeuristic,ASTAR_ManhattanHeuristic,ASTAR_DiagonalManhattanHeuristic,ASTAR_EucledianHeuristic]Forces the heuristic to be the chosen one or disables it altogether
//#define ASTAR_NoHScaling    //Should H score scaling be enabled. H Score is usually multiplied with UpdateH's parameter 'scale'
//#define ASTAR_NoTagPenalty		//Enables or disables tag penalties. Can give small performance boost
//#define ASTAR_ConfigureTagsAsMultiple
//#define ASTAR_SINGLE_THREAD_OPTIMIZE


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

namespace Pathfinding {
	/** Holds one node in a navgraph. A node is a simple object in the shape of a square (GridNode), triangle (NavMeshNode), or point (the rest).\n
	 * A node has a position and a list of neighbour nodes which are the nodes this node can form a straight path to\n
	 * Size: (4*3)+4+4+4+4+4+4+8+8*n = 44+8*n bytes where \a n is the number of connections (estimate) */

	public class Node
		: NodeRun
	{
		
		/** Global node index */
		private int nodeIndex;
		
		/** Returns the global node index. Used for internal pathfinding purposes */
		public int GetNodeIndex () {
			return nodeIndex;
		}
		
		//public override int GetHashCode () { return GetNodeIndex (); }
		
		public NodeRun GetNodeRun (NodeRunData data) {
			return (NodeRun)this;
		}
		public void SetNodeIndex (int index) { nodeIndex = index; }
		
		//Size = 4*3 + 4 + 4 + 4 + 4 + 4 + 4 = 28 bytes
		
		/** Position in world space of the node.
		 * The position is stored as integer coordinates to avoid precision loss when the node is far away from the world origin.
		 * The default precision is 0.001 (one millimeter).
		 * \see Pathfinding.Int3
		 */
		public Int3 position;
		
		/* public int pathID {
			get {
				return pathIDx & 0xFFFF;
			}
			set {
				pathIDx = (pathIDx & ~0xFFFF) | value;
			}
		}*/
		
		//Not used anymore, only slowed down pathfinding with no other positive effect
		/* public int heapIndex {
			get {
				return pathIDx >> 16;
			}
			set {
				pathIDx = (pathIDx & 0xFFFF) | (value << 16);
			}
		}*/
		
		//public static Path activePath; /*< Path which is currently being calculated */
		
		private uint _penalty; /**< Penlty cost for walking on this node. This can be used to make it harder/slower to walk over certain areas. */
		public uint penalty {
			get {
				return _penalty;
			}
			set {
				if (value > 0xFFFFF)
					Debug.LogWarning ("Very high penalty applied. Are you sure negative values haven't underflowed?\n" +
						"Penalty values this high could with long paths cause overflows and in some cases infinity loops because of that.\n" +
						"Penalty value applied: "+value);
				_penalty = value;
			}
		}
		
		//public int tags = 1; /* < Tags for walkability */
		
		/** List of all connections from this node. This node's neighbour nodes are stored in this array.\n
		 * \note Not all connections are stored in this array, some node types (such as Pathfinding.GridNode) use custom connection systems which they store somewhere else.
		 * \see #connectionCosts */
		public Node[] connections;
		
		/** Cost for the connections to other nodes. The cost of moving from this node to connections[x] is stored in connectionCosts[x].
		 * \see #connections */
		public int[] connectionCosts;
		
#region Flags
		
		//Last 8 bytes used (area = last 8, walkable = next 1, graphIndex = 18 - 22), can be used for other stuff
		
		/** Bit packed values for different fields. It's amazing how much information you can store in an integer!
		 * Below you can see a table showing how the different bits are used:
		 * \htmlonly
<table class="inlinetable">
<tr>
<th colspan="1"></th>
<th colspan="8">Area</th>
<th colspan="1">Walkability</th>
<th colspan="6">Graph Index</th>
<th colspan="2">Graph Specific Values</th>
<th colspan="6">Tags (tag)</th>
<th colspan="1">Bit 8 - Path specific value</th>
<th colspan="8">Graph specific values</th>
</tr>
<tr>
<th colspan="1"></th>
<th colspan="8"></th>
<th colspan="1"></th>
<th colspan="6"></th>
<th colspan="1"></th>
<th colspan="1">Walkable before erosion (Grid Graph and derived only)</th>
<th colspan="6"></th>
<th colspan="1">Used as a tag by some path types</th>
<th colspan="8">(GridGraph only) Connections</th>
</tr>
<tr><th>Bit</th>
<td>31</td><td>30</td><td>29</td><td>28</td><td>27</td><td>26</td><td>25</td><td>24</td><td>23</td><td>22</td><td>21</td><td>20</td><td>19</td><td>18</td><td>17</td>
<td>16</td><td>15</td><td>14</td><td>13</td><td>12</td><td>11</td><td>10</td><td>9</td><td>8</td><td>7</td><td>6</td><td>5</td><td>4</td><td>3</td><td>2</td>
<td>1</td><td>0</td></tr>
</table>
		 * \endhtmlonly
		 * Do not get or set this variable directly, instead, get or set the appropriate properties.
		  * \see walkable
		  * \see area
		  * \see graphIndex
		  * \see Bit8
		  * \see tags
		  */
		public int flags;
		
		const int WalkableBitNumber = 23; /**< Bit number for the #walkable bool */
		const int WalkableBit = 1 << WalkableBitNumber; /** 1 \<\< #WalkableBitNumber */
		
		const int AreaBitNumber = 24; /**< Bit number at which #area starts */
		const int AreaBitsSize = 0xFF; /**< Size of the #area bits */
		const int NotAreaBits = ~(AreaBitsSize << AreaBitNumber); /**< The bits in #flags which are NOT #area bits */
		
		const int GraphIndexBitNumber = 18;
		const int GraphIndexBitsSize = 0x1F;
		const int NotGraphIndexBits = ~(GraphIndexBitsSize << GraphIndexBitNumber); /**< Bits which are NOT #graphIndex bits */
		
		/** Tags for walkability. Determines which tag is set for this node. 0...31.
		 * \warning Do NOT pass a value larger than 31 to this variable, that could affect other parameters since it's bit-packed.
		 */
		public int tags {
			get {
				return (flags >> 9) & 0x1F;
			}
			set {
				//0x1F << 9
				flags = (flags & ~0x3E00) | value<<9;
			}
		}
		
		/** Returns bit 8 from #flags. Used to flag special nodes with special pathfinders */
		public bool Bit8 {
			get {
				return (flags & 0x100) != 0;
			}
			set {
				flags = (flags & ~0x100) | (value ? 0x100 : 0);
			}
		}
		
		/** Returns bit 15 from #flags. In the GridGraph (and derived) it is used to store if walkable before erosion.
		  * \see GridNode.WalkableErosion */
		public bool Bit15 {
			get { return ((flags >> 15) & 1) != 0; }
			set { flags = (flags & ~(1 << 15)) | ((value?1:0) << 15); }
		}
		
		/** Returns bit 16 from #flags.
		 * Graphs can use this value for any kind of data storage.
		 * Grid Graphs use it as a temporary variable when updating graphs using erosion.
		  * \see GridNode.WalkableErosion */
		public bool Bit16 {
			get { return ((flags >> 16) & 1) != 0; }
			set { flags = (flags & ~(1 << 16)) | ((value?1:0) << 16); }
		}
		
		/** Is the node walkable */
		public bool walkable {
			get {
				//return ((flags >> 23) & 1) == 1;
				return (flags & WalkableBit) == WalkableBit;
			}
			set {
				flags = (flags & ~WalkableBit) | (value ? WalkableBit : 0);
			}
		}
		
		/** Area ID of the node. Nodes which there are no valid path between have different area values.
		  * \note Small areas can have have the same area ID since only 256 ID values are available
		  * \see AstarPath.minAreaSize
		  */
		public int area {
			get {
				//Note, the & is required since #flags is a signed int it will fill on with 1s instead of 0s when the 31st bit is true.
				//(see signed versus unsigned bitshifts)
				return (flags >> AreaBitNumber) & AreaBitsSize;
			}
			set {
				flags = (flags & NotAreaBits) | (value << AreaBitNumber);
			}
		}
				
		/** The index of the graph this node is in.
		  * \see \link Pathfinding.AstarData.graphs AstarData.graphs \endlink */
		public int graphIndex {
			get {
				return ((flags >> GraphIndexBitNumber) & GraphIndexBitsSize);
			}
			set {
				flags = (flags & NotGraphIndexBits) | ((value & GraphIndexBitsSize) << GraphIndexBitNumber);
			}
		}
		
#endregion
		
		/* F score. The F score is the #g score + #h score, that is the cost it taken to move to this node from the start + the estimated cost to move to the end node.\n
		 * Nodes are sorted by their F score, nodes with lower F scores are opened first */
		/*public uint f {
			get {
				return g+h;
			}
		}*/
		
		/** Calculates and updates the H score.
		 * Calculates the H score with respect to the target position and chosen heuristic.
		 * \param targetPosition The position to calculate the distance to.
		 * \param heuristic Heuristic to use. The heuristic can also be hard coded using pre processor directives (check sourcecode)
		 * \param scale Scale of the heuristic
		 * \param nodeR NodeRun object associated with this node.
		 */
		public void UpdateH (Int3 targetPosition, Heuristic heuristic, float scale, NodeRun nodeR) {		
			//Choose the correct heuristic, compute it and store it in the \a h variable
			if (heuristic == Heuristic.None) {
				nodeR.h = 0;
				return;
			}
			
			if (heuristic == Heuristic.Euclidean) {
				nodeR.h = (uint)Mathfx.RoundToInt ((position-targetPosition).magnitude*scale);
			} else if (heuristic == Heuristic.Manhattan) {
				nodeR.h = (uint)Mathfx.RoundToInt  (
				                      (Abs (position.x-targetPosition.x) + 
				                      Abs (position.y-targetPosition.y) + 
				                      Abs (position.z-targetPosition.z))
				                      * scale
				                      );
			} else { //if (heuristic == Heuristic.DiagonalManhattan) {
				int xDistance = Abs (position.x-targetPosition.x);
				int zDistance = Abs (position.z-targetPosition.z);
				if (xDistance > zDistance) {
				     nodeR.h = (uint)(14*zDistance + 10*(xDistance-zDistance))/10;
				} else {
				     nodeR.h = (uint)(14*xDistance + 10*(zDistance-xDistance))/10;
				}
				nodeR.h = (uint)Mathfx.RoundToInt (nodeR.h * scale);
			}
		}
	
		
		/*
		public 
if !NoVirtualUpdateH
		virtual
endif
		void UpdateH (Int3 targetPosition, Heuristic heuristic, float scale) {		
if AstarFree || !DefinedHeuristic
			//Choose the correct heuristic, compute it and store it in the \a h variable
			if (heuristic == Heuristic.None) {
				h = 0;
				return;
			}
			
			if (heuristic == Heuristic.Manhattan) {
				h = (uint)Mathfx.RoundToInt  (
				                      (Abs (position.x-targetPosition.x) + 
				                      Abs (position.y-targetPosition.y) + 
				                      Abs (position.z-targetPosition.z))
				                      * scale
				                      );
			} else if (heuristic == Heuristic.DiagonalManhattan) {
				int xDistance = Abs (position.x-targetPosition.x);
				int zDistance = Abs (position.z-targetPosition.z);
				if (xDistance > zDistance) {
				     h = (uint)(14*zDistance + 10*(xDistance-zDistance))/10;
				} else {
				     h = (uint)(14*xDistance + 10*(zDistance-xDistance))/10;
				}
				h = (uint)Mathfx.RoundToInt (h * scale);
			} else {
				h = (uint)Mathfx.RoundToInt ((position-targetPosition).magnitude*scale);
			}
else
	//For faster execution (roughly a 4-5% speedup in my tests), the heuristic can be hard coded using pre-processor directives
	//See optimizations area in the A* inspector
	if NoHeuristic
			h = 0;
			return;
	elif ManhattanHeuristic
		if !NoHScaling
			h = (uint)Mathfx.RoundToInt  (
				                      (Abs (position.x-targetPosition.x) + 
				                      Abs (position.y-targetPosition.y) + 
				                      Abs (position.z-targetPosition.z))
				                      * scale
				                      );
		else
			h = (uint)(					 Abs (position.x-targetPosition.x) + 
				                      Abs (position.y-targetPosition.y) + 
				                      Abs (position.z-targetPosition.z));
		endif
	elif DiagonalManhattanHeuristic
			int xDistance = Abs (position.x-targetPosition.x);
			int zDistance = Abs (position.z-targetPosition.z);
			if (xDistance > zDistance) {
			     h = (uint)(14*zDistance + 10*(xDistance-zDistance))/10;
			} else {
			     h = (uint)(14*xDistance + 10*(zDistance-xDistance))/10;
			}
		if !NoHScaling
			h = (uint)Mathfx.RoundToInt (h * scale);
		endif
	elif EucledianHeuristic
		if !NoHScaling
			h = (uint)Mathfx.RoundToInt ((position-targetPosition).magnitude*scale);
		else
			h = (uint)Mathfx.RoundToInt ((position-targetPosition).magnitude);
		endif
	endif
endif
		}
		*/
		
		/** Implementation of the Absolute function */
		public static int Abs (int x) {
			return x < 0 ? -x : x;
		}
		
		public void UpdateG (NodeRun nodeR, NodeRunData nodeRunData) {
			nodeR.g = nodeR.parent.g+nodeR.cost+penalty
				+ nodeRunData.path.GetTagPenalty(tags)
					;
		}
		
		
		protected void BaseUpdateAllG (NodeRun nodeR, NodeRunData nodeRunData) {
			UpdateG (nodeR, nodeRunData);
			
			nodeRunData.open.Add (nodeR);
			
			if (connections == null) {
				return;
			}
			
			//Loop through the connections of this node and call UpdateALlG on nodes which have this node set as #parent and has been searched by the pathfinder for this path */
			for (int i=0;i<connections.Length;i++) {
				NodeRun otherR = connections[i].GetNodeRun (nodeRunData);
				if (otherR.parent == nodeR && otherR.pathID == nodeRunData.pathID) {
					connections[i].UpdateAllG (otherR, nodeRunData);
				}
			}
		}
		
		public virtual void UpdateAllG (NodeRun nodeR, NodeRunData nodeRunData) {
			BaseUpdateAllG (nodeR, nodeRunData);
		}
		
		public virtual int[] InitialOpen (BinaryHeapM open, Int3 targetPosition, Int3 position, Path path, bool doOpen) {
			return BaseInitialOpen (open,targetPosition,position,path,doOpen);
		}
		
		public int[] BaseInitialOpen (BinaryHeapM open, Int3 targetPosition, Int3 position, Path path, bool doOpen) {
			
			if (connectionCosts == null) {
				return null;
			}
			
			int[] costs = connectionCosts;
			connectionCosts = new int[connectionCosts.Length];
			
			
			for (int i=0;i<connectionCosts.Length;i++) {
				connectionCosts[i] = (connections[i].position-position).costMagnitude;
			}
			
			if (!doOpen) {	
				for (int i=0;i<connectionCosts.Length;i++) {
					Node other = connections[i];
					if (other.connections != null) {
						for (int q = 0;q < other.connections.Length;q++) {
							if (other.connections[q] == this) {
								other.connectionCosts[q] = connectionCosts[i];
								break;
							}
						}
					}
				}
			}
			
			//Should we open the node and reset the distances after that or only calculate the distances and don't reset them
			if (doOpen) {
				//Open (open,targetPosition,path);
				connectionCosts = costs;
			}
			
			return costs;
		}
		
		/** Resets the costs modified by the InitialOpen function when 'doOpen' was false (for the end node). This is called at the end of a pathfinding search */
		public virtual void ResetCosts (int[] costs) {
			BaseResetCosts (costs);
		}
		
		/** \copydoc ResetCosts */
		public void BaseResetCosts (int[] costs) {
			connectionCosts = costs;
			
			if (connectionCosts == null) {
				return;
			}
			
			for (int i=0;i<connectionCosts.Length;i++) {
				Node other = connections[i];
				if (other.connections != null) {
					for (int q = 0;q < other.connections.Length;q++) {
						if (other.connections[q] == this) {
							other.connectionCosts[q] = connectionCosts[i];
							break;
						}
					}
				}
			}
		}
		
		public virtual void Open (NodeRunData nodeRunData, NodeRun nodeR, Int3 targetPosition, Path path) {
			BaseOpen (nodeRunData,nodeR, targetPosition,path);
		}
		
		/** Opens the nodes connected to this node. This is a base call and can be called by node classes overriding the Open function to open all connections in the #connections array.
		 * \see #connections
		 * \see Open */
		public void BaseOpen (NodeRunData nodeRunData, NodeRun nodeR, Int3 targetPosition, Path path) {
			
			if (connections == null) return;
			
			for (int i=0;i<connections.Length;i++) {
				Node conNode = connections[i];
				
				if (!path.CanTraverse (conNode)) {
					continue;
				}
				
				NodeRun nodeR2 = conNode.GetNodeRun (nodeRunData);
				
				if (nodeR2.pathID != nodeRunData.pathID) {
					
					nodeR2.parent = nodeR;
					nodeR2.pathID = nodeRunData.pathID;
					
					nodeR2.cost = (uint)connectionCosts[i];
					
					conNode.UpdateH (targetPosition, path.heuristic, path.heuristicScale, nodeR2);
					conNode.UpdateG (nodeR2, nodeRunData);
					
					nodeRunData.open.Add (nodeR2);
					
					//Debug.DrawLine (position,node.position,Color.cyan);
					//Debug.Log ("Opening	Node "+node.position.ToString ()+" "+g+" "+node.cost+" "+node.g+" "+node.f);
				} else {
					//If not we can test if the path from the current node to this one is a better one then the one already used
					uint tmpCost = (uint)connectionCosts[i];
					
					if (nodeR.g+tmpCost+conNode.penalty
				+ path.GetTagPenalty(conNode.tags)
					    	< nodeR2.g) {
						
						nodeR2.cost = tmpCost;
						nodeR2.parent = nodeR;
						
						conNode.UpdateAllG (nodeR2,nodeRunData);
						
						nodeRunData.open.Add (nodeR2);
					}
					
					 else if (nodeR2.g+tmpCost+penalty
				+ path.GetTagPenalty(tags)
					         < nodeR.g) {//Or if the path from this node ("node") to the current ("current") is better
						
						bool contains = conNode.ContainsConnection (this);
						
						//Make sure we don't travel along the wrong direction of a one way link now, make sure the Current node can be moved to from the other Node.
						/*if (node.connections != null) {
							for (int y=0;y<node.connections.Length;y++) {
								if (node.connections[y] == this) {
									contains = true;
									break;
								}
							}
						}*/
						
						if (!contains) {
							continue;
						}
						
						nodeR.parent = nodeR2;
						nodeR.cost = tmpCost;
						
						UpdateAllG (nodeR,nodeRunData);
						
						nodeRunData.open.Add (nodeR);
					}
				}
			}
		}
		
		/** Get all connections for a node.
		 * This function will call the callback with every node this node is connected to.
		 * In contrast to the #connections array this function also includes custom connections which
		 * for example grid graphs use.
		 * \since Added in version 3.2
		 */
		public virtual void GetConnections (NodeDelegate callback) {
			GetConnectionsBase (callback);
		}
		
		/** Get all connections for a node.
		 * This function will call the callback with every node this node is connected to.
		 * This is a base function and will simply loop through the #connections array.
		 * \see GetConnections
		 */
		public void GetConnectionsBase (NodeDelegate callback) {
			if (connections == null) {
				return;
			}
			
			for (int i=0;i<connections.Length;i++) {
				if (connections[i].walkable) {
					callback (connections[i]);
				}
			}
		}
		
		/** Adds all connecting nodes to the \a stack and sets the #area variable to \a area */
		public virtual void FloodFill (Stack<Node> stack, int area) {
			BaseFloodFill (stack,area);
		}
		
		/** Adds all connecting nodes to the \a stack and sets the #area variable to \a area. This is a base function and can be called by node classes overriding the FloodFill function to add the connections in the #connections array */
		public void BaseFloodFill (Stack<Node> stack, int area) {
			
			if (connections == null) {
				return;
			}
			
			for (int i=0;i<connections.Length;i++) {
				if (connections[i].walkable && connections[i].area != area) {
					stack.Push (connections[i]);
					connections[i].area = area;
				}
			}
		}
		
		/** Remove connections to unwalkable nodes.
		 * This function loops through all connections and removes the ones which lead to unwalkable nodes.\n
		 * This can speed up performance if a lot of nodes have connections to unwalkable nodes, they usually don't though
		 * \note This function does not add connections which might have been removed previously
		*/
		public virtual void UpdateConnections () {
			
			if (connections != null) {
				List<Node> newConn = null;
				List<int> newCosts = null;
			
				for (int i=0;i<connections.Length;i++) {
					if (!connections[i].walkable) {
						
						if (newConn == null) {
							newConn = new List<Node> (connections.Length-1);
							newCosts = new List<int> (connections.Length-1);
							for (int j=0;j<i;j++) {
								newConn.Add (connections[j]);
								newCosts.Add (connectionCosts[j]);
							}
						}
					} else if (newConn != null) {
						newConn.Add (connections[i]);
						newCosts.Add (connectionCosts[i]);
					}
				}
			}
			
		}
		
		/** Calls UpdateConnections on all neighbours.
		 * Neighbours are all nodes in the connections array. Good to use if the node has been set to unwalkable-
		 * \see UpdateConnections */
		public virtual void UpdateNeighbourConnections () {
			if (connections != null) {
				for (int i=0;i<connections.Length;i++) {
					connections[i].UpdateConnections ();
				}
			}
		}
		
		/** Returns true if this node has a connection to the node.
		 * \note this might not return true for node classes using their own connection system (like GridNode)
		*/
		public virtual bool ContainsConnection (Node node) {
			if (connections != null) {
				for (int i=0;i<connections.Length;i++) {
					if (connections[i] == node) {
						return true;
					}
				}
			}
			return false;
		}
		
		/** Add a connection to the node with the specified cost.
		 * If a connection to the node already exists, this function will only change the cost of it.
		 * \note This will create a one-way connection, consider calling the same function on the other node too.
		 * 
		 * \note GridGraphs use custom connections and has a fixed cost for connections to neighbour nodes.
		 * So you will not be able to modify costs to neighbour nodes on grid graphs with this function.
		 * 
		 * \see RemoveConnection
		 * \see Pathfinding.Int3.costMagnitude */
		public void AddConnection (Node node, int cost) {
			
			if (connections == null) {
				connections = new Node[0];
				connectionCosts = new int[0];
			} else {
				for (int i=0;i<connections.Length;i++) {
					//Connection already exists
					if (connections[i] == node) {
						//Just update cost
						connectionCosts[i] = cost;
						return;
					}
				}
			}
			
			Node[] old_connections = connections;
			int[] old_costs = connectionCosts;
			
			connections = new Node[connections.Length+1];
			connectionCosts = new int[connections.Length];
			
			for (int i=0;i<old_connections.Length;i++) {
				connections[i] = old_connections[i];
				connectionCosts[i] = old_costs[i];
			}
			
			connections[old_connections.Length] = node;
			connectionCosts[old_connections.Length] = cost;
		}
		
		/** Removes the connection to the node if it exists.
		 * Returns true if a connection was removed, returns false if no connection to the node was found
		 * \note This will only remove the connection from this node to \a node, but it will still exist in the other direction
		 * consider calling the same function on the other node too
		 * 
		 * \see AddConnection
		 */
		public virtual bool RemoveConnection (Node node) {
			if (connections == null) { return false; }
			
			for (int i=0;i<connections.Length;i++) {
				if (connections[i] == node) {
					//Swap with last item
					connections[i] = connections[connections.Length-1];
					connectionCosts[i] = connectionCosts[connectionCosts.Length-1];
					
					//Create new arrays
					Node[] new_connections = new Node[connections.Length-1];
					int[] new_costs = new int[connections.Length-1];
					
					//Copy the remaining connections
					for (int j=0;j<connections.Length-1;j++) {
						new_connections[j] = connections[j];
						new_costs[j] = connectionCosts[j];
					}
					
					connections = new_connections;
					connectionCosts = new_costs;
					return true;
				}
			}
			return false;
		}

		/**
		 * Recalculate costs for each connection.
		 * All standard connections, which means those stored in the #connections array will have their
		 * costs recalculated. Non-standard connections such as most grid graph connections will
		 * not be recalculated (mostly because they are constant and cannot be recalculated).
		 *
		 * @param neighbours If true, recalculates connection costs on this node's neighbours as well.
		 * This makes sure costs are calculated correctly (the same) in both directions.
		 *
		 * \note Assumes the correct cost is simply the distance between the nodes (in Int3 space).
		 * This is what the built-in graphs (e.g Point Graph) use, so it should be fine.
		 */
		public void RecalculateConnectionCosts (bool neighbours) {
			//Calculate the cost for each connection
			if (connections == null) return;
			
			for (int i=0;i<connections.Length;i++)
				connectionCosts[i] = (int)System.Math.Round((position - connections[i].position).magnitude);
	
			//Recalculate connection costs for neighbours as well
			//Makes sure costs are calculated correctly (the same) in both directions
			if (neighbours) for (int i=0;i<connections.Length;i++) connections[i].RecalculateConnectionCosts (false);
		}
	}
}