using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteAnimator")]
public class Teli_Animation : MonoBehaviour {
	// Spawn point at beginning of level
	public GameObject spawn;
	
	// The main character's Charcter Controller	
	public CharacterController teliCharacter;
	
	// Value of character's Y velocity to begin falling animation at
	private float fallAnimVel = -0.25f;
	
	// Link to animated sprite
	private tk2dSpriteAnimator anim;
	
	// State variable to check if player is running (running by default)
	public bool runningP = true;
	
	// The background music track (used to reset after death)
	public AudioSource backgroundTrack;
	
	// An array of notes
	public GameObject[] Notes;
	
	// The latest timestamp to reset music track at right checkpoint
	float checkpoint_timestamp;
	
	// Use this for initialization
	void Start () {
		// This script must be attached to the sprite to work
		anim = GetComponent<tk2dSpriteAnimator>();
		
		Notes = GameObject.FindGameObjectsWithTag("Note");
	}
		
	// Update is called once per frame
	void Update () {
		// Jump Animation
		if (Input.GetKey(KeyCode.Space)){
			// Only play the clip if it is not already playing.
			// Calling play will restart the clip if it is already playing
			if (!anim.IsPlaying("Jump")){
				anim.Play("Jump");
			}
		}
		
		// If the Player presses the punch button
		if (Input.GetKey(KeyCode.A)){
			// And the charcter isn't currently punching
			if(!anim.IsPlaying("Punch")){
				anim.Play("Punch");
			}
		}
		
		// If none of the other animations are playing
		if (!anim.IsPlaying("Run") & !anim.IsPlaying("Jump") & !anim.IsPlaying("Run_Glow") & !anim.IsPlaying("Punch") & !anim.IsPlaying("Death")){
			// Loop running animation
			anim.Play("Run");
		}
		
		// If the character is falling
		if (teliCharacter.velocity.y < fallAnimVel){
			// And the falling animation isn't already playing
			if (!anim.IsPlaying("Fall")){
				anim.Play("Fall");
			}
		}
		
		// Wait to invoke death animation function until a few
		// seconds after game has begun.
		Invoke("DeathAnimation", 3);
		
		
	}
	
	// Handles death by Edges (death by obstacles is 
	// handled in ObstacleDeath() function)
	void DeathAnimation(){
		// If Teli's X-position stops increasing or the Y position is below level
		if (teliCharacter.velocity.x < 1 || teliCharacter.transform.position.y < -5){
			//Debug.Log(teliCharacter.velocity.x);
			Debug.Log("DEATH");
			// And the death animation isn't already playing
			if(!anim.IsPlaying("Death")){
				// Play the death animation
				anim.Play("Death");
			}
			// Call reset function after 2 seconds
			Invoke("Reset", 1);
		}
	}
	
	// Recieve the checkpoint timestamp from ObstacleDeath.cs and set
	// checkpoint timestamp variable to the latest checkpoint's timestamp
	void getCheckpoint(float timestamp){
		checkpoint_timestamp = timestamp;
	}
	
	// Reset Teli's position, the background track, and respawn Notes
	void Reset(){
		// Send a message to restart Teli's movement
		SendMessageUpwards("death", true);
		// Reset score to last saved score
		SendMessageUpwards("ResetScore");
		// Reset position to spawn
		teliCharacter.transform.position = spawn.transform.position;
		// Reset music
		backgroundTrack.Stop();
		// Set music's start time to checkpoint's start time
		backgroundTrack.time = checkpoint_timestamp;
		// Restart music track
		backgroundTrack.Play();
		
		// Reset the renderer of all Notes
		for(int i = 0; i < Notes.Length; i++){
			Notes[i].renderer.enabled = true;
		}
	}
	
	// Hanlde Glow Animation
	void GlowAnimation(){
		if(!anim.IsPlaying("Run_Glow")){
			anim.Stop();
			anim.Play("Run_Glow");
		}
	}
	
	// On death, stop movement and if the PunchAnimation isn't 
	// already playing	when Death broadcast is recieved, play it.
	void ObstacleDeath(){
		if(!anim.IsPlaying("Punch")){
			Debug.Log("DEATH - Wasn't punching");
			// And the death animation isn't already playing
			if(!anim.IsPlaying("Death")){
				// Play the death animation
				anim.Play("Death");
				// Send a message to stop Teli's movement
				SendMessageUpwards("death", false);
			}
			// Call reset function after 2 seconds
			Invoke("Reset", 1);
		}
	}
}