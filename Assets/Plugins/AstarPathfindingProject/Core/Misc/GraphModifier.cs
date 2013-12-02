using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	/** GraphModifier for modifying graphs or processing graph data based on events.
	 * This class is a simple container for a number of events.
	 * 
	 * \warning Some events will be called both in play mode <b>and in editor mode</b> (at least the scan events).
	 * So make sure your code handles both cases well. You may choose to ignore editor events.
	 * \see Application.IsPlaying
	 */
	public abstract class GraphModifier : MonoBehaviour {
		
		/** All active graph modifiers */
		static List<GraphModifier> activeModifiers = new List<GraphModifier>();
		
		/** Last frame a late scan event was triggered */
		static int lastLateScanEvent = -9999;
		/** Last frame a post cache event was triggered */
		static int lastPostCacheEvent = -9999;
		
		/** Returns all active graph modifiers in the scene */
		private static List<GraphModifier> GetActiveModifiers () {
			if (Application.isPlaying)
				return activeModifiers;
			else
				return new List<GraphModifier> (FindObjectsOfType(typeof(GraphModifier)) as GraphModifier[]);
		}
		
		/** GraphModifier event type.
		 * \see GraphModifier */
		public enum EventType {
			PostScan,
			PreScan,
			LatePostScan,
			PreUpdate,
			PostUpdate,
			PostCacheLoad
		}
		
		/** Triggers an event for all active graph modifiers */
		public static void TriggerEvent (GraphModifier.EventType type) {
			List<GraphModifier> mods = GetActiveModifiers();
			
			switch (type){
			case EventType.PreScan:
					for (int i=0;i<mods.Count;i++) mods[i].OnPreScan();
					break;
			case EventType.PostScan:
					for (int i=0;i<mods.Count;i++) mods[i].OnPostScan();
					break;
			case EventType.LatePostScan:
					lastLateScanEvent = Time.frameCount;
					for (int i=0;i<mods.Count;i++) mods[i].OnLatePostScan();
					break;
			case EventType.PreUpdate:
					for (int i=0;i<mods.Count;i++) mods[i].OnGraphsPreUpdate();
					break;
			case EventType.PostUpdate:
					for (int i=0;i<mods.Count;i++) mods[i].OnGraphsPostUpdate();
					break;
			case EventType.PostCacheLoad:
					lastPostCacheEvent = Time.frameCount;
					for (int i=0;i<mods.Count;i++) mods[i].OnPostCacheLoad ();
					break;
			}
		}
		
		/** Adds this modifier to list of active modifiers.
		 * Calls OnLatePostScan if the last late post scan event was triggered during this frame.
		 */
		public virtual void OnEnable () {
			activeModifiers.Add (this);
			if (lastLateScanEvent == Time.frameCount) {
				OnLatePostScan ();
			}
			if (lastPostCacheEvent == Time.frameCount) {
				OnPostCacheLoad ();
			}
		}
		
		/** Removes this modifier from list of active modifiers */
		public virtual void OnDisable () {
			activeModifiers.Remove (this);
		}
		
		/* Called just before a graph is scanned.
		  * Note that some other graphs might already be scanned */
		//public virtual void OnGraphPreScan (NavGraph graph) {}
		
		/* Called just after a graph has been scanned.
		  * Note that some other graphs might not have been scanned at this point. */
		//public virtual void OnGraphPostScan (NavGraph graph) {}
		
		/** Called right after all graphs have been scanned.
		 * FloodFill and other post processing has not been done.
		 * 
		 * \warning Since OnEnable and Awake are called roughly in the same time, the only way
		 * to ensure that these scripts get this call when scanning in Awake is to
		 * set the Script Execution Order for AstarPath to some time later than default time
		 * (see Edit -> Project Settings -> Script Execution Order).
		 * 
		 * \see OnLatePostScan
		 */
		public virtual void OnPostScan () {}
		
		/** Called right before graphs are going to be scanned.
		 * 
		 * \warning Since OnEnable and Awake are called roughly in the same time, the only way
		 * to ensure that these scripts get this call when scanning in Awake is to
		 * set the Script Execution Order for AstarPath to some time later than default time
		 * (see Edit -> Project Settings -> Script Execution Order).
		 * 
		 * \see OnLatePostScan
		 * */
		public virtual void OnPreScan () {}
		
		/** Called at the end of the scanning procedure.
		 * This is the absolute last thing done by Scan.
		 * 
		 * \note This event will be called even if Script Execution Order has messed things up
		 * (see the other two scan events).
		 */
		public virtual void OnLatePostScan () {}
		
		/** Called after cached graphs have been loaded.
		 * When using cached startup, this event is analogous to OnLatePostScan and implementing scripts
		 * should do roughly the same thing for both events.
		 * 
		 * \note This event will be called even if Script Execution Order has messed things up
		 * (see the other two scan events).
		 */
		public virtual void OnPostCacheLoad () {}
		
		/** Called before graphs are updated using GraphUpdateObjects */
		public virtual void OnGraphsPreUpdate () {}
		
		/** Called after graphs have been updated using GraphUpdateObjects.
		  * Eventual flood filling has been done */
		public virtual void OnGraphsPostUpdate () {}
	}
}