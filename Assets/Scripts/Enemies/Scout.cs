using System.Collections.Generic;
using UnityEngine;

public class Scout : MonoBehaviour, IEnemy, ITaskable {
    [SerializeField] GameObject forwardPoint;

    [SerializeField] GameObject scoutBody;

    [SerializeField] float pathfindingDistanceThreshold;

    [SerializeField] float moveSpeed;

    [SerializeField] ParticleSystem sparks;

    [SerializeField] ParticleSystem flash;

    [SerializeField] AudioSource explosion;
    public Vector3 Position => forwardPoint.transform.position;
    public ISpawner Spawner { get; set; }

    public int CapitolDamage => 1;

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || dying;

    [SerializeField] float hp;

    float max_hp;

    public float HitPoints => hp;

    public string Name => "Scout";

    bool dying;

    ITask _currentTask;

    int level = 1;

    bool isStopped;

    public ITask CurrentTask {
        get { return _currentTask; }

        set {
            if (value != null) {
                _currentTask?.OnTaskExit();
                _currentTask = value;
                _currentTask.OnTaskEnter();
            } else {
                _currentTask?.OnTaskExit();
                _currentTask = null;
            }
        }
    }

    public void SetLevel(int lv) {
        level = lv;
    }

    public bool Busy => throw new System.NotImplementedException();

    // Update is called once per frame
    void Update() {
        if (isStopped || dying) {
            return;
        }

        if (hp < 1.0f && !dying) {
            //We only want the player to get the scrap if they kill it
            int scrap = (int)(level * 0.7f * 30.0f);
            MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<int>(MessageConstants.AddScrap, scrap));
            Destroy();
        } else {
            CurrentTask?.OnTaskUpdate(Time.deltaTime);
        }
    }

    public void CancelTask() {
    }

    private void OnDestroy() {
        Spawner.DecreaseEnemyCount();
    }

    public void Damage(float value) {
        hp -= value;
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Bullet")) return;

        float dmg = collision.gameObject.GetComponentInParent<IBullet>().Damage;
        Destroy(collision.gameObject);
        Damage(dmg);
    }

    public void Destroy() {
        dying = true;
        scoutBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(gameObject, 2.0f);
        explosion.Play();
    }

    public void FollowPath(List<Vector3> path_points, System.Action onComplete) {
        CurrentTask = new ScoutPathFollowTask(this, path_points, pathfindingDistanceThreshold, moveSpeed, onComplete);
    }

    public void Attack(Building building) {
        throw new System.NotImplementedException();
    }

    public Dictionary<string, string> GetHoverData() {
        return new Dictionary<string, string>() {
            { "type", "enemy" },
            { "name", Name },
            { "hp", ((int)hp).ToString() },
            { "max_hp", ((int)max_hp).ToString() }
        };
    }

    public void OnHoverOver(HoverInfo info) {
    }

    public void OnHoverOff() {
    }

    private class ScoutPathFollowTask : ITask {
        private readonly List<Vector3> _path;
        private readonly Scout _owner;

        private Vector3 _targetPosition;
        private bool _isMoving;

        private readonly System.Action _completionCallback;
        private int _pathIndex;

        private readonly float _distanceThreshold;
        private readonly float _moveSpeed;

        public ScoutPathFollowTask(Scout recipient, List<Vector3> followPath, float threshold, float moveSpeed,
            System.Action onComplete) {
            _path = followPath;
            _owner = recipient;
            _completionCallback = onComplete;
            _distanceThreshold = threshold;
            _moveSpeed = moveSpeed;
        }

        public float Progress => 0.0f;

        public void OnTaskEnter() {
            //Ideally we could do some cleanup here to see if maybe we can join the path somewhere else apart from the start etc
            _isMoving = true;
            MoveTo(_path[0]);
        }

        public void OnTaskExit() {
        }

        public void Cancel() {
        }

        public void OnTaskCancel() {
        }

        public void OnTaskUpdate(float dt) {
            if (_owner.IsKilled) return;
            if (!_isMoving) return;

            Vector3 moveDir = (_targetPosition - _owner.transform.position).normalized;
            _owner.transform.position += (moveDir * (_moveSpeed * Time.deltaTime));

            if (Vector3.Distance(_owner.transform.position, _targetPosition) > _distanceThreshold) return;
            _pathIndex += 1;
            if (_pathIndex == _path.Count) {
                //We're done moving
                _isMoving = false;
                _completionCallback();
            } else {
                MoveTo(_path[_pathIndex]);
            }
        }

        private void MoveTo(Vector3 pos) {
            _targetPosition = pos;
            _targetPosition.y = 0.0f;
        }
    }

    private void Start() {
        max_hp = hp;

        moveSpeed *= 1.0f + (level - 1) * 0.1f;

        max_hp *= 1.0f + (level - 1) * 0.14f;

        hp = max_hp;
    }

    public void ClearTask() {
        CurrentTask = null;
    }

    public void Stop() {
        isStopped = true;
    }
}