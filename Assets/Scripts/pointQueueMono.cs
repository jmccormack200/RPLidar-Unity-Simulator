using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class pointQueueMono : MonoBehaviour {
	Queue queue = new Queue();
	public int scale = 10;
	
	// Use this for initialization
	// Update is called once per frame
	
	void Update () {
		if (queue.Count > 0) {
			LidarPoint lidarpoint = (LidarPoint)queue.Dequeue ();
			parseData (lidarpoint);
		}
	}

	private void parseData (LidarPoint lidarpoint){
		int angle = lidarpoint.X;
		int length = lidarpoint.Y;
		
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
		
		float distance = Vector3.Distance (transform.position, locationVector3);
		if (distance >= 50){
			transform.position = locationVector3;
		}
	}
}
