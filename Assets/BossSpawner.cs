using UnityEngine;

public class BossSpawner : MonoBehaviour, ISpawner {
    private GameMap _map;

    [SerializeField] private GameObject finalBossPrefab;
    public bool IsSpecialSpawner => true;

    public bool BossSpawnerForThisWave {
        get => false;
        set => _ = value;
    }

    public int EnemyCount { get; private set; }

    private void Start() {
        _map = FindObjectOfType<GameMap>();
    }

    public void StartSpawning() {
        SpawnFinalBoss();
    }

    public void StopSpawning() {
    }

    public void DecreaseEnemyCount() {
        EnemyCount -= 1;
    }

    private void SpawnFinalBoss() {
        //First position 3, -5
        //Second position -4 4
        //Third position -4 -5
        //Final position 5 4

        Vector3 target = _map.GetRandomTargetPosition();
        var path1 = _map.GetPath(transform.position, target);
        var path2 = _map.GetPath(new Vector3(-4.0f, 0.0f, 4.0f), target);
        var path3 = _map.GetPath(new Vector3(-4.0f, 0.0f, -5.0f), target);
        var path4 = _map.GetPath(new Vector3(5.0f, 0.0f, 4.0f), target);
        FinalBoss fb = Instantiate(finalBossPrefab, transform.position, Quaternion.identity).GetComponent<FinalBoss>();
        fb.Wander(path1, path2, path3, path4);
        fb.Spawner = this;
        EnemyCount += 1;
    }
}