//Define optimizations is a A* Pathfinding Project Pro only feature
//#define ProfileAstar	//Enables profiling of the pathfinding process
//#define ASTARDEBUG			//Enables more debugging messages, enable if this script is behaving weird (crashing or throwing NullReference exceptions or something)
//#define NoGUI			//Disables the use of the OnGUI function, can eventually improve performance by a tiny bit (disables the InGame option for path debugging)
//#define ASTAR_SINGLE_THREAD_OPTIMIZE //Optimizes performance and memory for single and dual core computers/smartphones. Recommended if you only run pathfinding in on thread or in the unity thread. Reduces memory usage quite a lot.
//#define ASTAR_MORE_PATH_IDS //Increases the number of pathIDs from 2^16 to 2^32. Uses more memory.
//#define ASTAR_FAST_BUT_NO_EXCEPTIONS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Pathfinding;

[AddComponentMenu ("Pathfinding/Pathfinder")]
/** Main Pathfinding System.
 * This class handles all the pathfinding system, calculates all paths and stores the info.\n
 * This class is a singleton class, meaning it should only exist at most one active instance of it in the scene.\n
 * It might be a bit hard to use directly, usually interfacing with the pathfinding system is done through the Seeker class.
 * 
 * \nosubgrouping
 * \ingroup relevant */
public class AstarPath : MonoBehaviour {
	
	/** The version number for the A* %Pathfinding Project
	 */
	public static System.Version Version {
		get {
			return new System.Version (3,2,5,1);
		}
	}
	
	public enum AstarDistribution { WebsiteDownload, AssetStore };
	
	/** Used by the editor to guide the user to the correct place to download updates */
	public static readonly AstarDistribution Distribution = AstarDistribution.WebsiteDownload;
	
	/** Used by the editor to show some Pro specific stuff.
	 * Note that setting this to true will not grant you any additional features */
	public static readonly bool HasPro = false;
	
	/** See Pathfinding.AstarData */
	public System.Type[] graphTypes {
		get {
			return astarData.graphTypes;
		}
	}
	
	/** Reference to the Pathfinding.AstarData object for this graph. The AstarData object stores information about all graphs. */
	public AstarData astarData;
	
	/** Returns the active AstarPath object in the scene.*/
	public new static AstarPath active;
	
	/** Shortcut to Pathfinding.AstarData.graphs */
	public NavGraph[] graphs {
		get {
			if (astarData == null)
				astarData = new AstarData ();
			return astarData.graphs;
		}
		set {
			if (astarData == null)
				astarData = new AstarData ();
			astarData.graphs = value;
		}
	}
	
#region InspectorDebug
	/** @name Inspector - Debug
	 * @{ */
	
	/** Toggle for showing the gizmo debugging for the graphs in the scene view (editor only). */
	public bool showNavGraphs = true;
	
	/** Toggle to show unwalkable nodes.
	  * \see unwalkableNodeDebugSize */
	public bool showUnwalkableNodes = true;
	
	/** The mode to use for drawing nodes in the sceneview.
	 * \see Pathfinding.GraphDebugMode
	 */
	public GraphDebugMode debugMode;
	
	/** Low value to use for certain #debugMode modes.
	 * For example if #debugMode is set to G, this value will determine when the node will be totally red.
	 * \see #debugRoof
	 */
	public float debugFloor = 0;
	
	/** High value to use for certain #debugMode modes.
	 * For example if #debugMode is set to G, this value will determine when the node will be totally green.
	 * \see #debugFloor
	 */
	public float debugRoof = 20000;
	
	/** If enabled, nodes will draw a line to their 'parent'.
	 * This will show the search tree for the latest path. This is editor only.
	 * \todo Add a showOnlyLastPath flag to indicate whether to draw every node or only the ones visited by the latest path.
	 */
	public bool	showSearchTree = false;
	
	/** Size of the red cubes shown in place of unwalkable nodes.
	  * \see showUnwalkableNodes */
	public float unwalkableNodeDebugSize = 0.3F;
	
	/** If enabled, only one node will be searched per search iteration (frame).
	 * Used for debugging
	 * \note Might not apply for all path types
	 * \deprecated Probably does not work for ANY path types
	 */
	public bool stepByStep = true;
	
	/** The amount of debugging messages.
	 * Use less debugging to improve performance (a bit) or just to get rid of the Console spamming.\n
	 * Use more debugging (heavy) if you want more information about what the pathfinding is doing.\n
	 * InGame will display the latest path log using in game GUI. */
	public PathLog logPathResults = PathLog.Normal;
	
	/** @} */
#endregion
	
#region InspectorSettings
	/** @name Inspector - Settings
	 * @{ */
	
	/** Max Nearest Node Distance.
	 * When searching for a nearest node, this is the limit (world units) for how far away it is allowed to be.
	 * \see Pathfinding.NNConstraint.constrainDistance
	 */
	public float maxNearestNodeDistance = 100;
	
	/** Max Nearest Node Distance Squared.
	 * \see #maxNearestNodeDistance */
	public float maxNearestNodeDistanceSqr {
		get { return maxNearestNodeDistance*maxNearestNodeDistance; }
	}
	
	/** If true, all graphs will be scanned in Awake.
	 * This does not include loading from the cache.
	 * If you disable this, you will have to call \link Scan AstarPath.active.Scan () \endlink yourself to enable pathfinding,
	 * alternatively you could load a saved graph from a file.
	 */
	public bool scanOnStartup = true;
	
	/** Do a full GetNearest search for all graphs.
	 * Additinal searches will normally only be done on the graph which in the first, fast searches proved to have the closest node.
	 * With this setting on, additional searches will be done on all graphs.\n
	 * More technically: GetNearestForce on all graphs will be called if true, otherwise only on the one graph which's GetNearest search returned the best node.\n
	 * Usually faster when disabled, but higher quality searches when enabled.
	 * When using a a navmesh or recast graph, for best quality, this setting should be combined with the Pathfinding.NavMeshGraph.accurateNearestNode setting set to true.
	 * \note For the PointGraph this setting doesn't matter much as it has only one search mode.
	 */
	public bool fullGetNearestSearch = false;
	
	/** Prioritize graphs.
	 * Graphs will be prioritized based on their order in the inspector.
	 * The first graph which has a node closer than #prioritizeGraphsLimit will be chosen instead of searching all graphs.
	 */
	public bool prioritizeGraphs = false;
	
	/** Distance limit for #prioritizeGraphs.
	 * \see #prioritizeGraphs
	 */
	public float prioritizeGraphsLimit = 1F;
	
	/** Reference to the color settings for this AstarPath object.
	 * Color settings include for example which color the nodes should be in, in the sceneview. */
	public AstarColor colorSettings;
	
	/** Stored tag names.
	 * \see AstarPath.FindTagNames
	 * \see AstarPath.GetTagNames
	 */
	[SerializeField]
	protected string[] tagNames = null;
	
	/** The heuristic to use.
	 * The heuristic, often referred to as 'H' is the estimated cost from a node to the target.
	 * Different heuristics affect how the path picks which one to follow from multiple possible with the same length
	 * \see Pathfinding.Heuristic
	 */
	public Heuristic heuristic = Heuristic.Euclidean;
	
	/** The scale of the heuristic. If a smaller value than 1 is used, the pathfinder will search more nodes (slower).
	 * If 0 is used, the pathfinding will be equal to dijkstra's algorithm.
	 * If a value larger than 1 is used the pathfinding will (usually) be faster because it expands fewer nodes, but the paths might not longer be optimal
	 */
	public float heuristicScale = 1F;

	/** Number of pathfinding threads to use.
	 * Multithreading puts pathfinding in another thread, this is great for performance on 2+ core computers since the framerate will barely be affected by the pathfinding at all.
	 * But this can cause strange errors and pathfinding stopping to work if you are not carefull (that is, if you are modifying the pathfinding scripts). For basic usage (not modding the pathfinding core) it should be safe.\n
	 * - None indicates that the pathfinding is run in the Unity thread as a coroutine
	 * - Automatic will try to adjust the number of threads to the number of cores and memory on the computer.
	 * 	Less than 512mb of memory or a single core computer will make it revert to using no multithreading
	 * \see CalculateThreadCount
	 * \astarpro
	 */
	public ThreadCount threadCount = ThreadCount.None;
	
	/** Max number of milliseconds to spend each frame for pathfinding.
	 * At least 500 nodes will be searched each frame (if there are that many to search).
	 * When using multithreading this value is quite irrelevant,
	 * but do not set it too low since that could add upp to some overhead, 10ms will work good for multithreading */
	public float maxFrameTime = 1F;
	
	/** The initial max size of the binary heap.
	 * The binary heaps will be expanded if necessary.
	 */
	public const int InitialBinaryHeapSize = 512;
	
	/** Recycle paths to reduce memory allocations.
	 * This will put paths in a pool to be reused over and over again.
	 * If you use this, your scripts using tht paths should copy the vectorPath array and node array (if used) because when the path is recycled,
	 * those arrays will be replaced.
	 * I.e you should not get data from it using myPath.someVariable (except when you get the path callback) because 'someVariable' might be changed when the path is recycled.
	 * \note This feature is currently disabled because it didn't lead to much better memory management, but could seriously screw up stuff if the user was not careful.
	 */
	public bool recyclePaths = false;
	
	/** Defines the minimum amount of nodes in an area.
	 * If an area has less than this amount of nodes, the area will be flood filled again with the area ID 254,
	 * it shouldn't affect pathfinding in any significant way.\n
	 * If you want to be able to separate areas from one another for some reason (for example to do a fast check to see if a path is at all possible)
	 * you should set this variable to 0.\n
	  * Can be found in A* Inspector-->Settings-->Min Area Size
	  */
	public int minAreaSize = 10;
	
	/** Limit graph updates. If toggled, graph updates will be executed less often (specified by #maxGraphUpdateFreq).*/
	public bool limitGraphUpdates = true;
	
	/** How often should graphs be updated. If #limitGraphUpdates is true, this defines the minimum amount of seconds between each graph update.*/
	public float maxGraphUpdateFreq = 0.2F;
	
	/** @} */
#endregion
	
#region DebugVariables
	/** @name Debug Members
	 * @{ */
	
	/** How many paths has been computed this run. From application start.\n
	 * Debugging variable */
	public static int PathsCompleted = 0;
	
	public static System.Int64 				TotalSearchedNodes = 0;
	public static System.Int64			 	TotalSearchTime = 0;
	
	/** The time it took for the last call to Scan() to complete.
	 * Used to prevent automatically rescanning the graphs too often (editor only) */
	public float lastScanTime = 0F;
	
	/** The path to debug using gizmos.
	 * This is equal to the last path which was calculated,
	 * it is used in the editor to draw debug information using gizmos.*/
	public Path debugPath;
	
	/** NodeRunData from #debugPath.
	 * Returns null if #debugPath is null
	 */
	public NodeRunData debugPathData {
		get {
			if (debugPath == null) return null;
			return debugPath.runData;
		}
	}
	
