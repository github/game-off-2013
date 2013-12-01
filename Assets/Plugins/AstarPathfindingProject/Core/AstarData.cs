//#define ASTAR_SINGLE_THREAD_OPTIMIZE
//#define ASTAR_FAST_NO_EXCEPTIONS //Needs to be enabled for the iPhone build setting Fast But No Exceptions to work.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using Pathfinding;
using Pathfinding.Util;

namespace Pathfinding {
	
	[System.Serializable]
	/** Stores the navigation graphs for the A* Pathfinding System.
	 * \ingroup relevant
	 * 
	 * An instance of this class is assigned to AstarPath.astarData, from it you can access all graphs loaded through the #graphs variable.\n
	 * This class also handles a lot of the high level serialization.
	 */
	public class AstarData {
		
		/** Shortcut to AstarPath.active */
		public AstarPath active {
			get {
				return AstarPath.active;
			}
		}
		
#region Fields
		[System.NonSerialized]
		public NavMeshGraph navmesh; 	/**< Shortcut to the first NavMeshGraph. Updated at scanning time. This is the only reference to NavMeshGraph in the core pathfinding scripts */
		
		[System.NonSerialized]
		public GridGraph gridGraph;		/**< Shortcut to the first GridGraph. Updated at scanning time. This is the only reference to GridGraph in the core pathfinding scripts */
		
		[System.NonSerialized]
		public PointGraph pointGraph;		/**< Shortcut to the first PointGraph. Updated at scanning time. This is the only reference to PointGraph in the core pathfinding scripts */
		
		[System.NonSerialized]
		/** Holds temporary path data for pathfinders.
		 * One array for every thread.
		 * Every array is itself an array with a number of NodeRun object of which there is one per node of.
		 * These objects holds the temporary path data, such as the G and H scores and the parent node.
		 * This is separate from the static path data, e.g connections between nodes.
		 * \see CreateNodeRuns
		 */
		public NodeRun[][] nodeRuns;
		
		/** All supported graph types. Populated through reflection search */
		public System.Type[] graphTypes = null;
		
#if ASTAR_FAST_NO_EXCEPTIONS
		/** Graph types to use when building with Fast But No Exceptions for iPhone.
		 * If you add any custom graph types, you need to add them to this hard-coded list.
		 */
		public static readonly System.Type[] DefaultGraphTypes = new System.Type[] {
			typeof(GridGraph),
			typeof(PointGraph),
			typeof(NavMeshGraph)
		};
#endif
		
		[System.NonSerialized]
		/** All graphs this instance holds.
		 * This will be filled only after deserialization has completed.
		 * May contain null entries if graph have been removed.
		 */
		public NavGraph[] graphs = new NavGraph[0];
		
		/** Links placed by the user in the scene view. */
		[System.NonSerialized]
		public UserConnection[] userConnections = new UserConnection[0];
		
		//Serialization Settings
		
		/** Has the data been reverted by an undo operation.
		 * Used by the editor's undo logic to check if the AstarData has been reverted by an undo operation and should be deserialized */
		public bool hasBeenReverted = false;
		
		[SerializeField]
		/** Serialized data for all graphs and settings.
		 */
		private byte[] data;
		
		public uint dataChecksum;
		
		/** Backup data if deserialization failed.
		 */
		public byte[] data_backup;
		
		/** Serialized data for cached startup */
		public byte[] data_cachedStartup;
		
		public byte[] revertData;
		
		/** Should graph-data be cached.
		 * Caching the startup means saving the whole graphs, not only the settings to an internal array (#data_cachedStartup) which can
		 * be loaded faster than scanning all graphs at startup. This is setup from the editor.
		 */
		public bool cacheStartup = false;
		
		public bool compress = false;
		
		//End Serialization Settings
		
#endregion
		
		public byte[] GetData () {
			return data;
		}
		
		public void SetData (byte[] data, uint checksum) {
			this.data = data;
			dataChecksum = checksum;
		}
		
