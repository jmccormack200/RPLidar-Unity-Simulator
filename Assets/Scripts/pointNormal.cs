using UnityEngine;
using System.Collections;

public class pointNormal : MonoBehaviour {

	public float normal;
	public float lastNormal;
	public float delta;

	// Use this for initialization
	void Start () {
		normal = 0.0f;
	}
	
	// Update is called once per frame
	public void saveNormal(float set){
		lastNormal = normal;
		normal = set;
		delta = normal - lastNormal;
	}

	public float getNormal(){
		return normal;
	}

	public float getDelta(){
		return delta;
	}

	public float getLastNormal(){
		return lastNormal;
	}
}
