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

    [SerializeField]
    GameMap map;

    float nextSpawnInterval;

    void Start()
    {
        nextSpawnInterval = Random.Range(minimumSpawnInterval, maximumSpawnInterval);
    }

    void Update()
    {
        nextSpawnInterval -= Time.deltaTime;

        if(nextSpawnInterval < 0.0f)
        {
            nextSpawnInterval = Random.Range(minimumSpawnInterval, maximumSpawnInterval);
            SpawnScout();
        }
        
    }

    private void SpawnScout()
    {
        Scout new_scout = GameObject.Instantiate(scoutPrefab, transform.position, Quaternion.identity).GetComponent<Scout>();
        Vector3 target = map.GetRandomTargetPosition();
        List<Vector3> path = map.GetPath(transform.position, target);
       
        new_scout.FollowPath(
            path,
            () => { new_scout.KillMe(); }
        );
    }
}
