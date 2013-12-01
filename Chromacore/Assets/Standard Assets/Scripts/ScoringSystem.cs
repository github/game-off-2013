using UnityEngine;
using System.Collections;

public class ScoringSystem : MonoBehaviour {
	string _numSeen = "0";
	string _numCollected  = "0";
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		ScoreString();
		SetPosition();
	}
	
	// Recieve the number of Notes seen from Inventory.cs
	// Save it to pass on to ScoreString function
	void ScoreSeen(string numSeen){
		_numSeen = numSeen;
	}
	
	// Recieve the number of Notes collected from Inventory.cs
	// Save it to pass on to ScoreString function
	void ScoreCollected(string numCollected){
		_numCollected = numCollected;
	}
	
	// Concatonate the number of Notes seen & collected into 1 string
	// and display the string with the GUI Text object.
	void ScoreString(){
		guiText.text = _numCollected + " / " + _numSeen;
	}
	
	// Set the position of Score GUI to top center
	void SetPosition(){
		guiText.pixelOffset = new Vector2(Screen.width / 500, (float)Screen.height / 2.5f);
	}
}
