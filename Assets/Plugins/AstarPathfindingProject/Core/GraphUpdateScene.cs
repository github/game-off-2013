using UnityEngine;
using System.Collections;
using Pathfinding;

[AddComponentMenu("Pathfinding/GraphUpdateScene")]
/** Helper class for easily updating graphs.
 * 
 * \see \ref graph-updates
 * for an explanation of how to use this class.
 */
public class GraphUpdateScene : GraphModifier {
	
	/** Do some stuff at start */
	public void Start () {
		
		//If firstApplied is true, that means the graph was scanned during Awake.
		//So we shouldn't apply it again because then we would end up applying it two times
		if (!firstApplied && applyOnStart) {
			Apply ();
		}
	}
	
	public override void OnPostScan ()
	{
		if (applyOnScan) Apply ();
	}
	
	/** Points which define the region to update */
	public Vector3[] points;
	
	/** Private cached convex hull of the #points */
	private Vector3[] convexPoints;
	
	[HideInInspector]
	/** Use the convex hull (XZ space) of the points. */
	public bool convex = true;
	[HideInInspector]
	/** Minumum height of the bounds of the resulting Graph Update Object.
	 * Useful when all points are laid out on a plane but you still need a bounds with a height greater than zero since a
	 * zero height graph update object would usually result in no nodes being updated.
	 */
	public float minBoundsHeight = 1;
	[HideInInspector]
	/** Penalty to add to nodes.
	 * Be careful when setting negative values since if a node get's a negative penalty it will underflow and instead get
	 * really large. In most cases a warning will be logged if that happens.
	 */
	public int penaltyDelta = 0;
	[HideInInspector]
	/** Set to true to set all targeted nodese walkability to #setWalkability */
	public bool modifyWalkability = false;
	[HideInInspector]
	/** See #modifyWalkability */
	public bool setWalkability = false;
	[HideInInspector]
	/** Apply this graph update object on start */
	public bool applyOnStart = true;
	[HideInInspector]
	/** Apply this graph update object whenever a graph is rescanned */
	public bool applyOnScan = true;
	[HideInInspector]
	/** Use world space for coordinates.
	 * If true, the shape will not follow when moving around the transform
	 */
	public bool useWorldSpace = true;
	
	/** Update node's walkability and connectivity using physics functions.
	 * For grid graphs, this will update the node's position and walkability exactly like when doing a scan of the graph.
	 * If enabled for grid graphs, #modifyWalkability will be ignored.
	 * 
	 * For Point Graphs, this will recalculate all connections which passes through the bounds of the resulting Graph Update Object
	 * using raycasts (if enabled).
	 * 
	 */
	public bool updatePhysics = false;
	
	/** \copydoc Pathfinding::GraphUpdateObject::resetPenaltyOnPhysics */
	public bool resetPenaltyOnPhysics = true;
	
	/** \copydoc Pathfinding::GraphUpdateObject::updateErosion */
	public bool updateErosion = true;
	
	[HideInInspector]
	/** Lock all points to Y = #lockToYValue */
	public bool lockToY = false;
	[HideInInspector]
	/** if #lockToY is enabled lock all points to this value */
	public float lockToYValue = 0;
	
	[HideInInspector]
	/** If enabled, set all nodes' tags to #setTag */
	public bool modifyTag = false;
	[HideInInspector]
	/** If #modifyTag is enabled, set all nodes' tags to this value */
	public int setTag = 0;
	
	/** Private cached inversion of #setTag.
	 * Used for InvertSettings() */
	private int setTagInvert = 0;
	
	/** Has apply been called yet.
	 * Used to prevent applying twice when both applyOnScan and applyOnStart are enabled */
	private bool firstApplied = false;
	
	/** Inverts all invertable settings for this GUS.
	 * Namely: penalty delta, walkability, tags.
	 * 
	 * Penalty delta will be changed to negative penalty delta.\n
	 * #setWalkability will be inverted.\n
	 * #setTag will be stored in a private variable, and the new value will be 0. When calling this function again, the saved
	 * value will be the new value.
	 * 
	 * Calling this function an even number of times without changing any settings in between will be identical to no change in settings.
	 */
	public virtual void InvertSettings () {
		setWalkability = !setWalkability;
		penaltyDelta = -penaltyDelta;
		if (setTagInvert == 0) {
			setTagInvert = setTag;
			setTag = 0;
		} else {
			setTag = setTagInvert;
			setTagInvert = 0;
		}
	}
	