		/** Loads the graphs from memory, will load cached graphs if any exists */
		public void Awake () {
			
			/* Set up default values, to not throw null reference errors */
			userConnections = new UserConnection[0];
			
			graphs = new NavGraph[0];
			/* End default values */
			
			if (cacheStartup && data_cachedStartup != null) {
				LoadFromCache ();
			} else {
				DeserializeGraphs ();
			}
		}
		
		[System.Obsolete]
		public void CollectNodes (int numTemporary) {
			/*int nodeCount = 0;
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i].nodes != null)
					nodeCount += graphs[i].nodes.Length;
			}
			
			nodes = new Node[nodeCount + numTemporary];
			int counter = 0;
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i].nodes != null) {
					Node[] gNodes = graphs[i].nodes;
					for (int j=0;j<gNodes.Length;j++, counter++) {
						nodes[counter] = gNodes[j];
						gNodes[j].nodeIndex = counter;
					}
				}
			}*/
		}
		
		public void AssignNodeIndices () {
			int counter = 0;
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null || graphs[i].nodes == null) continue;
				Node[] nodes = graphs[i].nodes;
				for (int j=0;j<nodes.Length;j++, counter++) {
					if (nodes[j] != null)
						nodes[j].SetNodeIndex(counter);
				}
			}
		}
		
		/** Creates the structure for holding temporary path data.
		 * The data is for example the G, H and F scores and the search tree.
		 * The number of nodeRuns must be no less than the number of nodes contained in all graphs.
		 * So after adding nodes, this MUST be called.\n
		 * Ideally, I would code an update function which reuses most of the previous ones instead of recreating it every time.
		 * \param numParallel Number of parallel threads which will use the data.
		 * \see #nodeRuns
		 * \see AstarPath.UpdatePathThreadInfoNodes
		 */
		public void CreateNodeRuns (int numParallel) {
			
			if (graphs == null) throw new System.Exception ("Cannot create NodeRuns when no graphs exist. (Scan and or Load graphs first)");
			
			int nodeCount = 0;
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] != null && graphs[i].nodes != null)
					nodeCount += graphs[i].nodes.Length;
			}
			
			AssignNodeIndices ();
			
			active.UpdatePathThreadInfoNodes ();
		}
		
		/** Updates shortcuts to the first graph of different types.
		 * Hard coding references to some graph types is not really a good thing imo. I want to keep it dynamic and flexible.
		 * But these references ease the use of the system, so I decided to keep them. It is the only reference to specific graph types in the pathfinding core.\n
		 */
		public void UpdateShortcuts () {
			navmesh = (NavMeshGraph)FindGraphOfType (typeof(NavMeshGraph));
			gridGraph = (GridGraph)FindGraphOfType (typeof(GridGraph));
			pointGraph = (PointGraph)FindGraphOfType (typeof(PointGraph));
		}
		
		public void LoadFromCache () {
			if (data_cachedStartup != null && data_cachedStartup.Length > 0) {
				//AstarSerializer serializer = new AstarSerializer (active);
				//DeserializeGraphs (serializer,data_cachedStartup);
				DeserializeGraphs (data_cachedStartup);
				
				GraphModifier.TriggerEvent (GraphModifier.EventType.PostCacheLoad);
			} else {
				Debug.LogError ("Can't load from cache since the cache is empty");
			}
		}
		
		public void SaveCacheData (Pathfinding.Serialization.SerializeSettings settings) {
			data_cachedStartup = SerializeGraphs (settings);
			cacheStartup = true;
		}
		
