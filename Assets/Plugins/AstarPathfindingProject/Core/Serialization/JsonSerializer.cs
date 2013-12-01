//#define ASTARDEBUG
//#define ASTAR_FAST_NO_EXCEPTIONS
using System;
using Pathfinding;
using Pathfinding.Serialization.JsonFx;
using Ionic.Zip;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Util;

namespace Pathfinding.Serialization
{
	
	public class AstarSerializer
	{
		
		private AstarData data;
		public JsonWriterSettings writerSettings;
		public JsonReaderSettings readerSettings;
		
		private ZipFile zip;
		private MemoryStream str;
		
		private GraphMeta meta;
		
		private SerializeSettings settings;
		
		private NavGraph[] graphs;
		
		const string jsonExt = ".json";
		const string binaryExt = ".binary";
		
		private uint checksum = 0xffffffff;
		
		System.Text.UTF8Encoding encoding=new System.Text.UTF8Encoding();
		
		private static System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
		
		/** Returns a cached StringBuilder.
		 * This function only has one string builder cached and should
		 * thus only be called from a single thread and should not be called while using an earlier got string builder.
		 */
		private static System.Text.StringBuilder GetStringBuilder () { _stringBuilder.Length = 0; return _stringBuilder; }
		
		public AstarSerializer (AstarData data) {
			this.data = data;
			settings = SerializeSettings.Settings;
		}
		
		public AstarSerializer (AstarData data, SerializeSettings settings) {
			this.data = data;
			this.settings = settings;
		}
		
		public void AddChecksum (byte[] bytes) {
			checksum = Checksum.GetChecksum (bytes,checksum);
		}
		
		public uint GetChecksum () { return checksum; }
		
#region Serialize
		
		public void OpenSerialize () {
			zip = new ZipFile();
			zip.AlternateEncoding = System.Text.Encoding.UTF8;
			zip.AlternateEncodingUsage = ZipOption.Always;
			
			writerSettings = new JsonWriterSettings();
			writerSettings.AddTypeConverter (new VectorConverter());
			writerSettings.AddTypeConverter (new BoundsConverter());
			writerSettings.AddTypeConverter (new LayerMaskConverter());
			writerSettings.AddTypeConverter (new MatrixConverter());
			writerSettings.AddTypeConverter (new GuidConverter());
			writerSettings.AddTypeConverter (new UnityObjectConverter());
			
			//writerSettings.DebugMode = true;
			writerSettings.PrettyPrint = settings.prettyPrint;
			
			meta = new GraphMeta();
		}
		
		public byte[] CloseSerialize () {
			byte[] bytes = SerializeMeta ();
			
			AddChecksum (bytes);
			zip.AddEntry("meta"+jsonExt,bytes);
			
			MemoryStream output = new MemoryStream();
    		zip.Save(output);
			output.Close();
			bytes = output.ToArray();
			
			
			zip.Dispose();
			
			zip = null;
			return bytes;
		}
		
		public void SerializeGraphs (NavGraph[] _graphs) {
			if (graphs != null) throw new InvalidOperationException ("Cannot serialize graphs multiple times.");
			graphs = _graphs;
			
			if (zip == null) throw new NullReferenceException ("You must not call CloseSerialize before a call to this function");
			
			if (graphs == null) graphs = new NavGraph[0];
			
			for (int i=0;i<graphs.Length;i++) {
				//Ignore graph if null
				if (graphs[i] == null) continue;
				
				byte[] bytes = Serialize(graphs[i]);
				
				AddChecksum (bytes);
				zip.AddEntry ("graph"+i+jsonExt,bytes);
			}
		}
		
		public void SerializeUserConnections (UserConnection[] conns) {
			
			if (conns == null) conns = new UserConnection[0];
			
			System.Text.StringBuilder output = GetStringBuilder ();//new System.Text.StringBuilder();
			JsonWriter writer = new JsonWriter (output,writerSettings);
			writer.Write (conns);
			
			byte[] bytes = encoding.GetBytes (output.ToString());
			output = null;
			
			//If length is <= 2 that means nothing was serialized (file is "[]")
			if (bytes.Length <= 2) return;
			
			AddChecksum (bytes);
			zip.AddEntry ("connections"+jsonExt,bytes);
		}
		
