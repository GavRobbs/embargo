using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAndTurretManager : MonoBehaviour, IMessageHandler
{
    [SerializeField]
    List<Drone> drones;

    GameObject current_turret_prefab;

    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);
    }
    Drone GetFreeDrone()
    {
        foreach (Drone drone in drones)
        {
            if (!drone.Busy)
            {
                return drone;
            }
        }

        return null;
    }

    public void HandleMessage(GameMessage message)
    {
        switch (message.MessageType)
        {
            case MessageConstants.CreateTurret:
                {
                    DroneBuildMessage msg = message as DroneBuildMessage;
                    Drone current_drone = GetFreeDrone();

                    if(current_drone != null)
                    {
                        float build_time = current_turret_prefab.GetComponent<ITurret>().BuildTime;
                        var task = new Task_BuildStructure(msg.building, current_drone, current_turret_prefab, build_time);
                        current_drone.CurrentTask = task;
                    }
                    else
                    {
                        //This should pop up on screen somewhere
                        Debug.Log("NO FREE DRONES!");
                    }
                    
                    break;
                }
            case MessageConstants.EngageBuildMode:
                {
                    EngageBuildModeMessage msg = message as EngageBuildModeMessage;
                    current_turret_prefab = msg.turret_prefab;
                    break;
                }
            case MessageConstants.DisengageBuildMode:
                {
                    current_turret_prefab = null;
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    
}
