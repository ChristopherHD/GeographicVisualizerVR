using System.Collections.Generic;
using UnityEngine;

public class LineObject : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Vector3 originalMiddlePoint;
    public Vector3[] originalPoints;
    public float lineWidthScaleFactor = 1f / 85f;

    private Vector3[] raycastHitPositions;
    private Vector3 raycastHitMiddlePointPosition;

    private static Camera mainCamera;

    void Start()
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

    void LateUpdate()
    {
        RecalculatePositions();
    }
    private void RecalculatePositions()
    {
        float minDistance = -1;
        List<Vector3> linePositions = new List<Vector3>();
        for (int i = 0; i < originalPoints.Length; i++)
        {
            Vector3 direction = this.originalPoints[i].normalized;
            if (!raycastHitPositions[i].Equals(Vector3.zero))
            {
                float lineVertexDistance = (mainCamera.transform.position - lineRenderer.GetPosition(i)).magnitude;
                if (minDistance < 0 || lineVertexDistance < minDistance) minDistance = lineVertexDistance;
                linePositions.Add(raycastHitPositions[i] + direction / 310 + 50 * CameraDistanceModifierFunction(mainCamera.transform.position.magnitude) * direction);
            }
            else return; // all points should collide with world mesh, if not, we shouldn't change the lineRenderer positions
        }
        lineRenderer.SetPositions(linePositions.ToArray());
        lineRenderer.widthMultiplier = minDistance * lineWidthScaleFactor;
        lineRenderer.transform.localScale = new Vector3(minDistance, minDistance, minDistance) / 200f;
        Vector3 directionMiddlePoint = this.originalMiddlePoint.normalized;
        this.transform.position = raycastHitMiddlePointPosition + directionMiddlePoint / 250 + 100 * CameraDistanceModifierFunction(mainCamera.transform.position.magnitude) * directionMiddlePoint;
    }

    private float CameraDistanceModifierFunction(float distance)
    {
        float value = 5 / (2 * UVSphereGenerator.radiusStatic) * distance - 5f / 2f;
        return value < 0 ? 0 : value;
    }
}