		/** Serialize metadata about alll graphs */
		private byte[] SerializeMeta () {
			
			meta.version = AstarPath.Version;
			meta.graphs = data.graphs.Length;
			meta.guids = new string[data.graphs.Length];
			meta.typeNames = new string[data.graphs.Length];
			meta.nodeCounts = new int[data.graphs.Length];
			//meta.settings = settings;
			
			for (int i=0;i<data.graphs.Length;i++) {
				if (data.graphs[i] == null) continue;
				
				meta.guids[i] = data.graphs[i].guid.ToString();
				meta.typeNames[i] = data.graphs[i].GetType().FullName;
				
				meta.nodeCounts[i] = data.graphs[i].nodes==null?0:data.graphs[i].nodes.Length;
			}
			
			System.Text.StringBuilder output = GetStringBuilder ();//new System.Text.StringBuilder();
			JsonWriter writer = new JsonWriter (output,writerSettings);
			writer.Write (meta);
			
			return encoding.GetBytes (output.ToString());
		}
		
		/** Serializes the graph settings to JSON and returns the data */
		public byte[] Serialize (NavGraph graph) {
			System.Text.StringBuilder output = GetStringBuilder ();//new System.Text.StringBuilder();
			JsonWriter writer = new JsonWriter (output,writerSettings);
			writer.Write (graph);
			
			return encoding.GetBytes (output.ToString());
		}
		
		public void SerializeNodes () {
			if (!settings.nodes) return;
			if (graphs == null) throw new InvalidOperationException ("Cannot serialize nodes with no serialized graphs (call SerializeGraphs first)");
			
			for (int i=0;i<graphs.Length;i++) {
				
				byte[] bytes = SerializeNodes (i);
				
				AddChecksum (bytes);
				zip.AddEntry ("graph"+i+"_nodes"+binaryExt,bytes);
			}
			
			for (int i=0;i<graphs.Length;i++) {
				byte[] bytes = SerializeNodeConnections (i);
				
				AddChecksum (bytes);
				zip.AddEntry ("graph"+i+"_conns"+binaryExt,bytes);
			}
		}
		
		private byte[] SerializeNodes (int index) {
			NavGraph graph = graphs[index];
			MemoryStream str = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(str);
			
			Node[] nodes = graph.nodes;
			
			if (nodes == null) nodes = new Node[0];
			
			//Write basic node data.
			//Divide in to different chunks to possibly yield better compression rates with zip
			//The integers above each chunk is a tag to identify each chunk to be able to load them correctly
			
			writer.Write(1);
			for (int i=0;i<nodes.Length;i++) {
				Node node = nodes[i];
				if (node == null) {
					writer.Write(0);
					writer.Write(0);
					writer.Write(0);
				} else {
					writer.Write (node.position.x);
					writer.Write (node.position.y);
					writer.Write (node.position.z);
				}
			}
			
			writer.Write(2);
			for (int i=0;i<nodes.Length;i++) {
				if (nodes[i] == null)	writer.Write (0);
				else					writer.Write (nodes[i].penalty);
			}
			
			writer.Write(3);
			for (int i=0;i<nodes.Length;i++) {
				if (nodes[i] == null)	writer.Write (0);
				else 					writer.Write (nodes[i].flags);
			}
			
			writer.Close();
			return str.ToArray();
		}
		
		public void SerializeExtraInfo () {
			if (!settings.nodes) return;
			
			for (int i=0;i<graphs.Length;i++) {
				
				byte[] bytes = graphs[i].SerializeExtraInfo ();
				
				if (bytes == null) continue;
				
				AddChecksum (bytes);
				zip.AddEntry ("graph"+i+"_extra"+binaryExt,bytes);
			}
		}
		
