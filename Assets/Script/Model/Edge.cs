using UnityEngine;
using System.Collections;

public class Edge : MonoBehaviour {

	GameObject top;
	GameObject bot;

	public static float density = 1;
	public static float radius = 1; 
	// Use this for initialization
	void Start () {
	
	}

	void OnEnable () {
		WorldManager.OnMoved += RelocateEndNodes;
		WorldManager.OnDestroyed += DestroyEndNodes;
	}

	void OnDisable () {
		WorldManager.OnMoved -= RelocateEndNodes;
		WorldManager.OnDestroyed -= DestroyEndNodes;
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

	void OnMouseOver() {
		if (Input.GetKeyDown (KeyCode.Delete) || Input.GetKeyDown (KeyCode.Backspace)) {
			Destroy (this.gameObject);
		} 
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

	void DestroyEndNodes(GameObject node) {
		if (node == top || node == bot) {
			Destroy (this.gameObject);
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
		return "    - [" + top.GetComponent<Node>().nodeName + ", " + bot.GetComponent<Node>().nodeName + "]";
	}
}
