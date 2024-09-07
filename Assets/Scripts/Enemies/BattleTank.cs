using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTank : MonoBehaviour, IEnemy, ITaskable
{
    [SerializeField]
    GameObject forwardPoint;

    [SerializeField]
    GameObject BattleTankBody;

    [SerializeField]
    float pathfindingDistanceThreshold;

    [SerializeField]
    float moveSpeed;

    [SerializeField]
    float turretRotationSpeed;

    float baseMoveSpeed;

    [SerializeField]
    ParticleSystem sparks;

    [SerializeField]
    ParticleSystem flash;

    [SerializeField]
    AudioSource explosion;

    [SerializeField]
    TargetDetector target_detector;

    [SerializeField]
    GameObject turretMesh;

    [SerializeField]
    GameObject shotPrefab;

    [SerializeField]
    GameObject bulletSpawnPoint;
    public Vector3 Position => forwardPoint.transform.position;
    public Spawner Spawner { get; set; }

    public int CapitolDamage => 1;

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || dying == true;

    [SerializeField]
    float hp;

    float max_hp;

    public float HitPoints => hp;

    public string Name => "BattleTank";

    bool dying = false;

    ITask _currentTask;

    int level = 1;

    bool isStopped = false;

    [SerializeField]
    float attack_interval_min = 2.0f;

    [SerializeField]
    float attack_interval_max = 4.0f;

    Building current_target;


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
        if (isStopped)
        {
            return;
        }

        if (hp < 1.0f && !dying)
        {
            //We only want the player to get the scrap if they kill it
            int scrap = (int)((float)level * 0.7f * 60.0f);
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
            IBullet b = collision.gameObject.GetComponentInParent<IBullet>();
            float dmg = b.Damage;
            float mul = b.ArmourBonus ? 1.0f : 1.6f;
            Destroy(collision.gameObject);
            Damage(dmg * mul);
        }

    }

    public void KillMe()
    {
        dying = true;
        BattleTankBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(this.gameObject, 2.0f);
        explosion.Play();

    }

    public void FollowPath(List<Vector3> path_points, System.Action onComplete)
    {
        CurrentTask = new BattleTankPathFollowTask(this, path_points, pathfindingDistanceThreshold, moveSpeed, onComplete);
    }

    public void Attack(Building building)
    {
        //throw new System.NotImplementedException();
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

    void Fire()
    {
        GameObject.Instantiate(shotPrefab, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
    }

    class BattleTankPathFollowTask : ITask
    {
        List<Vector3> path;
        BattleTank owner;

        Vector3 target_position;
        bool isMoving;

        System.Action completionCallback;
        int path_index = 0;

        float distance_threshold;
        float move_speed;
        float next_attack_timer = 0.0f;
        bool isTracking = false;

        public BattleTankPathFollowTask(BattleTank recipient, List<Vector3> follow_path, float threshold, float moveSpeed, System.Action onComplete)
        {
            path = follow_path;
            owner = recipient;
            completionCallback = onComplete;
            distance_threshold = threshold;
            move_speed = moveSpeed;

            next_attack_timer = Random.Range(recipient.attack_interval_min, recipient.attack_interval_max);

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

        private void LookAtTargetAndFire()
        {
            if (owner.current_target == null)
            {
                isTracking = false;
                next_attack_timer = Random.Range(owner.attack_interval_min, owner.attack_interval_max);
                return;
            }

            Quaternion current_rotation = owner.turretMesh.transform.rotation;
            Vector3 direction = Vector3.Normalize(owner.turretMesh.transform.position - owner.current_target.transform.position);
            float angle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg);

            Quaternion target_rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            float angular_diff = Quaternion.Angle(current_rotation, target_rotation);

            if (angular_diff > 1)
            {
                Quaternion interpolated = Quaternion.Slerp(current_rotation, target_rotation, owner.turretRotationSpeed * Time.deltaTime);
                owner.turretMesh.transform.rotation = interpolated;
            }
            else
            {
                owner.turretMesh.transform.rotation = target_rotation;
                owner.Fire();
                next_attack_timer = Random.Range(owner.attack_interval_min, owner.attack_interval_max);
                isTracking = false;
            }

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

                if (moveDir.x < -0.5f)
                {
                    owner.BattleTankBody.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);

                }
                else if (moveDir.x > 0.5f)
                {
                    owner.BattleTankBody.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.up);
                }
                else if (moveDir.z > 0.5f)
                {
                    owner.BattleTankBody.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);
                }
                else if (moveDir.z < -0.5f)
                {
                    owner.BattleTankBody.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
                }

                owner.transform.position = owner.transform.position + (moveDir * move_speed * Time.deltaTime);

                if (isTracking)
                {
                    //Rotate the turret appropriately
                    //and fire if we reach
                    LookAtTargetAndFire();
                }
                else
                {
                    next_attack_timer -= Time.deltaTime;
                    if (next_attack_timer <= 0.0)
                    {
                        next_attack_timer = Random.Range(owner.attack_interval_min, owner.attack_interval_max);
                        isTracking = true;
                        owner.current_target = owner.target_detector.GetRandomTarget();

                        if(owner.current_target == null)
                        {
                            isTracking = false;

                            //Make the next interval shorter so its more inclined to attack again
                            next_attack_timer /= 2.0f;
                        }

                    }

                }

                

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

        moveSpeed *= 1.0f + ((float)(level - 1)) * 0.06f;

        max_hp *= 1.0f + ((float)(level - 1)) * 0.18f;

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
