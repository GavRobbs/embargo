using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task_BuildStructure : ITask
{
    private Drone drone;
    public float Progress
    {
        get
        {
            if (status == TaskState.DONE)
            {
                return 1.1f;
            }
            else
            {
                return 1.0f - (duration / initial_duration);
            }
        }
    }

    public string Description => "Build a turret";

    float duration;
    float initial_duration;
    Building building;

    bool building_done = false;

    List<Vector3> path_to_target;

    enum TaskState { GO_TO_TARGET, BUILD_TURRET, DONE };
    TaskState status = TaskState.GO_TO_TARGET;

    bool at_target = false;

    GameObject turret_prefab;
    int cost;

    public Task_BuildStructure(Building target_building, Drone instruction_drone, GameObject tpfb, float ttb)
    {
        status = TaskState.GO_TO_TARGET;
        building = target_building;
        drone = instruction_drone;

        var r_adj_pp = building.RandomAdjacent;
        path_to_target = drone.FindPathToTarget(r_adj_pp.Position);

        cost = tpfb.GetComponent<ITurret>().Cost;

        duration = ttb;
        initial_duration = ttb;

        turret_prefab = tpfb;
    }

    public void OnTaskEnter()
    {
        //Create a callback here to follow the path to the target
        drone.FollowPath(path_to_target, () => { at_target = true; });
    }

    public void OnTaskUpdate(float dt)
    {
        if (status == TaskState.GO_TO_TARGET)
        {
            if (!Object.FindObjectOfType<CommandConsoleUI>().CheckMoney(cost))
            {
                //TODO: Remind you that you're broke
                status = TaskState.DONE;
            }
            else
            {
                if (at_target)
                {
                    status = TaskState.BUILD_TURRET;
                    MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.RemoveScrap, cost));
                    building.SetTask(
                            new Building.BuildTurretTask(building, turret_prefab, duration,
                            (float delta) => { },
                            () => { building_done = true; },
                            () => { }
                            )
                        );
                }

            }
            
        }
        else if (status == TaskState.BUILD_TURRET)
        {
            duration -= Time.deltaTime;
            if(building_done == true)
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
