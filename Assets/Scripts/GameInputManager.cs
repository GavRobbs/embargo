using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameInputManager : MonoBehaviour, IMessageHandler
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

   

    bool doRaycast = false;

    ISelectable current_selectable;

    IHoverable current_hoverable = null;
    float to_hover_time = 0.7f;

    public enum HOVER_MODE : int { INFO, BUILD, HIGHLIGHT };
    HOVER_MODE current_hover_mode = HOVER_MODE.INFO;

    [SerializeField]
    GameMap map_manager;

    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);
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
            //This is a bit misleading, this actually checks to make sure we're not clicking on a UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Pointer over UI element");
                //don't handle this if the pointer is not over a game object
                return;
            }

            Debug.Log("Pointer over game screen");
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

        if(Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building", "Turret", "Enemy")))
        {
            IHoverable hoverable = hit.collider.GetComponentInParent<IHoverable>();

            if(current_hover_mode == HOVER_MODE.INFO)
            {
                if (current_hoverable == null)
                {
                    current_hoverable = hoverable;
                    to_hover_time = 0.7f;
                }
                else if (current_hoverable == hoverable)
                {
                    if (to_hover_time > 0.0f)
                    {
                        to_hover_time -= Time.deltaTime;
                        if (to_hover_time <= 0.0f)
                        {
                            //This function should open the hover window
                            MessageDispatcher.GetInstance().Dispatch(new HoverInfoDisplayMessage(hoverable.GetHoverData(), Mouse.current.position.ReadValue()));
                            current_hoverable.OnHoverOver();
                        }
                    }

                }
                else
                {
                    current_hoverable.OnHoverOff();
                    //This function should close the hover window
                    MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.HideHoverPopupMessage));
                    current_hoverable = hoverable;
                    to_hover_time = 0.7f;
                }

            }           

        }
        else
        {
            if(current_hoverable != null)
            {
                current_hoverable.OnHoverOff();
                current_hoverable = null;
                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.HideHoverPopupMessage));
            }


            to_hover_time = 0.7f;
        }

    }

    public void OnClick(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            Debug.Log("Clicked!");
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

    public void OnPause(InputAction.CallbackContext callbackContext)
    {
        SceneManager.LoadScene(0);
    }

    public void HandleMessage(GameMessage message)
    {
    }
}
