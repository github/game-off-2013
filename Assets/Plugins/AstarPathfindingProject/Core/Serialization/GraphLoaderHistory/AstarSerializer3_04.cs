using UnityEngine;
using System.Collections;
using Pathfinding;
using System;
using System.Collections.Generic;

/** Deserializer for 3.04 Graphs.
  * This class will override deserialization functions which changed with the next version */
public class AstarSerializer3_04 : AstarSerializer3_05 {
	
	public AstarSerializer3_04 (AstarPath script) : base (script){}
	
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
				conn.type = (ConnectionType)stream.ReadInt32 ();
				userConnections[i] = conn;
			}
			return userConnections;
		} else {
			return new UserConnection[0];
		}
	}
			
}
