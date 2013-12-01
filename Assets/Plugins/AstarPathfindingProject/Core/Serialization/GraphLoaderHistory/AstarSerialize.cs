using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
//using Pathfinding;

namespace Pathfinding {
	public class AstarSerializer {
	
		public BinaryWriter writerStream;
		public BinaryReader readerStream;
		
		public AstarPath active;
		public AstarData astarData;
		
		public bool replaceOldGraphs = true;
		
		/** Mask for what to save. The first 4 bits in this mask is used, the rest can be used to pass information on what was saved in the file
		  *Assign values with += or |=, and remove them with -= or &= ~value
		  * Values should be one bit values (unless you know what you are doing), such as 1 << 20 */
		public BitMask mask = -1 & ~(SMask.SaveNodes);
		
		/** Should version differences be ignored.
		 * This should not be enabled if you don't know what you are doing.
		 * It will enable older version of the project to try open files saved with a newer project version */
		public static bool IgnoreVersionDifferences = false;
		
		//public const int SaveNodes = 				1 << 0;
		//public const int SaveNodeConnections = 		1 << 1;
		//public const int SaveNodeConnectionCosts = 	1 << 2;
		//public const int SaveNodePositions = 		1 << 3;
		
		/** Serializer mask for what is to be saved in the file */
		public class SMask {
			public static int SaveNodes = 					1 << 0;
			public static int SaveNodeConnections = 		1 << 1;
			public static int SaveNodeConnectionCosts = 	1 << 2;
			public static int SaveNodePositions = 			1 << 3;
			public static int RunLengthEncoding = 			1 << 4;
			
			public static string BitName (int i) {
				switch (i) {
					case 0: return "Nodes";
					case 1: return "Node Connections";
					case 2: return "Node Connection Costs";
					case 3: return "Node Positions";
					case 4: return "Run Length Encoding";
				}
				return null;
			}
		}
		
		//public const int SaveSettings = 			1 << 4;
		
		public bool onlySaveSettings = false;
		public bool compress = true;
		
		//public string[] graphGUIDReferences;
		
		/** The GUIDs of the graphs saved with the file (only set in load) */
		public string[] loadedGraphGuids;
		
		/** The indices to the graphs in the new AstarData. Conversion from loaded indices (newGraphIndex = graphRefGuids[oldGraphIndex]) */
		public int[] graphRefGuids;
		
		public SerializerError error = SerializerError.Nothing;
		
		public Dictionary<string,int> anchors;
		
		public delegate UnityEngine.Object ReadUnityReference_Editor (AstarSerializer serializer, string name, int instanceID, System.Type type);
		public delegate void WriteUnityReference_Editor (AstarSerializer serializer, UnityEngine.Object ob);
		
		public static ReadUnityReference_Editor readUnityReference_Editor = null;
		public static WriteUnityReference_Editor writeUnityReference_Editor = null;
		
		/** Key, position */
		public Dictionary<string,int> serializedVariables = new Dictionary<string, int> ();
		
		public Hashtable serializedData;
		/** Prefix to use before variables. Used to avoid name collisions */
		public string sPrefix = "";
		public byte prefix = 0;
		
		public int counter;
		public int positionAtCounter = -1;
		public long positionAtError = -1;
		
		public enum SerializerError {
			Nothing,
			WrongMagic,
			WrongVersion,
			DoesNotExist
		}
		
		public static AstarSerializer GetDeserializer (Version version, AstarPath script) {
			if (version == AstarPath.Version) {
				return new AstarSerializer (script);
			} else if (version > AstarPath.Version) {
				//Higher version, trying to load
				return new AstarSerializer (script);
			} else {
				//Load older version
				if (version >= new Version (3,0,7)) {
					return new AstarSerializer3_07 (script);
				} else if (version >= new Version (3,0,5)) {
					return new AstarSerializer3_05 (script);
				} else if (version >= new Version (3,0,4)) {
					return new AstarSerializer3_04 (script);
				} else {
					return new AstarSerializer3_01 (script);
				}
				/*if (version > new Version (3,0,1)) {
					return new AstarSerializer3_04 (script);
				} else {
					return new AstarSerializer3_01 (script);
				}*/
			}
		}
		
		protected AstarSerializer () {}
		
		public AstarSerializer (AstarPath script) {
			active = script;
			astarData = script.astarData;
			mask = -1;
			mask -= SMask.SaveNodes;
		}
		
		public void SetUpGraphRefs (NavGraph[] graphs) {
			graphRefGuids = new int[loadedGraphGuids.Length];
			
			for (int i=0;i<loadedGraphGuids.Length;i++) {
				
				Pathfinding.Util.Guid guid = new Pathfinding.Util.Guid (loadedGraphGuids[i]);
				
				graphRefGuids[i] = -1;
				
				for (int j=0;j<graphs.Length;j++) {
					if (graphs[j].guid == guid) {
						graphRefGuids[i] = i;
					}
				}
				//graphRefGuids[i] = astarData.GuidToIndex ());
			}
		}
		
		/** This is intended for quick saving of settings for e.g Undo operations */
		public void OpenSerialize () {
			onlySaveSettings = true;
			MemoryStream fs = new MemoryStream ();
	        
	        BinaryWriter stream = new BinaryWriter (fs);
	        writerStream = stream;
	        
			//This will be overwritten by the anchor count in SerializeAnchors
	        writerStream.Write (0);
	        anchors = new Dictionary<string,int>();
			
			SerializeSerializationInfo ();
			
			InitializeSerializeNodes ();
		}
		