	/** This is the debug string from the last completed path.
	 * Will be updated if #logPathResults == PathLog.InGame */
	public string inGameDebugPath;
	
	/* @} */
#endregion
	
#region StatusVariables
	
	/** Set when scanning is being done. It will be true up until the FloodFill is done.
	 * Used to better support Graph Update Objects called for example in OnPostScan */
	public bool isScanning = false;
	
	/** Disables or enables new paths to be added to the queue.
	  * Setting this to false also makes the pathfinding thread (if using multithreading) to abort as soon as possible.
	  * It is used when OnDestroy is called to abort the pathfinding thread. */
	private bool acceptNewPaths = true;
	
	/** Number of threads currently working.
	  * Threads are sleeping when there is no work to be done or there is a work block (such as when updating graphs) */
	private static int numActiveThreads = 0;
	
	/** Number of threads currently active.
	 * This includes the pathfinding coroutine used for non-multithreading mode.
	  */
	public static int ActiveThreadsCount {
		get { return numActiveThreads; }
	}
	
	/** Number of parallel pathfinders.
	 * Returns the number of concurrent processes which can calculate paths at once.
	 * When using multithreading, this will be the number of threads, if not using multithreading it is always 1 (since only 1 coroutine is used).
	 * \see threadInfos
	 * \see IsUsingMultithreading
	 */
	public static int NumParallelThreads {
		get {
			return threadInfos != null ? threadInfos.Length : 0;
		}
	}
	
	/** Returns whether or not multithreading is used.
	 * \exception System.Exception Is thrown when it could not be decided if multithreading was used or not.
	 * This should not happen if pathfinding is set up correctly.
	 * \note This uses info about if threads are running right now, it does not use info from the settings on the A* object.
	 */
	public static bool IsUsingMultithreading {
		get {
			if (threads != null && threads.Length > 0)
				return true;
			else if (threads != null && threads.Length == 0 && threadEnumerator != null)
				return false;
			else
				throw new System.Exception ("Not 'using threading' and not 'not using threading'... Are you sure pathfinding is set up correctly?\nIf scripts are reloaded in unity editor during play this could happen.");
		}
	}
	
	/** Returns if any graph updates are waiting to be applied */
	public bool IsAnyGraphUpdatesQueued { get { return graphUpdateQueue != null && graphUpdateQueue.Count > 0; }}
	
	private bool graphUpdateRoutineRunning = false;
	
	private bool isUpdatingGraphs = false;
	private bool isRegisteredForUpdate = false;
#endregion
	
#region Callbacks
	/** @name Callbacks */
	 /* Callbacks to pathfinding events.
	 * These allow you to hook in to the pathfinding process.\n
	 * Callbacks can be used like this:
	 * \code
	 * public void Start () {
	 * 	AstarPath.OnPostScan += SomeFunction;
	 * }
	 * 
	 * public void SomeFunction (AstarPath active) {
	 * 	//This will be called every time the graphs are scanned
	 * }
	 * \endcode
	*/
	 /** @{ */
	
	/** Called on Awake before anything else is done.
	  * This is called at the start of the Awake call, right after #active has been set, but this is the only thing that has been done.\n
	  * Use this when you want to set up default settings for an AstarPath component created during runtime since some settings can only be changed in Awake
	  * (such as multithreading related stuff)
	  * \code
	  * //Create a new AstarPath object on Start and apply some default settings
	  * public void Start () {
	  * 	AstarPath.OnAwakeSettings += ApplySettings;
	  * 	AstarPath astar = AddComponent<AstarPath>();
	  * }
	  * 
	  * public void ApplySettings () {
	  * 	//Unregister from the delegate
	  * 	AstarPath.OnAwakeSettings -= ApplySettings;
	  * 	
	  * 	//For example useMultithreading should not be changed after the Awake call
	  * 	//so here's the only place to set it if you create the component during runtime
	  * 	AstarPath.active.useMultithreading = true;
	  * }
	  * \endcode
	  */
	public static OnVoidDelegate OnAwakeSettings;
	
	public static OnGraphDelegate OnGraphPreScan; /**< Called for each graph before they are scanned */
	
	public static OnGraphDelegate OnGraphPostScan; /**< Called for each graph after they have been scanned. All other graphs might not have been scanned yet. */
	
	public static OnPathDelegate OnPathPreSearch; /**< Called for each path before searching. Be carefull when using multithreading since this will be called from a different thread. */
	public static OnPathDelegate OnPathPostSearch; /**< Called for each path after searching. Be carefull when using multithreading since this will be called from a different thread. */
	
	public static OnScanDelegate OnPreScan; /**< Called before starting the scanning */
	public static OnScanDelegate OnPostScan; /**< Called after scanning. This is called before applying links, flood-filling the graphs and other post processing. */
	public static OnScanDelegate OnLatePostScan; /**< Called after scanning has completed fully. This is called as the last thing in the Scan function. */
	
	public static OnScanDelegate OnGraphsUpdated; /**< Called when any graphs are updated. Register to for example recalculate the path whenever a graph changes. */
	
	/** Called when \a pathID overflows 65536.
	 * The Pathfinding.CleanupPath65K will be added to the queue, and directly after, this callback will be called.
	 * \note This callback will be cleared every timed it is called, so if you want to register to it repeatedly, register to it directly on receiving the callback as well. 
	 */
	public static OnVoidDelegate On65KOverflow;
	
	/** Will send a callback when it is safe to update the nodes. Register to this with RegisterSafeNodeUpdate
	 * When it is safe is defined as between the path searches.
	 * This callback will only be sent once and is nulled directly after the callback is sent.
	 * \warning Note that these callbacks are not thread safe when using multithreading, DO NOT call any part of the Unity API from these callbacks except for Debug.Log
	 */
	private static OnVoidDelegate OnSafeCallback;
	
	/** Will send a callback when it is safe to update the nodes. Register to this with RegisterThreadSafeNodeUpdate
	 * When it is safe is defined as between the path searches.
	 * This callback will only be sent once and is nulled directly after the callback is sent.
	 * \see OnSafeCallback
	 */
	private static OnVoidDelegate OnThreadSafeCallback;
	
	/** Used to enable gizmos in editor scripts.
	  * Used internally by the editor, do not use this in game code */
	public OnVoidDelegate OnDrawGizmosCallback;
	
	public OnVoidDelegate OnGraphsWillBeUpdated;
	public OnVoidDelegate OnGraphsWillBeUpdated2;
	
	/* @} */
#endregion
	
#region MemoryStructures
	
	/** Stack containing all waiting graph update queries. Add to this stack by using \link UpdateGraphs \endlink
	 * \see UpdateGraphs
	 */
	[System.NonSerialized]
	public Queue<GraphUpdateObject> graphUpdateQueue;
	
	/** Stack used for flood-filling the graph. It is saved to minimize memory allocations. */
	[System.NonSerialized]
	public Stack<Node> floodStack;
	
	/** \todo Check scene switches */
	public static Queue<Path> pathQueue = new Queue<Path> ();
	
	public static Thread[] threads;
	
	/** Holds info about each thread.
	 * The first item will hold information about the pathfinding coroutine when not using multithreading.
	 */
	public static PathThreadInfo[] threadInfos;
	
	/** When no multithreading is used, the IEnumerator is stored here.
	 * When no multithreading is used, a coroutine is used instead. It is not directly called with StartCoroutine
	 * but a separate function has just a while loop which increments the main IEnumerator.
	 * This is done so other functions can step the thread forward at any time, without having to wait for Unity to update it.
	 * \see CalculatePaths
	 * \see CalculatePathsHandler
	 */
	public static IEnumerator threadEnumerator;
	public static Pathfinding.Util.LockFreeStack pathReturnStack = new Pathfinding.Util.LockFreeStack();
	
	/** Stack to hold paths waiting to be recycled */
	public static Stack<Path> PathPool;
	
#endregion
	
	/** Shows or hides graph inspectors.
	 * Used internally by the editor */
	public bool showGraphs = false;
	
	/** The last area index which was used.
	 * Used for the \link FloodFill(Node node) FloodFill \endlink function to start flood filling with an unused area.
	 * \see FloodFill(Node node)
	 */
	public int lastUniqueAreaIndex = 0;
	
#region ThreadingMembers
	
	/** Flag set if there are paths to calculate.
	 * Might get reset if a function wants to pause pathfinding (for example to update graphs)
	 */
	private static readonly ManualResetEvent pathQueueFlag = new ManualResetEvent (false);		/**< Thread flag, reset if there are no more paths in the queue */
	private static readonly ManualResetEvent threadSafeUpdateFlag = new ManualResetEvent (false);	/**< Thread flag, reset while a thread wait for the Unity thread to call threadsafe updates */
	private static readonly ManualResetEvent safeUpdateFlag = new ManualResetEvent (false);		/**< Thread flag, set if safe or thread safe updates need to be done */
	private static bool threadSafeUpdateState = false;
	private static readonly System.Object safeUpdateLock = new object();
	private static bool doSetQueueState = true;
	
	/** Resets all event flags.
	 * This should be called when resetting the static members of AstarPath.
	 * \see OnDestroy
	 */
	private static void ResetQueueStates () {
		pathQueueFlag.Reset ();
		threadSafeUpdateFlag.Reset();
		safeUpdateFlag.Reset();
		threadSafeUpdateState = false;
		doSetQueueState = true;
	}
	
	/** This will trick threads to think there is more work to be done and then abort them gracefully.
	 * When they think there is more work to be done, they will first test if new paths should be accepted, now they are not, so the threads will abort gracefully.
	 * Only call this when you want to abort all threads. For example in the OnDestroy function.
	 * After this function has been called, it's good to loop through all threads and Thread.Join them with some timeout.
	 * If the timeout exceeds, you can call Thread.Abort on them
	 * \see OnDestroy
	 * \see acceptNewPaths
	 */
	private static void TrickAbortThreads () {
		AstarPath.active.acceptNewPaths = false;
		pathQueueFlag.Set();
	}
	
#endregion
	
	/** Time the last graph update was done.
	 * Used to group together frequent graph updates to batches */
	private float lastGraphUpdate = -9999F;
	
	/** The next unused Path ID.
	 * Incremented for every call to GetFromPathPool */
	private ushort nextFreePathID = 1;
	
	/** Returns tag names.
	 * Makes sure that the tag names array is not null and of length 32.
	 * If it is null or not of length 32, it creates a new array and fills it with 0,1,2,3,4 etc...
	 * \see AstarPath.FindTagNames
	 */
	public string[] GetTagNames () {
		
		if (tagNames == null || tagNames.Length	!= 32) {
			tagNames = new string[32];
			for (int i=0;i<tagNames.Length;i++) {
				tagNames[i] = ""+i;
			}
			tagNames[0] = "Basic Ground";
		}
		return tagNames;
	}
	
	/** Tries to find an AstarPath object and return tag names.
	 * If an AstarPath object cannot be found, it returns an array of length 1 with an error message.
	 * \see AstarPath.GetTagNames
	 */
	public static string[] FindTagNames () {
		if (active != null) return active.GetTagNames ();
		else {
			AstarPath a = GameObject.FindObjectOfType (typeof (AstarPath)) as AstarPath;
			if (a != null) { active = a; return a.GetTagNames (); }
			else {
				return new string[1] {"There is no AstarPath component in the scene"};
			}
		}
	}
	
