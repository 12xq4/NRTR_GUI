using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;

public class WorldManager : MonoBehaviour {
	public delegate void OnNodeChanged(GameObject changed); 
	public static event OnNodeChanged OnMoved;

	public enum ViewDir {Front, Back, Left, Right, Top, Bottom};
	[SerializeField]
	ViewDir view;
	public Text View;

	Camera viewCam;
	Vector3 lastFramePosition;
	Vector3 currentPosition;

	public enum Mode {Select, Place_Node, Place_Rod, Place_Str};
	[SerializeField]
	Mode mode;

	// Pointer to the currently selected Node and its position in 3D space.
	GameObject selected;
	public Text Coord;

	// 2 pointers to the two nodes selected for connection.
	GameObject topNode;
	GameObject botNode;

	// Pregabs
	public GameObject node;
	public GameObject rod;
	public GameObject str;

	// Use this for initialization
	void Start () {
		view = ViewDir.Front;
		View.text = "Perspective: Front";
		viewCam = GameObject.Find ("Front Camera").GetComponent<Camera>();

		selected = null;
		mode = Mode.Select;
	}
	
	// Update is called once per frame
	void Update () {
		currentPosition = viewCam.ScreenToWorldPoint (Input.mousePosition);

		CameraDrag ();
		CameraSwitch ();
		ModeSwitch ();

		if (Input.GetKey (KeyCode.Escape)) {
			CleanUp ();
			mode = Mode.Select;
		}
		if (mode == Mode.Select) {
			// select an object.
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = viewCam.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					if (hit.transform.tag == "Node")
						selected = hit.transform.gameObject;
				}
			}
			// follow mouse position for the meanwhile.
			if (Input.GetMouseButton (0) && selected != null)
				FollowMouseDrag ();
			// Drop object.
			if (Input.GetMouseButtonUp (0) && selected != null) {
				selected = null;
			}
		} else if (mode == Mode.Place_Node) {
			if (selected == null)
				selected = Instantiate (node, new Vector3 (), Quaternion.Euler (0, 0, 0)) as GameObject;
			FollowMouseDrag ();
			if (Input.GetMouseButtonDown (0) && selected != null) {
				selected = null;
			}
		} else if (mode == Mode.Place_Rod) {
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = viewCam.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					if (hit.transform.tag == "Node" && topNode == null)
						topNode = hit.transform.gameObject;
					else if (hit.transform.tag == "Node" && topNode != null && botNode == null) // some unnecessary safety check.
						botNode = hit.transform.gameObject;
				}
			}
			if (topNode != null && botNode != null) {
				GameObject clone = Instantiate (rod, Vector3.zero, Quaternion.Euler (0, 0, 0)) as GameObject;
				clone.GetComponent<Edge> ().AssignNode (topNode, botNode);
				topNode = null;
				botNode = null;
			}
		} else if (mode == Mode.Place_Str) {
			if (Input.GetMouseButtonDown (0)) {
				RaycastHit hit;
				Ray ray = viewCam.ScreenPointToRay (Input.mousePosition);
				if (Physics.Raycast (ray, out hit)) {
					if (hit.transform.tag == "Node" && topNode == null)
						topNode = hit.transform.gameObject;
					else if (hit.transform.tag == "Node" && topNode != null && botNode == null) // some unnecessary safety check.
						botNode = hit.transform.gameObject;
				}
			}
			if (topNode != null && botNode != null) {
				GameObject clone = Instantiate (str, Vector3.zero, Quaternion.Euler (0, 0, 0)) as GameObject;
				clone.GetComponent<StringCon> ().AssignNode (topNode, botNode);
				topNode = null;
				botNode = null;
			}
		}

		lastFramePosition = viewCam.ScreenToWorldPoint (Input.mousePosition);
	}

	/*
		 * The below segment handles Camera work. 
		 * Specifically switching orthogonal cameras.
	*/
	void CameraSwitch() {
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			view = ViewDir.Front;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Front Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Front";
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			view = ViewDir.Back;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Back Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Back";
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			view = ViewDir.Left;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Left Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Left";
		}
		if (Input.GetKeyDown (KeyCode.Alpha4)) {
			view = ViewDir.Right;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Right Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Right";
		}
		if (Input.GetKeyDown (KeyCode.Alpha5)) {
			view = ViewDir.Top;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Top Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Top";
		}
		if (Input.GetKeyDown (KeyCode.Alpha6)) {
			view = ViewDir.Bottom;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Bottom Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Bottom";
		}
	}

	/*
	 	* This is used to switch editing mode.
	 	* 
	 */ 
	void ModeSwitch ()	{
		if (Input.GetKeyDown (KeyCode.Z)) {
			CleanUp ();
			mode = Mode.Select;
		} else if (Input.GetKeyDown (KeyCode.X)) {
			CleanUp ();
			mode = Mode.Place_Node;
		} else if (Input.GetKeyDown (KeyCode.C)) {
			CleanUp ();
			mode = Mode.Place_Rod;
		} else if (Input.GetKeyDown (KeyCode.V)) {
			CleanUp ();
			mode = Mode.Place_Str;
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

	/*
	 	* This function define a dragging behavior.
	 	* 
	 */ 
	void FollowMouseDrag() {
		Vector3 worldMousePos = new Vector3();
		if (mode == Mode.Select && selected != null) {
			if (view == ViewDir.Front) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = Mathf.RoundToInt (selected.transform.position.z);
			} else if (view == ViewDir.Back) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = Mathf.RoundToInt (selected.transform.position.z);
			} else if (view == ViewDir.Left) {
				worldMousePos.x = Mathf.RoundToInt (selected.transform.position.x);
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			} else if (view == ViewDir.Right) {
				worldMousePos.x = Mathf.RoundToInt (selected.transform.position.x);
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			} else if (view == ViewDir.Top) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = Mathf.RoundToInt (selected.transform.position.y);
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			} else if (view == ViewDir.Bottom) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = Mathf.RoundToInt (selected.transform.position.y);
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			}
			selected.transform.position = worldMousePos; 
		} else if (mode == Mode.Place_Node) {
			if (view == ViewDir.Front) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = 0;
			} else if (view == ViewDir.Back) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = 0;
			} else if (view == ViewDir.Left) {
				worldMousePos.x = 0;
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			} else if (view == ViewDir.Right) {
				worldMousePos.x = 0;
				worldMousePos.y = Mathf.Floor (currentPosition.y);
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			} else if (view == ViewDir.Top) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = 0;
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			} else if (view == ViewDir.Bottom) {
				worldMousePos.x = Mathf.Floor (currentPosition.x);
				worldMousePos.y = 0;
				worldMousePos.z = Mathf.Floor (currentPosition.z);
			}
			selected.transform.position = worldMousePos; 
		}
		Coord.text = "X: " + selected.transform.position.x + "    " + "Y: " + selected.transform.position.y + "    "
			+ "Z: " + selected.transform.position.z + "\n";

		if (OnMoved != null) {
			OnMoved (selected);
		}
	}

	void CleanUp() {
		if (selected != null) {
			Destroy (selected);
			selected = null;
		}
		if (topNode != null)
			topNode = null;
		if (botNode != null)
			botNode = null;
	}

	void ExportToYAML() {
		var filename = "build.yaml";
		var Nodes = GameObject.FindGameObjectsWithTag ("Node");
		var Rods = GameObject.FindGameObjectsWithTag ("Rod");
		var Strs = GameObject.FindGameObjectsWithTag ("String");
		Debug.Log (Nodes.Length);
		Debug.Log (Rods.Length);
		Debug.Log (Strs.Length);

		if (File.Exists (filename)) {
			return;
		}
		var file = File.CreateText (filename);
		file.WriteLine ("nodes:");
		for (int i = 0; i < Nodes.Length; i++) {
			file.WriteLine (Nodes[i].GetComponent<Node>().ToString());
		}
		file.WriteLine ("pair_groups:");
		file.WriteLine ("\trod:");
		for (int i = 0; i < Rods.Length; i++) {
			file.WriteLine (Rods[i].GetComponent<Edge>().ToString());
		}
		file.WriteLine ("\tstring:");
		for (int i = 0; i < Strs.Length; i++) {
			file.WriteLine (Strs[i].GetComponent<StringCon>().ToString());
		}
		file.Close ();
	}
}