		/** Opens a deserialization session.
		 * This function returns an AstarSerializer,
		 * it will usually be the same as called on, but when loading from older graphs, it will have been replaced by another one.\n
		 * Use the returned AstarSerializer from now on */
		public AstarSerializer OpenDeserialize (byte[] data) {
			//onlySaveSettings = true;
			
			MemoryStream fs = new MemoryStream (data);
			
			readerStream = new BinaryReader (fs);
			
			DeserializeAnchors ();
			return DeserializeSerializationInfo ();
		}
		
		public void SerializeSerializationInfo () {
			AddAnchor ("SerializerSettings");
			BinaryWriter stream = writerStream;
			
			stream.Write (AstarPath.Version.ToString ());
			
			//Need to count graphs like this because graphs might be null in the array
			int countGraphs = 0;
			for (int i=0;i<astarData.graphs.Length;i++) if (astarData.graphs[i] != null) countGraphs++;
			
			stream.Write (countGraphs);
			
			for (int i=0;i<astarData.graphs.Length;i++) {
				if (astarData.graphs[i] != null) stream.Write (astarData.graphs[i].guid.ToString ());
			}
			stream.Write (mask);
		}
		
		/** Deserializes serialization info. Deserializes Version, mask and #loadedGraphGuids
		  * \see OpenDeserialize */
		public AstarSerializer DeserializeSerializationInfo () {
			if (!MoveToAnchor ("SerializerSettings")) {
				throw (new System.NullReferenceException ("Anchor SerializerSettings was not found in the data"));
			}
			
			BinaryReader stream = readerStream;
			
			System.Version astarVersion = null;
			
			try {
				astarVersion = new Version (stream.ReadString ());
			}
			
			catch (Exception e) {
				Debug.LogError ("Couldn't parse A* version ");
				error = SerializerError.WrongVersion;
				throw new System.FormatException ("Couldn't parse A* version",e);
			}
			
			//System.Version astarVersion2 = AstarPath.Version;
			
			AstarSerializer returnSerializer = this;
			
			if (!IgnoreVersionDifferences) {
				if (astarVersion > AstarPath.Version) {
					Debug.LogError ("Loading graph saved with a newer version of the A* Pathfinding Project, trying to load, but you might get errors.\nFile version: "+astarVersion+" Current A* version: "+AstarPath.Version);
					//error = SerializerError.WrongVersion;
					//return;
				} else if (astarVersion != AstarPath.Version) {
					Debug.LogWarning ("Loading graphs saved with an older version of the A* Pathfinding Project, trying to load.\nFile version: "+astarVersion+" Current A* version: "+AstarPath.Version);
					
					//Select the appropriate deserializer
					if (astarVersion < new Version (3,0,4)) {
						returnSerializer = new AstarSerializer3_01 (active);
					} else if (astarVersion < new Version (3,0,5)) {
						returnSerializer = new AstarSerializer3_04 (active);
					} else if (astarVersion < new Version (3,0,6)) {
						returnSerializer = new AstarSerializer3_05 (active);
					}
					
					//Copy this serializer's loaded values to the new serializer
					returnSerializer.readerStream = readerStream;
					returnSerializer.anchors = anchors;
				}
			}
			
			int count = stream.ReadInt32 ();
			
			//@Fix - Look up existing graph first
			returnSerializer.loadedGraphGuids = new string[count];
			
			for (int i=0;i<count;i++) {
				returnSerializer.loadedGraphGuids[i] = stream.ReadString ();
				//loadedGraphGuids[i] = i;
			}
			
			
			returnSerializer.mask = stream.ReadInt32 ();
			
			return returnSerializer;
		}
		
		/** Called to serialize a graphs settings. \note Before calling this, setting #sPrefix to something unique for the graph is a good idea to avoid collisions in variable names */
		public void SerializeSettings (NavGraph graph, AstarPath active) {
			
			ISerializableGraph serializeGraph = graph as ISerializableGraph;
				
			if (serializeGraph == null) {
				Debug.LogError ("The graph specified is not serializable, the graph is of type "+graph.GetType());
				return;
			}
	        
			serializeGraph.SerializeSettings (this);
		}
		
		public void InitializeSerializeNodes () {
			throw new NotImplementedException ("This function is deprecated");
		}
		