#region Serialization
		
		/** Serializes all graphs settings to a byte array.
		 * \see DeserializeGraphs(byte[])
		 */
		public byte[] SerializeGraphs () {
			return SerializeGraphs (Pathfinding.Serialization.SerializeSettings.Settings);
		}
		
		/** Main serializer function. */
		public byte[] SerializeGraphs (Pathfinding.Serialization.SerializeSettings settings) {
			uint checksum;
			return SerializeGraphs (settings, out checksum);
		}
		
		/** Main serializer function.
		 * Serializes all graphs to a byte array
		  * A similar function exists in the AstarEditor.cs script to save additional info */
		public byte[] SerializeGraphs (Pathfinding.Serialization.SerializeSettings settings, out uint checksum) {
			
			Pathfinding.Serialization.AstarSerializer sr = new Pathfinding.Serialization.AstarSerializer(this, settings);
			sr.OpenSerialize();
			SerializeGraphsPart (sr);
			byte[] bytes = sr.CloseSerialize();
			checksum = sr.GetChecksum ();
			return bytes;
		}
		
		/** Serializes common info to the serializer.
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 */
		public void SerializeGraphsPart (Pathfinding.Serialization.AstarSerializer sr) {
			sr.SerializeGraphs(graphs);
			sr.SerializeUserConnections (userConnections);
			sr.SerializeNodes();
			sr.SerializeExtraInfo();
		}
		
		/** Deserializes graphs from #data */
		public void DeserializeGraphs () {
			if (data != null) {
				DeserializeGraphs (data);
			}
		}
		
		/** Deserializes graphs from the specified byte array.
		 * If an error ocurred, it will try to deserialize using the old deserializer.
		 * A warning will be logged if all deserializers failed.
		  */
		public void DeserializeGraphs (byte[] bytes) {
			try {
				if (bytes != null) {
					Pathfinding.Serialization.AstarSerializer sr = new Pathfinding.Serialization.AstarSerializer(this);
					
					if (sr.OpenDeserialize(bytes)) {
						DeserializeGraphsPart (sr);
						sr.CloseDeserialize();
					} else {
						Debug.Log ("Invalid data file (cannot read zip). Trying to load with old deserializer (pre 3.1)...");
						AstarSerializer serializer = new AstarSerializer (active);
						DeserializeGraphs_oldInternal (serializer);
					}
				} else {
					throw new System.ArgumentNullException ("Bytes should not be null when passed to DeserializeGraphs");
				}
				active.DataUpdate ();
			} catch (System.Exception e) {
				Debug.LogWarning ("Caught exception while deserializing data.\n"+e);
				data_backup = bytes;
			}
		}
		
		/** Deserializes graphs from the specified byte array additively.
		 * If an error ocurred, it will try to deserialize using the old deserializer.
		 * A warning will be logged if all deserializers failed.
		 * This function will add loaded graphs to the current ones
		  */
		public void DeserializeGraphsAdditive (byte[] bytes) {
			try {
				if (bytes != null) {
					Pathfinding.Serialization.AstarSerializer sr = new Pathfinding.Serialization.AstarSerializer(this);
					
					if (sr.OpenDeserialize(bytes)) {
						DeserializeGraphsPartAdditive (sr);
						sr.CloseDeserialize();
					} else {
						Debug.Log ("Invalid data file (cannot read zip).");
					}
				} else {
					throw new System.ArgumentNullException ("Bytes should not be null when passed to DeserializeGraphs");
				}
				active.DataUpdate ();
			} catch (System.Exception e) {
				Debug.LogWarning ("Caught exception while deserializing data.\n"+e);
			}
		}
		
		/** Deserializes common info.
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 */
		public void DeserializeGraphsPart (Pathfinding.Serialization.AstarSerializer sr) {
			graphs = sr.DeserializeGraphs ();
			userConnections = sr.DeserializeUserConnections();
			sr.DeserializeNodes();
			sr.DeserializeExtraInfo();
			sr.PostDeserialization();
		}
		
		/** Deserializes common info additively
		 * Common info is what is shared between the editor serialization and the runtime serializer.
		 * This is mostly everything except the graph inspectors which serialize some extra data in the editor
		 */
		public void DeserializeGraphsPartAdditive (Pathfinding.Serialization.AstarSerializer sr) {
			if (graphs == null) graphs = new NavGraph[0];
			if (userConnections == null) userConnections = new UserConnection[0];
			
			List<NavGraph> gr = new List<NavGraph>(graphs);
			gr.AddRange (sr.DeserializeGraphs ());
			graphs = gr.ToArray();
			
			List<UserConnection> conns = new List<UserConnection>(userConnections);
			conns.AddRange (sr.DeserializeUserConnections());
			userConnections = conns.ToArray ();
			sr.DeserializeNodes();
			sr.DeserializeExtraInfo();
			sr.PostDeserialization();
			
			for (int i=0;i<graphs.Length;i++) {
				for (int j=i+1;j<graphs.Length;j++) {
					if (graphs[i] != null && graphs[j] != null && graphs[i].guid == graphs[j].guid) {
						Debug.LogWarning ("Guid Conflict when importing graphs additively. Imported graph will get a new Guid.\nThis message is (relatively) harmless.");
						graphs[i].guid = Pathfinding.Util.Guid.NewGuid ();
						break;
					}
				}
			}
		}
		
