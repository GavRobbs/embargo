using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneAndTurretManager : MonoBehaviour, IMessageHandler {
    [SerializeField] private List<Drone> drones;

    private readonly List<SupportTurret> _supportTurrets = new List<SupportTurret>();

    private GameObject _currentTurretPrefab;

    // Cache the singleton instance
    private static readonly MessageDispatcher MessageDispatcher = MessageDispatcher.GetInstance();


    private void Start() {
        MessageDispatcher.AddHandler(this);
    }

    private void OnDestroy() {
        MessageDispatcher.RemoveHandler(this);
    }

    private Drone GetFreeDrone() {
        //Just iterate through each drone to see if one is free
        return drones.FirstOrDefault(drone => !drone.Busy);
    }

    public void HandleMessage(GameMessage message) {
        switch (message.MessageType) {
            case MessageConstants.CreateTurret: {
                DroneBuildMessage msg = message as DroneBuildMessage;
                Drone current_drone = GetFreeDrone();

                if (current_drone) {
                    float build_time = _currentTurretPrefab.GetComponent<ITurret>().BuildTime;
                    var task = new Task_BuildStructure(msg.building, current_drone, _currentTurretPrefab, build_time);
                    current_drone.CurrentTask = task;
                } else {
                    MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                        "No available drones!"));
                }

                break;
            }
            case MessageConstants.DisengageEverything: {
                _currentTurretPrefab = null;
                break;
            }
            case MessageConstants.UpgradeTurret: {
                Building ab = (message as SingleValueMessage<Building>)?.value;
                if (!ab) {
                    MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                        "Invalid turret selection!"));
                    return;
                }

                Drone current_drone = GetFreeDrone();

                if (current_drone) {
                    ITurret t = ab.OccupyingTurret;
                    if (t.Level == 5) {
                        MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                            "Turret is at max level!"));
                        return;
                    }

                    float upgrade_time = ((float)t.Level + 1) * t.BuildTime;
                    int upgrade_cost = (t.Level + 1) * t.Cost;
                    var task = new Task_UpgradeStructure(ab, current_drone, upgrade_time, upgrade_cost);
                    current_drone.CurrentTask = task;
                } else {
                    MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                        "No available drones!"));
                }

                break;
            }
            case MessageConstants.ScrapTurret: {
                Building ab = (message as SingleValueMessage<Building>).value;
                if (!ab) {
                    MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                        "Invalid turret selection!"));
                    return;
                }

                Drone current_drone = GetFreeDrone();

                if (!current_drone) {
                    MessageDispatcher.Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                        "No available drones!"));
                    break;
                }

                ITurret t = ab.OccupyingTurret;
                var task = new Task_ScrapStructure(ab, current_drone, 8.0f);
                current_drone.CurrentTask = task;

                break;
            }
            case MessageConstants.EngageBuildMode: {
                if (message is EngageBuildModeMessage msg) _currentTurretPrefab = msg.turret_prefab;
                break;
            }
            case MessageConstants.DisengageBuildMode: {
                _currentTurretPrefab = null;
                break;
            }
            case MessageConstants.RegisterSupportTurretMessage: {
                if (message is SingleValueMessage<SupportTurret> st) _supportTurrets.Add(st.value);
                break;
            }
            case MessageConstants.UnregisterSupportTurretMessage: {
                if (message is SingleValueMessage<SupportTurret> st) _supportTurrets.Remove(st.value);
                break;
            }
            case MessageConstants.RegisterOffensiveTurretMessage: {
                if (message is SingleValueMessage<OffensiveTurret> otm) {
                    OffensiveTurret offensiveTurret = otm.value;
                    foreach (var st in _supportTurrets) {
                        if (!st) continue;

                        if (Vector3.Distance(st.transform.position, offensiveTurret.transform.position) <=
                            st.Influence) {
                            st.BestowBonus(offensiveTurret);
                        }
                    }
                }

                break;
            }
            case MessageConstants.CreateDroneMessage: {
                if (message is SingleValueMessage<Drone> new_drone) drones.Add(new_drone.value);
                break;
            }
        }
    }
}