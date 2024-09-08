using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour, IEnemy, ITaskable
{
    [SerializeField]
    GameObject forwardPoint;

    [SerializeField]
    GameObject mechBody;

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

    [SerializeField]
    AudioSource teleportSound;
    public Vector3 Position => forwardPoint.transform.position;
    public ISpawner Spawner { get; set; }

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || dying == true;

    public int CapitolDamage => 6;

    [SerializeField]
    float hp;

    float max_hp;

    public float HitPoints => hp;

    public string Name => "Zorg Overlord";

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

    public bool Busy => true;

    // Update is called once per frame
    void Update()
    {
        if (isStopped || dying)
        {
            return;
        }

        if (hp < 1.0f && !dying)
        {
            KillMe();
            MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.BossKilledMessage));
        }
        else
        {
            if (CurrentTask != null)
            {
                CurrentTask.OnTaskUpdate(Time.deltaTime);
            }
        }

    }

    void OnDestroy()
    {
        Spawner.DecreaseEnemyCount();
    }

    public void Damage(float value)
    {
        hp -= value;
    }

    public void CancelTask()
    {

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            IBullet b = collision.gameObject.GetComponentInParent<IBullet>();
            float dmg = b.Damage;
            float mul = b.ArmourBonus ? 1.8f : 1.0f;
            Destroy(collision.gameObject);
            Damage(dmg * mul);
        }

    }

    public void KillMe()
    {
        //TODO: Special Handling for when the boss is destroyed
        dying = true;
        mechBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(this.gameObject, 2.0f);
        explosion.Play();

    }

    public void Wander(List<Vector3> path1, List<Vector3> path2, List<Vector3> path3, List<Vector3> path4)
    {
        //The boss basically walks to the target - every 500 hp damage or so he teleports to a different path

        Vector3 teleport1 = path2[0];
        Vector3 teleport2 = path3[0];
        Vector3 teleport3 = path4[0];

        CurrentTask = new BossPathFollowTask(this, 1200, path1, pathfindingDistanceThreshold, moveSpeed, () =>
        {
            CurrentTask = new BossTeleportTask(this, teleport1, () => {
                CurrentTask = new BossPathFollowTask(this, 800, path2, pathfindingDistanceThreshold, moveSpeed, () =>
                {
                    CurrentTask = new BossTeleportTask(this, teleport2, () => {
                        CurrentTask = new BossPathFollowTask(this, 400, path3, pathfindingDistanceThreshold, moveSpeed, () =>
                        {
                            CurrentTask = new BossTeleportTask(this, teleport3, () => {
                                CurrentTask = new BossPathFollowTask(this, 0.0f, path4, pathfindingDistanceThreshold, moveSpeed, () =>
                                {
                                    //You're going to die so it doesn't really matter what happens here
                                });

                            });
                        });

                    });
                });

            });
        });
       
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

    class BossPathFollowTask : ITask
    {
        List<Vector3> path;
        FinalBoss owner;

        Vector3 target_position;
        bool isMoving;

        System.Action completionCallback;
        int path_index = 0;

        float distance_threshold;
        float move_speed;
        float hp_threshold_to_teleport;
        public BossPathFollowTask(FinalBoss recipient, float tp_thresh, List<Vector3> fpath, float threshold, float moveSpeed, System.Action onComplete)
        {
            owner = recipient;
            completionCallback = onComplete;
            distance_threshold = threshold;
            move_speed = moveSpeed;
            path = fpath;
            hp_threshold_to_teleport = tp_thresh;
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

            if(owner.hp <= hp_threshold_to_teleport)
            {
                isMoving = false;
                completionCallback();
                return;
            }

            if (isMoving)
            {
                Vector3 moveDir = (target_position - owner.transform.position).normalized;

                //Use this to orient our mech
                if (moveDir.x < -0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.up);

                }
                else if (moveDir.x > 0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
                }
                else if (moveDir.z > 0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
                }
                else if (moveDir.z < -0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);
                }
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

    class BossTeleportTask : ITask
    {
        public float Progress => 1.0f;
        FinalBoss boss;
        Vector3 new_pos;
        System.Action completeCallback;

        public BossTeleportTask(FinalBoss me, Vector3 teleportPosition, System.Action onComplete)
        {
            completeCallback = onComplete;
            boss = me;
            new_pos = teleportPosition;

        }

        public void Cancel()
        {
        }

        public void OnTaskCancel()
        {
        }

        public void OnTaskEnter()
        {
            boss.GetComponent<Rigidbody>().transform.position = new_pos;
            boss.teleportSound.Play();
            completeCallback();
            
        }

        public void OnTaskExit()
        {
        }

        public void OnTaskUpdate(float dt)
        {
        }
    }
    void Start()
    {
        max_hp = hp;
        baseMoveSpeed = moveSpeed;

        moveSpeed *= 1.0f + ((float)(level - 1)) * 0.06f;

        max_hp *= 1.0f + ((float)(level - 1)) * 0.45f;

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

    public void FollowPath(List<Vector3> path_points, Action onComplete)
    {
        throw new NotImplementedException();
    }
}
