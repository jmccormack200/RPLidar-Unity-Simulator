using UnityEngine;
using System; //Needed for Exceptions
using System.Collections;
using System.Threading; //Used for Thread
using System.Net; //Used for IPEndPoint
using System.Net.Sockets; //Used for UDPClient
using System.Text; //Used for Encoding

public class SimuLIDAR : MonoBehaviour {
	public float max_distance = 7.0f;
	public int id = 1;
    public int dataLimit= 20;
    public int limitFlag = 0;

	//This variable is used to reset the send queue
	Boolean stop_send = false;
	
	private float angle = 0.0f;
	private float distance = 0.0f;
    private string send_string = "";
    
	//Below Variables are for handling the UDP data
	UdpClient client;
	IPEndPoint remoteEndPoint;
	public int port = 8051;
	public string ip = "127.0.0.1";

	//Below is used to handle Sending UDP Data
	private Queue send_queue = new Queue();
    string data = " ";

    // Use this for initialization
    void Start () {
		remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        
        client = new UdpClient();
        client.Connect(remoteEndPoint);

    }

	
	// Update is called once per frame
	void Update () {
        data = client.Receive(ref remoteEndPoint).ToString();
		if (Input.GetKeyDown ("q")) {
			stop_send = true;
			print ("Thread Halted, safe to stop");
		}
        else if( data == "\x02\x00")
        {
            data = " ";
            StartCoroutine (DataQueueing());
        }
	}
    
	IEnumerator DataQueueing(){
        //if we reach our given limit, send the data
        while (limitFlag != dataLimit)
        {
            yield return StartCoroutine(CollectData());
        }
        
        ReadyDataOnQueue();
        SendData();
        yield return 0;
    }

    IEnumerator CollectData()
    {
        RaycastHit hit;

        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(transform.position, fwd, out hit, max_distance))
        {
            distance = hit.distance;
        }
        else {
            distance = 0;
        }
        angle = transform.parent.eulerAngles.y;


        //Data format : may change
        //r,θ;r,θ;r,θ;...;\n
        send_string += distance.ToString() + "," + angle.ToString() + ";";
        limitFlag++;
        yield return 0;
    }
    void ReadyDataOnQueue() {
        limitFlag = 0;
        //send_string += meta_data;  //metadata: id, etc.
        send_string += "| simulation \n"; //line end + metadata
        send_queue.Enqueue(send_string);
        send_string = ""; //reset string

    }


	//Borrowed below from previous LIDAR implementation
	private void SendData(){
		while(stop_send == false){
			try {
				if (send_queue.Count > 0){
					string packet = send_queue.Dequeue().ToString();
					print (packet);
					byte[] data = Encoding.UTF8.GetBytes(packet);
					client.Send(data, data.Length, remoteEndPoint);
				}
			} catch (Exception err){
				print (err.ToString ());
			}
		}
	}

	void onApplicationQuit(){
		print ("QUITTING!");
		client.Close ();
		print ("QUITTING!");
	}

	void OnGUI () {
		String stop_message = "Thread Stopped, Safe To Exit";
		String start_message = "Press Q Before Closing";
		if (stop_send == true) {
			GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), stop_message);
		} else {
			GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), start_message);
		}

	}
}


