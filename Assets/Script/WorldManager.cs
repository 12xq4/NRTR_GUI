using UnityEngine;
using System.Collections;

public class WorldManager : MonoBehaviour {
	public enum ViewDir {Front, Back, Left, Right, Top, Bottom};
	[SerializeField]
	ViewDir view;

	Camera viewCam;
	Vector3 lastFramePosition;
	Vector3 currentPosition;

	public enum Mode {Select, Place_Node, Place_Rod, Place_Str};
	[SerializeField]
	Mode mode;

	bool clicked;

	GameObject selected;

	// Use this for initialization
	void Start () {
		viewCam = GameObject.Find ("Front Camera").GetComponent<Camera>();
		clicked = false;
		selected = null;
	}
	
	// Update is called once per frame
	void Update () {
		currentPosition = viewCam.ScreenToWorldPoint (Input.mousePosition);
		Debug.Log (Input.mousePosition);
		CameraDrag ();
		CameraSwitch ();

		if (mode == Mode.Select) {
			if (!clicked && Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = viewCam.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					if (hit.transform.tag == "Node")
						selected = hit.transform.gameObject;
				}
			}
			// follow mouse position for the meanwhile.
			if (clicked && Input.GetMouseButtonDown (0)) {

			}

		} else if (mode == Mode.Place_Node) {

		} else if (mode == Mode.Place_Rod) {

		} else if (mode == Mode.Place_Str) {

		}

		lastFramePosition = viewCam.ScreenToWorldPoint (Input.mousePosition);
	}

	/*
		 * The below segment handles Camera work. 
		 * Specifically switching orthogonal cameras.
	*/
	void CameraSwitch() {
		if (Input.GetKey (KeyCode.Alpha1)) {
			view = ViewDir.Front;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Front Camera").GetComponent<Camera>();
			viewCam.enabled = true;
		}
		if (Input.GetKey (KeyCode.Alpha2)) {
			view = ViewDir.Back;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Back Camera").GetComponent<Camera>();
			viewCam.enabled = true;
		}
		if (Input.GetKey (KeyCode.Alpha3)) {
			view = ViewDir.Left;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Left Camera").GetComponent<Camera>();
			viewCam.enabled = true;
		}
		if (Input.GetKey (KeyCode.Alpha4)) {
			view = ViewDir.Right;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Right Camera").GetComponent<Camera>();
			viewCam.enabled = true;
		}
		if (Input.GetKey (KeyCode.Alpha5)) {
			view = ViewDir.Top;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Top Camera").GetComponent<Camera>();
			viewCam.enabled = true;
		}
		if (Input.GetKey (KeyCode.Alpha6)) {
			view = ViewDir.Bottom;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Bottom Camera").GetComponent<Camera>();
			viewCam.enabled = true;
		}
	}

	/*
	 	* This is used to move the viewing camera:
	 	* by right click then drag your mouse.
	 	* Actually discovered a cool/easy way of doing this, great!
	 	* 
	 */ 
	void CameraDrag() {
		if (Input.GetMouseButton (1)) {
			Vector3 diff = lastFramePosition - currentPosition;
			GameObject camGroup = GameObject.Find ("Camera Group");
			camGroup.transform.Translate (diff);
		}
	}
}
