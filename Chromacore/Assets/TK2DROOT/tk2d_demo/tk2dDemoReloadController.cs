using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Demo/tk2dDemoReloadController")]
public class tk2dDemoReloadController : MonoBehaviour 
{
	void Reload()
	{
		Application.LoadLevel(Application.loadedLevel);
	}
}