	/** Returns the next free path ID. If the next free path ID overflows 65535, a cleanup operation is queued
	  * \see Pathfinding.CleanupPath65K */
	public ushort GetNextPathID ()
	{
		if (nextFreePathID == 0) {
			nextFreePathID++;
			
			//Queue a cleanup operation to zero all path IDs
			//StartPath (new CleanupPath65K ());
			Debug.Log ("65K cleanup");
			
			//ushort toBeReturned = nextFreePathID;
			
			if (On65KOverflow != null) {
				OnVoidDelegate tmp = On65KOverflow;
				On65KOverflow = null;
				tmp ();
			}
			
			//return nextFreePathID++;
		}
		return nextFreePathID++;
	}
	
	
	/** Calls OnDrawGizmos on graph generators and also #OnDrawGizmosCallback */
	private void OnDrawGizmos () {
		AstarProfiler.StartProfile ("OnDrawGizmos");
		
		if (active == null) {
			active = this;
		} else if (active != this) {
			return;
		}
		
		if (graphs == null) return;
		
		for (int i=0;i<graphs.Length;i++) {
			if (graphs[i] == null) continue;
			
			if (graphs[i].drawGizmos)
				graphs[i].OnDrawGizmos (showNavGraphs);
		}
		
		if (showUnwalkableNodes && showNavGraphs) {
			Gizmos.color = AstarColor.UnwalkableNode;
			for (int i=0;i<graphs.Length;i++) {
				if (graphs[i] == null || graphs[i].nodes == null) continue;
				Node[] nodes = graphs[i].nodes;
				for (int j=0;j<nodes.Length;j++) {
					if (nodes[j] != null && !nodes[j].walkable) {
						Gizmos.DrawCube ((Vector3)nodes[j].position, Vector3.one*unwalkableNodeDebugSize);
					}
				}
			}
		}
		
		if (OnDrawGizmosCallback != null) {
			OnDrawGizmosCallback ();
		}
		
		AstarProfiler.EndProfile ("OnDrawGizmos");
	}
	
	/** Draws the InGame debugging (if enabled), also shows the fps if 'L' is pressed down.
	 * \see #logPathResults PathLog
	 */
	private void OnGUI () {
		
		if (Input.GetKey ("l")) {
			GUI.Label (new Rect (Screen.width-100,5,100,25),(1F/Time.smoothDeltaTime).ToString ("0")+" fps");
		}
		
		if (logPathResults == PathLog.InGame) {
			
			if (inGameDebugPath != "") {
						
				GUI.Label (new Rect (5,5,400,600),inGameDebugPath);
			}
		}
		
		/*if (GUI.Button (new Rect (Screen.width-100,5,100,20),"Load New Level")) {
			Application.LoadLevel (0);
		}*/
		
	}
	
#line hidden
	/** Logs a string while taking into account #logPathResults */
	private static void AstarLog (string s) {
		if (active == null) {
			Debug.Log ("No AstarPath object was found : "+s);
			return;
		}
		
		if (active.logPathResults != PathLog.None && active.logPathResults != PathLog.OnlyErrors) {
			Debug.Log (s);
		}
	}
	
	/** Logs an error string while taking into account #logPathResults */
	private static void AstarLogError (string s) {
		if (active == null) {
			Debug.Log ("No AstarPath object was found : "+s);
			return;
		}
		
		if (active.logPathResults != PathLog.None) {
			Debug.LogError (s);
		}
	}
#line default

	/** Prints path results to the log. What it prints can be controled using #logPathResults.
	 * \see #logPathResults
	 * \see PathLog
	 * \see Pathfinding.Path.DebugString
	 */
	private void LogPathResults (Path p) {
		
		if (logPathResults == PathLog.None || (logPathResults == PathLog.OnlyErrors && !p.error)) {
			return;
		}
		
		string debug = p.DebugString (logPathResults);
		
		if (logPathResults == PathLog.InGame) {
			inGameDebugPath = debug;
		} else {
			Debug.Log (debug);
		}
	}
	
	
	/* Checks if the OnThreadSafeCallback callback needs to be (and can) be called and if so, does it.
	 * Unpauses pathfinding threads after that.
	 * \see CallThreadSafeCallbacks
	 */
	private void Update () {
		TryCallThreadSafeCallbacks ();
	}
	
	/* Checks if the OnThreadSafeCallback callback needs to be (and can) be called and if so, does it.
	 * Unpauses pathfinding threads after that.
	 * Thread safe callbacks can only be called when no pathfinding threads are running at the momment.
	 * Should only be called from the main unity thread.
	 * \see FlushThreadSafeCallbacks
	 * \see Update
	 */
	private static void TryCallThreadSafeCallbacks () {
		if (threadSafeUpdateState) {
			if (OnThreadSafeCallback != null) {
				OnVoidDelegate tmp = OnThreadSafeCallback;
				OnThreadSafeCallback = null;
				tmp ();
			}
			threadSafeUpdateFlag.Set();
			threadSafeUpdateState = false;
		}
	}
	
	/** Forces thread safe callbacks to be called.
	 * This method should only be called from inside another thread safe callback to for example instantly run some graph updates.
	 * \throws System.InvalidOperationException if any threads are detected to be active and running
	 */
	public static void ForceCallThreadSafeCallbacks () {
		if (!threadSafeUpdateState) {
			throw new System.InvalidOperationException ("You should only call this function from a thread safe callback. That does not seem to be the case for this call.");
		}
		
		if (OnThreadSafeCallback != null) {
			OnVoidDelegate tmp = OnThreadSafeCallback;
			OnThreadSafeCallback = null;
			tmp ();
		}
	}
	
#region GraphUpdateMethods
	
	/** Will apply queued graph updates as soon as possible, regardless of #limitGraphUpdates.
	 * Calling this multiple times will not create multiple callbacks.
	 * Makes sure DoUpdateGraphs is called as soon as possible.\n
	 * This function is useful if you are limiting graph updates, but you want a specific graph update to be applied as soon as possible regardless of the time limit.
	 * \see FlushGraphUpdates
	 */
	public void QueueGraphUpdates () {
		if (!isRegisteredForUpdate) {
			isRegisteredForUpdate = true;
			RegisterSafeUpdate (DoUpdateGraphs,true);
		}
	}
	
	/** Waits a moment with updating graphs.
	 * If limitGraphUpdates is set, we want to keep some space between them to let pathfinding threads running and then calculate all queued calls at once
	 */
	private IEnumerator DelayedGraphUpdate () {
		graphUpdateRoutineRunning = true;
		
		yield return new WaitForSeconds (maxGraphUpdateFreq-(Time.time-lastGraphUpdate));
		QueueGraphUpdates ();
		graphUpdateRoutineRunning = false;
	}
	
	/** Will applying this GraphUpdateObject result in no possible path between \a n1 and \a n2.
	 * Use this only with basic GraphUpdateObjects since it needs special backup logic, it probably wont work with your own specialized ones.
	 * This function is quite a lot slower than a standart Graph Update, but not so much it will slow the game down.
	 * \note This might return false for small areas even if it would block the path if #minAreaSize is greater than zero (0).
	 * So when using this, it is recommended to set #minAreaSize to 0.
	 * \see AstarPath.GetNearest
	 * \deprecated Use Pathfinding.GraphUpdateUtilities.UpdateGraphsNoBlock instead
	 */
	[System.Obsolete("Use GraphUpdateUtilities.UpdateGraphsNoBlock instead")]
	public bool WillBlockPath (GraphUpdateObject ob, Node n1, Node n2) {
		return GraphUpdateUtilities.UpdateGraphsNoBlock (ob,n1,n2);
	}
	
	/** Returns if there is a walkable path from \a n1 to \a n2.
	 * If you are making changes to the graph, areas must first be recaculated using FloodFill()
	 * \note This might return true for small areas even if there is no possible path if #minAreaSize is greater than zero (0).
	 * So when using this, it is recommended to set #minAreaSize to 0.
	 * \note Only valid as long as both nodes are walkable.
	 * \see GetNearest
	 * \deprecated Use Pathfinding.GraphUpdateUtilities.IsPathPossible instead
	 */
	[System.Obsolete("Use GraphUpdateUtilities.IsPathPossible instead")]
	public static bool IsPathPossible (Node n1, Node n2) {
		return n1.area == n2.area;
	}
	
	/** Update all graphs within \a bounds after \a t seconds.
	 * This function will add a GraphUpdateObject to the #graphUpdateQueue.
	 * The graphs will be updated as soon as possible.
	 * \see Update
	 * \see DoUpdateGraphs
	 */
	public void UpdateGraphs (Bounds bounds, float t) {
		UpdateGraphs (new GraphUpdateObject (bounds),t);
	}
	
	/** Update all graphs using the GraphUpdateObject after \a t seconds.
	 * This can be used to, e.g make all nodes in an area unwalkable, or set them to a higher penalty.
	*/
	public void UpdateGraphs (GraphUpdateObject ob, float t) {
		StartCoroutine (UpdateGraphsInteral (ob,t));
	}
	
	/** Update all graphs using the GraphUpdateObject after \a t seconds */
	private IEnumerator UpdateGraphsInteral (GraphUpdateObject ob, float t) {
		yield return new WaitForSeconds (t);
		UpdateGraphs (ob);
	}
	
	/** Update all graphs within \a bounds.
	 * This function will add a GraphUpdateObject to the #graphUpdateQueue.
	 * The graphs will be updated as soon as possible.
	 * 
	 * This is equivalent to\n
	 * UpdateGraphs (new GraphUpdateObject (bounds))
	 * 
	 * \see FlushGraphUpdates
	 */
	public void UpdateGraphs (Bounds bounds) {
		UpdateGraphs (new GraphUpdateObject (bounds));
	}
	
	/** Update all graphs using the GraphUpdateObject.
	 * This can be used to, e.g make all nodes in an area unwalkable, or set them to a higher penalty.
	 * The graphs will be updated as soon as possible (with respect to #limitGraphUpdates)
	 * 
	 * \see FlushGraphUpdates
	*/
	public void UpdateGraphs (GraphUpdateObject ob) {
		
		//Create a new queue if no queue exists yet
		if (graphUpdateQueue == null) {
			graphUpdateQueue = new Queue<GraphUpdateObject> ();
		}
		
		//Put the GUO in the queue
		graphUpdateQueue.Enqueue (ob);
		
		//If we are updating graphs we should return here. 
		//The function updating the graphs should progress to the item we just added in the queue
		if (isUpdatingGraphs) {
			return;
		}
		
		//When called during scanning, it should be calculated directly
		if (isScanning) {
			DoUpdateGraphs ();
			return;
		}
		
		//If we should limit graph updates, start a coroutine which waits until we should update graphs
		if (limitGraphUpdates && Time.time-lastGraphUpdate < maxGraphUpdateFreq) {
			if (!graphUpdateRoutineRunning) {
				StartCoroutine (DelayedGraphUpdate ());
			}
		} else {
			//Otherwise, graph updates should be carried out as soon as possible
			QueueGraphUpdates ();
		}
		
	}
	
