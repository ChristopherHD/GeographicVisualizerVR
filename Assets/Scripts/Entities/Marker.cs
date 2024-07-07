using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mathd;
public class Marker
{
    private float lat;
    private float lon;
    private List<LineRenderer> joinedLines;
    Vector3 worldPosition;
    GameObject markerObject;
    
    private static GameObject markersContainer = GameObject.Find("Markers");
    private static Marker lastSelectedMarker;

    public Marker(float lat, float lon)
    {
        this.lat = lat;
        this.lon = lon;
        joinedLines = new List<LineRenderer>();
        this.worldPosition = GeoCord.GetPositionFromLatitudeLongitude(lat, lon);
        CreateWorldObject();
    }

    public void CreateWorldObject()
    {
        markerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        UnityEngine.Object.Destroy(markerObject.GetComponent<BoxCollider>());
        markerObject.layer = LayerMask.NameToLayer("Marker");
        markerObject.AddComponent<MarkerObject>();
        markerObject.GetComponent<MarkerObject>().marker = this;
        markerObject.transform.parent = markersContainer.transform;
        markerObject.transform.position = this.worldPosition;
        markerObject.transform.localScale = Vector3.one * 5;
        markerObject.GetComponent<MeshRenderer>().material.color = Color.blue;
        SphereCollider collider = markerObject.AddComponent<SphereCollider>();
        collider.radius = 1;
    }

    public void JoinMarker(Marker marker)
    {
        GameObject lineObject = new GameObject();
        //lineObject.transform.parent = 
        lineObject.AddComponent<Billboard>();
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.transform.parent = lineObject.transform;
        background.transform.localPosition = Vector3.zero;
        background.GetComponent<MeshRenderer>().material.color = Color.black;
        background.transform.localScale = new Vector3(40, 10, 1);

        TextMesh text = lineObject.AddComponent<TextMesh>();
        double distance = Distance(marker);
        Boolean isKM = true;
        if (distance < 1)
        {
            distance *= 1000;
            isKM = false;
        }
        text.text = System.Math.Round(distance, 2).ToString() + (isKM ? " Km" : " m");
        text.anchor = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontSize = 72;

        int linePositions = 2 + (int) (Math.Max(Math.Abs(this.lat - marker.lat), Math.Abs(this.lon - marker.lon)) / 0.5);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i <= linePositions ; i++)
        {
            Vector3 pos = this.markerObject.transform.position + i * (marker.markerObject.transform.position - this.markerObject.transform.position) / linePositions;
            positions.Add(GeoCord.GetPositionFromLatitudeLongitude(GeoCord.GetLatitudeFromPosition(pos), GeoCord.GetLongitudeFromPosition(pos)));
        }
        Vector3 middlePos = this.markerObject.transform.position + (marker.markerObject.transform.position - this.markerObject.transform.position) / 2;
        middlePos = GeoCord.GetPositionFromLatitudeLongitude(GeoCord.GetLatitudeFromPosition(middlePos), GeoCord.GetLongitudeFromPosition(middlePos));
        line.positionCount = positions.Count;
        //line.SetVertexCount(linePositions + 1);
        line.SetPositions(positions.ToArray());
        this.joinedLines.Add(line);

        lineObject.transform.position = middlePos;
    }

    public double Distance(Marker marker)
    {
        double radianLat1 = Deg2Rad * this.lat;
        double radianLon1 = Deg2Rad * this.lon;
        double radianLat2 = Deg2Rad * marker.lat;
        double radianLon2 = Deg2Rad * marker.lon;
        double diffLon = radianLon2 - radianLon1;

        // --- Haversine
        // double diffLat = radianLat2 - radianLat1;
        // double chordLengthSquared = Pow(Sin(diffLat / 2), 2) + Cos(this.lat) * Cos(marker.lat) * Pow(Sin(diffLon / 2), 2);
        // double chordLength = 2 * Asin(Sqrt(a));

        // --- Vincenty
        double chordLength = Atan2(Sqrt(Pow(Cos(radianLat2) * Sin(diffLon), 2)
            + Pow(Cos(radianLat1) * Sin(radianLat2) - Sin(radianLat1) * Cos(radianLat2) * Cos(diffLon), 2))
            , Sin(radianLat1) * Sin(radianLat2) + Cos(radianLat1) * Cos(radianLat2) * Cos(diffLon));
        double meanEarthRadius = 6371.009;
        return chordLength * meanEarthRadius;
    }

    public void Select()
    {
        markerObject.GetComponent<MeshRenderer>().material.color = Color.green;
        if (lastSelectedMarker != null && lastSelectedMarker != this)
        {
            lastSelectedMarker.GetMarkerObject().GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        lastSelectedMarker = this;
    }

    public float GetLat()
    {
        return this.lat;
    }

    public float GetLon()
    {
        return this.lon;
    }

    public Vector3 GetWorldPosition()
    {
        return this.worldPosition;
    }
    public GameObject GetMarkerObject()
    {
        return this.markerObject;
    }
    public List<LineRenderer> GetJoinedLines()
    {
        return this.joinedLines;
    }
    public static Marker GetLastSelectedMarker()
    {
        return lastSelectedMarker;
    }

}
