using UnityEngine;
using System.Collections;
using System.IO;
using System.Diagnostics;
using System;
using System.Text;

using YamlDotNet.RepresentationModel;

public class CommManager : MonoBehaviour {

	string filepath;

	// Pregabs
	public GameObject node;
	public GameObject rod;
	public GameObject str;

	// A reference point for cameras.
	public Transform center;

	private bool pathSelected = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void ImportFile(string filepath) {
		if (filepath.Length != 0){
			this.filepath = filepath;
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
						clone.GetComponent<Node> ().Initialize (Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
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

	public void ImportFromYAML() {
		//filepath = EditorUtility.OpenFilePanel ("Please select your import YAML file.", "", "yaml");
		WorldManager.editing = false;
		UniFileBrowser.use.OpenFileWindow(ImportFile);
		WorldManager.editing = true;
	}

	/*
	 * This is a helper method for writing/saving to a file.
	 * 
	 */
	void WriteFile (string path) {
		var Nodes = GameObject.FindGameObjectsWithTag ("Node");
		var Rods = GameObject.FindGameObjectsWithTag ("Rod");
		var Strs = GameObject.FindGameObjectsWithTag ("String");
		if (!string.IsNullOrEmpty(path) && File.Exists (path)) {
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
		/*
		if (string.IsNullOrEmpty (filepath)) {
			var filename = EditorUtility.SaveFilePanel("Save your work as a YAML file.", "", "build.yaml", "yaml");
		}
		if (filepath.Length != 0) {
			WriteFile (filepath);
		}
		*/
		WorldManager.editing = false;
		if (string.IsNullOrEmpty (filepath)) {
			UniFileBrowser.use.SaveFileWindow (WriteFile);
		} else {
			WriteFile (filepath);
		}
		WorldManager.editing = true;
	}

	/*
	 	* This method builds the Yaml file.
	 	* Note this will write any existing yaml file with the name "build.yaml". To prevent overwriting, rename
	 	* your saved build file to something else.
	*/
	public void ExportToYAML() {
		/*
		var filename = EditorUtility.SaveFilePanel("Save your work as a YAML file.", "", "build.yaml", "yaml");
		if (filename.Length != 0) {
			WriteFile (filename);
		}
		*/
		WorldManager.editing = false;
		UniFileBrowser.use.SaveFileWindow (WriteFile);
		WorldManager.editing = true;
	}

	public void TestYAML () {
		WorldManager.editing = false;
		var path = PlayerPrefs.GetString ("YAML Path");
		if (string.IsNullOrEmpty (path)) {
			UniFileBrowser.use.OpenFileWindow (SelectBuildPath);
			if (pathSelected)
				path = PlayerPrefs.GetString ("YAML Path");
			else {   // User didn't set a path.
				WorldManager.editing = true;
				return;
			}
		}
		Save ();
		Process ntrt = new Process ();
		ntrt.StartInfo.FileName = path;
		ntrt.StartInfo.Arguments = filepath;
		ntrt.Start ();
		pathSelected = false;
		WorldManager.editing = true;
	}

	void SelectBuildPath(string builderpath) {
		if (builderpath.Length > 0) {
			PlayerPrefs.SetString ("YAML Path", builderpath);
			pathSelected = true;
		} else {	// user didn't choose a file path at all. Stop this method.
			return;
		}
	}
}
