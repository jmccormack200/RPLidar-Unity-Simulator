using UnityEngine;
using System.Collections;
using System;
using Npgsql;


public class buttonPress : MonoBehaviour {

	public NpgsqlConnection conn;
	public GameObject LIDAR;
	public GameObject collectable;
	public GameObject[] pointArray = new GameObject[370];
	public int LidarRunNumber = 0;

	// Use this for initialization
	public void press () {
		pointArray = LIDAR.GetComponent<UDPTest>().pointArray;
		PostGresUtility();

		foreach(GameObject point in pointArray){
			try{
				float length = point.transform.position.magnitude;
				point.GetComponent<pointNormal>().saveNormal(length);
				float old_length = point.GetComponent<pointNormal>().getLastNormal();
				float delta = point.GetComponent<pointNormal>().getDelta();
				try{
					NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO points VALUES(" +
					                                      "DEFAULT, @lidarrun, @degree, @normal, @oldnormal, @delta)", conn);
					cmd.Parameters.Add (new NpgsqlParameter("@lidarrun", LidarRunNumber));
					int degree = int.Parse(point.name);
					cmd.Parameters.Add (new NpgsqlParameter("@degree", degree));
					cmd.Parameters.Add (new NpgsqlParameter("@normal", length));
					cmd.Parameters.Add (new NpgsqlParameter("@oldnormal", old_length));
					cmd.Parameters.Add (new NpgsqlParameter("@delta", delta));
					cmd.ExecuteNonQuery();
				} catch(Exception e) {
					//print(e.ToString ());
				}
			} catch {

			}
		}
		//Generate 10 random point
		for(int i = 0; i < 10; i++){
			int randAngle = UnityEngine.Random.Range (0, 360);
			float magnitude = pointArray[randAngle].transform.position.magnitude;
			if (magnitude == 0){
				i--;
			} else {
				float randomMagnitude = UnityEngine.Random.Range (0.5f, (magnitude * 0.75f));

				float radians = randAngle * (Mathf.PI / 180);
				float x = randomMagnitude * Mathf.Sin (radians);
				float y = randomMagnitude * Mathf.Cos (radians);
				float z = 0.0f;

				Vector3 locationVector3 = new Vector3 (x, y, z);
				GameObject collect = (GameObject)Instantiate (collectable, locationVector3, Quaternion.identity);
			}
		}
	}

	public void PostGresUtility(string server = "127.0.0.1", string port = "5432", string user_id = "postgres", string password = "1234", string database = "postgres"){
		string connectionString = "Server=" + server + ";Port=" + port + ";User Id=" + user_id + ";Password=" + password + ";Database=" + database;
		//conn = new NpgsqlConnection ("Server=localhost;Port=5432;User Id=postgres;Password=password;Database=postgres");
		conn = new NpgsqlConnection (connectionString);
		conn.Open();
      	string sql = "SELECT max(lidarrun) FROM points";
		NpgsqlCommand command = conn.CreateCommand();
		command.CommandText = sql;
		LidarRunNumber = (int) command.ExecuteScalar ();
		LidarRunNumber = LidarRunNumber + 1;
	}
}