		/** Serializes the nodes in the graph.
		 * \astarpro */
		public void SerializeNodes (NavGraph graph, AstarPath active) {
			if (mask == SMask.SaveNodes) {

				ISerializableGraph serializeGraph = graph as ISerializableGraph;
				
				if (serializeGraph == null) {
					Debug.LogError ("The graph specified is not serializable, the graph is of type "+graph.GetType());
					return;
				}
				
				if (graph.nodes == null || graph.nodes.Length == 0) {
					writerStream.Write (0);
					//Debug.LogWarning ("No nodes to serialize");
					return;
				}
				
				writerStream.Write (graph.nodes.Length);
				
				//writerStream.Write (savingToFile ? 753 : 1337);
				Debug.Log ("Stored nodes "+" "+writerStream.BaseStream.Position);
				
				SizeProfiler.Begin ("Graph specific nodes",writerStream);
				
				AddVariableAnchor ("DeserializeGraphNodes");
				serializeGraph.SerializeNodes (graph.nodes,this);
				
				SizeProfiler.End ("Graph specific nodes",writerStream);
				
				AddVariableAnchor ("DeserializeNodes");
				
				if (mask == SMask.RunLengthEncoding) {
					
					SizeProfiler.Begin ("RLE Penalty",writerStream);
					//Penalties
					int lastValue = (int)graph.nodes[0].penalty;
					int lastEntry = 0;
					
					for (int i=1;i<graph.nodes.Length;i++) {
						if (graph.nodes[i].penalty != lastValue || (i-lastEntry) >= byte.MaxValue-1) {
							writerStream.Write ((byte)(i-lastEntry));
							writerStream.Write (lastValue);
							lastValue = (int)graph.nodes[i].penalty;
							lastEntry = i;
						}
					}
					
					writerStream.Write ((byte)(graph.nodes.Length-lastEntry));
					writerStream.Write (lastValue);
					
					SizeProfiler.Begin ("RLE Flags",writerStream);
					
					//Flags
					lastValue = graph.nodes[0].flags;
					lastEntry = 0;
					
					for (int i=1;i<graph.nodes.Length;i++) {
						if (graph.nodes[i].flags != lastValue || (i-lastEntry) >= byte.MaxValue) {
							writerStream.Write ((byte)(i-lastEntry));
							writerStream.Write (lastValue);
							lastValue = graph.nodes[i].flags;
							lastEntry = i;
						}
					}
					writerStream.Write ((byte)(graph.nodes.Length-lastEntry));
					writerStream.Write (lastValue);
					
					SizeProfiler.End ("RLE Flags",writerStream);
				}
				
				SizeProfiler.Begin ("Nodes, other",writerStream);
				
				for (int i=0;i<graph.nodes.Length;i++) {
					SerializeNode (graph.nodes[i], writerStream);
				}
				
				SizeProfiler.End ("Nodes, other",writerStream);
			}
		}
		
		/** Deserializes nodes in the graph. The deserialized nodes will be created using graph.CreateNodes (numberOfNodes).
		 * \astarpro */
		public void DeserializeNodes (NavGraph graph, NavGraph[] graphs, int graphIndex, AstarPath active) {
			if (mask == SMask.SaveNodes) {
	
				ISerializableGraph serializeGraph = graph as ISerializableGraph;
				
				if (serializeGraph == null) {
					Debug.LogError ("The graph specified is not serializable, the graph is of type "+graph.GetType());
					return;
				}
				
				int numNodes = readerStream.ReadInt32 ();
				
				graph.nodes = serializeGraph.CreateNodes (numNodes);
				
				if (numNodes == 0) {
					return;
				}
				
				for (int i=0;i<graph.nodes.Length;i++) {
					graph.nodes[i].graphIndex = graphIndex;
				}
				
				Debug.Log ("Loading "+numNodes+ " nodes");
				if (!MoveToVariableAnchor ("DeserializeGraphNodes")) {
					Debug.LogError ("Error loading nodes - Couldn't find anchor");
				}
				
				serializeGraph.DeSerializeNodes (graph.nodes,this);
				
				if (!MoveToVariableAnchor ("DeserializeNodes")) {
					Debug.LogError ("Error loading nodes - Couldn't find anchor");
					return;
				}
				
				if (mask == SMask.RunLengthEncoding) {
					int totalCount = 0;
					
					//Penalties
					while (totalCount < graph.nodes.Length) {
						int runLength = (int)readerStream.ReadByte ();
						int value = readerStream.ReadInt32 ();
						int endIndex = totalCount+runLength;
						
						if (endIndex > graph.nodes.Length) {
							Debug.LogError ("Run Length Encoding is too long "+runLength+" "+endIndex+ " "+graph.nodes.Length+" "+totalCount);
							endIndex = graph.nodes.Length;
						}
						
						for (int i=totalCount;i<endIndex;i++) {
							graph.nodes[i].penalty = (uint)value;
						}
						totalCount = endIndex;
					}
					
					totalCount = 0;
					
					//Flags
					while (totalCount < graph.nodes.Length) {
						int runLength = (int)readerStream.ReadByte ();
						int value = readerStream.ReadInt32 ();
						int endIndex = totalCount+runLength;
						
						if (endIndex > graph.nodes.Length) {
							Debug.LogError ("Run Length Encoding is too long "+runLength+" "+endIndex+ " "+graph.nodes.Length+" "+totalCount);
							endIndex = graph.nodes.Length;
						}
						
						for (int i=totalCount;i<endIndex;i++) {
							graph.nodes[i].flags = value;
						}
						totalCount += runLength;
					}
				}
				
				for (int i=0;i<graph.nodes.Length;i++) {
					DeSerializeNode (graph.nodes[i], graphs, graphIndex, readerStream);
				}
			}
		}
		
		public void SerializeEditorSettings (NavGraph graph, ISerializableGraphEditor editor, AstarPath active) {
				
			if (editor == null) {
				Debug.LogError ("The editor specified is Null");
				return;
			}
			
			//The script will return to this value and write the number of variables serialized with the simple serializer
	        positionAtCounter = (int)writerStream.BaseStream.Position;
	        writerStream.Write (0);//This will be overwritten 
	        counter = 0;
	        
			editor.SerializeSettings (graph,this);
		}
		
		public void DeSerializeEditorSettings (NavGraph graph, ISerializableGraphEditor editor, AstarPath active) {
				
			if (editor == null) {
				Debug.LogError ("The editor specified is Null");
				return;
			}
			
			editor.DeSerializeSettings (graph,this);
		}
		
