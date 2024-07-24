using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class VRControllerManager : MonoBehaviour
{

    public GameObject leftControllerObject;
    public GameObject rightControllerObject;

    // referencia: https://forum.unity.com/threads/oculus-quest-how-to-detect-a-b-x-y-button-presses.1108232/
    public InputActionReference joystickReference;
        
    // Start is called before the first frame update
    void Start()
    {
        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
        }


    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(joystickReference.action.IsPressed());
        bool isPressed;
        //Debug.Log(leftControllerObject.GetComponent<ActionBasedController>().selectAction.action.IsPressed());
        /*Debug.Log("1-" + rightControllerObject.GetComponent<ActionBasedController>().translateAnchorAction.action.IsPressed()); // se activa al girar el touchpad
        Debug.Log(rightControllerObject.GetComponent<ActionBasedController>().translateAnchorAction.action.ReadValue<Vector2>());       // devuelve 2 valores, el segundo vale 1 cuando vamos arriba, -1 al ir atrás, sería suficiente, 0 al ir a uno de los lados
        Debug.Log("2-" + rightControllerObject.GetComponent<ActionBasedController>().scaleToggleAction.action.IsPressed()); // nop
        Debug.Log("3-" + rightControllerObject.GetComponent<ActionBasedController>().rotateAnchorAction.action.IsPressed()); // se activa al girar el touchpad
        Debug.Log("4-" + rightControllerObject.GetComponent<ActionBasedController>().hapticDeviceAction.action.IsPressed()); // nop
        Debug.Log("5-" + rightControllerObject.GetComponent<ActionBasedController>().scaleDeltaAction.action.IsPressed()); // se activa al girar el touchpad
        Debug.Log("9-" + rightControllerObject.GetComponent<ActionBasedController>().selectAction.action.IsPressed()); */ // imprime correctamente cuando he pulsado el trigger, hay que tener las gafas puestas (revisar si puedo quitarlo de modo suspensión o lo que quiera que sea que lo esté activando)
        // bool result = leftControllerObject.GetComponent<ActionBasedController>()..inputDevice.IsPressed(InputHelpers.Button.PrimaryAxis2DUp, out isPressed);
        //bool result = leftControllerObject.GetComponent<ActionBasedController>()
        //Debug.Log(result);
    }
}
