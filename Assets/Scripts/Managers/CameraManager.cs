using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraManager : MonoBehaviour {

	public GameObject meshObject;
	public float rotationSpeed = 7;
	public double forwardSpeed = 0.0025d;
	//public Vector3 raycastHitPoint;
	//public double raycastHitLatitude;
	//public double raycastHitLongitude;
	public MapPosition cameraHitPosition = new MapPosition();
	public static Boolean continued = false;
	public GameObject cameraPositionObject;
    public GameObject leftControllerObject;
    public GameObject rightControllerObject;

    private Vector3d transformd;
	private double heightBasedVelocity;

    void Start(){
		Camera.main.nearClipPlane = 0.01f; // a 0.000001f desaparece esfera, si estamos a una distancia considerable proporcionalmente hablando
		//transform.forward = meshObject.transform.position - transform.position;
		cameraPositionObject.transform.forward = meshObject.transform.position - transform.position;
    }

	void Update () {
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit cameraRaycastHit, Mathf.Infinity);
		
		cameraHitPosition.SetPosition(cameraRaycastHit.point);
		
		//raycastHitLatitude = GeoCord.GetLatitudeFromPosition(cameraRaycastHit.point);
		//raycastHitLongitude = GeoCord.GetLongitudeFromPosition(cameraRaycastHit.point);

		this.heightBasedVelocity = ((double)cameraRaycastHit.distance) * forwardSpeed;
        
        RotateCameraObject ();
		MoveCamera (cameraRaycastHit);
		RotateCameraCenter (cameraRaycastHit);

		//test(cameraRaycastHit);
	}

	/*private void test(RaycastHit cameraRaycastHit)
	{
		GameObject sphereCenter = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphereCenter.transform.position = cameraRaycastHit.point;
		sphereCenter.transform.localScale = Vector3.one * 50;
		sphereCenter.GetComponent<Renderer>().material.color = Color.blue;
		sphereCenter.GetComponent<SphereCollider>().enabled = false;
		//if (cameraRaycastHit.collider.gameObject.GetComponent<Mesh>() == null) return;
		//Debug.Log(cameraRaycastHit.collider.gameObject.name);
		Vector3[] vertices = cameraRaycastHit.collider.gameObject.GetComponent<MeshFilter>().mesh.vertices;
		double distance = Vector3.Distance(vertices[0], vertices[vertices.Length - 1]) / 2;
		createspheretest(Camera.main.transform.right + Camera.main.transform.up, distance);
		createspheretest(Camera.main.transform.right - Camera.main.transform.up, distance);
		createspheretest(-Camera.main.transform.right + Camera.main.transform.up, distance);
		createspheretest(-Camera.main.transform.right - Camera.main.transform.up, distance);
	}

	private void createspheretest(Vector3 direction, double distance)
	{
		Vector3 OffsetPosition2 = Camera.main.transform.position + direction * (float)distance;
		Physics.Raycast(OffsetPosition2, Camera.main.transform.forward, out RaycastHit cameraRaycastHit3, Mathf.Infinity);
		//if (cameraRaycastHit2.collider.gameObject.GetComponent<Tile>() == null) return;
		Double[] bbox2 = cameraRaycastHit3.collider.gameObject.GetComponent<Tile>().BBOX;
		Debug.Log(bbox2[0] + ":" + bbox2[1] + ":" + bbox2[2] + ":" + bbox2[3]);
		GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere2.transform.position = cameraRaycastHit3.point;
		sphere2.transform.localScale = Vector3.one * 50;
		sphere2.GetComponent<Renderer>().material.color = Color.red;
		sphere2.GetComponent<SphereCollider>().enabled = false;
	}*/

	private void MoveCamera(RaycastHit raycastHit) {
		
		if (raycastHit.distance > 0.25d) {
			
	     	//Debug.Log("Se mueve?: " + (rightControllerObject.GetComponent<ActionBasedController>().translateAnchorAction.action.ReadValue<Vector2>().y > 0.2f));
			if (Input.GetKey(KeyCode.W) || rightControllerObject.GetComponent<ActionBasedController>().translateAnchorAction.action.ReadValue<Vector2>().y > 0.2f) {
				
			//if (Input.GetKey(KeyCode.W)) { // && cameraDistance
										   //Debug.Log("Distancia 1: " + heightBasedVelocity);
				float x = cameraPositionObject.transform.forward.x;
				float y = cameraPositionObject.transform.forward.y;
				float z = cameraPositionObject.transform.forward.z;
				Vector3d test = new Vector3d(x, y, z);
				transformd = test * heightBasedVelocity;
				Vector3 test2 = new Vector3((float)transformd.x, (float)transformd.y, (float)transformd.z);
				cameraPositionObject.transform.position += test2;
			}
		}

		if (raycastHit.distance < 10000d)
		{
			//if (Input.GetKey(KeyCode.S))
			if (Input.GetKey(KeyCode.S) || rightControllerObject.GetComponent<ActionBasedController>().translateAnchorAction.action.ReadValue<Vector2>().y < -0.2f)
			{
				//Debug.Log("Distancia 2 : " + heightBasedVelocity);
				cameraPositionObject.transform.position -= cameraPositionObject.transform.forward * (float)heightBasedVelocity; // (transform.position - meshObject.transform.position + transform.forward * 12.5f)
			}
		}

		if (Input.GetKey(KeyCode.P))
		{
			//GameObject.FindGameObjectWithTag("GameController").GetComponent<LoDTesterSphere>().ShouldDivide(Selection.activeGameObject.GetComponent<Tile>(), true);
			//GameObject.FindGameObjectWithTag("GameController").GetComponent<LoDTesterSphere>().CheckLoD(Selection.activeTransform, true);
		}
	}

	private void RotateCameraObject(){
		if (Input.GetKeyDown(KeyCode.Space)) Debug.Log("Pos: " + cameraHitPosition.worldPosition + " :: " + cameraHitPosition.latitude + " :: " + cameraHitPosition.longitude);
        if (Input.GetKeyDown(KeyCode.L)) cameraPositionObject.transform.RotateAround(meshObject.transform.position, transform.up, 90);
        if (Input.GetKey (KeyCode.A)) cameraPositionObject.transform.RotateAround (meshObject.transform.position, meshObject.transform.up, (rotationSpeed * (float)heightBasedVelocity) / 360);
		if (Input.GetKey (KeyCode.D)) cameraPositionObject.transform.RotateAround (meshObject.transform.position, meshObject.transform.up, (-rotationSpeed * (float)heightBasedVelocity) / 360);
		if (Input.GetKey (KeyCode.Q)) cameraPositionObject.transform.RotateAround (meshObject.transform.position, transform.right, (-rotationSpeed * (float)heightBasedVelocity) / 360);
		if (Input.GetKey (KeyCode.E)) cameraPositionObject.transform.RotateAround (meshObject.transform.position, transform.right, (rotationSpeed * (float)heightBasedVelocity) / 360);
	}

	private void RotateCameraCenter(RaycastHit raycastHit){

		// para el ángulo límite hacer que se compare el ángulo del vector que une al centro con la cámara

		if(Input.GetKey(KeyCode.H) && !IsAngleLimitReached(raycastHit, 170f, meshObject.transform.position)) transform.RotateAround (raycastHit.point, transform.right, rotationSpeed / 5);
		if(Input.GetKey(KeyCode.J) && !IsAngleLimitReached(raycastHit, 10f, -meshObject.transform.position)) transform.RotateAround (raycastHit.point, transform.right, -rotationSpeed / 5);
		//if (Input.GetKey(KeyCode.H) && !IsAngleLimitReached2(-110f, true)) transform.RotateAround(raycastHit.point, transform.right, rotationSpeed / 5);
		//if (Input.GetKey(KeyCode.J) && !IsAngleLimitReached2(110f, false)) transform.RotateAround(raycastHit.point, transform.right, -rotationSpeed / 5);

		if (Input.GetKey(KeyCode.K)) transform.RotateAround (raycastHit.point, raycastHit.normal, rotationSpeed / 5);
		if(Input.GetKey(KeyCode.L)) transform.RotateAround (raycastHit.point, raycastHit.normal, -rotationSpeed / 5);
	}

	private bool IsAngleLimitReached2(float angle, Boolean greater)
	{
		Debug.Log(Vector3.SignedAngle(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position, transform.forward, Vector3.up));
		float finalAngle = Vector3.Angle(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position, transform.forward);
		if (angle * finalAngle < 0) return false;
		return (greater ? finalAngle > angle : finalAngle < angle);
		//return Vector3.Angle(transform.position - GameObject.FindGameObjectWithTag("Player").transform.position, transform.forward) < angle;
	}

	private bool IsAngleLimitReached(RaycastHit raycastHit, float angle, Vector3 vector){
		Debug.Log ("Angle: "+Vector3.SignedAngle(-transform.forward, raycastHit.point - vector,  meshObject.transform.up));
		return false;
	}
}