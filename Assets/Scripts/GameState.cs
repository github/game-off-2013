using UnityEngine;
using System.Collections;

public class GameState : MonoBehaviour {
	
	public enum GameStates {Defeat, Victory, Setup, Running};
	public static GameStates state;
	public static float endTime;
	
	// Use this for initialization
	void Start () {
		state = GameStates.Setup;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.timeScale != 0 && state != GameStates.Defeat && state != GameStates.Victory) {
			if (GameObject.FindGameObjectsWithTag("Zombie").Length == 0) {
				state = GameStates.Defeat;
				endTime = Time.timeSinceLevelLoad;
			}
			else if (GameObject.FindGameObjectsWithTag("Civilian").Length == 0 && GameObject.FindGameObjectsWithTag("Soldier").Length == 0) {
				state = GameStates.Victory;
				endTime = Time.timeSinceLevelLoad;
			}
			else {
				state = GameStates.Running;	
			}
		}
		else if (Time.timeScale == 0) {
			state = GameStates.Setup;	
		}
	}
}
