using UnityEngine;
using System.Collections;

public class pointDestroy : MonoBehaviour {
	public float time;
	// Use this for initialization
	void Awake () {
		Destroy (gameObject, time);
	}
}
