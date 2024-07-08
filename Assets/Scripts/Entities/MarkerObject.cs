using System.Collections.Generic;
using UnityEngine;

public class MarkerObject : MonoBehaviour
{
    public Marker marker;
    public Billboard billboard;
    private Sprite sprite;
    private Sprite spriteCircle;
    private Vector3 tileRayCastHitPosition = Vector3.zero;

    private static Camera mainCamera;
    private Vector3 originalPosition;


    void Start()
    {
        originalPosition = this.transform.position;
        if (mainCamera == null) mainCamera = Camera.main;

        //this.gameObject.AddComponent<MeshRenderer>();
        //this.GetComponent<MeshRenderer>().material.color = Color.blue;
        
        SpriteRenderer renderer = this.gameObject.AddComponent<SpriteRenderer>();
        renderer.color = Color.blue;

        Texture2D textureCircle = Resources.Load<Texture2D>("Sprites/Circle");
        spriteCircle = Sprite.Create(textureCircle, new Rect(0, 0, textureCircle.width, textureCircle.height), Vector2.one / 2);
        Debug.Log(Sprite.Create(textureCircle, new Rect(0, 0, textureCircle.width, textureCircle.height), Vector2.one / 2));
        renderer.sprite = spriteCircle;

        GameObject billboardObject = new GameObject();
        billboardObject.transform.parent = this.transform;
        billboardObject.transform.localPosition = new Vector3(0, 10, 0);
        billboard = billboardObject.AddComponent<Billboard>();
        billboardObject.AddComponent<SpriteRenderer>();

        Texture2D texture = Resources.Load<Texture2D>("Sprites/9-SlicedWithBorder");
        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one / 2);
        billboardObject.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    private void Update()
    {
        RecalculatePosition();
        float distance = (mainCamera.transform.position - transform.position).magnitude;
        transform.localScale = new Vector3(distance, distance, distance) / 30f; // escala de 5 con 6560 de distancia, la proporción debe mantenerse, 150/5 = 30
        // para valor de escalado proporcional: 6560 = (Camera.main.transform.position - Vector3.zero).magnitude 
        foreach (LineObject lineObject in marker.GetJoinedLines()){
            LineRenderer line = lineObject.lineRenderer;
            float centerDistance = (mainCamera.transform.position - line.transform.position).magnitude;
            float rightExtremeDistance = (mainCamera.transform.position - line.GetPosition(0)).magnitude;
            float leftExtremeDistance = (mainCamera.transform.position - line.GetPosition(line.positionCount - 1)).magnitude;
            float minDistance = Mathf.Min(centerDistance, rightExtremeDistance, leftExtremeDistance);
            line.widthMultiplier = minDistance / 50f;
            line.transform.localScale = new Vector3(minDistance, minDistance, minDistance) / 200f;
        }
        //Debug.Log((mainCamera.transform.position - transform.position).magnitude); // 150
    }

    private float linearFunction(float distance)
    {
        float value = 5 / (2 * UVSphereGenerator.radiusStatic) * distance - 5f / 2f;
        return value < 0 ? 0 : value;
    }
    private void RecalculatePosition()
    {
        Vector3 direction = this.marker.GetWorldPosition().normalized;
        if (tileRayCastHitPosition.Equals(Vector3.zero))
        {
            int layerMask = 1 << LayerMask.NameToLayer("Marker");
            Physics.Raycast(this.marker.GetWorldPosition(), -direction, out RaycastHit raycastHit, Mathf.Infinity, ~layerMask);
            tileRayCastHitPosition = raycastHit.point;
        } else
        {
            this.transform.position = tileRayCastHitPosition + direction / 300 + 50 * linearFunction(mainCamera.transform.position.magnitude) * direction;
        }
    }
}
