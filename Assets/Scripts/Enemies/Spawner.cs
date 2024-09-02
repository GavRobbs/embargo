using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    float minimumSpawnInterval;

    [SerializeField]
    float maximumSpawnInterval;

    [SerializeField]
    GameObject scoutPrefab;

    float modifiedMinSpawnInterval;
    float modifiedMaxSpawnInterval;

    GameMap map;

    float nextSpawnInterval;

    int enemy_count = 0;
    public int EnemyCount { get => enemy_count; }

    bool currently_active = false;

    int level = 1;

    void Start()
    {
        map = FindObjectOfType<GameMap>();
        modifiedMinSpawnInterval = minimumSpawnInterval;
        modifiedMaxSpawnInterval = maximumSpawnInterval;
        nextSpawnInterval = Random.Range(modifiedMinSpawnInterval, modifiedMaxSpawnInterval);
    }

    public void StartSpawning()
    {
        currently_active = true;
    }

    public void StopSpawning()
    {
        currently_active = false;
    }

    public void IncreaseLevel()
    {
        level += 1;
        modifiedMinSpawnInterval = (1.0f - ((float)level * 0.06f)) * minimumSpawnInterval;
        modifiedMaxSpawnInterval = (1.0f - ((float)level * 0.06f)) * maximumSpawnInterval;
    }

    void Update()
    {
        if (!currently_active)
        {
            return;
        }

        nextSpawnInterval -= Time.deltaTime;

        if(nextSpawnInterval < 0.0f)
        {
            nextSpawnInterval = Random.Range(modifiedMinSpawnInterval, modifiedMaxSpawnInterval);
            SpawnScout();
            enemy_count += 1;
        }
        
    }

    public void DecreaseEnemyCount()
    {
        enemy_count -= 1;
    }

    private void SpawnScout()
    {
        Scout new_scout = GameObject.Instantiate(scoutPrefab, transform.position, Quaternion.identity).GetComponent<Scout>();
        new_scout.Spawner = this;
        new_scout.SetLevel(level);
        Vector3 target = map.GetRandomTargetPosition();
        List<Vector3> path = map.GetPath(transform.position, target);
       
        new_scout.FollowPath(
            path,
            () => { new_scout.KillMe(); }
        );
    }
}
