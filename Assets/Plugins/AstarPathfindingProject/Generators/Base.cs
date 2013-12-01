using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Util;
using Pathfinding.Serialization.JsonFx;

namespace Pathfinding {
	/// <summary>
	/// Base class for all graphs
	/// </summary>
	public abstract class NavGraph {
		
		/** Used to store the guid value
		 * \see NavGraph.guid
		 */
		public byte[] _sguid;
		
		
		/** Reference to the AstarPath object in the scene.
		 * Might not be entirely safe to use, it's better to use AstarPath.active
		 */
		public AstarPath active;
		
		/** Used as an ID of the graph, considered to be unique.
		 * \note This is Pathfinding.Util.Guid not System.Guid. A replacement for System.Guid was coded for better compatibility with iOS
		 */
		[JsonMember]
		public Guid guid {
			get {
				if (_sguid == null || _sguid.Length != 16) {
					_sguid = Guid.NewGuid ().ToByteArray ();
				}
				return new Guid (_sguid);
			}
			set {
				_sguid = value.ToByteArray ();
			}
		}
		
		[JsonMember]
		public uint initialPenalty = 0;
		
		/// <summary>
		/// Is the graph open in the editor
		/// </summary>
		[JsonMember]
		public bool open;
		
		[JsonMember]
		public string name;
		
		[JsonMember]
		public bool drawGizmos = true;
		
//#if UNITY_EDITOR
		/** Used in the editor to check if the info screen is open.
		 * Should be inside UNITY_EDITOR only \#ifs but just in case anyone tries to serialize a NavGraph instance using Unity, I have left it like this as it would otherwise cause a crash when building.
		 * Version 3.0.8.1 was released because of this bug only
		 */
		[JsonMember]
		public bool infoScreenOpen;
//#endif
		
		/** All nodes this graph contains. This can be iterated to search for a specific node.
		 * This should be set if the graph does contain any nodes.
		 * \note Entries are permitted to be NULL, make sure you account for that when iterating a graph's nodes
		 */
		public Node[] nodes;
		
		
		/** A matrix for translating/rotating/scaling the graph.
		 * Not all graph generators sets this variable though.
		 */
		public Matrix4x4 matrix;
		
		public Matrix4x4 inverseMatrix {
			get { return matrix.inverse; }
		}
			
		/** Creates a number of nodes with the correct type for the graph.
		This should not set the #nodes array, only return the nodes.
		Called by graph generators and when deserializing a graph with nodes.
		Override this function if you do not use the default Pathfinding.Node class.
		*/
		public virtual Node[] CreateNodes (int number) {
			Node[] tmp = new Node[number];
			for (int i=0;i<number;i++) {
				tmp[i] = new Node ();
				tmp[i].penalty = initialPenalty;
			}
			return tmp;
		}
		
		/** Relocates the nodes in this graph.
		 * Assumes the nodes are translated using the "oldMatrix", then translates them according to the "newMatrix".
		 * The "oldMatrix" is not required by all implementations of this function though (e.g the NavMesh generator).
		 * \bug Does not always work for Grid Graphs, see http://www.arongranberg.com/forums/topic/relocate-nodes-fix/
		 */
		public virtual void RelocateNodes (Matrix4x4 oldMatrix, Matrix4x4 newMatrix) {
			
			if (nodes == null || nodes.Length == 0) {
				return;
			}
			
			Matrix4x4 inv = oldMatrix.inverse;
			Matrix4x4 m = inv * newMatrix;
			
			for (int i=0;i<nodes.Length;i++) {
				//Vector3 tmp = inv.MultiplyPoint3x4 ((Vector3)nodes[i].position);
				nodes[i].position = (Int3)m.MultiplyPoint ((Vector3)nodes[i].position);
			}
			this.matrix = newMatrix;
		}
		
		/** Returns the nearest node to a position using the default NNConstraint.
		  * \param position The position to try to find a close node to
		  * \see Pathfinding.NNConstraint.None
		  */
		public NNInfo GetNearest (Vector3 position) {
			return GetNearest (position, NNConstraint.None);
		}
		
		/** Returns the nearest node to a position using the specified NNConstraint.
		  * \param position The position to try to find a close node to
		  * \param constraint Can for example tell the function to try to return a walkable node. If you do not get a good node back, consider calling GetNearestForce. */
		public NNInfo GetNearest (Vector3 position, NNConstraint constraint) {
			return GetNearest (position, constraint, null);
		}
		
