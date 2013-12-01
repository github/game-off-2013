using UnityEngine;
using System.Collections;

public class tk2dUIDemo5Controller : tk2dUIBaseDemoController {

	public tk2dUILayout prefabItem;

	// Manually set up a scrollable area by working out offsets manually
	public tk2dUIScrollableArea manualScrollableArea;
	public tk2dUILayout lastListItem;

	// For automatically setting up a scrollable area using layout containers
	public tk2dUIScrollableArea autoScrollableArea;

	void CustomizeListObject( Transform contentRoot ) {
		string[] firstPart = { "Ba", "Po", "Re", "Zu", "Meh", "Ra'", "B'k", "Adam", "Ben", "George" };
		string[] secondPart = { "Hoopler", "Hysleria", "Yeinydd", "Nekmit", "Novanoid", "Toog1t", "Yboiveth", "Resaix", "Voquev", "Yimello", "Oleald", "Digikiki", "Nocobot", "Morath", "Toximble", "Rodrup", "Chillaid", "Brewtine", "Surogou", "Winooze", "Hendassa", "Ekcle", "Noelind", "Animepolis", "Tupress", "Jeren", "Yoffa", "Acaer" };
		string name = firstPart[Random.Range(0, firstPart.Length)] + " " + secondPart[Random.Range(0, secondPart.Length)];
 		Color color = new Color32((byte)Random.Range(192, 255), (byte)Random.Range(192, 255), (byte)Random.Range(192, 255), 255);
		contentRoot.Find("Name").GetComponent<tk2dTextMesh>().text = name;
		contentRoot.Find("HP").GetComponent<tk2dTextMesh>().text = "HP: " + Random.Range(100, 512).ToString();
		contentRoot.Find("MP").GetComponent<tk2dTextMesh>().text = "MP: " + (Random.Range(2, 40) * 10).ToString();
		contentRoot.Find("Portrait").GetComponent<tk2dBaseSprite>().color = color;
	}

	void Start () {
		// Disable the prefab item
		// don't want it visible when the game is running, as it is in the scene
		prefabItem.transform.parent = null;
		DoSetActive( prefabItem.transform, false );

		// Add a bunch of items to the manual list
		// You will need to parent the object manually and then calculate the step
		float x = 0;
		float w = (prefabItem.GetMaxBounds() - prefabItem.GetMinBounds()).x;
		for (int i = 0; i < 10; ++i) {
			tk2dUILayout layout = Instantiate(prefabItem) as tk2dUILayout;
			layout.transform.parent = manualScrollableArea.contentContainer.transform;
			layout.transform.localPosition = new Vector3(x, 0, 0);
			DoSetActive( layout.transform, true );
			CustomizeListObject( layout.transform );
			x += w;
		}
		lastListItem.transform.localPosition = new Vector3(x, lastListItem.transform.localPosition.y, 0);
		x += (lastListItem.GetMaxBounds() - lastListItem.GetMinBounds()).x;
		manualScrollableArea.ContentLength = x;

		// And some initial entries to the automatic layout list
		// ContentLayoutContainer.AddLayoutAtIndex inserts the layout at a position at the index
		// The main difference is that we don't need to calculate offset correctly - we simply insert
		// as needed and the layout container deals with the rest.
		for (int i = 0; i < 10; ++i) {
			tk2dUILayout layout = Instantiate(prefabItem) as tk2dUILayout;
			autoScrollableArea.ContentLayoutContainer.AddLayoutAtIndex(layout, tk2dUILayoutItem.FixedSizeLayoutItem(), autoScrollableArea.ContentLayoutContainer.ItemCount - 1);
			DoSetActive( layout.transform, true );
			CustomizeListObject( layout.transform );
		}
	}

	IEnumerator AddSomeItemsManual() {
		float x = lastListItem.transform.localPosition.x;
		float w = (prefabItem.GetMaxBounds() - prefabItem.GetMinBounds()).x;
		int numToAdd = Random.Range(1, 5);
		for (int i = 0; i < numToAdd; ++i) {
			tk2dUILayout layout = Instantiate(prefabItem) as tk2dUILayout;
			layout.transform.parent = manualScrollableArea.contentContainer.transform;
			layout.transform.localPosition = new Vector3(x, 0, 0);
			DoSetActive( layout.transform, true );
			CustomizeListObject( layout.transform );
			x += w;

			lastListItem.transform.localPosition = new Vector3(x, lastListItem.transform.localPosition.y, 0);
			manualScrollableArea.ContentLength = x + (lastListItem.GetMaxBounds() - lastListItem.GetMinBounds()).x;

			yield return new WaitForSeconds(0.2f);
		}
	}

	IEnumerator AddSomeItemsAuto() {
		int numToAdd = Random.Range(1, 5);
		for (int i = 0; i < numToAdd; ++i) {
			tk2dUILayout layout = Instantiate(prefabItem) as tk2dUILayout;
			autoScrollableArea.ContentLayoutContainer.AddLayoutAtIndex(layout, tk2dUILayoutItem.FixedSizeLayoutItem(), autoScrollableArea.ContentLayoutContainer.ItemCount - 1);
			DoSetActive( layout.transform, true );
			CustomizeListObject( layout.transform );

			yield return new WaitForSeconds(0.2f);
		}
	}
}
