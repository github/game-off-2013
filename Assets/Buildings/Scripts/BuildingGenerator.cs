using UnityEngine;
using System.Collections;

public class BuildingGenerator : MonoBehaviour {

	void Awake () {
		var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		cube.transform.localScale = new Vector3(10.0f, 6.096f, 10.0f);
		cube.transform.localPosition = new Vector3((float) Random.Range(-100,100), 3.048f, (float) Random.Range (-100,100));
		//cube.transform.position = new Vector3(0,0,0);
	}
	
}
