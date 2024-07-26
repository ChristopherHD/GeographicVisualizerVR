using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
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

	private RaycastHit cameraRaycastHit;
    private Vector3d transformd;
	private double heightBasedVelocity;

    void Start(){
		//Application.targetFrameRate = 60;

        Camera.main.nearClipPlane = 0.01f; // a 0.000001f desaparece esfera, si estamos a una distancia considerable proporcionalmente hablando
		//transform.forward = meshObject.transform.position - transform.position;
		cameraPositionObject.transform.forward = meshObject.transform.position - transform.position;
    }

	void Update () {
        int worldLayerMask = 1 << LayerMask.NameToLayer("World");
        Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out cameraRaycastHit, Mathf.Infinity, worldLayerMask);
		cameraHitPosition.SetPosition(cameraRaycastHit.point);

		this.heightBasedVelocity = ((double)cameraRaycastHit.distance) * forwardSpeed;
	}

	public void MoveForward() 
	{
		if (cameraRaycastHit.distance > 0.25d) {
			float x = cameraPositionObject.transform.forward.x;
			float y = cameraPositionObject.transform.forward.y;
			float z = cameraPositionObject.transform.forward.z;
			Vector3d test = new Vector3d(x, y, z);
			transformd = test * heightBasedVelocity * Time.deltaTime;
			Vector3 test2 = new Vector3((float)transformd.x, (float)transformd.y, (float)transformd.z);
			cameraPositionObject.transform.position += test2;
        }
    }

    public void MoveBackwards()
    {
        if (cameraRaycastHit.distance < 10000d)
		{
				cameraPositionObject.transform.position -= cameraPositionObject.transform.forward * (float)heightBasedVelocity * Time.deltaTime; // (transform.position - meshObject.transform.position + transform.forward * 12.5f)
		}
	}

	public void RotateCameraObject(Vector3 direction, float value){
        cameraPositionObject.transform.RotateAround(meshObject.transform.position, direction, value * (float)heightBasedVelocity * Time.deltaTime / 360);
    }
}