using UnityEngine;
using System.Collections;

/// <summary>
/// Sprite collection size.
/// Supports different methods of specifying size.
/// </summary>
[System.Serializable]
public class tk2dSpriteCollectionSize
{
	/// <summary>
	/// When you are using an ortho camera. Use this to create sprites which will be pixel perfect
	/// automatically at the resolution and orthoSize given.
	/// </summary>
	public static tk2dSpriteCollectionSize Explicit(float orthoSize, float targetHeight) { 
		return ForResolution(orthoSize, targetHeight, targetHeight);
	}

	/// <summary>
	/// When you are using an ortho camera. Use this to create sprites which will be pixel perfect
	/// automatically at the resolution and orthoSize given.
	/// </summary>
	public static tk2dSpriteCollectionSize PixelsPerMeter(float pixelsPerMeter) { 
		tk2dSpriteCollectionSize s = new tk2dSpriteCollectionSize();
		s.type = Type.PixelsPerMeter;
		s.pixelsPerMeter = pixelsPerMeter;
		return s;
	}

	/// <summary>
	/// When you are using an ortho camera. Use this to create sprites which will be pixel perfect
	/// automatically at the resolution and orthoSize given.
	/// </summary>
	public static tk2dSpriteCollectionSize ForResolution(float orthoSize, float width, float height) { 
		tk2dSpriteCollectionSize s = new tk2dSpriteCollectionSize();
		s.type = Type.Explicit;
		s.orthoSize = orthoSize;
		s.width = width;
		s.height = height;
		return s;
	}

	/// <summary>
	/// Use when you need the sprite to be pixel perfect on a tk2dCamera.
	/// </summary>
	public static tk2dSpriteCollectionSize ForTk2dCamera() { 
		tk2dSpriteCollectionSize s = new tk2dSpriteCollectionSize();
		s.type = Type.PixelsPerMeter;
		s.pixelsPerMeter = 1;
		return s;
	}

	/// <summary>
	/// Use when you need the sprite to be pixel perfect on a specific tk2dCamera.
	/// </summary>
	public static tk2dSpriteCollectionSize ForTk2dCamera( tk2dCamera camera ) { 
		tk2dSpriteCollectionSize s = new tk2dSpriteCollectionSize();
		tk2dCameraSettings cameraSettings = camera.SettingsRoot.CameraSettings;
		if (cameraSettings.projection == tk2dCameraSettings.ProjectionType.Orthographic) {
			switch (cameraSettings.orthographicType) {
				case tk2dCameraSettings.OrthographicType.PixelsPerMeter:
					s.type = Type.PixelsPerMeter;
					s.pixelsPerMeter = cameraSettings.orthographicPixelsPerMeter;
					break;
				case tk2dCameraSettings.OrthographicType.OrthographicSize:
					s.type = Type.Explicit;
					s.height = camera.nativeResolutionHeight;
					s.orthoSize = cameraSettings.orthographicSize;
					break;
			}
		}
		else if (cameraSettings.projection == tk2dCameraSettings.ProjectionType.Perspective) {
			s.type = Type.PixelsPerMeter;
			s.pixelsPerMeter = 20; // some random value
		}
		return s;
	}

	/// <summary>
	/// A default setting
	/// </summary>
	public static tk2dSpriteCollectionSize Default() {
		return PixelsPerMeter(20);
	}

	// Copy from legacy settings
	public void CopyFromLegacy( bool useTk2dCamera, float orthoSize, float targetHeight ) {
		if (useTk2dCamera) {
			this.type = Type.PixelsPerMeter;
			this.pixelsPerMeter = 1;
		}
		else {
			this.type = Type.Explicit;
			this.height = targetHeight;
			this.orthoSize = orthoSize;
		}
	}

	public void CopyFrom(tk2dSpriteCollectionSize source) {
		this.type = source.type;
		this.width = source.width;
		this.height = source.height;
		this.orthoSize = source.orthoSize;
		this.pixelsPerMeter = source.pixelsPerMeter;
	}
	
	public enum Type {
		Explicit,
		PixelsPerMeter
	};

	// What to do with the values below?
	public Type type = Type.PixelsPerMeter;

	// resolution, used to derive above values
	public float orthoSize = 10;
	public float pixelsPerMeter = 20;
	public float width = 960;
	public float height = 640;

	public float OrthoSize {
		get {
			switch (type) {
				case Type.Explicit: return orthoSize;
				case Type.PixelsPerMeter: return 0.5f;
			}
			return orthoSize;
		}
	}

	public float TargetHeight {
		get {
			switch (type) {
				case Type.Explicit: return height;
				case Type.PixelsPerMeter: return pixelsPerMeter;
			}
			return height;
		}
	}
}
