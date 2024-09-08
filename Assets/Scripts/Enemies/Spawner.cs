using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour, ISpawner, ILeveler {
    [SerializeField] private float minimumSpawnInterval;

    [SerializeField] private float maximumSpawnInterval;

    [SerializeField] private GameObject scoutPrefab;

    [SerializeField] private GameObject mechPrefab;

    [SerializeField] private GameObject battleTankPrefab;

    private float _modifiedMinSpawnInterval;
    private float _modifiedMaxSpawnInterval;

    private GameMap _map;

    private float _nextSpawnInterval;

    public int EnemyCount { get; private set; }

    private bool _currentlyActive;
    public bool BossSpawnerForThisWave { get; set; }

    public bool IsSpecialSpawner => false;

    private int _level = 1;

    private void Start() {
        _map = FindObjectOfType<GameMap>();
        _modifiedMinSpawnInterval = minimumSpawnInterval;
        _modifiedMaxSpawnInterval = maximumSpawnInterval;
        _nextSpawnInterval = Random.Range(_modifiedMinSpawnInterval, _modifiedMaxSpawnInterval);
    }

    public void StartSpawning() {
        _currentlyActive = true;
    }

    public void StopSpawning() {
        _currentlyActive = false;
    }

    public void IncreaseLevel() {
        _level += 1;
        _modifiedMinSpawnInterval = (1.0f - (_level * 0.09f)) * minimumSpawnInterval;
        _modifiedMaxSpawnInterval = (1.0f - (_level * 0.09f)) * maximumSpawnInterval;
    }

    private void Update() {
        if (!_currentlyActive) return;

        _nextSpawnInterval -= Time.deltaTime;

        if (_nextSpawnInterval >= 0.0f) return;
        _nextSpawnInterval = Random.Range(_modifiedMinSpawnInterval, _modifiedMaxSpawnInterval);

        var val = Random.Range(0, 100);
        if (val % 6 == 0) {
            SpawnBattleTank();
        } else {
            SpawnScout();
        }

        EnemyCount += 1;
    }

    public void SpawnBoss() {
        if (!BossSpawnerForThisWave) return;

        SpawnMech();
        EnemyCount += 1;
    }

    public void DecreaseEnemyCount() {
        EnemyCount -= 1;
    }

    private void SpawnScout() {
        Scout new_scout = Instantiate(scoutPrefab, transform.position, Quaternion.identity)
            .GetComponent<Scout>();
        new_scout.Spawner = this;
        new_scout.SetLevel(_level);
        Vector3 target = _map.GetRandomTargetPosition();
        List<Vector3> path = _map.GetPath(transform.position, target);

        new_scout.FollowPath(
            path,
            () => { new_scout.Destroy(); }
        );
    }

    private void SpawnBattleTank() {
        BattleTank new_bt = Instantiate(battleTankPrefab, transform.position, Quaternion.identity)
            .GetComponent<BattleTank>();
        new_bt.Spawner = this;
        new_bt.SetLevel(_level);
        Vector3 target = _map.GetRandomTargetPosition();
        List<Vector3> path = _map.GetPath(transform.position, target);

        new_bt.FollowPath(path, () => { new_bt.Destroy(); });
    }

    private void SpawnMech() {
        Mech new_mech = Instantiate(mechPrefab, transform.position, Quaternion.identity).GetComponent<Mech>();
        new_mech.Spawner = this;
        new_mech.SetLevel(_level);
        Vector3 target = _map.GetRandomTargetPosition();
        List<Vector3> path = _map.GetPath(transform.position, target);

        new_mech.FollowPath(path, () => { new_mech.Destroy(); });
    }
}