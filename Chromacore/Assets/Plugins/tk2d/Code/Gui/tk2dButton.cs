using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Deprecated/GUI/tk2dButton")]
public class tk2dButton : MonoBehaviour 
{
	/// <summary>
	/// The camera this button is meant to be viewed from.
	/// Set this explicitly for best performance.\n
	/// The system will automatically traverse up the hierarchy to find a camera if this is not set.\n
	/// If nothing is found, it will fall back to the active <see cref="tk2dCamera"/>.\n
	/// Failing that, it will use Camera.main.
	/// </summary>
	public Camera viewCamera;
	
	// Button Up = normal state
	// Button Down = held down
	// Button Pressed = after it is pressed and activated
	
	/// <summary>
	/// The button down sprite. This is resolved by name from the sprite collection of the sprite component.
	/// </summary>
	public string buttonDownSprite = "button_down";
	/// <summary>
	/// The button up sprite. This is resolved by name from the sprite collection of the sprite component.
	/// </summary>
	public string buttonUpSprite = "button_up";
	/// <summary>
	/// The button pressed sprite. This is resolved by name from the sprite collection of the sprite component.
	/// </summary>
	public string buttonPressedSprite = "button_up";
	
	int buttonDownSpriteId = -1, buttonUpSpriteId = -1, buttonPressedSpriteId = -1;
	
	/// <summary>
	/// Audio clip to play when the button transitions from up to down state. Requires an AudioSource component to be attached to work.
	/// </summary>
	public AudioClip buttonDownSound = null;
	/// <summary>
	/// Audio clip to play when the button transitions from down to up state. Requires an AudioSource component to be attached to work.
	/// </summary>
	public AudioClip buttonUpSound = null;
	/// <summary>
	/// Audio clip to play when the button is pressed. Requires an AudioSource component to be attached to work.
	/// </summary>
	public AudioClip buttonPressedSound = null;

	
	// Delegates
	
	/// <summary>
	/// Button event handler delegate.
	/// </summary>
	public delegate void ButtonHandlerDelegate(tk2dButton source);
	
	
	// Messaging
	
	/// <summary>
	/// Target object to send the message to. The event methods below are significantly more efficient.
	/// </summary>
	public GameObject targetObject = null;
	/// <summary>
	/// The message to send to the object. This should be the name of the method which needs to be called.
	/// </summary>
    public string messageName = "";
	
	/// <summary>
	/// Occurs when button is pressed (tapped, and finger lifted while inside the button)
	/// </summary>
	public event ButtonHandlerDelegate ButtonPressedEvent;
	
	/// <summary>
	/// Occurs every frame for as long as the button is held down.
	/// </summary>
	public event ButtonHandlerDelegate ButtonAutoFireEvent;
	/// <summary>
	/// Occurs when button transition from up to down state
	/// </summary>
	public event ButtonHandlerDelegate ButtonDownEvent;
	/// <summary>
	/// Occurs when button transitions from down to up state
	/// </summary>
	public event ButtonHandlerDelegate ButtonUpEvent;
	
	tk2dBaseSprite sprite;
	bool buttonDown = false;
	
	/// <summary>
	/// How much to scale the sprite when the button is in the down state
	/// </summary>
	public float targetScale = 1.1f;
	/// <summary>
	/// The length of time the scale operation takes
	/// </summary>
	public float scaleTime = 0.05f;
	/// <summary>
	/// How long to wait before allowing the button to be pressed again, in seconds.
	/// </summary>
	public float pressedWaitTime = 0.3f;
	
	void OnEnable()
	{
		buttonDown = false;	
	}
	
