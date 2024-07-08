using System.Collections.Generic;
using UnityEngine;

public class LineObject : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Vector3 originalMiddlePoint;
    public Vector3[] originalPoints;

    private Vector3[] raycastHitPositions;
    private Vector3 raycastHitMiddlePointPosition;

    private static Camera mainCamera;

    private void Start()
    {
        raycastHitPositions = new Vector3[originalPoints.Length];
        int layerMask = 1 << LayerMask.NameToLayer("Marker");
        if (mainCamera == null) mainCamera = Camera.main;
        for (int i = 0; i < originalPoints.Length; i++)
        {
            Vector3 direction = this.originalPoints[i].normalized;
            Physics.Raycast(this.originalPoints[i], -direction, out RaycastHit raycastHit, Mathf.Infinity, ~layerMask);
            raycastHitPositions[i] = raycastHit.point;
        }
        Vector3 directionMiddlePoint = this.originalMiddlePoint.normalized;
        Physics.Raycast(this.originalMiddlePoint, -directionMiddlePoint, out RaycastHit raycastHitMiddlePoint, Mathf.Infinity, ~layerMask);
        raycastHitMiddlePointPosition = raycastHitMiddlePoint.point;
    }

    void Update()
    {
        RecalculatePositions();
    }

    private void RecalculatePositions()
    {
        List<Vector3> linePositions = new List<Vector3>();
        for (int i = 0; i < originalPoints.Length; i++)
        {
            Vector3 direction = this.originalPoints[i].normalized;
            if (!raycastHitPositions[i].Equals(Vector3.zero))
            {
                linePositions.Add(raycastHitPositions[i] + direction / 310 + 50 * CameraDistanceModifierFunction(mainCamera.transform.position.magnitude) * direction);
            }
            else return; // all points should  collide with world mesh, if not, we shouldn't change the lineRenderer positions
        }
        lineRenderer.SetPositions(linePositions.ToArray());
        Vector3 directionMiddlePoint = this.originalMiddlePoint.normalized;
        this.transform.position = raycastHitMiddlePointPosition + directionMiddlePoint / 250 + 50 * CameraDistanceModifierFunction(mainCamera.transform.position.magnitude) * directionMiddlePoint;
    }

    private float CameraDistanceModifierFunction(float distance)
    {
        float value = 5 / (2 * UVSphereGenerator.radiusStatic) * distance - 5f / 2f;
        return value < 0 ? 0 : value;
    }
}
