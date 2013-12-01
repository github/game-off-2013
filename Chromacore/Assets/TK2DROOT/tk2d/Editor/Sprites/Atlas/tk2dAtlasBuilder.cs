// Atlasing code is adapted from Jukka Jylänki's public domain code.

using System;
using System.Collections.Generic;

namespace tk2dEditor.Atlas
{
	public class Entry
	{
		public int index;
		public int x, y;
		public int w, h;
		public bool flipped;
	}

	public class Data
	{
		public int width, height;
		public float occupancy;
		public Entry[] entries;
		
		public Entry FindEntryWithIndex(int index)
		{
			return System.Array.Find(entries, (e) => index == e.index);
		}
	}
	
	public class Builder
	{
		int maxAllowedAtlasCount = 0;
		int atlasWidth = 0;
		int atlasHeight = 0;
		bool forceSquare = false;
		bool allowOptimizeSize = true;
		int alignShift = 0;
		
		List<RectSize> sourceRects = new List<RectSize>();

		List<Data> atlases = new List<Data>();
		List<int> remainingRectIndices = new List<int>();
		
		bool oversizeTextures = false;

		public Builder(int atlasWidth, int atlasHeight, int maxAllowedAtlasCount, bool allowOptimizeSize, bool forceSquare)
		{
			this.atlasWidth = atlasWidth;
			this.atlasHeight = atlasHeight;
			this.maxAllowedAtlasCount = maxAllowedAtlasCount;
			this.forceSquare = forceSquare;
			this.allowOptimizeSize = allowOptimizeSize;
		}
		
		// Adds rect into sequence, indexed incrementally
		public void AddRect(int width, int height)
		{
			RectSize rs = new RectSize();
			rs.width = width;
			rs.height = height;
			sourceRects.Add(rs);
		}

		MaxRectsBinPack FindBestBinPacker(int width, int height, ref List<RectSize> currRects, ref bool allUsed)
		{
			List<MaxRectsBinPack> binPackers = new List<MaxRectsBinPack>();
			List<List<RectSize>> binPackerRects = new List<List<RectSize>>();
			List<bool> binPackerAllUsed = new List<bool>();

			//MaxRectsBinPack.FreeRectChoiceHeuristic[] heuristics = { MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit,
			//                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit,
			//                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit,
			//                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule,
			//                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectContactPointRule };

			MaxRectsBinPack.FreeRectChoiceHeuristic[] heuristics = { MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit,
			                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestLongSideFit,
			                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestShortSideFit,
			                                                         MaxRectsBinPack.FreeRectChoiceHeuristic.RectBottomLeftRule,
			                                                          };

			foreach (var heuristic in heuristics)
			{
				MaxRectsBinPack binPacker = new MaxRectsBinPack(width, height);
				List<RectSize> activeRects = new List<RectSize>(currRects);
				bool activeAllUsed = binPacker.Insert(activeRects, heuristic);

				binPackers.Add(binPacker);
				binPackerRects.Add(activeRects);
				binPackerAllUsed.Add(activeAllUsed);
			}

			int leastWastedPixels = Int32.MaxValue;
			int leastWastedIndex = -1;
			for (int i = 0; i < binPackers.Count; ++i)
			{
				int wastedPixels = binPackers[i].WastedBinArea();
				if (wastedPixels < leastWastedPixels)
				{
					leastWastedPixels = wastedPixels;
					leastWastedIndex = i;
					oversizeTextures = true;
				}
			}

			currRects = binPackerRects[leastWastedIndex];
			allUsed = binPackerAllUsed[leastWastedIndex];
			return binPackers[leastWastedIndex];
		}
		
