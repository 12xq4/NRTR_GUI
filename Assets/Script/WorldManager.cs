using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/*	Notes:
 * Phase 1 checklist
 * Reverse z and y for users' understanding. 						(Done)
 * Define a structure which can read YAML and import it into scene;	(Done)
 * Yaml use spaces for indentation, not tabs. Make sure i fix that	(Done)
 * Figure out a unit percision of work								(Done)
 * Make Interface look & feel better.								(Done for now - it will be ongoing effort)
 * Calculate the world center instead of setting it to (0,10,0)		(Done)
 * Implement Material information and a form system to set material. Then change the parser to accept material information. 
 * Push it to the internet, build as web application.
 * 
 * 
 * Phase 2 checklist
 * Improve usablilty by introducing movements by axis. 				(Continually)
 * - This will involve changing the existing click & hold system of selection.
 * Intergrate directly with NTRT by running commandline from Unity
 * - Create a save system first.
 * 
 * Constant Objective:
 * Debug, Keep a test suite
*/ 


public class WorldManager : MonoBehaviour {
	public delegate void OnNodeChanged(GameObject changed); 
	public static event OnNodeChanged OnMoved;
	public static event OnNodeChanged OnDestroyed;

	public static bool editing = true;

	public enum ViewDir {Front, Back, Left, Right, Top, Bottom};
	[SerializeField]
	ViewDir view;
	public Text View;

	Camera viewCam;
	Vector3 lastFramePosition;
	Vector3 currentPosition;

	// A reference point for cameras.
	public Transform center;

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

	// Controls
	bool clicked;

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