		/** Returns the nearest node to a position using the specified NNConstraint.
		  * \param position The position to try to find a close node to
		  * \param hint Can be passed to enable some graph generators to find the nearest node faster.
		  * \param constraint Can for example tell the function to try to return a walkable node. If you do not get a good node back, consider calling GetNearestForce. */
		public virtual NNInfo GetNearest (Vector3 position, NNConstraint constraint, Node hint) {
			//Debug.LogError ("This function (GetNearest) is not implemented in the navigation graph generator : Type "+this.GetType ().Name);
			
			if (nodes == null) {
				return new NNInfo ();
			}
			
			float maxDistSqr = constraint.constrainDistance ? AstarPath.active.maxNearestNodeDistanceSqr : float.PositiveInfinity;
			
			float minDist = float.PositiveInfinity;
			Node minNode = null;
			
			float minConstDist = float.PositiveInfinity;
			Node minConstNode = null;
			
			for (int i=0;i<nodes.Length;i++) {
				
				Node node = nodes[i];
				float dist = (position-(Vector3)node.position).sqrMagnitude;
				
				if (dist < minDist) {
					minDist = dist;
					minNode = node;
				}
				
				if (dist < minConstDist && dist < maxDistSqr && constraint.Suitable (node)) {
					minConstDist = dist;
					minConstNode = node;
				}
			}
			
			NNInfo nnInfo = new NNInfo (minNode);
			
			nnInfo.constrainedNode = minConstNode;
			
			if (minConstNode != null) {
				nnInfo.constClampedPosition = (Vector3)minConstNode.position;
			} else if (minNode != null) {
				nnInfo.constrainedNode = minNode;
				nnInfo.constClampedPosition = (Vector3)minNode.position;
			}
			
			return nnInfo;
		}
		
		/// <summary>
		/// Returns the nearest node to a position using the specified <see cref="NNConstraint">constraint</see>.
		/// </summary>
		/// <param name="position">
		/// A <see cref="Vector3"/>
		/// </param>
		/// <param name="constraint">
		/// A <see cref="NNConstraint"/>
		/// </param>
		/// <returns>
		/// A <see cref="NNInfo"/>. This function will only return an empty NNInfo if there is no nodes which comply with the specified constraint.
		/// </returns>
		public virtual NNInfo GetNearestForce (Vector3 position, NNConstraint constraint) {
			return GetNearest (position, constraint);
			//Debug.LogError ("This should not be called if not GetNearest has been overriden, and if GetNearest has been overriden, you should override this function too, always return a node which returns true when passed to constraint.Suitable (node)");
			//return new NNInfo ();
		}
		
		/// <summary>
		/// This will be called on the same time as Awake on the gameObject which the AstarPath script is attached to. (remember, not in the editor)
		/// Use this for any initialization code which can't be placed in Scan
		/// </summary>
		public virtual void Awake () {
		}
		
		/// <summary>
		/// SafeOnDestroy should be used when there is a risk that the pathfinding is searching through this graph when called
		/// </summary>
		public void SafeOnDestroy () {
			AstarPath.RegisterSafeUpdate (OnDestroy,false);
		}
		
		/// <summary>
		/// Function for cleaning up references.
		/// </summary>
		/// <remarks>This will be called on the same time as OnDisable on the gameObject which the AstarPath script is attached to (remember, not in the editor)
		/// Use for any cleanup code such as cleaning up static variables which otherwise might prevent resources from being collected
		/// Use by creating a function overriding this one in a graph class, but always call base.OnDestroy () in that function.</remarks>
		public virtual void OnDestroy () {
			//Clean up a refence to the node array so it can get collected even if a reference to this graph still exists somewhere
			nodes = null;
		}
		
		/// <summary>
		/// Consider using AstarPath.Scan () instead since this function might screw things up if there is more than one graph.
		/// This function does not perform all necessary postprocessing for the graph to work with pathfinding (e.g flood fill).
		/// See the source of the AstarPath.Scan function to see how it can be used.
		/// 
		/// In almost all cases you should use AstarPath.Scan instead.
		/// </summary>
		public void ScanGraph () {
			
			if (AstarPath.OnPreScan != null) {
				AstarPath.OnPreScan (AstarPath.active);
			}
			
			if (AstarPath.OnGraphPreScan != null) {
				AstarPath.OnGraphPreScan (this);
			}
			
			Scan ();
			
			if (AstarPath.OnGraphPostScan != null) {
				AstarPath.OnGraphPostScan (this);
			}
			
			if (AstarPath.OnPostScan != null) {
				AstarPath.OnPostScan (AstarPath.active);
			}
		}
		
