using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour, IMessageHandler
{
    List<Spawner> spawners;
    bool waveStarted = false;

    bool bossSpawned = false;

    bool gameStopped = false;

    bool last_wave = false;

    //Current wave time in seconds
    float current_wave_time = 60.0f;

    int wave = 1;

    [SerializeField]
    BossSpawner bossSpawner;

    // Start is called before the first frame update
    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);
        Spawner[] sps = GetComponentsInChildren<Spawner>();
        spawners = new List<Spawner>();
        spawners.AddRange(sps);
    }

    void OnDestroy()
    {
        MessageDispatcher.GetInstance().RemoveHandler(this);
    }

    IEnumerator BeginWaves()
    {
        yield return new WaitForSeconds(5.0f);
        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.TriggerFirstWaveMessage));
    }

    int GetActiveEnemyCount()
    {
        int total = 0;
        foreach (var spawner in spawners)
        {
            total += spawner.EnemyCount;
        }
        return total;
    }

    void SpawnBoss()
    {
        foreach (var spawner in spawners)
        {
            spawner.SpawnBoss();
        }
        bossSpawned = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStopped)
        {
            return;
        }

        /* A wave lasts a certain amount of time, but sometimes we finish the wave, but still have enemies alive.
         * This prolongs the wave, it won't end until all the enemies are destroyed. The logic here handles this case. */
        if (waveStarted)
        {
            if (last_wave)
            {
                if(bossSpawned && bossSpawner.EnemyCount == 0)
                {
                    MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.EndWaveMessage));
                }
                return;
            }

            current_wave_time -= Time.deltaTime;

            if (current_wave_time <= 0.0f)
            {
                foreach (var spawner in spawners)
                {
                    spawner.StopSpawning();
                }

                if (GetActiveEnemyCount() == 0)
                {
                    if (bossSpawned)
                    {
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.EndWaveMessage));
                    }
                    else
                    {
                        SpawnBoss();
                    }
                    //We can proceed to the next wave
                }
            }
        }
        
    }

    public void HandleMessage(GameMessage message)
    {
        switch (message.MessageType)
        {
            case MessageConstants.StartGameMessage:
                //This starts the game off
                StartCoroutine(BeginWaves());
                break;
            case MessageConstants.BeginWaveMessage:
                {
                    if(wave == 6)
                    {
                        bossSpawner.StartSpawning();
                        bossSpawned = true;
                        waveStarted = true;
                        last_wave = true;
                        current_wave_time = 60.0f;
                        MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.NotifyBossBattleMessage));
                        return;
                    }

                    current_wave_time = 60.0f;
                    waveStarted = true;
                    int special_spawner_index = Random.Range(0, spawners.Count - 1);
                    foreach(var spawner in spawners)
                    {
                        spawner.BossSpawnerForThisWave = false;
                        spawner.StartSpawning();
                    }

                    spawners[special_spawner_index].BossSpawnerForThisWave = true;
                    break;
                }
            case MessageConstants.EndWaveMessage:
                {
                    if(wave == 6)
                    {
                        bossSpawned = false;
                        waveStarted = false;
                        return;
                    }

                    bossSpawned = false;
                    waveStarted = false;
                    foreach (var spawner in spawners)
                    {
                        spawner.IncreaseLevel();
                        spawner.BossSpawnerForThisWave = false;
                    }
                    wave += 1;
                    break;
                }
            case MessageConstants.GameOverMessage:
                {
                    foreach (var spawner in spawners)
                    {
                        spawner.StopSpawning();
                    }

                    foreach (var mb in FindObjectsOfType<MonoBehaviour>())
                    {
                        foreach(var stoppable in mb.GetComponentsInChildren<IStoppable>())
                        {
                            stoppable.Stop();
                        }
                    }

                    gameStopped = true;
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
}
