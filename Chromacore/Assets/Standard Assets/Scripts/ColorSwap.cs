using UnityEngine;

using System.Collections;

// This script changes textures from black and white to color when Teli 
// collects the corresponding note. A master object holds both the color
// and monochrome textures, as well as the pickups in that area. As Teli
// picks up collectibles, the texture will transition from black and white
// to color

public class ColorSwap : MonoBehaviour {
	
	// Black and White Texture	
	public tk2dBaseSprite mono;
	// Color Texture
	public tk2dBaseSprite multi;
	// Color from the Color Texture
	Color multiC;
	// Amount of collectibles in area
	public int Collectibles;
	// Amount of collectibles picked up
	int collected;
	// Alpha level for textures
	float fade;
	// Current alpha value
	float curFade;
	// current alpha value for monochrome texture
	float curFadeOut;
	// current alpha value for colored texture
	float curFadeIn;
	// Color from the Black and White texture
	Color monoC;
	// Should the colors be swapped?
	bool swap;
	
	// Use this for initialization
	void Start () {
		// Initialize delta fade
		fade = (float)1/(float)Collectibles;
		// At the start, there should be no color swapping
		swap = false;
		// Get the color value of the color texture
		multiC = multi.color;
		// Set thet color texture invisible
		multiC.a = 0.0f;
		// Set the invisible color texture to the Color
		multi.color = multiC;
		// Get the color of the monochrome texture
		monoC = mono.color;
		//// Set the monochrome texture visible
		monoC.a = 1.0f;
		//Set the monochrome visibility to the texture
		mono.color = monoC;
		// The alpha value of the monochrome should start at 1
		curFadeOut = 1;
		// The alpha value of the color should start at 0
		curFadeIn = 0;
		// The fade value should start at 0
		curFade = 0;
	}
	
	void Update(){
		// If an item has been picked up, and the alpha values are
		// below their target fade values, run the fade
		if(swap && multiC.a < curFadeIn && monoC.a > curFadeOut)
		{
			// Increment the alpha value of thet color texture
			multiC.a += .01f;
			// Set this new color to the texure
			multi.color = multiC;
			// Decrement the alpha value of the monochrome texture
			monoC.a -= .01f;
			// Set new color to texture
			mono.color = monoC;
		}
	}
	
	// Sets the new fade values of the textures once the a collectible
	// is collected
	void ChangeColor (){
		// start the swap
		swap = true;
		// An object has been picked up so increment collected
		collected++;
		// The current fade value should be the change in fade times
		// the amount of collectibles collected
		curFade = fade * collected;
		// The monochrome texture should fade out, so subtract 
		// curFade from 1
		curFadeOut = 1 - curFade;
		// The color texture fades in, so it increments up
		curFadeIn = curFade;	
	}
	
}
