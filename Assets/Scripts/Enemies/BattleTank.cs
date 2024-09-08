using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BattleTank : MonoBehaviour, IEnemy, ITaskable {
    [SerializeField] private GameObject forwardPoint;

    [FormerlySerializedAs("BattleTankBody")] [SerializeField]
    private GameObject battleTankBody;

    [SerializeField] private float pathfindingDistanceThreshold;

    [SerializeField] private float moveSpeed;

    [SerializeField] private float turretRotationSpeed;

    [SerializeField] private ParticleSystem sparks;

    [SerializeField] private ParticleSystem flash;

    [SerializeField] private AudioSource explosion;

    [FormerlySerializedAs("target_detector")] [SerializeField]
    private TargetDetector targetDetector;

    [SerializeField] private GameObject turretMesh;

    [SerializeField] private GameObject shotPrefab;

    [SerializeField] private GameObject bulletSpawnPoint;
    public Vector3 Position => forwardPoint.transform.position;
    public ISpawner Spawner { get; set; }

    // Cache the singleton instance
    private static readonly MessageDispatcher MessageDispatcher = MessageDispatcher.GetInstance();
    [SerializeField] private float speedModifier = 0.1f;
    [SerializeField] private float hpModifier = 0.2f;

    public int CapitolDamage => 1;

    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || _dying;

    [SerializeField] private float hp;

    private float _maxHp;

    public float HitPoints => hp;

    public string Name => "BattleTank";

    private bool _dying;

    private ITask _currentTask;

    private int _level = 1;

    private bool _isStopped;

    [SerializeField] private float attackIntervalMin = 2.0f;

    [SerializeField] private float attackIntervalMax = 4.0f;

    private Building _currentTarget;


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
        if (_isStopped) {
            return;
        }

        if (hp < 1.0f && !_dying) {
            //We only want the player to get the scrap if they kill it
            int scrap = (int)(_level * 0.7f * 60.0f);
            MessageDispatcher.Dispatch(new SingleValueMessage<int>(MessageConstants.AddScrap, scrap));
            Destroy();
        } else {
            if (CurrentTask != null) {
                CurrentTask.OnTaskUpdate(Time.deltaTime);
            }
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

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Bullet")) {
            IBullet b = collision.gameObject.GetComponentInParent<IBullet>();
            float dmg = b.Damage;
            float mul = b.ArmourBonus ? 1.6f : 1.0f;
            Destroy(collision.gameObject);
            Damage(dmg * mul);
        }
    }

    public void Destroy() {
        _dying = true;
        battleTankBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(this.gameObject, 2.0f);
        explosion.Play();
    }

    public void FollowPath(List<Vector3> pathPoints, System.Action onComplete) {
        CurrentTask =
            new BattleTankPathFollowTask(this, pathPoints, pathfindingDistanceThreshold, moveSpeed, onComplete);
    }

    public void Attack(Building building) {
        //throw new System.NotImplementedException();
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

    private void Fire() {
        GameObject.Instantiate(shotPrefab, bulletSpawnPoint.transform.position, bulletSpawnPoint.transform.rotation);
    }

    private class BattleTankPathFollowTask : ITask {
        private readonly List<Vector3> _path;
        private readonly BattleTank _owner;

        private Vector3 _targetPosition;
        private bool _isMoving;

        private readonly System.Action _completionCallback;
        private int _pathIndex;

        private readonly float _distanceThreshold;
        private readonly float _moveSpeed;
        private float _nextAttackTimer;
        private bool _isTracking;

        public BattleTankPathFollowTask(BattleTank recipient, List<Vector3> followPath, float threshold,
            float moveSpeed, System.Action onComplete) {
            _path = followPath;
            _owner = recipient;
            _completionCallback = onComplete;
            _distanceThreshold = threshold;
            _moveSpeed = moveSpeed;

            _nextAttackTimer = Random.Range(recipient.attackIntervalMin, recipient.attackIntervalMax);
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

        private void LookAtTargetAndFire() {
            if (!_owner._currentTarget) {
                _isTracking = false;
                _nextAttackTimer = Random.Range(_owner.attackIntervalMin, _owner.attackIntervalMax);
                return;
            }

            Quaternion current_rotation = _owner.turretMesh.transform.rotation;
            Vector3 direction =
                Vector3.Normalize(_owner.turretMesh.transform.position - _owner._currentTarget.transform.position);
            float angle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg);

            Quaternion target_rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            float angular_diff = Quaternion.Angle(current_rotation, target_rotation);

            if (angular_diff > 1) {
                Quaternion interpolated = Quaternion.Slerp(current_rotation, target_rotation,
                    _owner.turretRotationSpeed * Time.deltaTime);
                _owner.turretMesh.transform.rotation = interpolated;
            } else {
                _owner.turretMesh.transform.rotation = target_rotation;
                _owner.Fire();
                _nextAttackTimer = Random.Range(_owner.attackIntervalMin, _owner.attackIntervalMax);
                _isTracking = false;
            }
        }

        public void OnTaskUpdate(float dt) {
            if (_owner.IsKilled) {
                return;
            }

            if (!_isMoving) return;

            Vector3 moveDir = (_targetPosition - _owner.transform.position).normalized;

            if (moveDir.x < -0.5f) {
                _owner.battleTankBody.transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.up);
            } else if (moveDir.x > 0.5f) {
                _owner.battleTankBody.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.up);
            } else if (moveDir.z > 0.5f) {
                _owner.battleTankBody.transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.up);
            } else if (moveDir.z < -0.5f) {
                _owner.battleTankBody.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
            }

            _owner.transform.position += (moveDir * (_moveSpeed * Time.deltaTime));

            if (_isTracking) {
                //Rotate the turret appropriately
                //and fire if we reach
                LookAtTargetAndFire();
            } else {
                _nextAttackTimer -= Time.deltaTime;
                if (_nextAttackTimer <= 0.0) {
                    _nextAttackTimer = Random.Range(_owner.attackIntervalMin, _owner.attackIntervalMax);
                    _isTracking = true;
                    _owner._currentTarget = _owner.targetDetector.GetRandomTarget();

                    if (!_owner._currentTarget) {
                        _isTracking = false;

                        //Make the next interval shorter so its more inclined to attack again
                        _nextAttackTimer /= 2.0f;
                    }
                }
            }


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

        moveSpeed *= 1.0f + (_level - 1) * 0.1f;

        _maxHp *= 1.0f + (_level - 1) * 0.20f;

        hp = _maxHp;
    }

    public void ClearTask() {
        CurrentTask = null;
    }

    public void Stop() {
        _isStopped = true;
    }
}