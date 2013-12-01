using UnityEngine;
using System.Collections;
using Pathfinding;

[AddComponentMenu("Pathfinding/Modifiers/Alternative Path")]
[System.Serializable]
/** Applies penalty to the paths it processes telling other units to avoid choosing the same path.
 * 
 * Note that this might not work properly if penalties are modified by other actions as well (e.g graph updates which reset the penalty to zero).
 * 
 * \ingroup modifiers
 */
public class AlternativePath : MonoModifier {
	
#if UNITY_EDITOR
	[UnityEditor.MenuItem ("CONTEXT/Seeker/Add Alternative Path Modifier")]
	public static void AddComp (UnityEditor.MenuCommand command) {
		(command.context as Component).gameObject.AddComponent (typeof(AlternativePath));
	}
#endif
	
	public override ModifierData input {
		get { return ModifierData.Original; }
	}
	
	public override ModifierData output {
		get { return ModifierData.All; }
	}
	
	/** How much penalty (weight) to apply to nodes */
	public int penalty = 1000;
	
	/** Max number of nodes to skip in a row */
	public int randomStep = 10;
	
	/** The previous path */
	Node[] prevNodes;
	
	int prevSeed; /**< Previous seed. Used to figure out which nodes to revert penalty on without storing them in an array */
	int prevPenalty = 0; /**< The previous penalty used. Stored just in case it changes during operation */
	
	bool waitingForApply = false;
	
	System.Object lockObject = new System.Object ();
	
	/** A random object */
	private System.Random rnd = new System.Random ();
	
	/** A random object generating random seeds for other random objects */
	private System.Random seedGenerator = new System.Random ();
	
	/** The nodes waiting to have their penalty changed */
	Node[] toBeApplied;
	public override void Apply (Path p, ModifierData source) {
		
		lock (lockObject) {
			toBeApplied = p.path.ToArray();
			//AstarPath.active.RegisterCanUpdateGraphs (ApplyNow);
			if (!waitingForApply) {
				waitingForApply = true;
				AstarPath.OnPathPreSearch += ApplyNow;
			}
		}
	}
		
	void ApplyNow (Path somePath) {
		lock (lockObject) {
			waitingForApply = false;
			AstarPath.OnPathPreSearch -= ApplyNow;
			//toBeApplied.Add (p.nodes);
			int seed = prevSeed;
			rnd = new System.Random (seed);
			
			//Add previous penalty
			if (prevNodes != null) {
				bool warnPenalties = false;
				int rndStart = rnd.Next (randomStep);
				for (int i=rndStart;i<prevNodes.Length;i+= rnd.Next (1,randomStep)) {
					if (prevNodes[i].penalty < prevPenalty) {
						warnPenalties = true;
					}
					prevNodes[i].penalty = (uint)(Mathf.Max(prevNodes[i].penalty-prevPenalty));
				}
				if (warnPenalties) {
					Debug.LogWarning ("Penalty for some nodes has been reset while this modifier was active. Penalties might not be correctly set.");
				}
			}
			
			//Calculate a new seed
			seed = seedGenerator.Next ();
			rnd = new System.Random (seed);
			//seed = Random.Range (0,10000);
			//Random.seed = seed;
			
			if (toBeApplied != null) {
				//int rnd = //Random.Range (0,randomStep);
				int rndStart = rnd.Next (randomStep);
				for (int i=rndStart;i<toBeApplied.Length;i+= rnd.Next (1,randomStep)) {
					toBeApplied[i].penalty = (uint)(toBeApplied[i].penalty+penalty);
				}
			}
			
			prevPenalty = penalty;
			prevSeed = seed;
			prevNodes = toBeApplied;
		}
	}
}
