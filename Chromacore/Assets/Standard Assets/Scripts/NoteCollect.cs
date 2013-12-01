using UnityEngine;
using System.Collections;

public class NoteCollect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	
	void OnTriggerEnter(Collider col)
	{
		col.SendMessage("SeeNote");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
