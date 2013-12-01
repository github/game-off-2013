//#define ASTAR_SINGLE_THREAD_OPTIMIZE
//#define ASTAR_MORE_PATH_IDS //Increases the number of pathIDs from 2^16 to 2^32. Uses more memory.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

namespace Pathfinding {
	public class NodeRunData {
		/** Path currently being calculated.
		 * Could also be the last path calculated */
		public Path path;
		
		/** Path ID of the last path */
		public ushort pathID;
		
		/** Temporary calculation data for all nodes */
		public NodeRun[] nodes;
		
		/** The open list.
		  * A binary heap holds and sorts the open list for the pathfinding. 
	 	  * Binary Heaps are extreamly fast in providing a priority queue for the node with the lowest F score.*/
		public BinaryHeapM open = new BinaryHeapM (AstarPath.InitialBinaryHeapSize);
		
		/** String builder for debug output */
		private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
		
		/** Returns a StringBuilder which is to be used for constructing debug strings for paths.
		 * This StringBuilder should only be used in Pathfinding.Path.DebugString.
		 * Caching it in this class is faster than creating a new one every time.
		 * It is not guaranteed to be cleared.
		 */
		public System.Text.StringBuilder DebugStringBuilder {
			get { return stringBuilder; }
		}
		
		/** Initializes the NodeRunData for calculation of the specified path.
		 * Called by core pathfinding functions just before starting to calculate a path.
		 */
		public void Initialize (Path p) {
			path = p;
			
			pathID = p.pathID;
			
			//Resets the binary heap, don't clear everything because that takes an awful lot of time, instead we can just change the numberOfItems in it (which is just an int)
			//Binary heaps are just like a standard array but are always sorted so the node with the lowest F value can be retrieved faster
			open.Clear ();
		}
		
		/** Set all node's pathIDs to 0.
		 * \see Pathfinding.NodeRun.pathID
		 */
		public void ClearPathIDs () {
			
			NavGraph[] graphs = AstarPath.active.astarData.graphs;
			for (int i=0;i<graphs.Length;i++) {
				Node[] nodes = graphs[i].nodes;
				if (nodes != null) {
					for (int j=0;j<nodes.Length;j++) {
						if (nodes[j] != null) nodes[j].pathID = 0;
					}
				}
			}
		}
	}
	
	public class NodeRun {
		/** G score of this node.
		 * The G score is the total cost from the start to this node.
		 */
		public uint g;
		
		/** H score for this node.
		 * The H score is the estimated cost from this node to the end.
		 */
		public uint h;
		
		public Node node {
			get {
				return (Node)this;
			}
			set {}
		}
		
		public ushort pathID;
		
		public uint cost;
		
		public NodeRun parent;
		
		public void Reset () {
			g = 0;
			h = 0;
			pathID = 0;
			cost = 0;
			parent = null;
		}
		
		/** F score. The F score is the #g score + #h score, that is the cost it taken to move to this node from the start + the estimated cost to move to the end node.\n
		 * Nodes are sorted by their F score, nodes with lower F scores are opened first */
		public uint f {
			get {
				return g+h;
			}
		}
		
		/** Links this NodeRun with the specified node.
		 * \param node Node to link to
		 * \param index Index of this NodeRun in the nodeRuns array in Pathfinding.AstarData
		 */
		public void Link (Node node, int index) {
			if (index != node.GetNodeIndex ())
				throw new System.Exception ("Node indices out of sync when creating NodeRun data (node index != specified index)");
		}
	}
}