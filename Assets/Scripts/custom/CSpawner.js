#pragma strict

/*
	Variables:
	- Boolean to decide when to show the error message to user
	- Boolean to drop the crates in testing form
	- Crate Prefab that will be used to clone the other crates
	- Hand object that will represent the wand
	- Maximum distance to check for beacons around the wand's intersection point
*/
private var showMessage : boolean = false;
//public var testingDrop : boolean = true;
public var cratePrefab : GameObject;
public var hand : GameObject;
public var distance : float;

public var dropTime : float;

private var startCrateSpawn = false;
static  var crateNumber = 0;
private  var planeNumber = 0;
public  var numberOfPlanes = 0;

//time between dropping each crate
public var dropRate = 0.0 ;
//time between each round of drops
public var planeRate = 0.0 ;
private var nextDrop = 0.0;
private var nextPlane = 0.0;

public var PositionBias : Vector3 ;

//private var beaconArray = new Array ();
public var beaconArray : GameObject[];
public var Crates : GameObject[];
public var CratesInstances : GameObject[];
private var CratesInstancesOld : GameObject[];

private var cratesLog : int[];
private var cratesExplodeLog : int[];

private var debugger : Logger;

function spawnCrateWithTimer(){
	if ((Time.time > nextPlane)&&(planeNumber < numberOfPlanes)){
		
		if ((Time.time > nextDrop)&&(crateNumber < beaconArray.length)){
			
			nextDrop = Time.time + dropRate;
			
						
			var rndExpode = Random.Range(0.0F, 10.0F);
		    var doExplode : boolean;
		    var intExplode : int;
		    if (rndExpode>=5.0){
		    	doExplode = false; intExplode=0;}
		    else {
		    	doExplode = true; intExplode =1;}
		    	
		    while (cratesExplodeLog[intExplode]>=((numberOfPlanes*6)/2)){
		    	rndExpode = Random.Range(0.0F, 10.0F);
			    if (rndExpode>=5.0) {doExplode = false; intExplode=0;}
			    else 			   	{doExplode = true; intExplode=1;}
		    }
		    cratesExplodeLog[intExplode]+=1;
		    
			
			var rnd = Random.Range(0.0F, 10.0F);
			var rndInt : int =  rnd;
			while (cratesLog[rndInt]>= ((numberOfPlanes*6)/10)){
				rnd = Random.Range(0.0F, 10.0F);
				rndInt =  rnd;
			}
			
			var currentBeacon : String = " ---- ";
			if (crateNumber==0)  currentBeacon = " Green ";
			else if (crateNumber==1) currentBeacon = " Blue ";
			else if (crateNumber==2) currentBeacon = " Yellow ";
			else if (crateNumber==3) currentBeacon = " Purple ";
			else if (crateNumber==4) currentBeacon = " Cyan ";
			else if (crateNumber==5) currentBeacon = " Red ";
			
			currentBeacon = ((planeNumber*6)+crateNumber+1).ToString() + currentBeacon;
			
			if (doExplode) currentBeacon = currentBeacon + "- Explode - ";
			else currentBeacon = currentBeacon + "- ------- - ";

			PositionBias.y = 0f;
			if ((rnd<=10F)&&(rnd>=9.75F)){
			    PositionBias.x = 5.7; PositionBias.z = 5.7;		cratesLog[9]+=1;
			    debugger.setText( currentBeacon  + "Outside");}
			else if (rnd>=9.50F){
			    PositionBias.x = 5.7; PositionBias.z = -5.7;	cratesLog[9]+=1;
			    debugger.setText( currentBeacon  + "Outside");}
			else if (rnd>=9.25F){
			    PositionBias.x = -5.7; PositionBias.z = 5.7;	cratesLog[9]+=1;
			    debugger.setText( currentBeacon  + "Outside");}
			else if (rnd>=9.0F){
			    PositionBias.x = -5.7; PositionBias.z = -5.7;	cratesLog[9]+=1;
			    debugger.setText( currentBeacon  + "Outside");} 
			else if (rnd>=8.0F){
			    PositionBias.x = -3.5; PositionBias.z = 3.5;		cratesLog[8]+=1;
			    debugger.setText( currentBeacon  + "8");}
			else if (rnd>=7.0F){
			    PositionBias.x = 3.5; PositionBias.z = 3.5;	cratesLog[7]+=1;
			    debugger.setText( currentBeacon  + "7");}
			else if (rnd>=6.0F){
			    PositionBias.x = 3.5; PositionBias.z = -3.5;	cratesLog[6]+=1;
			    debugger.setText( currentBeacon  + "6");}
			else if (rnd>=5.0F){
			    PositionBias.x = -3.5; PositionBias.z = -3.5;	cratesLog[5]+=1;
			    debugger.setText( currentBeacon  + "5");}		        
			else if (rnd>=4.0F){
			    PositionBias.x = -2.1; PositionBias.z = 2.1;		cratesLog[4]+=1;
			    debugger.setText( currentBeacon  + "4");}
			else if (rnd>=3.0F){
			    PositionBias.x = 2.1; PositionBias.z = 2.1;	cratesLog[3]+=1;
			    debugger.setText( currentBeacon  + "3");}
			else if (rnd>=2.0F){
			    PositionBias.x = 2.1; PositionBias.z = -2.1;	cratesLog[2]+=1;
			    debugger.setText( currentBeacon  + "2");}
			else if (rnd>=1.0F){
			    PositionBias.x = -2.1; PositionBias.z = -2.1;	cratesLog[1]+=1;
			    debugger.setText( currentBeacon  + "1");}    
			else if (rnd>=0.0F){
			    PositionBias.x = 0.0; PositionBias.z = -0.0;	cratesLog[0]+=1;
			    debugger.setText( currentBeacon  + "0");}   
			else
			    Debug.LogError ("Incorrect Drop Position");
			
			
			//Debug.Log("["+cratesLog[0]+","+cratesLog[1]+","+cratesLog[2]+","+cratesLog[3]+","+cratesLog[4]+","+cratesLog[5]+","+
			//		  cratesLog[6]+","+cratesLog[7]+","+cratesLog[8]+","+cratesLog[9]+"]");
			

			
			//Debug.Log("["+cratesExplodeLog[0]+","+cratesExplodeLog[1]+"]");
			//Debug.Log(Time.time);
			//Debug.Log("__________________________________________________");
			
			var cratePosition : Vector3;
			cratePosition = Vector3(beaconArray[crateNumber].transform.position.x - 30, 
								beaconArray[crateNumber].transform.position.y + 100, 
								beaconArray[crateNumber].transform.position.z);
								
			//createCrate(cratePosition, beaconArray[crateNumber].transform.position + PositionBias, dropTime, false);
			
			ReSpawnCrate(Crates[crateNumber], cratePosition, beaconArray[crateNumber].transform.position, PositionBias, dropTime, doExplode);
			
			//ReSpawnCrate(CratesInstancesOld[crateNumber], cratePosition, beaconArray[crateNumber].transform.position, PositionBias, dropTime, false);
			ReSpawnCrate(CratesInstances[crateNumber], cratePosition, beaconArray[crateNumber].transform.position, PositionBias, dropTime, doExplode);
					
			//Debug.LogWarning("PositionBias["+crateNumber+"]:  "+PositionBias);
			crateNumber += 1;
		}
		if (crateNumber >= beaconArray.length){
			//startCrateSpawn = false;
			crateNumber = 0;
			nextDrop = 0;
			
			nextPlane = Time.time + planeRate;
			planeNumber +=1;
			debugger.setText( "___________________________");
		}
	}
}