		//This is intended for quick saving of settings for e.g Undo operations
		public void DeSerializeSettings (NavGraph graph, AstarPath active) {
	        
	        ISerializableGraph serializeGraph = graph as ISerializableGraph;
				
			if (serializeGraph == null) {
				Debug.LogError ("The graph specified is not (de)serializable (how it could be serialized in the first place is a mystery) the graph was of type "+graph.GetType());
				return;
			}
			
			graph.open = readerStream.ReadBoolean ();
			
			//readerStream.ReadString ();
			serializeGraph.DeSerializeSettings (this);
		}
		
		//===== ANCHORS
		
		public void SerializeAnchors () {
			
			if (anchors == null) {
				Debug.LogError ("The anchors dictionary is null");
				return;
			}
			
			int pos = (int)writerStream.BaseStream.Position;
			
			writerStream.Write (anchors.Count);
			
			foreach (KeyValuePair<string,int> anchor in anchors) {
				writerStream.Write (anchor.Key);
				writerStream.Write (anchor.Value);
			}
			
			int prePos = (int)writerStream.BaseStream.Position;
			
			writerStream.BaseStream.Position = 0;
			writerStream.Write (pos);
			
			writerStream.BaseStream.Position = prePos;
		}
		
		public void DeserializeAnchors () {
			
			readerStream.BaseStream.Position = 0;
			
			int pos = readerStream.ReadInt32 ();
			int prePos = (int)readerStream.BaseStream.Position;
			
			readerStream.BaseStream.Position = pos;
			
			//Debug.Log ("Anchor position "+pos);
			
			int count = readerStream.ReadInt32 ();
			
			anchors = new Dictionary<string,int> (count);
			
			for (int i=0;i<count;i++) {
				anchors.Add (readerStream.ReadString (),readerStream.ReadInt32 ());
			}
			
			
			readerStream.BaseStream.Position = prePos;
		}
		
		public void AddVariableAnchor (string name) {
			AddAnchor (sPrefix + "#"+name);
		}
		
		public void AddAnchor (string name) {
			if (anchors.ContainsKey	 (name)) {
				Debug.Log ("Duplicate Anchor : "+name + " - A graph's serialization method is probably faulty");
			} else {
				anchors.Add (name,(int)writerStream.BaseStream.Position);
			}
		}
		
		public bool MoveToVariableAnchor (string name) {
			return MoveToAnchor (sPrefix+"#"+name);
		}
		
		public bool MoveToAnchor (string name) {
			int pos;
			if (anchors.TryGetValue (name,out pos)) {
				readerStream.BaseStream.Position = pos;
				return true;
			}
			return false;
		}
		
		//====== END ANCHORS
		
		//================= DeSerialization - variables
		
		public delegate void DeSerializationInterrupt (AstarSerializer serializer, bool newer, Guid guid);
		
		//public void DeSerialize (AstarPath active, bool runtime, out NavGraph graph) {
		//	DeSerialize (active, runtime, out graph, null);
		//}
		
		/** Serializes links placed by the user */
		public virtual void SerializeUserConnections (UserConnection[] userConnections) {
			
			System.IO.BinaryWriter stream = writerStream;
			
			AddAnchor ("UserConnections");
			if (userConnections != null) {
				stream.Write (userConnections.Length);
				
				for (int i=0;i<userConnections.Length;i++) {
					UserConnection conn = userConnections[i];
					stream.Write (conn.p1.x);
					stream.Write (conn.p1.y);
					stream.Write (conn.p1.z);
					
					stream.Write (conn.p2.x);
					stream.Write (conn.p2.y);
					stream.Write (conn.p2.z);
					
					stream.Write (conn.doOverrideCost);
					stream.Write (conn.overrideCost);
					
					stream.Write (conn.oneWay);
					stream.Write (conn.width);
					stream.Write ((int)conn.type);
					
					stream.Write (conn.enable);
					stream.Write (conn.doOverrideWalkability);
					stream.Write (conn.doOverridePenalty);
					stream.Write (conn.overridePenalty);
				}
			} else {
				stream.Write (0);
			}
		}
		
		/** Deserializes links placed by the user */
		public virtual UserConnection[] DeserializeUserConnections () {
			System.IO.BinaryReader stream = readerStream;
			
			if (MoveToAnchor ("UserConnections")) {
				int count = stream.ReadInt32 ();
				
				UserConnection[] userConnections = new UserConnection[count];
				
				for (int i=0;i<count;i++) {
					UserConnection conn = new UserConnection ();
					conn.p1 = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
					conn.p2 = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
					
					conn.doOverrideCost = stream.ReadBoolean ();
					conn.overrideCost = stream.ReadInt32 ();
					
					conn.oneWay = stream.ReadBoolean ();
					conn.width = stream.ReadSingle ();
					conn.type = (ConnectionType)stream.ReadInt32 ();
					
					conn.enable = stream.ReadBoolean ();
					conn.doOverrideWalkability = stream.ReadBoolean ();
					conn.doOverridePenalty = stream.ReadBoolean ();
					conn.overridePenalty = stream.ReadUInt32 ();
					
					userConnections[i] = conn;
				}
				return userConnections;
			} else {
				return new UserConnection[0];
			}
		}
		

		//One node uses approximately 4*3 + 4 + 4 + (connections ? 1 + (4+4)*numConnections) ? 20 + 1 + 8*numConnections ? 21 + 8*3 ? 45 bytes serialized
		/** Serializes one node to the stream.
		 * \astarpro */
		private void SerializeNode (Node node, BinaryWriter stream) {
			throw new NotSupportedException ("This function has been deprecated");
		}
		