	/** Forces graph updates to run.
	 * This will force all graph updates to run immidiately. Or rather, it will block the Unity main thread until graph updates can be performed and then issue them.
	 * This will force the pathfinding threads to finish calculate the path they are currently calculating (if any) and then pause.
	 * When all threads have paused, graph updates will be performed.
	 * \warning Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	 * But you probably wont have to worry about that.
	 * 
	 * \note This is almost identical to FlushThreadSafeCallbacks, but added for more descriptive name.
	 * This function will also override any time limit delays for graph updates.
	 * This is because graph updates are implemented using thread safe callbacks.
	 * So calling this function will also call other thread safe callbacks (if any are waiting).
	 * 
	 * Will not do anything if there are no graph updates queued (not even call other callbacks).
	 */
	public void FlushGraphUpdates () {
		if (IsAnyGraphUpdatesQueued) {
			QueueGraphUpdates ();
			FlushThreadSafeCallbacks();
		}
	}
	
	/** Updates the graphs based on the #graphUpdateQueue
	 * \see UpdateGraphs
	 */
	private void DoUpdateGraphs () {
		isRegisteredForUpdate = false;
		isUpdatingGraphs = true;
		lastGraphUpdate = Time.time;
		
		if (OnGraphsWillBeUpdated2 != null) {
			OnVoidDelegate callbacks = OnGraphsWillBeUpdated2;
			OnGraphsWillBeUpdated2 = null;
			callbacks ();
		}
		
		if (OnGraphsWillBeUpdated != null) {
			OnVoidDelegate callbacks = OnGraphsWillBeUpdated;
			OnGraphsWillBeUpdated = null;
			callbacks ();
		}
		GraphModifier.TriggerEvent (GraphModifier.EventType.PreUpdate);
		
		//If any GUOs requires a flood fill, then issue it, otherwise we can skip it to save processing power
		bool anyRequiresFloodFill = false;
		
		if (graphUpdateQueue != null) {
			while (graphUpdateQueue.Count > 0) {
				GraphUpdateObject ob = graphUpdateQueue.Dequeue ();
				
				if (ob.requiresFloodFill) anyRequiresFloodFill = true;
				
				foreach (IUpdatableGraph g in astarData.GetUpdateableGraphs ()) {
					NavGraph gr = g as NavGraph;
					if (ob.nnConstraint == null || ob.nnConstraint.SuitableGraph (active.astarData.GetGraphIndex (gr),gr)) {
						
						g.UpdateArea (ob);
					}
				}
				
			}
		}
		
		isUpdatingGraphs = false;
		
		//If any GUOs required flood filling and if we are not scanning graphs at the moment (in that case a FloodFill will be done later)
		if (anyRequiresFloodFill && !isScanning) {
			FloodFill ();
		}
		
		//this callback should not be called if scanning
		//Notify scripts that the graph has been updated
		if (OnGraphsUpdated != null && !isScanning) {
			OnGraphsUpdated (this);
		}
		GraphModifier.TriggerEvent (GraphModifier.EventType.PostUpdate);
	}
	
#endregion
	
	/** Forces thread safe callbacks to run.
	 * This will force all thread safe callbacks to run immidiately. Or rather, it will block the Unity main thread until callbacks can be called and then issue them.
	 * This will force the pathfinding threads to finish calculate the path they are currently calculating (if any) and then pause.
	 * When all threads have paused, thread safe callbacks will be called (which can be e.g graph updates).
	 * 
	 * \warning Using this very often (many times per second) can reduce your fps due to a lot of threads waiting for one another.
	 * But you probably wont have to worry about that
	 * 
	 * \note This is almost (note almost) identical to FlushGraphUpdates, but added for more appropriate name.
	 */
	public void FlushThreadSafeCallbacks () {
		
		//No callbacks? why wait?
		if (OnThreadSafeCallback == null) {
			return;
		}
		
		bool didLock = true;
		
		if (IsUsingMultithreading) {
			
			//Wait for pathfinding threads to pause
			for (int i=0;i<threadInfos.Length;i++) {
				bool locked = false;
				while (!locked) {
					//There might be another function which has got further in the process of claiming the locks,
					//if so just continue without locking since the other function has already claimed them
					//and will not release them until the Unity thread has responded (which it does now)
					if (threadSafeUpdateState) break;
					locked = Monitor.TryEnter(threadInfos[i].Lock,10);
				}
				if (threadSafeUpdateState) {
					if (i != 0 || locked) throw new System.Exception ("Wait wut! This should not happen! "+i+" "+locked);
					didLock = false;
					break;
				}
			}
			threadSafeUpdateState = true;
		} else {
			//Wait for pathfinding coroutine to finish calculating the current path
			while (!threadSafeUpdateState && threadEnumerator.MoveNext()) {}
		}
		
		//Call update to force a check for if thread safe updates should be called now
		TryCallThreadSafeCallbacks ();
		
		//We can set the pathQueueFlag now since we have updated all graphs
		doSetQueueState = true;
		pathQueueFlag.Set();
		
		if (IsUsingMultithreading && didLock) {
			//Release locks
			for (int i=0;i<threadInfos.Length;i++) Monitor.Exit(threadInfos[i].Lock);
		}
	}
	
	//[ContextMenu("Log Profiler")]
	public void LogProfiler () {
		//AstarProfiler.PrintFastResults ();
		
	}
	
	//[ContextMenu("Reset Profiler")]
	public void ResetProfiler () {
		//AstarProfiler.Reset ();
	}
	
	/** Calculates number of threads to use.
	 * If \a count is not Automatic, simply returns \a count casted to an int.
	 * \returns An int specifying how many threads to use, 0 means a coroutine should be used for pathfinding instead of a separate thread.
	 * 
	 * If \a count is set to Automatic it will return a value based on the number of processors and memory for the current system.
	 * If memory is <= 512MB or logical cores are <= 1, it will return 0. If memory is <= 1024 it will clamp threads to max 2.
	 * Otherwise it will return the number of logical cores clamped to 6.
	 */
	public static int CalculateThreadCount (ThreadCount count) {
		if (count == ThreadCount.Automatic) {
			
			int logicalCores = SystemInfo.processorCount;
			int memory = SystemInfo.systemMemorySize;
			
			if (logicalCores <= 1) return 0;
			
			if (memory <= 512) return 0;
			
			if (memory <= 1024) logicalCores = System.Math.Min (logicalCores,2);
			
			logicalCores = System.Math.Min (logicalCores,6);
			
			return logicalCores;
		} else {
			int val = (int)count;
			return val;
		}
	}
	
	/** Sets up all needed variables and scans the graphs.
	 * Calls Initialize, starts the ReturnPaths coroutine and scans all graphs.
	 * Also starts threads if using multithreading
	 * \see #OnAwakeSettings */
	public void Awake () {
		//Very important to set this. Ensures the singleton pattern holds
		active = this;
		
		if (FindObjectsOfType (typeof(AstarPath)).Length > 1) {
			Debug.LogError ("You should NOT have more than one AstarPath component in the scene at any time.\n" +
				"This can cause serious errors since the AstarPath component builds around a singleton pattern.");
		}
		
		//Disable GUILayout to gain some performance, it is not used in the OnGUI call
		useGUILayout = false;
		
		if (OnAwakeSettings != null) {
			OnAwakeSettings ();
		}
		
		int numThreads = CalculateThreadCount (threadCount);
		threads = new Thread[numThreads];
		//Thread info, will contain at least one item since the coroutine "thread" is thought of as a real thread in this case
		threadInfos = new PathThreadInfo[System.Math.Max(numThreads,1)];
		
		for (int i=0;i<threadInfos.Length;i++) {
			threadInfos[i] = new PathThreadInfo(i,this,new NodeRunData());
		}
		for (int i=0;i<threads.Length;i++) {
			threads[i] = new Thread (new ParameterizedThreadStart (CalculatePathsThreaded));
			threads[i].IsBackground = true;
		}
		
		Initialize ();
		
		StartCoroutine (ReturnsPathsHandler());
		
		if (scanOnStartup) {
			if (!astarData.cacheStartup || astarData.data_cachedStartup == null) {
				Scan ();
			}
		}
		
		UpdatePathThreadInfoNodes ();
		
		//Start pathfinding threads
		if (threads.Length > 0) {
			Thread lockThread = new Thread(new ParameterizedThreadStart(LockThread));
			lockThread.Start (this);
		}
		
		for (int i=0;i<threads.Length;i++) {
			if (logPathResults == PathLog.Heavy)
				Debug.Log ("Starting pathfinding thread "+i);
			threads[i].Start (threadInfos[i]);
		}
		
		//Or if there are no threads, it should run as a coroutine
		if (threads.Length == 0)
			StartCoroutine (CalculatePathsHandler(threadInfos[0]));
		
	}
	
	/** Called when a major data update has been done, makes sure everything is wired up correctly.
	 * For example when updating number of graphs, rescanning graphs, adding or removing nodes are considered major changes.
	 * Should not be called during or before Awake has been called on the AstarPath object.
	 */
	public void DataUpdate () {
		
		if (active != this) {
			throw new System.Exception ("Singleton pattern broken. Make sure you only have one AstarPath object in the scene");
		}
		
		/* if (threads == null) {
			throw new System.NullReferenceException ("Threads are null... Astar not set up correctly?");
		}
		
		if (threads.Length == 0 && threadEnumerator == null) {
			throw new System.NullReferenceException ("No threads and no coroutine running... Astar not set up correctly?");
		}
		
		if (threadInfos == null || threadInfos.Length == 0 || (threadInfos.Length != 1 && threadInfos.Length != threads.Length)) {
			throw new System.Exception ("Thread info count not correct... Astar not set up correctly?");
		}*/
		
		if (astarData == null) {
			throw new System.NullReferenceException ("AstarData is null... Astar not set up correctly?");
		}
		
		if (astarData.graphs == null) {
			astarData.graphs = new NavGraph[0];
		}
		
		astarData.AssignNodeIndices();
		
		if (Application.isPlaying) {
			astarData.CreateNodeRuns (threadInfos.Length);
		}
	}
	
	/** Updates NodeRun data in threads.
	 * Called by AstarData.CreateNodeRuns
	 * 
	 * If the ASTAR_SINGLE_THREAD_OPTIMIZE define is enabled this function will only do some simple error checking.
	 * 
	 * \see AstarData.CreateNodeRuns
	 */
	public void UpdatePathThreadInfoNodes () {
		for (int i=0;i<threadInfos.Length;i++) {
			PathThreadInfo info = threadInfos[i];
			if (info.threadIndex != i) throw new System.Exception ("threadInfos["+i+"] did not have a matching index member. Expected "+i+" found "+info.threadIndex);
			NodeRunData nrd = info.runData;
			if (nrd == null) throw new System.NullReferenceException ("A thread info.node run data was null");
			
		}
	}
	
