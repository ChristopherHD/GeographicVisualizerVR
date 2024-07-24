using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
    public CameraManager cameraManager;

    void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			Debug.Log("Modo: "+ Screen.fullScreenMode);
			Screen.fullScreen = !Screen.fullScreen;
		}

		if (Input.GetKey(KeyCode.W)) cameraManager.MoveForward();
        if (Input.GetKey(KeyCode.S)) cameraManager.MoveBackwards();
        CheckRotation();
	}

	private void CheckRotation()
	{
        //if (Input.GetKeyDown(KeyCode.L)) cameraManager.RotateCameraObject(transform.up, 90);
        if (Input.GetKey(KeyCode.A)) cameraManager.RotateCameraObject(cameraManager.meshObject.transform.up, cameraManager.rotationSpeed);
        if (Input.GetKey(KeyCode.D)) cameraManager.RotateCameraObject(cameraManager.meshObject.transform.up, -cameraManager.rotationSpeed);
        if (Input.GetKey(KeyCode.Q)) cameraManager.RotateCameraObject(cameraManager.gameObject.transform.right, -cameraManager.rotationSpeed);
        if (Input.GetKey(KeyCode.E)) cameraManager.RotateCameraObject(cameraManager.gameObject.transform.right, cameraManager.rotationSpeed);
    }
}
