// Atlasing code is adapted from Jukka Jylänki's public domain code.
// Rect.cs is a direct translation to C#

using System.Collections.Generic;

namespace tk2dEditor.Atlas
{
	class RectSize
	{
		public int width = 0;
		public int height = 0;
	};

	class Rect
	{
		public int x = 0;
		public int y = 0;
		public int width = 0;
		public int height = 0;

		/// Performs a lexicographic compare on (rect short side, rect long side).
		/// @return -1 if the smaller side of a is shorter than the smaller side of b, 1 if the other way around.
		///   If they are equal, the larger side length is used as a tie-breaker.
		///   If the rectangles are of same size, returns 0.
		// public static int CompareRectShortSide(Rect a, Rect b);

		/// Performs a lexicographic compare on (x, y, width, height).
		// public static int NodeSortCmp(Rect a, Rect b);

		/// Returns true if a is contained in b.
		public static bool IsContainedIn(Rect a, Rect b)
		{
			return (a.x >= b.x) && (a.y >= b.y)
				&& (a.x + a.width <= b.x + b.width)
				&& (a.y + a.height <= b.y + b.height);
		}

		public Rect Copy()
		{
			Rect r = new Rect();
			r.x = x;
			r.y = y;
			r.width = width;
			r.height = height;
			return r;
		}
	};

	class DisjointRectCollection
	{
		public List<Rect> rects = new List<Rect>();

		public bool Add(Rect r)
		{
			// Degenerate rectangles are ignored.
			if (r.width == 0 || r.height == 0)
				return true;

			if (!Disjoint(r))
				return false;

			rects.Add(r);

			return true;
		}

		public void Clear()
		{
			rects.Clear();
		}

		bool Disjoint(Rect r)
		{
			// Degenerate rectangles are ignored.
			if (r.width == 0 || r.height == 0)
				return true;

			for (int i = 0; i < rects.Count; ++i)
				if (!IsDisjoint(rects[i], r))
					return false;
			return true;
		}

		static bool IsDisjoint(Rect a, Rect b)
		{
			if ((a.x + a.width <= b.x) ||
				(b.x + b.width <= a.x) ||
				(a.y + a.height <= b.y) ||
				(b.y + b.height <= a.y))
				return true;
			return false;
		}
	};

}