		/** Serialize node connections for given graph index.
Connections structure is as follows. Bracket structure has nothing to do with data, just how it is structured:\n
\code
for every node {
	Int32 NodeIndex
	Int16 ConnectionCount
	for every connection of the node {
		Int32 OtherNodeIndex
		Int32 ConnectionCost
	}
}
\endcode
		*/
		private byte[] SerializeNodeConnections (int index) {
			NavGraph graph = graphs[index];
			MemoryStream str = new MemoryStream();
			BinaryWriter writer = new BinaryWriter(str);
			
			if (graph.nodes == null) return new byte[0];
			
			Node[] nodes = graph.nodes;
			
			for (int i=0;i<nodes.Length;i++) {
				Node node = nodes[i];
				if (node.connections == null) { writer.Write((ushort)0); continue; }
				
				if (node.connections.Length	!= node.connectionCosts.Length)
					throw new IndexOutOfRangeException ("Node.connections.Length != Node.connectionCosts.Length. In node "+i+" in graph "+index);
				
				//writer.Write(node.GetNodeIndex());
				writer.Write ((ushort)node.connections.Length);
				
				for (int j=0;j<node.connections.Length;j++) {
					writer.Write(node.connections[j].GetNodeIndex());
					writer.Write(node.connectionCosts[j]);
				}
			}
			
			writer.Close();
			return str.ToArray();
		}
		
#if UNITY_EDITOR
		public void SerializeEditorSettings (GraphEditorBase[] editors) {
			if (editors == null || !settings.editorSettings) return;
			
			for (int i=0;i<editors.Length;i++) {
				if (editors[i] == null) return;
				
				System.Text.StringBuilder output = GetStringBuilder ();//new System.Text.StringBuilder();
				JsonWriter writer = new JsonWriter (output,writerSettings);
				writer.Write (editors[i]);
				
				byte[] bytes = encoding.GetBytes (output.ToString());
				
				//Less or equal to 2 bytes means that nothing was saved (file is "{}")
				if (bytes.Length <= 2)
					continue;
				
				AddChecksum(bytes);
				zip.AddEntry ("graph"+i+"_editor"+jsonExt,bytes);
			}
		}
#endif
		
#endregion
		
#region Deserialize
		
		public bool OpenDeserialize (byte[] bytes) {
			readerSettings = new JsonReaderSettings();
			readerSettings.AddTypeConverter (new VectorConverter());
			readerSettings.AddTypeConverter (new BoundsConverter());
			readerSettings.AddTypeConverter (new LayerMaskConverter());
			readerSettings.AddTypeConverter (new MatrixConverter());
			readerSettings.AddTypeConverter (new GuidConverter());
			readerSettings.AddTypeConverter (new UnityObjectConverter());
			
			str = new MemoryStream();
			str.Write(bytes,0,bytes.Length);
			str.Position = 0;
			try {
				zip = ZipFile.Read(str);
			} catch (ZipException e) {
				//Catches exceptions when an invalid zip file is found
				Debug.LogWarning ("Caught exception when loading from zip\n"+e);
				str.Close();
				return false;
			}
			meta = DeserializeMeta (zip["meta"+jsonExt]);
			
			if (meta.version > AstarPath.Version) {
				Debug.LogWarning ("Trying to load data from a newer version of the A* Pathfinding Project\nCurrent version: "+AstarPath.Version+" Data version: "+meta.version);
			} else if (meta.version < AstarPath.Version) {
				Debug.LogWarning ("Trying to load data from an older version of the A* Pathfinding Project\nCurrent version: "+AstarPath.Version+" Data version: "+meta.version
					+ "\nThis is usually fine, it just means you have upgraded to a new version");
			}
			return true;
		}
		
		public void CloseDeserialize () {
			str.Close();
			zip.Dispose();
			zip = null;
			str = null;
		}
		
