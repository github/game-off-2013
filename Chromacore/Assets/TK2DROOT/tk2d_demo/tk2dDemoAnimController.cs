using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Demo/tk2dDemoAnimController")]
public class tk2dDemoAnimController : MonoBehaviour 
{
	tk2dSpriteAnimator animator;
	public tk2dTextMesh popupTextMesh;
	
	// Use this for initialization
	void Start () 
	{
		animator = GetComponent<tk2dSpriteAnimator>();
		animator.AnimationEventTriggered += AnimationEventHandler;
		
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		popupTextMesh.gameObject.active = false;
#else
		popupTextMesh.gameObject.SetActive(false);
#endif
	}
	
	void AnimationEventHandler(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNum)
	{
		string str = animator.name + "\n" + clip.name + "\n" + "INFO: " + clip.GetFrame(frameNum).eventInfo;
		StartCoroutine( PopupText( str ) );
	}
	
	IEnumerator PopupText(string text)
	{
		popupTextMesh.text = text;
		popupTextMesh.Commit();
		
#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		popupTextMesh.gameObject.active = true;
#else
		popupTextMesh.gameObject.SetActive(true);
#endif
		
		float fadeTime = 1.0f;
		Color c1 = popupTextMesh.color, c2 = popupTextMesh.color2;
		for (float f = 0.0f; f < fadeTime; f += Time.deltaTime)
		{
			float alpha = Mathf.Clamp01( 2.0f * (1.0f - f / fadeTime) );
			c1.a = alpha;
			c2.a = alpha;
			popupTextMesh.color = c1;
			popupTextMesh.color2 = c2;
			popupTextMesh.Commit();
			yield return 0;
		}

#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
		popupTextMesh.gameObject.active = false;
#else
		popupTextMesh.gameObject.SetActive(false);
#endif
	}

	void OnGUI()
	{
		GUILayout.BeginVertical();
		
		GUILayout.Label("Animation wrap modes");
		
		
		if (GUILayout.Button("Loop", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_loop");			
		}
		GUILayout.Label("  This animation will play indefinitely");
		

		if (GUILayout.Button("LoopSection", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_loopsection");
		}
		GUILayout.Label("  This animation has been set up to loop from frame 3." + "\nIt will play 0 1 2 3 4 2 3 4 2 3 4 indefinitely");

		
		if (GUILayout.Button("Once", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_once");			
		}
		GUILayout.Label("  This animation will play once and stop at the last frame");
		

		if (GUILayout.Button("Ping pong", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_pingpong");			
		}
		GUILayout.Label("  This animation will play once forward, and then reverse, repeating indefinitely");
		
		
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Single", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_single1");			
		}
		if (GUILayout.Button("Single", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_single2");			
		}
		GUILayout.EndHorizontal();
		GUILayout.Label("  Use this for non-animated states and placeholders.");
		

		GUILayout.Space(20.0f);
		GUILayout.Label("Animation delegate example");
		
		if (GUILayout.Button("Delegate", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_once");
			animator.AnimationCompleted = delegate(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip) 
				{ 
					Debug.Log("Delegate");
					animator.Play("demo_pingpong"); 
					animator.AnimationCompleted = null;
				};
		}
		GUILayout.Label("Play demo_once, then immediately play demo_pingpong after that");
		
		if (GUILayout.Button("Message", GUILayout.MaxWidth(100)))
		{
			animator.Play("demo_message");
		}
		GUILayout.Label("Plays demo_message once, will trigger an event when frame 3 is hit.\nLook at how this animation is set up.");
		
		GUILayout.EndVertical();
	}
}
