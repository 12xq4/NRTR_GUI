using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour {
	
	[SerializeField]
	int x;
	[SerializeField]
	int y;
	[SerializeField]
	int z;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void UpdatePosition() {
		x = Mathf.RoundToInt(transform.position.x);
		y = Mathf.RoundToInt(transform.position.y);
		z = Mathf.RoundToInt(transform.position.z);
	}

	void OnMouseEnter() {
		transform.GetComponent<Renderer> ().material.color = Color.red;
	}

	void OnMouseExit() {
		transform.GetComponent<Renderer> ().material.color = Color.white;
	}
}