		/** Deserializes graph settings.
		 * \note Stored in files named "graph#.json" where # is the graph number.
		 */
		public NavGraph[] DeserializeGraphs () {
			
			//for (int j=0;j<1;j++) {
			//System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			//watch.Start();
			
			graphs = new NavGraph[meta.graphs];
			
			for (int i=0;i<meta.graphs;i++) {
				Type tp = meta.GetGraphType(i);
				
				//Graph was null when saving, ignore
				if (tp == null) continue;
				
				ZipEntry entry = zip["graph"+i+jsonExt];
				
				if (entry == null)
					throw new FileNotFoundException ("Could not find data for graph "+i+" in zip. Entry 'graph+"+i+jsonExt+"' does not exist");
				
				//Debug.Log ("Reading graph " +i+" with type "+tp.FullName);
				String entryText = GetString(entry);
				//Debug.Log (entryText);
					
				NavGraph tmp = data.CreateGraph(tp);//(NavGraph)System.Activator.CreateInstance(tp);
				JsonReader reader = new JsonReader(entryText,readerSettings);
				
				//NavGraph graph = tmp.Deserialize(reader);//reader.Deserialize<NavGraph>();
				reader.PopulateObject (tmp);
				
				graphs[i] = tmp;
				if (graphs[i].guid.ToString () != meta.guids[i])
					throw new System.Exception ("Guid in graph file not equal to guid defined in meta file. Have you edited the data manually?\n"+graphs[i].guid.ToString()+" != "+meta.guids[i]);
				
				//NavGraph graph = (NavGraph)JsonConvert.DeserializeObject (entryText,tp,settings);
			}
			
			return graphs;
			
			//watch.Stop();
			//Debug.Log ((watch.ElapsedTicks*0.0001).ToString ("0.00"));
			//}
		}
		
		/** Deserializes manually created connections.
		 * Connections are created in the A* inspector.
		 * \note Stored in a file named "connections.json".
		 */
		public UserConnection[] DeserializeUserConnections () {
			ZipEntry entry = zip["connections"+jsonExt];
			
			if (entry == null) return new UserConnection[0];
			
			string entryText = GetString (entry);
			JsonReader reader = new JsonReader(entryText,readerSettings);
			UserConnection[] conns = (UserConnection[])reader.Deserialize(typeof(UserConnection[]));
			return conns;
		}
		
		/** Deserializes nodes.
		 * Nodes can be saved to enable loading a full scanned graph from memory/file without scanning the graph first.
		 * \note Node info is stored in files named "graph#_nodes.binary" where # is the graph number.
		 * \note Connectivity info is stored in files named "graph#_conns.binary" where # is the graph number.
		 */
		public void DeserializeNodes () {
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) continue;
				
