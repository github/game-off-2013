//Uncomment the next line to enable debugging (also uncomment it in AstarPath.cs)
//#define ProfileAstar
//#define UNITY_PRO_PROFILER //Requires ProfileAstar, profiles section of astar code which will show up in the Unity Pro Profiler.

using System.Collections.Generic;
using System;
using UnityEngine;

public class AstarProfiler
{
	public struct ProfilePoint
	{
		public DateTime lastRecorded;
		public TimeSpan totalTime;
		public int totalCalls;
	}
	
	private static Dictionary<string, ProfilePoint> profiles = new Dictionary<string, ProfilePoint>();
	private static DateTime startTime = DateTime.UtcNow;
	
	public static ProfilePoint[] fastProfiles;
	public static string[] fastProfileNames;
	
	private AstarProfiler()
	{
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void InitializeFastProfile (string[] profileNames) {
		fastProfileNames = profileNames;
		fastProfiles = new ProfilePoint[profileNames.Length];
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void StartFastProfile(int tag)
	{
		//profiles.TryGetValue(tag, out point);
		fastProfiles[tag].lastRecorded = DateTime.UtcNow;
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void EndFastProfile(int tag)
	{
		DateTime now = DateTime.UtcNow;
		/*if (!profiles.ContainsKey(tag))
		{
			Debug.LogError("Can only end profiling for a tag which has already been started (tag was " + tag + ")");
			return;
		}*/
		ProfilePoint point = fastProfiles[tag];
		point.totalTime += now - point.lastRecorded;
		point.totalCalls++;
		fastProfiles[tag] = point;
	}
	
	[System.Diagnostics.Conditional ("UNITY_PRO_PROFILER")]
	public static void EndProfile () {
		Profiler.EndSample ();
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void StartProfile(string tag)
	{
		//Console.WriteLine ("Profile Start - " + tag);
		ProfilePoint point;
		
		profiles.TryGetValue(tag, out point);
		point.lastRecorded = DateTime.UtcNow;
		profiles[tag] = point;
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void EndProfile(string tag)
	{
		if (!profiles.ContainsKey(tag))
		{
			Debug.LogError("Can only end profiling for a tag which has already been started (tag was " + tag + ")");
			return;
		}
		//Console.WriteLine ("Profile End - " + tag);
		DateTime now = DateTime.UtcNow;
		ProfilePoint point = profiles[tag];
		point.totalTime += now - point.lastRecorded;
		++point.totalCalls;
		profiles[tag] = point;
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void Reset()
	{
		profiles.Clear();
		startTime = DateTime.UtcNow;
		
		if (fastProfiles != null) {
			for (int i=0;i<fastProfiles.Length;i++) {
				fastProfiles[i] = new ProfilePoint ();
			}
		}
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void PrintFastResults()
	{
		TimeSpan endTime = DateTime.UtcNow - startTime;
		System.Text.StringBuilder output = new System.Text.StringBuilder();
		output.Append("============================\n\t\t\t\tProfile results:\n============================\n");
		output.Append ("Name		|	Total Time	|	Total Calls	|	Avg/Call	");
		//foreach(KeyValuePair<string, ProfilePoint> pair in profiles)
		for (int i=0;i<fastProfiles.Length;i++)
		{
			string name = fastProfileNames[i];
			ProfilePoint value = fastProfiles[i];
			
			double totalTime = value.totalTime.TotalMilliseconds;
			int totalCalls = value.totalCalls;
			if (totalCalls < 1) continue;
			
			
			output.Append ("\n").Append(name.PadLeft(10)).Append("|   ");
			output.Append (totalTime.ToString("0.0").PadLeft (10)).Append ("|   ");
			output.Append (totalCalls.ToString().PadLeft (10)).Append ("|   ");
			output.Append ((totalTime / totalCalls).ToString("0.000").PadLeft(10));
			
			/* output.Append("\nProfile");
			output.Append(name);
			output.Append(" took \t");
			output.Append(totalTime.ToString("0.0"));
			output.Append(" ms to complete over ");
			output.Append(totalCalls);
			output.Append(" iteration");
			if (totalCalls != 1) output.Append("s");
			output.Append(", averaging \t");
			output.Append((totalTime / totalCalls).ToString("0.000"));
			output.Append(" ms per call"); */
			
		}
		output.Append("\n\n============================\n\t\tTotal runtime: ");
		output.Append(endTime.TotalSeconds.ToString("F3"));
		output.Append(" seconds\n============================");
		Debug.Log(output.ToString());
	}
	
	[System.Diagnostics.Conditional ("ProfileAstar")]
	public static void PrintResults()
	{
		TimeSpan endTime = DateTime.UtcNow - startTime;
		System.Text.StringBuilder output = new System.Text.StringBuilder();
		output.Append("============================\n\t\t\t\tProfile results:\n============================\n");
		
		int maxLength = 5;
		foreach(KeyValuePair<string, ProfilePoint> pair in profiles)
		{
			maxLength = Math.Max (pair.Key.Length,maxLength);
		}
		
		output.Append (" Name ".PadRight (maxLength)).
			Append("|").Append(" Total Time	".PadRight(20)).
			Append("|").Append(" Total Calls ".PadRight(20)).
			Append("|").Append(" Avg/Call ".PadRight(20));
		
		
		
		foreach(KeyValuePair<string, ProfilePoint> pair in profiles)
		{
			double totalTime = pair.Value.totalTime.TotalMilliseconds;
			int totalCalls = pair.Value.totalCalls;
			if (totalCalls < 1) continue;
			
			string name = pair.Key;
			
			output.Append ("\n").Append(name.PadRight(maxLength)).Append("| ");
			output.Append (totalTime.ToString("0.0").PadRight (20)).Append ("| ");
			output.Append (totalCalls.ToString().PadRight (20)).Append ("| ");
			output.Append ((totalTime / totalCalls).ToString("0.000").PadRight(20));
			
			/*output.Append("\nProfile ");
			output.Append(pair.Key);
			output.Append(" took ");
			output.Append(totalTime.ToString("0"));
			output.Append(" ms to complete over ");
			output.Append(totalCalls);
			output.Append(" iteration");
			if (totalCalls != 1) output.Append("s");
			output.Append(", averaging ");
			output.Append((totalTime / totalCalls).ToString("0.0"));
			output.Append(" ms per call");*/
		}
		output.Append("\n\n============================\n\t\tTotal runtime: ");
		output.Append(endTime.TotalSeconds.ToString("F3"));
		output.Append(" seconds\n============================");
		Debug.Log(output.ToString());
	}
}