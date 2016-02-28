using UnityEngine;
using System.Collections;


public class ActivateBinocular : MonoBehaviour {

	//public GameObject serverCamera;
	//public GameObject binocularCamera;

	public static GameObject binocularRenderPlane;
	public static GameObject binocualrMask;

	public Camera Server;
	public Camera FrontTop;
	public Camera FromBottom;
	public Camera RightTop;
	public Camera RightBottom;
	public Camera LeftTop;
	public Camera LeftBottom;
	public Camera FloorTop;
	public Camera FloorBottom;
	//public Camera BinocularCamera;


	private static bool isBinocularOn = false;


	void OnEnable() {
		WandEventManager.OnButton2 += BinocularActivationSwap;
	}


	// Use this for initialization
	void Start () {
		binocularRenderPlane = GameObject.Find ("Binocular Render Plane");
		binocualrMask = GameObject.Find ("Binocular Mask");

		binocularRenderPlane.SetActive(false);
		binocualrMask.SetActive(false);

		Server.farClipPlane = 1000f;
		FrontTop.farClipPlane = 1000f;
		FromBottom.farClipPlane = 1000f;
		RightTop.farClipPlane = 1000f;
		RightBottom.farClipPlane = 1000f;
		LeftTop.farClipPlane = 1000f;
		LeftBottom.farClipPlane = 1000f;
		FloorTop.farClipPlane = 1000f;
		FloorBottom.farClipPlane = 1000f;
		//BinocularCamera.farClipPlane = 1000f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.X) ) {
			BinocularActivationSwap();
			RPCBinocularActivationSwap();
		}
		if (Input.GetMouseButtonDown (0) || Input.GetMouseButtonDown (1) || Input.GetMouseButtonDown (2)) {
			isBinocularOn = true;
			SetBinocularActivation(true);
			BinocularActivationTrigger();
			RPCBinocularActivationTrigger();
		}
		if (Input.GetMouseButtonUp (0) || Input.GetMouseButtonUp (1) || Input.GetMouseButtonUp (2)) {
			isBinocularOn = false;
			SetBinocularActivation(false);
			BinocularActivationTrigger();
			RPCBinocularActivationTrigger();
		}
	}

	void SetBinocularActivation(bool bin){
		networkView.RPC("RPCSetBinocularActivation", RPCMode.Others, bin);
	}
	
	[RPC] void RPCSetBinocularActivation(bool binAct){
		isBinocularOn = binAct;
	}

	void BinocularActivationTrigger(){
		networkView.RPC("RPCBinocularActivationTrigger", RPCMode.Others);
	}
	
	[RPC] void RPCBinocularActivationTrigger(){
		if (isBinocularOn){
			binocularRenderPlane.SetActive(true);
			binocualrMask.SetActive(true);
			Server.farClipPlane = 5f;
			FrontTop.farClipPlane = 5f;
			FromBottom.farClipPlane = 5f;
			RightTop.farClipPlane = 5f;
			RightBottom.farClipPlane = 5f;
			LeftTop.farClipPlane = 5f;
			LeftBottom.farClipPlane = 5f;
			FloorTop.farClipPlane = 5f;
			FloorBottom.farClipPlane = 5f;
			//BinocularCamera.farClipPlane = 5f;
		}
		else{
			binocularRenderPlane.SetActive(false);
			binocualrMask.SetActive(false);
			Server.farClipPlane = 1000f;
			FrontTop.farClipPlane = 1000f;
			FromBottom.farClipPlane = 1000f;
			RightTop.farClipPlane = 1000f;
			RightBottom.farClipPlane = 1000f;
			LeftTop.farClipPlane = 1000f;
			LeftBottom.farClipPlane = 1000f;
			FloorTop.farClipPlane = 1000f;
			FloorBottom.farClipPlane = 1000f;
			//BinocularCamera.farClipPlane = 1000f;
		}
	}

	void BinocularActivationSwap(){
		networkView.RPC("RPCBinocularActivationSwap", RPCMode.Others);
	}

	[RPC] void RPCBinocularActivationSwap(){
		isBinocularOn = !isBinocularOn;
		if (isBinocularOn){
			binocularRenderPlane.SetActive(true);
			binocualrMask.SetActive(true);
			Server.farClipPlane = 5f;
			FrontTop.farClipPlane = 5f;
			FromBottom.farClipPlane = 5f;
			RightTop.farClipPlane = 5f;
			RightBottom.farClipPlane = 5f;
			LeftTop.farClipPlane = 5f;
			LeftBottom.farClipPlane = 5f;
			FloorTop.farClipPlane = 5f;
			FloorBottom.farClipPlane = 5f;
			//BinocularCamera.farClipPlane = 5f;
		}
		else{
			binocularRenderPlane.SetActive(false);
			binocualrMask.SetActive(false);
			Server.farClipPlane = 1000f;
			FrontTop.farClipPlane = 1000f;
			FromBottom.farClipPlane = 1000f;
			RightTop.farClipPlane = 1000f;
			RightBottom.farClipPlane = 1000f;
			LeftTop.farClipPlane = 1000f;
			LeftBottom.farClipPlane = 1000f;
			FloorTop.farClipPlane = 1000f;
			FloorBottom.farClipPlane = 1000f;
			//BinocularCamera.farClipPlane = 1000f;
		}
	}
}