	// Use this for initialization
	void Start () 
	{
		if (viewCamera == null)
		{
			// Find a camera parent 
            Transform node = transform;
            while (node && node.camera == null)
            {
                node = node.parent;
            }
            if (node && node.camera != null) 
			{
				viewCamera = node.camera;
			}
			
			// See if a tk2dCamera exists
			if (viewCamera == null && tk2dCamera.Instance)
			{
				viewCamera = tk2dCamera.Instance.camera;
			}
			
			// ...otherwise, use the main camera
			if (viewCamera == null)
			{
				viewCamera = Camera.main;
			}
		}
		
		sprite = GetComponent<tk2dBaseSprite>();
		
		// Further tests for sprite not being null aren't necessary, as the IDs will default to -1 in that case. Testing them will be sufficient
		if (sprite)
		{
			// Change this to use animated sprites if necessary
			// Same concept here, lookup Ids and call Play(xxx) instead of .spriteId = xxx
			UpdateSpriteIds();
		}
		
		if (collider == null)
		{
			BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();
			Vector3 colliderSize = newCollider.size;
			colliderSize.z = 0.2f;
			newCollider.size = colliderSize;
		}
		
		if ((buttonDownSound != null || buttonPressedSound != null || buttonUpSound != null) &&
			audio == null)
		{
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSource.playOnAwake = false;
		}
	}
	
	/// <summary>
	/// Call this when the sprite names have changed
	/// </summary>
	public void UpdateSpriteIds()
	{
		buttonDownSpriteId 		= (buttonDownSprite.Length > 0)?sprite.GetSpriteIdByName(buttonDownSprite):-1;
		buttonUpSpriteId 		= (buttonUpSprite.Length > 0)?sprite.GetSpriteIdByName(buttonUpSprite):-1;
		buttonPressedSpriteId 	= (buttonPressedSprite.Length > 0)?sprite.GetSpriteIdByName(buttonPressedSprite):-1;
	}
	
	// Modify this to suit your audio solution
	// In our case, we have a global audio manager to play one shot sounds and pool them
	void PlaySound(AudioClip source)
	{
		if (audio && source)
		{
			audio.PlayOneShot(source);
		}
	}
	
	IEnumerator coScale(Vector3 defaultScale, float startScale, float endScale)
    {
		float t0 = Time.realtimeSinceStartup;
		
		Vector3 scale = defaultScale;
		float s = 0.0f;
		while (s < scaleTime)
		{
			float t = Mathf.Clamp01(s / scaleTime);
			float scl = Mathf.Lerp(startScale, endScale, t);
			scale = defaultScale * scl;
			transform.localScale = scale;
			
			yield return 0;
			s = (Time.realtimeSinceStartup - t0);
		}
		
		transform.localScale = defaultScale * endScale;
    }
	
	IEnumerator LocalWaitForSeconds(float seconds)
	{
		float t0 = Time.realtimeSinceStartup;
		float s = 0.0f;
		while (s < seconds)
		{
			yield return 0;
			s = (Time.realtimeSinceStartup - t0);
		}
	}
	
