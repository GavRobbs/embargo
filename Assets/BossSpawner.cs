using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour, ISpawner
{
    GameMap map;

    [SerializeField]
    GameObject finalBossPrefab;
    public bool IsSpecialSpawner => true;

    public bool BossSpawnerForThisWave { get => false; set => _ = value; }

    public int EnemyCount => enemy_count;

    bool currently_active = false;

    int enemy_count = 0;

    void Start()
    {
        map = FindObjectOfType<GameMap>();

    }

    public void StartSpawning()
    {
        currently_active = true;
        SpawnFinalBoss();
    }

    public void StopSpawning()
    {
        currently_active = false;
    }

    public void DecreaseEnemyCount()
    {
        enemy_count -= 1;
    }

    public void IncreaseLevel()
    {

    }

    void SpawnFinalBoss()
    {
        //First position 3, -5
        //Second position -4 4
        //Third position -4 -5
        //Final position 5 4

        Vector3 target = map.GetRandomTargetPosition();
        var path1 = map.GetPath(transform.position, target);
        var path2 = map.GetPath(new Vector3(-4.0f, 0.0f, 4.0f), target);
        var path3 = map.GetPath(new Vector3(-4.0f, 0.0f, -5.0f), target);
        var path4 = map.GetPath(new Vector3(5.0f, 0.0f, 4.0f), target);
        FinalBoss fb = GameObject.Instantiate(finalBossPrefab, transform.position, Quaternion.identity).GetComponent<FinalBoss>();
        fb.Wander(path1, path2, path3, path4);
        fb.Spawner = this;
        enemy_count += 1;
    }
}
