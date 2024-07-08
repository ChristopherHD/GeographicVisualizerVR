using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Mathd;
public class Marker
{
    private float lat;
    private float lon;
    private List<LineObject> joinedLines;
    Vector3 worldPosition;
    GameObject markerObject;
    
    private static GameObject markersContainer = GameObject.Find("Markers");
    private static GameObject linesContainer = GameObject.Find("Lines");
    private static Marker lastSelectedMarker;

    public Marker(float lat, float lon)
    {
        this.lat = lat;
        this.lon = lon;
        joinedLines = new List<LineObject>();
        this.worldPosition = CoordUtils.GetPositionFromLatitudeLongitude(lat, lon);
        CreateWorldObject();
    }

    public void CreateWorldObject()
    {
        //markerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //UnityEngine.Object.Destroy(markerObject.GetComponent<BoxCollider>());
        markerObject = new GameObject();
        markerObject.layer = LayerMask.NameToLayer("Marker");
        markerObject.AddComponent<MarkerObject>();
        markerObject.GetComponent<MarkerObject>().marker = this;
        markerObject.AddComponent<Billboard>();
        markerObject.transform.parent = markersContainer.transform;
        markerObject.transform.position = this.worldPosition;
        markerObject.transform.localScale = Vector3.zero; // it'll scale inmediatly after
        SphereCollider collider = markerObject.AddComponent<SphereCollider>();
        collider.radius = 1;
    }

    public void JoinMarker(Marker marker)
    {
        GameObject lineObject = new GameObject();
        lineObject.transform.parent = linesContainer.transform;
        lineObject.AddComponent<Billboard>();
        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
        background.GetComponent<MeshCollider>().enabled = false;
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
        LineObject line = lineObject.AddComponent<LineObject>();
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        List<Vector3> positions = new List<Vector3>();

        //Vector3 diffPosition = worldPosition - this.markerObject.transform.position;
        for (int i = 0; i <= linePositions ; i++)
        {
            //Vector3 pos = this.markerObject.transform.position + i * (marker.markerObject.transform.position - this.markerObject.transform.position) / linePositions;
            Vector3 pos = this.worldPosition + i * (marker.worldPosition - this.worldPosition) / linePositions;
            positions.Add(CoordUtils.GetPositionFromLatitudeLongitude(CoordUtils.GetLatitudeFromPosition(pos), CoordUtils.GetLongitudeFromPosition(pos)));
        }
        Vector3 middlePos = this.worldPosition + (marker.worldPosition - this.worldPosition) / 2;
        middlePos = CoordUtils.GetPositionFromLatitudeLongitude(CoordUtils.GetLatitudeFromPosition(middlePos), CoordUtils.GetLongitudeFromPosition(middlePos));
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        line.lineRenderer = lineRenderer;
        line.originalPoints = positions.ToArray();
        line.originalMiddlePoint = middlePos;
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
        //markerObject.GetComponent<MeshRenderer>().material.color = Color.green;
        markerObject.GetComponent<SpriteRenderer>().color = Color.green;
        if (lastSelectedMarker != null && lastSelectedMarker != this)
        {
            lastSelectedMarker.GetMarkerObject().GetComponent<SpriteRenderer>().color = Color.blue;
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
    public List<LineObject> GetJoinedLines()
    {
        return this.joinedLines;
    }
    public static Marker GetLastSelectedMarker()
    {
        return lastSelectedMarker;
    }

}
