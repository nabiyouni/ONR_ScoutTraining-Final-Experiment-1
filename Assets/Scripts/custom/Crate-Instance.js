 #pragma strict

/*
	Variables:
	- Position Vector3 where the crate will spawn above
	- The time it will take the crate to reach its position
	- The start time of this script, used to calculate each delta change in time
	- Boolean deciding whether or not the crate will be destroyed on impact
*/
private var positionV : Vector3;
private var deltaVector : Vector3;
private var targetVector : Vector3;
private var totalDroptime : float;
private var startTime : float;
private var prevTime : float;
private var collisionTime : float;
public var crateGlowTiming : float;
public var isActualCrate : boolean ;
private var explode : boolean = false;
private var isDropping : boolean = false;
private var doprint : boolean = false;

private var debugger : Logger;

/*
	Start:
	- Set the initial time
*/
function Start () {
	startTime = Time.time;
	

}

/*
	Update (Every frame):
	- If startDrop is true, then drop the crate
*/
function Update () {
	dropCrate();
}


function OnCollisionEnter(collision: Collision) {

	if ((collision.transform.name == "Target")||(collision.transform.name == "NearTerrain")){
		//Debug.Log(transform.name +"   ->  "+ collision.transform.name);
		transform.rotation = Quaternion.identity;
		transform.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		isDropping = false;
		collisionTime = Time.time;
	}
}


/*
	Set Variables:
	- Set the position, time of descent, and explode variables
	- Used by the CSpawner when creating new crates
*/
@RPC
function setVariables(newPosition : Vector3, offTarget : Vector3, newTime : float, destroyImpact : boolean, newTag : String) {
	positionV = newPosition;
	positionV.y = positionV.y + .5;
	
	totalDroptime = newTime;
	
	explode = destroyImpact;
	gameObject.tag = newTag;
	targetVector = positionV;
	targetVector.y -= 50f;
	
	positionV = positionV + offTarget;
}

@RPC
function startCrate(pri:boolean) {
	//positionV = calculateArea(new List.<float>(new List.<float>([.68, .95, .997])));
	//positionV = calculateArea(new List.<float>(new List.<float>([.0, .0, .0])));
	
	//Debug.Log(positionV.x + " " + positionV.y +  " " + positionV.z);
	
	//debugger = gameObject.Find("CAVE Mono").GetComponent(Logger);
	
	transform.active = true;
	
	//deltaVector = (positionV - transform.position);
	deltaVector.x = (positionV.x - transform.position.x);
	deltaVector.z = (positionV.z - transform.position.z);
	deltaVector.y = 0.0;
	
	isDropping = true;
	
	doprint = pri;
	
	startTime = Time.time;
	prevTime = Time.time;
	
	transform.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	
	var halo : Component = GetComponent("Halo"); 
	halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
}

