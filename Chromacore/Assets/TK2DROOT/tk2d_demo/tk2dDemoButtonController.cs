using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Demo/tk2dDemoButtonController")]
public class tk2dDemoButtonController : MonoBehaviour 
{
	float spinSpeed = 0.0f;
	
	// update
	void Update() 
	{
		transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
	}
	
	void SpinLeft()
	{
		spinSpeed = 4.0f;
	}
	
	void SpinRight()
	{
		spinSpeed = -4.0f;
	}
	
	void StopSpinning()
	{
		spinSpeed = 0.0f;
	}
}
