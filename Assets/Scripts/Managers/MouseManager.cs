using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour
{
    private Boolean buttonPressed = false;
    private Boolean buttonUp = false;
    public Button buttonMarker;
    private void Start()
    {
        buttonMarker.onClick.AddListener(ManageUIMarker);
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int layerMask = 1 << LayerMask.NameToLayer("Marker");
            Ray mouseClickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool markerImpact = Physics.Raycast(mouseClickRay, out RaycastHit raycastHit, Mathf.Infinity, layerMask);
            if (markerImpact)
            {
                raycastHit.collider.gameObject.GetComponent<MarkerObject>().marker.Select();
            }

            if(!buttonUp)
            {
                buttonUp = true;
            } else
            {
                if (buttonPressed)
                {
                    bool markerImpact2 = Physics.Raycast(mouseClickRay, out RaycastHit raycastHit2, Mathf.Infinity);
                    if (markerImpact2)
                    {
                        float lon = CoordUtils.GetLongitudeFromPosition(raycastHit2.point);
                        float lat = CoordUtils.GetLatitudeFromPosition(raycastHit2.point);
                        Marker marker = new(lat, lon);
                        buttonPressed = false;
                    }
                }
            }

        }
        else if (Input.GetMouseButtonUp(1))
        {
            int layerMask = 1 << LayerMask.NameToLayer("Marker");
            Ray mouseClickRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool markerImpact = Physics.Raycast(mouseClickRay, out RaycastHit raycastHit, Mathf.Infinity, layerMask);
            if (markerImpact)
            {
                if (Marker.GetLastSelectedMarker() != null)
                {
                    raycastHit.collider.gameObject.GetComponent<MarkerObject>().marker.JoinMarker(Marker.GetLastSelectedMarker());
                }
            }
        }
    }

    public void ManageUIMarker()
    {
        if (!buttonPressed)
        {
            buttonPressed = true;
            buttonUp = false;
        }
    }
}
