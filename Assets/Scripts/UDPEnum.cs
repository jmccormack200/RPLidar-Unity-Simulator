/* This code was found to cause Unity to crash
 * May be worth trying again later, but for now 
 * switching back to using Threads instead of Coroutines


using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;



public struct LidarPoints{
	public int Id;
	public int X;
	public int Y;
	
	public LidarPoints(string id, string x, string y){
		Id = int.Parse (id);
		X = int.Parse (x);
		Y = int.Parse (y);
	}
}

public class UDPEnum : MonoBehaviour {

	UdpClient client;
	
	public int port;
	public int scale = 10;
	public Rigidbody pointCloud;
	
	// Use this for initialization
	public void Start () {
		print ("UDPSend.init()");
		port = 8051;
		print ("Sending to 127.0.0.1 : " + port);
		print ("Test-Sending to this Port: nc -u 127.0.0.1 " + port + "");
	}
	
	private IEnumerable ReceiveData(){
		for (;;) {
			client = new UdpClient (port);

			IPEndPoint anyIP = new IPEndPoint (IPAddress.Any, 0);
			byte[] data = client.Receive (ref anyIP);				
			string text = Encoding.UTF8.GetString (data);
			LidarPoints lidarpoint = convertData (text);

			int angle = lidarpoint.X;
			int length = lidarpoint.Y;
		
			//float angle = N ["angle"].AsFloat;
			//float length = N ["length"].AsFloat;
		
			float radians = angle * (Mathf.PI / 180);
			float x = length * Mathf.Sin (radians);
			float y = length * Mathf.Cos (radians);
			float z = 0.0f;
		
			//transform gets the location of the "cube" root asset
			//This way all lidar points are in relation to the center.
			//Not important now, but will be with more Lidars. 
			float new_x = transform.position.x + x;
			new_x = new_x / scale;
			float new_y = transform.position.y + y;
			new_y = new_y / scale;
			float new_z = transform.position.z + z;
		
			Vector3 locationVector3 = new Vector3 (new_x, new_y, new_z);
		
			Debug.Log (new_x);
			Debug.Log (new_y);
			string message = "That outta do it";
			print (message);

			
			//Rigidbody pointInstance = (Rigidbody)GameObject.Instantiate (pointCloud, locationVector3, Quaternion.Euler (0, 0, 0));
			//queue.Enqueue(lidarpoint);\
		}
	}
	
	private static LidarPoints convertData(string data){
		//ID number [(][0-9]+[,][ ]?[(]
		Match idRawMatch = Regex.Match (data, "[(][0-9]+[,][ ]?[(]");
		string idRawString = (string)idRawMatch.ToString ();
		//Remove paranthesis and comma
		Match idMatch = Regex.Match (idRawString, "[0-9]+");
		string idString = (string)idMatch.ToString ();
		
		//Remove both digits [0-9]+[,][0-9]+ gives 100,100 format
		Match pointRawMatch = Regex.Match (data, "[0-9]+[,][ ]?[0-9]+");
		string pointRawString = (string)pointRawMatch.ToString ();
		//First digit [0-9]+[,] gives 100,
		Match xRawMatch = Regex.Match (pointRawString, "[0-9]+[,]");
		string xRawString = (string)xRawMatch.ToString ();
		//Second digit [,][0-9]+ gives ,100
		Match yRawMatch = Regex.Match (data, "[,][ ]?[0-9]+");
		string yRawString = (string)yRawMatch.ToString ();
		//First digit pure
		Match xFinalMatch = Regex.Match (xRawString, "[0-9]+");
		string xFinal = (string) xFinalMatch.ToString ();
		//Second digit pure
		Match yFinalMatch = Regex.Match (yRawString, "[0-9]+");
		string yFinal = (string)yFinalMatch.ToString ();
		
		return new LidarPoints (idString, xFinal, yFinal);
		
	}
	

	void Update () {
		//StartCoroutine (ReceiveData());
	}
}
*/

