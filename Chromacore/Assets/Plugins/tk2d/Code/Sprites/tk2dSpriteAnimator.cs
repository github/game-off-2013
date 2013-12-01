using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteAnimator")]
/// <summary>
/// Sprite animator.
/// Attach this to a sprite class to animate it.
/// </summary>
public class tk2dSpriteAnimator : MonoBehaviour
{
	[SerializeField] tk2dSpriteAnimation library;
	[SerializeField] int defaultClipId = 0;

	/// <summary>
	/// Interface option to play the animation automatically when instantiated / game is started. Useful for background looping animations.
	/// </summary>
	public bool playAutomatically = false;
	
	// This is now an int so we'll be able to or bitmasks
	static State globalState = 0;

	/// <summary>
	/// Globally pause all animated sprites
	/// </summary>
	public static bool g_Paused
	{
		get { return (globalState & State.Paused) != 0; }
		set { globalState = value?State.Paused:(State)0; }
	}

	/// <summary>
	/// Get or set pause state on this current sprite
	/// </summary>
	public bool Paused
	{
		get { return (state & State.Paused) != 0; }
		set 
		{ 
			if (value) state |= State.Paused;
			else state &= ~State.Paused;
		}
	}


	/// <summary>
	/// <see cref="tk2dSpriteAnimation"/>
	/// </summary>
	public tk2dSpriteAnimation Library {
		get { return library; }
		set { library = value; }
	}

	/// <summary>
	/// The default clip used when Play is called with out any parameters
	/// </summary>
	public int DefaultClipId {
		get { return defaultClipId; }
		set { defaultClipId = value; }
	}

	/// <summary>
	/// The default clip
	/// </summary>
	public tk2dSpriteAnimationClip DefaultClip {
		get { return GetClipById(defaultClipId); }
	}


	/// <summary>
	/// Currently active clip
	/// </summary>
	tk2dSpriteAnimationClip currentClip = null;
	
	/// <summary>
	/// Time into the current clip. This is in clip local time (i.e. (int)clipTime = currentFrame)
	/// </summary>
    float clipTime = 0.0f;

	/// <summary>
	/// This is the frame rate of the current clip. Can be changed dynamicaly, as clipTime is accumulated time in real time.
	/// </summary>
    float clipFps = -1.0f;
	
	/// <summary>
	/// Previous frame identifier
	/// </summary>
	int previousFrame = -1;
	
	/// <summary>
	/// Animation completed callback. 
	/// This is called when the animation has completed playing. 
	/// Will not trigger on looped animations.
	/// Parameters (caller, currentClip)
	/// Set to null to clear.
	/// </summary>
	public System.Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip> AnimationCompleted;

	/// <summary>
	/// Animation callback. 
	/// This is called when the frame displayed has <see cref="tk2dSpriteAnimationFrame.triggerEvent"/> set.
	/// The triggering frame index is passed through, and the eventInfo / Int / Float can be extracted.
	/// Parameters (caller, currentClip, currentFrame)
	/// Set to null to clear.
	/// </summary>
	public System.Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int> AnimationEventTriggered;

	enum State 
	{
		Init = 0,
		Playing = 1,
		Paused = 2,
	}
	State state = State.Init; // init state. Do not use elsewhere
	
	void OnEnable() {
		if (Sprite == null) {
			enabled = false;
		}
	}

	void Start()
	{
		if (playAutomatically) {
			Play(DefaultClip);
		}
	}
	
	protected tk2dBaseSprite _sprite = null;
	/// <summary>
	/// Gets the sprite the animator is currently animating
	/// </summary>
	virtual public tk2dBaseSprite Sprite {
		get {
			if (_sprite == null) {
				_sprite = GetComponent<tk2dBaseSprite>();
				if (_sprite == null) {
					Debug.LogError("Sprite not found attached to tk2dSpriteAnimator.");
				}
			}
			return _sprite;
		}
	}

