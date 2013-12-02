//#define NoPooling //Disable pooling for some reason. Could be debugging or just for measuring the difference.

using System;
using System.Collections.Generic;

namespace Pathfinding.Util
{
	/** Lightweight List Pool.
	 * Handy class for pooling lists of type T.
	 * 
	 * Usage:
	 * - Claim a new list using \code List<SomeClass> foo = ListPool<SomeClass>.Claim (); \endcode
	 * - Use it and do stuff with it
	 * - Release it with \code ListPool<SomeClass>.Release (foo); \endcode
	 * 
	 * You do not need to clear the list before releasing it.
	 * After you have released a list, you should never use it again.
	 * 
	 * \since Version 3.2
	 * \see Pathfinding.Util.StackPool
	 */
	public static class ListPool<T>
	{
		/** Internal pool */
		static List<List<T>> pool;
		
		/** Static constructor */
		static ListPool ()
		{
			pool = new List<List<T>> ();
		}
		
		/** Claim a list.
		 * Returns a pooled list if any are in the pool.
		 * Otherwise it creates a new one.
		 * After usage, this list should be released using the Release function (though not strictly necessary).
		 */
		public static List<T> Claim () {
			if (pool.Count > 0) {
				List<T> ls = pool[pool.Count-1];
				pool.RemoveAt(pool.Count-1);
				return ls;
			} else {
				return new List<T>();
			}
		}
		
		/** Claim a list with minimum capacity
		 * Returns a pooled list if any are in the pool.
		 * Otherwise it creates a new one.
		 * After usage, this list should be released using the Release function (though not strictly necessary).
		 * This list returned will have at least the capacity specified.
		 */
		public static List<T> Claim (int capacity) {
			if (pool.Count > 0) {
				//List<T> list = pool.Pop();
				List<T> list = pool[pool.Count-1];
				pool.RemoveAt(pool.Count-1);
				
				if (list.Capacity < capacity) list.Capacity = capacity;
				
				return list;
			} else {
				return new List<T>(capacity);
			}
		}
		
		/** Makes sure the pool contains at least \a count pooled items with capacity \a size.
		 * This is good if you want to do all allocations at start.
		 */
		public static void Warmup (int count, int size) {
			List<T>[] tmp = new List<T>[count];
			for (int i=0;i<count;i++) tmp[i] = Claim (size);
			for (int i=0;i<count;i++) Release (tmp[i]);
		}
		
		/** Releases a list.
		 * After the list has been released it should not be used anymore.
		 * 
		 * \throws System.InvalidOperationException
		 * Releasing a list when it has already been released will cause an exception to be thrown.
		 * 
		 * \see Claim
		 */
		public static void Release (List<T> list) {
			
			for (int i=0;i<pool.Count;i++)
				if (pool[i] == list)
					throw new System.InvalidOperationException ("The List is released even though it is in the pool");
			
			list.Clear ();
			pool.Add (list);
		}
		
		/** Clears the pool for lists of this type.
		 * This is an O(n) operation, where n is the number of pooled lists.
		 */
		public static void Clear () {
			pool.Clear ();
		}
		
		/** Number of lists of this type in the pool */
		public static int GetSize () {
			return pool.Count;
		}
	}
}

