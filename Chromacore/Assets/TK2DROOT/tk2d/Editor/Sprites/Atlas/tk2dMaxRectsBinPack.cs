// Atlasing code is adapted from Jukka Jylänki's public domain code.
// MaxRectsBinPack.cs is a direct translation to C#

using System;
using System.Collections.Generic;

namespace tk2dEditor.Atlas
{

	/** MaxRectsBinPack implements the MAXRECTS data structure and different bin packing algorithms that 
		use this structure. */
	class MaxRectsBinPack
	{
		/// Instantiates a bin of size (0,0). Call Init to create a new bin.
		public MaxRectsBinPack()
		{
		}

		/// Instantiates a bin of the given size.
		public MaxRectsBinPack(int width, int height)
		{
			Init(width, height);
		}

		/// (Re)initializes the packer to an empty bin of width x height units. Call whenever
		/// you need to restart with a new bin.
		public void Init(int width, int height)
		{
			binWidth = width;
			binHeight = height;

			Rect n = new Rect();
			n.x = 0;
			n.y = 0;
			n.width = width;
			n.height = height;

			usedRectangles.Clear();

			freeRectangles.Clear();
			freeRectangles.Add(n);
		}

		/// Specifies the different heuristic rules that can be used when deciding where to place a new rectangle.
		public enum FreeRectChoiceHeuristic
		{
			RectBestShortSideFit, /// -BSSF: Positions the rectangle against the short side of a free rectangle into which it fits the best.
			RectBestLongSideFit, /// -BLSF: Positions the rectangle against the long side of a free rectangle into which it fits the best.
			RectBestAreaFit, /// -BAF: Positions the rectangle into the smallest free rect into which it fits.
			RectBottomLeftRule, /// -BL: Does the Tetris placement.
			RectContactPointRule /// -CP: Choosest the placement where the rectangle touches other rects as much as possible.
		};

		/// Inserts the given list of rectangles in an offline/batch mode, possibly rotated.
		/// @param rects The list of rectangles to insert. This vector will be destroyed in the process.
		/// @param dst [out] This list will contain the packed rectangles. The indices will not correspond to that of rects.
		/// @param method The rectangle placement rule to use when packing.
		public bool Insert(List<RectSize> rects, FreeRectChoiceHeuristic method)
		{
			int numRects = rects.Count;
			while (rects.Count > 0)
			{
				int bestScore1 = Int32.MaxValue;
				int bestScore2 = Int32.MaxValue;
				int bestRectIndex = -1;
				Rect bestNode = null;

				for (int i = 0; i < rects.Count; ++i)
				{
					int score1 = 0;
					int score2 = 0;
					Rect newNode = ScoreRect(rects[i].width, rects[i].height, method, ref score1, ref score2);

					if (score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2))
					{
						bestScore1 = score1;
						bestScore2 = score2;
						bestNode = newNode;
						bestRectIndex = i;
					}
				}

				if (bestRectIndex == -1)
					return usedRectangles.Count == numRects;

				PlaceRect(bestNode);
				rects.RemoveAt(bestRectIndex);
			}

			return usedRectangles.Count == numRects;
		}

		public List<Rect> GetMapped()
		{
			return usedRectangles;
		}


		/// Inserts a single rectangle into the bin, possibly rotated.
		public Rect Insert(int width, int height, FreeRectChoiceHeuristic method)
		{
			Rect newNode = new Rect();
			int score1 = 0; // Unused in this function. We don't need to know the score after finding the position.
			int score2 = 0;
			switch (method)
			{
				case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
				case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
				case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1); break;
				case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
				case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
			}

			if (newNode.height == 0)
				return newNode;

			int numRectanglesToProcess = freeRectangles.Count;
			for (int i = 0; i < numRectanglesToProcess; ++i)
			{
				if (SplitFreeNode(freeRectangles[i], newNode))
				{
					freeRectangles.RemoveAt(i);
					--i;
					--numRectanglesToProcess;
				}
			}

			PruneFreeList();

