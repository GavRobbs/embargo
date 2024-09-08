using System.Collections.Generic;
using UnityEngine;

public class Task_BuildStructure : ITask {
    private Drone drone;

    public float Progress {
        get {
            if (status == TaskState.DONE) {
                return 1.01f;
            }

            return 1.0f - (duration / initial_duration);
        }
    }

    public string Description => "Build a turret";

    float duration;
    float initial_duration;
    Building building;

    bool building_done;

    List<Vector3> path_to_target;

    private enum TaskState {
        GO_TO_TARGET,
        BUILD_TURRET,
        DONE
    };

    TaskState status = TaskState.GO_TO_TARGET;

    bool _atTarget;

    GameObject turret_prefab;
    int cost;

    bool cancelled;

    public Task_BuildStructure(Building targetBuilding, Drone instructionDrone, GameObject tpfb, float ttb) {
        status = TaskState.GO_TO_TARGET;
        building = targetBuilding;
        drone = instructionDrone;

        var r_adj_pp = building.RandomAdjacent;
        path_to_target = drone.FindPathToTarget(r_adj_pp.Position);

        cost = tpfb.GetComponent<ITurret>().Cost;

        duration = ttb;
        initial_duration = ttb;

        turret_prefab = tpfb;
    }

    public void OnTaskEnter() {
        //Create a callback here to follow the path to the target
        drone.FollowPath(path_to_target, () => { _atTarget = true; });
    }

    public void Cancel() {
        OnTaskCancel();
    }

    public void OnTaskCancel() {
        if (status == TaskState.DONE) {
            return;
        }

        cancelled = true;
        building.CancelCurrentTask();
        drone.StopMoving();
        drone.ClearTask();
    }

    public void OnTaskUpdate(float dt) {
        if (cancelled) {
            return;
        }

        if (status == TaskState.GO_TO_TARGET) {
            if (!Object.FindObjectOfType<CommandConsoleUI>().CheckMoney(cost)) {
                //Remind you that you're broke
                MessageDispatcher.GetInstance()
                    .Dispatch(new SingleValueMessage<string>(MessageConstants.DisplayAlertMessage,
                        "You require more scrap!"));
                status = TaskState.DONE;
            } else {
                if (_atTarget) {
                    status = TaskState.BUILD_TURRET;
                    MessageDispatcher.GetInstance()
                        .Dispatch(new SingleValueMessage<int>(MessageConstants.RemoveScrap, cost));
                    building.SetTask(
                        new Building.BuildTurretTask(building, turret_prefab, duration,
                            (float delta) => { },
                            () => { building_done = true; },
                            () => { }
                        )
                    );
                }
            }
        } else if (status == TaskState.BUILD_TURRET) {
            duration -= Time.deltaTime;
            if (building_done) {
                status = TaskState.DONE;
            }
        } else {
            drone.ClearTask();
        }
    }

    public void OnTaskExit() {
    }
}