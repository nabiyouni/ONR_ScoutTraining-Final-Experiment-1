
#pragma strict

/*
	Class Variables:
	- XML File that will contain the beacon coordinates
	- List of Vector3's that represent the beacon locations
	- Boolean deciding whether or not to remove the beacons from the world
	- Prefab object that will be cloned to create multiple beacons in the actual scene
*/
private var asset : String;
private var list : List.<Vector3> = new List.<Vector3>();
private var destroyBeacons : boolean = true;
public var beaconPrefab : GameObject;
public var targetPrefab : GameObject;

/*
	Start:
	- Read in the initial trial of beacon data
	- Spawn the beacons in the world
*/
function Start () {
	readXMLData();
	
	//instantiateBeacons();
}

function OnEnable() {
	WandEventManager.OnButton4 += toggleBeacons;
}

/*
	Update (Every frame):
	- Check to see if the 'B' button is pressed
	- If it is, toggle the boolean and either destroy or create the beacons
*/
function Update () {
	if(Input.GetKeyDown(KeyCode.B)) {
		toggleBeacons();
	}
	
	if(Input.GetKeyDown(KeyCode.R)) {
		readXMLData();
	}
}


function toggleBeacons() {
	destroyBeacons = !destroyBeacons;
	
	if(destroyBeacons) {
		destroyAllBeacons();
	}
	else {
		instantiateBeacons();
	}
}
/*
	Spawn Beacons:
	- Traverses the list of beacon Vector3's and creates them from a prefab
	- Sets the initial color to green (will change later) and adds a tag for easy searching when destroying
*/
function instantiateBeacons() {
	var debugger:Logger = gameObject.Find("CAVE Mono").GetComponent(Logger);
	for(position in list) {
		var newBeacon : GameObject = Network.Instantiate(beaconPrefab, position, Quaternion.identity, 1);
		//debugger.setText("Position: " + position + " Group ID: " + 1);
		newBeacon.GetComponent(Beacon).networkView.RPC("setColorandTag", RPCMode.AllBuffered, "Beacon", "0.0, 1.0, 0.0, 1.0");
		//Network.RemoveRPCs(newBeacon.GetComponent(Beacon).networkView.viewID);
		
		var newTarget : GameObject = Network.Instantiate(targetPrefab, position, Quaternion.identity, 1);
		newTarget.transform.Rotate(90f,0f,0f);
		newTarget.GetComponent(Beacon).networkView.RPC("setTag", RPCMode.AllBuffered, "Target");
		//newTarget.gameObject.tag = "Target";
	}
}

/*
	Destroy Beacons:
	- Find all the game objects with tag "Beacon"
	- Destroy all of those objects
*/
function destroyAllBeacons() {
	for(beacon in GameObject.FindGameObjectsWithTag("Beacon")) {
		if(Network.isServer) {
			Network.Destroy(beacon);
		}
	}
	for(target in GameObject.FindGameObjectsWithTag("Target")) {
		if(Network.isServer) {
			Network.Destroy(target);
		}
	}
}

/*
	XML Reader:
	- Find the xml file named "TrialData.xml"
	- Find all of the (x, y, z) values within the beacon tags
*/
function readXMLData() {
	var tempV : Vector3;
	var filePath : String;
	
	list.Clear();
		
	if (Application.isEditor) {
		filePath = "Assets/XML/TrialData.xml";
	} else {
		filePath = Application.dataPath+"/Resources/TrialData.xml";
	}

	if(System.IO.File.Exists(filePath)) {
		asset = System.IO.File.ReadAllText(filePath);
			
		if(asset != null) {
			var reader : XmlTextReader = new XmlTextReader(new StringReader(asset));
		    while(reader.Read()) {
		    	if(reader.Name.Contains("beacon")) {
		    		if(float.TryParse(reader.GetAttribute("x"), tempV.x) && float.TryParse(reader.GetAttribute("y"), tempV.y) && float.TryParse(reader.GetAttribute("z"), tempV.z)) {
		    			list.Add(tempV);
		    		}
		    	}
		    }
		}
	}
}