	/// <summary>
	/// Adds a tk2dSpriteAnimator as a component to the gameObject passed in, setting up necessary parameters and building geometry.
	/// </summary>
	public static tk2dSpriteAnimator AddComponent(GameObject go, tk2dSpriteAnimation anim, int clipId)
	{
		tk2dSpriteAnimationClip clip = anim.clips[clipId];
		tk2dSpriteAnimator animSprite = go.AddComponent<tk2dSpriteAnimator>();
		animSprite.Library = anim;
		animSprite.SetSprite(clip.frames[0].spriteCollection, clip.frames[0].spriteId);
		return animSprite;
	}
	
	// This is used for Play("name") and has verbose error messages
	tk2dSpriteAnimationClip GetClipByNameVerbose(string name) {
		if (library == null) {
			Debug.LogError("Library not set");
			return null;
		}
		else {
			tk2dSpriteAnimationClip clip = library.GetClipByName( name );
			if (clip == null) {
				Debug.LogError("Unable to find clip '" + name + "' in library");
				return null;
			}
			else {
				return clip;
			}
		}
	}

#region Play
	/// <summary>
	/// Play the current / last played clip. If no clip has been played, the default clip is used.
	/// Will not restart the clip if it is already playing.
	/// </summary>
	public void Play()
	{
		if (currentClip == null) {
			currentClip = DefaultClip;
		}

		Play(currentClip);
	}

	/// <summary>
	/// Play the specified clip.
	/// Will not restart the clip if it is already playing.
	/// </summary>
	/// <param name='name'>
	/// Name of clip. Try to cache the animation clip Id and use that instead for performance.
	/// </param>
	public void Play(string name)
	{
		Play(GetClipByNameVerbose(name));
	}

	/// <summary>
	/// Play the specified clip.
	/// Will not restart the clip if it is already playing.
	/// </summary>
	public void Play(tk2dSpriteAnimationClip clip) {
		Play(clip, 0, DefaultFps);
	}
	
#region PlayFromFrame
	/// <summary>
	/// Play the current / last played clip. If no clip has been played, the default clip is used.
	/// Will restart the clip at frame if called while the clip is playing.
	/// </summary>
	public void PlayFromFrame(int frame)
	{
		if (currentClip == null) {
			currentClip = DefaultClip;
		}

		PlayFromFrame(currentClip, frame);
	}

	/// <summary>
	/// Play the specified clip, starting at the frame specified.
	/// Will restart the clip at frame if called while the clip is playing.
	/// </summary>
	/// <param name='name'> Name of clip. Try to cache the animation clip Id and use that instead for performance. </param>
	/// <param name='frame'> Frame to start playing from. </param>
	public void PlayFromFrame(string name, int frame)
	{
		PlayFromFrame(GetClipByNameVerbose(name), frame);
	}
	
	/// <summary>
	/// Play the clip specified by identifier, starting at the specified frame.
	/// Will restart the clip at frame if it is already playing.
	/// </summary>
	/// <param name='clip'>Use <see cref="GetClipByName"/> to resolve a named clip id</param>	
	/// <param name='frame'> Frame to start from. </param>
	public void PlayFromFrame(tk2dSpriteAnimationClip clip, int frame)
	{
		PlayFrom(clip, (frame + 0.001f) / clip.fps); // offset ever so slightly to round down correctly
	}
#endregion

#region PlayFrom
	/// <summary>
	/// Play the current / last played clip. If no clip has been played, the default clip is used.
	/// Will restart the clip at frame if called while the clip is playing.
	/// </summary>
	public void PlayFrom(float clipStartTime)
	{
		if (currentClip == null) {
			currentClip = DefaultClip;
		}

		PlayFrom(currentClip, clipStartTime);
	}

	/// <summary>
	/// Play the specified clip, starting "clipStartTime" seconds into the clip.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='name'> Name of clip. Try to cache the animation clip Id and use that instead for performance. </param>
	/// <param name='clipStartTime'> Clip start time in seconds. </param>
	public void PlayFrom(string name, float clipStartTime)
	{
		tk2dSpriteAnimationClip clip = library ? library.GetClipByName(name) : null;
		if (clip == null) {
			ClipNameError(name);
		}
		else {
			PlayFrom(clip, clipStartTime);
		}
	}

