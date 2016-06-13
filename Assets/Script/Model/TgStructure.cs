using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TgStructure : MonoBehaviour {
	List<GameObject> nodes;
	List<GameObject> rods;
	List<GameObject> strs;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddNode(GameObject node) {
		nodes.Add (node);
	}

	public void AddRod(GameObject rod) {
		rods.Add (rod);
	}

	public void AddString(GameObject str) {
		strs.Add (str);
	}
}