function ReSpawnCrate(crate: GameObject, cPosition : Vector3, bPosition : Vector3,  offTarget : Vector3, newTime : float, destroyImpact : boolean) {
	//debugger = gameObject.Find("CAVE Mono").GetComponent(Logger);
	crate.SetActive( true);
	crate.transform.position = cPosition;
	
	crate.networkView.RPC("setVariables", RPCMode.AllBuffered, bPosition, offTarget, newTime, destroyImpact, "Crate");
	//if (crateNumber == 0)
	//	crate.networkView.RPC("startCrate", RPCMode.AllBuffered, true);
	//else
	crate.networkView.RPC("startCrate", RPCMode.AllBuffered, false);
	
	//var newCrate : GameObject = Network.Instantiate(cratePrefab, cPosition, Quaternion.identity, 0);
	//newCrate.GetComponent(Crate).networkView.RPC("setVariables", RPCMode.AllBuffered, bPosition, time, destroy, "Crate");
	//newCrate.GetComponent(Crate).networkView.RPC("startCrate", RPCMode.AllBuffered);
}


function spawnAllCrates(){
	startCrateSpawn = true;
	crateNumber = 0;
	nextDrop = 0;
	nextPlane = 0;
	planeNumber = 0;
	
	debugger.setText( "####################################################");

	for (var i=0; i<10; i++)
		cratesLog[i] = 0;
	for (var j=0; j<2; j++)
		cratesExplodeLog[j] = 0;
		
	/*
	if (CratesInstancesOld == null){
		CratesInstancesOld = new GameObject[Crates.Length];
		for (var i=0; i<Crates.Length; i++){
			CratesInstancesOld[i] = GameObject.Find( "CratesInstance"+i.ToString()	);
			
			//Crates[i].GetComponent<Halo>().enabled = false;
			//Behaviour crateBehaviour = (Behaviour)GetComponent("Halo");
			//Behaviour crateBehaviour = Crates[i].(Behaviour)GetComponent("Halo");
			//crateBehaviour.enabled = true;
			//Crates[i].GetComponent("Halo").enabled=false;
			//Crates[i].GetComponent(Halo).enabled=false;
			
			//Component halo = Crates[i].GetComponent("Halo"); 
			//halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
			
			//Crates[i].GetComponent<Crate>().GetComponent("Halo").enabled = false;
			
			//var aaa:Crate ;
			//aaa = Crates[i].GetComponent(Crate);
			
			//Crates[i].GetComponent(Crate).Getcomponent("Halo").enabled = false;
			
		}
	}
	*/
	//beaconArray = GameObject.FindGameObjectsWithTag("Beacon");
	
	
	
	//for(var i : int = 0; i < beaconArray.length; i++)
	//	beaconArray.Pop();
	//beaconArray.Clear();
	
	//for(beacon in GameObject.FindGameObjectsWithTag("Beacon")) {
	//	beaconArray.Push (beacon);
	//}
}