		if (!EventSystem.current.IsPointerOverGameObject () && editing) {
			PerspectiveControl ();
			CameraSwitch ();
			ModeSwitch ();
			if (Input.GetKeyDown (KeyCode.Escape)) {
				CleanUp ();
				mode = Mode.Select;
			}
			if (mode == Mode.Select) {
				// select an object.
				// Adding select rods/strings
				if (Input.GetMouseButtonDown (0)) {
					RaycastHit hit;
					Ray ray = viewCam.ScreenPointToRay (Input.mousePosition);
					if (Physics.Raycast (ray, out hit)) {
						selected = hit.transform.gameObject;
					}
				}
				// follow mouse position for the meanwhile.
				if (selected != null) {
					if (selected.transform.tag == "Node") {
						if (Input.GetMouseButton (0) && selected != null)
							FollowMouseDrag ();
						// Drop object.
						if (Input.GetMouseButtonUp (0) && selected != null) {
							selected.GetComponent<Renderer> ().material.color = Color.white;
							selected = null;
						}
						if (Input.GetKeyDown (KeyCode.Delete) || Input.GetKey (KeyCode.Backspace)) {
							if (OnDestroyed != null) {
								OnDestroyed (selected);
							}
							Destroy (selected);
							CleanUp ();
						} 
					} 
					/*
					else {
						selected.transform.FindChild ("Cylinder").transform.GetComponent<Renderer> ().material.color = Color.red;
						if (Input.GetKeyDown (KeyCode.Delete) || Input.GetKey (KeyCode.Backspace)) {
							Destroy (selected);
							CleanUp ();
						} 
					}
					*/
				}
			} else if (mode == Mode.Place_Node) {
				if (selected == null)
					selected = Instantiate (node, new Vector3 (), Quaternion.Euler (0, 0, 0)) as GameObject;
				FollowMouseDrag ();
				if (Input.GetMouseButtonDown (0) && selected != null) {
					selected.GetComponent<Renderer> ().material.color = Color.white;
					selected = null;
				}
			} else if (mode == Mode.Place_Rod) {
				if (Input.GetMouseButtonDown (0)) {
					RaycastHit hit;
					Ray ray = viewCam.ScreenPointToRay (Input.mousePosition);
					if (Physics.Raycast (ray, out hit)) {
						if (hit.transform.tag == "Node" && topNode == null)
							topNode = hit.transform.gameObject;
						else if (hit.transform !=  topNode.transform && hit.transform.tag == "Node" && topNode != null && botNode == null) // some unnecessary safety check.
						botNode = hit.transform.gameObject;
					}
				}
				if (topNode != null && botNode != null) {
					GameObject clone = Instantiate (rod, center.position, Quaternion.Euler (0, 0, 0)) as GameObject;
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
						else if (hit.transform !=  topNode.transform && hit.transform.tag == "Node" && topNode != null && botNode == null) // some unnecessary safety check.
						botNode = hit.transform.gameObject;
					}
				}
				if (topNode != null && botNode != null) {
					GameObject clone = Instantiate (str, center.position, Quaternion.Euler (0, 0, 0)) as GameObject;
					clone.GetComponent<StringCon> ().AssignNode (topNode, botNode);
					topNode = null;
					botNode = null;
				}
			}
		}
		lastFramePosition = viewCam.ScreenToWorldPoint (Input.mousePosition);
	}
	public void CameraSwitch (string dir) {
		if (dir.Equals ("Front")) {
			view = ViewDir.Front;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Front Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Front";
		}
		if (dir.Equals ("Back")) {
			view = ViewDir.Back;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Back Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Back";
		}
		if (dir.Equals ("Top")) {
			view = ViewDir.Top;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Top Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Top";
		}
		if (dir.Equals ("Bottom")) {
			view = ViewDir.Bottom;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Bottom Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Bottom";
		}
		if (dir.Equals ("Left")) {
			view = ViewDir.Left;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Left Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Left";
		}
		if (dir.Equals ("Right")) {
			view = ViewDir.Right;
			viewCam.enabled = false;
			viewCam = GameObject.Find ("Right Camera").GetComponent<Camera>();
			viewCam.enabled = true;
			View.text = "Perspective: Right";	
		}
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
	 	* Implement a perspective camera for which users could browse their work.
	 	* Doesn't show any numbers.
	 	* 
	 	* - Fix Camera angle problem (best solution is to clamp the cameras) 
	 */ 
	void PerspectiveControl(){
		if (Input.GetKey (KeyCode.LeftArrow)) {
			Camera.main.transform.RotateAround (center.position, Vector3.up, 30 * Time.deltaTime);
		} else if (Input.GetKey (KeyCode.RightArrow)) {
			Camera.main.transform.RotateAround (center.position, Vector3.up, -30 * Time.deltaTime);
		} else if (Input.GetKey (KeyCode.UpArrow)) {
			// Camera.main.transform.RotateAround (Vector3.zero, Vector3.right, 30 * Time.deltaTime);
			if (Camera.main.transform.rotation.eulerAngles.x < 89 || Camera.main.transform.rotation.eulerAngles.x > 271) {
				Camera.main.transform.Translate (new Vector3 (0, 10, 0) * Time.deltaTime);
				Camera.main.transform.LookAt (center.position);
			}
		} else if (Input.GetKey (KeyCode.DownArrow)) {
			// Camera.main.transform.RotateAround (Vector3.zero, Vector3.right, -30 * Time.deltaTime);
			if (Camera.main.transform.rotation.eulerAngles.x > 271 || Camera.main.transform.rotation.eulerAngles.x < 89) {
				Camera.main.transform.Translate (new Vector3 (0, -10, 0) * Time.deltaTime);
				Camera.main.transform.LookAt (center.position);
			}
		}
	}

	/*
	 	* This is used to switch editing mode.
	 	* 
	 */ 
	void ModeSwitch ()	{
		if (Input.GetKeyDown (KeyCode.Z)) {
			SwitchToSelect ();
		} else if (Input.GetKeyDown (KeyCode.X)) {
			SwitchToNodePlace ();
		} else if (Input.GetKeyDown (KeyCode.C)) {
			SwitchToRodPlace ();
		} else if (Input.GetKeyDown (KeyCode.V)) {
			SwitchToStrPlace ();
		}
	}

	public void SwitchToSelect() {
		CleanUp ();
		mode = Mode.Select;
	}

	public void SwitchToNodePlace() {
		CleanUp ();
		mode = Mode.Place_Node;
	}

	public void SwitchToRodPlace() {
		CleanUp ();
		mode = Mode.Place_Rod;
	}

	public void SwitchToStrPlace() {
		CleanUp ();
		mode = Mode.Place_Str;
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
	 	* - Implement a structure where adding to the negative y-axis/height is not accepted.
	 	* 
	 */ 
	void FollowMouseDrag() {
		Vector3 worldMousePos = new Vector3();
		selected.GetComponent<Renderer> ().material.color = Color.red;
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
		// restricting placement of nodes below 0 height. i.e. in the ground.
		if (selected.transform.position.y < 0) {
			UnityEngine.Debug.Log ("Cannot Place Node below ground");
			Vector3 newPos = selected.transform.position;
			newPos.y = 0;
			selected.transform.position = newPos;
		}

		Coord.text = "X: " + selected.transform.position.x + "    " + "Y: " + selected.transform.position.y + "    "
			+ "Z: " + selected.transform.position.z + "\n";

		if (OnMoved != null) {
			OnMoved (selected);
		}
	}

	void CleanUp() {
		if (selected != null) {
			if (selected.transform.tag == "Node")
				Destroy (selected);
			else 
				selected.transform.FindChild ("Cylinder").transform.GetComponent<Renderer> ().material.color = Color.white;
			selected = null;
		}
		if (topNode != null)
			topNode = null;
		if (botNode != null)
			botNode = null;
	}

	public void ClearScene() {
		var Nodes = GameObject.FindGameObjectsWithTag ("Node");
		var Rods = GameObject.FindGameObjectsWithTag ("Rod");
		var Strs = GameObject.FindGameObjectsWithTag ("String");
		for (int i = 0; i < Nodes.Length; i++)
			Destroy (Nodes [i]);
		for (int i = 0; i < Rods.Length; i++)
			Destroy (Rods [i]);
		for (int i = 0; i < Strs.Length; i++)
			Destroy (Strs [i]);
	}
}
