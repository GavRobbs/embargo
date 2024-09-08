using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class CommandConsoleUI : MonoBehaviour, IMessageHandler {
    [SerializeField] private PopupManager popupManager;

    [SerializeField] private GameObject dangerWarning;

    [SerializeField] private GameObject clearMessage;

    [FormerlySerializedAs("timer_text")] [SerializeField]
    private TextMeshProUGUI timerText;

    [FormerlySerializedAs("scrap_counter")] [SerializeField]
    private TextMeshProUGUI scrapCounter;

    [SerializeField] private GameObject fader;

    [SerializeField] private AlertMessage alertMessage;

    [SerializeField] private TextMeshProUGUI waveCounter;

    private bool _updateScrapCounter = true;

    private int _scrap = 2000;

    private System.Action _onTimerComplete;
    private float _timerTime;
    private string _timerCaption;
    private bool _timerActive;
    private bool _showTime;

    private bool _bossKilled;

    // Cache the singleton instance
    private static readonly MessageDispatcher MessageDispatcher = MessageDispatcher.GetInstance();


    public void HandleMessage(GameMessage message) {
        switch (message.MessageType) {
            case MessageConstants.HideHoverPopupMessage: {
                popupManager.HideAllPopups();
                break;
            }
            case MessageConstants.HoverInfoDisplayMessage: {
                //Case and dispatch to the appropriate popup type
                var him = (HoverInfoDisplayMessage)message;
                if (him.info == null) {
                    return;
                }

                switch (him.info["type"]) {
                    case "building":
                        popupManager.ShowBuildingPopup(int.Parse(him.info["hp"]), him.display_position);
                        break;
                    case "enemy":
                        popupManager.ShowEnemyInfoPopup(him.info["name"], int.Parse(him.info["hp"]),
                            int.Parse(him.info["max_hp"]), him.display_position);
                        break;
                    case "offensive_turret":
                        popupManager.ShowOffensiveTurretBuildingPopup(him.info["name"], int.Parse(him.info["level"]),
                            int.Parse(him.info["bhp"]), float.Parse(him.info["atk_bonus"]),
                            float.Parse(him.info["cd_bonus"]), float.Parse(him.info["range_bonus"]),
                            him.display_position);
                        break;
                    case "support_turret":
                        popupManager.ShowDefensiveTurretBuildingPopup(him.info["name"], int.Parse(him.info["level"]),
                            int.Parse(him.info["bhp"]), him.display_position);
                        break;
                    case "drone":
                        popupManager.ShowDronePopup(him.display_position);
                        break;
                }

                break;
            }
            case MessageConstants.WaveAlertMessage: {
                dangerWarning.SetActive(true);
                dangerWarning.GetComponentInChildren<AudioSource>().Play();

                SetTimer("Under Attack ", 6.0f, false, () => {
                    dangerWarning.GetComponentInChildren<AudioSource>().Stop();
                    dangerWarning.SetActive(false);
                    MessageDispatcher.Dispatch(new GameMessage(MessageConstants.BeginWaveMessage));
                    SetTimer("Under Attack ", 60.0f, true, () => { });
                });
                break;
            }
            case MessageConstants.BossKilledMessage: {
                _bossKilled = true;
                break;
            }
            case MessageConstants.EndWaveMessage: {
                clearMessage.SetActive(true);
                clearMessage.GetComponentInChildren<AudioSource>().Play();

                SetTimer("Area Secure ", 4.0f, false, () => {
                    clearMessage.GetComponentInChildren<AudioSource>().Stop();
                    clearMessage.SetActive(false);

                    if (!_bossKilled) {
                        SetTimer("Enemy Incoming ", 30.0f, true,
                            () => { MessageDispatcher.Dispatch(new GameMessage(MessageConstants.WaveAlertMessage)); });
                    } else {
                        var capitol = FindObjectOfType<Capitol>();
                        if (capitol.Health > 0) {
                            MessageDispatcher.Dispatch(new GameMessage(MessageConstants.WonGameMessage));
                        }
                    }
                });
                break;
            }
            case MessageConstants.TriggerFirstWaveMessage: {
                Debug.Log("Triggering first wave");
                SetTimer("Enemy Incoming ", 30.0f, true,
                    () => { MessageDispatcher.Dispatch(new GameMessage(MessageConstants.WaveAlertMessage)); });
                break;
            }
            case MessageConstants.AddScrap: {
                _scrap += ((SingleValueMessage<int>)message).value;
                _updateScrapCounter = true;
                break;
            }
            case MessageConstants.RemoveScrap: {
                _scrap -= ((SingleValueMessage<int>)message).value;
                _updateScrapCounter = true;
                break;
            }
            case MessageConstants.GameOverMessage: {
                fader.SetActive(true);
                break;
            }
            case MessageConstants.DisplayAlertMessage: {
                string message_text = ((SingleValueMessage<string>)message).value;
                alertMessage.gameObject.SetActive(true);
                alertMessage.Display(message_text);
                break;
            }
            case MessageConstants.UpdateWaveCounterMessage: {
                int current_wave = ((SingleValueMessage<int>)message).value;
                waveCounter.text = $"Wave {current_wave}/6";
                break;
            }
        }
    }

    private void SetTimer(string text, float t, bool displayTime, System.Action onCompleteEvent) {
        _timerActive = true;
        _timerCaption = text;
        _timerTime = t;
        _showTime = displayTime;
        _onTimerComplete = onCompleteEvent;
    }

    // Start is called before the first frame update
    private void Start() {
        MessageDispatcher.AddHandler(this);
    }

    private void OnDestroy() {
        MessageDispatcher.RemoveHandler(this);
    }

    // Update is called once per frame
    private void Update() {
        if (_updateScrapCounter) {
            scrapCounter.text = "Scrap: " + _scrap;
            _updateScrapCounter = false;
        }

        if (!_timerActive) return;

        _timerTime -= Time.deltaTime;
        if (_timerTime <= 0.0f) {
            _timerActive = false;
            _timerTime = 0.0f;
            _onTimerComplete();
        }

        if (_showTime) {
            timerText.text = _timerCaption + Mathf.FloorToInt(_timerTime);
        } else {
            timerText.text = _timerCaption;
        }
    }

    public bool CheckMoney(int cost) {
        return cost <= _scrap;
    }

    public void RequestTurretBuild(GameObject prefab) {
        if (prefab.GetComponent<ITurret>().Cost > _scrap) {
            //TELL THE PLAYER THEY'RE BROKE
            MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                "You require more scrap!"));
            return;
        }

        MessageDispatcher.Dispatch(new EngageBuildModeMessage(prefab));
    }
}