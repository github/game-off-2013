using UnityEngine;
using System.Collections;

public class EndOfLevel : MonoBehaviour {
	// Texture used to fade in and out from black
	public GUITexture fadeToBlackTexture;
	// Text fading in, "Thanks for playing!"
	public GUIText fadeToBlackThanksText;
	// Text credits developers
	public GUIText creditsDEVS;
	// Text credits Northeastern
	public GUIText creditsNEU;
	
	// Get the scoring system to display final score
	public GameObject scoringSystem;
	
	// The alpha value used to fade in and out
	float alphaFadeValue = 0;
	float text_alphaFadeValue = 0;
	// Bool used to determine if we should fade out.
	bool fadeOutP;
	
	// Use this for initialization
	void Start () {
		// Set texture invisible & black at beginning
		fadeToBlackTexture.color = new Color(0, 0, 0, alphaFadeValue);
		// Set text invisisble & white at beginning
		fadeToBlackThanksText.color = new Color(255, 255, 255, text_alphaFadeValue);
		// Set credits invisible & white at beginning
		creditsDEVS.color = new Color(255, 255, 255, text_alphaFadeValue);
		creditsNEU.color = new Color(255, 255, 255, text_alphaFadeValue);
		
		
		// Do not fade out to black by default
		fadeOutP = false;	
	}
	
	// Update is called once per frame
	void Update () {
		if (fadeOutP){
			FadeOutToBlack();
			FadeInText();
			SetTextPosition();
		}
	}
	
	// Upon colliding with the player, trigger the end of level
	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Player")
		{
			fadeOutP = true;
			Debug.Log(fadeOutP);
		}
	}
	
	// Mimic the camera fading out to black
	private void FadeOutToBlack(){
		Debug.Log("Fade out");
		// Dividing by 8 makes fade lasts 8 secs
		alphaFadeValue += Mathf.Clamp01(Time.deltaTime / 8);
		
		fadeToBlackTexture.color = new Color(0, 0, 0, alphaFadeValue);
		
		// After 15 seconds, reset game
		Invoke("Reset", 15);
	}
	
	// Fade in "Thanks for playing"
	private void FadeInText(){
		// Dividing by 8 makes fade lasts 8 secs
		text_alphaFadeValue += Mathf.Clamp01(Time.deltaTime / 10);
		
		fadeToBlackThanksText.color = new Color(255, 255, 255, text_alphaFadeValue);
		creditsDEVS.color =  new Color(255, 255, 255, text_alphaFadeValue);
		creditsNEU.color =  new Color(255, 255, 255, text_alphaFadeValue);
	}
	
	// Set the position of "Thanks for playing text" GUI to middle center
	void SetTextPosition(){
		fadeToBlackThanksText.pixelOffset = new Vector2(Screen.width / 500, (float)Screen.height / 30f);
		creditsDEVS.pixelOffset = new Vector2(Screen.width / 500, (float)Screen.height / 30f - 70f);
		creditsNEU.pixelOffset = new Vector2(Screen.width / 500, (float)Screen.height / 30f - 120f);
	}
	
	// Reset entire game by reloading level after 15 seconds
	void Reset(){
		Application.LoadLevel(0);
	}
}
