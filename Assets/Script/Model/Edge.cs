using UnityEngine;
using System.Collections;

public class Edge : MonoBehaviour {

	GameObject top;
	GameObject bot;

	// Use this for initialization
	void Start () {
	
	}

	void OnEnable () {
		WorldManager.OnMoved += RelocateEndNodes;
	}

	void OnDisable () {
		WorldManager.OnMoved -= RelocateEndNodes;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseEnter() {
		transform.FindChild("Cylinder").transform.GetComponent<Renderer> ().material.color = Color.red;
	}

	void OnMouseExit() {
		transform.FindChild("Cylinder").transform.GetComponent<Renderer> ().material.color = Color.gray;
	}

	public void AssignNode ( GameObject tp, GameObject bt)
	{
		top = tp;
		bot = bt;
		ShapeConnector ();
	}

	void RelocateEndNodes(GameObject node) {
		if (node == top || node == bot) {
			ShapeConnector ();
		}
	}

	void ShapeConnector () {
		Vector3 distance = top.transform.position - bot.transform.position;
		Vector3 midPos = distance / 2.0f + bot.transform.position;
		Vector3 scale = transform.localScale;
		transform.position = midPos;
		scale.z = distance.magnitude/2.0f;
		transform.localScale = scale;
		transform.LookAt (bot.transform.position);
	}

	public string ToString() {
		return "\t\t - [" + top.GetComponent<Node>().nodeName + ", " + bot.GetComponent<Node>().nodeName + "]";
	}
}
