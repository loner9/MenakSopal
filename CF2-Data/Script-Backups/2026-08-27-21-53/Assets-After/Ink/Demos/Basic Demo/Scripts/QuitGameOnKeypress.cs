using UnityEngine;

public class QuitGameOnKeypress : MonoBehaviour {
	
	public KeyCode key = KeyCode.Escape;
	
	void Update () {
		if(ControlFreak2.CF2Input.GetKeyDown(key)) Application.Quit();
	}
}