		// Returns number of remaining rects
		public int Build()
		{
			// Initialize
			atlases = new List<Data>();
			remainingRectIndices = new List<int>();
			bool[] usedRect = new bool[sourceRects.Count];

			int atlasWidth = this.atlasWidth >> alignShift;
			int atlasHeight = this.atlasHeight >> alignShift;
			
			// Sanity check, can't build with textures larger than the actual max atlas size
			int align = (1 << alignShift) - 1;
			int minSize = Math.Min(atlasWidth, atlasHeight);
			int maxSize = Math.Max(atlasWidth, atlasHeight);
			foreach (RectSize rs in sourceRects)
			{
				int maxDim = (Math.Max(rs.width, rs.height) + align) >> alignShift;
				int minDim = (Math.Min(rs.width, rs.height) + align) >> alignShift;
				
				// largest texture needs to fit in an atlas
				if (maxDim > maxSize || (maxDim <= maxSize && minDim > minSize))
				{
					remainingRectIndices = new List<int>();
					for (int i = 0; i < sourceRects.Count; ++i)
						remainingRectIndices.Add(i);
					return remainingRectIndices.Count;
				}
			}
			
			// Start with all source rects, this list will get reduced over time
			List<RectSize> rects = new List<RectSize>();
			foreach (RectSize rs in sourceRects)
			{
				RectSize t = new RectSize();
				t.width = (rs.width + align) >> alignShift;
				t.height = (rs.height + align) >> alignShift;
				rects.Add(t);
			}

			bool allUsed = false;
			while (allUsed == false && atlases.Count < maxAllowedAtlasCount)
			{
				int numPasses = 1;
				int thisCellW = atlasWidth, thisCellH = atlasHeight;
				bool reverted = false;

				while (numPasses > 0)
				{
					// Create copy to make sure we can scale textures down when necessary
					List<RectSize> currRects = new List<RectSize>(rects);

//					MaxRectsBinPack binPacker = new MaxRectsBinPack(thisCellW, thisCellH);
//					allUsed = binPacker.Insert(currRects, MaxRectsBinPack.FreeRectChoiceHeuristic.RectBestAreaFit);
					MaxRectsBinPack binPacker = FindBestBinPacker(thisCellW, thisCellH, ref currRects, ref allUsed);
					float occupancy = binPacker.Occupancy();

					// Consider the atlas resolved when after the first pass, all textures are used, and the occupancy > 0.5f, scaling
					// down by half to maintain PO2 requirements means this is as good as it gets
					bool firstPassFull = numPasses == 1 && occupancy > 0.5f;

					// Reverted copes with the case when halving the atlas size when occupancy < 0.5f, the textures don't fit in the
					// atlas anymore. At this point, size is reverted to the previous value, and the loop should accept this as the final value
					if ( firstPassFull ||
						(numPasses > 1 && occupancy > 0.5f && allUsed) ||
						reverted || !allowOptimizeSize)
					{
						List<Entry> atlasEntries = new List<Entry>();
						
						foreach (var t in binPacker.GetMapped())
						{
							int matchedWidth = 0;
							int matchedHeight = 0;

							int matchedId = -1;
							bool flipped = false;
							for (int i = 0; i < sourceRects.Count; ++i)
							{
								int width = (sourceRects[i].width + align) >> alignShift;
								int height = (sourceRects[i].height + align) >> alignShift;
								if (!usedRect[i] && width == t.width && height == t.height)
								{
									matchedId = i;
									matchedWidth = sourceRects[i].width;
									matchedHeight = sourceRects[i].height;
									break;
								}
							}

							// Not matched anything yet, so look for the same rects rotated
							if (matchedId == -1)
							{
								for (int i = 0; i < sourceRects.Count; ++i)
								{
									int width = (sourceRects[i].width + align) >> alignShift;
									int height = (sourceRects[i].height + align) >> alignShift;
									if (!usedRect[i] && width == t.height && height == t.width)
									{
										matchedId = i;
										flipped = true;
										matchedWidth = sourceRects[i].height;
										matchedHeight = sourceRects[i].width;
										break;
									}
								}
							}
							
							// If this fails its a catastrophic error
							usedRect[matchedId] = true;
							Entry newEntry = new Entry();
							newEntry.flipped = flipped;
							newEntry.x = t.x << alignShift;
							newEntry.y = t.y << alignShift;
							newEntry.w = matchedWidth;
							newEntry.h = matchedHeight;
							newEntry.index = matchedId;
							atlasEntries.Add(newEntry);
						}

						Data currAtlas = new Data();
						currAtlas.width = thisCellW << alignShift;
						currAtlas.height = thisCellH << alignShift;
						currAtlas.occupancy = binPacker.Occupancy();
						currAtlas.entries = atlasEntries.ToArray();
						
						atlases.Add(currAtlas);

						rects = currRects;
						break; // done
					}
					else
					{
						if (!allUsed) 
						{
							if (forceSquare)
							{
								thisCellW *= 2;
								thisCellH *= 2;
							}
							else
							{
								// Can only try another size when it already has been scaled down for the first time
								if (thisCellW < atlasWidth || thisCellH < atlasHeight)
								{
									// Tried to scale down, but the texture doesn't fit, so revert previous change, and 
									// iterate over the data again forcing a pass even though there is wastage
									if (thisCellW < thisCellH) thisCellW *= 2;
									else thisCellH *= 2;
								}
							}

							reverted = true;
						}
						else
						{
							if (forceSquare)
							{
								thisCellH /= 2;
								thisCellW /= 2;
							}
							else
							{
								// More than half the texture was unused, scale down by one of the dimensions
								if (thisCellW < thisCellH) thisCellH /= 2;
								else thisCellW /= 2;
							}
						}

						numPasses++;
					}
				}
			}
		
			remainingRectIndices = new List<int>();
			for (int i = 0; i < usedRect.Length; ++i)
			{
				if (!usedRect[i])
				{
					remainingRectIndices.Add(i);
				}
			}
				
			return remainingRectIndices.Count;
		}

		public Data[] GetAtlasData()
		{
			return atlases.ToArray();
		}

		public int[] GetRemainingRectIndices()
		{
			return remainingRectIndices.ToArray();
		}
		
		public bool HasOversizeTextures()
		{
			return oversizeTextures;
		}
	}
}

