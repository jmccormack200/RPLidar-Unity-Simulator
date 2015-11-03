using UnityEngine;
using System.Collections;

public class CubeCollision : MonoBehaviour {

	private increaseScore otherScript;

	public void Start(){
		GetComponent<Renderer>().material.color = Color.red;
	}
	void OnCollisionEnter(Collision collision){
		GetComponent<Renderer>().material.color = Color.blue;
		otherScript = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<increaseScore>();
		otherScript.UpdateCount();
		Destroy(gameObject);

	}
}