#region OldSerializer
		
		/** Main deserializer function (old), loads from the #data variable \deprecated */
		[System.Obsolete("This function is obsolete. Use DeserializeGraphs () instead")]
		public void DeserializeGraphs (AstarSerializer serializer) {
			DeserializeGraphs_oldInternal (serializer);
		}
		
		/** Main deserializer function (old), loads from the #data variable \deprecated */
		public void DeserializeGraphs_oldInternal (AstarSerializer serializer) {
			DeserializeGraphs_oldInternal (serializer, data);
		}
		
		/** Main deserializer function (old). Loads from \a bytes variable \deprecated */
		[System.Obsolete("This function is obsolete. Use DeserializeGraphs (bytes) instead")]
		public void DeserializeGraphs (AstarSerializer serializer, byte[] bytes) {
			DeserializeGraphs_oldInternal (serializer, bytes);
		}
		
		/** Main deserializer function (old). Loads from \a bytes variable \deprecated */
		public void DeserializeGraphs_oldInternal (AstarSerializer serializer, byte[] bytes) {
			
			System.DateTime startTime = System.DateTime.UtcNow;
			
			if (bytes == null || bytes.Length == 0) {
				Debug.Log ("No previous data, assigning default");
				graphs = new NavGraph[0];
				return;
			}
			
			Debug.Log ("Deserializing...");
			
			serializer = serializer.OpenDeserialize (bytes);
			
			DeserializeGraphsPart (serializer);
			
			serializer.Close ();
			
			System.DateTime endTime = System.DateTime.UtcNow;
			Debug.Log ("Deserialization complete - Process took "+((endTime-startTime).Ticks*0.0001F).ToString ("0.00")+" ms");
		}
		
		/** Deserializes all graphs and also user connections \deprecated */
		public void DeserializeGraphsPart (AstarSerializer serializer) {
			
			if (serializer.error != AstarSerializer.SerializerError.Nothing) {
				data_backup = (serializer.readerStream.BaseStream as System.IO.MemoryStream).ToArray ();
				Debug.Log ("Error encountered : "+serializer.error+"\nWriting data to AstarData.data_backup");
				graphs = new NavGraph[0];
				return;
			}
			
			try {
				int count1 = serializer.readerStream.ReadInt32 ();
				int count2 = serializer.readerStream.ReadInt32 ();
				
				if (count1 != count2) {
					Debug.LogError ("Data is corrupt ("+count1 +" != "+count2+")");
					graphs = new NavGraph[0];
					return;
				}
				
				NavGraph[] _graphs = new NavGraph[count1];
				//graphs = new NavGraph[count1];
				
				for (int i=0;i<_graphs.Length;i++) {
					
					if (!serializer.MoveToAnchor ("Graph"+i)) {
						Debug.LogError ("Couldn't find graph "+i+" in the data");
						Debug.Log ("Logging... "+serializer.anchors.Count);
						foreach (KeyValuePair<string,int> value in serializer.anchors) {
							Debug.Log ("KeyValuePair "+value.Key);
						}
						_graphs[i] = null;
						continue;
					}
					string graphType = serializer.readerStream.ReadString ();
					
					//Old graph naming
					graphType = graphType.Replace ("ListGraph","PointGraph");
					
					Guid guid = new Guid (serializer.readerStream.ReadString ());
					
					//Search for existing graphs with the same GUID. If one is found, that means that we are loading another version of that graph
					//Use that graph then and just load it with some new settings
					NavGraph existingGraph = GuidToGraph (guid);
					
					if (existingGraph != null) {
						_graphs[i] = existingGraph;
						//Replace
						//graph.guid = new System.Guid ();
						//serializer.loadedGraphGuids[i] = graph.guid.ToString ();
					} else {
						_graphs[i] = CreateGraph (graphType);
					}
					
					NavGraph graph = _graphs[i];
					
					if (graph == null) {
						Debug.LogError ("One of the graphs saved was of an unknown type, the graph was of type '"+graphType+"'");
						data_backup = data;
						graphs = new NavGraph[0];
						return;
					}
					
					_graphs[i].guid = guid;
					
					//Set an unique prefix for all variables in this graph
					serializer.sPrefix = i.ToString ();
					serializer.DeSerializeSettings (graph,active);
				}
				
				serializer.SetUpGraphRefs (_graphs);
				
	
				for (int i=0;i<_graphs.Length;i++) {
					
					NavGraph graph = _graphs[i];
					
					if (serializer.MoveToAnchor ("GraphNodes_Graph"+i)) {
						serializer.mask = serializer.readerStream.ReadInt32 ();
						serializer.sPrefix = i.ToString ()+"N";
						serializer.DeserializeNodes (graph,_graphs,i,active);
						serializer.sPrefix = "";
					}
					
					//Debug.Log ("Graph "+i+" has loaded "+(graph.nodes != null ? graph.nodes.Length.ToString () : "null")+" nodes");
					
				}
				
				userConnections = serializer.DeserializeUserConnections ();
				
				//Remove null graphs
				List<NavGraph> tmp = new List<NavGraph>(_graphs);
				for (int i=0;i<_graphs.Length;i++) {
					if (_graphs[i] == null) {
						tmp.Remove (_graphs[i]);
					}
				}
				
				
				graphs = tmp.ToArray ();
			} catch (System.Exception e) {
				data_backup = (serializer.readerStream.BaseStream as System.IO.MemoryStream).ToArray ();
				Debug.LogWarning ("Deserializing Error Encountered - Writing data to AstarData.data_backup:\n"+e.ToString ());
				graphs = new NavGraph[0];
				return;
			}
		}
