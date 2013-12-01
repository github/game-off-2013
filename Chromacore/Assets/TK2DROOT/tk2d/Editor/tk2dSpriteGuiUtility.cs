using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class tk2dSpriteGuiUtility 
{
    public static int NameCompare(string na, string nb)
    {
		if (na.Length == 0 && nb.Length != 0) return 1;
		else if (na.Length != 0 && nb.Length == 0) return -1;
		else if (na.Length == 0 && nb.Length == 0) return 0;

        int numStartA = na.Length - 1;

        // last char is not a number, compare as regular strings
        if (na[numStartA] < '0' || na[numStartA] > '9')
            return System.String.Compare(na, nb, true);

        while (numStartA > 0 && na[numStartA - 1] >= '0' && na[numStartA - 1] <= '9')
            numStartA--;

        int comp = System.String.Compare(na, 0, nb, 0, numStartA);

        if (comp == 0)
        {
            if (nb.Length > numStartA)
            {
                bool numeric = true;
                for (int i = numStartA; i < nb.Length; ++i)
                {
                    if (nb[i] < '0' || nb[i] > '9')
                    {
                        numeric = false;
                        break;
                    }
                }

                if (numeric)
                {
                    int numA = System.Convert.ToInt32(na.Substring(numStartA));
                    int numB = System.Convert.ToInt32(nb.Substring(numStartA));
                    return numA - numB;
                }
            }
        }

        return System.String.Compare(na, nb);
    }

    public delegate void SpriteChangedCallback(tk2dSpriteCollectionData spriteCollection, int spriteIndex, object callbackData);
	
	class SpriteCollectionLUT
	{
		public int buildKey;
		public string[] sortedSpriteNames;
		public int[] spriteIdToSortedList;
		public int[] sortedListToSpriteId;
	}
	static Dictionary<string, SpriteCollectionLUT> spriteSelectorLUT = new Dictionary<string, SpriteCollectionLUT>();
	
	static int GetNamedSpriteInNewCollection(tk2dSpriteCollectionData spriteCollection, int spriteId, tk2dSpriteCollectionData newCollection) {
		int newSpriteId = spriteId;
		string oldSpriteName = (spriteCollection == null || spriteCollection.inst == null) ? "" : spriteCollection.inst.spriteDefinitions[spriteId].name;
		int distance = -1;
		for (int i = 0; i < newCollection.inst.spriteDefinitions.Length; ++i) {
			if (newCollection.inst.spriteDefinitions[i].Valid) {
				string newSpriteName = newCollection.inst.spriteDefinitions[i].name;

				int tmpDistance = (newSpriteName == oldSpriteName) ? 0 :
								  Mathf.Abs ( (oldSpriteName.ToLower()).CompareTo(newSpriteName.ToLower ()));

				if (distance == -1 || tmpDistance < distance) {
					distance = tmpDistance;
					newSpriteId = i;
				}
			}
		}
		return newSpriteId;
	}

	public static bool showOpenEditShortcuts = true;

	public static void SpriteSelector( tk2dSpriteCollectionData spriteCollection, int spriteId, SpriteChangedCallback callback, object callbackData) {
		tk2dSpriteCollectionData newCollection = spriteCollection;
		int newSpriteId = spriteId;

		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical();

		GUILayout.BeginHorizontal();
		newCollection = SpriteCollectionList("Collection", newCollection);

		if (newCollection != spriteCollection) {
			newSpriteId = GetNamedSpriteInNewCollection( spriteCollection, spriteId, newCollection );
		}

		if (showOpenEditShortcuts && newCollection != null && GUILayout.Button("o", EditorStyles.miniButton, GUILayout.Width(18))) {
			EditorGUIUtility.PingObject(newCollection);
		}
		GUILayout.EndHorizontal();

		if (newCollection != null && newCollection.Count != 0) {
			if (newSpriteId < 0 || newSpriteId >= newCollection.Count || !newCollection.inst.spriteDefinitions[newSpriteId].Valid) {
				newSpriteId = newCollection.FirstValidDefinitionIndex;
			}

			GUILayout.BeginHorizontal();
			newSpriteId = SpriteList( "Sprite", newSpriteId, newCollection );

			if ( showOpenEditShortcuts &&
				 newCollection != null && newCollection.dataGuid != TransientGUID && 
				 GUILayout.Button( "e", EditorStyles.miniButton, GUILayout.Width(18), GUILayout.MaxHeight( 14f ) ) ) {
				tk2dSpriteCollection gen = AssetDatabase.LoadAssetAtPath( AssetDatabase.GUIDToAssetPath(newCollection.spriteCollectionGUID), typeof(tk2dSpriteCollection) ) as tk2dSpriteCollection;
				if ( gen != null ) {
					tk2dSpriteCollectionEditorPopup v = EditorWindow.GetWindow( typeof(tk2dSpriteCollectionEditorPopup), false, "Sprite Collection Editor" ) as tk2dSpriteCollectionEditorPopup;
					v.SetGeneratorAndSelectedSprite(gen, newSpriteId);
				}
			}  

			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();

		if (newCollection != null && GUILayout.Button("...", GUILayout.Height(32), GUILayout.Width(32))) {
			SpriteSelectorPopup( newCollection, newSpriteId, callback, callbackData );	
		}

		GUILayout.EndHorizontal();

		// Handle drag and drop
		Rect rect = GUILayoutUtility.GetLastRect();
		if (rect.Contains(Event.current.mousePosition)) {
			if (Event.current.type == EventType.DragUpdated) {
				bool valid = false;
				if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0] is GameObject) {
					GameObject go = DragAndDrop.objectReferences[0] as GameObject;
					if (go.GetComponent<tk2dSpriteCollection>() || go.GetComponent<tk2dSpriteCollectionData>()) {
						valid = true;
					}
					Event.current.Use();
				}
				
				if (valid) {
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				}
				else {
					DragAndDrop.visualMode = DragAndDropVisualMode.None;
				}
				Event.current.Use();
			}
			else if (Event.current.type == EventType.DragPerform) {
				DragAndDrop.visualMode = DragAndDropVisualMode.None;
				Event.current.Use();

				GameObject go = DragAndDrop.objectReferences[0] as GameObject;
				tk2dSpriteCollection sc = go.GetComponent<tk2dSpriteCollection>();
				tk2dSpriteCollectionData scd = go.GetComponent<tk2dSpriteCollectionData>();
				if (sc != null && scd == null) {
					scd = sc.spriteCollection;
				}
				if (scd != null) {
					newCollection = scd;
					if (newCollection != spriteCollection) {
						newSpriteId = GetNamedSpriteInNewCollection( spriteCollection, spriteId, newCollection );
					}
				}
			}
		}

		// Final callback
		if (callback != null && (newCollection != spriteCollection || newSpriteId != spriteId)) {
			callback(newCollection, newSpriteId, callbackData);
		}
	}

	public static void SpriteSelectorPopup( tk2dSpriteCollectionData spriteCollection, int spriteId, SpriteChangedCallback callback, object callbackData) {
		tk2dSpritePickerPopup.DoPickSprite(spriteCollection, spriteId, "Select sprite", callback, callbackData);
	}

	static int SpriteList(string label, int spriteId, tk2dSpriteCollectionData rootSpriteCollection)
	{
		tk2dSpriteCollectionData spriteCollection = rootSpriteCollection.inst;
		int newSpriteId = spriteId;
		
		// cope with guid not existing
		if (spriteCollection.dataGuid == null || spriteCollection.dataGuid.Length == 0)
		{
			spriteCollection.dataGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spriteCollection));
		}
		
		SpriteCollectionLUT lut = null; 
		spriteSelectorLUT.TryGetValue(spriteCollection.dataGuid, out lut);
		if (lut == null)
		{
			lut = new SpriteCollectionLUT();
			lut.buildKey = spriteCollection.buildKey - 1; // force mismatch
			spriteSelectorLUT[spriteCollection.dataGuid] = lut;
		}
		
		if (lut.buildKey != spriteCollection.buildKey)
		{
			var spriteDefs = spriteCollection.spriteDefinitions;
			string[] spriteNames = new string[spriteDefs.Length];
			int[] spriteLookupIndices = new int[spriteNames.Length];
			for (int i = 0; i < spriteDefs.Length; ++i)
			{
				if (spriteDefs[i].name != null && spriteDefs[i].name.Length > 0)
				{
					if (tk2dPreferences.inst.showIds)
						spriteNames[i] = spriteDefs[i].name + "\t[" + i.ToString() + "]";
					else
						spriteNames[i] = spriteDefs[i].name;
					spriteLookupIndices[i] = i;
				}
			}
			System.Array.Sort(spriteLookupIndices, (int a, int b) => tk2dSpriteGuiUtility.NameCompare((spriteDefs[a]!=null)?spriteDefs[a].name:"", (spriteDefs[b]!=null)?spriteDefs[b].name:""));
			
			lut.sortedSpriteNames = new string[spriteNames.Length];
			lut.sortedListToSpriteId = new int[spriteNames.Length];
			lut.spriteIdToSortedList = new int[spriteNames.Length];
			
			for (int i = 0; i < spriteLookupIndices.Length; ++i)
			{
				lut.spriteIdToSortedList[spriteLookupIndices[i]] = i;
				lut.sortedListToSpriteId[i] = spriteLookupIndices[i];
				lut.sortedSpriteNames[i] = spriteNames[spriteLookupIndices[i]];
			}
			
			lut.buildKey = spriteCollection.buildKey;
		}
		
		GUILayout.BeginHorizontal();
		if (spriteId >= 0 && spriteId < lut.spriteIdToSortedList.Length) {
			int spriteLocalIndex = lut.spriteIdToSortedList[spriteId];
			int newSpriteLocalIndex = (label == null)?EditorGUILayout.Popup(spriteLocalIndex, lut.sortedSpriteNames):EditorGUILayout.Popup(label, spriteLocalIndex, lut.sortedSpriteNames);
			if (newSpriteLocalIndex != spriteLocalIndex)
			{
				newSpriteId = lut.sortedListToSpriteId[newSpriteLocalIndex];
			}
		}
		GUILayout.EndHorizontal();
		
		return newSpriteId;
	}
	
	static List<tk2dSpriteCollectionIndex> allSpriteCollections = new List<tk2dSpriteCollectionIndex>();
	static Dictionary<string, int> allSpriteCollectionLookup = new Dictionary<string, int>();
	static string[] spriteCollectionNames = new string[0];
	static string[] spriteCollectionNamesInclTransient = new string[0];

	public static void GetSpriteCollectionAndCreate( System.Action<tk2dSpriteCollectionData> create ) {
		// try to inherit from other Sprites in scene
		tk2dBaseSprite spr = GameObject.FindObjectOfType(typeof(tk2dBaseSprite)) as tk2dBaseSprite;
		if (spr) {
			create( spr.Collection );
			return;
		}
		else {
			tk2dSpriteCollectionData data = GetDefaultSpriteCollection();
			if (data != null) {
				create( data );
				return;
			}
		}
		EditorUtility.DisplayDialog("Create Sprite", "Unable to create sprite as no valid SpriteCollections have been found.", "Ok");
	}

	public static tk2dSpriteCollectionData GetDefaultSpriteCollection() {
		BuildLookupIndex(false);
		
		foreach (tk2dSpriteCollectionIndex indexEntry in allSpriteCollections)
		{
			if (!indexEntry.managedSpriteCollection && indexEntry.spriteNames != null)
			{
				foreach (string name in indexEntry.spriteNames)
				{
					if (name != null && name.Length > 0)
					{
						GameObject scgo = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(indexEntry.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
						if (scgo != null)
							return scgo.GetComponent<tk2dSpriteCollectionData>();
					}
				}
			}
		}
		
		Debug.LogError("Unable to find any sprite collections.");
		return null;
	}
		
	static void BuildLookupIndex(bool force)
	{
		if (force)
			tk2dEditorUtility.ForceCreateIndex();
		
		allSpriteCollections = new List<tk2dSpriteCollectionIndex>();
		tk2dSpriteCollectionIndex[] mainIndex = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
		foreach (tk2dSpriteCollectionIndex i in mainIndex)
		{
			if (!i.managedSpriteCollection)
				allSpriteCollections.Add(i);
		}

		allSpriteCollections = allSpriteCollections.OrderBy( e => e.name, new tk2dEditor.Shared.NaturalComparer() ).ToList();
		allSpriteCollectionLookup = new Dictionary<string, int>();
		
		spriteCollectionNames = new string[allSpriteCollections.Count];
		spriteCollectionNamesInclTransient = new string[allSpriteCollections.Count + 1];
		for (int i = 0; i < allSpriteCollections.Count; ++i)
		{
			allSpriteCollectionLookup[allSpriteCollections[i].spriteCollectionDataGUID] = i;
			spriteCollectionNames[i] = allSpriteCollections[i].name;
			spriteCollectionNamesInclTransient[i] = allSpriteCollections[i].name;
		}
		spriteCollectionNamesInclTransient[allSpriteCollections.Count] = "-"; // transient sprite collection
	}

	public static void ResetCache()
	{
		allSpriteCollections.Clear();
	}
	
	static tk2dSpriteCollectionData GetSpriteCollectionDataAtIndex(int index, tk2dSpriteCollectionData defaultValue)
	{
		if (index >= allSpriteCollections.Count) return defaultValue;
		GameObject go = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allSpriteCollections[index].spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
		if (go == null) return defaultValue;
		tk2dSpriteCollectionData data = go.GetComponent<tk2dSpriteCollectionData>();
		if (data == null) return defaultValue;
		return data;
	}
	
	public static string TransientGUID { get { return "transient"; } }
	
	public static int GetValidSpriteId(tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		if (! (spriteId > 0 && spriteId < spriteCollection.spriteDefinitions.Length && 
			spriteCollection.spriteDefinitions[spriteId].Valid) )
		{
			spriteId = spriteCollection.FirstValidDefinitionIndex;
			if (spriteId == -1) spriteId = 0;
		}
		return spriteId;
	}
	
	public static tk2dSpriteCollectionData SpriteCollectionList(tk2dSpriteCollectionData currentValue) {
		// Initialize guid if not present
		if (currentValue != null && (currentValue.dataGuid == null || currentValue.dataGuid.Length == 0))
		{
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(currentValue));
			currentValue.dataGuid = (guid.Length == 0)?TransientGUID:guid;
		}
		
		if (allSpriteCollections == null || allSpriteCollections.Count == 0)
			BuildLookupIndex(false);
		
		// if the sprite collection asset path == "", it means its either been created in the scene, or loaded from an asset bundle
		if (currentValue == null || AssetDatabase.GetAssetPath(currentValue).Length == 0 || currentValue.dataGuid == TransientGUID)
		{
			int currentSelection = allSpriteCollections.Count;
			int newSelection = EditorGUILayout.Popup(currentSelection, spriteCollectionNamesInclTransient);
			if (newSelection != currentSelection)
			{
				currentValue = GetSpriteCollectionDataAtIndex(newSelection, currentValue);
				GUI.changed = true;
			}
		}
		else
		{
			int currentSelection = -1;
			for (int iter = 0; iter < 2; ++iter) // 2 passes in worst case
			{
				for (int i = 0; i < allSpriteCollections.Count; ++i)
				{
					if (allSpriteCollections[i].spriteCollectionDataGUID == currentValue.dataGuid)
					{
						currentSelection = i;
						break;
					}
				}
				
				if (currentSelection != -1) break; // found something on first pass

				// we are missing a sprite collection, rebuild index
				BuildLookupIndex(true);
			}
			
			if (currentSelection == -1)
			{
				Debug.LogError("Unable to find sprite collection. This is a serious problem.");
				GUILayout.Label(currentValue.spriteCollectionName, EditorStyles.popup);
			}
			else
			{
				int newSelection = EditorGUILayout.Popup(currentSelection, spriteCollectionNames);
				if (newSelection != currentSelection)
				{
					tk2dSpriteCollectionData newData = GetSpriteCollectionDataAtIndex(newSelection, currentValue);
					if (newData == null)
					{
						Debug.LogError("Unable to load sprite collection. Please rebuild index and try again.");
					}
					else if (newData.Count == 0)
					{
						EditorUtility.DisplayDialog("Error", 
							string.Format("Sprite collection '{0}' has no sprites", newData.name), 
							"Ok");						
					}
					else if (newData != currentValue)
					{
						currentValue = newData;
						GUI.changed = true;						
					}
				}
			}
		}	
		
		return currentValue;
	}

	public static tk2dSpriteCollectionData SpriteCollectionList(string label, tk2dSpriteCollectionData currentValue)
	{
		GUILayout.BeginHorizontal();
		if (label.Length > 0)
			EditorGUILayout.PrefixLabel(label);

		currentValue = SpriteCollectionList(currentValue);
		GUILayout.EndHorizontal();

		return currentValue;
	}
}
