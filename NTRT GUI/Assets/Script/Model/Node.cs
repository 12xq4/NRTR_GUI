using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour {
	public static int totalNode;
	
	[SerializeField]
	int x;
	[SerializeField]
	int y;
	[SerializeField]
	int z;

	public string nodeName;

	// Use this for initialization
	void Awake() {
		nodeName = "node" + totalNode;
		totalNode++;
	}

	void Start () {

	}

	void OnDestroy() {
		totalNode--;
	}

	void OnEnable () {
		WorldManager.OnMoved += UpdatePosition;
	}

	void OnDisable () {
		WorldManager.OnMoved -= UpdatePosition;
	}

	// Update is called once per frame
	void Update () {
	}

	void UpdatePosition(GameObject changed) {
		if (changed = gameObject) {
			x = Mathf.RoundToInt(transform.position.x);
			y = Mathf.RoundToInt(transform.position.y);
			z = Mathf.RoundToInt(transform.position.z);
		}
	}

	void OnMouseEnter() {
		transform.GetComponent<Renderer> ().material.color = Color.red;
	}

	void OnMouseExit() {
		transform.GetComponent<Renderer> ().material.color = Color.white;
	}

	public string ToString() {
		return "  " + nodeName + ": [" + x + ", " + y + ", " + z + "]";
	}
}
