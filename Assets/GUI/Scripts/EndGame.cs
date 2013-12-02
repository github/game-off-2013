using UnityEngine;
using System.Collections;

public class EndGame : MonoBehaviour {
	
	public GUIStyle defeat;
	public GUIStyle victory;
	
	void OnGUI() {
		if (GameState.state == GameState.GameStates.Defeat) {
			GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 500, 50), "Zombies Eradicated!", defeat);
		}
		else if (GameState.state == GameState.GameStates.Victory) {
			GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 500, 50), "Total Infection Achieved!", victory);
			GUI.Label(new Rect(Screen.width / 2, Screen.height / 2 + 50, 500, 50), "Time (seconds): " + GameState.endTime, victory);
		}
	}
}
