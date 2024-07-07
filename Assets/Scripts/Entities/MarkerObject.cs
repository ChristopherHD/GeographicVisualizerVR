using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MarkerObject : MonoBehaviour
{
    public Marker marker;
    public Billboard billboard;

    private static Sprite sprite;
    private static Camera mainCamera;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = this.transform.position;
        if (mainCamera == null) mainCamera = Camera.main;
        if(sprite == null) sprite = Resources.Load<Sprite>("Sprites/9-SlicedWithBorder");
        GameObject billboardObject = new GameObject();
        billboardObject.transform.parent = this.transform;
        billboardObject.transform.localPosition = new Vector3(0, 10, 0);
        billboard = billboardObject.AddComponent<Billboard>();
        billboardObject.AddComponent<SpriteRenderer>();
        billboardObject.GetComponent<SpriteRenderer>().sprite = MarkerObject.sprite;
    }

    private void Update()
    {
        RecalculatePosition();
        float distance = (mainCamera.transform.position - transform.position).magnitude;
        transform.localScale = new Vector3(distance, distance, distance) / 30f; // escala de 5 con 6560 de distancia, la proporción debe mantenerse, 150/5 = 30
        // para valor de escalado proporcional: 6560 = (Camera.main.transform.position - Vector3.zero).magnitude 
        foreach (LineRenderer line in marker.GetJoinedLines()){
            float centerDistance = (mainCamera.transform.position - line.transform.position).magnitude;
            float rightExtremeDistance = (mainCamera.transform.position - line.GetPosition(0)).magnitude;
            float leftExtremeDistance = (mainCamera.transform.position - line.GetPosition(line.positionCount - 1)).magnitude;
            float minDistance = Mathf.Min(centerDistance, rightExtremeDistance, leftExtremeDistance);
            line.widthMultiplier = minDistance / 50f;
            line.transform.localScale = new Vector3(minDistance, minDistance, minDistance) / 200f;
        }
        //Debug.Log((mainCamera.transform.position - transform.position).magnitude); // 150
    }

    private void RecalculatePosition()
    {
        Physics.Raycast(this.transform.position, (-this.transform.position).normalized, out RaycastHit cameraRaycastHit, Mathf.Infinity);
        // can't assign the same position, sometimes the object go through the globe
       // Vector3 originPosition = this.transform.position;
        this.transform.position = cameraRaycastHit.point + (this.transform.position).normalized / 300;

        Vector3 diffPosition = originalPosition - this.transform.position;
        

        foreach (LineRenderer line in marker.GetJoinedLines())
        {
            line.transform.position -= diffPosition;
            List<Vector3> linePositions = new List<Vector3>();
            for (int i = 0; i < line.positionCount; i++) {
                linePositions.Add(line.GetPosition(i) - diffPosition);
            }
            line.SetPositions(linePositions.ToArray());
        }
    }
}