	/** Makes sure #active is set to this object and that #astarData is not null.
	 * Also calls OnEnable for the #colorSettings and initializes astarData.userConnections if it wasn't initialized before */
	public void SetUpReferences () {
		active = this;
		if (astarData == null) {
			astarData = new AstarData ();
		}
		
		if (astarData.userConnections == null) {
			astarData.userConnections = new UserConnection[0];
		}
		
		if (colorSettings == null) {
			colorSettings = new AstarColor ();
		}
			
		colorSettings.OnEnable ();
	}
	
	/** Initializes various variables.
	 * \link SetUpReferences Sets up references \endlink, 
	 * Searches for graph types, calls Awake on #astarData and on all graphs
	 * 
	 * \see AstarData.FindGraphTypes 
	 * \see SetUpReferences
	 */
	private void Initialize () {
		
		AstarProfiler.InitializeFastProfile (new string [14] {
			"Prepare", 			//0
			"Initialize",		//1
			"CalculateStep",	//2
			"Trace",			//3
			"Open",				//4
			"UpdateAllG",		//5
			"Add",				//6
			"Remove",			//7
			"PreProcessing",	//8
			"Callback",			//9
			"Overhead",			//10
			"Log",				//11
			"ReturnPaths",		//12
			"PostPathCallback"	//13
		});
		
		SetUpReferences ();
		
		astarData.FindGraphTypes ();
		
		astarData.Awake ();
		
		//Initialize all graphs by calling their Awake functions
		for (int i=0;i<astarData.graphs.Length;i++) {			
			if (astarData.graphs[i] != null) astarData.graphs[i].Awake ();
		}
	}
	
	/** Clears up variables and other stuff, destroys graphs.
	 * Note that when destroying an AstarPath object, all static variables such as callbacks will be cleared.
	 */
	public void OnDestroy () {
		
		if (logPathResults == PathLog.Heavy)
			Debug.Log ("+++ AstarPath Component Destroyed - Cleaning Up Pathfinding Data +++");
		
		
		//Don't accept any more path calls to this AstarPath instance.
		//This will cause all eventual multithreading threads to exit
		TrickAbortThreads ();
		
		
		//Try to join pathfinding threads
		if (threads != null) {
			for (int i=0;i<threads.Length;i++) {
#if UNITY_WEBPLAYER
				if (!threads[i].Join(200)) {
					Debug.LogError ("Could not terminate pathfinding thread["+i+"] in 200ms." +
						"Not good.\nUnity webplayer does not support Thread.Abort\nHoping that it will be terminated by Unity WebPlayer");
				}
#else
				if (!threads[i].Join (50)) {
					Debug.LogError ("Could not terminate pathfinding thread["+i+"] in 50ms, trying Thread.Abort");
					threads[i].Abort ();
				}
#endif
			}
		}
		
		if (logPathResults == PathLog.Heavy)
			Debug.Log ("Destroying Graphs");

		
		//Clean graphs up
		if (astarData.graphs != null) {
			for (int i=0;i<astarData.graphs.Length;i++) {
				if (astarData.graphs[i] != null) astarData.graphs[i].OnDestroy ();
			}
		}
		astarData.graphs = null;
		
		if (logPathResults == PathLog.Heavy)
			Debug.Log ("Returning Paths");
		
		
		//Return all paths with errors
		/*Path p = pathReturnStack.PopAll ();
		while (p != null) {
			p.Error ();
			p.LogError ("Canceled because AstarPath object was destroyed\n");
			p.AdvanceState (PathState.Returned);
			p.ReturnPath ();
			Path tmp = p;
			p = p.next;
			tmp.next = null;
		}*/
		ReturnPaths (false);
		//Just in case someone happened to request a path in ReturnPath() (even though they should get canceled)
		pathReturnStack.PopAll ();
		
		if (logPathResults == PathLog.Heavy)
			Debug.Log ("Cleaning up variables");
		
		//Clear variables up, static variables are good to clean up, otherwise the next scene might get weird data
		floodStack = null;
		graphUpdateQueue = null;
		
		//Clear all callbacks
		OnDrawGizmosCallback	= null;
		OnAwakeSettings			= null;
		OnGraphPreScan			= null;
		OnGraphPostScan			= null;
		OnPathPreSearch			= null;
		OnPathPostSearch		= null;
		OnPreScan				= null;
		OnPostScan				= null;
		OnLatePostScan			= null;
		On65KOverflow			= null;
		OnGraphsUpdated			= null;
		OnSafeCallback			= null;
		OnThreadSafeCallback	= null;
		
		pathQueue.Clear ();
		threads = null;
		threadInfos = null;
		numActiveThreads = 0;
		ResetQueueStates ();
		
		PathsCompleted = 0;
		
		active = null;
		
	}
	
#region ScanMethods
	
	/** Floodfills starting from the specified node */
	public void FloodFill (Node seed) {
		FloodFill (seed, lastUniqueAreaIndex+1);
		lastUniqueAreaIndex++;
	}
	
	/** Floodfills starting from 'seed' using the specified area */
	public void FloodFill (Node seed, int area) {
		
		if (area > 255) {
			Debug.LogError ("Too high area index - The maximum area index is 255");
			return;
		}
		
		if (area < 0) {
			Debug.LogError ("Too low area index - The minimum area index is 0");
			return;
		}
					
		if (floodStack == null) {
			floodStack = new Stack<Node> (1024);
		}
		
		Stack<Node> stack = floodStack;
					
		stack.Clear ();
		
		stack.Push (seed);
		seed.area = area;
		
		while (stack.Count > 0) {
			stack.Pop ().FloodFill (stack,area);
		}
				
	}
	
	/** Floodfills all graphs and updates areas for every node.
	  * \see Pathfinding.Node.area */
	public void FloodFill () {
		
		
		if (astarData.graphs == null) {
			return;
		}
		
		int area = 0;
		
		lastUniqueAreaIndex = 0;
		
		if (floodStack == null) {
			floodStack = new Stack<Node> (1024);
		}
		
		Stack<Node> stack = floodStack;
		
		for (int i=0;i<graphs.Length;i++) {
			NavGraph graph = graphs[i];
			
			if (graph != null && graph.nodes != null) {
				for (int j=0;j<graph.nodes.Length;j++) {
					if (graph.nodes[j] != null)
						graph.nodes[j].area = 0;
				}
			}
		}
		
		int smallAreasDetected = 0;
		
		for (int i=0;i<graphs.Length;i++) {
			
			NavGraph graph = graphs[i];
			
			if (graph == null) continue;
			
			if (graph.nodes == null) {
				Debug.LogWarning ("Graph "+i+" has not defined any nodes");
				continue;
			}
			
			for (int j=0;j<graph.nodes.Length;j++) {
				if (graph.nodes[j] != null && graph.nodes[j].walkable && graph.nodes[j].area == 0) {
					
					area++;
					
					if (area > 255) {
						Debug.LogError ("Too many areas - The maximum number of areas is 256");
						area--;
						break;
					}
					
					stack.Clear ();
					
					stack.Push (graph.nodes[j]);
					
					int counter = 1;
					
					graph.nodes[j].area = area;
					
					while (stack.Count > 0) {
						counter++;
						stack.Pop ().FloodFill (stack,area);
					}
					
					if (counter < minAreaSize) {
						
						//Flood fill the area again with area ID 254, this identifies a small area
						stack.Clear ();
						
						stack.Push (graph.nodes[j]);
						graph.nodes[j].area = 254;
					
						while (stack.Count > 0) {
							stack.Pop ().FloodFill (stack,254);
						}
					
						smallAreasDetected++;
						area--;
					}
				}
			}
		}
		
		lastUniqueAreaIndex = area;
		
		
		if (smallAreasDetected > 0) {
			AstarLog (smallAreasDetected +" small areas were detected (fewer than "+minAreaSize+" nodes)," +
				"these might have the same IDs as other areas, but it shouldn't affect pathfinding in any significant way (you might get All Nodes Searched as a reason for path failure)." +
				"\nWhich areas are defined as 'small' is controlled by the 'Min Area Size' variable, it can be changed in the A* inspector-->Settings-->Min Area Size" +
				"\nThe small areas will use the area id 254");
		}
		
	}
	
#if UNITY_EDITOR
	[UnityEditor.MenuItem ("Edit/Pathfinding/Scan All Graphs %&s")]
	public static void MenuScan () {
		
		if (AstarPath.active == null) {
			AstarPath.active = FindObjectOfType(typeof(AstarPath)) as AstarPath;
			if (AstarPath.active == null) {
				return;
			}
		}
		
		if (!Application.isPlaying && (AstarPath.active.astarData.graphs == null || AstarPath.active.astarData.graphTypes == null)) {
			UnityEditor.EditorUtility.DisplayProgressBar ("Scanning","Deserializing",0);
			AstarPath.active.astarData.DeserializeGraphs ();
		}
		
		UnityEditor.EditorUtility.DisplayProgressBar ("Scanning","Scanning...",0);
		
		try {
			foreach (Progress progress in AstarPath.active.ScanLoop ()) {
				UnityEditor.EditorUtility.DisplayProgressBar ("Scanning",progress.description,progress.progress);
			}
		} catch (System.Exception e) {
			Debug.LogError ("There was an error generating the graphs:\n"+e.ToString ()+"\n\nIf you think this is a bug, please contact me on arongranberg.com (post a comment)\n");
			UnityEditor.EditorUtility.DisplayDialog ("Error Generating Graphs","There was an error when generating graphs, check the console for more info","Ok");
		} finally {
			UnityEditor.EditorUtility.ClearProgressBar();
		}
	}
	
	/** Called by editor scripts to rescan the graphs e.g when the user moved a graph.
	  * Will only scan graphs if not playing and time to scan last graph was less than some constant (to avoid lag with large graphs) */
	public bool AutoScan () {
		
		if (!Application.isPlaying && lastScanTime < 0.11F) {
			Scan ();
			return true;
		}
		return false;
	}
#endif
	
	/** Scans all graphs */
	public void Scan () {
		IEnumerator<Progress> scanning = ScanLoop ().GetEnumerator ();
		
		while (scanning.MoveNext ()) {
		}
		
	}
	
