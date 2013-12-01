using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
#endif

[System.Serializable]
public class tk2dSpriteCollectionIndex
{
	public string name;
	public string spriteCollectionGUID;
	public string spriteCollectionDataGUID;
	public string[] spriteNames = new string[0];
	public string[] spriteTextureGUIDs = new string[0];
	public string[] spriteTextureTimeStamps = new string[0];
	public bool managedSpriteCollection = false;
	public bool loadable = false;
	public string assetName = "";
	public int version;
}

[System.Serializable]
public class tk2dGenericIndexItem
{
	public tk2dGenericIndexItem(string guid) { this.assetGUID = guid; }
	public string assetGUID;
	public string dataGUID;
	public bool managed = false;
	public bool loadable = false;
	public string AssetName
	{
		get
		{
			string assetName = "unknown";
#if UNITY_EDITOR
			assetName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(assetGUID));
#endif
			return assetName;
		}
	}

	public T GetAsset<T>() where T : UnityEngine.Object
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetGUID), typeof(T)) as T;
#else
		return null;
#endif
	}
	
	public T GetData<T>() where T : UnityEngine.Object
	{
#if UNITY_EDITOR
		return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(dataGUID), typeof(T)) as T;
#else
		return null;
#endif
	}
}


public class tk2dIndex : ScriptableObject
{
	public int version = 0;
	public static int CURRENT_VERSION = 4;

	[SerializeField] List<tk2dGenericIndexItem> spriteAnimationIndex = new List<tk2dGenericIndexItem>();
	[SerializeField] List<tk2dGenericIndexItem> fontIndex = new List<tk2dGenericIndexItem>();
	[SerializeField] List<tk2dSpriteCollectionIndex> spriteCollectionIndex = new List<tk2dSpriteCollectionIndex>();
	
	public tk2dSpriteCollectionIndex[] GetSpriteCollectionIndex()
	{
#if UNITY_EDITOR
		int i = 0;
		string assetsPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
		foreach (var v in spriteCollectionIndex)
		{
			if (v != null)
			{
				string thisAssetPath = AssetDatabase.GUIDToAssetPath(v.spriteCollectionDataGUID);
				string p = assetsPath + thisAssetPath;
				if (thisAssetPath != null && !System.IO.File.Exists(p))
				{
					spriteCollectionIndex[i] = null;
				}
			}
			++i;
		}
#endif
		spriteCollectionIndex.RemoveAll(item => item == null);
		return spriteCollectionIndex.ToArray();
	}
	
	public void AddSpriteCollectionData(tk2dSpriteCollectionData sc)
	{
#if UNITY_EDITOR
		// prune list
		GetSpriteCollectionIndex(); 
		spriteCollectionIndex.RemoveAll(item => item == null);
		string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(sc));
		
		bool existing = false;
		tk2dSpriteCollectionIndex indexEntry = null;
		foreach (var v in spriteCollectionIndex) 
		{
			if (v.spriteCollectionDataGUID == guid)
			{
				indexEntry = v;
				existing = true;
				break;
			}
		}
		if (indexEntry == null)
			indexEntry = new tk2dSpriteCollectionIndex();
			
		indexEntry.name = sc.spriteCollectionName;
		indexEntry.spriteCollectionDataGUID = guid;
		indexEntry.spriteCollectionGUID = sc.spriteCollectionGUID;
		indexEntry.spriteNames = new string[sc.spriteDefinitions.Length];
		indexEntry.spriteTextureGUIDs = new string[sc.spriteDefinitions.Length];
		indexEntry.spriteTextureTimeStamps = new string[sc.spriteDefinitions.Length];
		indexEntry.version = sc.version;
		indexEntry.managedSpriteCollection = sc.managedSpriteCollection;
		indexEntry.loadable = sc.loadable;
		indexEntry.assetName = sc.assetName;
		for (int i = 0; i < sc.spriteDefinitions.Length; ++i)
		{
			var s = sc.spriteDefinitions[i];
			if (s != null)
			{
				indexEntry.spriteNames[i] = sc.spriteDefinitions[i].name;
				indexEntry.spriteTextureGUIDs[i] = sc.spriteDefinitions[i].sourceTextureGUID;
				string assetPath = AssetDatabase.GUIDToAssetPath(indexEntry.spriteTextureGUIDs[i]);
				if (assetPath.Length > 0 && System.IO.File.Exists(assetPath))
					indexEntry.spriteTextureTimeStamps[i] = (System.IO.File.GetLastWriteTime(assetPath) - new System.DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds.ToString();
				else
					indexEntry.spriteTextureTimeStamps[i] = "0";
			}
			else
			{
				indexEntry.spriteNames[i] = "";
				indexEntry.spriteTextureGUIDs[i] = "";
				indexEntry.spriteTextureTimeStamps[i] = "";
			}
		}

		if (sc.spriteCollectionPlatforms.Length > 0) {
			indexEntry.spriteNames = new string[] { "dummy" };
			indexEntry.spriteTextureGUIDs = new string[] { "" };
			indexEntry.spriteTextureTimeStamps = new string[] { "0" };
		}

		if (!existing)
			spriteCollectionIndex.Add(indexEntry);
#endif
	}

	void PruneGenericList(ref List<tk2dGenericIndexItem> list)
	{
#if UNITY_EDITOR
		for (int i = 0; i < list.Count; ++i)
		{
			if (list[i] != null && AssetDatabase.GUIDToAssetPath(list[i].assetGUID).Length == 0)
				list[i] = null;

		}
		list.RemoveAll(item => item == null);
#endif
	}

	public tk2dGenericIndexItem[] GetSpriteAnimations()
	{
		PruneGenericList(ref spriteAnimationIndex);
		return spriteAnimationIndex.ToArray();
	}
	
	public void AddSpriteAnimation(tk2dSpriteAnimation anim)
	{
#if UNITY_EDITOR
		string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(anim));
		
		PruneGenericList(ref spriteAnimationIndex);
		foreach (tk2dGenericIndexItem v in spriteAnimationIndex) 
			if (v.assetGUID == guid) return;
		spriteAnimationIndex.Add(new tk2dGenericIndexItem(guid));
#endif
	}

	public tk2dGenericIndexItem[] GetFonts()
	{
		PruneGenericList(ref fontIndex);
		return fontIndex.ToArray();
	}
	
	public void AddOrUpdateFont(tk2dFont font)
	{
#if UNITY_EDITOR
		string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(font));
		
		PruneGenericList(ref fontIndex);

		tk2dGenericIndexItem item = null;
		foreach (tk2dGenericIndexItem v in fontIndex) 
			if (v.assetGUID == guid) { item = v; break; }

		if (item == null) // not found
		{
			item = new tk2dGenericIndexItem(guid);
			fontIndex.Add(item);
		}
		
		item.loadable = font.loadable;
		item.managed = (font.data == null) ? false : font.data.managedFont;
		item.dataGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(font.data));
#endif		
	}
}

