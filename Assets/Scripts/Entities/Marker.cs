using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathd;
public class Marker
{
    private float lat;
    private float lon;
    private List<LineObject> joinedLines;
    private Vector3 worldPosition;
    private GameObject markerObject;
    
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
        markerObject = new GameObject();
        markerObject.layer = LayerMask.NameToLayer("Marker");
        markerObject.AddComponent<MarkerObject>();
        markerObject.GetComponent<MarkerObject>().marker = this;
        markerObject.AddComponent<Billboard>();
        markerObject.transform.parent = markersContainer.transform;
        markerObject.transform.position = this.worldPosition;
        markerObject.transform.localScale = Vector3.zero; // it'll scale inmediatly after
        SphereCollider collider = markerObject.AddComponent<SphereCollider>();
        collider.radius = 1.3f;
        Debug.Log(lat + " :: " + lon);
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
        float textScaleFactor = 1f / 2f;
        background.transform.localScale = new Vector3(40, 10, 1) * textScaleFactor;

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
        text.fontSize = (int) (72 * textScaleFactor);

        int linePositions = 2 + (int) (Math.Max(Math.Abs(this.lat - marker.lat), Math.Abs(this.lon - marker.lon)));
        LineObject line = lineObject.AddComponent<LineObject>();
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        List<Vector3> positions = new List<Vector3>();

        Vector3 middlePos = this.worldPosition + (marker.worldPosition - this.worldPosition) / 2;
        middlePos = CoordUtils.GetPositionFromLatitudeLongitude(CoordUtils.GetLatitudeFromPosition(middlePos), CoordUtils.GetLongitudeFromPosition(middlePos));
        positions.Add(marker.worldPosition);
        for (int i = 1; i < linePositions ; i++)
        {
            Vector3 direction1 = -this.worldPosition.normalized;
            Vector3 direction2 = -marker.worldPosition.normalized;

            // Using Mathf.Clamp to fix some NaN values generated with high LoD
            float angle = Mathf.Rad2Deg * Mathf.Acos(Mathf.Clamp(Vector3.Dot(direction2, direction1), -1, 1)) * i / linePositions;
            Vector3 axis = Vector3.Cross(direction2, direction1).normalized;
            Vector3 pos = Quaternion.AngleAxis(angle, axis) * -direction2;
            pos = CoordUtils.GetPositionFromLatitudeLongitude(CoordUtils.GetLatitudeFromPosition(pos), CoordUtils.GetLongitudeFromPosition(pos));
            
            float distance1 = (middlePos - this.worldPosition).magnitude;
            float distance2 = (middlePos - pos).magnitude;
            // In some high LoD cases, points are generated further away than they should because of precision things related
            // with angles too close between the markers, they are not necessary at that LoD anyway, we descart it
            if (distance2 > distance1)
            {
                Debug.Log("Point outside the line, discarting");
                continue;
            }

            positions.Add(pos);
        }
        positions.Add(this.worldPosition);
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
        // double chordLength = 2 * Asin(Sqrt(chordLengthSquared));

        // --- Vincenty
        double chordLength = Atan2(Sqrt(Pow(Cos(radianLat2) * Sin(diffLon), 2)
            + Pow(Cos(radianLat1) * Sin(radianLat2) - Sin(radianLat1) * Cos(radianLat2) * Cos(diffLon), 2))
            , Sin(radianLat1) * Sin(radianLat2) + Cos(radianLat1) * Cos(radianLat2) * Cos(diffLon));
        double meanEarthRadius = 6371.009;
        return chordLength * meanEarthRadius;
    }

    public void Select()
    {
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
