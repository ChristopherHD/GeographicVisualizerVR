using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        this.mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        transform.rotation = mainCamera.transform.rotation;

        // other type of billboard
        /*transform.LookAt(camera.transform);
        transform.Rotate(0, 180, 0);*/
    }
}
