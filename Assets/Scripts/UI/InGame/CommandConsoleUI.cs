using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandConsoleUI : MonoBehaviour, IMessageHandler
{
    [SerializeField]
    PopupManager popupManager;
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
                    var him = (HoverInfoDisplayMessage)message;
                    if (him.info["type"] == "building")
                    {
                        popupManager.ShowBuildingPopup(int.Parse(him.info["hp"]), him.display_position);
                    }
                    else if(him.info["type"] == "enemy")
                    {
                        popupManager.ShowEnemyInfoPopup(him.info["name"], int.Parse(him.info["hp"]), int.Parse(him.info["max_hp"]), him.display_position);
                    }
                    break;
                }
            default:
                break;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BuildMachineGun()
    {

    }

    public void BuildPlasmaCannon()
    {

    }

    public void BuildAttackBooster()
    {

    }

    public void BuildDefenseBooster()
    {

    }
}