/*
	Drop the Crate:
	- Moves the crate to above the position where it will drop
	- Make the crate visible and start calculating the delta time values
	- Interpolate the crate between its starting position and end position
	- When the crate reaches its end position, if explode is true, then destroy the crate
*/
function dropCrate() {

	if (isDropping){


		//transform.position = Vector3(positionV.x - 30, positionV.y + 200, positionV.z);
		
		
		
		deltaVector.x = (positionV.x - transform.position.x);
		deltaVector.z = (positionV.z - transform.position.z);
		deltaVector.y = 0.0;
		
		var delta = ((Time.time - startTime) / totalDroptime);
		
		var deltaT = (Time.time - prevTime);
		var remainigTime = totalDroptime - (Time.time - startTime);
		
		if (remainigTime == 0)
			remainigTime = 0.001;
		
	
		
		//transform.position = Vector3.Lerp(transform.position, positionV, delta);
		
		// Using translate function
	//	if(transform.position.x < positionV.x) {
	//		transform.Translate(deltaVector * (Time.deltaTime / 10), Space.World);
	//	}
		
		// Using Rigidbody AddForce function
	//	if(Mathf.Abs(transform.position.x - positionV.x) > .1) {
	//		GetComponent.<Rigidbody>().AddForce(deltaVector * (Time.deltaTime / 10));
	//	}
		
		//##################################################################################
		// Stop the crate if close to target
		
		if (doprint){
		//Debug.Log("1: "+deltaT);
			//Debug.Log("Rtime: "+remainigTime);
			//Debug.Log("delta: "+deltaVector );
			//Debug.LogWarning("X force: "+(deltaVector * (deltaT/remainigTime)).x);
			//Debug.Log(positionV.x - transform.position.x);
		}
		/*
		if (Vector3.Distance(transform.position, targetVector) < 1f){
			//Debug.LogWarning("crate has been freezed"+ transform.position);
			//Debug.Log(Vector3.Distance(transform.position, targetVector));
			transform.rigidbody.constraints = RigidbodyConstraints.FreezePosition;
			
			isDropping = false;
		}
		else*/ 
		if (remainigTime <= -1){
			//Debug.LogWarning("crate out of time -> freezed"+ transform.position);
			//Debug.Log(Vector3.Distance(transform.position, targetVector));
			transform.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
			transform.rotation = Quaternion.identity;
			
			isDropping = false;
		}
		
		
		//##################################################################################
		// Using AddForce based off framerate
		//else{// if(transform.position.y - positionV.y > 3) {
			//GetComponent.<Rigidbody>().AddForce(deltaVector * ( Time.deltaTime *5 ));
			//GetComponent.<Rigidbody>().AddForce((deltaVector * (deltaT/remainigTime)) * ( Time.deltaTime ));
		transform.Translate(deltaVector * (deltaT/remainigTime), Space.World);
			//transform.Translate(deltaVector * (Time.deltaTime/remainigTime), Space.World);
		//}
		//##################################################################################
		
		prevTime = Time.time;
		
		//debugger = gameObject.Find("CAVE Mono").GetComponent(Logger);
		//debugger.setText(transform.position.ToString());
		
		//if(Mathf.Abs(Vector3.Distance(transform.position, positionV)) <= 0.2f) {
		//	if(explode) {
		//		if(Network.isServer) {
		//			Destroy(this);
		//		}
		//	}
		//}
	}
	else if (isActualCrate){
		if ((collisionTime +crateGlowTiming)>Time.time ){
			//start glowing
			Debug.Log("start glowing");
			//GetComponent("Halo").enabled=true;
			
			//Component halo = GetComponent("Halo"); 
			//halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
			
			var halo : Component = GetComponent("Halo"); 
			halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
		}
		else {
			//end glowing and disappear the crate
			Debug.Log("end glowing");
			//var halo2 : Component = GetComponent("Halo"); 
			//halo2.GetType().GetProperty("enabled").SetValue(halo, false, null);
			transform.active = false;
			//GetComponent("Halo").enabled=false;
		}
	}
}

/*
	Calcuate Probability Area:
	- Takes a list of probabilities and sudo-randomly finds the new position to drop the crate
	- Used to account for concepts such as error, wind, etc
*/
function calculateArea(probability : List.<float>) : Vector3 {
	//Debug.Log(positionV.x + " " + positionV.y +  " " + positionV.z);
	probability.Sort();
	
	var roll : float = Random.value;
	var tCompare : float = 0;
	var index : int = 0;
	
	for(var i in probability) {
		if(i >= roll) {
			tCompare = i;
			index = probability.IndexOf(i);
			break;
		}
	}
	
	if(tCompare == 0) {
		tCompare = probability[probability.Count - 1];
		index = probability.Count - 1;
	}
	
	var newPosition : Vector3;
	
	if(Random.value < 0.5) {
		newPosition = Vector3((tCompare * -5) + positionV.x, positionV.y, (tCompare * -5) + positionV.z);
	}
	else {
		newPosition = Vector3((tCompare * 5) + positionV.x, positionV.y, (tCompare * -5) + positionV.z);
	}
	
	newPosition.y = findAt(newPosition).y;
	
	return newPosition;
}

/*
	Find Position Below Crate:
	- Finds the first object below the crate to make sure that it will not descent through buildings or anything solid
*/
function findAt(position : Vector3) : Vector3 {
	position.y += 1;
	
	var hit : RaycastHit;
	
	if(Physics.Raycast(position, Vector3.down, hit)) {
		return Vector3(hit.point.x, hit.point.y, hit.point.z);
	}
	else {
		return Vector3.zero;
	}
}