	/** Scans all graphs. This is a IEnumerable, you can loop through it to get the progress
	  * \code foreach (Progress progress in AstarPath.active.ScanLoop ()) {
	*	 Debug.Log ("Scanning... " + progress.description + " - " + (progress.progress*100).ToString ("0") + "%");
	  * } \endcode
	  * \see Scan
	  */
	public IEnumerable<Progress> ScanLoop () {
		
		if (graphs == null) {
			yield break;
		}
		
		isScanning = true;
		
		yield return new Progress (0.02F,"Updating graph shortcuts");
		
		astarData.UpdateShortcuts ();
		
		yield return new Progress (0.05F,"Pre processing graphs");
		
		if (OnPreScan != null) {
			OnPreScan (this);
		}
		GraphModifier.TriggerEvent (GraphModifier.EventType.PreScan);
		
		//float startTime = Time.realtimeSinceStartup;
		System.DateTime startTime = System.DateTime.UtcNow;
		
		for (int i=0;i<graphs.Length;i++) {
			
			NavGraph graph = graphs[i];
			
			if (graph == null) {
				yield return new Progress (Mathfx.MapTo (0.05F,0.7F,(float)(i+0.5F)/(graphs.Length+1)),"Skipping graph "+(i+1)+" of "+graphs.Length+" because it is null");
				continue;
			}
			
			if (OnGraphPreScan != null) {
				yield return new Progress (Mathfx.MapTo (0.05F,0.7F,(float)(i+0.5F)/(graphs.Length+1)),"Scanning graph "+(i+1)+" of "+graphs.Length+" - Pre processing");
				OnGraphPreScan (graph);
			}
			
			yield return new Progress (Mathfx.MapTo (0.05F,0.7F,(float)(i+1F)/(graphs.Length+1)),"Scanning graph "+(i+1)+" of "+graphs.Length);
			
			graph.Scan ();
			
			yield return new Progress (Mathfx.MapTo (0.05F,0.7F,(float)(i+1.1F)/(graphs.Length+1)),"Scanning graph "+(i+1)+" of "+graphs.Length+" - Assigning graph indices");
			if (graph.nodes != null) {
				for (int j=0;j<graph.nodes.Length;j++) {
					if (graph.nodes[j] != null) 
						graph.nodes[j].graphIndex = i;
				}
			}
			
			if (OnGraphPostScan != null) {
				yield return new Progress (Mathfx.MapTo (0.05F,0.7F,(float)(i+1.5F)/(graphs.Length+1)),"Scanning graph "+(i+1)+" of "+graphs.Length+" - Post processing");
				OnGraphPostScan (graph);
			}
			
		}
		
		yield return new Progress (0.8F,"Post processing graphs");
		
		if (OnPostScan != null) {
			OnPostScan (this);
		}
		GraphModifier.TriggerEvent (GraphModifier.EventType.PostScan);
		
		isScanning = false;
		
		yield return new Progress (0.85F,"Applying links");
		
		ApplyLinks ();
		
		yield return new Progress (0.90F,"Computing areas");
		
		FloodFill ();
		
		yield return new Progress (0.92F,"Updating misc. data");
		
		DataUpdate ();
		
		yield return new Progress (0.95F,"Late post processing");
		
		if (OnLatePostScan != null) {
			OnLatePostScan (this);
		}
		GraphModifier.TriggerEvent (GraphModifier.EventType.LatePostScan);
		
		
		lastScanTime = (float)(System.DateTime.UtcNow-startTime).TotalSeconds;//Time.realtimeSinceStartup-startTime;
		
		System.GC.Collect ();
		
		AstarLog ("Scanning - Process took "+(lastScanTime*1000).ToString ("0")+" ms to complete ");
		
	}
	
	/** Should be called whenever the total nodecount has changed for the graphs.
	 * Recreates temporary node data for every thread.
	 */
	public void NodeCountChanged () {
		//Create temporary path data
		//At least one should be created
		if (Application.isPlaying)
			astarData.CreateNodeRuns (System.Math.Max(threads.Length,1));
	}
	
	/** Applies links to the scanned graphs. Called right after #OnPostScan and before #FloodFill(). */
	public void ApplyLinks () {
		for (int i=0;i<astarData.userConnections.Length;i++) {
			UserConnection conn = astarData.userConnections[i];
			
			if (conn.type == ConnectionType.Connection) {
				Node n1 = GetNearest (conn.p1).node;
				Node n2 = GetNearest (conn.p2).node;
				
				if (n1 == null || n2 == null) {
					continue;
				}
				
				int cost = conn.doOverrideCost ? conn.overrideCost : (n1.position-n2.position).costMagnitude;
				
				if (conn.enable) {
					n1.AddConnection (n2, cost);
				
					if (!conn.oneWay) {
						n2.AddConnection (n1, cost);
					}
				} else {
					n1.RemoveConnection (n2);
					if (!conn.oneWay) {
						n2.RemoveConnection (n1);
					}
				}
			} else {
				Node n1 = GetNearest (conn.p1).node;
				if (n1 == null) { continue; }
				
				if (conn.doOverrideWalkability) {
					n1.walkable = conn.enable;
					if (!n1.walkable) {
						n1.UpdateNeighbourConnections ();
						n1.UpdateConnections ();
					}
				}
				
				if (conn.doOverridePenalty) {
					n1.penalty = conn.overridePenalty;
				}
				
			}
		}
		
		NodeLink[] nodeLinks = FindObjectsOfType (typeof(NodeLink)) as NodeLink[];
		
		for (int i=0;i<nodeLinks.Length;i++) {
			nodeLinks[i].Apply ();
		}
	}
	
#endregion
	
	private static int waitForPathDepth = 0;
	
	/** Wait for the specified path to be calculated.
	 * Normally it takes a few frames for a path to get calculated and returned.
	 * This function will ensure that the path will be calculated when this function returns
	 * and that the callback for that path has been called.
	 * 
	 * \note Do not confuse this with Pathfinding.Path.WaitForPath. This one will halt all operations until the path has been calculated
	 * while Pathfinding.Path.WaitForPath will wait using yield until it has been calculated.
	 * 
	 * If requesting a lot of paths in one go and waiting for the last one to complete,
	 * it will calculate most of the paths in the queue (only most if using multithreading, all if not using multithreading).
	 * 
	 * Use this function only if you really need to.
	 * There is a point to spreading path calculations out over several frames.
	 * It smoothes out the framerate and makes sure requesting a large
	 * number of paths at the same time does not cause lag.
	 * 
	 * \note Graph updates and other callbacks might get called during the execution of this function.
	 * 
	 * When the pathfinder is shutting down. I.e in OnDestroy, this function will not do anything.
	 * 
	 * \param p The path to wait for. The path must be started, otherwise an exception will be thrown.
	 * 
	 * \throws Exception if pathfinding is not initialized properly for this scene (most likely no AstarPath object exists)
	 * or if the path has not been started yet.
	 * Also throws an exception if critical errors ocurr such as when the pathfinding threads have crashed (which should not happen in normal cases).
	 * This prevents an infinite loop while waiting for the path.
	 * 
	 * \see Pathfinding.Path.WaitForPath
	 */
	public static void WaitForPath (Path p) {
		
		if (active == null)
			throw new System.Exception ("Pathfinding is not correctly initialized in this scene (yet?). " +
				"AstarPath.active is null.\nDo not call this function in Awake");
		
		if (p == null) throw new System.ArgumentNullException ("Path must not be null");
		
		if (!active.acceptNewPaths) return;
		
		if (p.GetState () == PathState.Created){
			throw new System.Exception ("The specified path has not been started yet.");
		}
		
		waitForPathDepth++;
		
		if (waitForPathDepth == 5) {
			Debug.LogError ("You are calling the WaitForPath function recursively (maybe from a path callback). Please don't do this.");
		}
		
		if (p.GetState() < PathState.ReturnQueue) {
			if (IsUsingMultithreading) {
				
				while (p.GetState() < PathState.ReturnQueue) {
					if (ActiveThreadsCount == 0) {
						waitForPathDepth--;
						throw new System.Exception ("Pathfinding Threads seems to have crashed. No threads are running.");
					}
					
					//Wait for threads to calculate paths
					Thread.Sleep (1);
					TryCallThreadSafeCallbacks();
				}
			} else {
				while (p.GetState() < PathState.ReturnQueue) {
					if (pathQueue.Count == 0 && p.GetState () != PathState.Processing) {
						waitForPathDepth--;
						throw new System.Exception ("Critical error. Path Queue is empty but the path state is '" + p.GetState() + "'");
					}
					
					//Calculate some paths
					threadEnumerator.MoveNext ();
					TryCallThreadSafeCallbacks();
				}
			}
		}
		
		active.ReturnPaths (false);
		
		waitForPathDepth--;
	}
	
	/** Will send a callback when it is safe to update nodes. This is defined as between the path searches.
	  * This callback will only be sent once and is nulled directly after the callback has been sent.
	  * When using more threads than one, calling this often might decrease pathfinding performance due to a lot of idling in the threads.
	  * Not performance as in it will use much CPU power,
	  * but performance as in the number of paths per second will probably go down (though your framerate might actually increase a tiny bit)
	  * 
	  * You should only call this function from the main unity thread (i.e normal game code).
	  * 
	  * \warning Note that if you do not set \a threadSafe to true, the callback might not be called from the Unity thread,
	  * DO NOT call any part of the Unity API from those callbacks except for Debug.Log
	  * 
	  * \code
Node node = AstarPath.active.GetNearest (transform.position).node;
AstarPath.RegisterSafeUpdate (delegate () {
	node.walkable = false;
}, false);
\endcode

\code
Node node = AstarPath.active.GetNearest (transform.position).node;
AstarPath.RegisterSafeUpdate (delegate () {
	node.position = (Int3)transform.position;
}, true);
\endcode
	  * Note that the second example uses transform in the callback, and must thus be threadSafe.
	  */
	public static void RegisterSafeUpdate (OnVoidDelegate callback, bool threadSafe) {
		if (callback == null || !Application.isPlaying) {
			return;
		}
		
		//If it already is safe to call any callbacks. call them.
		if (threadSafeUpdateState) {
			callback ();
			return;
		}
		
		if (IsUsingMultithreading) {
			int max = 0;
			//Try to aquire all locks, this will not block
			for (int i=0;i<threadInfos.Length;i++) {
				if (Monitor.TryEnter (threadInfos[i].Lock))
					max = i;
				else
					break;
			}
			
			//We could aquire all locks
			if (max == threadInfos.Length-1) {
				//Temporarily set threadSafeUpdateState to true to tell error checking code that it is safe to update graphs
				threadSafeUpdateState = true;
				callback ();
				threadSafeUpdateState = false;
			}
			
			//Release all locks we managed to aquire
			for (int i=0;i<=max;i++)
				Monitor.Exit (threadInfos[i].Lock);
			
			//If we could not aquire all locks, put it in a queue to be called as soon as possible
			if (max != threadInfos.Length-1) {
				//To speed up things, the path queue flag is reset and it is flagged that it should not be set until callbacks have been updated
				//This will trick the threads to think there is nothing to process and go to sleep (thereby allowing us to update graphs)
				doSetQueueState = false;
				pathQueueFlag.Reset();
				
				lock (safeUpdateLock) {
					
					if (threadSafe)
						OnThreadSafeCallback += callback;
					else
						OnSafeCallback += callback;
					
					//SetSafeUpdateState (true);
					safeUpdateFlag.Set();
				}
			}
		} else {
			
			if (threadSafeUpdateState) {
				callback();
			} else {
				lock (safeUpdateLock) {
					if (threadSafe)
						OnThreadSafeCallback += callback;
					else
						OnSafeCallback += callback;
				}
			}
		}
	}
	
