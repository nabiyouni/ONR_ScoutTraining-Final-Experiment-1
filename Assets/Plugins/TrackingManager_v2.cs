using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;


public class TrackingManager_v2 : MonoBehaviour 
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct GenericTracker
	{
		// float[6]
		[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6)]
		public float[] data;
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct WandTracker
	{
		// float[6]
		[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 6)]
		public float[] data;
		
		// float[2]
		[MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
		public float[] joystick;
		
		/// unsigned char
		public byte buttons;
	}
	
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SensorData
	{
		public long timestamp; 
		public GenericTracker head;
		public WandTracker wand;
		
		// generics[1]
		[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 1)]
		public GenericTracker[] generics;
	}
	
	
	public struct ARObject {
		public GameObject obj;
		public Vector3 position;
		public Quaternion orientation;
		public Vector3 PosNowLastFrame;
		public Vector3 PosThenLastFrame;
		
		public Vector3 OriNowLastFrame;
		public Vector3 OriThenLastFrame;
	}
	
	public struct PositionOrientation {
		public Vector3 position;
		public Quaternion orientation;
	}
	
	
	[DllImport ("dtkPlugin")]
	private static extern void init(bool includeHand);
	
	[DllImport ("dtkPlugin")]
	private static extern void getData(long ms, IntPtr pSensorData);
	
	private long now;
	private IntPtr pData;
	private Matrix4x4 rotMatrix;
	private Matrix4x4 flipMatrix;
	
	private SensorData trackingDataNow;
	private SensorData trackingDataRealWorld;
	private SensorData trackingDataCam;
	private SensorData trackingDataAR;
	
	private Vector3 latency;
	private SliderAndTextControl headSlider;
	private SliderAndTextControl handSlider;
	private SliderAndTextControl ARSlider;
	
	private SliderAndTextControl BCFOVSlider;
	
	private Vector3 caveCenterOffset;
	private Vector3TextControl caveOffsetControls;
	
	private float trackingScalingFactor;
	private SliderAndTextControl trackingScalingSlider;
	
	private float turnSensitivity;
	private SliderAndTextControl turnSensitivitySlider;
	
	private byte wandButtons;
	
	private Logger logger;
	private GameObject cave;
	private GameObject tempNow;
	private GameObject tempHead;
	
	public GameObject worldRefNow;
	public GameObject worldRefThen;
	
	public GameObject headNow;
	private GameObject headNowLastFrame;
	private Vector3 headNowLastFramePos;
	private GameObject headThen;
	private GameObject headThenLastFrame;
	
	public List<Vector3> headPrevPos;
	public List<Quaternion> headPrevOri;
	
	public GameObject head;
	public GameObject world;
	public GameObject wand;
	//public String[] ARObjTags;
	//public GameObject[] ARPrefabs;
	public GameObject[] ARBeacons;
	//public GameObject[] ARTargets;
	public GameObject[] ARCrates;
	public GameObject[] ARCratesInstances;
	
	public ARObject[] ARBeaconsInstances;
	//public ARObject[] ARTargetsInstances;
	public ARObject[] ARCratesInstancesARObj;
	public GameObject crateInstancePrefab;
	private GameObject worldInstance;
	
	//public GameObject ARContainer;
	//public GameObject ARContainer2;
	//private GameObject ARMaster;
	//private GameObject ARMaster2;
	
	public delegate void wandData(wandEvent e);
	public static event wandData OnWandUpdate;

	//Amount of AR and RW latency:
	public float minRWLatency;
	public float lowRWLatency;
	public float highRWLatency;
	public float minARLatency;
	public float lowARLatency;
	public float highARLatency;

	private float sensitivityX = 0.8F;
	private float sensitivityY = 0.8F;
	
	private float sensitivityKeyboardX = 0.5f;
	private float sensitivityKeyboardY = 0.5f;
	private float sensitivityKeyboardZ = 0.5f;

	private float minimumY = -60F;
	private float maximumY = 60F;
	
	float rotationX = 0F;
	float rotationY = 0F;
	float rotationX_old = 0F;
	float rotationY_old = 0F;
	//float rotationX_offset = 45F;
	//float rotationY_offset = 0F;
	
	Quaternion originalRotation;
	Quaternion Head_originalRotation;
	Quaternion Binocular_originalRotation;
	
	public Camera BinocularCamera;
	
	float FPS = 0f;
	int Fcount = 0;
	
	// Use this for initialization
	void Start () 
	{ 
		cave = GameObject.Find("CAVE Mono");
		
		logger = (Logger) gameObject.GetComponent("Logger");
		//logger.setText("Attempting to load tracker plugin");
		
		//Our plugin only exists for a linux machine
		if (Application.platform == RuntimePlatform.LinuxPlayer)
		{
			init(false);
			pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(SensorData)));
			
			//logger.setText("Tracker plugin initialized");
		}
		
		headSlider = new SliderAndTextControl();
		handSlider = new SliderAndTextControl();
		ARSlider = new SliderAndTextControl();
		
		BCFOVSlider = new SliderAndTextControl();
		
		//The cave origin is x = 0, y = 1.5m, z = 0
		caveCenterOffset.y = 1.5f;
		caveOffsetControls = new Vector3TextControl("0", "1.5", "0");
		
		//World scale is in meters
		trackingScalingFactor = 1.6f;
		trackingScalingSlider = new SliderAndTextControl("1.6");
		
		turnSensitivitySlider = new SliderAndTextControl();
		
		//Conversion Matrix for tracker Z-up configuration to Unity Y-up
		rotMatrix = new Matrix4x4();
		rotMatrix.SetRow(0, new Vector4(1, 0,  0, 0));
		rotMatrix.SetRow(1, new Vector4(0, 0, -1, 0));
		rotMatrix.SetRow(2, new Vector4(0, 1,  0, 0));
		rotMatrix.SetRow(3, new Vector4(0, 0,  0, 1));
		
		flipMatrix = new Matrix4x4();
		flipMatrix.SetRow(0, new Vector4(1, 0,  0, 0));
		flipMatrix.SetRow(1, new Vector4(0, 1,  0, 0));
		flipMatrix.SetRow(2, new Vector4(0, 0, -1, 0));
		flipMatrix.SetRow(3, new Vector4(0, 0,  0, 1));
		
		ARBeaconsInstances = new ARObject[ARBeacons.Length];
		for (int i=0; i< ARBeacons.Length; i++) {
			ARBeaconsInstances[i].obj = (GameObject) Instantiate(ARBeacons[i]);
			ARBeaconsInstances[i].obj.renderer.material = new Material (Shader.Find("Transparent/Diffuse"));
			ARBeaconsInstances[i].obj.renderer.material.mainTexture = null;
			ARBeaconsInstances[i].obj.renderer.material.color = new Color(0.0F, 0.0F, 0.0F, 0.5F);
			
			ARBeaconsInstances[i].position = ARBeacons[i].transform.localPosition;
			ARBeaconsInstances[i].orientation = ARBeacons[i].transform.localRotation;
			ARBeaconsInstances[i].PosNowLastFrame = ARBeacons[i].transform.localPosition;
			ARBeaconsInstances[i].PosThenLastFrame = ARBeacons[i].transform.localPosition;
			ARBeaconsInstances[i].OriNowLastFrame = new Vector3(0f,0f,0f);
			ARBeaconsInstances[i].OriThenLastFrame = new Vector3(0f,0f,0f);
			
			ARBeaconsInstances[i].obj.renderer.enabled = false;
			
		}
		/*
		ARTargetsInstances = new ARObject[ARTargets.Length];
		for (int i=0; i< ARTargets.Length; i++) {
			ARTargetsInstances[i].obj = (GameObject) Instantiate(ARTargets[i]);
			ARTargetsInstances[i].obj.renderer.material = new Material (Shader.Find("Transparent/Diffuse"));
			ARTargetsInstances[i].obj.renderer.material.mainTexture = null;
			ARTargetsInstances[i].obj.renderer.material.color = new Color(0.0F, 0.0F, 0.0F, 0.5F);
			
			ARTargetsInstances[i].position = ARTargets[i].transform.localPosition;
			ARTargetsInstances[i].orientation = ARTargets[i].transform.localRotation;
			ARTargetsInstances[i].PosNowLastFrame = ARTargets[i].transform.localPosition;
			ARTargetsInstances[i].PosThenLastFrame = ARTargets[i].transform.localPosition;
			ARTargetsInstances[i].OriNowLastFrame = new Vector3(0f,0f,0f);
			ARTargetsInstances[i].OriThenLastFrame = new Vector3(0f,0f,0f);
			
			ARTargetsInstances[i].obj.renderer.enabled = false;
			foreach (Transform part in ARTargetsInstances[i].obj.transform.GetComponentsInChildren<Transform>())
				part.renderer.enabled = false;
		}
		*/
		/*
		ARCratesInstancesARObj = new ARObject[ARCrates.Length];
		for (int i=0; i< ARCrates.Length; i++) {
			//var newCrate : GameObject = Network.Instantiate(crateInstancePrefab, cPosition, Quaternion.identity, 0);

			//ARCratesInstancesARObj[i].obj = (GameObject) Instantiate(ARCrates[i]);
			//ARCratesInstancesARObj[i].obj = (GameObject) Network.Instantiate(crateInstancePrefab, ARCrates[i].transform.position, 
			//                                               ARCrates[i].transform.rotation, 0);

			ARCratesInstancesARObj[i].obj = (GameObject)Instantiate(crateInstancePrefab);
			//ARCratesInstancesARObj[i].obj.transform.position = ARCrates[i].transform.position;
			ARCratesInstancesARObj[i].obj.transform.rotation = ARCrates[i].transform.rotation;
			//Debug.Log(ARCrates[i].transform.position);
			Vector3 test = ARCrates[i].transform.position;
			test.y += 10f;
			ARCratesInstancesARObj[i].obj.transform.position = test;

			ARCratesInstancesARObj[i].obj.name = "CratesInstance"+i.ToString();
			//ARCratesInstancesARObj[i].obj.renderer.material = new Material (Shader.Find("Transparent/Diffuse"));
			//ARCratesInstancesARObj[i].obj.renderer.material.mainTexture = null;
			//ARCratesInstancesARObj[i].obj.renderer.material.color = new Color(0.0F, 0.0F, 0.0F, 0.5F);
			ARCratesInstancesARObj[i].obj.SetActive(true);

			//ARCratesInstancesARObj[i].obj.(Behaviour)GetComponent("Halo")
			//Behaviour crateBehaviour = (Behaviour)GetComponent("Halo");
			//crateBehaviour.enabled = false;

			ARCratesInstancesARObj[i].position = ARCrates[i].transform.localPosition;
			//ARCratesInstancesARObj[i].obj.transform.position = ARCratesInstancesARObj[i].position;
			ARCratesInstancesARObj[i].orientation = ARCrates[i].transform.localRotation;
			ARCratesInstancesARObj[i].PosNowLastFrame = ARCrates[i].transform.localPosition;
			ARCratesInstancesARObj[i].PosThenLastFrame = ARCrates[i].transform.localPosition;
			ARCratesInstancesARObj[i].OriNowLastFrame = new Vector3(0f,0f,0f);
			ARCratesInstancesARObj[i].OriThenLastFrame = new Vector3(0f,0f,0f);
			
			//ARCratesInstancesARObj[i].obj.renderer.enabled = false;
		}
		*/
		worldInstance = new GameObject ("world Instance");//(GameObject) Instantiate(world);
		worldInstance.transform.position = world.transform.position;
		worldInstance.transform.rotation = world.transform.rotation;
		
		//world.transform.SetParent (head.transform, true);
		
		//Head for calculating AR transforms
		tempNow = new GameObject("Now");
		tempNow.transform.SetParent(cave.transform, true);
		
		tempHead = new GameObject("Temp Head");
		tempHead.transform.SetParent(cave.transform, true);
		
		headNowLastFrame = new GameObject("head Now Last Frame");
		headNowLastFrame.transform.SetParent(cave.transform, true);
		headNowLastFrame.transform.localPosition = headNow.transform.localPosition;
		headNowLastFrame.transform.localRotation = headNow.transform.localRotation;
		
		headThen = new GameObject("head Then");
		headThen.transform.SetParent(cave.transform, true);
		headThen.transform.localPosition = headNow.transform.localPosition;
		headThen.transform.localRotation = headNow.transform.localRotation;
		
		headThenLastFrame = new GameObject("head Then Last Frame");
		headThenLastFrame.transform.SetParent(cave.transform, true);
		headThenLastFrame.transform.localPosition = headNow.transform.localPosition;
		headThenLastFrame.transform.localRotation = headNow.transform.localRotation;
		
		
		////////////////////////////////////////////////////////////////////////////
		/// Mahdi: Test:
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		
		originalRotation = transform.localRotation;
		Head_originalRotation = head.transform.localRotation;
		//Binocular_originalRotation = binocular.transform.localRotation;
		
		headPrevPos = new List<Vector3> ();
		headPrevOri = new List<Quaternion> ();
		
		//ARMaster = (GameObject) Instantiate(ARContainer);
		//ARMaster.renderer.material = new Material (Shader.Find("Transparent/Diffuse"));
		//ARMaster.renderer.material.mainTexture = null;
		//ARMaster.renderer.material.color = new Color(0.0F, 1.0F, 0.0F, 0.5F);
		
		//ARMaster2 = (GameObject) Instantiate(ARContainer2);
		//ARMaster2.renderer.material = new Material (Shader.Find("Transparent/Diffuse"));
		//ARMaster2.renderer.material.mainTexture = null;
		//ARMaster2.renderer.material.color = new Color(0.0F, 1.0F, 0.0F, 0.5F);
	}
	
	void OnGUI() 
	{
		GUI.BeginGroup(new Rect(10, 10, 270, 190));
		GUI.Box(new Rect(0, 0, 270, 190), "Latency Controls");
		latency = latencySlider (new Rect (10,30,200,30), latency);
		GUI.EndGroup();
		
		GUI.BeginGroup(new Rect(10, 200, 270, 90));
		GUI.Box(new Rect(0, 0, 270, 90), "Binocular Control");
		BinocularCamera.fieldOfView = BinocularCamFOVSlider (new Rect (10,30,200,30), BinocularCamera.fieldOfView);
		GUI.EndGroup();
		
		GUI.BeginGroup(new Rect(10, Screen.height - 250, 270, 240));
		GUI.Box(new Rect(0, 0, 270, 240), "CAVE Calibration");
		caveCenterOffset = caveOffsetControls.CreateControl (new Rect (10,30,270,30), caveCenterOffset, "CAVE Origin Offset");
		trackingScalingFactor = trackingScalingSlider.CreateControl (new Rect (10,90,200,30), trackingScalingFactor, 5.0f, "Tracker Scaling", "");
		turnSensitivity = turnSensitivitySlider.CreateControl (new Rect (10,140,200,30), turnSensitivity, 10.0f, "Turn Sensitivity", "");
		if (GUI.Button (new Rect (10,200,250,30), "Get Tracker Snapshot")) {
			string headTransform = "head: " + transformToString(head.transform);
			//string wandTransform = "wand: " + transformToString(wand.transform);
			
			logger.setText("-----------");
			//logger.setText(wandTransform);
			logger.setText(headTransform);
		}
		GUI.EndGroup();
	}
	
	// Update is called once per frame
	void Update () 
	{
		TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
		now = (long)t.TotalMilliseconds;

		if (Application.platform == RuntimePlatform.LinuxPlayer)
		{
			try
			{	
				//logger.setText(" 1 ");
				getData((now), pData);
				trackingDataNow = (SensorData)Marshal.PtrToStructure(pData, typeof(SensorData));
				
				ZupToYup(worldRefNow, trackingDataNow.head.data);
				ZupToYup(headNow, trackingDataNow.head.data);
				
				ZupToYup(tempNow, trackingDataNow.head.data, true);
				if(OnWandUpdate != null)
				{
					var eventArgs = new wandEvent(trackingDataNow.wand.joystick, trackingDataNow.wand.buttons);
					OnWandUpdate(eventArgs);
				}
				//logger.setText(" 2 ");
				getData((now - (long)latency.x), pData);
				trackingDataRealWorld = (SensorData)Marshal.PtrToStructure(pData, typeof(SensorData));
				//logger.setText(" 3 ");
				
				//Mahdi: here we are mapping the tracking data to user's head, wand and binocular
				//ZupToYup(head, trackingDataRealWorld.head.data, true);
				//ZupToYup(head, trackingDataRealWorld.head.data);
				ZupToYup(worldRefThen, trackingDataRealWorld.head.data);
				// we are not going to use the wand anymore
				//ZupToYup(wand, trackingDataRealWorld.wand.data);
				
				//##############################################################################3
				//Mahdi: world latency
				pastARTransformWorld (world, worldInstance, worldRefNow, worldRefThen);
				//##############################################################################3
				
				//logger.setText(" 4 ");
				getData((now - (long)latency.y), pData);
				trackingDataCam = (SensorData)Marshal.PtrToStructure(pData, typeof(SensorData));
				
				
				getData((now - (long)latency.z), pData);
				
				trackingDataAR = (SensorData)Marshal.PtrToStructure(pData, typeof(SensorData));
				//logger.setText(" 5 ");
				
				//ZupToYup(tempHead, trackingDataAR.head.data);
				ZupToYup(headThen, trackingDataAR.head.data);
				
				ZupToYup(tempHead, trackingDataAR.head.data, true);
				//pastARTransform(ARContainer, tempNow, tempHead);
				//pastARTransform2(ARContainer2, tempNow, tempHead);
				
				
				
				/*
				for (int i=0; i< ARTargets.Length; i++) {
					pastARTransformAR (ARTargets[i], ARTargetsInstances[i], headNow, headThen);
				}
				*/

				if (latency.z > 5.0){
					for (int i=0; i< ARBeacons.Length; i++) {
						pastARTransformAR (ARBeacons[i], ARBeaconsInstances[i], headNow , headThen);
					}
					for (int i=0; i< ARCrates.Length; i++) {
						pastARTransform(ARCrates[i], ARCratesInstances[i] , headNow, headThen);
					}
				}
				else{
					for (int i=0; i< ARBeacons.Length; i++) {
						pastARTransformAR (ARBeacons[i], ARBeaconsInstances[i], headNow , headNow);
					}
					for (int i=0; i< ARCrates.Length; i++) {
						pastARTransform(ARCrates[i], ARCratesInstances[i] , headNow, headNow);
					}
				}
				
				
			}
			catch
			{
				logger.setText("Error with getting tracker data!");
			}
		}
		else{
			try{
				//Mahdi: here we are mapping the tracking data to user's head, wand and binocular
				
				ZupToYup_Test (head, Head_originalRotation);
				ZupToYup_Test (headThen, Head_originalRotation);
				ZupToYup_Test (headNow, Head_originalRotation);
				
				//Debug.Log("world"+world.transform.position);
				//Debug.Log("world"+world.transform.localPosition);
				//Debug.Log("worldInstance"+worldInstance.transform.position);
				//Debug.Log("worldInstance"+worldInstance.transform.localPosition);
				//Debug.Log("headNow"+headNow.transform.position);
				//Debug.Log("headNow"+headNow.transform.localPosition);
				//Debug.Log("headThen"+headThen.transform.position);
				//Debug.Log("headThen"+headThen.transform.localPosition);
				
				
				
				pastARTransformWorld (world, worldInstance, headNow , headThen);

				if (latency.z > 5.0){
					for (int i=0; i< ARBeacons.Length; i++) {
						pastARTransformAR (ARBeacons[i], ARBeaconsInstances[i], headNow , headThen);
					}
					for (int i=0; i< ARCrates.Length; i++) {
						pastARTransform(ARCrates[i], ARCratesInstances[i] , headNow, headThen);
					}
				}
				else{
					for (int i=0; i< ARBeacons.Length; i++) {
						pastARTransformAR (ARBeacons[i], ARBeaconsInstances[i], headNow , headNow);
					}
					for (int i=0; i< ARCrates.Length; i++) {
						pastARTransform(ARCrates[i], ARCratesInstances[i] , headNow, headNow);
					}
				}

				/*
				for (int i=0; i< ARTargets.Length; i++) {
					pastARTransformAR(ARTargets[i], ARTargetsInstances[i] , headNow, headThen);
				}
				*/

				//////////////////////
				/// Mahdi: New Code for new latency method
				/*Debug.Log(Time.deltaTime);
				Debug.Log(1.0f / Time.deltaTime);
				FPS = ((FPS*Fcount) + (1.0f / Time.deltaTime))/(float)(Fcount+1);
				Debug.LogWarning(FPS);
				Fcount++;
				*/
			}
			catch{
				logger.setText("Error with non-Linux platform!");
			}
		}

		if (Input.GetKey(KeyCode.Alpha1)){
			latency.x = minRWLatency;
			latency.z = minARLatency;
		}
		if (Input.GetKey(KeyCode.Alpha2)){
			latency.x = minRWLatency;
			latency.z = lowARLatency;
		}
		if (Input.GetKey(KeyCode.Alpha3)){
			latency.x = lowRWLatency;
			latency.z = minARLatency;
		}
		if (Input.GetKey(KeyCode.Alpha4)){
			latency.x = lowRWLatency;
			latency.z = lowARLatency;
		}
		if (Input.GetKey(KeyCode.Alpha5)){
			latency.x = minRWLatency;
			latency.z = highARLatency;
		}
		if (Input.GetKey(KeyCode.Alpha6)){
			latency.x = highRWLatency;
			latency.z = minARLatency;
		}
		if (Input.GetKey(KeyCode.Alpha7)){
			latency.x = highRWLatency;
			latency.z = highARLatency;
		}

	}
	Vector3 distract = new Vector3 (0, 0, 0);

	void pastARTransform (GameObject arObj, GameObject arInstace, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(arInstace);
		tempObj.transform.SetParent(parentThen.transform, true);
		
		arObj.transform.SetParent(parentNow.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		//arObj.transform.SetParent(parentNow.transform, true);
		
		Destroy(tempObj);
		
	}
	
	void pastARTransformAR (GameObject arObj, ARObject arInstace, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(arInstace.obj);
		tempObj.transform.SetParent(parentThen.transform, true);
		
		arObj.transform.SetParent(parentNow.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		//arObj.transform.SetParent(parentNow.transform, true);
		
		Destroy(tempObj);
		
	}
	
	void pastARTransformARNegative (GameObject arObj, ARObject arInstace, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(arInstace.obj);
		tempObj.transform.SetParent( parentNow.transform, true);
		
		arObj.transform.SetParent(parentThen.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		//arObj.transform.SetParent(parentNow.transform, true);
		
		Destroy(tempObj);
		
	}
	
	void pastARTransformWorld (GameObject arObj, GameObject arInstace, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(arInstace);
		tempObj.transform.SetParent(parentNow.transform, true);
		
		arObj.transform.SetParent(parentThen.transform, true);
		//arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		//arObj.transform.SetParent(parentNow.transform, true);
		
		Destroy(tempObj);
	}
	
	void pastARTransformWorldNegative (GameObject arObj, GameObject arInstace, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(arInstace);
		tempObj.transform.SetParent(parentThen.transform, true);
		
		arObj.transform.SetParent( parentNow.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		//arObj.transform.SetParent(parentNow.transform, true);
		
		Destroy(tempObj);
	}
	
	
	void pastARTransformListPosition (GameObject arObj, ARObject arInstace, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(arInstace.obj);
		tempObj.transform.SetParent(parentNow.transform, true);
		
		arObj.transform.SetParent(parentThen.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		Destroy(tempObj);
		
	}
	
	
	/*void pastARTransform2 (GameObject arObj, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(ARMaster2);
		tempObj.transform.SetParent(parentNow.transform, true);
		
		arObj.transform.SetParent(parentThen.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		Destroy(tempObj);
	}

	void pastARTransform (GameObject arObj, GameObject parentNow, GameObject parentThen) {
		GameObject tempObj = (GameObject) Instantiate(ARMaster);
		tempObj.transform.SetParent(parentNow.transform, true);
		
		arObj.transform.SetParent(parentThen.transform, true);
		arObj.transform.localPosition = tempObj.transform.localPosition;
		arObj.transform.localRotation = tempObj.transform.localRotation;
		arObj.transform.SetParent(null, true);
		
		Destroy(tempObj);
	}*/
	
	void ZupToYup (GameObject obj, float[] transforms, bool posOnly = false, bool applyPosOffsets = true)
	{
		Vector3 t = new Vector3(transforms[0], transforms[1], -transforms[2]);
		Quaternion r = Quaternion.Euler(transforms[3], transforms[4], transforms[5]);
		Vector3 s = new Vector3(1, 1, 1);
		
		Matrix4x4 m = Matrix4x4.TRS(t, r, s);
		m = rotMatrix*m*flipMatrix;
		
		// Extract new local position
		if (applyPosOffsets)
		{
			obj.transform.localPosition = (Vector3)m.GetColumn(3)*trackingScalingFactor + caveCenterOffset;
		}
		else
		{
			obj.transform.localPosition = (Vector3)m.GetColumn(3)*trackingScalingFactor;
		}
		
		// Extract new local rotation
		if (posOnly)
		{
			obj.transform.eulerAngles = Vector3.zero;
		}
		else 
		{
			//obj.transform.localRotation = Quaternion.LookRotation(
			//  	m.GetColumn(2),
			//  	m.GetColumn(1)
			//);
			
			obj.transform.rotation = Quaternion.Euler(-transforms[4], -transforms[3], -transforms[5]);
		}
		
		// Extract new local scale
		obj.transform.localScale = new Vector3 (
			m.GetColumn(0).magnitude,
			m.GetColumn(1).magnitude,
			m.GetColumn(2).magnitude
			);
	} 
	
	void ZupToYup_Test (GameObject obj, Quaternion objOriginialRotation, bool posOnly = false, bool applyPosOffsets = true)
	{
		Vector3 localPos = new Vector3 (0f, 0f, 0f);
		// Extract new local position
		if (applyPosOffsets)
		{
			if (Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.W))
				localPos.z += sensitivityKeyboardZ;
			if (Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S))
				localPos.z += -sensitivityKeyboardZ;
			if (Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D))
				localPos.x +=sensitivityKeyboardX;
			if (Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A))
				localPos.x += -sensitivityKeyboardX;
			if (Input.GetKey(KeyCode.RightBracket)||Input.GetKey(KeyCode.E))
				localPos.y += sensitivityKeyboardY;
			if (Input.GetKey(KeyCode.LeftBracket)||Input.GetKey(KeyCode.Q))
				localPos.y += -sensitivityKeyboardY;
			
			obj.transform.localPosition += localPos;// + obj.transform.localPosition + caveCenterOffset;
		}
		else
		{
			//obj.transform.localPosition = (Vector3)m.GetColumn(3)*trackingScalingFactor;
		}
		
		// Extract new local rotation
		if (posOnly)
		{
			//obj.transform.eulerAngles = Vector3.zero;
			
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
			//rotationX = ClampAngle (rotationX, minimumX, maximumX);
			//rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
			
			obj.transform.rotation = objOriginialRotation * xQuaternion * yQuaternion;
		}
		else 
		{
			/////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////
			//Mahdi: Test:
			// Read the mouse input axis
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
			//rotationX = ClampAngle (rotationX, minimumX, maximumX);
			//rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
			
			obj.transform.rotation = objOriginialRotation * xQuaternion * yQuaternion;
			
			//Debug.Log ("rotationX: "+rotationX.ToString());
			//Debug.Log ("rotationY: "+rotationY.ToString());
			//Debug.Log ("xQuaternion: "+xQuaternion.ToString());
			//Debug.Log ("xQuaternion: "+xQuaternion.ToString());
			//Debug.Log ("rotation: "+obj.transform.rotation.ToString());
			
			/////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////
			
			//obj.transform.rotation = Quaternion.Euler(-transforms[4], -transforms[3], -transforms[5]);
			
			//Mahdi: Test: make the view rotate with mouse for test
			
		}
	}
	
	void ZupToYup_HeadBasedRotate_Test (GameObject obj,Quaternion objOriginialRotation, GameObject pivot, int orientation = 1, bool posOnly = false, bool applyPosOffsets = true)
	{
		Vector3 localPos = new Vector3 (0f, 0f, 0f);
		// Extract new local position
		if (applyPosOffsets)
		{
			if (Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.W))
				localPos.z += 0.02f;
			if (Input.GetKey(KeyCode.DownArrow)||Input.GetKey(KeyCode.S))
				localPos.z += -0.02f;
			if (Input.GetKey(KeyCode.RightArrow)||Input.GetKey(KeyCode.D))
				localPos.x += 0.02f;
			if (Input.GetKey(KeyCode.LeftArrow)||Input.GetKey(KeyCode.A))
				localPos.x += -0.02f;
			if (Input.GetKey(KeyCode.RightBracket)||Input.GetKey(KeyCode.E))
				localPos.y += 0.02f;
			if (Input.GetKey(KeyCode.LeftBracket)||Input.GetKey(KeyCode.Q))
				localPos.y += -0.02f;
			
			obj.transform.localPosition += localPos;// + obj.transform.localPosition + caveCenterOffset;
		}
		else
		{
			//obj.transform.localPosition = (Vector3)m.GetColumn(3)*trackingScalingFactor;
		}
		
		// Extract new local rotation
		if (posOnly)
		{
			Debug.Log ("Pos Only ");
		}
		else 
		{
			/////////////////////////////////////////////////////////////////////////////////////
			/////////////////////////////////////////////////////////////////////////////////////
			//Mahdi: Test:
			// Read the mouse input axis
			rotationX += Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			
			//rotationX = ClampAngle (rotationX, minimumX, maximumX);
			//rotationY = ClampAngle (rotationY, minimumY, maximumY);
			
			Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
			Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, orientation * -Vector3.right);
			
			obj.transform.rotation = objOriginialRotation * xQuaternion * yQuaternion;
			
			float rotationX_delta = (rotationX - rotationX_old);//*sensitivityX;
			float rotationY_delta = (rotationY - rotationY_old);//*sensitivityY;
			
			//rotationX_delta = ClampAngle (rotationX_delta, minimumX, maximumX);
			//rotationY_delta = ClampAngle (rotationY_delta, minimumY, maximumY);
			
			xQuaternion = Quaternion.AngleAxis (rotationX_delta, Vector3.up);
			yQuaternion = Quaternion.AngleAxis (rotationY_delta, -Vector3.right);
			
			obj.transform.position = RotatePointAroundPivot(obj.transform.position, 
			                                                pivot.transform.position, 
			                                                xQuaternion.eulerAngles);
			
			obj.transform.position = RotatePointAroundPivot(obj.transform.position, 
			                                                pivot.transform.position, 
			                                                yQuaternion.eulerAngles);
			
			rotationX_old = rotationX;
			rotationY_old = rotationY;
			
		}
		
	} 
	
	public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
	{
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler (angle) * dir;
		point = dir + pivot;
		
		return point;
	}
	
	public static float Angle(Vector3 vec1, Vector3 vec2){
		
		if ((vec1.magnitude == 0) || (vec2.magnitude == 0))
			return 0f;
		else{
			float dot = Vector3.Dot(vec1,vec2);
			// Divide the dot by the product of the magnitudes of the vectors
			dot = dot/(vec1.magnitude*vec2.magnitude);
			
			if ((dot>1)||(dot<-1))
				return 0f;
			//Get the arc cosin of the angle, you now have your angle in radians 
			float acos = Mathf.Acos(dot);
			//Multiply by 180/Mathf.PI to convert to degrees
			float angle = acos*180/Mathf.PI;
			return angle;
		}
	}
	
	//Creates Binocular Latency
	float BinocularCamFOVSlider (Rect screenRect, float fov) 
	{
		float BC_FOV = BCFOVSlider.CreateControl (screenRect, fov, 179f, "Binocular Camera FOV", "");
		
		return BC_FOV;
	} 
	
	//Creates latency sliders
	Vector3 latencySlider (Rect screenRect, Vector3 latency) 
	{
		latency.x = headSlider.CreateControl (screenRect, latency.x, 2000.0f, "Real World Latency", " ms");
		
		// <- Move the next control down a bit to avoid overlapping
		screenRect.y += 50; 
		latency.y = handSlider.CreateControl (screenRect, latency.y, 2000.0f, "Hand Cam Latency", " ms");
		
		// <- Move the next control down a bit to avoid overlapping
		screenRect.y += 50; 
		latency.z = ARSlider.CreateControl (screenRect, latency.z, 2000.0f, "AR Obj Latency", " ms");
		
		return latency;
	} 
	
	string transformToString (Transform component){
		return component.transform.position.x.ToString("0.0") + ", " + component.transform.position.y.ToString("0.0")
			+ ", " + component.transform.position.z.ToString("0.0") + "| " + component.transform.localEulerAngles.x.ToString("0.0")
				+ ", " + component.transform.localEulerAngles.y.ToString("0.0") + ", " + component.transform.localEulerAngles.z.ToString("0.0");
	}
}
/// <other classes>
/// //////////////////////////////////////////////////////////////////////
/// </summary>

