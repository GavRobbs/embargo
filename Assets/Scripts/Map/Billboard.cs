using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCamera;

    void Start()
    {
        // Get the main camera in the scene
        mainCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Make the object face the camera
        transform.LookAt(transform.position + mainCamera.forward);
    }
}