	/** Puts the Path in queue for calculation.
	  * The callback specified when constructing the path will be called when the path has been calculated.
	  * Usually you should use the Seeker component instead of calling this function directly.
	  */
	public static void StartPath (Path p) {
		
		if (active == null) {
			Debug.LogError ("There is no AstarPath object in the scene");
			return;
		}
		
		if (p.GetState() != PathState.Created) {
			throw new System.Exception ("The path has an invalid state. Expected " + PathState.Created + " found " + p.GetState() + "\n" +
				"Make sure you are not requesting the same path twice");
		}
		
		if (!active.acceptNewPaths) {
			p.Error ();
			p.LogError ("No new paths are accepted");
			//Debug.LogError (p.errorLog);
			//p.ReturnPath ();
			return;
		}
		
		if (active.graphs == null || active.graphs.Length == 0) {
			Debug.LogError ("There are no graphs in the scene");
			p.Error ();
			p.LogError ("There are no graphs in the scene");
			Debug.LogError (p.errorLog);
			//p.ReturnPath ();
			return;
		}
		
		/*MultithreadPath p2 = p as MultithreadPath;
		if (p2 == null) {
			Debug.LogError ("Path Not Set Up For Multithreading");
			return;
		}*/
		
		p.Claim (active);
		
		lock (pathQueue) {
			//Will increment to PathQueue
			p.AdvanceState (PathState.PathQueue);
			pathQueue.Enqueue (p);
			if (doSetQueueState)
				pathQueueFlag.Set ();
		}
	}
	

	/** Terminates eventual pathfinding threads when the application quits.
	 */
	public void OnApplicationQuit () {
		if (threads == null) return;
#if !UNITY_WEBPLAYER
		//Unity webplayer does not support Abort (even though it supports starting threads). Hope that UnityPlayer aborts the threads
		for (int i=0;i<threads.Length;i++) {
			threads[i].Abort ();
		}
#endif
	}
	
#region MainThreads
	
	/** Coroutine to return thread safe path callbacks.
	 * This method will infinitely loop and call #ReturnPaths
	 * \see ReturnPaths
	 */
	public IEnumerator ReturnsPathsHandler () {
		while (true) {
			ReturnPaths(true);
			yield return 0;
		}
	}
	
	/** A temporary queue for paths which weren't returned due to large processing time.
	 * When some time limit is exceeded in ReturnPaths, paths are put on this queue until the next frame.
	 * \see ReturnPaths
	 */
	private Path pathReturnPop;
	
	/** Returns all paths in the return stack.
	  * Paths which have been processed are put in the return stack.
	  * This function will pop all items from the stack and return them to e.g the Seeker requesting them.
	  * 
	  * \param timeSlice Do not return all paths at once if it takes a long time, instead return some and wait until the next call.
	  */
	public void ReturnPaths (bool timeSlice) {
		
		//Pop all items from the stack
		Path p = pathReturnStack.PopAll ();
		
		if(pathReturnPop == null) {
			pathReturnPop = p;
		} else {
			Path tail = pathReturnPop;
			while (tail.next != null) tail = tail.next;
			tail.next = p;
		}
		
		long targetTick = timeSlice ? System.DateTime.UtcNow.Ticks + 1 * 5000 : 0;
		
		int counter = 0;
		//Loop through the linked list and return all paths
		while (pathReturnPop != null) {
			
			//Move to the next path
			Path prev = pathReturnPop;
			pathReturnPop = pathReturnPop.next;
			
			/* Remove the reference to prevent possible memory leaks
			If for example the first path computed was stored somewhere,
			it would through the linked list contain references to all comming paths to be computed,
			and thus the nodes those paths searched.
			That adds up to a lot of memory not being released */
			prev.next = null;
			
			//Return the path
			prev.ReturnPath ();
			
			//Will increment to Returned
			//However since multithreading is annoying, it might be set to ReturnQueue for a small time until the pathfinding calculation
			//thread advanced the state as well
			prev.AdvanceState (PathState.Returned);
			
			prev.ReleaseSilent (this);
			
			counter++;
			if (counter > 5 && timeSlice) {
				counter = 0;
				if (System.DateTime.UtcNow.Ticks >= targetTick) {
					return;
				}
			}
		}
	}
	
	private static void LockThread (System.Object _astar) {
		AstarPath astar = (AstarPath)_astar;
		
		while (astar.acceptNewPaths) {
			safeUpdateFlag.WaitOne ();
			
			PathThreadInfo[] infos = threadInfos;
			if (infos == null) { Debug.LogError ("Path Thread Infos are null"); return; }
			
			//Claim all locks
			for (int i=0;i<infos.Length;i++)
				Monitor.Enter (infos[i].Lock);
			
			lock (safeUpdateLock) {
				safeUpdateFlag.Reset ();
				OnVoidDelegate tmp = OnSafeCallback;
				OnSafeCallback = null;
				if (tmp != null) tmp();
				
				if (OnThreadSafeCallback != null) {
					threadSafeUpdateFlag.Reset ();
				} else {
					threadSafeUpdateFlag.Set();
				}
			}
			threadSafeUpdateState = true;
			
			//Wait until threadsafe updates have been called
			threadSafeUpdateFlag.WaitOne();
			
			//We can set the pathQueueFlag now since we have updated all graphs
			doSetQueueState = true;
			pathQueueFlag.Set();
			
			//Release all locks
			for (int i=0;i<infos.Length;i++)
				Monitor.Exit (infos[i].Lock);
			
		}
	}
	
