//#define ASTAR_NoTagPenalty
using System;
using Pathfinding;
using System.Collections.Generic;

namespace Pathfinding.Nodes
{
	public class GridNode : Node {
		
		//Flags used
		//last 8 bits - see Node class
		//First 8 bits for connectivity info inside this grid
		
		//Size = [Node, 28 bytes] + 4 = 32 bytes
		
		/** First 24 bits used for the index value of this node in the graph specified by the last 8 bits. \see #gridGraphs */
		protected int indices;
		
		/** Internal list of grid graphs.
		 * Used for fast typesafe lookup of graphs.
		 */
		public static GridGraph[] gridGraphs;
		
		/** Returns if this node has any valid grid connections */
		public bool HasAnyGridConnections () {
			return (flags & 0xFF) != 0;
		}
		
		/** Has connection to neighbour \a i.
		 * The neighbours are the up to 8 grid neighbours of this node.
		 * \see SetConnection
		 */
		public bool GetConnection (int i) {
			return ((flags >> i) & 1) == 1;
		}
		
		/** Set connection to neighbour \a i.
		 * \param i Connection to set [0...7]
		 * \param value 1 or 0 (true/false)
		 * The neighbours are the up to 8 grid neighbours of this node
		 * \see GetConnection
		 */
		public void SetConnection (int i, int value) {
			flags = flags & ~(1 << i) | (value << i);
		}
		
		/** Sets a connection without clearing the previous value.
		 * Faster if you are setting all connections at once and have cleared the value before calling this function
		 * \see SetConnection */
		public void SetConnectionRaw (int i, int value) {
			flags = flags | (value << i);
		}
		
		/** Returns index of which GridGraph this node is contained in */
		public int GetGridIndex () {
			return indices >> 24;
		}
		
		/** Returns the grid index in the graph */
		public int GetIndex () {
			return indices & 0xFFFFFF;
		}
		
		/** Set the grid index in the graph */
		public void SetIndex (int i) {
			indices &= ~0xFFFFFF;
			indices |= i;
		}
		
		/** Sets the index of which GridGraph this node is contained in.
		 * Only changes lookup variables, does not actually move the node to another graph.
		 */
		public void SetGridIndex (int gridIndex) {
			indices &= 0xFFFFFF;
			indices |= gridIndex << 24;
		}
		
		/** Returns if walkable before erosion.
		  * Identical to Pathfinding.Node.Bit15 but should be used when possible to make the code more readable */
		public bool WalkableErosion {
			get { return Bit15; }//return ((flags >> 15) & 1) != 0 ? true : false; }
			set { Bit15 = value; }//flags = (flags & ~(1 << 15)) | (value?1:0 << 15); }
		}
		
		/*public override bool ContainsConnection (Node node, Path p) {
			if (!node.IsWalkable (p)) {
				return false;
			}
			
			if (connections != null) {
				for (int i=0;i<connections.Length;i++) {
					if (connections[i] == node) {
						return true;
					}
				}
			}
			
			int index = indices & 0xFFFFFF;
			
			int[] neighbourOffsets = gridGraphs[indices >> 24].neighbourOffsets;
			GridNode[] nodes = gridGraphs[indices >> 24].nodes;
			
			for (int i=0;i<8;i++) {
				if (((flags >> i) & 1) == 1) {
					
					Node other = nodes[index+neighbourOffsets[i]];
					if (other == node) {
						return true;
					}
				}
			}
			return false;
		}*/
		
		/** Updates the grid connections of this node and it's neighbour nodes to reflect walkability of this node */
		public void UpdateGridConnections () {
			
			GridGraph graph = gridGraphs[indices >> 24];
			
			int index = GetIndex();
			
			int x = index % graph.width;
			int z = index/graph.width;
			graph.CalculateConnections (graph.nodes, x, z, this);
			
			int[] neighbourOffsets = graph.neighbourOffsets;
			int[] neighbourOffsetsX = graph.neighbourXOffsets;
			int[] neighbourOffsetsZ = graph.neighbourZOffsets;
			
			//Loop through neighbours
			for (int i=0;i<8;i++) {
				//Find the coordinates for the neighbour
				int nx = x+neighbourOffsetsX[i];
				int nz = z+neighbourOffsetsZ[i];
				
				//Make sure it is not out of bounds
				if (nx < 0 || nz < 0 || nx >= graph.width || nz >= graph.depth) {
					continue;
				}
				
				GridNode node = (GridNode)graph.nodes[index+neighbourOffsets[i]];
				
				//Calculate connections for neighbour
				graph.CalculateConnections (graph.nodes, nx, nz, node);
			}
			
		}	

