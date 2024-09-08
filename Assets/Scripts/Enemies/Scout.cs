﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scout : MonoBehaviour, IEnemy, ITaskable
{
    [SerializeField]
    GameObject forwardPoint;

    [SerializeField]
    GameObject scoutBody;

    [SerializeField]
    float pathfindingDistanceThreshold;

    [SerializeField]
    float moveSpeed;

    float baseMoveSpeed;

    [SerializeField]
    ParticleSystem sparks;

    [SerializeField]
    ParticleSystem flash;

    [SerializeField]
    AudioSource explosion;
    public Vector3 Position => forwardPoint.transform.position;
    public ISpawner Spawner { get; set; }

    public int CapitolDamage => 1;

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || dying == true;

    [SerializeField]
    float hp;

    float max_hp;

    public float HitPoints => hp;

    public string Name => "Scout";

    bool dying = false;

    ITask _currentTask;

    int level = 1;

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
                _currentTask?.OnTaskExit();
                _currentTask = value;
                _currentTask.OnTaskEnter();
            }
            else
            {
                _currentTask?.OnTaskExit();
                _currentTask = null;
            }
        }
    }

    public void SetLevel(int lv)
    {
        level = lv;
    }

    public bool Busy => throw new System.NotImplementedException();

    // Update is called once per frame
    void Update()
    {
        if (isStopped || dying)
        {
            return;
        }

        if(hp < 1.0f && !dying)
        {
            //We only want the player to get the scrap if they kill it
            int scrap = (int)((float)level * 0.7f * 30.0f);
            MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.AddScrap, scrap));
            KillMe();
        }
        else
        {
            if(CurrentTask != null)
            {
                CurrentTask.OnTaskUpdate(Time.deltaTime);
            }
        }
        
    }

    public void CancelTask()
    {

    }

    void OnDestroy()
    {
        Spawner.DecreaseEnemyCount();
    }

    public void Damage(float value)
    {
        hp -= value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            float dmg = collision.gameObject.GetComponentInParent<IBullet>().Damage;
            Destroy(collision.gameObject);
            Damage(dmg);
        }

    }

    public void KillMe()
    {
        dying = true;
        scoutBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(this.gameObject, 2.0f);
        explosion.Play();

    }

    public void FollowPath(List<Vector3> path_points, System.Action onComplete)
    {
        CurrentTask = new ScoutPathFollowTask(this, path_points, pathfindingDistanceThreshold, moveSpeed, onComplete);
    }

    public void Attack(Building building)
    {
        throw new System.NotImplementedException();
    }

    public Dictionary<string, string> GetHoverData()
    {
        return new Dictionary<string, string>()
        {
            {"type", "enemy"},
            {"name", Name },
            {"hp", ((int)hp).ToString() },
            {"max_hp", ((int)max_hp).ToString() }
        };
    }

    public void OnHoverOver(HoverInfo info)
    {
    }

    public void OnHoverOff()
    {
    }

    class ScoutPathFollowTask : ITask
    {
        List<Vector3> path;
        Scout owner;

        Vector3 target_position;
        bool isMoving;

        System.Action completionCallback;
        int path_index = 0;

        float distance_threshold;
        float move_speed;
        public ScoutPathFollowTask(Scout recipient, List<Vector3> follow_path, float threshold, float moveSpeed, System.Action onComplete)
        {
            path = follow_path;
            owner = recipient;
            completionCallback = onComplete;
            distance_threshold = threshold;
            move_speed = moveSpeed;
        }

        public float Progress => 0.0f;

        public void OnTaskEnter()
        {
            //Ideally we could do some cleanup here to see if maybe we can join the path somewhere else apart from the start etc
            isMoving = true;
            MoveTo(path[0]);
        }

        public void OnTaskExit()
        {
        }

        public void Cancel()
        {

        }

        public void OnTaskCancel()
        {

        }

        public void OnTaskUpdate(float dt)
        {
            if (owner.IsKilled)
            {
                return;
            }

            if (isMoving)
            {
                Vector3 moveDir = (target_position - owner.transform.position).normalized;
                owner.transform.position = owner.transform.position + (moveDir * move_speed * Time.deltaTime);

                if (Vector3.Distance(owner.transform.position, target_position) <= distance_threshold)
                {
                    path_index += 1;
                    if (path_index == path.Count)
                    {
                        //We're done moving
                        isMoving = false;
                        completionCallback();
                    }
                    else
                    {
                        MoveTo(path[path_index]);
                    }
                }


            }
        }

        private void MoveTo(Vector3 pos)
        {
            target_position = pos;
            target_position.y = 0.0f;
        }

    }

    void Start()
    {
        max_hp = hp;
        baseMoveSpeed = moveSpeed;

        moveSpeed *= 1.0f + ((float)(level - 1)) * 0.1f;

        max_hp *= 1.0f + ((float)(level - 1)) * 0.14f;

        hp = max_hp;
    }

    public void ClearTask()
    {
        CurrentTask = null;
    }

    public void Stop()
    {
        isStopped = true;
    }
}
