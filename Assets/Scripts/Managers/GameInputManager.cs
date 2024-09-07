using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Processors;
using System;

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

    [SerializeField]
    AudioSource gameMusic;

    [SerializeField]
    AudioSource gameOverMusic;

    float rotation_angle = 0.0f;

   

    bool doRaycast = false;

    ISelectable current_selectable;

    IHoverable current_hoverable = null;
    float to_hover_time = 0.7f;

    public enum HOVER_MODE : int { INFO, BUILD, HIGHLIGHT, UPGRADE, SCRAP };
    HOVER_MODE current_hover_mode = HOVER_MODE.INFO;

    //The click mode can either be to place a building or to do nothing
    public enum CLICK_MODE : int {  NONE, PLACE, SELECT };
    CLICK_MODE current_click_mode = CLICK_MODE.NONE;

    GameMap map_manager;

    GameObject current_turret_pfb;

    void Start()
    {
        map_manager = FindObjectOfType<GameMap>();
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
                //don't handle this if the pointer is not over a game object
                return;
            }

            if (current_click_mode == CLICK_MODE.NONE)
            {
                return;
            }
            else if(current_click_mode == CLICK_MODE.PLACE)
            {
                Vector2 mouse_pos = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mouse_pos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building")))
                {
                    Building b = hit.collider.GetComponentInParent<Building>();
                    if(b == null)
                    {
                        //Throw an error about not being a valid building
                        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "Not a valid building!"));
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageBuildMode));
                        return;
                    }

                    if (b.hasTurret || b.building_turret)
                    {
                        //Throw an error about the building already having a turret
                        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "Building already has a turret!"));
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageBuildMode));
                        return;
                    }

                    MessageDispatcher.GetInstance().Dispatch(new DroneBuildMessage(b));
                    MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageBuildMode));

                }
            } 
            else if(current_click_mode == CLICK_MODE.SELECT)
            {
                Vector2 mouse_pos = Mouse.current.position.ReadValue();
                Ray ray = Camera.main.ScreenPointToRay(mouse_pos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building", "Turret")))
                {
                    Building b = hit.collider.GetComponentInParent<Building>();
                    if (b != null)
                    {
                        if (b.hasTurret)
                        {
                            if (current_hover_mode == HOVER_MODE.UPGRADE && (!b.upgrading_turret || !b.scrapping_turret) )
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<Building>(MessageConstants.UpgradeTurret, b));
                                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageUpgradeMode));
                            } else if (current_hover_mode == HOVER_MODE.SCRAP && (!b.upgrading_turret || !b.scrapping_turret))
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<Building>(MessageConstants.ScrapTurret, b));
                                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageScrapMode));
                            }

                        }
                        else
                        {
                            if (current_hover_mode == HOVER_MODE.UPGRADE)
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "No valid target to upgrade!"));
                                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageUpgradeMode));
                            }
                            else if (current_hover_mode == HOVER_MODE.SCRAP)
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "No valid target to scrap!"));
                                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageScrapMode));
                            }
                        }
                        return;
                    }
                    else
                    {
                        ITurret t = hit.collider.GetComponentInParent<ITurret>();

                        if (current_hover_mode == HOVER_MODE.UPGRADE)
                        {
                            if(t != null && (!t.AttachedBuilding.upgrading_turret || !t.AttachedBuilding.scrapping_turret))
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<Building>(MessageConstants.UpgradeTurret, t.AttachedBuilding));

                            }
                            else
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "No valid target to upgrade!"));
                            }
                            MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageUpgradeMode));
                        }
                        else if(current_hover_mode == HOVER_MODE.SCRAP)
                        {
                            if(t != null && (!t.AttachedBuilding.upgrading_turret || !t.AttachedBuilding.scrapping_turret))
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<Building>(MessageConstants.ScrapTurret, t.AttachedBuilding));
                            }
                            else
                            {
                                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "No valid target to scrap!"));
                            }
                            MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageScrapMode));

                        }
                        
                    }


                }

            }

        }

    }

    private void CheckMouseHovering()
    {
        //Stubs at the moment
        Vector2 mouse_pos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouse_pos);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building", "Turret", "Enemy", "Drone")))
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
                            current_hoverable.OnHoverOver(null);
                        }
                    }
                    else
                    {
                        MessageDispatcher.GetInstance().Dispatch(new HoverInfoDisplayMessage(hoverable.GetHoverData(), Mouse.current.position.ReadValue()));
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
            else if(current_hover_mode == HOVER_MODE.BUILD)
            {
                if(hoverable == null || hit.collider.gameObject.layer != LayerMask.NameToLayer("Building"))
                {
                    return;
                }
                
                if(hoverable != current_hoverable)
                {
                    current_hoverable?.OnHoverOff();
                    current_hoverable = hoverable;
                    current_hoverable.OnHoverOver(new BuildHoverInfo(current_turret_pfb));
                }
            }
            else if(current_hover_mode == HOVER_MODE.UPGRADE)
            {
                if (hoverable == null || (hit.collider.gameObject.layer != LayerMask.NameToLayer("Building") &&  hit.collider.gameObject.layer != LayerMask.NameToLayer("Turret")))
                {
                    return;
                }

                current_hoverable?.OnHoverOff();
                current_hoverable = hoverable;
                current_hoverable.OnHoverOver(new HoverInfo(HOVER_MODE.UPGRADE, 0));

            }
            else if (current_hover_mode == HOVER_MODE.SCRAP)
            {
                if (hoverable == null || (hit.collider.gameObject.layer != LayerMask.NameToLayer("Building") && hit.collider.gameObject.layer != LayerMask.NameToLayer("Turret")))
                {
                    return;
                }

                current_hoverable?.OnHoverOff();
                current_hoverable = hoverable;
                current_hoverable.OnHoverOver(new HoverInfo(HOVER_MODE.SCRAP, 0));

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
            doRaycast = true;
        }
        else if (callbackContext.canceled)
        {
            doRaycast = false;
        }
    }

    public void OnRightClick(InputAction.CallbackContext callbackContext)
    {
        //Right click cancels a selected action
        if (callbackContext.started)
        {
            if(current_hoverable != null)
            {
                current_hoverable.OnHoverOff();
                current_hoverable = null;
            }

            current_hover_mode = HOVER_MODE.INFO;
            current_click_mode = CLICK_MODE.NONE;
            to_hover_time = 0.7f;
            current_turret_pfb = null;

            MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageEverything));
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

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1.0f);
        gameMusic.Stop();
        gameOverMusic.Play();
        yield return new WaitWhile(() => gameOverMusic.isPlaying);
        SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
    }

    IEnumerator BoostMusic()
    {
        gameMusic.pitch = 1f;  // Pitch rises straight away

        // Volume rises in steps
        var steps = 10;
        var volumeDiff = (1f - gameMusic.volume) / steps;

        for (int i = 0; i < steps; i++)
        {
            gameMusic.volume += volumeDiff;
            yield return new WaitForSeconds(0.1f);
        }

        // Set explicitly to 1 to avoid rounding errors
        gameMusic.volume = 1f;
    }

    public void HandleMessage(GameMessage message)
    {
        switch (message.MessageType)
        {
            case MessageConstants.StartGameMessage:
                StartCoroutine("BoostMusic");
                break;
            case MessageConstants.EngageBuildMode:
                {
                    current_hover_mode = HOVER_MODE.BUILD;
                    current_click_mode = CLICK_MODE.PLACE;
                    current_turret_pfb = (message as EngageBuildModeMessage).turret_prefab;
                    break;
                }
            case MessageConstants.DisengageBuildMode:
                {
                    current_hover_mode = HOVER_MODE.INFO;
                    current_click_mode = CLICK_MODE.NONE;
                    current_turret_pfb = null;
                    break;
                }
            case MessageConstants.GameOverMessage:
                {
                    StartCoroutine(GameOver());
                    break;
                }
            case MessageConstants.EngageUpgradeMode:
                {
                    current_hover_mode = HOVER_MODE.UPGRADE;
                    current_click_mode = CLICK_MODE.SELECT;
                    current_turret_pfb = null;
                    break;
                }
            case MessageConstants.DisengageUpgradeMode:
                {
                    current_hover_mode = HOVER_MODE.INFO;
                    current_click_mode = CLICK_MODE.NONE;
                    current_turret_pfb = null;
                    break;
                }
            case MessageConstants.EngageScrapMode:
                {
                    current_hover_mode = HOVER_MODE.SCRAP;
                    current_click_mode = CLICK_MODE.SELECT;
                    current_turret_pfb = null;
                    break;
                }
            case MessageConstants.DisengageScrapMode:
                {
                    current_hover_mode = HOVER_MODE.INFO;
                    current_click_mode = CLICK_MODE.NONE;
                    current_turret_pfb = null;
                    break;
                }
        }
    }
}