			usedRectangles.Add(newNode);
			return newNode;
		}

		/// Computes the ratio of used surface area to the total bin area.
		public float Occupancy()
		{
			long usedSurfaceArea = 0;
			for (int i = 0; i < usedRectangles.Count; ++i)
				usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;

			return (float)usedSurfaceArea / (float)(binWidth * binHeight);
		}

		public int WastedBinArea()
		{
			long usedSurfaceArea = 0;
			for (int i = 0; i < usedRectangles.Count; ++i)
				usedSurfaceArea += usedRectangles[i].width * usedRectangles[i].height;

			return (int)((long)(binWidth * binHeight) - usedSurfaceArea);
		}


		int binWidth = 0;
		int binHeight = 0;

		List<Rect> usedRectangles = new List<Rect>();
		List<Rect> freeRectangles = new List<Rect>();

		/// Computes the placement score for placing the given rectangle with the given method.
		/// @param score1 [out] The primary placement score will be outputted here.
		/// @param score2 [out] The secondary placement score will be outputted here. This isu sed to break ties.
		/// @return This struct identifies where the rectangle would be placed if it were placed.
		Rect ScoreRect(int width, int height, FreeRectChoiceHeuristic method, ref int score1, ref int score2)
		{
			Rect newNode = null;
			score1 = Int32.MaxValue;
			score2 = Int32.MaxValue;
			switch (method)
			{
				case FreeRectChoiceHeuristic.RectBestShortSideFit: newNode = FindPositionForNewNodeBestShortSideFit(width, height, ref score1, ref score2); break;
				case FreeRectChoiceHeuristic.RectBottomLeftRule: newNode = FindPositionForNewNodeBottomLeft(width, height, ref score1, ref score2); break;
				case FreeRectChoiceHeuristic.RectContactPointRule: newNode = FindPositionForNewNodeContactPoint(width, height, ref score1);
					score1 = -score1; // Reverse since we are minimizing, but for contact point score bigger is better.
					break;
				case FreeRectChoiceHeuristic.RectBestLongSideFit: newNode = FindPositionForNewNodeBestLongSideFit(width, height, ref score2, ref score1); break;
				case FreeRectChoiceHeuristic.RectBestAreaFit: newNode = FindPositionForNewNodeBestAreaFit(width, height, ref score1, ref score2); break;
			}

			// Cannot fit the current rectangle.
			if (newNode.height == 0)
			{
				score1 = Int32.MaxValue;
				score2 = Int32.MaxValue;
			}

			return newNode;
		}

		/// Places the given rectangle into the bin.
		void PlaceRect(Rect node)
		{
			int numRectanglesToProcess = freeRectangles.Count;
			for (int i = 0; i < numRectanglesToProcess; ++i)
			{
				if (SplitFreeNode(freeRectangles[i], node))
				{
					freeRectangles.RemoveAt(i);
					--i;
					--numRectanglesToProcess;
				}
			}

			PruneFreeList();

			usedRectangles.Add(node);
			//		dst.push_back(bestNode); ///\todo Refactor so that this compiles.
		}

		/// Computes the placement score for the -CP variant.
		int ContactPointScoreNode(int x, int y, int width, int height)
		{
			int score = 0;

			if (x == 0 || x + width == binWidth)
				score += height;
			if (y == 0 || y + height == binHeight)
				score += width;

			for (int i = 0; i < usedRectangles.Count; ++i)
			{
				if (usedRectangles[i].x == x + width || usedRectangles[i].x + usedRectangles[i].width == x)
					score += CommonIntervalLength(usedRectangles[i].y, usedRectangles[i].y + usedRectangles[i].height, y, y + height);
				if (usedRectangles[i].y == y + height || usedRectangles[i].y + usedRectangles[i].height == y)
					score += CommonIntervalLength(usedRectangles[i].x, usedRectangles[i].x + usedRectangles[i].width, x, x + width);
			}
			return score;
		}

		Rect FindPositionForNewNodeBottomLeft(int width, int height, ref int bestY, ref int bestX)
		{
			Rect bestNode = new Rect();
			// memset(&bestNode, 0, sizeof(Rect)); // done in constructor

			bestY = Int32.MaxValue;

			for (int i = 0; i < freeRectangles.Count; ++i)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
				{
					int topSideY = freeRectangles[i].y + height;
					if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestY = topSideY;
						bestX = freeRectangles[i].x;
					}
				}
				if (freeRectangles[i].width >= height && freeRectangles[i].height >= width)
				{
					int topSideY = freeRectangles[i].y + width;
					if (topSideY < bestY || (topSideY == bestY && freeRectangles[i].x < bestX))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestY = topSideY;
						bestX = freeRectangles[i].x;
					}
				}
			}
			return bestNode;
		}

		Rect FindPositionForNewNodeBestShortSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
		{
			Rect bestNode = new Rect();
			//memset(&bestNode, 0, sizeof(Rect));

			bestShortSideFit = Int32.MaxValue;

			for (int i = 0; i < freeRectangles.Count; ++i)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
				{
					int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
					int leftoverVert = Math.Abs(freeRectangles[i].height - height);
					int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

					if (shortSideFit < bestShortSideFit || (shortSideFit == bestShortSideFit && longSideFit < bestLongSideFit))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}

				if (freeRectangles[i].width >= height && freeRectangles[i].height >= width)
				{
					int flippedLeftoverHoriz = Math.Abs(freeRectangles[i].width - height);
					int flippedLeftoverVert = Math.Abs(freeRectangles[i].height - width);
					int flippedShortSideFit = Math.Min(flippedLeftoverHoriz, flippedLeftoverVert);
					int flippedLongSideFit = Math.Max(flippedLeftoverHoriz, flippedLeftoverVert);

					if (flippedShortSideFit < bestShortSideFit || (flippedShortSideFit == bestShortSideFit && flippedLongSideFit < bestLongSideFit))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestShortSideFit = flippedShortSideFit;
						bestLongSideFit = flippedLongSideFit;
					}
				}
			}
			return bestNode;
		}

		Rect FindPositionForNewNodeBestLongSideFit(int width, int height, ref int bestShortSideFit, ref int bestLongSideFit)
		{
			Rect bestNode = new Rect();
			bestLongSideFit = Int32.MaxValue;

			for (int i = 0; i < freeRectangles.Count; ++i)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
				{
					int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
					int leftoverVert = Math.Abs(freeRectangles[i].height - height);
					int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

					if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}

				if (freeRectangles[i].width >= height && freeRectangles[i].height >= width)
				{
					int leftoverHoriz = Math.Abs(freeRectangles[i].width - height);
					int leftoverVert = Math.Abs(freeRectangles[i].height - width);
					int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);
					int longSideFit = Math.Max(leftoverHoriz, leftoverVert);

					if (longSideFit < bestLongSideFit || (longSideFit == bestLongSideFit && shortSideFit < bestShortSideFit))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestShortSideFit = shortSideFit;
						bestLongSideFit = longSideFit;
					}
				}
			}
			return bestNode;
		}

		Rect FindPositionForNewNodeBestAreaFit(int width, int height, ref int bestAreaFit, ref int bestShortSideFit)
		{
			Rect bestNode = new Rect();

			bestAreaFit = Int32.MaxValue;

			for (int i = 0; i < freeRectangles.Count; ++i)
			{
				int areaFit = freeRectangles[i].width * freeRectangles[i].height - width * height;

				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
				{
					int leftoverHoriz = Math.Abs(freeRectangles[i].width - width);
					int leftoverVert = Math.Abs(freeRectangles[i].height - height);
					int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

					if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestShortSideFit = shortSideFit;
						bestAreaFit = areaFit;
					}
				}

				if (freeRectangles[i].width >= height && freeRectangles[i].height >= width)
				{
					int leftoverHoriz = Math.Abs(freeRectangles[i].width - height);
					int leftoverVert = Math.Abs(freeRectangles[i].height - width);
					int shortSideFit = Math.Min(leftoverHoriz, leftoverVert);

					if (areaFit < bestAreaFit || (areaFit == bestAreaFit && shortSideFit < bestShortSideFit))
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestShortSideFit = shortSideFit;
						bestAreaFit = areaFit;
					}
				}
			}
			return bestNode;
		}

		Rect FindPositionForNewNodeContactPoint(int width, int height, ref int bestContactScore)
		{
			Rect bestNode = new Rect();
			bestContactScore = -1;

			for (int i = 0; i < freeRectangles.Count; ++i)
			{
				// Try to place the rectangle in upright (non-flipped) orientation.
				if (freeRectangles[i].width >= width && freeRectangles[i].height >= height)
				{
					int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, width, height);
					if (score > bestContactScore)
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = width;
						bestNode.height = height;
						bestContactScore = score;
					}
				}
				if (freeRectangles[i].width >= height && freeRectangles[i].height >= width)
				{
					int score = ContactPointScoreNode(freeRectangles[i].x, freeRectangles[i].y, width, height);
					if (score > bestContactScore)
					{
						bestNode.x = freeRectangles[i].x;
						bestNode.y = freeRectangles[i].y;
						bestNode.width = height;
						bestNode.height = width;
						bestContactScore = score;
					}
				}
			}
			return bestNode;
		}

		/// @return True if the free node was split.
		bool SplitFreeNode(Rect freeNode, Rect usedNode)
		{
			// Test with SAT if the rectangles even intersect.
			if (usedNode.x >= freeNode.x + freeNode.width || usedNode.x + usedNode.width <= freeNode.x ||
				usedNode.y >= freeNode.y + freeNode.height || usedNode.y + usedNode.height <= freeNode.y)
				return false;

			if (usedNode.x < freeNode.x + freeNode.width && usedNode.x + usedNode.width > freeNode.x)
			{
				// New node at the top side of the used node.
				if (usedNode.y > freeNode.y && usedNode.y < freeNode.y + freeNode.height)
				{
					Rect newNode = freeNode.Copy();
					newNode.height = usedNode.y - newNode.y;
					freeRectangles.Add(newNode);
				}

				// New node at the bottom side of the used node.
				if (usedNode.y + usedNode.height < freeNode.y + freeNode.height)
				{
					Rect newNode = freeNode.Copy();
					newNode.y = usedNode.y + usedNode.height;
					newNode.height = freeNode.y + freeNode.height - (usedNode.y + usedNode.height);
					freeRectangles.Add(newNode);
				}
			}

			if (usedNode.y < freeNode.y + freeNode.height && usedNode.y + usedNode.height > freeNode.y)
			{
				// New node at the left side of the used node.
				if (usedNode.x > freeNode.x && usedNode.x < freeNode.x + freeNode.width)
				{
					Rect newNode = freeNode.Copy();
					newNode.width = usedNode.x - newNode.x;
					freeRectangles.Add(newNode);
				}

				// New node at the right side of the used node.
				if (usedNode.x + usedNode.width < freeNode.x + freeNode.width)
				{
					Rect newNode = freeNode.Copy();
					newNode.x = usedNode.x + usedNode.width;
					newNode.width = freeNode.x + freeNode.width - (usedNode.x + usedNode.width);
					freeRectangles.Add(newNode);
				}
			}

			return true;
		}

		/// Goes through the free rectangle list and removes any redundant entries.
		void PruneFreeList()
		{
			/* 
			///  Would be nice to do something like this, to avoid a Theta(n^2) loop through each pair.
			///  But unfortunately it doesn't quite cut it, since we also want to detect containment. 
			///  Perhaps there's another way to do this faster than Theta(n^2).

			if (freeRectangles.size() > 0)
				clb::sort::QuickSort(&freeRectangles[0], freeRectangles.size(), NodeSortCmp);

			for(size_t i = 0; i < freeRectangles.size()-1; ++i)
				if (freeRectangles[i].x == freeRectangles[i+1].x &&
					freeRectangles[i].y == freeRectangles[i+1].y &&
					freeRectangles[i].width == freeRectangles[i+1].width &&
					freeRectangles[i].height == freeRectangles[i+1].height)
				{
					freeRectangles.erase(freeRectangles.begin() + i);
					--i;
				}
			*/

			/// Go through each pair and remove any rectangle that is redundant.
			for (int i = 0; i < freeRectangles.Count; ++i)
			{
				for (int j = i + 1; j < freeRectangles.Count; ++j)
				{
					if (Rect.IsContainedIn(freeRectangles[i], freeRectangles[j]))
					{
						freeRectangles.RemoveAt(i);
						--i;
						break;
					}
					if (Rect.IsContainedIn(freeRectangles[j], freeRectangles[i]))
					{
						freeRectangles.RemoveAt(j);
						--j;
					}
				}
			}
		}

		/// Returns 0 if the two intervals i1 and i2 are disjoint, or the length of their overlap otherwise.
		int CommonIntervalLength(int i1start, int i1end, int i2start, int i2end)
		{
			if (i1end < i2start || i2end < i1start)
				return 0;
			return Math.Min(i1end, i2end) - Math.Max(i1start, i2start);
		}
	}

}