public class SliderAndTextControl 
{     
	private float f;
	private string s;
	
	public string sliderString;
	
	public SliderAndTextControl () {
		this.sliderString = "0";
	}
	
	public SliderAndTextControl (string sSlider) {
		this.sliderString = sSlider;
	}
	
	public float CreateControl (Rect screenRect, float sliderValue, float sliderMaxValue, string labelText, string units) 
	{
		GUI.Label (screenRect, labelText + ": " + sliderValue.ToString("0.0") + units);
		
		// <- Moves the Slider under the label
		screenRect.y += screenRect.height;
		f = GUI.HorizontalSlider (screenRect, sliderValue, 0.0f, sliderMaxValue);
		
		if (f != sliderValue) 
		{
			sliderValue = f;
			sliderString = f.ToString("0.0");
		}
		
		// <- Moves the Input after the slider and makes it smaller
		screenRect.x += screenRect.width + 10;
		screenRect.y -= 4;
		screenRect.width = 40; 
		screenRect.height = 20; 
		s = GUI.TextField(screenRect, sliderString);
		
		if (sliderString != s) 
		{
			sliderString = s;
			
			if (float.TryParse(s, out f)) 
			{
				sliderString = s;
				sliderValue = f;
			}
		}
		
		return sliderValue;
	}
}

