using System;
using System.Collections.Generic;
using UnityEngine;

public class FinalBoss : MonoBehaviour, IEnemy, ITaskable {
    [SerializeField] private GameObject forwardPoint;

    [SerializeField] private GameObject mechBody;

    [SerializeField] private float pathfindingDistanceThreshold;

    [SerializeField] private float moveSpeed;

    [SerializeField] private ParticleSystem sparks;

    [SerializeField] private ParticleSystem flash;

    [SerializeField] private AudioSource explosion;

    [SerializeField] private AudioSource teleportSound;
    public Vector3 Position => forwardPoint.transform.position;

    public ISpawner Spawner { get; set; }

    // Cache the singleton instance
    private static readonly MessageDispatcher MessageDispatcher = MessageDispatcher.GetInstance();

    [SerializeField] private float speedModifier = 0.06f;
    [SerializeField] private float hpModifier = 0.45f;
    public bool IsFriendly => false;

    public bool IsKilled => (HitPoints <= 0.0f) || _dying;

    public int CapitolDamage => 6;

    [SerializeField] private float hp;

    private float _maxHp;

    public float HitPoints => hp;

    public string Name => "Zorg Overlord";

    private bool _dying;

    private ITask _currentTask;

    private int _level = 1;

    private bool _isStopped;

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
        _level = lv;
    }

    public bool Busy => true;

    // Update is called once per frame
    private void Update() {
        if (_isStopped || _dying) {
            return;
        }

        if (hp < 1.0f && !_dying) {
            Destroy();
            MessageDispatcher.Dispatch(new GameMessage(MessageConstants.BossKilledMessage));
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
        IBullet bullet = collision.gameObject.GetComponentInParent<IBullet>();
        Destroy(collision.gameObject);
        Damage(bullet.Damage * (bullet.ArmourBonus ? 1.8f : 1.0f));
    }

    public void Destroy() {
        //TODO: Special Handling for when the boss is destroyed
        _dying = true;
        mechBody.SetActive(false);
        sparks.Play();
        flash.Play();
        GameObject.Destroy(gameObject, 2.0f);
        explosion.Play();
    }

    public void Wander(List<Vector3> path1, List<Vector3> path2, List<Vector3> path3, List<Vector3> path4) {
        //The boss basically walks to the target - every 500 hp damage or so he teleports to a different path

        Vector3 teleport1 = path2[0];
        Vector3 teleport2 = path3[0];
        Vector3 teleport3 = path4[0];

        CurrentTask = new BossPathFollowTask(this, 1200, path1, pathfindingDistanceThreshold, moveSpeed, () => {
            CurrentTask = new BossTeleportTask(this, teleport1, () => {
                CurrentTask = new BossPathFollowTask(this, 800, path2, pathfindingDistanceThreshold, moveSpeed, () => {
                    CurrentTask = new BossTeleportTask(this, teleport2, () => {
                        CurrentTask = new BossPathFollowTask(this, 400, path3, pathfindingDistanceThreshold, moveSpeed,
                            () => {
                                CurrentTask = new BossTeleportTask(this, teleport3, () => {
                                    CurrentTask = new BossPathFollowTask(this, 0.0f, path4,
                                        pathfindingDistanceThreshold, moveSpeed, () => {
                                            //You're going to die so it doesn't really matter what happens here
                                        });
                                });
                            });
                    });
                });
            });
        });
    }

    public void Attack(Building building) {
        throw new NotImplementedException();
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

    private class BossPathFollowTask : ITask {
        private readonly List<Vector3> _path;
        private readonly FinalBoss _owner;

        private Vector3 _targetPosition;
        private bool _isMoving;

        private readonly Action _completionCallback;
        private int _pathIndex;

        private readonly float _distanceThreshold;
        private readonly float _moveSpeed;
        private readonly float _hpThresholdToTeleport;

        public BossPathFollowTask(FinalBoss recipient, float tpThresh, List<Vector3> fpath, float threshold,
            float moveSpeed, Action onComplete) {
            _owner = recipient;
            _completionCallback = onComplete;
            _distanceThreshold = threshold;
            _moveSpeed = moveSpeed;
            _path = fpath;
            _hpThresholdToTeleport = tpThresh;
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

            if (_owner.hp <= _hpThresholdToTeleport) {
                _isMoving = false;
                _completionCallback();
                return;
            }

            if (_isMoving) {
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

                if (Vector3.Distance(_owner.transform.position, _targetPosition) <= _distanceThreshold) {
                    _pathIndex += 1;
                    if (_pathIndex == _path.Count) {
                        //We're done moving
                        _isMoving = false;
                        _completionCallback();
                    } else {
                        MoveTo(_path[_pathIndex]);
                    }
                }
            }
        }

        private void MoveTo(Vector3 pos) {
            _targetPosition = pos;
            _targetPosition.y = 0.0f;
        }
    }

    private class BossTeleportTask : ITask {
        public float Progress => 1.0f;
        private readonly FinalBoss _boss;
        private readonly Vector3 _newPos;
        private readonly Action _completeCallback;

        public BossTeleportTask(FinalBoss me, Vector3 teleportPosition, Action onComplete) {
            _completeCallback = onComplete;
            _boss = me;
            _newPos = teleportPosition;
        }

        public void Cancel() {
        }

        public void OnTaskCancel() {
        }

        public void OnTaskEnter() {
            _boss.GetComponent<Rigidbody>().transform.position = _newPos;
            _boss.teleportSound.Play();
            _completeCallback();
        }

        public void OnTaskExit() {
        }

        public void OnTaskUpdate(float dt) {
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

    public void FollowPath(List<Vector3> pathPoints, Action onComplete) {
        throw new NotImplementedException();
    }
}