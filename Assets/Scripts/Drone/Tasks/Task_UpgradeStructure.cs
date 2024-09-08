using System.Collections.Generic;
using UnityEngine;

public class Task_UpgradeStructure : ITask
{
    private Drone drone;
    public float Progress
    {
        get
        {
            if (status == TaskState.DONE)
            {
                return 1.01f;
            }
            else
            {
                return 1.0f - (duration / initial_duration);
            }
        }
    }

    public string Description => "Upgrade a turret";

    float duration;
    float initial_duration;
    Building building;

    bool building_done;

    List<Vector3> path_to_target;

    enum TaskState { GO_TO_TARGET, UPGRADE_TURRET, DONE };
    TaskState status = TaskState.GO_TO_TARGET;

    bool at_target;

    GameObject turret_prefab;
    int cost;

    bool cancelled;

    public Task_UpgradeStructure(Building target_building, Drone instruction_drone, float ttu, int upgrade_cost)
    {
        status = TaskState.GO_TO_TARGET;
        building = target_building;
        drone = instruction_drone;

        var r_adj_pp = building.RandomAdjacent;
        path_to_target = drone.FindPathToTarget(r_adj_pp.Position);

        cost = upgrade_cost;

        duration = ttu;
        initial_duration = ttu;

    }

    public void OnTaskEnter()
    {
        //Create a callback here to follow the path to the target
        drone.FollowPath(path_to_target, () => { at_target = true; });
    }

    public void Cancel()
    {
        OnTaskCancel();
    }

    public void OnTaskCancel()
    {
        if (status == TaskState.DONE)
        {
            return;
        }

        cancelled = true;
        building.CancelCurrentTask();
        drone.StopMoving();
        drone.ClearTask();

    }

    public void OnTaskUpdate(float dt)
    {
        if (cancelled)
        {
            return;
        }

        if (status == TaskState.GO_TO_TARGET)
        {
            if (!Object.FindObjectOfType<CommandConsoleUI>().CheckMoney(cost))
            {
                //Remind you that you're broke
                MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage, "You require more scrap!"));
                status = TaskState.DONE;
            }
            else
            {
                if (at_target)
                {
                    status = TaskState.UPGRADE_TURRET;
                    MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.RemoveScrap, cost));
                    building.SetTask(
                            new Building.UpgradeTurretTask(building, cost, duration,
                            (float delta) => { },
                            () => { building_done = true; },
                            () => { }
                            )
                        );
                }

            }

        }
        else if (status == TaskState.UPGRADE_TURRET)
        {
            duration -= Time.deltaTime;
            if (building_done)
            {
                status = TaskState.DONE;
            }
        }
        else
        {
            drone.ClearTask();
        }
    }

    public void OnTaskExit()
    {

    }

}