public class Vector3TextControl 
{     
	private int w;
	private float f;
	private Rect screenRect2;
	
	public string x, y, z;
	
	public Vector3TextControl () 
	{
		this.x = "0";
		this.y = "0";
		this.z = "0";
	}
	
	public Vector3TextControl (string x, string y, string z) 
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
	
	public Vector3 CreateControl (Rect screenRect, Vector3 values, string labelText) 
	{
		GUI.Label (screenRect, labelText);
		screenRect2 = screenRect;
		
		// <- Moves the text boxes under the label
		screenRect.y += screenRect.height;
		screenRect2.y += screenRect.height;
		w = (int)(screenRect.width-20)/3;
		
		screenRect.x += 5;
		screenRect.width = 15;
		
		screenRect2.x = screenRect.x + screenRect.width + 6;
		screenRect2.width = w - screenRect.width - 20;
		screenRect2.height = 20;
		
		GUI.Label (screenRect, "x:");
		x = GUI.TextField(screenRect2, x);
		
		screenRect.x += w + 5;
		screenRect2.x = screenRect.x + screenRect.width + 6;
		
		GUI.Label (screenRect, "y:");
		y = GUI.TextField(screenRect2, y);
		
		screenRect.x += w + 5;
		screenRect2.x = screenRect.x + screenRect.width + 6;
		
		GUI.Label (screenRect, "z:");
		z = GUI.TextField(screenRect2, z);
		
		if (float.TryParse(x, out f)) 
		{
			values.x = f;
		}
		
		if (float.TryParse(y, out f)) 
		{
			values.y = f;
		}
		
		if (float.TryParse(z, out f)) 
		{
			values.z = f;
		}
		
		return values;
	}
}

public class wandEvent : EventArgs
{
	public float[] Joystick { get; private set; }
	public byte Buttons { get; private set; }
	
	public wandEvent (float[] joystick , byte buttons )
	{
		Joystick = joystick ;
		Buttons = buttons ;
	}
}

