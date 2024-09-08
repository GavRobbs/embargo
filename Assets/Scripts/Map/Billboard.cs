using UnityEngine;

public class Billboard : MonoBehaviour {
    private Transform mainCamera;

    private void Start() {
        // Get the main camera in the scene
        mainCamera = Camera.main.transform;
    }

    void LateUpdate() {
        // Make the object face the camera
        transform.LookAt(transform.position + mainCamera.forward);
    }
}