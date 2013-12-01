using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class tk2dUIBaseDemoController : MonoBehaviour {

#region Helpers
	
	class InitTransform {
		public Vector3 pos;
		public Vector3 scale;
		public float angle;
	}
	Dictionary<Transform, InitTransform> registeredWindows = new Dictionary<Transform, InitTransform>();

	protected void RegisterWindow( Transform t ) {
		RemoveUnity3HackFromWindow( t );
		ShowWindow( t );
		InitTransform i = new InitTransform();
		i.pos = t.position;
		i.scale = t.localScale;
		i.angle = t.eulerAngles.z;
		registeredWindows.Add( t, i );
		HideWindow( t );
	}

    protected void AnimateShowWindow(Transform t)
    {
    	if (!registeredWindows.ContainsKey(t)) {
    		RegisterWindow(t);
    	}

    	InitTransform it = registeredWindows[t];

    	ShowWindow(t);
        t.localPosition = new Vector3(-5, 0, 0);
        t.localScale = Vector3.zero;
        t.localEulerAngles = new Vector3(0, 0, 10);
        StartCoroutine( coTweenTransformTo( t, 0.3f, it.pos, it.scale, it.angle ) );
    }

    protected void AnimateHideWindow(Transform t)
    {
    	if (!registeredWindows.ContainsKey(t)) {
    		RegisterWindow(t);
    	}

        StartCoroutine(coAnimateHideWindow(t));
    }

    private IEnumerator coAnimateHideWindow( Transform t ) {
        yield return StartCoroutine( coTweenTransformTo( t, 0.3f, new Vector3(5, 0, 0), Vector3.zero, -10 ) );
        HideWindow(t);
    }

#endregion


#region Simple Tweens

	protected IEnumerator coResizeLayout( tk2dUILayout layout, Vector3 min, Vector3 max, float time ) {
		Vector3 minFrom = layout.GetMinBounds();
		Vector3 maxFrom = layout.GetMaxBounds();
		for (float t = 0; t < time; t += tk2dUITime.deltaTime) {
			float nt = Mathf.SmoothStep(0, 1, Mathf.Clamp01( t / time ));
			Vector3 currMin = Vector3.Lerp( minFrom, min, nt );
			Vector3 currMax = Vector3.Lerp( maxFrom, max, nt );
			layout.SetBounds( currMin, currMax );
			yield return 0;
		}
		layout.SetBounds( min, max );
	}

	protected IEnumerator coTweenAngle( Transform t, float xAngle, float time ) {
		float xStart = t.localEulerAngles.x;
		if (xStart > 0) xStart -= 360;
		for (float ut = 0; ut < time; ut += Time.deltaTime) {
			float nt = Mathf.SmoothStep( 0, 1, Mathf.Clamp01( ut / time ) );
			float a = Mathf.Lerp(xStart, xAngle, nt);
			t.localEulerAngles = new Vector3(a, 0, 0);
			yield return 0;
		}
		t.localEulerAngles = new Vector3(xAngle, 0, 0);
	}

	protected IEnumerator coMove( Transform t, Vector3 targetPosition, float time ) {
		Vector3 startPosition = t.position;
		for (float ut = 0; ut < time; ut += Time.deltaTime) {
			float nt = Mathf.SmoothStep( 0, 1, Mathf.Clamp01( ut / time ) );
			t.position = Vector3.Lerp(startPosition, targetPosition, nt);
			yield return 0;
		}
		t.position = targetPosition;
	}

	protected IEnumerator coShake( Transform t, Vector3 translateConstraint, Vector3 rotationConstraint, float time ) {
		Vector3 pos = t.position;
		Quaternion rot = t.rotation;
		for (float ut = 0; ut < time; ut += Time.deltaTime) {
			float nt = Mathf.Clamp01( ut / time );
			float strength = 1 - nt;

			t.position = pos + Vector3.Scale(Random.onUnitSphere, translateConstraint).normalized * strength * 0.01f;
			t.rotation = rot;
			t.Rotate(Vector3.Scale(Random.onUnitSphere, rotationConstraint), 2.0f * strength);

			yield return 0;
		}
		t.position = pos;
		t.rotation = rot;
	}

    protected IEnumerator coTweenTransformTo( Transform transform, float time, Vector3 toPos, Vector3 toScale, float toRotation) 
    {
        Vector3 fromPos = transform.localPosition;
        Vector3 fromScale = transform.localScale;
        Vector3 euler = transform.localEulerAngles;
        float fromRotation = euler.z;

        for (float t = 0; t < time; t += tk2dUITime.deltaTime) {
            float nt = Mathf.Clamp01( t / time );
            nt = Mathf.Sin(nt * Mathf.PI * 0.5f);

            transform.localPosition = Vector3.Lerp( fromPos, toPos, nt );
            transform.localScale = Vector3.Lerp( fromScale, toScale, nt );
            euler.z = Mathf.Lerp( fromRotation, toRotation, nt );
            transform.localEulerAngles = euler;
            yield return 0;
        }

        euler.z = toRotation;
        transform.localPosition = toPos;
        transform.localScale = toScale;
        transform.localEulerAngles = euler;
    }	

#endregion

#region Window management

	protected void DoSetActive( Transform t, bool state ) {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		t.gameObject.SetActiveRecursively(state);
#else
		t.gameObject.SetActive(state);
#endif
	}

	protected void ShowWindow(Transform t) {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		Vector3 v = t.position;
		v.y = v.y % 1;
		v.x = v.x % 1;
		t.position = v;
#else
		t.gameObject.SetActive( true );
#endif
	}

	protected void HideWindow(Transform t) {
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		Vector3 v = t.position;
		v.y = (v.y % 1) + 100;
		t.position = v;
#else
		t.gameObject.SetActive( false );
#endif
	}

	// We move things away from the screen to "disable" them in Unity 3.x
	// It is horrible, but inevitable because SetActiveRecursively doesn't remember
	// disabled objects.
	// This isn't necessary in Unity 4.x, and we simply move everything back to the correct positions
	// on startup.
	protected void RemoveUnity3HackFromWindow(Transform t) {
#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9)
		Vector3 v = t.position;
		v.y = v.y % 1;
		v.x = v.x % 2;
		t.position = v;
#endif
	}

#endregion
}
