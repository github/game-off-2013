using UnityEngine;
using System.Collections;

public class SelectDifficulty : MonoBehaviour {

	void OnGUI() {
		if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 55, 200, 50), "Downtown"))
			Application.LoadLevel(4);	
		else if (GUI.Button (new Rect(Screen.width / 2 - 100, Screen.height / 2, 200, 50), "Parish City"))
			Application.LoadLevel(1);
		else if (GUI.Button (new Rect(Screen.width / 2 - 100, Screen.height / 2 + 55, 200, 50), "The Octagon"))
			Application.LoadLevel(3);
		else if (GUI.Button (new Rect(Screen.width / 2 - 100, Screen.height / 2 + 110, 200, 50), "Patient Zero"))
			Application.LoadLevel(2);
	}
}