	IEnumerator coHandleButtonPress(int fingerId)
	{
		buttonDown = true; // inhibit processing in Update()
		bool buttonPressed = true; // the button is currently being pressed
		
		Vector3 defaultScale = transform.localScale;
		
		// Button has been pressed for the first time, cursor/finger is still on it
		if (targetScale != 1.0f)
		{
			// Only do this when the scale is actually enabled, to save one frame of latency when not needed
			yield return StartCoroutine( coScale(defaultScale, 1.0f, targetScale) );
		}
		PlaySound(buttonDownSound);
		if (buttonDownSpriteId != -1)
			sprite.spriteId = buttonDownSpriteId;
		
		if (ButtonDownEvent != null)
			ButtonDownEvent(this);
		
		while (true)
		{
			Vector3 cursorPosition = Vector3.zero;
			bool cursorActive = true;

			// slightly akward arrangement to keep exact backwards compatibility
#if !UNITY_FLASH
			if (fingerId != -1)
			{
				bool found = false;
				for (int i = 0; i < Input.touchCount; ++i)
				{
					Touch touch = Input.GetTouch(i);
					if (touch.fingerId == fingerId)
					{
						if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
							break; // treat as not found
						cursorPosition = touch.position;
						found = true;
					}
				}

				if (!found) cursorActive = false;
			} 			
			else
#endif
			{
				if (!Input.GetMouseButton(0))
					cursorActive = false;
				cursorPosition = Input.mousePosition;
			}

			// user is no longer pressing mouse or no longer touching button
			if (!cursorActive) 
				break; 

            Ray ray = viewCamera.ScreenPointToRay(cursorPosition);

            RaycastHit hitInfo;
			bool colliderHit = collider.Raycast(ray, out hitInfo, Mathf.Infinity);
            if (buttonPressed && !colliderHit)
			{
				if (targetScale != 1.0f)
				{
					// Finger is still on screen / button is still down, but the cursor has left the bounds of the button
					yield return StartCoroutine( coScale(defaultScale, targetScale, 1.0f) );
				}
				PlaySound(buttonUpSound);
				if (buttonUpSpriteId != -1)
					sprite.spriteId = buttonUpSpriteId;
				
				if (ButtonUpEvent != null)
					ButtonUpEvent(this);

				buttonPressed = false;
			}
			else if (!buttonPressed & colliderHit)
			{
				if (targetScale != 1.0f)
				{
					// Cursor had left the bounds before, but now has come back in
					yield return StartCoroutine( coScale(defaultScale, 1.0f, targetScale) );
				}
				PlaySound(buttonDownSound);
				if (buttonDownSpriteId != -1)
					sprite.spriteId =  buttonDownSpriteId;
				
				if (ButtonDownEvent != null)
					ButtonDownEvent(this);

				buttonPressed = true;
			}
			
			if (buttonPressed && ButtonAutoFireEvent != null)
			{
				ButtonAutoFireEvent(this);
			}
			
			yield return 0;
		}
		
		if (buttonPressed)
		{
			if (targetScale != 1.0f)
			{
				// Handle case when cursor was in bounds when the button was released / finger lifted
				yield return StartCoroutine( coScale(defaultScale, targetScale, 1.0f) );
			}
			PlaySound(buttonPressedSound);
			if (buttonPressedSpriteId != -1)
				sprite.spriteId = buttonPressedSpriteId;
				
			if (targetObject)
			{
				targetObject.SendMessage(messageName);
			}

			if (ButtonUpEvent != null)
				ButtonUpEvent(this);
			
			if (ButtonPressedEvent != null)
				ButtonPressedEvent(this);
			
			// Button may have been deactivated in ButtonPressed / Up event
			// Don't wait in that case
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
			if (gameObject.active)
#else
			if (gameObject.activeInHierarchy)
#endif
			{
				yield return StartCoroutine(LocalWaitForSeconds(pressedWaitTime));
			}
			
			if (buttonUpSpriteId != -1)
				sprite.spriteId = buttonUpSpriteId;
		}
		
		buttonDown = false;
	}

	
	// Update is called once per frame
	void Update ()
	{
		if (buttonDown) // only need to process if button isn't down
			return;

#if !UNITY_FLASH
		bool detected = false;
		if (Input.multiTouchEnabled)
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase != TouchPhase.Began) continue;
	            Ray ray = viewCamera.ScreenPointToRay(touch.position);
	            RaycastHit hitInfo;
	            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
	            {
					if (!Physics.Raycast(ray, hitInfo.distance - 0.01f))
					{
						StartCoroutine(coHandleButtonPress(touch.fingerId));
						detected = true;
						break; // only one finger on a buton, please.
					}
	            }	            
			}
		}
		if (!detected)
#endif
		{
			if (Input.GetMouseButtonDown(0))
	        {
	            Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
	            RaycastHit hitInfo;
	            if (collider.Raycast(ray, out hitInfo, 1.0e8f))
	            {
					if (!Physics.Raycast(ray, hitInfo.distance - 0.01f))
						StartCoroutine(coHandleButtonPress(-1));
	            }
	        }
		}
	}
}
