using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using SimpleJSON;



public class fireBaseTest : MonoBehaviour {
	public string url = "https://radiant-torch-9117.firebaseio.com/datapoints.json";
	public Rigidbody pointCloud;
	public int scale = 10;

	// Use this for initialization
	IEnumerator parse () {
		WWW www = new WWW(url);
		yield return www;

		SimpleJSON.JSONNode N = SimpleJSON.JSONNode.Parse (www.text);

		for (int i = 0; i < 10; i++) {
			float angle = N["array"][i][0].AsFloat;
			float length = N["array"][i][1].AsFloat;		

			//float angle = N ["angle"].AsFloat;
			//float length = N ["length"].AsFloat;

			float radians = angle * (Mathf.PI / 180);
			float x = length * Mathf.Sin (radians);
			float y = length * Mathf.Cos (radians);
			float z = 0.0f;


			float new_x = transform.position.x + x;
			new_x = new_x / scale;
			float new_y = transform.position.y + y;
			new_y = new_y / scale;
			float new_z = transform.position.z + z;

			Vector3 locationVector3 = new Vector3 (new_x, new_y, new_z);

			Debug.Log (new_x);
			Debug.Log (new_y);
			Rigidbody pointInstance = (Rigidbody)GameObject.Instantiate (pointCloud, locationVector3, Quaternion.Euler (0, 0, 0));
			Destroy(pointInstance, 10);
			//Debug.Log (angle.ToString());
			//Debug.Log (length.ToString ());
		}
	}
	
	// Update is called once per frame
	void Update () {
		StartCoroutine (parse());
	}
}
