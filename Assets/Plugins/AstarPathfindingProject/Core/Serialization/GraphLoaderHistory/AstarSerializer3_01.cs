using UnityEngine;
using System.Collections;
using Pathfinding;
using System;
using System.Collections.Generic;

/** Deserializer for 3.01 Graphs.
  * This class will override deserialization functions which changed with the next version */
public class AstarSerializer3_01 : AstarSerializer3_04 {
	
	public AstarSerializer3_01 (AstarPath script) : base (script){}
	
	/** Serializes links placed by the user */
	public override void SerializeUserConnections (UserConnection[] userConnections) {
		Debug.Log ("Loading from 3.0.1");
		
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
				
				stream.Write (conn.overrideCost);
				
				stream.Write (conn.oneWay);
				stream.Write (conn.width);
				
				//stream.Write ((int)conn.type);
				Debug.Log ("End - "+stream.BaseStream.Position);
			}
		} else {
			stream.Write (0);
		}
	}
	
	/** Deserializes links placed by the user */
	public override UserConnection[] DeserializeUserConnections () {
		System.IO.BinaryReader stream = readerStream;
		
		if (MoveToAnchor ("UserConnections")) {
			int count = stream.ReadInt32 ();
			
			UserConnection[] userConnections = new UserConnection[count];
			
			for (int i=0;i<count;i++) {
				UserConnection conn = new UserConnection ();
				conn.p1 = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
				conn.p2 = new Vector3 (stream.ReadSingle (),stream.ReadSingle (),stream.ReadSingle ());
				
				conn.overrideCost = stream.ReadInt32 ();
				
				conn.oneWay = stream.ReadBoolean ();
				conn.width = stream.ReadSingle ();
				userConnections[i] = conn;
			}
			return userConnections;
		} else {
			return new UserConnection[0];
		}
	}
}
