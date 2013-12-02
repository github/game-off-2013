//#define ASTAR_PROFILE

using Pathfinding;
using Pathfinding.Util;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding
{
	/** Contains useful functions for working with paths and nodes.
	 * This class works a lot with the Node class, a useful function to get nodes is AstarPath.GetNearest.
	  * \see AstarPath.GetNearest
	  * \see Pathfinding.Utils.GraphUpdateUtilities
	  * \since Added in version 3.2
	  * \ingroup utils
	  * 
	  */
	public static class PathUtilities {
		/** Returns if there is a walkable path from \a n1 to \a n2.
		 * If you are making changes to the graph, areas must first be recaculated using FloodFill()
		 * \note This might return true for small areas even if there is no possible path if AstarPath.minAreaSize is greater than zero (0).
		 * So when using this, it is recommended to set AstarPath.minAreaSize to 0. (A* Inspector -> Settings -> Pathfinding)
		 * \see AstarPath.GetNearest
		 */
		public static bool IsPathPossible (Node n1, Node n2) {
			return n1.walkable && n2.walkable && n1.area == n2.area;
		}
		
		/** Returns if there are walkable paths between all nodes.
		 * If you are making changes to the graph, areas must first be recaculated using FloodFill()
		 * \note This might return true for small areas even if there is no possible path if AstarPath.minAreaSize is greater than zero (0).
		 * So when using this, it is recommended to set AstarPath.minAreaSize to 0. (A* Inspector -> Settings -> Pathfinding)
		 * \see AstarPath.GetNearest
		 */
		public static bool IsPathPossible (List<Node> nodes) {
			int area = nodes[0].area;
			for (int i=0;i<nodes.Count;i++) if (!nodes[i].walkable || nodes[i].area != area) return false;
			return true;
		}
		
		/** Returns all nodes reachable from the seed node.
		 * This function performs a BFS (breadth-first-search) or flood fill of the graph and returns all nodes which can be reached from
		 * the seed node. In almost all cases this will be identical to returning all nodes which have the same area as the seed node.
		 * In the editor areas are displayed as different colors of the nodes.
		 * The only case where it will not be so is when there is a one way path from some part of the area to the seed node
		 * but no path from the seed node to that part of the graph.
		 * 
		 * The returned list is sorted by node distance from the seed node
		 * i.e distance is measured in the number of nodes the shortest path from \a seed to that node would pass through.
		 * Note that the distance measurement does not take heuristics, penalties or tag penalties.
		 * 
		 * Depending on the number of reachable nodes, this function can take quite some time to calculate
		 * so don't use it too often or it might affect the framerate of your game.
		 * 
		 * \param seed The node to start the search from
		 * \param tagMask Optional mask for tags. This is a bitmask.
		 * 
		 * \returns A List<Node> containing all nodes reachable from the seed node.
		 * For better memory management the returned list should be pooled, see Pathfinding.Util.ListPool
		 */
		public static List<Node> GetReachableNodes (Node seed, int tagMask = -1) {
			Stack<Node> stack = Pathfinding.Util.StackPool<Node>.Claim ();
			List<Node> list = Pathfinding.Util.ListPool<Node>.Claim ();
			
			
			HashSet<Node> map = new HashSet<Node>();
			
			NodeDelegate callback;
			if (tagMask == -1) {
				callback = delegate (Node node) {
					if (node.walkable && map.Add (node)) {
						list.Add (node);
						stack.Push (node);
					}
				};
			} else {
				callback = delegate (Node node) {
					if (node.walkable && ((tagMask >> node.tags) & 0x1) != 0 && map.Add (node)) {
						list.Add (node);
						stack.Push (node);
					}
				};
			}
			
			callback (seed);
			
			while (stack.Count > 0) {
				stack.Pop ().GetConnections (callback);
			}
			
			Pathfinding.Util.StackPool<Node>.Release (stack);
			
			return list;
		}
	}
}

