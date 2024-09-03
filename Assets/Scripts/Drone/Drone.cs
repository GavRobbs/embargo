using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour, IHoverable, ITaskable, IStoppable
{
    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float distance_threshold;

    GameMap mapManager;

    bool isMoving = false;

    Vector3 target_position;
    List<Vector3> current_path;
    int path_index = 0;

    //ParticleSystem ps;

    System.Action onPathComplete;

    ITask _currentTask;

    bool isStopped = false;

    public ITask CurrentTask
    {
        get
        {
            return _currentTask;
        }

        set
        {
            if (value != null)
            {
                if(_currentTask != null)
                {
                    _currentTask.OnTaskExit();
                }
                _currentTask = value;
                _currentTask.OnTaskEnter();
            }
            else
            {
                _currentTask = null;
            }
        }
    }

    public bool Busy
    {
        get
        {
            if (CurrentTask == null)
            {
                return false;
            }
            else
            {
                return CurrentTask.Progress < 1.0f;
            }
        }
    }

    public Dictionary<string, string> GetHoverData()
    {
        return new Dictionary<string, string>()
        {
            {"type", "drone"}
        };
    }

    void Start()
    {
        //ps = GetComponentInChildren<ParticleSystem>();
        mapManager = FindObjectOfType<GameMap>();
    }

    public void FollowPath(List<Vector3> path, System.Action complete)
    {
        if (path != null)
        {
            isMoving = true;
            current_path = path;
            path_index = 0;
            //ps.Play();
            onPathComplete = complete;
        }
        else
        {
            isMoving = false;
            current_path = null;
            path_index = 0;
            //ps.Stop();
        }

    }

    private void MoveTo(Vector3 tpos)
    {
        target_position = tpos;
        target_position.y = 0.0f;

    }

    // Update is called once per frame
    void Update()
    {
        if (isStopped)
        {
            return;
        }

        if (CurrentTask != null)
        {
            CurrentTask.OnTaskUpdate(Time.deltaTime);
        }

        if (isMoving)
        {
            Vector3 cpos = transform.position;
            cpos.y = 0.0f;
            Vector3 moveDir = (target_position - cpos).normalized;
            TurnToDirection(moveDir);

            transform.position = transform.position + (moveDir * moveSpeed * Time.deltaTime);
            Vector3 checkVector = transform.position;
            checkVector.y = 0.0f;

            if (Vector3.Distance(checkVector, target_position) <= distance_threshold)
            {
                path_index += 1;
                if (path_index == current_path.Count)
                {
                    //We're done moving
                    isMoving = false;
                    onPathComplete();
                    //ps.Stop();
                }
                else
                {
                    target_position = current_path[path_index];
                }
            }

        }
    }

    void TurnToDirection(Vector3 dir)
    {
        //Debug.Log(dir);

        GameObject mesh = GetComponentInChildren<MeshRenderer>().gameObject;

        float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        //Debug.Log(angle);
        mesh.transform.localRotation = Quaternion.Euler(-89.98f, 0.0f, angle + 90);
    }

    public List<Vector3> FindPathToTarget(Vector3 target)
    {
        Vector3 drone_pos = transform.position;
        drone_pos.y = 0.0f;
        target.y = 0.0f;

        var path = mapManager.GetPath(drone_pos, target);
        return path;
    }

    public void ClearTask()
    {
        CurrentTask = null;
    }

    public void OnHoverOver(HoverInfo info)
    {
    }

    public void OnHoverOff()
    {
    }

    public void Stop()
    {
        isStopped = true;
    }

}