				if (zip.ContainsEntry("graph"+i+"_nodes"+binaryExt)) {
					//Create nodes
					graphs[i].nodes = graphs[i].CreateNodes (meta.nodeCounts[i]);
				} else {
					graphs[i].nodes = graphs[i].CreateNodes (0);
				}
			}
			
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) continue;
				
				ZipEntry entry = zip["graph"+i+"_nodes"+binaryExt];
				if (entry == null) continue;
				
				MemoryStream str = new MemoryStream();
				
				
				entry.Extract (str);
				str.Position = 0;
				BinaryReader reader = new BinaryReader(str);
				
				DeserializeNodes (i, reader);
			}
			
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) continue;
				
				ZipEntry entry = zip["graph"+i+"_conns"+binaryExt];
				if (entry == null) continue;
				
				MemoryStream str = new MemoryStream();
				
				entry.Extract (str);
				str.Position = 0;
				BinaryReader reader = new BinaryReader(str);
				
				DeserializeNodeConnections (i, reader);
			}
		}
		
		/** Deserializes extra graph info.
		 * Extra graph info is specified by the graph types.
		 * \see Pathfinding.NavGraph.DeserializeExtraInfo
		 * \note Stored in files named "graph#_extra.binary" where # is the graph number.
		 */
		public void DeserializeExtraInfo () {
			for (int i=0;i<graphs.Length;i++) {
				ZipEntry entry = zip["graph"+i+"_extra"+binaryExt];
				if (entry == null) continue;
				
				MemoryStream str = new MemoryStream();
				
				entry.Extract (str);
				byte[] bytes = str.ToArray();
				
				graphs[i].DeserializeExtraInfo (bytes);
			}
		}
		
		public void PostDeserialization () {
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null) continue;
				
				graphs[i].PostDeserialization();
			}
		}
		
		/** Deserializes nodes for a specified graph */
		private void DeserializeNodes (int index, BinaryReader reader) {
			
			Node[] nodes = graphs[index].nodes;
			
			if (nodes == null)
				throw new Exception ("No nodes exist in graph "+index+" even though it has been requested to create "+meta.nodeCounts[index]+" nodes");
			
			if (reader.BaseStream.Length < nodes.Length*(4*(3+1+1)))
				throw new Exception ("Expected more data than was available in stream when reading node data for graph "+index+" at position "+(reader.BaseStream.Position));
			
			int chunk = reader.ReadInt32();
			if (chunk != 1)
				throw new Exception ("Expected chunk 1 (positions) when reading node data for graph "+index+" at position "+(reader.BaseStream.Position-4)+" in stream");
			
			for (int i=0;i<nodes.Length;i++)
				nodes[i].position = new Int3(reader.ReadInt32(),reader.ReadInt32(),reader.ReadInt32());
			
			chunk = reader.ReadInt32();
			if (chunk != 2)
				throw new Exception ("Expected chunk 2 (penalties) when reading node data for graph "+index+" at position "+(reader.BaseStream.Position-4)+" in stream");
			
			for (int i=0;i<nodes.Length;i++)
				nodes[i].penalty = reader.ReadUInt32();
			
			chunk = reader.ReadInt32();
			if (chunk != 3)
				throw new Exception ("Expected chunk 3 (flags) when reading node data for graph "+index+" at position "+(reader.BaseStream.Position-4)+" in stream");
			
			for (int i=0;i<nodes.Length;i++)
				nodes[i].flags = reader.ReadInt32();
			
		}
		
		/** Deserializes node connections for a specified graph
		 */
		private void DeserializeNodeConnections (int index, BinaryReader reader) {
			
			Node[] nodes = graphs[index].nodes;
			
			for (int i=0;i<nodes.Length;i++) {
				Node node = nodes[i];
				
				int count = reader.ReadUInt16();
				node.connections = new Node[count];
				node.connectionCosts = new int[count];
				
				for (int j=0;j<count;j++) {
					int otherNodeIndex = reader.ReadInt32();
					int cost = reader.ReadInt32();
					node.connections[j] = GetNodeWithIndex (otherNodeIndex);
					node.connectionCosts[j] = cost;
				}
			}
		}
		
#if UNITY_EDITOR
		/** Deserializes graph editor settings.
		 * For future compatibility this method does not assume that the \a graphEditors array matches the #graphs array in order and/or count.
		 * It searches for a matching graph (matching if graphEditor.target == graph) for every graph editor.
		 * Multiple graph editors should not refer to the same graph.\n
		 * \note Stored in files named "graph#_editor.json" where # is the graph number.
		 */
		public void DeserializeEditorSettings (GraphEditorBase[] graphEditors) {
			
			if (graphEditors == null) return;
			
			for (int i=0;i<graphEditors.Length;i++) {
				if (graphEditors[i] == null) continue;
				for (int j=0;j<graphs.Length;j++) {
					if (graphs[j] == null || graphEditors[i].target != graphs[j]) continue;
					
					ZipEntry entry = zip["graph"+j+"_editor"+jsonExt];
					if (entry == null) continue;
					
					string entryText = GetString (entry);
					
					JsonReader reader = new JsonReader(entryText,readerSettings);
					reader.PopulateObject (graphEditors[i]);
					break;
				}
			}
			
			
		}
