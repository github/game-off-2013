//#define ASTAR_SizeProfile    //"Size Profile Debug" If enabled, size profiles will be logged when serializing a graph

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathfinding;

/** Simple profiler for what is written to a BinaryWriter stream */
public class SizeProfiler {
	
	public struct ProfileSizePoint {
		public long lastBegin;
		public long totalSize;
		public bool open;
	}
	
	private static Dictionary<string, ProfileSizePoint> profiles = new Dictionary<string, ProfileSizePoint>();
	private static string lastOpen = "";
	private static bool hasClosed = false;
	
	public static void Initialize () {
		profiles.Clear ();
	}
	
	[System.Diagnostics.Conditional ("ASTAR_SizeProfile")]
	public static void Begin (string s, BinaryWriter stream) {
		Begin (s, stream.BaseStream, true);
	}
	
	[System.Diagnostics.Conditional ("ASTAR_SizeProfile")]
	public static void Begin (string s, BinaryWriter stream, bool autoClosing) {
		Begin (s, stream.BaseStream, autoClosing);
	}
	
	[System.Diagnostics.Conditional ("ASTAR_SizeProfile")]
	public static void Begin (string s, Stream stream, bool autoClosing) {
		
		if (!hasClosed && profiles.ContainsKey(lastOpen)) {
			End (lastOpen, stream);
		}
		
		ProfileSizePoint p = new ProfileSizePoint ();
		
		if (!profiles.ContainsKey (s)) {
			profiles[s] = new ProfileSizePoint ();
		} else {
			p = profiles[s];
		}
		
		if (p.open) {
			Debug.LogWarning ("Opening an already open entry ("+s+")");
		}
		
		p.lastBegin = stream.Position;
		p.open = true;
		
		if (autoClosing) {
			hasClosed = false;
			lastOpen = s;
		}
		
		profiles[s] = p;
	}
	
	[System.Diagnostics.Conditional ("ASTAR_SizeProfile")]
	public static void End (string s, BinaryWriter stream) {
		End (s, stream.BaseStream);
	}
	
	[System.Diagnostics.Conditional ("ASTAR_SizeProfile")]
	public static void End (string s, Stream stream) {
		
		ProfileSizePoint p;
		
		if (!profiles.ContainsKey (s)) {
			Debug.LogError ("Can't end profile before one has started ("+s+")");
			return;
		} else {
			p = profiles[s];
		}
		
		if (!p.open) {
			Debug.LogWarning ("Cannot close an already closed entry ("+s+")");
			return;
		}
		
		hasClosed = true;
		p.totalSize += stream.Position - p.lastBegin;
		p.open = false;
		profiles[s] = p;
	}
	
	[System.Diagnostics.Conditional ("ASTAR_SizeProfile")]
	public static void Log () {
		System.Text.StringBuilder output = new System.Text.StringBuilder();
		output.Append("============================\n\t\t\t\tSize Profile results:\n============================\n");
		//foreach(KeyValuePair<string, ProfilePoint> pair in profiles)
		foreach(KeyValuePair<string, ProfileSizePoint> pair in profiles)
		{
			output.Append (pair.Key);
			output.Append ("	used	");
			output.Append (Mathfx.FormatBytes ((int)pair.Value.totalSize));
			output.Append ("\n");
		}
		Debug.Log (output.ToString ());
	}
}
