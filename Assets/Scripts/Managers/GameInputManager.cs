using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameInputManager : MonoBehaviour, IMessageHandler {
    private float _camRotDir;
    private bool _isCameraRotating;
    private bool _isCameraZooming;
    private bool _isCameraPanning;

    private float _camZoomDirection;
    private Vector2 _camPanDirection;

    [FormerlySerializedAs("camera_rotation_speed")] [SerializeField]
    private float cameraRotationSpeed;

    [FormerlySerializedAs("camera_zoom_speed")] [SerializeField]
    private float cameraZoomSpeed;

    [FormerlySerializedAs("camera_pan_speed")] [SerializeField]
    private float cameraPanSpeed;

    [SerializeField] private AudioSource gameMusic;

    [SerializeField] private AudioSource gameOverMusic;

    [SerializeField] private AudioSource victoryMusic;

    [SerializeField] private AudioSource bossBattleMusic;

    private float _rotationAngle;

    private bool _started;
    private bool _suspend;

    private bool _doRaycast;

    private ISelectable _currentSelectable;

    private IHoverable _currentHoverable;
    private float _toHoverTime = 0.7f;

    public enum HoverMode {
        INFO,
        BUILD,
        HIGHLIGHT,
        UPGRADE,
        SCRAP
    };

    private HoverMode _currentHoverMode = HoverMode.INFO;

    //The click mode can either be to place a building or to do nothing
    private enum ClickMode {
        NONE,
        PLACE,
        SELECT
    };

    private ClickMode _currentClickMode = ClickMode.NONE;


    private GameObject _currentTurretPfb;

    private Camera _mainCamera;

    private void Start() {
        MessageDispatcher.GetInstance().AddHandler(this);
        _mainCamera = Camera.main;
    }

    private void OnDestroy() {
        MessageDispatcher.GetInstance().RemoveHandler(this);
    }

    // Update is called once per frame
    private void Update() {
        if (_suspend) {
            return;
        }

        if (_isCameraRotating) {
            float new_angle = Time.deltaTime * cameraRotationSpeed * _camRotDir;
            _rotationAngle += new_angle;
            _mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, new_angle);
        }

        if (_isCameraZooming) {
            _mainCamera.fieldOfView =
                Mathf.Clamp(_mainCamera.fieldOfView + cameraZoomSpeed * Time.deltaTime * _camZoomDirection, 30.0f,
                    90.0f);
        }

        if (_isCameraPanning) {
            Vector3 new_forward = Quaternion.Euler(0.0f, _rotationAngle, 0.0f) * Vector3.forward;
            Vector3 new_pos = _mainCamera.transform.position +
                              _mainCamera.transform.right * (cameraPanSpeed * Time.deltaTime * _camPanDirection.x) +
                              new_forward * (cameraPanSpeed * Time.deltaTime * _camPanDirection.y);
            new_pos.x = Mathf.Clamp(new_pos.x, -20.0f, 20.0f);
            new_pos.z = Mathf.Clamp(new_pos.z, -20.0f, 20.0f);
            _mainCamera.transform.position = new_pos;
        }

        if (_started) {
            CheckMouseHovering();
            CheckClick();
        }
    }

    private void CheckClick() {
        //Stubs at the moment
        if (!_doRaycast) return;

        _doRaycast = false;
        //This is a bit misleading, this actually checks to make sure we're not clicking on a UI element
        if (EventSystem.current.IsPointerOverGameObject()) {
            //don't handle this if the pointer is not over a game object
            return;
        }

        switch (_currentClickMode) {
            case ClickMode.NONE:
                return;
            case ClickMode.PLACE: {
                Vector2 mouse_pos = Mouse.current.position.ReadValue();
                Ray ray = _mainCamera.ScreenPointToRay(mouse_pos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building"))) {
                    Building b = hit.collider.GetComponentInParent<Building>();
                    if (b == null) {
                        //Throw an error about not being a valid building
                        MessageDispatcher.GetInstance()
                            .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                                "Not a valid building!"));
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageBuildMode));
                        return;
                    }

                    if (b.hasTurret || b.building_turret) {
                        //Throw an error about the building already having a turret
                        MessageDispatcher.GetInstance()
                            .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                                "Building already has a turret!"));
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageBuildMode));
                        return;
                    }

                    MessageDispatcher.GetInstance().Dispatch(new DroneBuildMessage(b));
                    MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageBuildMode));
                }

                break;
            }
            case ClickMode.SELECT: {
                Vector2 mouse_pos = Mouse.current.position.ReadValue();
                Ray ray = _mainCamera.ScreenPointToRay(mouse_pos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building", "Turret"))) {
                    Building building = hit.collider.GetComponentInParent<Building>();
                    if (building) {
                        if (building.hasTurret) {
                            if (_currentHoverMode == HoverMode.UPGRADE &&
                                (!building.upgrading_turret || !building.scrapping_turret)) {
                                MessageDispatcher.GetInstance()
                                    .Dispatch(
                                        new SingleValueMessage<Building>(MessageConstants.UpgradeTurret, building));
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new GameMessage(MessageConstants.DisengageUpgradeMode));
                            } else if (_currentHoverMode == HoverMode.SCRAP &&
                                       (!building.upgrading_turret || !building.scrapping_turret)) {
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new SingleValueMessage<Building>(MessageConstants.ScrapTurret, building));
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new GameMessage(MessageConstants.DisengageScrapMode));
                            }
                        } else {
                            if (_currentHoverMode == HoverMode.UPGRADE) {
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                                        "No valid target to upgrade!"));
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new GameMessage(MessageConstants.DisengageUpgradeMode));
                            } else if (_currentHoverMode == HoverMode.SCRAP) {
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                                        "No valid target to scrap!"));
                                MessageDispatcher.GetInstance()
                                    .Dispatch(new GameMessage(MessageConstants.DisengageScrapMode));
                            }
                        }
                    } else {
                        ITurret t = hit.collider.GetComponentInParent<ITurret>();

                        switch (_currentHoverMode) {
                            case HoverMode.UPGRADE: {
                                if (t != null && (!t.AttachedBuilding.upgrading_turret ||
                                                  !t.AttachedBuilding.scrapping_turret)) {
                                    MessageDispatcher.GetInstance()
                                        .Dispatch(new SingleValueMessage<Building>(MessageConstants.UpgradeTurret,
                                            t.AttachedBuilding));
                                } else {
                                    MessageDispatcher.GetInstance()
                                        .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                                            "No valid target to upgrade!"));
                                }

                                MessageDispatcher.GetInstance()
                                    .Dispatch(new GameMessage(MessageConstants.DisengageUpgradeMode));
                                break;
                            }
                            case HoverMode.SCRAP: {
                                if (t != null && (!t.AttachedBuilding.upgrading_turret ||
                                                  !t.AttachedBuilding.scrapping_turret)) {
                                    MessageDispatcher.GetInstance()
                                        .Dispatch(new SingleValueMessage<Building>(MessageConstants.ScrapTurret,
                                            t.AttachedBuilding));
                                } else {
                                    MessageDispatcher.GetInstance()
                                        .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                                            "No valid target to scrap!"));
                                }

                                MessageDispatcher.GetInstance()
                                    .Dispatch(new GameMessage(MessageConstants.DisengageScrapMode));
                                break;
                            }
                        }
                    }
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckMouseHovering() {
        //Stubs at the moment
        Vector2 mouse_pos = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mouse_pos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000.0f, LayerMask.GetMask("Building", "Turret", "Enemy", "Drone"))) {
            IHoverable hoverable = hit.collider.GetComponentInParent<IHoverable>();

            switch (_currentHoverMode) {
                case HoverMode.INFO when _currentHoverable == null:
                    _currentHoverable = hoverable;
                    _toHoverTime = 0.7f;
                    break;
                case HoverMode.INFO when _currentHoverable == hoverable: {
                    if (_toHoverTime > 0.0f) {
                        _toHoverTime -= Time.deltaTime;
                        if (_toHoverTime <= 0.0f) {
                            //This function should open the hover window
                            MessageDispatcher.GetInstance()
                                .Dispatch(new HoverInfoDisplayMessage(hoverable.GetHoverData(),
                                    Mouse.current.position.ReadValue()));
                            _currentHoverable.OnHoverOver(null);
                        }
                    } else {
                        MessageDispatcher.GetInstance().Dispatch(new HoverInfoDisplayMessage(hoverable.GetHoverData(),
                            Mouse.current.position.ReadValue()));
                    }

                    break;
                }
                case HoverMode.INFO:
                    _currentHoverable.OnHoverOff();
                    //This function should close the hover window
                    MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.HideHoverPopupMessage));
                    _currentHoverable = hoverable;
                    _toHoverTime = 0.7f;
                    break;
                case HoverMode.BUILD when hoverable == null ||
                                          hit.collider.gameObject.layer != LayerMask.NameToLayer("Building"):
                    return;
                case HoverMode.BUILD: {
                    if (hoverable != _currentHoverable) {
                        _currentHoverable?.OnHoverOff();
                        _currentHoverable = hoverable;
                        _currentHoverable.OnHoverOver(new BuildHoverInfo(_currentTurretPfb));
                    }

                    break;
                }
                case HoverMode.UPGRADE when hoverable == null ||
                                            (hit.collider.gameObject.layer != LayerMask.NameToLayer("Building") &&
                                             hit.collider.gameObject.layer != LayerMask.NameToLayer("Turret")):
                    return;
                case HoverMode.UPGRADE:
                    _currentHoverable?.OnHoverOff();
                    _currentHoverable = hoverable;
                    _currentHoverable.OnHoverOver(new HoverInfo(HoverMode.UPGRADE, 0));
                    break;
                case HoverMode.SCRAP when hoverable == null ||
                                          (hit.collider.gameObject.layer != LayerMask.NameToLayer("Building") &&
                                           hit.collider.gameObject.layer != LayerMask.NameToLayer("Turret")):
                    return;
                case HoverMode.SCRAP:
                    _currentHoverable?.OnHoverOff();
                    _currentHoverable = hoverable;
                    _currentHoverable.OnHoverOver(new HoverInfo(HoverMode.SCRAP, 0));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } else {
            if (_currentHoverable != null) {
                _currentHoverable.OnHoverOff();
                _currentHoverable = null;
                MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.HideHoverPopupMessage));
            }


            _toHoverTime = 0.7f;
        }
    }

    public void OnClick(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started) {
            _doRaycast = true;
        } else if (callbackContext.canceled) {
            _doRaycast = false;
        }
    }

    public void OnRightClick(InputAction.CallbackContext callbackContext) {
        //Right click cancels a selected action
        if (!callbackContext.started) return;
        if (_currentHoverable != null) {
            _currentHoverable.OnHoverOff();
            _currentHoverable = null;
        }

        _currentHoverMode = HoverMode.INFO;
        _currentClickMode = ClickMode.NONE;
        _toHoverTime = 0.7f;
        _currentTurretPfb = null;

        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.DisengageEverything));
    }

    public void OnRotate(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started || callbackContext.performed) {
            _camRotDir = callbackContext.ReadValue<float>();
            _isCameraRotating = true;
        }

        if (!callbackContext.canceled) return;

        _camRotDir = 0.0f;
        _isCameraRotating = false;
    }

    public void OnZoom(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started || callbackContext.performed) {
            _camZoomDirection = callbackContext.ReadValue<float>();
            _isCameraZooming = true;
        }

        if (!callbackContext.canceled) return;
        _camZoomDirection = 0.0f;
        _isCameraZooming = false;
    }

    public void OnPan(InputAction.CallbackContext callbackContext) {
        if (callbackContext.started || callbackContext.performed) {
            _camPanDirection = callbackContext.ReadValue<Vector2>();
            _isCameraPanning = true;
        }

        if (!callbackContext.canceled) return;
        _camPanDirection = Vector2.zero;
        _isCameraPanning = false;
    }

    public void OnResetZoom(InputAction.CallbackContext callbackContext) {
        if (!callbackContext.started && !callbackContext.performed) return;
        _isCameraZooming = false;
        _mainCamera.fieldOfView = 60.0f;
    }

    public void OnResetRotation(InputAction.CallbackContext callbackContext) {
        if (!callbackContext.started && !callbackContext.performed) return;
        _isCameraRotating = false;
        _mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, -_rotationAngle);
        _rotationAngle = 0.0f;
    }

    public void OnPause(InputAction.CallbackContext callbackContext) {
        SceneManager.LoadScene(0);
    }

    private IEnumerator GameOver() {
        yield return new WaitForSeconds(1.0f);
        gameMusic.Stop();
        gameOverMusic.Play();
        yield return new WaitWhile(() => gameOverMusic.isPlaying);
        SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
    }

    private IEnumerator BoostMusic() {
        gameMusic.pitch = 1f; // Pitch rises straight away

        // Volume rises in steps
        const int steps = 10;
        var volumeDiff = (1f - gameMusic.volume) / steps;

        for (var i = 0; i < steps; i++) {
            gameMusic.volume += volumeDiff;
            yield return new WaitForSeconds(0.1f);
        }

        // Set explicitly to 1 to avoid rounding errors
        gameMusic.volume = 1f;
    }

    private static IEnumerator LoadVictoryScreen() {
        yield return new WaitForSeconds(10.0f);
        SceneManager.LoadScene(3);
    }

    public void HandleMessage(GameMessage message) {
        if (_suspend) {
            return;
        }

        switch (message.MessageType) {
            case MessageConstants.StartGameMessage:
                StartCoroutine("BoostMusic");
                _started = true;
                break;
            case MessageConstants.EngageBuildMode: {
                _currentHoverMode = HoverMode.BUILD;
                _currentClickMode = ClickMode.PLACE;
                _currentTurretPfb = (message as EngageBuildModeMessage)?.turret_prefab;
                break;
            }
            case MessageConstants.DisengageBuildMode: {
                _currentHoverMode = HoverMode.INFO;
                _currentClickMode = ClickMode.NONE;
                _currentTurretPfb = null;
                break;
            }
            case MessageConstants.GameOverMessage: {
                StartCoroutine(GameOver());
                break;
            }
            case MessageConstants.EngageUpgradeMode: {
                _currentHoverMode = HoverMode.UPGRADE;
                _currentClickMode = ClickMode.SELECT;
                _currentTurretPfb = null;
                break;
            }
            case MessageConstants.DisengageUpgradeMode: {
                _currentHoverMode = HoverMode.INFO;
                _currentClickMode = ClickMode.NONE;
                _currentTurretPfb = null;
                break;
            }
            case MessageConstants.EngageScrapMode: {
                _currentHoverMode = HoverMode.SCRAP;
                _currentClickMode = ClickMode.SELECT;
                _currentTurretPfb = null;
                break;
            }
            case MessageConstants.DisengageScrapMode: {
                _currentHoverMode = HoverMode.INFO;
                _currentClickMode = ClickMode.NONE;
                _currentTurretPfb = null;
                break;
            }
            case MessageConstants.WonGameMessage: {
                _suspend = true;
                _currentHoverable = null;
                _currentClickMode = ClickMode.NONE;
                _currentHoverMode = HoverMode.INFO;
                bossBattleMusic.Stop();
                victoryMusic.Play();
                StartCoroutine(LoadVictoryScreen());
                break;
            }
            case MessageConstants.NotifyBossBattleMessage: {
                gameMusic.Stop();
                bossBattleMusic.Play();
                break;
            }
        }
    }
}