using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour, IEnemy, ITaskable {
    [SerializeField] private GameObject forwardPoint;

    [SerializeField] private GameObject mechBody;

    [SerializeField] private float pathfindingDistanceThreshold;

    [SerializeField] private float moveSpeed;

    [SerializeField] private ParticleSystem sparks;

    [SerializeField] private ParticleSystem flash;

    [SerializeField] private AudioSource explosion;
    public Vector3 Position => forwardPoint.transform.position;
    public ISpawner Spawner { get; set; }

    // Cache the singleton instance
    private static readonly MessageDispatcher MessageDispatcher = MessageDispatcher.GetInstance();

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || _dying;

    public int CapitolDamage => 3;

    [SerializeField] private float hp;

    private float _maxHp;

    public float HitPoints => hp;

    public string Name => "Bossmech";

    private bool _dying;

    private ITask _currentTask;

    private int _level = 1;

    private bool _isStopped;

    [SerializeField] private float speedModifier = 0.08f;
    [SerializeField] private float hpModifier = 0.45f;

    public ITask CurrentTask {
        get => _currentTask;

        set {
            _currentTask?.OnTaskExit();
            if (value != null) {
                _currentTask = value;
                _currentTask.OnTaskEnter();
            } else {
                _currentTask = null;
            }
        }
    }

    public void SetLevel(int lv) {
        _level = lv;
    }

    public bool Busy => throw new System.NotImplementedException();

    // Update is called once per frame
    private void Update() {
        if (_isStopped || _dying) {
            return;
        }

        if (hp < 1.0f && !_dying) {
            //We only want the player to get the scrap if they kill it
            int scrap = (int)(_level * 0.7f * 500.0f);
            MessageDispatcher.Dispatch(new SingleValueMessage<int>(MessageConstants.AddScrap, scrap));
            Destroy();
        } else {
            CurrentTask?.OnTaskUpdate(Time.deltaTime);
        }
    }

    private void OnDestroy() {
        Spawner.DecreaseEnemyCount();
    }

    public void Damage(float value) {
        hp -= value;
    }

    public void CancelTask() {
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Bullet")) return;
        var bullet = collision.gameObject.GetComponentInParent<IBullet>();
        Destroy(collision.gameObject);
        Damage(bullet.Damage * (bullet.ArmourBonus ? 2.0f : 1.0f));
    }

    public void Destroy() {
        _dying = true;
        mechBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(gameObject, 2.0f);
        explosion.Play();
    }

    public void FollowPath(List<Vector3> pathPoints, System.Action onComplete) {
        CurrentTask = new MechPathFollowTask(this, pathPoints, pathfindingDistanceThreshold, moveSpeed, onComplete);
    }

    public void Attack(Building building) {
        throw new System.NotImplementedException();
    }

    public Dictionary<string, string> GetHoverData() {
        return new Dictionary<string, string>() {
            { "type", "enemy" },
            { "name", Name },
            { "hp", ((int)hp).ToString() },
            { "max_hp", ((int)_maxHp).ToString() }
        };
    }

    public void OnHoverOver(HoverInfo info) {
    }

    public void OnHoverOff() {
    }

    private class MechPathFollowTask : ITask {
        private readonly List<Vector3> _path;
        private readonly Mech _owner;

        private Vector3 _targetPosition;
        private bool _isMoving;

        private readonly System.Action _completionCallback;
        private int _pathIndex;

        private readonly float _distanceThreshold;
        private readonly float _moveSpeed;

        public MechPathFollowTask(Mech recipient, List<Vector3> followPath, float threshold, float moveSpeed,
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
            if (_owner.IsKilled) {
                return;
            }

            if (!_isMoving) return;
            Vector3 moveDir = (_targetPosition - _owner.transform.position).normalized;

            //Use this to orient our mech
            if (moveDir.x < -0.5f) {
                _owner.mechBody.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.up);
            } else if (moveDir.x > 0.5f) {
                _owner.mechBody.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
            } else if (moveDir.z > 0.5f) {
                _owner.mechBody.transform.rotation = Quaternion.AngleAxis(0.0f, Vector3.up);
            } else if (moveDir.z < -0.5f) {
                _owner.mechBody.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);
            }

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
        _maxHp = hp;

        moveSpeed *= 1.0f + (_level - 1) * speedModifier;

        _maxHp *= 1.0f + (_level - 1) * hpModifier;

        hp = _maxHp;
    }

    public void ClearTask() {
        CurrentTask = null;
    }

    public void Stop() {
        _isStopped = true;
    }
}