		/// <summary>
		/// Scans the graph, called from <see cref="AstarPath.Scan"/>
		/// Override this function to implement custom scanning logic
		/// </summary>
		public abstract void Scan ();
		
		/* Color to use for gizmos.
		 * Returns a color to be used for the specified node with the current debug settings (editor only)
		 */
		public virtual Color NodeColor (Node node, NodeRunData data) {
			
			Color c = AstarColor.NodeConnection;
			bool colSet = false;
			
			if (node == null) return AstarColor.NodeConnection;
			
			switch (AstarPath.active.debugMode) {
				case GraphDebugMode.Areas:
					c = AstarColor.GetAreaColor (node.area);
					colSet = true;
					break;
				case GraphDebugMode.Penalty:
					c = Color.Lerp (AstarColor.ConnectionLowLerp,AstarColor.ConnectionHighLerp, (float)node.penalty / (float)AstarPath.active.debugRoof);
					colSet = true;
					break;
				case GraphDebugMode.Tags:
					c = Mathfx.IntToColor (node.tags,0.5F);
					colSet = true;
					break;
				
				/* Wasn't really usefull
				case GraphDebugMode.Position:
					float r = Mathf.PingPong (node.position.x/10000F,1F) + Mathf.PingPong (node.position.x/300000F,1F);
					float g = Mathf.PingPong (node.position.y/10000F,1F) + Mathf.PingPong (node.position.y/200000F,1F);
					float b = Mathf.PingPong (node.position.z/10000F,1F) + Mathf.PingPong (node.position.z/100000F,1F);
					
					
					c = new Color (r,g,b);
					break;
				*/
			}
			
			if (!colSet) {
				if (data == null) return AstarColor.NodeConnection;
				
				NodeRun nodeR = node.GetNodeRun (data);
				
				if (nodeR == null) return AstarColor.NodeConnection;
				
				switch (AstarPath.active.debugMode) {
					case GraphDebugMode.G:
						//c = Mathfx.IntToColor (node.g,0.5F);
						c = Color.Lerp (AstarColor.ConnectionLowLerp,AstarColor.ConnectionHighLerp, (float)nodeR.g / (float)AstarPath.active.debugRoof);
						break;
					case GraphDebugMode.H:
						c = Color.Lerp (AstarColor.ConnectionLowLerp,AstarColor.ConnectionHighLerp, (float)nodeR.h / (float)AstarPath.active.debugRoof);
						break;
					case GraphDebugMode.F:
						c = Color.Lerp (AstarColor.ConnectionLowLerp,AstarColor.ConnectionHighLerp, (float)nodeR.f / (float)AstarPath.active.debugRoof);
						break;
				}
			}
			c.a *= 0.5F;
			return c;
			
		}
		
		/** Serializes graph type specific node data.
		 * This function can be overriden to serialize extra node information (or graph information for that matter)
		 * which cannot be serialized using the standard serialization.
		 * Serialize the data in any way you want and return a byte array.
		 * When loading, the exact same byte array will be passed to the DeserializeExtraInfo function.\n
		 * These functions will only be called if node serialization is enabled.\n
		 * If null is returned from this function, the DeserializeExtraInfo function will not be called on load.
		 */
		public virtual byte[] SerializeExtraInfo () {
			return null;
		}
		
		/** Deserializes graph type specific node data.
		 * \see SerializeExtraInfo
		 */
		public virtual void DeserializeExtraInfo (byte[] bytes) {
		}
		
		/** Called after all deserialization has been done for all graphs.
		 * Can be used to set up more graph data which is not serialized
		 */
		public virtual void PostDeserialization () {
		}
		
		/** Returns if the node is in the search tree of the path.
		 * Only guaranteed to be correct if \a path is the latest path calculated.
		 * Use for gizmo drawing only.
		 */
		public bool InSearchTree (Node node, Path path) {
			if (path == null || path.runData == null) return true;
			NodeRun nodeR = node.GetNodeRun (path.runData);
			return nodeR.pathID == path.pathID;
		}
		