#endif
		
		/** Returns node with specified global index.
		 * Graphs must be deserialized or serialized first */
		public Node GetNodeWithIndex (int index) {
			if (graphs == null) throw new InvalidOperationException ("Cannot find node with index because graphs have not been serialized/deserialized yet");
			
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i].nodes.Length > index) {
					return graphs[i].nodes[index];
				} else {
					index -= graphs[i].nodes.Length;
				}
			}
			Debug.LogError ("Could not find node with index "+index);
			return null;
		}
		private string GetString (ZipEntry entry) {
			MemoryStream buffer = new MemoryStream();
			entry.Extract(buffer);
			buffer.Position = 0;
			StreamReader reader = new StreamReader(buffer);
			string s = reader.ReadToEnd();
			buffer.Position = 0;
			reader.Close();
			return s;
		}
		
		private GraphMeta DeserializeMeta (ZipEntry entry) {
			string s = GetString (entry);
			
			JsonReader reader = new JsonReader(s,readerSettings);
			return (GraphMeta)reader.Deserialize(typeof(GraphMeta));
			 //JsonConvert.DeserializeObject<GraphMeta>(s,settings);
		}
		
		
#endregion
		
#region Utils
		
		public static void SaveToFile (string path, byte[] data) {
			using (FileStream stream = new FileStream(path, FileMode.Create)) {
				stream.Write (data,0,data.Length);
			}
		}
		
		public static byte[] LoadFromFile (string path) {
			using (FileStream stream = new FileStream(path, FileMode.Open)) {
				byte[] bytes = new byte[(int)stream.Length];
				stream.Read (bytes,0,(int)stream.Length);
				return bytes;
			}
		}
		
#endregion		
	}
	
	/** Metadata for all graphs included in serialization */
	class GraphMeta {
		/** Project version it was saved with */
		public Version version;
		
		/** Number of graphs serialized */
		public int graphs;
		
		/** Guids for all graphs */
		public string[] guids;
		
		/** Type names for all graphs */
		public string[] typeNames;
		
		/** Number of nodes for every graph. Nodes are not necessarily serialized */
		public int[] nodeCounts;
		
		/** Returns the Type of graph number \a i */
		public Type GetGraphType (int i) {
			
			//The graph was null when saving. Ignore it
			if (typeNames[i] == null) return null;
			
#if ASTAR_FAST_NO_EXCEPTIONS
			System.Type[] types = AstarData.DefaultGraphTypes;
			
			Type type = null;
			for (int j=0;j<types.Length;j++) {
				if (types[j].FullName == typeNames[i]) type = types[j];
			}
#else
			Type type = Type.GetType (typeNames[i]);
#endif
			if (type != null)
				return type;
			else
				throw new Exception ("No graph of type '"+typeNames[i]+"' could be created, type does not exist");
		}
	}
	
	/** Holds settings for how graphs should be serialized */
	public class SerializeSettings {
		/** Is node data to be included in serialization */
		public bool nodes = true;
		public bool prettyPrint = false;
		
		/** Save editor settings. \warning Only applicable when saving from the editor using the AstarPathEditor methods */
		public bool editorSettings = false;
		
		/** Returns serialization settings for only saving graph settings */
		public static SerializeSettings Settings {
			get {
				SerializeSettings s = new SerializeSettings();
				s.nodes = false;
				return s;
			}
		}
		
		/** Returns serialization settings for saving everything the can be saved.
		 * This included all node data */
		public static SerializeSettings All {
			get {
				SerializeSettings s = new SerializeSettings();
				s.nodes = true;
				return s;
			}
		}
		
#if UNITY_EDITOR
		public void OnGUI () {
			nodes =  UnityEditor.EditorGUILayout.Toggle ("Save Node Data", nodes);
			prettyPrint = UnityEditor.EditorGUILayout.Toggle (new GUIContent ("Pretty Print","Format Json data for readability. Yields slightly smaller files when turned off"),prettyPrint);
		}
#endif
	}
	
}