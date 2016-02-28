using UnityEngine;
using System.Collections;

public class CollisionDetection : MonoBehaviour {

	public int crateGlowTiming ;

	// Use this for initialization
	void Start () {
		Behaviour crateBehaviour = (Behaviour)GetComponent("Halo");
		crateBehaviour.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void DestroyCrate(){
		networkView.RPC("RPCDestroyCrate", RPCMode.Others);
	}
	
	[RPC] void RPCDestroyCrate(){
		Destroy (gameObject, 1);
		Destroy (gameObject);
		Destroy(this.gameObject);
		Destroy(this);
	}

	void OnCollisionStay(Collision col)
	{
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(crateGlowTiming);
	}

	void OnCollisionEnter(Collision col)
	{


		Debug.Log (transform.name+" --> "+ col.transform.name);
		// destory this crate after 3 seconds

		Behaviour crateBehaviour = (Behaviour)GetComponent("Halo");
		crateBehaviour.enabled = true;


		if ((col.transform.name.Contains("ID-grey")) || (col.transform.name.Contains("ID-yelllow")) || (col.transform.name.Contains("ID-black")) || 
		    (col.transform.name.Contains("ID-red")) || (col.transform.name.Contains("ID-pink")) || (col.transform.name.Contains("ID-pink")) || 
		    (col.transform.name.Contains("ID-blue")) || (col.transform.name.Contains("ID-purple")) || (col.transform.name.Contains("ID-white")) ){
			Debug.Log ("Condition True1");
		}

		//DestroyCrate();

		if(col.transform.name == "ID-grey" || col.transform.name ==  "ID-yelllow" || col.transform.name == "ID-black" ||  
		   col.transform.name == "ID-red" || col.transform.name ==  "ID-pink" ||  col.transform.name == "ID-green" || 
		   col.transform.name == "ID-blue" || col.transform.name ==  "ID-purple" || col.transform.name == "ID-white"){

			Debug.Log ("Condition True2");

			Debug.Log (col.transform.name);
			//DestroyCrate();	

		}

		StartCoroutine("Wait");

		//crateBehaviour = (Behaviour)GetComponent("Halo");
		crateBehaviour.enabled = false;

		//Destroy (gameObject, crateGlowTiming);
		//gameObject.SetActive(false); // deactivate object
		//renderer.enabled = false;
	}
	/*void OnTriggerEnter (Collision col)
	{
		Debug.Log (col.transform.name);
	}
	void OnTriggerStay (Collision col)
	{
		//Debug.Log (col.transform.name);
		if(col.gameObject.name == "ID-grey" || col.gameObject.name ==  "ID-yelllow" || col.gameObject.name == "ID-black" ||  
		   col.gameObject.name == "ID-red" || col.gameObject.name ==  "ID-pink" ||  col.gameObject.name == "ID-green" || 
		   col.gameObject.name == "ID-blue" || col.gameObject.name ==  "ID-purple" || col.gameObject.name == "ID-white"){
			Destroy(this);
		}
	}*/
}
