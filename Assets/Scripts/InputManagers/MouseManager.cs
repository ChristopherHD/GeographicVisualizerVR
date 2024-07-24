using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MouseManager : MonoBehaviour
{
    private Boolean buttonPressed = false;
    private Boolean buttonReleased = false;

    public Button buttonMarker;
    public Button attributionButton;

    void Start()
    {
        buttonMarker.onClick.AddListener(ManageUIMarker);
        attributionButton.onClick.AddListener(OpenAttributionLink);
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            int layerMask = 1 << LayerMask.NameToLayer("Marker");
            
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Ray mouseClickRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            bool markerImpact = Physics.Raycast(mouseClickRay, out RaycastHit raycastHit, Mathf.Infinity, layerMask);
            if (markerImpact)
            {
                raycastHit.collider.gameObject.GetComponent<MarkerObject>().marker.Select();
            }

            if(!buttonReleased)
            {
                buttonReleased = true;
            } else
            {
                if (buttonPressed)
                {
                    int worldMask = 1 << LayerMask.NameToLayer("World");
                    bool markerImpact2 = Physics.Raycast(mouseClickRay.origin, mouseClickRay.direction, out RaycastHit raycastHit2, Mathf.Infinity, worldMask);
                    if (markerImpact2)
                    {
                        Debug.Log(raycastHit2.collider.gameObject.name);
                        Debug.Log(raycastHit2.collider.gameObject.layer);
                        float lat = CoordUtils.GetLatitudeFromPosition(raycastHit2.point);
                        float lon = CoordUtils.GetLongitudeFromPosition(raycastHit2.point);
                        GameObject a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        a.transform.localScale = Vector3.one * 20;
                        a.transform.position = raycastHit2.point;
                        Debug.DrawLine(mouseClickRay.direction * 100, mouseClickRay.origin, Color.red, 10);
                        Debug.DrawRay(mouseClickRay.origin, mouseClickRay.direction * 100, Color.green,10);
                        Debug.Log(lon + " :: " +  lat);
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
            buttonReleased = false;
        }
    }
    public void OpenAttributionLink()
    {
        TMP_LinkInfo linkInfo = attributionButton.transform.GetChild(0).GetComponent<TMP_Text>().textInfo.linkInfo[0];
        Application.OpenURL(linkInfo.GetLinkID());
    }
}