#endregion
		
#endregion
		
		/** Find all graph types supported in this build.
		 * Using reflection, the assembly is searched for types which inherit from NavGraph. */
		public void FindGraphTypes () {
			
#if !ASTAR_FAST_NO_EXCEPTIONS
			System.Reflection.Assembly asm = System.Reflection.Assembly.GetAssembly (typeof(AstarPath));
			
			System.Type[] types = asm.GetTypes ();
			
			List<System.Type> graphList = new List<System.Type> ();
			
			foreach (System.Type type in types) {
				
				System.Type baseType = type.BaseType;
				while (baseType != null) {
					
					if (baseType == typeof(NavGraph)) {
						
						graphList.Add (type);
						
						break;
					}
					
					baseType = baseType.BaseType;
				}
			}
			
			graphTypes = graphList.ToArray ();
			
#else		
			graphTypes = DefaultGraphTypes;
#endif
		}
		
#region GraphCreation
		/** \returns A System.Type which matches the specified \a type string. If no mathing graph type was found, null is returned */
		public System.Type GetGraphType (string type) {
			for (int i=0;i<graphTypes.Length;i++) {
				
				if (graphTypes[i].Name == type) {
					return graphTypes[i];
				}
			}
			return null;
		}
		
		/** Creates a new instance of a graph of type \a type. If no matching graph type was found, an error is logged and null is returned
		 * \returns The created graph 
		 * \see CreateGraph(System.Type) */
		public NavGraph CreateGraph (string type) {
			Debug.Log ("Creating Graph of type '"+type+"'");
			
			for (int i=0;i<graphTypes.Length;i++) {
				
				if (graphTypes[i].Name == type) {
					return CreateGraph (graphTypes[i]);
				}
			}
			Debug.LogError ("Graph type ("+type+") wasn't found");
			return null;
		}
		
		/** Creates a new graph instance of type \a type
		 * \see CreateGraph(string) */
		public NavGraph CreateGraph (System.Type type) {
			NavGraph g = System.Activator.CreateInstance (type) as NavGraph;
			g.active = active;
			return g;
		}
		
		/** Adds a graph of type \a type to the #graphs array */
		public NavGraph AddGraph (string type) {
			NavGraph graph = null;
			
			for (int i=0;i<graphTypes.Length;i++) {
				
				if (graphTypes[i].Name == type) {
					graph = CreateGraph (graphTypes[i]);
				}
			}
			
			if (graph == null) {
				Debug.LogError ("No NavGraph of type '"+type+"' could be found");
				return null;
			}
			
			AddGraph (graph);
			
			return graph;
		}
		
		/** Adds a graph of type \a type to the #graphs array */
		public NavGraph AddGraph (System.Type type) {
			NavGraph graph = null;
			
			for (int i=0;i<graphTypes.Length;i++) {
				
				if (graphTypes[i] == type) {
					graph = CreateGraph (graphTypes[i]);
				}
			}
			
			if (graph == null) {
				Debug.LogError ("No NavGraph of type '"+type+"' could be found, "+graphTypes.Length+" graph types are avaliable");
				return null;
			}
			
			AddGraph (graph);
			
			return graph;
		}
		
		/** Adds the specified graph to the #graphs array */
		public void AddGraph (NavGraph graph) {
			
			//Try to fill in an empty position
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) {
					graphs[i] = graph;
					return;
				}
			}
			
			//Add a new entry to the list
			List<NavGraph> ls = new List<NavGraph> (graphs);
			ls.Add (graph);
			graphs = ls.ToArray ();
		}
		
		/** Removes the specified graph from the #graphs array and Destroys it in a safe manner.
		 * To avoid changing graph indices for the other graphs, the graph is simply nulled in the array instead
		 * of actually removing it from the array.
		 * The empty position will be reused if a new graph is added.
		 * 
		 * \returns True if the graph was sucessfully removed (i.e it did exist in the #graphs array). False otherwise.
		 * 
		 * \see NavGraph.SafeOnDestroy
		 * 
		 * \version Changed in 3.2.5 to call SafeOnDestroy before removing
		 * and nulling it in the array instead of removing the element completely in the #graphs array.
		 * 
		 */
		public bool RemoveGraph (NavGraph graph) {
			
			//Safe OnDestroy is called since there is a risk that the pathfinding is searching through the graph right now,
			//and if we don't wait until the search has completed we could end up with evil NullReferenceExceptions
			graph.SafeOnDestroy ();
			
			int i=0;
			for (;i<graphs.Length;i++) if (graphs[i] == graph) break;
			if (i == graphs.Length) {
				return false;
			}
			
			graphs[i] = null;
			return true;
		}
		