		private List<Node> tmpConnections;
		private List<int> tmpConnectionCosts;
		
		/** Deserializes one node from the stream into the specified graphs and to the specified graph index.
		  * \astarpro */
		private void DeSerializeNode (Node node, NavGraph[] graphs, int graphIndex, BinaryReader stream) {
			
			//NavGraph graph = graphs[graphIndex];
			
			if (mask == SMask.SaveNodePositions) {
				node.position = new Int3(
					stream.ReadInt32 (), //X
					stream.ReadInt32 (), //Y
					stream.ReadInt32 ()	 //Z
				);
			}
			
			if (mask != SMask.RunLengthEncoding) {
				node.penalty = (uint)stream.ReadInt32 ();
				node.flags = stream.ReadInt32 ();
			}
			
			if (mask == SMask.SaveNodeConnections) {
				
				if (tmpConnections == null) {
					tmpConnections = new List<Node> ();
					tmpConnectionCosts = new List<int> ();
				} else {
					tmpConnections.Clear ();
					tmpConnectionCosts.Clear ();
				}
				
				int numConn = (int)stream.ReadByte ();
				
				for (int i=0;i<numConn;i++) {
					
					int nodeIndex = stream.ReadInt32 ();
					
					//Graph index as in, which graph
					int nodeGraphIndex = (nodeIndex >> 26) & 0x3F;
					
					nodeIndex &= 0x3FFFFFF;
					
					int cost = stream.ReadInt32 ();
					
					bool containsLink = false;
					
					if (nodeGraphIndex != graphIndex) {
						containsLink = stream.ReadBoolean ();
					}
	
					if (graphRefGuids[graphIndex] != -1) {
						Node other = active.astarData.GetNode (graphRefGuids[nodeGraphIndex],nodeIndex, graphs);
						
						//Shouldn't really have to check for this, but just in case of corrupt serialization data
						if (other != null) {
							tmpConnections.Add (other);
							
							if (mask == SMask.SaveNodeConnectionCosts) {
								tmpConnectionCosts.Add (cost);
							}
							
							if (containsLink) {
								other.AddConnection (node,cost);
							}
						}
					}
				}
				
				node.connections = tmpConnections.ToArray ();
				
				if (mask == SMask.SaveNodeConnectionCosts) {
					node.connectionCosts = tmpConnectionCosts.ToArray ();
				} else {
					node.connectionCosts = new int[node.connections.Length];
				}
			}
		}
		
		public void WriteError () {

			
			if (positionAtError == -1) {
				return;
			}
			if (writerStream == null) {
				Debug.LogError ("You should only call the WriteError function when Serializing, not when DeSerializing");
				return;
			}
			

			
			long prePos = writerStream.BaseStream.Position;
			writerStream.BaseStream.Position = positionAtError;
			writerStream.Write (true);
			writerStream.BaseStream.Position = prePos;
		}
			
		
		public void Close () {
			if (readerStream != null) {
				readerStream.Close ();
			}
			if (writerStream != null) {
				
				if (anchors != null) {
					SerializeAnchors ();
				}
				
				writerStream.Close ();
			}
		}
		
		//============== Custom Simpler Serializer
		
		/** Serializes a Unity Reference. Serializer references such as Transform, GameObject, Texture or other unity objects */
		public virtual void AddUnityReferenceValue (string key, UnityEngine.Object value) {
			//Segment --- Should be identical to a segment in AddUnityReferenceValue/AddValue
			BinaryWriter stream = writerStream;
			
			AddVariableAnchor (key);
			
			if (value == null) {
				stream.Write ((byte)0);//Magic number indicating a null reference
				return;
			}
			
			//Magic number indicating that the data is written and not null
			stream.Write ((byte)1);
			//Segment --- End
			
			if (value == active.gameObject) {
				stream.Write (-128);//Magic number (random) indicates that the reference is the A* object
			} else if (value == active.transform) {
				stream.Write (-129);
			} else {
				stream.Write (value.GetInstanceID ());
			}
			stream.Write (value.name);
			
			//Write scene path if the object is a Component or GameObject
			Component component = value as Component;
			GameObject go = value as GameObject;
			
			if (component == null && go == null) {
				stream.Write ("");
			} else {
				if (component != null && go == null) {
					go = component.gameObject;
				}
				
				UnityReferenceHelper helper = go.GetComponent<UnityReferenceHelper>();
				
				if (helper == null) {
					Debug.Log ("Adding UnityReferenceHelper to Unity Reference '"+value.name+"'");
					helper = go.AddComponent<UnityReferenceHelper>();
				}
				
				//Make sure it has a unique GUID
				helper.Reset ();
				
				stream.Write (helper.GetGUID ());
				
				/*if (go == null) {
					go = component.gameObject;
				}
				string path = go.name;
				
				while (go.transform.parent != null) {
					go = go.transform.parent.gameObject;
					path = go.name+"/" +path;
				}
				stream.Write (path);*/
			}
			
			/*if (writeUnityReference_Editor != null) {
				stream.Write (true);
				writeUnityReference_Editor (this,value);
			} else {*/
			
				//Backwards compability
				stream.Write (false);
			//}
		}
		