		/** Removes a connection from the node.
		 * This can be a standard connection or a grid connection
		 * \returns True if a connection was removed, false otherwsie */
		public override bool RemoveConnection (Node node) {
			
			bool standard = base.RemoveConnection (node);
			
			GridGraph graph = gridGraphs[indices >> 24];
			
			int index = indices & 0xFFFFFF;
			
			int x = index % graph.width;
			int z = index/graph.width;
			graph.CalculateConnections (graph.nodes, x, z, this);
			
			int[] neighbourOffsets = graph.neighbourOffsets;
			int[] neighbourOffsetsX = graph.neighbourXOffsets;
			int[] neighbourOffsetsZ = graph.neighbourZOffsets;
			
			for (int i=0;i<8;i++) {
				
				int nx = x+neighbourOffsetsX[i];
				int nz = z+neighbourOffsetsZ[i];
				
				if (nx < 0 || nz < 0 || nx >= graph.width || nz >= graph.depth) {
					continue;
				}
				
				GridNode gNode = (GridNode)graph.nodes[index+neighbourOffsets[i]];
				if (gNode == node) {
					SetConnection (i,0);
					return true;
				}
			}
			return standard;
		}
		
		public override void UpdateConnections () {
			base.UpdateConnections ();
			UpdateGridConnections ();
		}
		
		public override	void UpdateAllG (NodeRun nodeR, NodeRunData nodeRunData) {
			BaseUpdateAllG (nodeR, nodeRunData);
			
			int index = GetIndex ();
			
			int[] neighbourOffsets = gridGraphs[indices >> 24].neighbourOffsets;
			Node[] nodes = gridGraphs[indices >> 24].nodes;
			
			for (int i=0;i<8;i++) {
				if (GetConnection (i)) {
				//if (((flags >> i) & 1) == 1) {
					
					Node node = nodes[index+neighbourOffsets[i]];
					NodeRun nodeR2 = node.GetNodeRun (nodeRunData);
					
					if (nodeR2.parent == nodeR && nodeR2.pathID == nodeRunData.pathID) {
						node.UpdateAllG (nodeR2,nodeRunData);
					}
				}
			}
		}
		
		public override void GetConnections (NodeDelegate callback) {
			
			GetConnectionsBase (callback);
			
			GridGraph graph = gridGraphs[indices >> 24];
			
			int index = GetIndex ();
			
			int[] neighbourOffsets = graph.neighbourOffsets;
			Node[] nodes = graph.nodes;
			
			for (int i=0;i<8;i++) {
				if (((flags >> i) & 1) == 1) {
					
					Node node = nodes[index+neighbourOffsets[i]];
					
					callback (node);
				}
			}
		}
		
		public override void FloodFill (Stack<Node> stack, int area) {
			
			base.FloodFill (stack,area);
			
			GridGraph graph = gridGraphs[indices >> 24];
			
			int index = indices & 0xFFFFFF;
			
			int[] neighbourOffsets = graph.neighbourOffsets;
			//int[] neighbourCosts = graph.neighbourCosts;
			Node[] nodes = graph.nodes;
			
			for (int i=0;i<8;i++) {
				if (((flags >> i) & 1) == 1) {
					
					Node node = nodes[index+neighbourOffsets[i]];
					
					if (node.walkable && node.area != area) {
						stack.Push (node);
						node.area = area;
					}
				}
			}
		}
		
