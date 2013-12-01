using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class tk2dScaleUtility
{
	static void BakeRecursive(Transform node, Vector3 accumulatedScale)
	{
		accumulatedScale = new Vector3(accumulatedScale.x * node.localScale.x,
									   accumulatedScale.y * node.localScale.y,
									   accumulatedScale.z * node.localScale.z);
		
		tk2dBaseSprite sprite = node.GetComponent<tk2dBaseSprite>();
		tk2dTextMesh textMesh = node.GetComponent<tk2dTextMesh>();
		if (sprite)
		{
			Vector3 spriteAccumScale = new Vector3(accumulatedScale.x * sprite.scale.x,
										   		   accumulatedScale.y * sprite.scale.y,
										   		   accumulatedScale.z * sprite.scale.z);
			node.localScale = Vector3.one;
			sprite.scale = spriteAccumScale;
		}
		if (textMesh)
		{
			Vector3 spriteAccumScale = new Vector3(accumulatedScale.x * textMesh.scale.x,
										   		   accumulatedScale.y * textMesh.scale.y,
										   		   accumulatedScale.z * textMesh.scale.z);
			node.localScale = Vector3.one;
			textMesh.scale = spriteAccumScale;
			textMesh.Commit();
		}
		
		for (int i = 0; i < node.childCount; ++i)
		{
			BakeRecursive(node.GetChild(i), accumulatedScale);
		}
	}
	
	public static void Bake(Transform rootObject)
	{
		BakeRecursive(rootObject, Vector3.one);
	}
}