	/// <summary>
	/// Play the clip specified by identifier.
	/// Will restart the clip at clipStartTime if called while the clip is playing.
	/// </summary>
	/// <param name='clip'>The clip to play. </param>	
	/// <param name='clipStartTime'> Clip start time in seconds. A value of 0 will start the clip from the beginning </param>
	public void PlayFrom(tk2dSpriteAnimationClip clip, float clipStartTime)
	{
		Play(clip, clipStartTime, DefaultFps);
	}
#endregion

	/// <summary>
	/// Play the clip specified by identifier.
	/// Will not restart the clip if called while it is already playing
	/// unless clipStartTime is set.
	/// </summary>
	/// <param name='clip'>The clip to play. </param>	
	/// <param name='clipStartTime'> Clip start time in seconds. A value of 0 will start the clip from the beginning.</param>
	/// <param name='overrideFps'> Overriden framerate of clip. Set to DefaultFps to use default.</param>
	public void Play(tk2dSpriteAnimationClip clip, float clipStartTime, float overrideFps)
	{
		if (clip != null)
		{
			float fps = (overrideFps > 0.0f) ? overrideFps : clip.fps;
			bool isAlreadyPlayingClip = (clipStartTime == 0) && IsPlaying(clip);

			if (isAlreadyPlayingClip) {
				// Update fps if it has changed
				clipFps = fps;
			}
			else {
				state |= State.Playing;
				currentClip = clip;
				clipFps = fps;

				// Simply swap, no animation is played
				if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Single || currentClip.frames == null)
				{
					WarpClipToLocalTime(currentClip, 0.0f);
					state &= ~State.Playing;
				}
				else if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame || currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomLoop)
				{
					int rnd = Random.Range(0, currentClip.frames.Length);
					WarpClipToLocalTime(currentClip, rnd);

					if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.RandomFrame)
					{
						previousFrame = -1;
						state &= ~State.Playing;
					}
				}
				else
				{
					// clipStartTime is in seconds
					// clipTime is in clip local time (ignoring fps)
					float time = clipStartTime * clipFps;
					if (currentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once && time >= clipFps * currentClip.frames.Length)
					{
						// warp to last frame
						WarpClipToLocalTime(currentClip, currentClip.frames.Length - 1);
						state &= ~State.Playing;
					}
					else
					{
						WarpClipToLocalTime(currentClip, time);
						
						// force to the last frame
						clipTime = time;
					}
				}
			}
		}
		else
		{
			Debug.LogError("Calling clip.Play() with a null clip");
			OnAnimationCompleted();
			state &= ~State.Playing;
		}
	}
