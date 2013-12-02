======== A* Pathfinding Project ========================

========================================================
======== Upgrading from older versions =================
========================================================

Read the respective sections on how to upgrade from older versions

See also http://arongranberg.com/astar/docs/upgrading.php

========================================================
======== Upgrading from 3.1.x to 3.2.x =================
========================================================
	
	Syntax has changed a bit for the 3.2.x version to make path pooling as streamlined as possible
	You might not need to change anything, but if you are creating new paths manually, e.g using:
		
		Path p = new Path (startPoint, endPoint, callback);
	
	You will now need to change that. Path is a now an abstract class, and the basic path which calculates a path
	from a point A to a point B is now called ABPath. Constructors are not used either
	instead static Construct methods are used. These will use pooled paths if possible
	
		ABPath p = ABPath.Construct (startPoint, endPoint, callback);
	
	All path types implement Construct methods in a similar fashion.
	
	-------------------------------------
	
	The vectorPath and path arrays on path objects are now List<Vector3> and List<Node> respectively.
	This is done so that they can be reused if the path object is recycled.
	
	-------------------------------------
	
	If you have been getting the closest node to a point with
	
		Node node = AstarPath.active.GetNearest(somePosition);
	
	You will now have to either explicitly cast from NNInfo (which the GetNearest function actually returns)
	or get the node by the .node field
	
		Node node = AstarPath.active.GetNearest(somePosition).node;
	
	See changelog for more changes.
	
========================================================
======== Upgrading from 3.0.x versions =================
========================================================

	If you are upgrading from versions earlier than 3.1 your settings for
	each graph might be lost during the upgrade process.
	Please back up your project before upgrading
	(well, if you are reading this notice, I can just hope that
	you have seen the messages about this on other places)
	
	Note that A* Pathfinding Project 3.1 dropped support for Unity 3.3
	
	If you are having problems upgrading. If you see compiler errors for example
	Try to delete the AstarPathfindingProject folder in Unity and import
	the package again. This can help removing old scripts which are not included
	in the project anymore but since UnityPackages merges directories, they are
	still there.

	If you have problems with some compiler messages saying that some members
	or functions do not exist in a class. It is likely that your
	project contains a class with that name in the global namespace.
	This causes a conflict between the classes.
	To solve it, the simplest solution is to put the conflicting class in a
	namespace or just rename it.
	
	If you have written code interfacing with the system, you might need to update it:
	
	The serialization API has changed a bit for 3.1 see docs for new api.
	Int3 is no longer implicitly convertible to Vector3 so for example if you do something like
		Debug.Log (someNode.position)
	You will now have to write
		Debug.Log ((Vector3)someNode.position
	
	These were the most important changes, for more changes, see the change log.
		http://arongranberg.com/astar/docs/changelog.php

========================================================
======== Upgrading from very old versions (2.x) ========
========================================================

	see http://arongranberg.com/astar/docs/upgrading.php