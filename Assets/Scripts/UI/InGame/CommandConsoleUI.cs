using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CommandConsoleUI : MonoBehaviour, IMessageHandler
{
    [SerializeField]
    PopupManager popupManager;

    [SerializeField]
    GameObject dangerWarning;

    [SerializeField]
    GameObject clearMessage;

    [SerializeField]
    TextMeshProUGUI timer_text;

    [SerializeField]
    TextMeshProUGUI scrap_counter;

    [SerializeField]
    GameObject fader;

    [SerializeField]
    AlertMessage alertMessage;

    bool updateScrapCounter = true;

    int scrap = 1500;

    System.Action onTimerComplete;
    float timer_time;
    string timer_caption;
    bool timerActive = false;
    bool showTime = false;
    public void HandleMessage(GameMessage message)
    {
        switch (message.MessageType)
        {
            case MessageConstants.HideHoverPopupMessage:
                {
                    popupManager.HideAllPopups();
                    break;
                }
            case MessageConstants.HoverInfoDisplayMessage:
                {
                    //Case and dispatch to the appropriate popup type
                    var him = (HoverInfoDisplayMessage)message;
                    if(him.info == null)
                    {
                        return;
                    }

                    if (him.info["type"] == "building")
                    {
                        popupManager.ShowBuildingPopup(int.Parse(him.info["hp"]), him.display_position);
                    }
                    else if(him.info["type"] == "enemy")
                    {
                        popupManager.ShowEnemyInfoPopup(him.info["name"], int.Parse(him.info["hp"]), int.Parse(him.info["max_hp"]), him.display_position);
                    }
                    else if(him.info["type"] == "offensive_turret")
                    {
                        popupManager.ShowOffensiveTurretBuildingPopup(him.info["name"], int.Parse(him.info["level"]), int.Parse(him.info["bhp"]), float.Parse(him.info["atk_bonus"]),
                            float.Parse(him.info["cd_bonus"]), float.Parse(him.info["range_bonus"]), him.display_position);
                    }
                    else if(him.info["type"] == "support_turret")
                    {
                        popupManager.ShowDefensiveTurretBuildingPopup(him.info["name"], int.Parse(him.info["level"]), int.Parse(him.info["bhp"]), him.display_position);
                    }
                    else if(him.info["type"] == "drone")
                    {
                        popupManager.ShowDronePopup(him.display_position);
                    }
                    break;
                }
            case MessageConstants.WaveAlertMessage:
                {
                    dangerWarning.SetActive(true);
                    dangerWarning.GetComponentInChildren<AudioSource>().Play();

                    SetTimer("Under Attack ", 6.0f, false, () =>
                    {
                        dangerWarning.GetComponentInChildren<AudioSource>().Stop();
                        dangerWarning.SetActive(false);
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.BeginWaveMessage));
                        SetTimer("Under Attack ", 60.0f, true, () =>
                        {

                        });
                    });
                    break;
                }
            case MessageConstants.EndWaveMessage:
                {
                    clearMessage.SetActive(true);
                    clearMessage.GetComponentInChildren<AudioSource>().Play();

                    SetTimer("Area Secure ", 4.0f, false, () =>
                    {
                        clearMessage.GetComponentInChildren<AudioSource>().Stop();
                        clearMessage.SetActive(false);
                        SetTimer("Enemy Incoming ", 30.0f, true, () =>
                        {
                            MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.WaveAlertMessage));
                        });
                    });
                    break;
                }
            case MessageConstants.TriggerFirstWaveMessage:
                {
                    Debug.Log("Triggering first wave");
                    SetTimer("Enemy Incoming ", 30.0f, true, () =>
                    {
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.WaveAlertMessage));
                    });
                    break;
                }
            case MessageConstants.AddScrap:
                {
                    scrap += (message as SingleValueMessage<int>).value;
                    updateScrapCounter = true;
                    break;
                }
            case MessageConstants.RemoveScrap:
                {
                    scrap -= (message as SingleValueMessage<int>).value;
                    updateScrapCounter = true;
                    break;
                }
            case MessageConstants.GameOverMessage:
                {
                    fader.SetActive(true);
                    break;
                }
            case MessageConstants.DisplayAlertMessage:
                {
                    string message_text = (message as SingleValueMessage<string>).value;
                    alertMessage.gameObject.SetActive(true);
                    alertMessage.Display(message_text);
                    break;
                }
            default:
                break;
        }
        
    }

    void SetTimer(string text, float t, bool displayTime, System.Action onCompleteEvent)
    {
        timerActive = true;
        timer_caption = text;
        timer_time = t;
        showTime = displayTime;
        onTimerComplete = onCompleteEvent;
    }

    // Start is called before the first frame update
    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);

    }

    // Update is called once per frame
    void Update()
    {
        if (updateScrapCounter)
        {
            scrap_counter.text = "Scrap: " + scrap.ToString();
            updateScrapCounter = false;
        }

        if (timerActive)
        {
            timer_time -= Time.deltaTime;
            if(timer_time <= 0.0f)
            {
                timerActive = false;
                timer_time = 0.0f;
                onTimerComplete();
            }

            if (showTime)
            {
                timer_text.text = timer_caption + System.Math.Round(timer_time, 2).ToString();
            }
            else
            {
                timer_text.text = timer_caption;
            }
        }
        
    }

    public bool CheckMoney(int cost)
    {
        return cost <= scrap;
    }

    public void RequestTurretBuild(GameObject prefab)
    {
        if(prefab.GetComponent<ITurret>().Cost <= scrap)
        {
            MessageDispatcher.GetInstance().Dispatch(new EngageBuildModeMessage(prefab));
        }
        else
        {
            //TODO: TELL THE PLAYER THEY'RE BROKE
            MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "You require more scrap!"));
        }

    }
}