		/** Serializes value with key and value */
		public void AddValue (string key, System.Object value) {	
			//Segment --- Should be identical to a segment in AddUnityReferenceValue/AddValue
			BinaryWriter stream = writerStream;
			
			AddVariableAnchor (key);
			
			if (value == null) {
				stream.Write ((byte)0);//Magic number indicating a null reference
				return;
			}
			
			//Magic number indicating that the data is written and not null
			stream.Write ((byte)1);
			//Segment --- End
			
			Type type = value.GetType ();
			
			//stream.Write (type.Name);
			if (type == typeof (int)) {
				stream.Write ((int)value);
			} else if (type == typeof (string)) {
				string st = (string)value;
				stream.Write (st);
			} else if (type == typeof (float)) {
				stream.Write ((float)value);
			} else if (type == typeof (bool)) {
				stream.Write ((bool)value);
			} else if (type == typeof (Vector3)) {
				Vector3 d = (Vector3)value;
				stream.Write (d.x);
				stream.Write (d.y);
				stream.Write (d.z);
			} else if (type == typeof (Vector2)) {
				Vector2 d = (Vector2)value;
				stream.Write (d.x);
				stream.Write (d.y);
			} else if (type == typeof (Matrix4x4)) {
				Matrix4x4 m = (Matrix4x4)value;
				for (int i=0;i<16;i++) {
					stream.Write (m[i]);
				}
			} else if (type == typeof (Bounds)) {
				Bounds b = (Bounds)value;
				
				stream.Write (b.center.x);
				stream.Write (b.center.y);
				stream.Write (b.center.z);
				
				stream.Write (b.extents.x);
				stream.Write (b.extents.y);
				stream.Write (b.extents.z);
			} else {
				ISerializableObject sOb = value as ISerializableObject;
				
				if (sOb != null) {
					
					string prePrefix = sPrefix;
					//Add to the prefix to avoid name collisions
					sPrefix += key + ".";
					sOb.SerializeSettings (this);
					sPrefix = prePrefix;
				} else {
						
					UnityEngine.Object ueOb = value as UnityEngine.Object;
					
					if (ueOb != null) {
						
						Debug.LogWarning ("Unity Object References should be added using AddUnityReferenceValue");
						WriteError ();
						
						stream.BaseStream.Position -= 1;//Overwrite the previous magic number
						stream.Write ((byte)2);//Magic number indicating an error while serializing
					} else {
						Debug.LogError ("Can't serialize type '"+type.Name+"'");
						WriteError ();
						
						stream.BaseStream.Position -= 1;//Overwrite the previous magic number
						stream.Write ((byte)2);//Magic number indicating an error while serializing
					}
				}
			}
		}
		
		/** Deserializes a Unity Reference. Deserializes references such as Transform, GameObject, Texture or other unity objects */
		public virtual UnityEngine.Object GetUnityReferenceValue (string key, Type type, UnityEngine.Object defaultValue = null) {
			//Segment --- Should be (except for the defaultValue cast) identical to a segment in GetUnityReferenceValue/GetValue
			if (!MoveToVariableAnchor (key)) {
				Debug.Log ("Couldn't find key '"+key+"' in the data, returning default");
				return (defaultValue == null ? GetDefaultValue (type) : defaultValue) as UnityEngine.Object;
			}
			
			BinaryReader stream = readerStream;
			
			int magicNumber = (int)stream.ReadByte ();
			
			if (magicNumber == 0) {
				return (defaultValue == null ? GetDefaultValue (type) : defaultValue) as UnityEngine.Object;//Null reference
			} else if (magicNumber == 2) {
				Debug.Log ("The variable '"+key+"' was not serialized correctly and can therefore not be deserialized");
				return (defaultValue == null ? GetDefaultValue (type) : defaultValue) as UnityEngine.Object;
			}
			//Else - magic number is 1 - indicating correctly serialized data
			//Segment --- End
			
			int instanceID = stream.ReadInt32 ();
			string obName = stream.ReadString ();
							
			if (instanceID == -128) {//Magic number
				return active.gameObject;
			} else if (instanceID == -129) { //Magic number
				return active.transform;
			}
			
			//GUID
			string guid = stream.ReadString ();
			
			UnityReferenceHelper[] helpers = UnityEngine.Object.FindSceneObjectsOfType (typeof(UnityReferenceHelper)) as UnityReferenceHelper[];
			
			for (int i=0;i<helpers.Length;i++) {
				if (helpers[i].GetGUID () == guid) {
					if (type == typeof(GameObject)) {
						return helpers[i].gameObject;
					} else {
						return helpers[i].GetComponent (type);
					}
				}
			}
			
			//Always false from 3.0.8 and up
			//bool didSaveFromEditor = 
				stream.ReadBoolean ();
			
			//UnityEngine.Object[] ueObs = Resources.FindObjectsOfTypeAll (type);
			
			//Try to load from resources
			UnityEngine.Object[] objs = Resources.LoadAll (obName,type);
			
			for (int i=0;i<objs.Length;i++) {
				if (objs[i].name == obName || objs.Length == 1) {
					return objs[i];
				}
			}
			
			return null;
			
			
			
			/*if (readUnityReference_Editor != null && didSaveFromEditor) {
				UnityEngine.Object eob = readUnityReference_Editor (this,obName,instanceID,type);
				if (eob != null && eob.name == obName) {
					return eob;
				} else if (ob3 != null) {
					return ob3;
				} else {
					return eob;
				}
			}
			
			//If the editor deserialization didn't come up with a better answer, return ob3 if it isn't null
			if (ob3 != null) {
				return ob3;
			}
			
			//Last resort, find all objects of type and check them for the instance ID
			UnityEngine.Object[] ueObs = Resources.FindObjectsOfTypeAll (type);
				//UnityEngine.Object.FindObjectsOfType (type);
			
			UnityEngine.Object ob1 = null;
			
			for (int i=0;i<ueObs.Length;i++) {
				if (ueObs[i].GetInstanceID () == instanceID) {
					ob1 = ueObs[i];
					break;
				}
				
				//Connecting it based on name is a bit too vague
				/*if (ueObs[i].name == obName) {
					ob2 = ueObs[i];
				}*
			}
			
			if (ob1 != null) {
				return ob1;
			}
			
			//Try to load from resources
			UnityEngine.Object ob4 = Resources.Load (obName);
			
			return ob4;
			if (ob1 != null) {
				return ob1;
			} else {
				return ob2;
			}*/
		}
		
