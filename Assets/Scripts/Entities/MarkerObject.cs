using System.Collections.Generic;
using UnityEngine;

public class MarkerObject : MonoBehaviour
{
    public Marker marker;
    public Billboard billboard;
    public float markerScaleFactor = 1f / 85f;

    private Sprite markerSprite;
    private Vector3 tileRayCastHitPosition = Vector3.zero;
    private float scaleFactor = UVSphereGenerator.radiusStatic / 127.42f;

    private static Camera mainCamera;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        GameObject billboardObject = new GameObject();
        billboardObject.transform.parent = this.transform;
        billboardObject.transform.localPosition = new Vector3(0, 10, 0);
        billboard = billboardObject.AddComponent<Billboard>();
        billboardObject.AddComponent<SpriteRenderer>();

        SpriteRenderer renderer = this.gameObject.AddComponent<SpriteRenderer>();
        Texture2D textureMarker = Resources.Load<Texture2D>("Sprites/Marker");
        markerSprite = Sprite.Create(textureMarker, new Rect(0, 0, textureMarker.width, textureMarker.height), Vector2.one / 2);
        renderer.sprite = markerSprite;
    }

    void Update()
    {
        RecalculatePosition();
        float distance = (mainCamera.transform.position - transform.position).magnitude;
        transform.localScale = new Vector3(distance, distance, distance) * markerScaleFactor;
    }

    private float CameraDistanceModifierFunction(float distance)
    {
        float value = 5 / (2 * UVSphereGenerator.radiusStatic) * distance - 5f / 2f;
        return value < 0 ? 0.0001f : value;
    }
    private void RecalculatePosition()
    {
        Vector3 direction = this.marker.GetWorldPosition().normalized;
        if (tileRayCastHitPosition.Equals(Vector3.zero))
        {
            int layerMask = 1 << LayerMask.NameToLayer("Marker");
            Physics.Raycast(this.marker.GetWorldPosition(), -direction, out RaycastHit raycastHit, Mathf.Infinity, ~layerMask);
            tileRayCastHitPosition = raycastHit.point;
            marker.setWorldPositionAdjusted(tileRayCastHitPosition);
        } else
        {
            this.transform.position = tileRayCastHitPosition + direction / (scaleFactor * 5) + 1.5f * scaleFactor * CameraDistanceModifierFunction(mainCamera.transform.position.magnitude) * direction;
        }
    }
}
