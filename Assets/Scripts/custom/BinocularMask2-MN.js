/*
	Old script, not used
*/

#pragma strict

public var mask : Texture;

public var offsetX : float;
public var offsetY : float;

//public var topMask : Texture;
//public var bottomMask : Texture;
//public var blankMask : Texture;

private var maskEnabled : boolean;

function OnEnable() {
    WandEventManager.OnButton1 += enableMask;
}

function Start () {
	//maskEnabled = false;
}


function Update () {
	
	//Enable the Binocular mask
	//if(Input.GetKeyDown(KeyCode.Z)) {
	//	//Debug.Log("1-maskEnabled " + maskEnabled.ToString());
	//	enableMask();
	//}
	
	/*if (maskEnabled || Input.GetKeyUp(KeyCode.Z)) {
		mask.active = true;

		for (var camera : Camera in Camera.allCameras) {
			if (camera.enabled) {
				mask = camera.guiTexture;
				mask.active = true;
				
				if (camera.name.Contains("FrontTop")) {
					mask.texture = topMask;
				} else if (camera.name.Contains("FrontBottom")) {
					mask.texture = bottomMask;
				} else if (camera.name.Contains("ServerView")) {
					mask.active = false;
				} else if (camera.name.Contains("AR")) {
					mask.active = false;
				} else {
					mask.texture = blankMask;
				}
			}
		}
		
		enableMask();
	} else {
		mask.active = false;
	}*/
}

function enableMask() {
	//Debug.Log("2-maskEnabled " + maskEnabled.ToString());
	this.gameObject.GetComponent(NetworkView).RPC("toggleMask", RPCMode.AllBuffered);
}

@RPC
function toggleMask() {
	maskEnabled = !maskEnabled;
	//Debug.Log("3-maskEnabled " + maskEnabled.ToString());
}

function OnGUI() {
	if (maskEnabled)
		GUI.DrawTexture(Rect(offsetX, offsetY, Screen.width, Screen.height), mask, ScaleMode.ScaleToFit, true, 10.0f);

}


//function maskCameras() {
//	maskEnabled = !maskEnabled;
//	
//	if (maskEnabled) {
//		mask.active = true;
//
//		for (var camera : Camera in Camera.allCameras) {
//			if (camera.enabled) {
//				if (camera.name.Contains("FrontTop")) {
//					mask.texture = topMask;
//				} else if (camera.name.Contains("FrontBottom")) {
//					mask.texture = bottomMask;
//				} else if (camera.name.Contains("ServerView")) {
//					mask.active = false;
//				} else if (camera.name.Contains("AR")) {
//					mask.active = false;
//				} else {
//					mask.texture = blankMask;
//				}
//			}
//		}
//	} else {
//		mask.active = false;
//	}
//}