using UnityEngine;

public interface ISpawner 
{
    int EnemyCount { get; }
    bool BossSpawnerForThisWave { get; set; }
    bool IsSpecialSpawner { get; }

    void StartSpawning();

    void StopSpawning();

    void DecreaseEnemyCount();

    void IncreaseLevel();


}
