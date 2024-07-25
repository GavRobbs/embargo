using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class GameInputManager : MonoBehaviour
{
    float cam_rot_dir = 0.0f;
    bool isCameraRotating = false;
    bool isCameraZooming = false;
    bool isCameraPanning = false;

    float cam_zoom_direction = 0.0f;
    Vector2 cam_pan_direction;

    [SerializeField]
    float camera_rotation_speed;

    [SerializeField]
    float camera_zoom_speed;

    [SerializeField]
    float camera_pan_speed;

    float rotation_angle = 0.0f;

    IHoverable current_hoverable = null;
    float to_hover_time = 0.3f;

    bool doRaycast = false;

    ISelectable current_selectable;

    [SerializeField]
    GameMap map_manager;

    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
        if (isCameraRotating)
        {
            float new_angle = Time.deltaTime * camera_rotation_speed * cam_rot_dir;
            rotation_angle += new_angle;
            Camera.main.transform.RotateAround(Vector3.zero, Vector3.up, new_angle);
        }

        if (isCameraZooming)
        {
            Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + camera_zoom_speed * Time.deltaTime * cam_zoom_direction, 30.0f, 90.0f);
        }

        if (isCameraPanning)
        {
            Vector3 new_forward = Quaternion.Euler(0.0f, rotation_angle, 0.0f) * Vector3.forward;
            Vector3 new_pos = Camera.main.transform.position + Camera.main.transform.right * camera_pan_speed * Time.deltaTime * cam_pan_direction.x + new_forward * camera_pan_speed * Time.deltaTime * cam_pan_direction.y;
            new_pos.x = Mathf.Clamp(new_pos.x, -20.0f, 20.0f);
            new_pos.z = Mathf.Clamp(new_pos.z, -20.0f, 20.0f);
            Camera.main.transform.position = new_pos;

        }

        CheckMouseHovering();
        CheckClick();

    }

    private void CheckClick()
    {
        //Stubs at the moment
        if (doRaycast)
        {
            doRaycast = false;
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //don't handle this if the pointer is over a game object
                return;
            }

            Vector2 mouse_pos = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mouse_pos);
            RaycastHit hit;
        }

    }

    private void CheckMouseHovering()
    {
        //Stubs at the moment
        Vector2 mouse_pos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouse_pos);
        RaycastHit hit;
    }

    public void OnClick(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            doRaycast = true;
        }
        else if (callbackContext.canceled)
        {
            doRaycast = false;
        }
    }

    public void OnRotate(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started || callbackContext.performed)
        {
            cam_rot_dir = callbackContext.ReadValue<float>();
            isCameraRotating = true;
        }

        if (callbackContext.canceled)
        {
            cam_rot_dir = 0.0f;
            isCameraRotating = false;
        }
    }

    public void OnZoom(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started || callbackContext.performed)
        {
            cam_zoom_direction = callbackContext.ReadValue<float>();
            isCameraZooming = true;
        }

        if (callbackContext.canceled)
        {
            cam_zoom_direction = 0.0f;
            isCameraZooming = false;
        }
    }

    public void OnPan(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started || callbackContext.performed)
        {
            cam_pan_direction = callbackContext.ReadValue<Vector2>();
            isCameraPanning = true;
        }

        if (callbackContext.canceled)
        {
            cam_pan_direction = Vector2.zero;
            isCameraPanning = false;
        }
    }

    public void OnResetZoom(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started || callbackContext.performed)
        {
            isCameraZooming = false;
            Camera.main.fieldOfView = 60.0f;
        }
    }

    public void OnResetRotation(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started || callbackContext.performed)
        {
            isCameraRotating = false;
            Camera.main.transform.RotateAround(Vector3.zero, Vector3.up, -rotation_angle);
            rotation_angle = 0.0f;
        }

    }

}