#endregion
		
#region GraphUtility
		
		/** Returns the graph which contains the specified node. The graph must be in the #graphs array.
		 * \returns Returns the graph which contains the node. Null if the graph wasn't found */
		public static NavGraph GetGraph (Node node) {
			
			if (node == null) return null;
			
			AstarPath script = AstarPath.active;
			
			if (script == null) return null;
			
			AstarData data = script.astarData;
			
			if (data == null) return null;
			
			if (data.graphs == null) return null;
			
			int graphIndex = node.graphIndex;
			
			if (graphIndex < 0 || graphIndex >= data.graphs.Length) {
				return null;
			}
			
			return data.graphs[graphIndex];
		}
		
		/** Returns the node at \a graphs[graphIndex].nodes[nodeIndex]. All kinds of error checking is done to make sure no exceptions are thrown. */
		public Node GetNode (int graphIndex, int nodeIndex) {
			return GetNode (graphIndex,nodeIndex, graphs);
		}
		
		/** Returns the node at \a graphs[graphIndex].nodes[nodeIndex]. The graphIndex refers to the specified graphs array.\n
		 * All kinds of error checking is done to make sure no exceptions are thrown */
		public Node GetNode (int graphIndex, int nodeIndex, NavGraph[] graphs) {
			
			if (graphs == null) {
				return null;
			}
			
			if (graphIndex < 0 || graphIndex >= graphs.Length) {
				Debug.LogError ("Graph index is out of range"+graphIndex+ " [0-"+(graphs.Length-1)+"]");
				return null;
			}
			
			NavGraph graph = graphs[graphIndex];
			
			if (graph.nodes == null) {
				return null;
			}
			
			if (nodeIndex < 0 || nodeIndex >= graph.nodes.Length) {
				Debug.LogError ("Node index is out of range : "+nodeIndex+ " [0-"+(graph.nodes.Length-1)+"]"+" (graph "+graphIndex+")");
				return null;
			}
			
			return graph.nodes[nodeIndex];
		}
		
		/** Returns the first graph of type \a type found in the #graphs array. Returns null if none was found */
		public NavGraph FindGraphOfType (System.Type type) {
			
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] != null && graphs[i].GetType () == type) {
					return graphs[i];
				}
			}
			return null;
		}
		
		/** Loop through this function to get all graphs of type 'type' 
		 * \code foreach (GridGraph graph in AstarPath.astarData.FindGraphsOfType (typeof(GridGraph))) {
		 * 	//Do something with the graph
		 * } \endcode
		 * \see AstarPath.RegisterSafeNodeUpdate */
		public IEnumerable FindGraphsOfType (System.Type type) {
			if (graphs == null) { yield break; }
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] != null && graphs[i].GetType () == type) {
					yield return graphs[i];
				}
			}
		}
		
		/** All graphs which implements the UpdateableGraph interface
		 * \code foreach (IUpdatableGraph graph in AstarPath.astarData.GetUpdateableGraphs ()) {
		 * 	//Do something with the graph
		 * } \endcode
		 * \see AstarPath.RegisterSafeNodeUpdate
		 * \see Pathfinding.IUpdatableGraph */
		public IEnumerable GetUpdateableGraphs () {
			if (graphs == null) { yield break; }
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] != null && graphs[i] is IUpdatableGraph) {
					yield return graphs[i];
				}
			}
		}
		
		/** All graphs which implements the UpdateableGraph interface
		  * \code foreach (IRaycastableGraph graph in AstarPath.astarData.GetRaycastableGraphs ()) {
		 * 	//Do something with the graph
		 * } \endcode
		 * \see Pathfinding.IRaycastableGraph*/
		public IEnumerable GetRaycastableGraphs () {
			if (graphs == null) { yield break; }
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] != null && graphs[i] is IRaycastableGraph) {
					yield return graphs[i];
				}
			}
		}
		
		/** Gets the index of the NavGraph in the #graphs array */
		public int GetGraphIndex (NavGraph graph) {
			if (graph == null) throw new System.ArgumentNullException ("graph");
			
			for (int i=0;i<graphs.Length;i++) {
				if (graph == graphs[i]) {
					return i;
				}
			}
			Debug.LogError ("Graph doesn't exist");
			return -1;
		}
	
		/** Tries to find a graph with the specified GUID in the #graphs array.
		 * If a graph is found it returns its index, otherwise it returns -1
		 * \see GuidToGraph */
		public int GuidToIndex (Guid guid) {
			
			if (graphs == null) {
				return -1;
				//CollectGraphs ();
			}
			
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) {
					continue;
				}
				if (graphs[i].guid == guid) {
					return i;
				}
			}
			return -1;
		}
		
		/** Tries to find a graph with the specified GUID in the #graphs array. Returns null if none is found
		 * \see GuidToIndex */
		public NavGraph GuidToGraph (Guid guid) {
			
			if (graphs == null) {
				return null;
				//CollectGraphs ();
			}
			
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) {
					continue;
				}
				if (graphs[i].guid == guid) {
					return graphs[i];
				}
			}
			return null;
		}
		
		#endregion
		
	}
}