		/** Makes \a path relative to the folder \a relativeFolder when \a relativeFolder exists in \a path.
		 * A path folder1/someFolder/testingFolder/myPic.png with \a relativeFolder = "someFolder" would become testingFolder/myPic.png\n
		 * Returns "" (empty string) if the path didn't contain \a relativeFolder
		 */
		public static string StripPathOfFolder (string path, string relativeFolder) {
			int pos = path.IndexOf (relativeFolder);
			if (pos == -1) {
				return "";
			} else {
				path =  path.Remove (0,pos+relativeFolder.Length);
				if (path.StartsWith ("/")) {
					path = path.Remove (0,1);
				}
				return path;
			}
		}
		
		/** Returns the default value for the given type */
		public System.Object GetDefaultValue (Type type) {
			if (type.IsValueType) {
				return System.Activator.CreateInstance (type);
			} else {
				return null;
			}
		}
		
		/** Deserializes a variable with key of the specified type */
		public System.Object GetValue (string key, Type type, System.Object defaultValue = null) {
			
			//Segment --- Should be identical to a segment in GetUnityReferenceValue/GetValue
			if (!MoveToVariableAnchor (key)) {
				Debug.Log ("Couldn't find key '"+key+"' in the data, returning default ("+(defaultValue == null ? "null" : defaultValue.ToString ())+")");
				return defaultValue == null ? GetDefaultValue (type) : defaultValue;
			}
			
			BinaryReader stream = readerStream;
			
			int magicNumber = (int)stream.ReadByte ();
			
			if (magicNumber == 0) {
				return defaultValue == null ? GetDefaultValue (type) : defaultValue;//Null reference usually
			} else if (magicNumber == 2) {
				Debug.Log ("The variable '"+key+"' was not serialized correctly and can therefore not be deserialized");
				return defaultValue == null ? GetDefaultValue (type) : defaultValue;
			}
			//Else - magic number is 1 - indicating correctly serialized data
			//Segment --- End
			
			System.Object ob = null;
			
			if (type == typeof (int)) {
				ob = stream.ReadInt32 ();
			} else if (type == typeof (string)) {
				ob = stream.ReadString ();
			} else if (type == typeof (float)) {
				ob = stream.ReadSingle ();
			} else if (type == typeof (bool)) {
				ob = stream.ReadBoolean ();
			} else if (type == typeof (Vector3)) {
				ob = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
			} else if (type == typeof (Vector2)) {
				ob = new Vector2 (stream.ReadSingle (),stream.ReadSingle ());
			} else if (type == typeof (Matrix4x4)) {
				Matrix4x4 m = new Matrix4x4 ();
				for (int i=0;i<16;i++) {
					m[i] = stream.ReadSingle ();
				}
				ob = m;
			} else if (type == typeof (Bounds)) {
				Bounds b = new Bounds ();
				b.center = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
				b.extents = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
				ob = b;
			} else {
				
				if (type.GetConstructor (Type.EmptyTypes) != null) {
					System.Object testOb = Activator.CreateInstance (type);
					
					ISerializableObject sOb = (ISerializableObject)testOb;
					
					if (sOb != null) {
						string prePrefix = sPrefix;
						//Add to the prefix to avoid name collisions
						sPrefix += key + ".";
						
						sOb.DeSerializeSettings (this);
						ob = sOb;
						sPrefix = prePrefix;
					}
				}
				
				if (ob == null) {
					Debug.LogError ("Can't deSerialize type '"+type.Name+"'");
					ob = defaultValue == null ? GetDefaultValue (type) : defaultValue;
				}
			}
			
			return ob;
		}
		
		public byte[] Compress (byte[] bytes) {
			
			/*int[] freq = new int[256];
			
			for (int i=0;i<256;i++) {
				freq[i] = 0;
			}
	
			//Create Frequency table
			for (int i=0;i<bytes.Length;i++) {
				freq[bytes[i]]++;
			}
			
			byte[] byteSample = new byte[256];
			for (int i=0;i<256;i++){
				byteSample[i] = (byte)i;
			}
			
			Sort (freq, byteSample);
			
			string st = "";//"Frequency Table\n";
			for (int i=0;i<256;i++) {
				
				System.Char c = (System.Char)0;
				
				//Ignore the 'Null' character
				if (byteSample[i] == 0) {
					c = "N"[0];
					//continue;
				} else {
					c = (System.Char)byteSample[i];
				}
				
				//System.Char c = (System.Char)byteSample[i];
				string line = i+"	"+c.ToString ()+"	"+freq[i]+"\n";
				st = st + line;
			}
			
			int[] newFreq;
			byte[] byteSample2;
			for (int i=0;i<256;i++) {
				if (freq[i] != 0) {
					newFreq = new int[256-i];
					byteSample2 = new byte[256-i];
					for (int q=i;q < 256;q++) {
						newFreq[q-i] = freq[q];
						byteSample2[q-i] = byteSample[q];
					}
					break;
				}
			}
			
			List<int> freqList = new List<int> (newFreq);
			List<byte> byteList = new List<byte> (byteSample2);
			
			byte[] list = new byte[1024];
			int[] freqList = new int[1024];
			
			list[0] = freqList[0];*/
			
			//Huffman h = new Huffman (bytes);
			
			//Debug.Log (st);
			return bytes;//h.Encode ();
		}
		
