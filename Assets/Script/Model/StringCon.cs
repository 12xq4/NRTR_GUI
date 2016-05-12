using UnityEngine;
using System.Collections;

public class StringCon : MonoBehaviour {

	Node top;
	Node bot;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void AssignNode( Node tp, Node bt)
	{
		top = tp;
		bot = bt;
	}

	void ShapeConnector () {
		Vector3 distance = top.transform.position - bot.transform.position;
		Vector3 midPos = distance / 2.0f + bot.transform.position;
		Vector3 scale = transform.localScale;
		scale.z = distance.magnitude;
		transform.localScale = scale;
		transform.rotation = Quaternion.FromToRotation (Vector3.up, distance);
	}
}
