using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour, ISpawner
{
    [SerializeField]
    float minimumSpawnInterval;

    [SerializeField]
    float maximumSpawnInterval;

    [SerializeField]
    GameObject scoutPrefab;

    [SerializeField]
    GameObject mechPrefab;

    [SerializeField]
    GameObject battleTankPrefab;

    float modifiedMinSpawnInterval;
    float modifiedMaxSpawnInterval;

    GameMap map;

    float nextSpawnInterval;

    int enemy_count = 0;
    public int EnemyCount { get => enemy_count; }

    bool currently_active = false;
    public bool BossSpawnerForThisWave { get; set; }

    public bool IsSpecialSpawner => false;

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
        modifiedMinSpawnInterval = (1.0f - ((float)level * 0.09f)) * minimumSpawnInterval;
        modifiedMaxSpawnInterval = (1.0f - ((float)level * 0.09f)) * maximumSpawnInterval;
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

            int val = Random.Range(0, 100);
            if(val % 6 == 0)
            {
                SpawnBattletank();
            }
            else
            {
                SpawnScout();
            }
            enemy_count += 1;
        }
        
    }

    public void SpawnBoss()
    {
        if (BossSpawnerForThisWave)
        {
            SpawnMech();
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

    private void SpawnBattletank()
    {
        BattleTank new_bt = GameObject.Instantiate(battleTankPrefab, transform.position, Quaternion.identity).GetComponent<BattleTank>();
        new_bt.Spawner = this;
        new_bt.SetLevel(level);
        Vector3 target = map.GetRandomTargetPosition();
        List<Vector3> path = map.GetPath(transform.position, target);

        new_bt.FollowPath(
            path,
            () => { new_bt.KillMe(); }
        );
    }

    private void SpawnMech()
    {
        Mech new_mech = GameObject.Instantiate(mechPrefab, transform.position, Quaternion.identity).GetComponent<Mech>();
        new_mech.Spawner = this;
        new_mech.SetLevel(level);
        Vector3 target = map.GetRandomTargetPosition();
        List<Vector3> path = map.GetPath(transform.position, target);

        new_mech.FollowPath(
            path,
            () => { new_mech.KillMe(); }
        );

    }
}