		public virtual void OnDrawGizmos (bool drawNodes) {
			
			if (nodes == null || !drawNodes) {
				if (!Application.isPlaying) {
					//Scan (0);
				}
				return;
			}
			
			for (int i=0;i<nodes.Length;i++) {
				Node node = nodes[i];
				
				if (node.connections != null) {
					
					Gizmos.color = NodeColor (node, AstarPath.active.debugPathData);
					if (AstarPath.active.showSearchTree && !InSearchTree(node,AstarPath.active.debugPath)) return;
					
					if (AstarPath.active.showSearchTree && AstarPath.active.debugPathData != null && node.GetNodeRun(AstarPath.active.debugPathData).parent != null) {
						Gizmos.DrawLine ((Vector3)node.position,(Vector3)node.GetNodeRun(AstarPath.active.debugPathData).parent.node.position);
					} else {
						for (int q=0;q<node.connections.Length;q++) {
							Gizmos.DrawLine ((Vector3)node.position,(Vector3)node.connections[q].position);
						}
					}
				}
			}
		}
	}
	

	[System.Serializable]
	/** Handles collision checking for graphs.
	  * Mostly used by grid based graphs */
	public class GraphCollision : ISerializableObject {
		
		/** Collision shape to use.
		  * Pathfinding.ColliderType */
		public ColliderType type = ColliderType.Capsule;
		
		/** Diameter of capsule or sphere when checking for collision.
		 * 1 equals \link Pathfinding.GridGraph.nodeSize nodeSize \endlink.
		 * If #type is set to Ray, this does not affect anything */
		public float diameter = 1F;
		
		/** Height of capsule or length of ray when checking for collision.
		 * If #type is set to Sphere, this does not affect anything
		 */
		public float height = 2F;
		public float collisionOffset = 0;
		
		/** Direction of the ray when checking for collision.
		 * If #type is not Ray, this does not affect anything
		 * \note This variable is not used currently, it does not affect anything
		 */
		public RayDirection rayDirection = RayDirection.Both;
		
		/** Layer mask to use for collision check.
		 * This should only contain layers of objects defined as obstacles */
		public LayerMask mask;
		
		/** Layer mask to use for height check. */
		public LayerMask heightMask = -1;
		
		/** The height to check from when checking height */
		public float fromHeight = 100;
		
		/** Toggles thick raycast */
		public bool thickRaycast = false;
		
		/** Diameter of the thick raycast in nodes.
		 * 1 equals \link Pathfinding.GridGraph.nodeSize nodeSize \endlink */
		public float thickRaycastDiameter = 1;
		
		/** Direction to use as \a UP.
		 * \see Initialize */
		public Vector3 up;
		
		/** #up * #height.
		 * \see Initialize */
		private Vector3 upheight;
		
		/** #diameter * scale * 0.5.
		 * Where \a scale usually is \link Pathfinding.GridGraph.nodeSize nodeSize \endlink
		 * \see Initialize */
		private float finalRadius;
		
		/** #thickRaycastDiameter * scale * 0.5. Where \a scale usually is \link Pathfinding.GridGraph.nodeSize nodeSize \endlink \see Initialize */
		private float finalRaycastRadius;
		
		/** Offset to apply after each raycast to make sure we don't hit the same point again in CheckHeightAll */
		public const float RaycastErrorMargin = 0.005F;
		
		public bool collisionCheck = true; /**< Toggle collision check */
		public bool heightCheck = true; /**< Toggle height check. If false, the grid will be flat */
	
		/** Make nodes unwalkable when no ground was found with the height raycast. If height raycast is turned off, this doesn't affect anything. */
		public bool unwalkableWhenNoGround = true;

//#if !PhotonImplementation
		
		/** Sets up several variables using the specified matrix and scale.
		  * \see GraphCollision.up
		  * \see GraphCollision.upheight
		  * \see GraphCollision.finalRadius
		  * \see GraphCollision.finalRaycastRadius
		  */
		public void Initialize (Matrix4x4 matrix, float scale) {
			up = matrix.MultiplyVector (Vector3.up);
			upheight = up*height;
			finalRadius = diameter*scale*0.5F;
			finalRaycastRadius = thickRaycastDiameter*scale*0.5F;
		}
		
