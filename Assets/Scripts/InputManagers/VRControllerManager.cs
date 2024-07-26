using Newtonsoft.Json.Linq;
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
    public Button buttonMarker;
    public GameObject worldColliderRotation;

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

        if (leftJoystickReference.translateAnchorAction.action != null && leftJoystickReference.translateAnchorAction.action.ReadValue<Vector2>().y < -0.2f) cameraManager.RotateCameraObject(cameraManager.gameObject.transform.right, -cameraManager.rotationSpeed);
        if (leftJoystickReference.translateAnchorAction.action != null && leftJoystickReference.translateAnchorAction.action.ReadValue<Vector2>().y > 0.2f) cameraManager.RotateCameraObject(cameraManager.gameObject.transform.right, cameraManager.rotationSpeed);
        if (leftJoystickReference.rotateAnchorAction.action != null && leftJoystickReference.rotateAnchorAction.action.ReadValue<Vector2>().x > 0.2f) cameraManager.RotateCameraObject(cameraManager.meshObject.transform.up, -cameraManager.rotationSpeed);
        if (leftJoystickReference.rotateAnchorAction.action != null && leftJoystickReference.rotateAnchorAction.action.ReadValue<Vector2>().x < -0.2f) cameraManager.RotateCameraObject(cameraManager.meshObject.transform.up, cameraManager.rotationSpeed);

        float raycastDistanceRight = (rightRaycast.rayEndPoint - Camera.main.transform.position).magnitude;
        if (raycastDistanceRight < 25000)
        {
            CheckTriggerAddMarker(rightJoystickReference, rightRaycast.rayEndPoint, rightRaycast.gameObject);
        }
        float raycastDistanceLeft = (leftRaycast.rayEndPoint - Camera.main.transform.position).magnitude;
        if (raycastDistanceLeft < 25000)
        {
            CheckTriggerAddMarker(leftJoystickReference, leftRaycast.rayEndPoint, leftRaycast.gameObject);
        }

        cameraManager.cameraPositionObject.transform.RotateAround(cameraManager.meshObject.transform.position, transform.up, worldColliderRotation.transform.rotation.y);
    }

    private void CheckTriggerAddMarker(ActionBasedController controller, Vector3 hitPosition, GameObject gameObject)
    {
        if (controller.activateAction.action.IsPressed())
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
        if (!buttonPressed)
        {
            buttonPressed = true;
            buttonReleased = false;
        }
    }

}