		public override int[] InitialOpen (BinaryHeapM open, Int3 targetPosition, Int3 position, Path path, bool doOpen) {
			
			if (doOpen) {
				//Open (open,targetPosition,path);
			}
			
			return base.InitialOpen (open,targetPosition,position,path,doOpen);
		}
		
		
		public override void Open (NodeRunData nodeRunData, NodeRun nodeR, Int3 targetPosition, Path path) {
			
			BaseOpen (nodeRunData, nodeR, targetPosition, path);
			
			GridGraph graph = gridGraphs[indices >> 24];
			
			int[] neighbourOffsets = graph.neighbourOffsets;
			int[] neighbourCosts = graph.neighbourCosts;
			Node[] nodes = graph.nodes;
			
			int index = GetIndex ();//indices & 0xFFFFFF;
			
			for (int i=0;i<8;i++) {
				if (GetConnection (i)) {
				//if (((flags >> i) & 1) == 1) {
					
					Node node = nodes[index+neighbourOffsets[i]];
					
					if (!path.CanTraverse (node)) continue;
					
					NodeRun nodeR2 = node.GetNodeRun (nodeRunData);
					
					
					if (nodeR2.pathID != nodeRunData.pathID) {
						
						nodeR2.parent = nodeR;
						nodeR2.pathID = nodeRunData.pathID;
						
						nodeR2.cost = (uint)neighbourCosts[i];
						
						node.UpdateH (targetPosition,path.heuristic,path.heuristicScale, nodeR2);
						node.UpdateG (nodeR2,nodeRunData);
						
						nodeRunData.open.Add (nodeR2);
					
					} else {
						//If not we can test if the path from the current node to this one is a better one then the one already used
						uint tmpCost = (uint)neighbourCosts[i];//(current.costs == null || current.costs.Length == 0 ? costs[current.neighboursKeys[i]] : current.costs[current.neighboursKeys[i]]);
						
						if (nodeR.g+tmpCost+node.penalty
				+ path.GetTagPenalty(node.tags)
						  		< nodeR2.g) {
							nodeR2.cost = tmpCost;
							
							nodeR2.parent = nodeR;
							
							node.UpdateAllG (nodeR2,nodeRunData);
							
						}
						
						 else if (nodeR2.g+tmpCost+penalty
				+ path.GetTagPenalty(tags)
						         < nodeR.g) {//Or if the path from this node ("node") to the current ("current") is better
							/*bool contains = false;
							
							//[Edit, no one-way links between nodes in a single grid] Make sure we don't travel along the wrong direction of a one way link now, make sure the Current node can be accesed from the Node.
							/*for (int y=0;y<node.connections.Length;y++) {
								if (node.connections[y].endNode == this) {
									contains = true;
									break;
								}
							}
							
							if (!contains) {
								continue;
							}*/
							
							nodeR.parent = nodeR2;
							nodeR.cost = tmpCost;
							
							UpdateAllG (nodeR,nodeRunData);
						}
					}
				}
			}
			
		}
		
		public static void RemoveGridGraph (GridGraph graph) {
			if (gridGraphs == null) {
				return;
			}
			
			for (int i=0;i<gridGraphs.Length;i++) {
				if (gridGraphs[i] == graph) {
					if (gridGraphs.Length == 1) {
						gridGraphs = null;
						return;
					}
					
						
					for (int j=i+1;j<gridGraphs.Length;j++) {
						
						GridGraph gg = gridGraphs[j];
						
						if (gg.nodes != null) {
							for (int n=0;n<gg.nodes.Length;n++) {
								if (gg.nodes[n] != null)
									((GridNode)gg.nodes[n]).SetGridIndex (j-1);
							}
						}
					}
					
					GridGraph[] tmp = new GridGraph[gridGraphs.Length-1];
					for (int j=0;j<i;j++) {
						tmp[j] = gridGraphs[j];
					}
					for (int j=i+1;j<gridGraphs.Length;j++) {
						tmp[j-1] = gridGraphs[j];
					}
					return;
				}
			}
		}
		
		public static int SetGridGraph (GridGraph graph) {
			if (gridGraphs == null) {
				gridGraphs = new GridGraph[1];
			} else {
				
				for (int i=0;i<gridGraphs.Length;i++) {
					if (gridGraphs[i] == graph) {
						return i;
					}
				}
				
				GridGraph[] tmp = new GridGraph[gridGraphs.Length+1];
				for (int i=0;i<gridGraphs.Length;i++) {
					tmp[i] = gridGraphs[i];
				}
				gridGraphs = tmp;
			}
			
			gridGraphs[gridGraphs.Length-1] = graph;
			return gridGraphs.Length-1;
		}
	}
}

