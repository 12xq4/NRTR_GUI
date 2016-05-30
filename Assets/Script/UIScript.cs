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
			float result = float.Parse (userInput);
			if (result > 0) {
				Edge.radius = result;
				radiusView.placeholder.GetComponent<Text> ().text = "Radius: " + Edge.radius;
				radiusView.text = string.Empty;
			} else {
				radiusView.placeholder.GetComponent<Text> ().text = "Invalid input.";
				radiusView.text = string.Empty;
			}
		}
	}

	public void OnDensityChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			float result = float.Parse (userInput);
			if (result > 0) {
				Edge.density = result;
				densityView.placeholder.GetComponent<Text> ().text = "Density: " + Edge.density;
				densityView.text = string.Empty;
			} else {
				densityView.placeholder.GetComponent<Text> ().text = "Invalid input.";
				densityView.text = string.Empty;
			}
		}
	}

	public void OnStiffnessChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			float result = float.Parse (userInput);
			if (result > 0) {
				StringCon.stiffness = result;
				stiffnessView.placeholder.GetComponent<Text> ().text = "Stiffness: " + StringCon.stiffness;
				stiffnessView.text = string.Empty;
			} else {
				stiffnessView.placeholder.GetComponent<Text> ().text = "Invalid input.";
				stiffnessView.text = string.Empty;
			}
		}
	}

	public void OnDampingChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			float result = float.Parse (userInput);
			if (result > 0) {
				StringCon.damping = result;
				dampingView.placeholder.GetComponent<Text> ().text = "Damping: " + StringCon.damping;
				dampingView.text = string.Empty;
			} else {
				dampingView.placeholder.GetComponent<Text> ().text = "Invalid input.";
				dampingView.text = string.Empty;
			}
		}
	}

	public void OnPretensionChanged(string userInput) {
		if (!string.IsNullOrEmpty (userInput)) {
			float result = float.Parse (userInput);
			if (result > 0) {
				StringCon.pretension = result;
				pretensionView.placeholder.GetComponent<Text> ().text = "Pretension: " + StringCon.pretension;
				pretensionView.text = string.Empty;
			} else {
				pretensionView.placeholder.GetComponent<Text> ().text = "Invalid input.";
				pretensionView.text = string.Empty;
			}
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
