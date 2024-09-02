using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour, IMessageHandler
{
    List<Spawner> spawners;
    bool waveStarted = false;

    //Current wave time in seconds
    float current_wave_time = 60.0f;

    // Start is called before the first frame update
    void Start()
    {
        MessageDispatcher.GetInstance().AddHandler(this);
        Spawner[] sps = GetComponentsInChildren<Spawner>();
        spawners = new List<Spawner>();
        spawners.AddRange(sps);

        //This starts the game off
        StartCoroutine(BeginWaves());
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

    // Update is called once per frame
    void Update()
    {
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
                    //We can proceed to the next wave
                    MessageDispatcher.GetInstance().Dispatch(new GameMessage(MessageConstants.EndWaveMessage));
                }
            }
        }
        
    }

    public void HandleMessage(GameMessage message)
    {
        switch (message.MessageType)
        {
            case MessageConstants.BeginWaveMessage:
                {
                    current_wave_time = 60.0f;
                    waveStarted = true;
                    foreach(var spawner in spawners)
                    {
                        spawner.StartSpawning();
                    }
                    break;
                }
            case MessageConstants.EndWaveMessage:
                {
                    waveStarted = false;
                    foreach (var spawner in spawners)
                    {
                        spawner.IncreaseLevel();
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
}