function OnEnable()
{
    WandEventManager.OnTrigger += spawnAllCrates;
}

function Start () {
	cratesLog =  new Array (10);
	for (var i=0; i<10; i++)
		cratesLog[i] = 0;
		
	cratesExplodeLog =  new Array (2);
	for (var j=0; j<2; j++)
		cratesExplodeLog[j] = 0;
		
	debugger = gameObject.Find("CAVE Mono").GetComponent(Logger);
}

function Update () {	
	if(Input.GetKeyUp(KeyCode.Space)) {
		//if(testingDrop) {
		spawnAllCrates();
		//} else {
		//	spawnCrate();
		//}
	}
	if (startCrateSpawn){
		spawnCrateWithTimer();
	}
}

/*
	Update Distance:
	- Takes a new float that represents the new maximum distance to check for beacons
*/
function updateDistance(newDistance : float) {
	distance = newDistance;
}

/*
	Spawn New Crate:
	- Shoots a ray from the wand straight into the world to find the intersection point
	- Finds the closest beacon to the intersection point and creates a new beacon
	- If no beacon is found, tell the user to re-select a location
*/
/*
function spawnCrate() {
	var position = drawHandRay();
	var cratePosition : Vector3;
	var beaconPosition : Vector3;
	var shortestDistance : float = Mathf.Infinity;
	
	for(beacon in GameObject.FindGameObjectsWithTag("Beacon")) {
		var newDistance = Mathf.Abs(Vector3.Distance(beacon.transform.position, position));
		
		if(newDistance <= distance) {
			if(newDistance <= shortestDistance) {
				shortestDistance = newDistance;
				beaconPosition = beacon.transform.position;
				cratePosition = Vector3(beacon.transform.position.x - 30, beacon.transform.position.y + 200, beacon.transform.position.z);
			}
		}
	}
	
	if(cratePosition == null) {
		Invoke("toggleMessage", 5);
		return;
	}
	
	createCrate(cratePosition, beaconPosition, dropTime, false);
}
*/
/*

function spawnAllCratesAtSameTime() {
	var cratePosition : Vector3;

	for(beacon in GameObject.FindGameObjectsWithTag("Beacon")) {
		cratePosition = Vector3(beacon.transform.position.x - 30, beacon.transform.position.y + 100, beacon.transform.position.z);
		createCrate(cratePosition, beacon.transform.position, dropTime, false);
	}
}
*/
/*
	Inner Function for spawnCrate:
	- Instantiates a new crate from the cratePrefab given a position, certain time of descent, 
	  and whether to destroy on impact or not
	- Tags the object with "Crate" to search for it later
*/
/*
function createCrate(cPosition : Vector3, bPosition : Vector3, time : float, destroy : boolean) {
	//debugger = gameObject.Find("CAVE Mono").GetComponent(Logger);
	
	var newCrate : GameObject = Network.Instantiate(cratePrefab, cPosition, Quaternion.identity, 0);
	newCrate.GetComponent(Crate).networkView.RPC("setVariables", RPCMode.AllBuffered, bPosition, time, destroy, "Crate");
	newCrate.GetComponent(Crate).networkView.RPC("startCrate", RPCMode.AllBuffered);
}
*/
/*
	Draw Wand Ray:
	- Shoots a ray straight into the world and finds the intersection point
	- Returns this position to spawnCrate
*/
/*
function drawHandRay() : Vector3 {
	var ray : Ray = new Ray(hand.transform.localPosition, hand.transform.forward);
	var target : RaycastHit;
	var intersect : Vector3;
	
	if(Physics.Raycast(ray, target, Mathf.Infinity)) {		
		intersect = Vector3(target.point.x, target.point.y, target.point.z);
	}
	
	return intersect;
}
*/
/*
	Toggle Message Boolean:
	- Changes the showMessage boolean to either true or false
*/
function toggleMessage() {
	showMessage = !showMessage;
}

/*
	On GUI Call:
	- If showMessage is true, then show the user the error message
*/
function OnGUI() {
	if(showMessage) {
		GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 200f, 200f), "Please re-select a point.");
	}
}