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
private var isHaloOn : boolean = false;
private var isActive : boolean = false;

public var collisionTime : float;
public var crateGlowTiming : float;
public var crateExplodeTiming : float;
public var isActualCrate : boolean ;

public var peer : GameObject;
private var peerInstance : Crate;
private var halo : Component;

private var explode : boolean = false;
public var isDropping : boolean = false;
private var doprint : boolean = false;

private var debugger : Logger;

/*
	Start:
	- Set the initial time
*/
function Start () {
	startTime = Time.time;
	
	peerInstance = peer.GetComponent("Crate");
	halo = GetComponent("Halo"); 
	
	debugger = gameObject.Find("CAVE Mono").GetComponent(Logger);
}

/*
	Update (Every frame):
	- If startDrop is true, then drop the crate
*/
function Update () {
	dropCrate();
}



function OnCollisionEnter(collision: Collision) {
///*
	//if ((collision.transform.name == "Target")||(collision.transform.name == "NearTerrain")){
	//Debug.Log("Collide: "+collision.transform.name);
	if (collision.transform.name == "Target"){
		//Debug.LogWarning(transform.name +"   ->  "+ collision.transform.name);
		
		transform.rotation = Quaternion.identity;
		transform.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
		isDropping = false;
		collisionTime = Time.time;
		//Debug.LogError("droptime: "+(Time.time - startTime));
	}
//*/
}


/*
	Set Variables:
	- Set the position, time of descent, and explode variables
	- Used by the CSpawner when creating new crates
*/
@RPC
function setVariables(newPosition : Vector3, offTarget : Vector3, newTime : float, destroyImpact : boolean, newTag : String) {
	if (isActualCrate){
		renderer.enabled = true;
		transform.renderer.enabled = true;
	}

	positionV = newPosition;
	positionV.y = positionV.y + .5;
	
	totalDroptime = newTime;
	
	explode = destroyImpact;
	gameObject.tag = newTag;
	targetVector = positionV;
	targetVector.y -= 50f;
	
	//Debug.Log("PositionV: "+positionV);
	//Debug.Log("offTarget: "+offTarget);
	
	positionV = positionV + offTarget;
	
	//Debug.Log("positionVF: "+positionV);
	
	
}

@RPC
function startCrate(pri:boolean) {
	
	transform.active = true;
	
	//deltaVector = (positionV - transform.position);
	deltaVector.x = (positionV.x - transform.position.x);
	deltaVector.z = (positionV.z - transform.position.z);
	deltaVector.y = 0.0;
	
	isDropping = true;
	isActive = true;
	
	doprint = pri;
	
	startTime = Time.time;
	prevTime = Time.time;
	
	transform.rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	
	isHaloOn = false;
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
		
		if (!isActualCrate)
		{
			//transform.position = Vector3(positionV.x - 30, positionV.y + 200, positionV.z);
			
			deltaVector.x = (positionV.x - transform.position.x);
			deltaVector.z = (positionV.z - transform.position.z);
			deltaVector.y = 0.0;
			
			var delta = ((Time.time - startTime) / totalDroptime);
			
			var deltaT = (Time.time - prevTime);
			var remainigTime = totalDroptime - (Time.time - startTime);
			
			if (remainigTime == 0)
				remainigTime = 0.001;
			
			//##################################################################################
			// Stop the crate if close to target
			
			//if (doprint){
			//}

			if (remainigTime <= -1){
				transform.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
				transform.rotation = Quaternion.identity;
				
				isDropping = false;
				
				Debug.LogWarning("timeout");
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
		}
		
		if (isActualCrate){
			isDropping = peerInstance.isDropping;
			collisionTime = peerInstance.collisionTime;
			//transform.rigidbody.constraints = peerInstance.transform.rigidbody.constraints;
		}

	}
	
	if ((!isDropping)&&(isActualCrate)&&(isActive)){
		//debugger.setText(Time.time+"A "+transform.name+" Drp: " + isDropping + " ActCrt: " + isActualCrate+" Hlo:"+isHaloOn+" exp:"+explode);
		//debugger.setText("B ");
		
		if (!isHaloOn){
			//debugger.setText("C ");
			isHaloOn = true;
			if (!explode){
				// subject to remove
				halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
				transform.rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
				//debugger.setText("1 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
				//Debug.Log("1 "+transform.name+" collisionTime:"+collisionTime);
			}
			else{
				halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
				transform.rigidbody.constraints = RigidbodyConstraints.FreezePositionY;
				//debugger.setText("2 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
				//Debug.Log("2 "+transform.name+" collisionTime:"+collisionTime);
			}
			
		}
		else {
			//debugger.setText("D ");
			if (!explode){
				//debugger.setText("E ");
				if ((collisionTime +crateGlowTiming)<= Time.time ){
					halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
					//transform.position.x += -200; transform.position.z += -300;
					//transform.active = false;
					renderer.enabled = false;
					isDropping = false; isHaloOn = false; isActive = false;
					//Debug.Log("3 "+transform.name+" collisionTime:"+collisionTime+"  crateGlowTiming:"+crateGlowTiming);
					//Debug.Log("Time.time:"+Time.time);
					//debugger.setText("3 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
				}
			}
			else{
				//debugger.setText("F ");
				if ((collisionTime +crateExplodeTiming)<= Time.time ){
					halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
					//transform.position.x += -200; transform.position.z += -300;
					//transform.active = false;
					renderer.enabled = false;
					isDropping = false; isHaloOn = false; isActive = false;
					//Debug.Log("4 "+transform.name+" collisionTime:"+collisionTime+"  crateExplodeTiming:"+crateExplodeTiming);
					//Debug.Log("Time.time:"+Time.time);
					//debugger.setText("4 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
				}
			}
		}
		
	}
	
	if ((!isDropping)&& (!isActualCrate) && (isActive)){
		
		
		if (!isHaloOn){
			
			isHaloOn = true;
			/*if (explode){
				halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
				//debugger.setText("5 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
			}
			else{
				halo.GetType().GetProperty("enabled").SetValue(halo, true, null);
				//debugger.setText("6 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
			}*/
		}
		else {
			if (explode){
				if ((collisionTime +crateExplodeTiming)<= Time.time ){
					halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
					//transform.rigidbody.constraints = RigidbodyConstraints.None;
					//transform.position.x += -200; transform.position.z += -300;
					//transform.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					//transform.active = false;
					isDropping = false; isHaloOn = false; isActive = false;
					//debugger.setText("7 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
				}
			}
			else{
				if ((collisionTime +crateGlowTiming)<= Time.time ){
					halo.GetType().GetProperty("enabled").SetValue(halo, false, null);
					//transform.rigidbody.constraints = RigidbodyConstraints.None;
					//transform.position.x += -200; transform.position.z += -300;
					//transform.rigidbody.constraints = RigidbodyConstraints.FreezeAll;
					//transform.active = false;
					isDropping = false; isHaloOn = false; isActive = false;
					//debugger.setText("8 "+transform.name+" isDropping: " + isDropping + " isActualCrate: " + isActualCrate+" isHaloOn:"+isHaloOn+" explode:"+explode);
				}
			}
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
