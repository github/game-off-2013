using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class tk2dSystemUtility
{
	static string GetObjectGUID(Object obj) { return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)); }

	// Make this asset loadable at runtime, using the guid and the given name
	// The guid MUST match the GUID of the object.
	// The name is arbitrary and should be unique to make all assets findable using name, 
	// but doesn't have to be. Name can be an empty string, but not null.
	public static bool MakeLoadableAsset(Object obj, string name)
	{
		string guid = GetObjectGUID(obj);

		// Check if it already is Loadable
		bool isLoadable = IsLoadableAsset(obj);
		if (isLoadable)
		{
			// Update name if it is different
			foreach (tk2dResourceTocEntry t in tk2dSystem.inst.Editor__Toc)
			{
				if (t.assetGUID == guid)
				{
					t.assetName = name;
					break;
				}
			}

			EditorUtility.SetDirty(tk2dSystem.inst);

			// Already loadable
			return true;
		}

		// Create resource object
		string resourcePath = GetOrCreateResourcesDir() + "/" + "tk2d_" + guid + ".asset";
		tk2dResource resource = ScriptableObject.CreateInstance<tk2dResource>();
		resource.objectReference = obj;
		AssetDatabase.CreateAsset(resource, resourcePath);
		AssetDatabase.SaveAssets();

		// Add index entry
		tk2dResourceTocEntry tocEntry = new tk2dResourceTocEntry();
		tocEntry.resourceGUID = AssetDatabase.AssetPathToGUID(resourcePath);
		tocEntry.assetName = (name.Length == 0)?obj.name:name;
		tocEntry.assetGUID = guid;
		List<tk2dResourceTocEntry> toc = new List<tk2dResourceTocEntry>(tk2dSystem.inst.Editor__Toc);
		toc.Add(tocEntry);
		tk2dSystem.inst.Editor__Toc = toc.ToArray();

		EditorUtility.SetDirty(tk2dSystem.inst);
		AssetDatabase.SaveAssets();

		return true;
	}

	// Deletes the asset from the global asset dictionary
	// and removes the associated the asset from the build
	public static bool UnmakeLoadableAsset(Object obj)
	{
		string guid = GetObjectGUID(obj);
		List<tk2dResourceTocEntry> toc = new List<tk2dResourceTocEntry>(tk2dSystem.inst.Editor__Toc);
		foreach (tk2dResourceTocEntry entry in toc)
		{
			if (entry.assetGUID == guid)
			{
				// Delete the corresponding resource object
				string resourceObjectPath = AssetDatabase.GUIDToAssetPath(entry.resourceGUID);
				AssetDatabase.DeleteAsset(resourceObjectPath);

				// remove from TOC
				toc.Remove(entry);
				break;
			}
		}

		if (tk2dSystem.inst.Editor__Toc.Length == toc.Count)
		{
			Debug.LogError("tk2dSystem.UnmakeLoadableAsset - Unable to delete asset");
			return false;
		}
		else
		{
			tk2dSystem.inst.Editor__Toc = toc.ToArray();
			EditorUtility.SetDirty(tk2dSystem.inst);
			AssetDatabase.SaveAssets();
			
			return true;
		}
	}

	// Update asset name
	public static void UpdateAssetName(Object obj, string name)
	{
		MakeLoadableAsset(obj, name);
	}

	// This will return false if the system hasn't been initialized, without initializing it.
	public static bool IsLoadableAsset(Object obj)
	{
		string resourcesDir = GetResourcesDir();
		if (resourcesDir.Length == 0) // tk2dSystem hasn't been initialized yet
			return false;

		string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
		string resourcePath = GetResourcesDir() + "/tk2d_" + guid + ".asset";
		return System.IO.File.Exists(resourcePath);
	}

	// Returns the path to the global resources directory
	// It is /Assets/TK2DSYSTEM/Resources by default, but can be moved anywhere
	// When the tk2dSystem object exists, the path to the object will be returned
	public static string GetOrCreateResourcesDir()
	{
		tk2dSystem inst = tk2dSystem.inst;
		string assetPath = AssetDatabase.GetAssetPath(inst);
		if (assetPath.Length > 0)
		{
			// This has already been serialized, just return path as is
			return System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/'); // already serialized
		}
		else
		{
			// Create the system asset
			const string resPath = "Assets/Resources";
			if (!System.IO.Directory.Exists(resPath)) System.IO.Directory.CreateDirectory(resPath);

			const string basePath = resPath + "/tk2d";
			if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);

			assetPath = basePath + "/" + tk2dSystem.assetFileName;
			AssetDatabase.CreateAsset(inst, assetPath);
			
			return basePath;
		}
	}

	// Returns the path to the global resources directory
	// Will not create if it doesn't exists
	static string GetResourcesDir()
	{
		tk2dSystem inst = tk2dSystem.inst_NoCreate;
		if (inst == null) 
			return "";
		else return GetOrCreateResourcesDir(); // this already exists, so this function will follow the correct path
	}

	// Call when platform has changed
	public static void PlatformChanged()
	{
		List<tk2dSpriteCollectionData> changedSpriteCollections = new List<tk2dSpriteCollectionData>();
		tk2dSpriteCollectionData[] allSpriteCollections = Resources.FindObjectsOfTypeAll(typeof(tk2dSpriteCollectionData)) as tk2dSpriteCollectionData[];
		foreach (tk2dSpriteCollectionData scd in allSpriteCollections)
		{
			if (scd.hasPlatformData)
			{
				scd.ResetPlatformData();
				changedSpriteCollections.Add(scd);
			}
		}
		allSpriteCollections = null;

		tk2dFontData[] allFontDatas = Resources.FindObjectsOfTypeAll(typeof(tk2dFontData)) as tk2dFontData[];
		foreach (tk2dFontData fd in allFontDatas)
		{
			if (fd.hasPlatformData)
			{
				fd.ResetPlatformData();
			}
		}
		allFontDatas = null;

		if (changedSpriteCollections.Count == 0)
			return; // nothing worth changing has changed

		// Scan all loaded sprite assets and rebuild them
		foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				System.Type[] types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type.GetInterface("tk2dRuntime.ISpriteCollectionForceBuild") != null)
					{
						Object[] objects = Resources.FindObjectsOfTypeAll(type);

						foreach (var spriteCollectionData in changedSpriteCollections)
						{
							foreach (var o in objects)
							{
								if (tk2dEditorUtility.IsPrefab(o))
									continue;

								tk2dRuntime.ISpriteCollectionForceBuild isc = o as tk2dRuntime.ISpriteCollectionForceBuild;
								if (isc.UsesSpriteCollection(spriteCollectionData))
									isc.ForceBuild();
							}
						}
					}
				}
			}
			catch { }
		}
	}

	public static void RebuildResources()
	{
		// Delete all existing resources
		string systemFileName = tk2dSystem.assetFileName.ToLower();
		string tk2dIndexDir = "Assets/Resources/tk2d";
		if (System.IO.Directory.Exists(tk2dIndexDir))
		{
			string[] files = System.IO.Directory.GetFiles(tk2dIndexDir);
			foreach (string file in files)
			{
				string filename = System.IO.Path.GetFileName(file).ToLower();
				if (filename.IndexOf(systemFileName) != -1) continue; // don't delete system object
				if (filename.IndexOf("tk2d_") == -1)
				{
					Debug.LogError(string.Format("Unknown file '{0}' in tk2d resources directory, ignoring.", filename));
					continue;
				}
				AssetDatabase.DeleteAsset(file);
			}
		}

		// Delete all referenced resources, in the event they've been moved out of the directory
		if (tk2dSystem.inst_NoCreate != null)
		{
			tk2dSystem sys = tk2dSystem.inst;
			tk2dResourceTocEntry[] toc = sys.Editor__Toc;
			for (int i = 0; i < toc.Length; ++i)
			{
				string path = AssetDatabase.GUIDToAssetPath(toc[i].resourceGUID);
				if (path.Length > 0)
					AssetDatabase.DeleteAsset(path);
			}
			sys.Editor__Toc = new tk2dResourceTocEntry[0]; // clear index
			EditorUtility.SetDirty(sys);
			AssetDatabase.SaveAssets();
		}

		AssetDatabase.Refresh();

		// Need to create new index?
		tk2dSpriteCollectionIndex[] spriteCollectionIndex = tk2dEditorUtility.GetExistingIndex().GetSpriteCollectionIndex();
		tk2dGenericIndexItem[] fontIndex = tk2dEditorUtility.GetExistingIndex().GetFonts();
		int numLoadableAssets = 0;
		foreach (tk2dGenericIndexItem font in fontIndex) { if (font.managed || font.loadable) numLoadableAssets++; }
		foreach (tk2dSpriteCollectionIndex sc in spriteCollectionIndex) { if (sc.managedSpriteCollection || sc.loadable) numLoadableAssets++; }

		// Need an index
		if (numLoadableAssets > 0)
		{
			// If it already existed, the index would have been cleared by now
			tk2dSystem sys = tk2dSystem.inst;

			foreach (tk2dGenericIndexItem font in fontIndex)
			{
				if (font.managed || font.loadable) AddFontFromIndex(font);
				tk2dEditorUtility.CollectAndUnloadUnusedAssets();
			}
			foreach (tk2dSpriteCollectionIndex sc in spriteCollectionIndex)
			{
				if (sc.managedSpriteCollection || sc.loadable) AddSpriteCollectionFromIndex(sc);
				tk2dEditorUtility.CollectAndUnloadUnusedAssets();
			}

			Debug.Log(string.Format("Rebuilt {0} resources for tk2dSystem", sys.Editor__Toc.Length));
		}

		tk2dEditorUtility.CollectAndUnloadUnusedAssets();
	}

	static void AddSpriteCollectionFromIndex(tk2dSpriteCollectionIndex indexEntry)
	{
		string path = AssetDatabase.GUIDToAssetPath( indexEntry.spriteCollectionDataGUID );
		tk2dSpriteCollectionData data = AssetDatabase.LoadAssetAtPath(path, typeof(tk2dSpriteCollectionData)) as tk2dSpriteCollectionData;
		if (data == null)
		{
			Debug.LogError(string.Format("Unable to load sprite collection '{0}' at path '{1}'", indexEntry.name, path));
			return;
		}
		MakeLoadableAsset(data, data.assetName);
		data = null;
	}

	static void AddFontFromIndex(tk2dGenericIndexItem indexEntry)
	{
		string path = AssetDatabase.GUIDToAssetPath( indexEntry.dataGUID );
		tk2dFontData data = AssetDatabase.LoadAssetAtPath(path, typeof(tk2dFontData)) as tk2dFontData;
		if (data == null)
		{
			Debug.LogError(string.Format("Unable to load font data '{0}' at path '{1}'", indexEntry.AssetName, path));
			return;
		}
		MakeLoadableAsset(data, ""); // can't make it directly loadable, hence no asset name
		data = null;
	}
}
