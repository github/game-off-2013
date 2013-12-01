using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class tk2dSpriteCollectionTextureWatcher : AssetPostprocessor
{
	void OnPreprocessTexture()
	{
		if (tk2dPreferences.inst.autoRebuild)
		{
			// Make sure sprite textures always have the correct format set up
			if (tk2dSpriteCollectionBuilder.IsSpriteSourceTexture(assetPath))
			{
				tk2dSpriteCollectionBuilder.ConfigureSpriteTextureImporter(assetPath);
			}
		}
	}
	
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		if (tk2dPreferences.inst.autoRebuild && importedAssets != null && importedAssets.Length	!= 0)
		{
			tk2dSpriteCollectionBuilder.RebuildOutOfDate(importedAssets);
		}
	}
}



