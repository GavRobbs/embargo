using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour, IMessageHandler {
    private List<Spawner> _spawners;
    private bool _waveStarted;

    private bool _bossSpawned;

    private bool _gameStopped;

    private bool _lastWave;

    //Current wave time in seconds
    private float _currentWaveTime = 60.0f;

    private int _wave = 1;
    
    private List<IStoppable> _stoppables;

    [SerializeField] private BossSpawner bossSpawner;

    // Cache the singleton instance
    private static readonly MessageDispatcher MessageDispatcher = MessageDispatcher.GetInstance();

    // Start is called before the first frame update
    private void Start() {
        MessageDispatcher.AddHandler(this);
        Spawner[] sps = GetComponentsInChildren<Spawner>();
        _spawners = new List<Spawner>();
        _spawners.AddRange(sps);
        
        // Cache all IStoppable components
        _stoppables = new List<IStoppable>();
        foreach (var mb in FindObjectsOfType<MonoBehaviour>()) {
            _stoppables.AddRange(mb.GetComponentsInChildren<IStoppable>());
        }
    }

    private void OnDestroy() {
        MessageDispatcher.RemoveHandler(this);
    }

    private IEnumerator BeginWaves() {
        yield return new WaitForSeconds(5.0f);
        MessageDispatcher.Dispatch(new GameMessage(MessageConstants.TriggerFirstWaveMessage));
    }

    private int GetActiveEnemyCount() {
        return _spawners.Sum(spawner => spawner.EnemyCount);
    }

    private void SpawnBoss() {
        foreach (var spawner in _spawners) {
            spawner.SpawnBoss();
        }

        _bossSpawned = true;
    }

    // Update is called once per frame
    private void Update() {
        if (_gameStopped) return;

        /* A wave lasts a certain amount of time, but sometimes we finish the wave, but still have enemies alive.
         * This prolongs the wave, it won't end until all the enemies are destroyed. The logic here handles this case. */
        if (!_waveStarted) return;

        if (_lastWave) {
            if (_bossSpawned && bossSpawner.EnemyCount == 0) {
                MessageDispatcher.Dispatch(new GameMessage(MessageConstants.EndWaveMessage));
            }

            return;
        }

        _currentWaveTime -= Time.deltaTime;

        if (_currentWaveTime > 0.0f) return;
        foreach (var spawner in _spawners) {
            spawner.StopSpawning();
        }

        if (GetActiveEnemyCount() != 0) return;

        if (_bossSpawned) {
            MessageDispatcher.Dispatch(new GameMessage(MessageConstants.EndWaveMessage));
        } else {
            SpawnBoss();
        }
        //We can proceed to the next wave
    }

    public void HandleMessage(GameMessage message) {
        switch (message.MessageType) {
            case MessageConstants.StartGameMessage:
                //This starts the game off
                StartCoroutine(BeginWaves());
                break;
            case MessageConstants.BeginWaveMessage: {
                if (_wave == 6) {
                    bossSpawner.StartSpawning();
                    _bossSpawned = true;
                    _waveStarted = true;
                    _lastWave = true;
                    _currentWaveTime = 60.0f;
                    MessageDispatcher.Dispatch(new GameMessage(MessageConstants.NotifyBossBattleMessage));
                    return;
                }

                _currentWaveTime = 60.0f;
                _waveStarted = true;
                int special_spawner_index = Random.Range(0, _spawners.Count - 1);
                foreach (var spawner in _spawners) {
                    spawner.BossSpawnerForThisWave = false;
                    spawner.StartSpawning();
                }

                _spawners[special_spawner_index].BossSpawnerForThisWave = true;
                break;
            }
            case MessageConstants.EndWaveMessage: {
                if (_wave == 6) {
                    _bossSpawned = false;
                    _waveStarted = false;
                    return;
                }

                _bossSpawned = false;
                _waveStarted = false;
                foreach (var spawner in _spawners) {
                    spawner.IncreaseLevel();
                    spawner.BossSpawnerForThisWave = false;
                }

                _wave += 1;
                MessageDispatcher.Dispatch(
                    new SingleValueMessage<int>(MessageConstants.UpdateWaveCounterMessage, _wave));
                break;
            }
            case MessageConstants.GameOverMessage: {
                foreach (var spawner in _spawners) {
                    spawner.StopSpawning();
                }

                foreach (var stoppable in _stoppables) {
                    stoppable.Stop();
                }

                _gameStopped = true;
                break;
            }
        }
    }
}