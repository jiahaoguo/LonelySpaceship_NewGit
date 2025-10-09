using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // Cache the main camera
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Make the object face the camera
        transform.forward = mainCamera.transform.forward;
    }
}
    