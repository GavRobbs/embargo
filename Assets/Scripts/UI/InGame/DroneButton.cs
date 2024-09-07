using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneButton : MonoBehaviour
{
    [SerializeField]
    int _cost;

    [SerializeField]
    Sprite emptySlotSprite;

    [SerializeField]
    Sprite fullSlotSprite;

    [SerializeField]
    Drone attached_drone;

    [SerializeField]
    Image buttonImage;

    [SerializeField]
    Image progressOverlayImage;

    [SerializeField]
    Transform spawnPosition;

    [SerializeField]
    GameObject cost_object;

    [SerializeField]
    GameObject dronePrefab;
    int Cost { get => _cost; }
    public Drone AttachedDrone { get => attached_drone; set => attached_drone = value; }
    void Start()
    {
        if (attached_drone)
        {
            buttonImage.sprite = fullSlotSprite;
            progressOverlayImage.gameObject.SetActive(true);
            cost_object.SetActive(false);
        }
        else
        {
            buttonImage.sprite = emptySlotSprite;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(attached_drone != null)
        {
            if (attached_drone.Busy)
            {
                progressOverlayImage.fillAmount = attached_drone.CurrentTask.Progress;
            }
            else
            {
                progressOverlayImage.fillAmount = 0;
            }
        }
        
    }

    public void OnClick()
    {
        if (attached_drone == null)
        {
            //Check if the player can afford it, if not, drop an alert
            if (!Object.FindObjectOfType<CommandConsoleUI>().CheckMoney(Cost))
            {
                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "You require more scrap!"));
            }
            else
            {
                var drone_go = GameObject.Instantiate(dronePrefab, spawnPosition, false);
                attached_drone = drone_go.GetComponent<Drone>();
                buttonImage.sprite = fullSlotSprite;
                progressOverlayImage.gameObject.SetActive(true);
                cost_object.SetActive(false);
                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.RemoveScrap, _cost));
                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<Drone>(MessageConstants.CreateDroneMessage, attached_drone));
            }
            
        }
        else
        {
            if (AttachedDrone.Busy)
            {
                AttachedDrone.CancelTask();
            }
        }

    }
}
