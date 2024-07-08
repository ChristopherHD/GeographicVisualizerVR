using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPosition
{
    public Vector3d worldPosition;
    public double latitude;
    public double longitude;

    public MapPosition()
    {
        this.worldPosition = Vector3d.down;
        this.latitude = -200;
        this.longitude = -200;
    }

    public MapPosition(Vector3d position)
    {
        this.SetPosition(position);
    }

    public MapPosition(Vector3d position, double latitude, double longitude)
    {
        this.worldPosition = position;
        this.latitude = latitude;
        this.longitude = longitude;
    }

    public void SetPosition(Vector3 position)
    {
        SetPosition(new Vector3d(position));
    }
    public void SetPosition(Vector3d position)
    {
        GameObject worldObject = GameObject.FindWithTag("Player");
        Matrix4x4 localToWorld = worldObject.transform.worldToLocalMatrix;
        
        Vector3 worldRotationPosition = new Vector3((float)position.x, (float)position.y, (float)position.z);
        //Debug.Log("++ " + position);
        position = new Vector3d(localToWorld.MultiplyPoint3x4(worldRotationPosition));
        //Debug.Log("-- " + position);
        this.worldPosition = position;
        this.latitude = CoordUtils.GetLatitudeFromPosition(position);
        this.longitude = CoordUtils.GetLongitudeFromPosition(position);
        /*Debug.Log("lat: " + latitude);
        Debug.Log("lon: " + longitude);*/
    }
}