		/** Returns if the position is obstructed. If #collisionCheck is false, this will always return true.\n */
		public bool Check (Vector3 position) {
			
			if (!collisionCheck) {
				return true;
			}
			
			position += up*collisionOffset;
			
			switch (type) {
				case ColliderType.Capsule:
					return !Physics.CheckCapsule (position, position+upheight,finalRadius,mask);
				case ColliderType.Sphere:
					return !Physics.CheckSphere (position, finalRadius,mask);
				default:
					switch (rayDirection) {
						case RayDirection.Both:
							return !Physics.Raycast (position, up, height, mask) && !Physics.Raycast (position+upheight, -up, height, mask);
						case RayDirection.Up:
							return !Physics.Raycast (position, up, height, mask);
						default:
							return !Physics.Raycast (position+upheight, -up, height, mask);
					}
			}
		}
		
		/** Returns the position with the correct height. If #heightCheck is false, this will return \a position.\n */
		public Vector3 CheckHeight (Vector3 position) {
			RaycastHit hit;
			bool walkable;
			return CheckHeight (position,out hit, out walkable);
		}
		
		/** Returns the position with the correct height. If #heightCheck is false, this will return \a position.\n
		  * \a walkable will be set to false if nothing was hit. The ray will check a tiny bit further than to the grids base to avoid floating point errors when the ground is exactly at the base of the grid */
		public Vector3 CheckHeight (Vector3 position, out RaycastHit hit, out bool walkable) {
			walkable = true;
			
			if (!heightCheck) {
				hit = new RaycastHit ();
				return position;
			}
			
			if (thickRaycast) {
				Ray ray = new Ray (position+up*fromHeight,-up);
				if (Physics.SphereCast (ray, finalRaycastRadius,out hit, fromHeight+0.005F, heightMask)) {
					
					return Mathfx.NearestPoint (ray.origin,ray.origin+ray.direction,hit.point);
					//position+up*(fromHeight-hit.distance);
				} else {
					if (unwalkableWhenNoGround) {
						walkable = false;
					}
				}
			} else {
				if (Physics.Raycast (position+up*fromHeight, -up,out hit, fromHeight+0.005F, heightMask)) {
					return hit.point;
				} else {
					if (unwalkableWhenNoGround) {
						walkable = false;
					}
				}
			}
			return position;
		}
		
		/** Same as #CheckHeight, except that the raycast will always start exactly at \a origin
		  * \a walkable will be set to false if nothing was hit. The ray will check a tiny bit further than to the grids base to avoid floating point errors when the ground is exactly at the base of the grid */
		public Vector3 Raycast (Vector3 origin, out RaycastHit hit, out bool walkable) {
			walkable = true;
			
			if (!heightCheck) {
				hit = new RaycastHit ();
				return origin -up*fromHeight;
			}
			
			if (thickRaycast) {
				Ray ray = new Ray (origin,-up);
				if (Physics.SphereCast (ray, finalRaycastRadius,out hit, fromHeight+0.005F, heightMask)) {
					
					return Mathfx.NearestPoint (ray.origin,ray.origin+ray.direction,hit.point);
					//position+up*(fromHeight-hit.distance);
				} else {
					if (unwalkableWhenNoGround) {
						walkable = false;
					}
				}
			} else {
				if (Physics.Raycast (origin, -up,out hit, fromHeight+0.005F, heightMask)) {
					return hit.point;
				} else {
					if (unwalkableWhenNoGround) {
						walkable = false;
					}
				}
			}
			return origin -up*fromHeight;
		}
		