		public byte[] DeCompress (byte[] bytes) {
			//Huffman h = new Huffman ();
			
			//Debug.Log (st);
			return bytes;//h.Decode (bytes);
		}
		
		//Sort b by a
		public void Sort (int[] a, byte[] b) {
			
			bool changed = true;
		
			while (changed) {
				changed = false;
				for (int i=0;i<a.Length-1;i++) {
					if (a[i] > a[i+1]) {
						int tmp = a[i];
						a[i] = a[i+1];
						a[i+1] = tmp;
						
						byte tmp2 = b[i];
						b[i] = b[i+1];
						b[i+1] = tmp2;
						changed = true;
					}
				}
			}
		}
		
		public void SaveToFile (string path, byte[] data) {
			
			FileStream fs = File.Create(path);
			
			//Magic number = 0x31, 0x44 = :D (in unicode)
			//fs.WriteByte (0x3A); // :
			//fs.WriteByte (0x44); // D
			
	        fs.Write (data,0,data.Length);
	        
			fs.Close ();
			
		}
		
		public byte[] LoadFromFile (string path) {
			
			if (!File.Exists (path)) {
				error = SerializerError.DoesNotExist;
				Debug.LogError ("File does not exist : "+path);
				return new byte[0];
			}
			
			FileStream fs = File.Open (path, FileMode.Open);
	        
			/*int b1 = fs.ReadByte ();
			int b2 = fs.ReadByte ();
	        if (b1 != 0x3A || b2 != 0x44) {
	        	Debug.LogWarning ("Magic Numbers did not match - The file is probably corrupt\nThe magic numbers in the file were "+(char)b1+" and "+(char)b2+" ("+b1+", "+b2+")");
	        	error = SerializerError.WrongMagic;
	        	return null;
	        }*/
	        
	        byte[] bytes = new byte[fs.Length];
	        
	        fs.Read (bytes,0,bytes.Length);
			fs.Close ();
			
			return bytes;
			
		}
		
		public static void TestLoadFile (string path) {
			FileStream fs = File.Open (path, FileMode.Open);
			fs.Close ();
		}
		
		//============== End Custom Simpler Serializer
		
		//A bitmask implementation, 32 bits, -1 = Everything, 0 = Nothing, 1 << n = Bit n is on (where n is an int from 0 to 31)
		//Assign values with += or |=, and remove them with -= or &= ~value
		//Values should be one bit values (unless you know what you are doing), such as 1 << 20
		public struct BitMask {
			int value;
			
			public BitMask (int v) {
				value = v;
			}
			
			public static bool operator == (BitMask mask, int value) {
				return (mask.value & value) == value;
			}
			
			public static bool operator != (BitMask mask, int value) {
				return (mask.value & value) != value;
			}
			
			public static BitMask operator | (BitMask mask, int value) {
				mask.value |= value;
				return mask;
			}
			
			public static BitMask operator & (BitMask mask, int value) {
				mask.value &= value;
				return mask;
			}
			
			public static BitMask operator + (BitMask mask, int value) {
				mask.value |= value;
				return mask;
			}
			
			public static BitMask operator - (BitMask mask, int value) {
				mask.value &= ~value;
				return mask;
			}
			
			public static implicit operator BitMask (int v) {
				return new BitMask (v);
			}
			
			public static implicit operator int (BitMask v) {
				return v.value;
			}
			
			public override bool Equals (System.Object o) {
			
				if (o == null) return false;
				
				BitMask rhs = (BitMask)o;
			
				return 	(rhs.value & value) == value;
			}
		
			public override int GetHashCode () {
				return value;
			}
		}
	}
	
	public interface ISerializableGraphEditor {
		void SerializeSettings (NavGraph target, AstarSerializer serializer);
		void DeSerializeSettings (NavGraph target, AstarSerializer serializer);
	}
	
	public interface ISerializableObject {
		/** Called to serialize the object.
		  * All variables and data which are to be saved should be passed to the serialized using Pathfinding.AstarSerializer.AddValue\n
		  * \code serializer.AddValue ("myVariable",myVariable); \endcode */
		void SerializeSettings (AstarSerializer serializer);
		
		/** Called to deserialize the object. All variables and data which are to be loaded should be loaded using Pathfinding.AstarSerializer.GetValue\n
		  * \code //Loads the integer variable myVariable from the serialized data
		  * myVariable = (int)serializer.GetValue ("myVariable",typeof(int)); \endcode \n
		  * A default value can also be passed, in case the variable isn't contained in the data that will be returned instead\n
		  * \code //Loads the integer variable myVariable with the default value of 512
		  * myVariable = (int)serializer.GetValue ("myVariable",typeof(int),512); \endcode */
		void DeSerializeSettings (AstarSerializer serializer);
	}
	
	public interface ISerializableGraph : ISerializableObject {
		void SerializeNodes (Node[] nodes, AstarSerializer serializer);
		void DeSerializeNodes (Node[] nodes, AstarSerializer serializer);
		Node[] CreateNodes (int num);
	}
}