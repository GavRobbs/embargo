using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneAndTurretManager : MonoBehaviour, IMessageHandler
{
    public void HandleMessage(GameMessage message)
    {
    }

    void BuildTurret(Building target, GameObject tower_type_prefab, float build_time)
    {
        Building.BuildTurretTask new_turret_task = new Building.BuildTurretTask(target, tower_type_prefab, build_time, (dt) =>
        {
        },
        () =>
        {

        },
        () =>
        {

        });
        target.SetTask(new_turret_task);
    }

    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);
    }
}
