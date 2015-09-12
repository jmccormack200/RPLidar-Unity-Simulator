using UnityEngine;
using System.Collections;

public class RotateLaser : MonoBehaviour {
	public Vector3 rotationMask = new Vector3(0,1,0);
	public float rotationSpeed = 50.0f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.Rotate (new Vector3(0,1,0) * rotationSpeed * Time.deltaTime);
	}
}