	/** Recalculate convex points.
	  * Will not do anything if not #convex is enabled
	  */
	public void RecalcConvex () {
		if (convex) convexPoints = Polygon.ConvexHull (points); else convexPoints = null;
	}
	
	/** Switches between using world space and not using local space.
	 * Changes point coordinates to stay the same in world space after the change.
	 * 
	 * \see #useWorldSpace
	 */
	public void ToggleUseWorldSpace () {
		useWorldSpace = !useWorldSpace;
		
		if (points == null) return;
		
		Matrix4x4 matrix = useWorldSpace ? transform.localToWorldMatrix : transform.worldToLocalMatrix;
		
		for (int i=0;i<points.Length;i++) {
			points[i] = matrix.MultiplyPoint3x4 (points[i]);
		}
	}
	
	/** Lock all points to a specific Y value.
	 * 
	 * \see lockToYValue
	 */
	public void LockToY () {
		if (points == null) return;
		
		for (int i=0;i<points.Length;i++)
			points[i].y = lockToYValue;
	}
	
	/** Apply the update.
	 * Will only do anything if #applyOnScan is enabled */
	public void Apply (AstarPath active) {
		if (applyOnScan) {
			Apply ();
		}
	}
	
	/** Updates graphs with a created GUO.
	 * Creates a Pathfinding.GraphUpdateObject with a Pathfinding.GraphUpdateShape
	 * representing the polygon of this object and update all graphs using AstarPath.UpdateGraphs.
	 * This will not update graphs directly. See AstarPath.UpdateGraph for more info.
	 */
	public void Apply () {
		
		if (AstarPath.active == null) {
			Debug.LogError ("There is no AstarPath object in the scene");
			return;
		}
		
		firstApplied = true;
		
		Pathfinding.GraphUpdateShape shape = new Pathfinding.GraphUpdateShape ();
		shape.convex = convex;
		Vector3[] worldPoints = points;
		if (!useWorldSpace) {
			worldPoints = new Vector3[points.Length];
			Matrix4x4 matrix = transform.localToWorldMatrix;
			for (int i=0;i<worldPoints.Length;i++) worldPoints[i] = matrix.MultiplyPoint3x4 (points[i]);
		}
		
		shape.points = worldPoints;
		
		Bounds b = shape.GetBounds ();
		if (b.size.y < minBoundsHeight) b.size = new Vector3(b.size.x,minBoundsHeight,b.size.z);
		
		GraphUpdateObject guo = new GraphUpdateObject (b);
		guo.shape = shape;
		guo.modifyWalkability = modifyWalkability;
		guo.setWalkability = setWalkability;
		guo.addPenalty = penaltyDelta;
		guo.updatePhysics = updatePhysics;
		guo.updateErosion = updateErosion;
		guo.resetPenaltyOnPhysics = resetPenaltyOnPhysics;
		
		guo.modifyTag = modifyTag;
		guo.setTag = setTag;
		
		AstarPath.active.UpdateGraphs (guo);
	}
	
	/** Draws some gizmos */
	public void OnDrawGizmos () {
		OnDrawGizmos (false);
	}
	
	/** Draws some gizmos */
	public void OnDrawGizmosSelected () {
		OnDrawGizmos (true);
	}
	
	/** Draws some gizmos */
	public void OnDrawGizmos (bool selected) {
		
		if (points == null) return;
		
		Gizmos.color = selected ? new Color (0,0.9F,0,1F) : new Color (0,0.9F,0,0.5F);
		Matrix4x4 matrix = useWorldSpace ? Matrix4x4.identity : transform.localToWorldMatrix;
		
		for (int i=0;i<points.Length;i++) {
			Gizmos.DrawLine (matrix.MultiplyPoint3x4(points[i]),matrix.MultiplyPoint3x4(points[(i+1)%points.Length]));
			//Gizmos.DrawRay (points[i],Vector3.down);
		}
		
		
		if (convex) {
			if (convexPoints == null) RecalcConvex ();
			
			Gizmos.color = selected ? new Color (0.9F,0,0,1F) : new Color (0.9F,0,0,0.5F);
			for (int i=0;i<convexPoints.Length;i++) {
				Gizmos.DrawLine (matrix.MultiplyPoint3x4(convexPoints[i]),matrix.MultiplyPoint3x4(convexPoints[(i+1)%convexPoints.Length]));
			}
		}
	}
}