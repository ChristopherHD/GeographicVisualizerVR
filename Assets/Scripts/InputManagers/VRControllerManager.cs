using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRControllerManager : MonoBehaviour
{
    public CameraManager cameraManager;
    public GameObject leftControllerObject;
    public GameObject rightControllerObject;
    public InputActionReference keyAPressed;
    public Button buttonMarker;

    private ActionBasedController rightJoystickReference;
    private ActionBasedController leftJoystickReference;
    private XRRayInteractor rightRaycast;
    private XRRayInteractor leftRaycast;
    private Boolean buttonPressed = false;
    private Boolean buttonReleased = false;

    void Awake()
    {
        rightJoystickReference = rightControllerObject.GetComponent<ActionBasedController>();
        leftJoystickReference = leftControllerObject.GetComponent<ActionBasedController>();
        rightRaycast = rightControllerObject.GetComponent<XRRayInteractor>();
        leftRaycast = rightControllerObject.GetComponent<XRRayInteractor>();
        buttonMarker.onClick.AddListener(ManageUIMarker);
    }

    void Update()
    {
        if (rightJoystickReference.translateAnchorAction.action != null && rightJoystickReference.translateAnchorAction.action.ReadValue<Vector2>().y > 0.2f) cameraManager.MoveForward();
        if (rightJoystickReference.translateAnchorAction.action != null && rightJoystickReference.translateAnchorAction.action.ReadValue<Vector2>().y < -0.2f) cameraManager.MoveBackwards();

        if (leftJoystickReference.translateAnchorAction.action != null && leftJoystickReference.translateAnchorAction.action.ReadValue<Vector2>().y > 0.2f) cameraManager.MoveForward();
        if (leftJoystickReference.translateAnchorAction.action != null && leftJoystickReference.translateAnchorAction.action.ReadValue<Vector2>().y < -0.2f) cameraManager.MoveBackwards();

        float raycastDistance = (rightRaycast.rayEndPoint - Camera.main.transform.position).magnitude;
        if (raycastDistance < 25000)
        {
            CheckTriggerAddMarker(rightRaycast.rayEndPoint, rightRaycast.gameObject);
        }
        /*if (Input.GetKeyDown(KeyCode.JoystickButton0)) //(Input.GetKeyUp(KeyCode.JoystickButton0)) // A --> only works in first click
        {
            Debug.Log("A");
            Vector3 markerPosition = rightRaycast.rayEndPoint;
            float lat = CoordUtils.GetLatitudeFromPosition(markerPosition);
            float lon = CoordUtils.GetLongitudeFromPosition(markerPosition);
            new Marker(lat, lon);
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton1)) // B
        {
            Debug.Log("B");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) // X
        {
            Debug.Log("X");
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton3)) // Y
        {
            Debug.Log("Y");
        }*/
    }

    private void CheckTriggerAddMarker(Vector3 hitPosition, GameObject gameObject)
    {
        if (rightJoystickReference.activateAction.action.IsPressed())
        {
            /*int layerMask = 1 << LayerMask.NameToLayer("Marker");
            if (gameObject.layer == layerMask)
            {
                gameObject.GetComponent<MarkerObject>().marker.Select();
            }*/

            if (!buttonReleased)
            {
                buttonReleased = true;
            }
            else
            {
                if (buttonPressed)
                {
                    float lat = CoordUtils.GetLatitudeFromPosition(hitPosition);
                    float lon = CoordUtils.GetLongitudeFromPosition(hitPosition);
                    new Marker(lat, lon);
                    buttonPressed = false;
                }
            }

        }
    }

    public void ManageUIMarker()
    {
        Debug.Log("1");
        if (!buttonPressed)
        {
            buttonPressed = true;
            buttonReleased = false;
        }
    }

}
