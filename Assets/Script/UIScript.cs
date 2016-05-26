using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/*
 * Script that contains list of UI functions. This will be attached to the Manager as well. 
*/

public class UIScript : MonoBehaviour {
	public InputField radiusView;
	public InputField densityView;
	public InputField stiffnessView;
	public InputField dampingView;
	public InputField pretensionView;

	void Start() {
		ChangeDisplay ();
	}

	public void OnRadiusChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			Edge.radius = float.Parse (userInput);
			radiusView.placeholder.GetComponent<Text> ().text = "Radius: " + Edge.radius;
			radiusView.text = string.Empty;
		}
	}

	public void OnDensityChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			Edge.density = float.Parse (userInput);
			densityView.placeholder.GetComponent<Text> ().text = "Density: " + Edge.density;
			densityView.text = string.Empty;
		}
	}

	public void OnStiffnessChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			StringCon.stiffness = float.Parse (userInput);
			stiffnessView.placeholder.GetComponent<Text> ().text = "Stiffness: " + StringCon.stiffness;
			stiffnessView.text = string.Empty;
		}
	}

	public void OnDampingChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			StringCon.damping = float.Parse (userInput);
			dampingView.placeholder.GetComponent<Text> ().text = "Damping: " + StringCon.damping;
			dampingView.text = string.Empty;
		}
	}

	public void OnPretensionChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			StringCon.pretension = float.Parse (userInput);
			pretensionView.placeholder.GetComponent<Text> ().text = "Pretension: " + StringCon.pretension;
			pretensionView.text = string.Empty;
		}
	}

	public void ChangeDisplay() {
		radiusView.placeholder.GetComponent<Text>().text = "Radius: " + Edge.radius;
		densityView.placeholder.GetComponent<Text>().text = "Density: " + Edge.density;
		stiffnessView.placeholder.GetComponent<Text>().text = "Stiffness: " + StringCon.stiffness;
		dampingView.placeholder.GetComponent<Text>().text = "Damping: " + StringCon.damping;
		pretensionView.placeholder.GetComponent<Text>().text = "Pretension: " + StringCon.pretension;
	}
}
