using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System;
using UnityEditor;
using UnityEngine.EventSystems;

using YamlDotNet.RepresentationModel;

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


	public enum ViewDir {Front, Back, Left, Right, Top, Bottom};
	[SerializeField]
	ViewDir view;
	public Text View;

	Camera viewCam;
	Vector3 lastFramePosition;
	Vector3 currentPosition;

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

	// Used for saving
	string filepath;

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

		if (!EventSystem.current.IsPointerOverGameObject ()) {
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
						else if (hit.transform.tag == "Node" && topNode != null && botNode == null) // some unnecessary safety check.
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
						else if (hit.transform.tag == "Node" && topNode != null && botNode == null) // some unnecessary safety check.
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

	public void CleanUp() {
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

	public void ImportFromYAML() {
		filepath = EditorUtility.OpenFilePanel ("Please select your import YAML file.", "", "yaml");
		if (filepath.Length != 0){
			var pass = new StringBuilder();
			try 
			{
				// Create an instance of StreamReader to read from a file.
				// The using statement also closes the StreamReader.
				using (StreamReader sr = new StreamReader(filepath)) 
				{
					string line;
					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null) 
					{
						pass.AppendLine(line);
					}
				}
			}
			catch (Exception e) 
			{
				// Let the user know what went wrong.
				Console.WriteLine("The file could not be read:");
				Console.WriteLine(e.Message);
			}
			var input = new StringReader(pass.ToString());
			var yaml = new YamlStream();
			yaml.Load(input);

			// Examine the stream
			var mapping =
				(YamlMappingNode)yaml.Documents[0].RootNode;
		
			// Debug.Log (mapping);	// mapping contains all information.

			// var output = new StringBuilder();
			foreach (var entry in mapping.Children)
			{
				if (entry.Key.ToString ().Equals ("nodes")) {
					var childMapping = ((YamlMappingNode)entry.Value);
					foreach (var child in childMapping.Children) {
						string name = child.Key.ToString ();
						Vector3 pos = new Vector3 ();
						int count = 0;
						var coordinates = ((YamlSequenceNode)child.Value);
						foreach (var num in coordinates.Children) {
							count++;
							if (count == 1) {
								pos.x = Mathf.Round (float.Parse (num.ToString ()));
							} else if (count == 2) {
								pos.y = Mathf.Round (float.Parse (num.ToString ()));
							} else {
								pos.z = Mathf.Round (float.Parse (num.ToString ()));
							}
						}
						GameObject clone = Instantiate (node, pos, Quaternion.Euler (0, 0, 0)) as GameObject;
						clone.name = name;
						clone.GetComponent<Node> ().nodeName = name;
						if (OnMoved != null) {
							OnMoved (selected);
						}
					}
				} else if (entry.Key.ToString ().Equals ("pair_groups")) {
					var childMapping = ((YamlMappingNode)entry.Value);
					foreach (var child in childMapping.Children) {
						// Debug.Log (child.Key);
						if (child.Key.ToString ().Contains ("rod")) {
							var coordinates = ((YamlSequenceNode)child.Value);
							foreach (var num in coordinates.Children) {
								string name1 = "";
								string name2 = "";
								int count = 0;
								var names = (YamlSequenceNode)num;
								foreach (var name in names.Children) {
									count++;
									if (count == 1)
										name1 = name.ToString ();
									else
										name2 = name.ToString ();
								}
								GameObject clone = Instantiate (rod, center.position, Quaternion.Euler (0, 0, 0)) as GameObject;
								clone.GetComponent<Edge> ().AssignNode (GameObject.Find (name1), GameObject.Find (name2));
							}
						} else if (child.Key.ToString ().Contains ("string")) {
							var coordinates = ((YamlSequenceNode)child.Value);
							foreach (var num in coordinates.Children) {
								string name1 = "";
								string name2 = "";
								int count = 0;
								var names = (YamlSequenceNode)num;
								foreach (var name in names.Children) {
									count++;
									if (count == 1)
										name1 = name.ToString ();
									else
										name2 = name.ToString ();
								}
								GameObject clone = Instantiate (str, center.position, Quaternion.Euler (0, 0, 0)) as GameObject;
								clone.GetComponent<StringCon> ().AssignNode (GameObject.Find (name1), GameObject.Find (name2));
							}
						}
					}
				} else if (entry.Key.ToString ().Equals ("builders")) {
					var sublvl1 = ((YamlMappingNode)entry.Value);
					foreach (var sub in sublvl1.Children) {
						if (sub.Key.ToString ().Contains ("rod")) {
							var sublvl2 = ((YamlMappingNode)sub.Value);
							foreach (var sub2 in sublvl2.Children) {
								if (sub2.Key.ToString ().Equals ("parameters")) {
									var sublvl3 = ((YamlMappingNode)sub2.Value);
									foreach (var sub3 in sublvl3.Children) {
										if (sub3.Key.ToString ().Equals ("density")) {
											Edge.density = float.Parse (sub3.Value.ToString());
										} else if (sub3.Key.ToString ().Equals ("radius")) {
											Edge.radius = float.Parse (sub3.Value.ToString());
										}
									}
								}
							}
						} else if (sub.Key.ToString ().Contains ("string")) {
							var sublvl2 = ((YamlMappingNode)sub.Value);
							foreach (var sub2 in sublvl2.Children) {
								if (sub2.Key.ToString ().Equals ("parameters")) {
									var sublvl3 = ((YamlMappingNode)sub2.Value);
									foreach (var sub3 in sublvl3.Children) {
										if (sub3.Key.ToString ().Equals ("stiffness")) {
											StringCon.stiffness = float.Parse (sub3.Value.ToString());
										} else if (sub3.Key.ToString ().Equals ("damping")) {
											StringCon.damping = float.Parse (sub3.Value.ToString());
										} else if (sub3.Key.ToString ().Equals ("pretension")) {
											StringCon.pretension = float.Parse (sub3.Value.ToString());
										}
									}
								}
							}
						}
					}
					transform.GetComponent<UIScript> ().ChangeDisplay ();
				}
			} 
		// Debug.Log(output);
		}
	}

	/*
	 * This is a helper method for writing/saving to a file.
	 * 
	 */
	public void WriteFile (string path) {
		var Nodes = GameObject.FindGameObjectsWithTag ("Node");
		var Rods = GameObject.FindGameObjectsWithTag ("Rod");
		var Strs = GameObject.FindGameObjectsWithTag ("String");
		if (File.Exists (path)) {
			var file = File.CreateText ("temp.txt");
			if (Nodes.Length != 0) {
				file.WriteLine ("nodes:");
				for (int i = 0; i < Nodes.Length; i++) {
					file.WriteLine (Nodes [i].GetComponent<Node> ().ToString ());
				}
			}
			if (!(Rods.Length == 0 && Strs.Length == 0)) {
				file.WriteLine ("pair_groups:");
				if (Rods.Length != 0) {
					file.WriteLine ("  rod:");
					for (int i = 0; i < Rods.Length; i++) {
						file.WriteLine (Rods [i].GetComponent<Edge> ().ToString ());
					}
				}
				if (Strs.Length != 0) {
					file.WriteLine ("  string:");
					for (int i = 0; i < Strs.Length; i++) {
						file.WriteLine (Strs [i].GetComponent<StringCon> ().ToString ());
					}
				}
				file.WriteLine ("builders:");
				if (Rods.Length != 0) {
					file.WriteLine ("  rod:");
					file.WriteLine ("    class: tgRodInfo");
					file.WriteLine ("    parameters:");
					file.WriteLine ("      density: " + Edge.density);
					file.WriteLine ("      radius: " + Edge.radius);
				}
				if (Strs.Length != 0) {
					file.WriteLine ("  string:");
					file.WriteLine ("    class: tgBasicActuatorInfo");
					file.WriteLine ("    parameters:");
					file.WriteLine ("      stiffness: " + StringCon.stiffness);
					file.WriteLine ("      damping: " + StringCon.damping);
					file.WriteLine ("      pretension: " + StringCon.pretension);
				}
			}
			file.Close ();
			File.Replace ("temp.txt", path, null); 
		} else {
			var file = File.CreateText (path);
			if (Nodes.Length != 0) {
				file.WriteLine ("nodes:");
				for (int i = 0; i < Nodes.Length; i++) {
					file.WriteLine (Nodes [i].GetComponent<Node> ().ToString ());
				}
			}
			if (!(Rods.Length == 0 && Strs.Length == 0)) {
				file.WriteLine ("pair_groups:");
				if (Rods.Length != 0) {
					file.WriteLine ("  rod:");
					for (int i = 0; i < Rods.Length; i++) {
						file.WriteLine (Rods [i].GetComponent<Edge> ().ToString ());
					}
				}
				if (Strs.Length != 0) {
					file.WriteLine ("  string:");
					for (int i = 0; i < Strs.Length; i++) {
						file.WriteLine (Strs [i].GetComponent<StringCon> ().ToString ());
					}
				}
				file.WriteLine ("builders:");
				if (Rods.Length != 0) {
					file.WriteLine ("  rod:");
					file.WriteLine ("    class: tgRodInfo");
					file.WriteLine ("    parameters:");
					file.WriteLine ("      density: " + Edge.density);
					file.WriteLine ("      radius: " + Edge.radius);
				}
				if (Strs.Length != 0) {
					file.WriteLine ("  string:");
					file.WriteLine ("    class: tgBasicActuatorInfo");
					file.WriteLine ("    parameters:");
					file.WriteLine ("      stiffness: " + StringCon.stiffness);
					file.WriteLine ("      damping: " + StringCon.damping);
					file.WriteLine ("      pretension: " + StringCon.pretension);
				}
			}
			file.Close ();
		}
	}

	public void Save () {
		if (string.IsNullOrEmpty (filepath)) {
			var filename = EditorUtility.SaveFilePanel("Save your work as a YAML file.", "", "build.yaml", "yaml");
		}
		if (filepath.Length != 0) {
			WriteFile (filepath);
		}
	}
		
	/*
	 	* This method builds the Yaml file.
	 	* Note this will write any existing yaml file with the name "build.yaml". To prevent overwriting, rename
	 	* your saved build file to something else.
	*/
	public void ExportToYAML() {
		var filename = EditorUtility.SaveFilePanel("Save your work as a YAML file.", "", "build.yaml", "yaml");
		if (filename.Length != 0) {
			WriteFile (filename);
		}
	}

	public void TestYAML () {
		var path = EditorUtility.OpenFilePanel ("Please input your YAML builder file path.", "", "");
		if (path.Length != 0) {
			Save ();
			Process ntrt = new Process ();
			ntrt.StartInfo.FileName = path;
			ntrt.StartInfo.Arguments = filepath;
			ntrt.Start ();
		}
	}
}
