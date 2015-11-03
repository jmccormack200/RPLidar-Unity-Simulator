/*
 * Adapted from http://forum.unity3d.com/threads/simple-udp-implementation-send-read-via-mono-c.15900/
 * 
 */


using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public struct LidarPoint{
	public int Id;
	public int X;
	public int Y;

	public LidarPoint(string id, string x, string y){
		Id = int.Parse (id);
		X  = int.Parse (x);
		Y  = int.Parse (y);
	}
}


public class UDPTest : MonoBehaviour {

	Thread receiveThread;
	Thread sendThread;

	UdpClient client;
	IPEndPoint remoteEndPoint;

	public int port;
	public float scale = 50.0f;
	public GameObject pointCloud;
	private string lastReceivedUDPPackets="";
	public string allReceivedUDPPackets = "";
	private Queue queue = new Queue();
	private Queue send_queue = new Queue();

	//Used for bad data points
	// Where the distance = 0
	public int badDataPoint = 0;
	public int totalPoints = 0;

	//Track latency
	public float lastTime = 0;
	public float currentTime = 0;

	//check accuracy
	public int childrenSweep = 0;
	public int numberofChildren = 0;

	public string arduinoIP = "192.168.0.111";
	public int arduinoport = 2390;


	//Dictionary for storing the name/gameobject pairs
	//private Dictionary<string, GameObject> pointDictionary = new Dictionary<string, GameObject>();
	public GameObject[] pointArray = new GameObject[370];

	private static void Main(){
		UDPTest receiveObj = new UDPTest ();
		receiveObj.init ();

		string text = "";
		do {
			text = Console.ReadLine ();
		}
		while(!text.Equals ("exit"));
	}

	// Use this for initialization
	public void Start () {
		init ();
		//Coroutine (Loop());
		//InvokeRepeating ("Loop", 0, 0.00002f);
		InvokeRepeating ("Loop", 0, .002f);
	}

	void OnGUI(){
			Rect rectObj = new Rect (40, 10, 200, 400);
			GUIStyle style = new GUIStyle ();
			style.alignment = TextAnchor.UpperLeft;
			GUI.Box(rectObj,"# UDPReceive\n127.0.0.1 "+port+" #\n"
			        + "shell> nc -u 127.0.0.1 : "+port+" \n"
			        + "\n\nUpdate Time: \n" + currentTime.ToString()
		        	+ "\n Number of Bad Data Points \n" + badDataPoint.ToString()
		        	+ "\n Total Number of Packets \n" + totalPoints.ToString()
		        	+ "\n Total Number of Angles \n" + numberofChildren.ToString()
		        	+ "\n Size of Queue : \n " + queue.Count.ToString()
			        ,style);
	}

	private void init(){
		lastTime = Time.time;
		print ("UDPSend.init()");
		port = 8051;
		print ("Sending to 127.0.0.1 : " + port);
		print ("Test-Sending to this Port: nc -u 127.0.0.1 " + port + "");

		addPoints();

		remoteEndPoint = new IPEndPoint(IPAddress.Parse(arduinoIP), arduinoport);

		receiveThread = new Thread (
			new ThreadStart (ReceiveData));
		//receiveThread.IsBackground = true;
		receiveThread.Start ();
		sendThread = new Thread(
			new ThreadStart (SendData));
		sendThread.IsBackground = true;
		sendThread.Start();
	}

	private void addPoints(){
		for(int i = 0; i <= 360; i++){
			GameObject pointInstance = (GameObject)Instantiate (pointCloud, Vector3.zero, Quaternion.identity);
			pointInstance.transform.parent = GameObject.Find("LIDAR").transform;
			pointInstance.name = i.ToString();
			pointArray[i] = pointInstance;
		}
	}


//Threaded portion for recieving and preprocessing the data. 
	private void ReceiveData(){
		client = new UdpClient (port);
		while (true) {
			try {
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = client.Receive (ref anyIP);
				string text = Encoding.UTF8.GetString (data);
				LidarPoint lidarpoint = convertData(text);
				queue.Enqueue(lidarpoint);
			}
			catch (Exception err){
				print (err.ToString ());
			}
		}
	}

	private void SendData(){
		while(true){
			try {
				if (send_queue.Count > 0){
					string length = send_queue.Dequeue().ToString();
					byte[] data = Encoding.UTF8.GetBytes(length);
					client.Send(data, data.Length, remoteEndPoint);
				}
			} catch (Exception err){
				print (err.ToString ());
			}
		}
	}

	private static LidarPoint convertData(string data){
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

		return new LidarPoint (idString, xFinal, yFinal);

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

		//string name = (string)lidarpoint.Id.ToString () + lidarpoint.X.ToString ();

		/*
		if (length < 700 && length > 30){
			send_queue.Enqueue(length.ToString());
		}
		*/

		numberofChildren = transform.childCount;
		totalPoints += 1;
		childrenSweep += 1;
		if (length == 0){
			badDataPoint += 1;
		}
		if (childrenSweep >= numberofChildren){
			currentTime = Time.time - lastTime;
			childrenSweep = 0;
			lastTime = Time.time;

		} 

		GameObject pointInstance = pointArray[angle];
		float normal = pointInstance.GetComponent<pointNormal>().getNormal();
		if (normal == 0.0f){
			pointInstance.transform.position = locationVector3;
			pointInstance.GetComponent<Renderer>().enabled = true;
			pointInstance.GetComponent<Collider>().enabled = true;
		} else if((length / scale >= normal * .90) && (length / scale <= normal * 1.10)){
			pointInstance.GetComponent<Renderer>().enabled = false;
			pointInstance.GetComponent<Collider>().enabled = false;
		} else {
			pointInstance.transform.position = locationVector3;
			pointInstance.GetComponent<Renderer>().enabled = true;
			pointInstance.GetComponent<Collider>().enabled = true;
		}
	}
	
	public string getLatestUDPPacket(){
			allReceivedUDPPackets = "";
			return lastReceivedUDPPackets;
	}
	//On each frame, read from the Lidar Queue 
	//Then if there is data call parseData

	void Loop (){

		if (queue.Count >= 500) {
			for (int i = 0; i < 360; i++){
				if (queue.Count > 0) {
					try {
						LidarPoint lidarpoint = (LidarPoint)queue.Dequeue ();
						parseData (lidarpoint);
					} catch {
						break;
					}
				} 
			}
		} 
	}
	

	void Update(){
		//queue.Clear ();
	}

	void OnAplicationQuit()
	{
		if (receiveThread.IsAlive) {
			receiveThread.Abort(); 
		}
		if (sendThread.IsAlive){
			sendThread.Abort();
		}
		client.Close(); 
	}	
}
