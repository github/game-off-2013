using UnityEngine;
using System.Collections;

/// <summary>
/// Time proxy class, independent to Time.timeScale
/// </summary>
public static class tk2dUITime {

	/// <summary>
	/// Use this in UI classes / when you need deltaTime unaffected by Time.timeScale
	/// </summary>
	public static float deltaTime {
		get {
			return _deltaTime;
		}
	}

	static double lastRealTime = 0;
	static float _deltaTime = 1.0f / 60.0f;

	/// <summary>
	/// Do not call. This is updated by tk2dUIManager
	/// </summary>
	public static void Init() 
	{
		lastRealTime = Time.realtimeSinceStartup;
		_deltaTime = Time.maximumDeltaTime;
	}

	/// <summary>
	/// Do not call. This is updated by tk2dUIManager
	/// </summary>
	public static void Update() 
	{
		float currentTime = Time.realtimeSinceStartup;
		if (Time.timeScale < 0.001f) {
			_deltaTime = Mathf.Min( 2.0f / 30.0f, (float)(currentTime - lastRealTime) );
		}
		else {
			_deltaTime = Time.deltaTime / Time.timeScale;
		}
		lastRealTime = currentTime;
	}
}
