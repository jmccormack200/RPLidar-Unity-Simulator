using UnityEngine;
using System; //Needed for Exceptions
using System.Collections;
//using System.Threading; //Used for Thread
using System.Net; //Used for IPEndPoint
using System.Net.Sockets; //Used for UDPClient
using System.Text; //Used for Encoding

public class LidarReciever : MonoBehaviour {

    //Related to scene objects
    private GameObject[] pointObjects = new GameObject[370];
    public GameObject pointObjectPrefab;

    //related to client server connection
    private UdpClient client;
    private  IPEndPoint remoteEndPoint;
    public int port = 8051;
    public string ip = "127.0.0.1";

    //related to requesting data
    public string data = " ";
    public string requestString = " ";
    private byte[] requestBytes;

    //flags
    public bool okToRecieve = false;
    public float scale = 1;
    
    // Use this for initialization
    void Start () {
        //sets up socket client
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        client = new UdpClient();
        client.Connect(remoteEndPoint);
        okToRecieve = true;

        //translate request string to bytes
        requestBytes = Encoding.UTF8.GetBytes(requestString);
        
    }
	
	// Update is called once per frame
	void Update () {
        
        //if ready, makes request for data
        if (okToRecieve)
        {
            okToRecieve = false;
            client.Send(requestBytes, requestBytes.Length);
            StartCoroutine(WaitForData());//waits to recieve an then proceed with data manipultaion

        }
        
	}
    
    //client.Recieve will block until recieves data
    IEnumerator WaitForData() {
        data = client.Receive(ref remoteEndPoint).ToString();
        yield return StartCoroutine(ParseData(data));
    }
    //parse data
    IEnumerator ParseData(string data) {
        Queue lidarQueue = new Queue();
        LidarPoint newPoint;


        //translate r,0;r,0;...|metadata into pairs into LidarPoint Struct
        string[] dataSplit = data.Split('|');//[LidarData(r,0;r,0;...),metadata]
        string metaData = dataSplit[1];
        Console.Write("Meta Data: " + metaData);

        string[] dataBunch = dataSplit[0].Split(';');//["r,0","r,0"....]

        string[] dataPair;
        int id = 0;//for id

        //process pairs
        foreach (string s in dataBunch)
        {
            dataPair = s.Split(',');//[distance,angle]
            newPoint = new LidarPoint(id + " :: " + metaData, dataPair[0], dataPair[1]);
            lidarQueue.Enqueue(newPoint); // puts LidarPoint on the queue
            id++;
        }
        yield return DrawPoints(lidarQueue);//draw points from queue
    }
    //draw lidar point objects
    IEnumerator DrawPoints(Queue lidarQueue)
    {

        while (lidarQueue.Count > 0)
        {
            LidarPoint lidarPoint = (LidarPoint)lidarQueue.Dequeue();
            int angle = lidarPoint.mAngle;
            int length = lidarPoint.mDistance;

            //float angle = N ["angle"].AsFloat;
            //float length = N ["length"].AsFloat;

            float radians = angle * (Mathf.PI / 180);
            float x = length * Mathf.Sin(radians);
            float y = length * Mathf.Cos(radians);
            float z = 0.0f;


            float new_x = transform.position.x + x;
            new_x = new_x / scale;
            float new_y = transform.position.y + y;
            new_y = new_y / scale;
            float new_z = transform.position.z + z;

            Vector3 locationVector3 = new Vector3(new_x, new_y, new_z);
            if (pointObjects[angle]) { 

                float distance = Vector3.Distance(pointObjects[angle].transform.position, locationVector3);
                if (distance >= 50)
                {
                    pointObjects[angle].transform.position = locationVector3;
                }

            }
            else
            {
                Instantiate(pointObjectPrefab, locationVector3, transform.rotation);
            }
        }

        okToRecieve = true;
        yield return 0;
    }
}