		//[System.Obsolete ("Does not work well, will only return an object a single time")]
		/** Returns all hits when checking height for \a position.
		  * \note Does not work well with thick raycast, will only return an object a single time */
		public RaycastHit[] CheckHeightAll (Vector3 position) {
			
			/*RaycastHit[] hits;
			
			if (!heightCheck) {
				RaycastHit hit = new RaycastHit ();
				hit.point = position;
				hit.distance = 0;
				return new RaycastHit[1] {hit};
			}
			
			
			if (thickRaycast) {
				Ray ray = new Ray (position+up*fromHeight,-up);
				
				hits = Physics.SphereCastAll (ray, finalRaycastRadius, fromHeight, heightMask);
					
				for (int i=0;i<hits.Length;i++) {
					hits[i].point = Mathfx.NearestPoint (ray.origin,ray.origin+ray.direction,hits[i].point);
					//position+up*(fromHeight-hit.distance);
				}
			} else {
				hits = Physics.RaycastAll (position+up*fromHeight, -up, fromHeight, heightMask);
			}
			return hits;*/
			
			if (!heightCheck) {
				RaycastHit hit = new RaycastHit ();
				hit.point = position;
				hit.distance = 0;
				return new RaycastHit[1] {hit};
			}
			
			if (thickRaycast) {
				Debug.LogWarning ("Thick raycast cannot be used with CheckHeightAll. Disabling thick raycast...");
				thickRaycast = false;
			}
			
			List<RaycastHit> hits = new List<RaycastHit>();
			
			bool walkable = true;
			Vector3 cpos = position + up*fromHeight;
			Vector3 prevHit = Vector3.zero;
			
			int numberSame = 0;
			while (true) {
				RaycastHit hit;
				Raycast (cpos, out hit, out walkable);
				if (hit.transform == null) { //Raycast did not hit anything
					break;
				} else {
					
					//Make sure we didn't hit the same position
					if (hit.point != prevHit || hits.Count == 0) {
						cpos = hit.point - up*RaycastErrorMargin;
						prevHit = hit.point;
						numberSame = 0;
						
						hits.Add (hit);
					} else {
						cpos -= up*0.001F;
						numberSame++;
						//Check if we are hitting the same position all the time, even though we are decrementing the cpos variable
						if (numberSame > 10) {
							Debug.LogError ("Infinite Loop when raycasting. Please report this error (arongranberg.com)\n"+cpos+" : "+prevHit);
							break;
						}
					}
				}
			}
			return hits.ToArray ();
		}
		
		/** \copydoc Pathfinding.ISerializableObject.SerializeSettings \copybrief Pathfinding.ISerializableObject.SerializeSettings */
		public void SerializeSettings (AstarSerializer serializer) {
			serializer.AddValue ("Mask",(int)mask);
			serializer.AddValue ("Diameter",diameter);
			serializer.AddValue ("Height",height);
			serializer.AddValue ("Type",(int)type);
			serializer.AddValue ("RayDirection",(int)rayDirection);
			
			serializer.AddValue ("heightMask",(int)heightMask);
			serializer.AddValue ("fromHeight",fromHeight);
			serializer.AddValue ("thickRaycastDiameter",thickRaycastDiameter);
			serializer.AddValue ("thickRaycast",thickRaycast);
			
			serializer.AddValue ("collisionCheck",collisionCheck);
			serializer.AddValue ("heightCheck",heightCheck);
			
			serializer.AddValue ("unwalkableWhenNoGround",unwalkableWhenNoGround);
			
			serializer.AddValue ("collisionOffset",collisionOffset);
		}
		
		/** \copydoc Pathfinding.ISerializableObject.DeSerializeSettings */
		public void DeSerializeSettings (AstarSerializer serializer) {
			mask.value = (int)serializer.GetValue ("Mask",typeof (int));
			diameter = (float)serializer.GetValue ("Diameter",typeof (float));
			height = (float)serializer.GetValue ("Height",typeof (float));
			type = (ColliderType)serializer.GetValue ("Type",typeof(int));
			rayDirection = (RayDirection)serializer.GetValue ("RayDirection",typeof(int));
			
			heightMask.value = (int)serializer.GetValue ("heightMask",typeof (int),-1);
			fromHeight = (float)serializer.GetValue ("fromHeight",typeof (float), 100.0F);
			thickRaycastDiameter = (float)serializer.GetValue ("thickRaycastDiameter",typeof (float));
			thickRaycast = (bool)serializer.GetValue ("thickRaycast",typeof (bool));
			
			collisionCheck = (bool)serializer.GetValue ("collisionCheck",typeof(bool),true);
			heightCheck = (bool)serializer.GetValue ("heightCheck",typeof(bool),true);
			
			unwalkableWhenNoGround = (bool)serializer.GetValue ("unwalkableWhenNoGround",typeof(bool),true);
			
			collisionOffset = (float)serializer.GetValue ("collisionOffset",typeof(float),0.0F);
			
			if (fromHeight == 0) fromHeight = 100;
			
			
		}
	}

	
	/** Determines collision check shape */
	public enum ColliderType {
		Sphere,		/**< Uses a Sphere, Physics.CheckSphere */
		Capsule,	/**< Uses a Capsule, Physics.CheckCapsule */
		Ray			/**< Uses a Ray, Physics.Linecast */
	}
	
	/** Determines collision check ray direction */
	public enum RayDirection {
		Up,	 	/**< Casts the ray from the bottom upwards */
		Down,	/**< Casts the ray from the top downwards */
		Both	/**< Casts two rays in either direction */
	}
}