#endregion

	/// <summary>
	/// Stop the currently playing clip.
	/// </summary>
	public void Stop()
	{
		state &= ~State.Playing;
	}
	
	/// <summary>
	/// Stops the currently playing animation and reset to the first frame in the animation
	/// </summary>
	public void StopAndResetFrame()
	{
		if (currentClip != null)
		{
			SetSprite(currentClip.frames[0].spriteCollection, currentClip.frames[0].spriteId);
		}
		Stop();
	}
	
	/// <summary>
	/// Is the named clip currently active & playing?
	/// </summary>
	public bool IsPlaying(string name)
	{
		return Playing && CurrentClip != null && CurrentClip.name == name;
	}

	/// <summary>
	/// Is this clip currently active & playing?
	/// </summary>
	public bool IsPlaying(tk2dSpriteAnimationClip clip)
	{
		return Playing && CurrentClip != null && CurrentClip == clip;
	}

	/// <summary>
	/// Is a clip currently playing? 
	/// Will return true if the clip is playing, but is paused.
	/// </summary>
	public bool Playing
	{ 
		get { return (state & State.Playing) != 0; }
	}

	/// <summary>
	/// The currently active or playing <see cref="tk2dSpriteAnimationClip"/>
	/// </summary>
	public tk2dSpriteAnimationClip CurrentClip
	{
		get { return currentClip; }
	}
	
	/// <summary>
	/// The current clip time in seconds
	/// </summary>
	public float ClipTimeSeconds
	{
		get { return (clipFps > 0.0f) ? (clipTime / clipFps) : (clipTime / currentClip.fps); }
	}
	
	/// <summary>
	/// Current frame rate of the playing clip. May have been overriden by the user.
	/// Set to 0 to default to the clips fps
	/// </summary>
	public float ClipFps
	{
		get { return clipFps; }
		set 
		{ 
			if (currentClip != null)
			{
				clipFps = (value > 0) ? value : currentClip.fps;
			}
		}
	}

	/// <summary>
	/// Finds a named clip from the current library.
	/// Returns null if not found
	/// </summary>
	public tk2dSpriteAnimationClip GetClipById(int id) {
		if (library == null) {
			return null;
		}
		else {
			return library.GetClipById(id);
		}
	}

	/// <summary>
	/// The default Fps of the clip
	/// </summary>
	public static float DefaultFps { get { return 0; } }
	
	/// <summary>
	/// Resolves an animation clip by name and returns a unique id.
	/// This is a convenient alias to <see cref="tk2dSpriteAnimation.GetClipIdByName"/>
	/// </summary>
	/// <returns>
	/// Unique Animation Clip Id.
	/// </returns>
	/// <param name='name'>Case sensitive clip name, as defined in <see cref="tk2dSpriteAnimationClip"/>. </param>
	public int GetClipIdByName(string name)
	{
		return library ? library.GetClipIdByName(name) : -1;
	}
		/// <summary>
	/// Resolves an animation clip by name and returns a reference to it.
	/// This is a convenient alias to <see cref="tk2dSpriteAnimation.GetClipByName"/>
	/// </summary>
	/// <returns>
	/// tk2dSpriteAnimationClip reference, null if not found
	/// </returns>
	/// <param name='name'>Case sensitive clip name, as defined in <see cref="tk2dSpriteAnimationClip"/>. </param>
	public tk2dSpriteAnimationClip GetClipByName(string name)
	{
		return library ? library.GetClipByName(name) : null;
	}
	
	/// <summary>
	/// Pause the currently playing clip. Will do nothing if the clip is currently paused.
	/// </summary>
	public void Pause()
	{
		state |= State.Paused;
	}
	
	/// <summary>
	/// Resume the currently paused clip. Will do nothing if the clip hasn't been paused.
	/// </summary>
	public void Resume()
	{
		state &= ~State.Paused;
	}
	
	/// <summary>
	/// Sets the current frame. The animation will wrap if the selected frame exceeds the 
	/// number of frames in the clip.
	/// This variant WILL trigger an event if the current frame has a trigger defined.
	/// </summary>
	public void SetFrame(int currFrame)
	{
		SetFrame(currFrame, true);
	}

	/// <summary>
	/// Sets the current frame. The animation will wrap if the selected frame exceeds the 
	/// number of frames in the clip.
	/// </summary>
	public void SetFrame(int currFrame, bool triggerEvent)
	{
		if (currentClip == null) {
			currentClip = DefaultClip;
		}

		if (currentClip != null) {
			int frame = currFrame % currentClip.frames.Length;
			SetFrameInternal(frame);

			if (triggerEvent && currentClip.frames.Length > 0 && currFrame >= 0) {
				ProcessEvents(frame - 1, frame, 1);
			}
		}
	}

	/// <summary>
	/// Returns the current frame of the animation
	/// This is a zero based index into CurrentClip.frames
	/// </summary>
	public int CurrentFrame {
		get {
			switch (currentClip.wrapMode) {
				case tk2dSpriteAnimationClip.WrapMode.Once:
					return Mathf.Min((int)clipTime, currentClip.frames.Length);

				case tk2dSpriteAnimationClip.WrapMode.Loop:
				case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
					return (int)clipTime % currentClip.frames.Length;

				case tk2dSpriteAnimationClip.WrapMode.LoopSection: {
					int currFrame = (int)clipTime;
					int currFrameLooped = currentClip.loopStart + ((currFrame - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart));
					if (currFrame >= currentClip.loopStart) return currFrameLooped;
					else return currFrame;
				}
				
				case tk2dSpriteAnimationClip.WrapMode.PingPong: {
					int currFrame = (int)clipTime % (currentClip.frames.Length + currentClip.frames.Length - 2);
					if (currFrame >= currentClip.frames.Length) {
						currFrame = 2 * currentClip.frames.Length - 2 - currFrame;
					}
					return currFrame;
				}

				default: {
					Debug.LogError("Unhandled clip wrap mode");
					goto case tk2dSpriteAnimationClip.WrapMode.Loop;
				}
			}
		}
	}

	/// <summary>
	/// Steps the animation based on the given deltaTime
	/// Disable tk2dSpriteAnimator, and then call UpdateAnimation manually to feed your own time
	/// eg. when you need an animation to play when the game is paused using Time.timeScale.
	/// </summary>
	public void UpdateAnimation(float deltaTime)
	{
		// Only process when clip is playing
		var localState = state | globalState;
		if (localState != State.Playing)
			return;

		// Current clip should not be null at this point
		clipTime += deltaTime * clipFps;
		int _previousFrame = previousFrame;
		
		switch (currentClip.wrapMode)
		{
			case tk2dSpriteAnimationClip.WrapMode.Loop: 
			case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
			{
				int currFrame = (int)clipTime % currentClip.frames.Length;
				SetFrameInternal(currFrame);
				if (currFrame < _previousFrame) // wrap around
				{
					ProcessEvents(_previousFrame, currentClip.frames.Length - 1, 1); // up to end of clip
					ProcessEvents(-1, currFrame, 1); // process up to current frame
				}
				else
				{
					ProcessEvents(_previousFrame, currFrame, 1);
				}
				break;
			}

			case tk2dSpriteAnimationClip.WrapMode.LoopSection:
			{
				int currFrame = (int)clipTime;
				int currFrameLooped = currentClip.loopStart + ((currFrame - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart));
				if (currFrame >= currentClip.loopStart)
				{
					SetFrameInternal(currFrameLooped);
					currFrame = currFrameLooped;
					if (_previousFrame < currentClip.loopStart)
					{
						ProcessEvents(_previousFrame, currentClip.loopStart - 1, 1); // processed up to loop-start
						ProcessEvents(currentClip.loopStart - 1, currFrame, 1); // to current frame, doesn't cope if already looped once
					}
					else 
					{
						if (currFrame < _previousFrame)
						{
							ProcessEvents(_previousFrame, currentClip.frames.Length - 1, 1); // up to end of clip
							ProcessEvents(currentClip.loopStart - 1, currFrame, 1); // up to current frame
						}
						else
						{
							ProcessEvents(_previousFrame, currFrame, 1); // this doesn't cope with multi loops within one frame
						}
					}
				}
				else
				{
					SetFrameInternal(currFrame);
					ProcessEvents(_previousFrame, currFrame, 1);
				}
				break;
			}

			case tk2dSpriteAnimationClip.WrapMode.PingPong:
			{
				int currFrame = (int)clipTime % (currentClip.frames.Length + currentClip.frames.Length - 2);
				int dir = 1;
				if (currFrame >= currentClip.frames.Length)
				{
					currFrame = 2 * currentClip.frames.Length - 2 - currFrame;
					dir = -1;
				}
				// This is likely to be buggy - this needs to be rewritten storing prevClipTime and comparing that rather than previousFrame
				// as its impossible to detect direction with this, when running at frame speeds where a transition occurs within a frame
				if (currFrame < _previousFrame) dir = -1;
				SetFrameInternal(currFrame);
				ProcessEvents(_previousFrame, currFrame, dir);
				break;
			}		

			case tk2dSpriteAnimationClip.WrapMode.Once:
			{
				int currFrame = (int)clipTime;
				if (currFrame >= currentClip.frames.Length)
				{
					SetFrameInternal(currentClip.frames.Length - 1); // set to last frame
					state &= ~State.Playing; // stop playing before calling event - the event could start a new animation playing here
					ProcessEvents(_previousFrame, currentClip.frames.Length - 1, 1);
					OnAnimationCompleted();
				}
				else
				{
					SetFrameInternal(currFrame);
					ProcessEvents(_previousFrame, currFrame, 1);
				}
				break;
			}
		}
	}

	// Error helpers
	void ClipNameError(string name) {
		Debug.LogError("Unable to find clip named '" + name + "' in library");
	}

	void ClipIdError(int id) {
		Debug.LogError("Play - Invalid clip id '" + id.ToString() + "' in library");
	}

	// Warps the current active frame to the local time (i.e. float frame number) specified. 
	// Ensure that time doesn't exceed the number of frames. Will warp silently otherwise
	void WarpClipToLocalTime(tk2dSpriteAnimationClip clip, float time)
	{
		clipTime = time;
		int frameId = (int)clipTime % clip.frames.Length;
		tk2dSpriteAnimationFrame frame = clip.frames[frameId];
		
		SetSprite(frame.spriteCollection, frame.spriteId);
		if (frame.triggerEvent)
		{
			if (AnimationEventTriggered != null) {
				AnimationEventTriggered(this, clip, frameId);
			}
		}
		previousFrame = frameId;
	}

	void SetFrameInternal(int currFrame)
	{
		if (previousFrame != currFrame)
		{
			SetSprite( currentClip.frames[currFrame].spriteCollection, currentClip.frames[currFrame].spriteId );
			previousFrame = currFrame;
		}
	}
	
	void ProcessEvents(int start, int last, int direction)
	{
		if (AnimationEventTriggered == null || start == last) 
			return;
		int end = last + direction;
		var frames = currentClip.frames;
		for (int frame = start + direction; frame != end; frame += direction)
		{
			if (frames[frame].triggerEvent && AnimationEventTriggered != null) {
				AnimationEventTriggered(this, currentClip, frame);
			}
		}
	}
	
	void OnAnimationCompleted()
	{
		previousFrame = -1;
		if (AnimationCompleted != null) {
			AnimationCompleted(this, currentClip);
		}
	}
	
	public virtual void LateUpdate() 
	{
		UpdateAnimation(Time.deltaTime);
	}

	public virtual void SetSprite(tk2dSpriteCollectionData spriteCollection, int spriteId) {
		Sprite.SetSprite(spriteCollection, spriteId);
	}

#if UNITY_EDITOR
	public float EditorClipTime
	{
		get 
		{
			switch (currentClip.wrapMode)
			{
				case tk2dSpriteAnimationClip.WrapMode.Once:
					return Mathf.Min(clipTime, currentClip.frames.Length);
				case tk2dSpriteAnimationClip.WrapMode.Loop:
				case tk2dSpriteAnimationClip.WrapMode.RandomLoop:
					return clipTime % currentClip.frames.Length;
				case tk2dSpriteAnimationClip.WrapMode.LoopSection:
				{
					float currFrame = clipTime;
					float currFrameLooped = currentClip.loopStart + ((currFrame - currentClip.loopStart) % (currentClip.frames.Length - currentClip.loopStart));
					if (currFrame >= currentClip.loopStart) return currFrameLooped;
					else return currFrame;
				}
				case tk2dSpriteAnimationClip.WrapMode.PingPong:
				{
					int t = currentClip.frames.Length * 2 - 2;
					float f = ((clipTime - 0.5f) % t);
					f = (f > t * 0.5f) ? (t - f) : f;
					return f + 0.5f;
				}
			}
			return clipTime % currentClip.frames.Length;
		}
	}
#endif
}
