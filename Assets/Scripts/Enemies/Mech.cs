using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour, IEnemy, ITaskable
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
    public Vector3 Position => forwardPoint.transform.position;
    public Spawner Spawner { get; set; }

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || dying == true;

    [SerializeField]
    float hp;

    float max_hp;

    public float HitPoints => hp;

    public string Name => "Bossmech";

    bool dying = false;

    ITask _currentTask;

    int level = 1;

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
        if (hp <= 0.0f && !dying)
        {
            //We only want the player to get the scrap if they kill it
            int scrap = (int)((float)level * 0.7f * 500.0f);
            MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.AddScrap, scrap));
            KillMe();
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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            float dmg = collision.gameObject.GetComponentInParent<MGBullet>().Damage;
            Destroy(collision.gameObject);
            //TODO: Patch so that the damage is done appropriately
            Damage(dmg);
        }

    }

    public void KillMe()
    {
        dying = true;
        mechBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(this.gameObject, 2.0f);
        explosion.Play();

    }

    public void FollowPath(List<Vector3> path_points, System.Action onComplete)
    {
        CurrentTask = new MechPathFollowTask(this, path_points, pathfindingDistanceThreshold, moveSpeed, onComplete);
    }

    public void Attack(ITurret turret)
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

    class MechPathFollowTask : ITask
    {
        List<Vector3> path;
        Mech owner;

        Vector3 target_position;
        bool isMoving;

        System.Action completionCallback;
        int path_index = 0;

        float distance_threshold;
        float move_speed;
        public MechPathFollowTask(Mech recipient, List<Vector3> follow_path, float threshold, float moveSpeed, System.Action onComplete)
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

        public void OnTaskUpdate(float dt)
        {
            if (owner.IsKilled)
            {
                return;
            }

            if (isMoving)
            {
                Vector3 moveDir = (target_position - owner.transform.position).normalized;

                //Use this to orient our mech
                if(moveDir.x < -0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.up);

                } else if(moveDir.x > 0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
                } else if(moveDir.z > 0.5f)
                {
                    owner.mechBody.transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
                } else if(moveDir.z < -0.5f)
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
                        //TODO: When I reimplement the particle effects
                        //ps.Stop();
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

        moveSpeed *= 1.0f + ((float)(level - 1)) * 0.06f;

        max_hp *= 1.0f + ((float)(level - 1)) * 0.28f;

        hp = max_hp;
    }

    public void ClearTask()
    {
        CurrentTask = null;
    }
}
