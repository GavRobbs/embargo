using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour, IMessageHandler
{
    List<Spawner> spawners;
    bool waveStarted = false;

    bool bossSpawned = false;

    bool gameStopped = false;

    //Current wave time in seconds
    float current_wave_time = 60.0f;

    // Start is called before the first frame update
    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);
        Spawner[] sps = GetComponentsInChildren<Spawner>();
        spawners = new List<Spawner>();
        spawners.AddRange(sps);
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
                    current_wave_time = 60.0f;
                    waveStarted = true;
                    int special_spawner_index = Random.Range(0, spawners.Count);
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
                    bossSpawned = false;
                    waveStarted = false;
                    foreach (var spawner in spawners)
                    {
                        spawner.IncreaseLevel();
                        spawner.BossSpawnerForThisWave = false;
                    }
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
