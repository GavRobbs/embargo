using System.Collections;
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

    [SerializeField]
    ParticleSystem sparks;

    [SerializeField]
    ParticleSystem flash;

    [SerializeField]
    AudioSource explosion;
    public Vector3 Position => forwardPoint.transform.position;

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || dying == true;

    [SerializeField]
    float hp;

    float max_hp;

    public float HitPoints => hp;

    public string Name => "Scout";

    bool dying = false;

    ITask current_task;

    // Update is called once per frame
    void Update()
    {
        if(hp <= 0.0f && !dying)
        {
            KillMe();
        }
        else
        {
            if(current_task != null)
            {
                current_task.OnTaskUpdate(Time.deltaTime);
            }
        }
        
    }

    public void Damage(float value)
    {
        hp -= value;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet"))
        {
            Destroy(collision.gameObject);
            //TODO: Patch so that the damage is done appropriately
            Damage(5.0f);
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

    public void SetTask(ITask task)
    {
        if(current_task != null)
        {
            current_task.OnTaskExit();
        }
        current_task = task;
        current_task.OnTaskEnter();
    }

    public void FollowPath(List<Vector3> path_points, System.Action onComplete)
    {
        SetTask(new ScoutPathFollowTask(this, path_points, pathfindingDistanceThreshold, moveSpeed, onComplete));
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

    public void OnHoverOver()
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
    }


}
