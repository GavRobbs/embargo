using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAndTurretManager : MonoBehaviour, IMessageHandler
{
    [SerializeField]
    List<Drone> drones;

    List<SupportTurret> supportTurrets = new List<SupportTurret>();

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
                        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "No available drones!"));
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
            case MessageConstants.RegisterSupportTurretMessage:
                {
                    SingleValueMessage<SupportTurret> st = message as SingleValueMessage<SupportTurret>;
                    supportTurrets.Add(st.value);
                    break;
                }
            case MessageConstants.UnregisterSupportTurretMessage:
                {
                    SingleValueMessage<SupportTurret> st = message as SingleValueMessage<SupportTurret>;
                    supportTurrets.Remove(st.value);
                    break;
                }
            case MessageConstants.RegisterOffensiveTurretMessage:
                {
                    SingleValueMessage<OffensiveTurret> otm = message as SingleValueMessage<OffensiveTurret>;
                    OffensiveTurret ot = otm.value;
                    foreach (var st in supportTurrets)
                    {
                        if(Vector3.Distance(st.transform.position, ot.transform.position) <= st.Influence)
                        {
                            st.BestowBonus(ot);
                        }
                    }
                    break;

                }
            case MessageConstants.CreateDroneMessage:
                {
                    SingleValueMessage<Drone> new_drone = message as SingleValueMessage<Drone>;
                    drones.Add(new_drone.value);
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    
}
