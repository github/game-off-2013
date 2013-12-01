using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour {
	
	// Notes that have been collected by the player
	private int notesCollected = 0;
	// Notes that have been seen by the player
	private int notesSeen = 0;
	public GameObject scoringSystem;
	
	// Variables used to save score, zero by default
	private int save_notesCollected = 0;
	private int save_notesSeen = 0;
	
	// Use this for initialization
	void Start () {
		save_notesCollected = 0;
		save_notesSeen = 0;
	}
	
	// Increment notes when the player passes them
	void SeeNote()
	{
		notesSeen++;
		scoringSystem.SendMessage("ScoreSeen", notesSeen.ToString());
		//Debug.Log("Notes Seen: " + notesSeen.ToString());
	}
	
	// Increment notes when the player collects them
	void CollectNote()
	{
		notesCollected++;
		scoringSystem.SendMessage("ScoreCollected", notesCollected.ToString());
		//Debug.Log("Notes Collected: " + notesSeen.ToString());
	}
	
	// Save the current score at this checkpoint
	void getCheckpoint(){
		save_notesCollected = notesCollected;
		save_notesSeen = notesSeen;
	}
	
	// On death, reset score to the saved scores at latest checkpoint
	void ResetScore(){
		notesCollected = save_notesCollected;
		scoringSystem.SendMessage("ScoreCollected", notesCollected.ToString());
		
		notesSeen = save_notesSeen;
		scoringSystem.SendMessage("ScoreSeen", notesSeen.ToString());
	}
		
	
	// Update is called once per frame
	void Update () {
		
	}
}