	/** Main pathfinding function (multithreaded). This function will calculate the paths in the pathfinding queue when multithreading is enabled.
	 * \see CalculatePaths
	 * \astarpro 
	 */
	private static void CalculatePathsThreaded (System.Object _threadInfo) {
		
		//Increment the counter for how many threads are calculating
		System.Threading.Interlocked.Increment (ref numActiveThreads);
		
		PathThreadInfo threadInfo;
		
		try {
			threadInfo = (PathThreadInfo)_threadInfo;
		} catch (System.Exception e) {
			Debug.LogError ("Arguments to pathfinding threads must be of type ThreadStartInfo\n"+e);
			throw new System.ArgumentException ("Argument must be of type ThreadStartInfo",e);
		}
		
		AstarPath astar = threadInfo.astar;
		
		try {
			
			//Initialize memory for this thread
			NodeRunData runData = threadInfo.runData;
			
			
			//Max number of ticks before yielding/sleeping
			long maxTicks = (long)(astar.maxFrameTime*10000);
			long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
			
			while (true) {
				
				//The path we are currently calculating
				Path p = null;
				
				while (true) {
					//Cancel function (and thus the thread) if no more paths should be accepted.
					//This is done when the A* object is about to be destroyed
					if (!astar.acceptNewPaths) {
						System.Threading.Interlocked.Decrement (ref numActiveThreads);
						return;
					}
					
					//Wait until there are paths to process
					pathQueueFlag.WaitOne ();
					
					//Cancel function (and thus the thread) if no more paths should be accepted.
					//This is done when the A* object is about to be destroyed
					if (!astar.acceptNewPaths) {
						System.Threading.Interlocked.Decrement (ref numActiveThreads);
						return;
					}
					
					//Lock on a standard lock, the path queue
					lock (pathQueue) {
						
						//Pop the next path from the path queue
						if (pathQueue.Count > 0) {
							p = pathQueue.Dequeue ();
							break;
						} else {
							//Console.WriteLine ("Ran out of paths..."+runIndex +" ("+threadsIdle+")");
							pathQueueFlag.Reset ();
						}
					}
				}
				
				//Aquire lock for this thread
				//Another thread can try to aquire locks for all threads to be able to update stuff while making sure no pathfinding is run at the same time
				Monitor.Enter (threadInfo.Lock);
				
				//Max number of ticks we are allowed to continue working in one run
				//One tick is 1/10000 of a millisecond
				maxTicks = (long)(astar.maxFrameTime*10000);
				
				AstarProfiler.StartFastProfile (0);
				p.PrepareBase (runData);
				
				//Now processing the path
				//Will advance to Processing
				p.AdvanceState (PathState.Processing);
				
				//Call some callbacks
				if (OnPathPreSearch != null) {
					OnPathPreSearch (p);
				}
				
				//Tick for when the path started, used for calculating how long time the calculation took
				long startTicks = System.DateTime.UtcNow.Ticks;
				long totalTicks = 0;
				
				//Prepare the path
				p.Prepare ();
				
				AstarProfiler.EndFastProfile (0);
				
				if (!p.IsDone()) {
					
					//For debug uses, we set the last computed path to p, so we can view debug info on it in the editor (scene view).
					astar.debugPath = p;
					
					AstarProfiler.StartFastProfile (1);
					
					//Initialize the path, now ready to begin search
					p.Initialize ();
					
					AstarProfiler.EndFastProfile (1);
					
					//The error can turn up in the Init function
					while (!p.IsDone ()) {
						//Do some work on the path calculation.
						//The function will return when it has taken too much time
						//or when it has finished calculation
						AstarProfiler.StartFastProfile (2);
						p.CalculateStep (targetTick);
						p.searchIterations++;
						
						AstarProfiler.EndFastProfile (2);
						
						//If the path has finished calculation, we can break here directly instead of sleeping
						if (p.IsDone ()) break;
						
						//Yield/sleep so other threads can work
						totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
						Thread.Sleep (0);
						startTicks = System.DateTime.UtcNow.Ticks;
						
						targetTick = startTicks + maxTicks;
						
						//Cancel function (and thus the thread) if no more paths should be accepted.
						//This is done when the A* object is about to be destroyed
						//The path is returned and then this function will be terminated (see similar IF statement higher up in the function)
						if (!astar.acceptNewPaths) {
							p.Error ();
						}
					}
					
					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					p.duration = totalTicks*0.0001F;
					
				}
				
				AstarProfiler.StartFastProfile (9);
				
				//Log path results
				astar.LogPathResults (p);
				
				if (OnPathPostSearch != null) {
					OnPathPostSearch (p);
				}
				
				//Push the path onto the return stack
				//It will be detected by the main Unity thread and returned as fast as possible (the next late update hopefully)
				pathReturnStack.Push (p);
				
				//Will advance to ReturnQueue
				p.AdvanceState (PathState.ReturnQueue);
				
				AstarProfiler.EndFastProfile (9);
				
				//Release lock for this thread
				Monitor.Exit (threadInfo.Lock);
				
				//Wait a bit if we have calculated a lot of paths
				if (System.DateTime.UtcNow.Ticks > targetTick) {
					Thread.Sleep (1);
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
			}
		} catch (System.Exception e) {
			if (e is System.Threading.ThreadAbortException) {
				if (astar.logPathResults == PathLog.Heavy)
					Debug.LogWarning ("Shutting down pathfinding thread #"+threadInfo.threadIndex+" with Thread.Abort call");
				System.Threading.Interlocked.Decrement (ref numActiveThreads);
				return;
			}
			Debug.LogError (e);
		}
		
		Debug.LogError ("Error : This part should never be reached");
		System.Threading.Interlocked.Decrement (ref numActiveThreads);
	}
	
	/** Handler for the CalculatePaths function.
	 * Will initialize an IEnumerator from the CalculatePaths function.
	 * This function has a loop which will increment the CalculatePaths function state by one every time.
	 * Supposed to be called using StartCoroutine, this enabled other functions to also increment the state of the function when needed
	 * using the #threadEnumerator variable.
	 * \see CalculatePaths
	 * \see threadEnumerator
	 */
	private static IEnumerator CalculatePathsHandler (System.Object _threadData) {
		threadEnumerator = CalculatePaths (_threadData);
		while (threadEnumerator.MoveNext ()) {
			yield return 0;
		}
	}
	/** Main pathfinding function. This function will calculate the paths in the pathfinding queue
	 * \see CalculatePaths
	 */
	private static IEnumerator CalculatePaths (System.Object _threadInfo) {
		
		
		//Increment the counter for how many threads are calculating
		System.Threading.Interlocked.Increment (ref numActiveThreads);
		
		PathThreadInfo threadInfo;
		try {
			threadInfo = (PathThreadInfo)_threadInfo;
		} catch (System.Exception e) {
			Debug.LogError ("Arguments to pathfinding threads must be of type ThreadStartInfo\n"+e);
			throw new System.ArgumentException ("Argument must be of type ThreadStartInfo",e);
		}
		
		int numPaths = 0;
		
		//Initialize memory for this thread
		NodeRunData runData = threadInfo.runData;
		
		
		//Max number of ticks before yielding/sleeping
		long maxTicks = (long)(active.maxFrameTime*10000);
		long targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
		
		threadSafeUpdateState = true;
		
		while (true) {
			
			//The path we are currently calculating
			Path p = null;
			
			AstarProfiler.StartProfile ("Path Queue");
			
			//Try to get the next path to be calculated
			while (true) {
				//Cancel function (and thus the thread) if no more paths should be accepted.
				//This is done when the A* object is about to be destroyed
				if (!active.acceptNewPaths) {
					System.Threading.Interlocked.Decrement (ref numActiveThreads);
					yield break;
				}
				
				if (pathQueue.Count > 0) {
					p = pathQueue.Dequeue ();
				}
				
				//System.Threading.Interlocked.Increment(ref threadsIdle);
				
				//Last thread alive
				//Call callbacks if any are requested
				OnVoidDelegate tmp = OnSafeCallback;
				OnSafeCallback = null;
				if (tmp != null) tmp();
				
				TryCallThreadSafeCallbacks ();
				//The threadSafeUpdateState is still enabled since this is coroutine mode
				//It would be reset in TryCallThreadSafeCallbacks
				threadSafeUpdateState = true;
				
				if (p == null) {
					AstarProfiler.EndProfile ();
					yield return 0;
					AstarProfiler.StartProfile ("Path Queue");
				}
				
				//If we have a path, start calculating it
				if (p != null) break;
			}
			
			AstarProfiler.EndProfile ();
			
			AstarProfiler.StartProfile ("Path Calc");
			
			//Max number of ticks we are allowed to continue working in one run
			//One tick is 1/10000 of a millisecond
			maxTicks = (long)(active.maxFrameTime*10000);
			
			threadSafeUpdateState = false;
			
			p.PrepareBase (runData);
			
			//Now processing the path
			//Will advance to Processing
			p.AdvanceState (PathState.Processing);
			
			//Call some callbacks
			if (OnPathPreSearch != null) {
				OnPathPreSearch (p);
			}
			
			numPaths++;
			
			//Tick for when the path started, used for calculating how long time the calculation took
			long startTicks = System.DateTime.UtcNow.Ticks;
			long totalTicks = 0;
			
			AstarProfiler.StartFastProfile(8);
			
			AstarProfiler.StartFastProfile(0);
			//Prepare the path
			AstarProfiler.StartProfile ("Path Prepare");
			p.Prepare ();
			AstarProfiler.EndProfile ();
			AstarProfiler.EndFastProfile (0);
			
			if (!p.IsDone()) {
				
				//For debug uses, we set the last computed path to p, so we can view debug info on it in the editor (scene view).
				active.debugPath = p;
				
				//Initialize the path, now ready to begin search
				AstarProfiler.StartProfile ("Path Initialize");
				p.Initialize ();
				AstarProfiler.EndProfile ();
				
				//The error can turn up in the Init function
				while (!p.IsDone ()) {
					//Do some work on the path calculation.
					//The function will return when it has taken too much time
					//or when it has finished calculation
					AstarProfiler.StartFastProfile(2);
					
					AstarProfiler.StartProfile ("Path Calc Step");
					p.CalculateStep (targetTick);
					AstarProfiler.EndFastProfile(2);
					p.searchIterations++;
					
					AstarProfiler.EndProfile ();
					
					//If the path has finished calculation, we can break here directly instead of sleeping
					if (p.IsDone ()) break;
					
					AstarProfiler.EndFastProfile(8);
					totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
					//Yield/sleep so other threads can work
						
					AstarProfiler.EndProfile ();
					yield return 0;
					AstarProfiler.StartProfile ("Path Calc");
					
					startTicks = System.DateTime.UtcNow.Ticks;
					AstarProfiler.StartFastProfile(8);
					
					//Cancel function (and thus the thread) if no more paths should be accepted.
					//This is done when the A* object is about to be destroyed
					//The path is returned and then this function will be terminated (see similar IF statement higher up in the function)
					if (!active.acceptNewPaths) {
						p.Error ();
					}
					
					targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				}
				
				totalTicks += System.DateTime.UtcNow.Ticks-startTicks;
				p.duration = totalTicks*0.0001F;
				
			}
			
			//Log path results
			AstarProfiler.StartProfile ("Log Path Results");
			active.LogPathResults (p);
			AstarProfiler.EndProfile ();
			
			AstarProfiler.EndFastProfile(8);
			
			AstarProfiler.StartFastProfile(13);
			if (OnPathPostSearch != null) {
				OnPathPostSearch (p);
			}
			AstarProfiler.EndFastProfile(13);
			
			//Push the path onto the return stack
			//It will be detected by the main Unity thread and returned as fast as possible (the next late update)
			pathReturnStack.Push (p);
			
			p.AdvanceState (PathState.ReturnQueue);
			
			AstarProfiler.EndProfile ();
			
			threadSafeUpdateState = true;
			
			//Wait a bit if we have calculated a lot of paths
			if (System.DateTime.UtcNow.Ticks > targetTick) {
				yield return 0;
				targetTick = System.DateTime.UtcNow.Ticks + maxTicks;
				numPaths = 0;
			}
		}
		
		//Debug.LogError ("Error : This part should never be reached");
		//System.Threading.Interlocked.Decrement (ref numActiveThreads);
	}
#endregion
	
	
	/** Returns the nearest node to a position using the specified NNConstraint.
	 Searches through all graphs for their nearest nodes to the specified position and picks the closest one.\n
	 Using the NNConstraint.None constraint.
	 \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (Vector3 position) {
		return GetNearest(position,NNConstraint.None);
	}
	
	/** Returns the nearest node to a position using the specified NNConstraint.
	 Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	 The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	 \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint) {
		return GetNearest(position,constraint,null);
	}
	
	/** Returns the nearest node to a position using the specified NNConstraint.
	 Searches through all graphs for their nearest nodes to the specified position and picks the closest one.
	 The NNConstraint can be used to specify constraints on which nodes can be chosen such as only picking walkable nodes.
	 \see Pathfinding.NNConstraint
	 */
	public NNInfo GetNearest (Vector3 position, NNConstraint constraint, Node hint) {
		
		if (graphs == null) { return new NNInfo(); }
		
		if (constraint == null) {
			constraint = NNConstraint.None;
		}
		
		float minDist = float.PositiveInfinity;//Math.Infinity;
		NNInfo nearestNode = new NNInfo ();
		int nearestGraph = -1;
		
		for (int i=0;i<graphs.Length;i++) {
			
			NavGraph graph = graphs[i];
			
			if (graph == null) continue;
			
			//Check if this graph should be searched
			if (!constraint.SuitableGraph (i,graph)) {
				continue;
			}
			
			NNInfo nnInfo;
			if (fullGetNearestSearch) {
				nnInfo = graph.GetNearestForce (position, constraint);
			} else {
				nnInfo = graph.GetNearest (position, constraint);
			}
			
			Node node = nnInfo.node;
			
			if (node == null) {
				continue;
			}
			
			float dist = ((Vector3)nnInfo.clampedPosition-position).magnitude;
			
			if (prioritizeGraphs && dist < prioritizeGraphsLimit) {
				//The node is close enough, choose this graph and discard all others
				minDist = dist;
				nearestNode = nnInfo;
				nearestGraph = i;
				break;
			} else {
				if (dist < minDist) {
					minDist = dist;
					nearestNode = nnInfo;
					nearestGraph = i;
				}
			}
		}
		
		//No matches found
		if (nearestGraph == -1) {
			return nearestNode;
		}
		
		//Check if a constrained node has already been set
		if (nearestNode.constrainedNode != null) {
			nearestNode.node = nearestNode.constrainedNode;
			nearestNode.clampedPosition = nearestNode.constClampedPosition;
		}
		
		if (!fullGetNearestSearch && nearestNode.node != null && !constraint.Suitable (nearestNode.node)) {
			
			//Otherwise, perform a check to force the graphs to check for a suitable node
			NNInfo nnInfo = graphs[nearestGraph].GetNearestForce (position, constraint);
			
			if (nnInfo.node != null) {
				nearestNode = nnInfo;
			}
		}
		
		if (!constraint.Suitable (nearestNode.node) || (constraint.constrainDistance && (nearestNode.clampedPosition - position).sqrMagnitude > maxNearestNodeDistanceSqr)) {
			return new NNInfo();
		}
		
		return nearestNode;
	}
	
	/** Returns the node closest to the ray (slow).
	  * \warning This function is brute-force and very slow, it can barely be used once per frame */
	public Node GetNearest (Ray ray) {
		
		if (graphs == null) { return null; }
		
		float minDist = Mathf.Infinity;
		Node nearestNode = null;
		
		Vector3 lineDirection = ray.direction;
		Vector3 lineOrigin = ray.origin;
		
		for (int i=0;i<graphs.Length;i++) {
			
			NavGraph graph = graphs[i];
		
			Node[] nodes = graph.nodes;
			
			if (nodes == null) {
				continue;
			}
			
			for (int j=0;j<nodes.Length;j++) {
				
				Node node = nodes[j];
				
				if (node == null) continue;
				
	        	Vector3 pos = (Vector3)node.position;
				Vector3 p = lineOrigin+(Vector3.Dot(pos-lineOrigin,lineDirection)*lineDirection);
				
				float tmp = Mathf.Abs (p.x-pos.x);
				tmp *= tmp;
				if (tmp > minDist) continue;
				
				tmp = Mathf.Abs (p.z-pos.z);
				tmp *= tmp;
				if (tmp > minDist) continue;
				
				float dist = (p-pos).sqrMagnitude;
				
				if (dist < minDist) {
					minDist = dist;
					nearestNode = node;
				}
			}
			
		}
		
		return nearestNode;
	}
	
}
