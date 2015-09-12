using UnityEngine;
using System; //Needed for Exceptions
using System.Collections;
using System.Threading; //Used for Thread
using System.Net; //Used for IPEndPoint
using System.Net.Sockets; //Used for UDPClient
using System.Text; //Used for Encoding

public class LaserRange : MonoBehaviour {
	public float max_distance = 7.0f;
	public int id = 1;

	private float angle = 0.0f;
	private float distance = 0.0f;
	private float x = 0.0f;
	private float y = 0.0f;

	//Below Variables are for handling the UDP data
	UdpClient client;
	IPEndPoint remoteEndPoint;
	public int port = 8051;
	public string ip = "127.0.0.1";

	//Below is used to create a separate Thread for Sending UDP Data
	Thread sendThread;
	private Queue send_queue = new Queue();


	// Use this for initialization
	void Start () {
		remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

		sendThread = new Thread(
			new ThreadStart (SendData));
		sendThread.IsBackground = true;
		sendThread.Start();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate(){
		RaycastHit hit;

		Vector3 fwd = transform.TransformDirection (Vector3.forward);
		if (Physics.Raycast (transform.position, fwd, out hit, max_distance)) {
			distance = hit.distance;
		} else {
			distance = 0;
		}
		angle = transform.parent.eulerAngles.y;

		x = distance * Mathf.Cos (angle);
		y = distance * Mathf.Sin (angle);
		string send_string = "(" + id.ToString () + ",(" + x.ToString() + y.ToString () +"))";
		send_queue.Enqueue (send_string);
	}

	//Borrowed below from previous LIDAR implementation
	private void SendData(){
		while(true){
			try {
				if (send_queue.Count > 0){
					string packet = send_queue.Dequeue().ToString();
					print (packet);
					byte[] data = Encoding.UTF8.GetBytes(packet);
					client.Send(data, data.Length, remoteEndPoint);
				}
			} catch (Exception err){
				//print (err.ToString ());
			}
		}
	}

	void onApplicationQuit(){
		sendThread.Abort ();
		if (client != null) {
			client.Close ();
		}
	}
}
