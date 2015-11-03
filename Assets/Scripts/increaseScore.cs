using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class increaseScore : MonoBehaviour {

	private Text countText;
	private int count = 0;

	// Use this for initialization
	void Start () {
		countText = GetComponent<Text>();
		countText.text = "Score: " + count.ToString();
	}
	
	// Update is called once per frame
	public void UpdateCount () {
		count += 1;
		countText.text = "Score: " + count.ToString();
	}
}
