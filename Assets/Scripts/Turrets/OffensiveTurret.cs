using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OffensiveTurret : MonoBehaviour, ITurret
{

    [SerializeField]
    protected float rotationSpeed;

    [SerializeField]
    protected AudioSource turnSound;

    [SerializeField]
    protected AudioSource fireSound;

    [SerializeField]
    protected AudioSource reloadSound;

    //The animator controlling the animations of the turret
    [SerializeField]
    protected Animator animator;

    //The time between each shot
    [SerializeField]
    protected float base_delay_time;

    [SerializeField]
    protected float base_attack_range;

    [SerializeField]
    protected float base_attack_damage;

    [SerializeField]
    protected GameObject turretMesh;

    [SerializeField]
    protected EnemyDetector enemyDetector;
   
    protected List<AttackBoostBonus> attack_bonuses = new List<AttackBoostBonus>();

    protected ITargetable current_target = null;
    protected bool atTarget = true;

    [SerializeField]
    bool _online = false;

    [SerializeField]
    int _cost;

    public bool Online { get => _online; set => _online = value; }

    public int Cost => _cost;

    bool isStopped = false;


    public enum TurretState { SEEKING, FIRING, RESTING, CHILLING };

    /* Seeking is when the turret is rotating to try and get a lock on a target. 
     * Firing is when it fires on the target. 
     * Resting is while the turret reloads but it still tracks the target. 
     * Chilling is when the turret isn't tracking anything. */

    public TurretState currentState = TurretState.CHILLING;

    virtual public string TurretClass { get => throw new System.NotImplementedException(); }
    public int Level { get; set; }

    //This is the attack range for offensive turrets after bonuses are calculated
    public float Influence
    {
        get
        {
            float level_bar_bonus = 2.0f * (float)Level / 100.0f;
            float boost_bar_bonus = RangeBonus;
            float total = Mathf.Clamp(level_bar_bonus + boost_bar_bonus, 0.0f, 0.5f);
            return base_attack_range * (1.0f + total);
        }
    }

    [SerializeField]
    Building _building;

    public Building AttachedBuilding { get => _building; set => _building = value; }

    virtual public float BuildTime { get; }


    //This is how much damage a single shot does after bonuses are calculated
    protected float DamagePerShot
    {
        get
        {
            float level_atk_bonus = (float)Level / 100.0f;
            float boost_atk_bonus = AttackBonus;
            float total = Mathf.Clamp(level_atk_bonus + boost_atk_bonus, 0.0f, 1.0f);
            return base_attack_damage * (1.0f + total);
        }
    }

    //This is how long a turret takes to reload after bonuses are calculated
    protected float ReloadTime
    {
        get
        {
            float level_cd_bonus = 1.5f * (float)Level / 100.0f;
            float boost_cd_bonus = CooldownBonus;
            float total = Mathf.Clamp(level_cd_bonus + boost_cd_bonus, 0.0f, 0.35f);
            return base_delay_time * (1.0f - total);
        }
    }
    public bool CanFire { get => _canFire; }

    protected float current_delay_time = 0.0f;
    protected bool _canFire = true;

    //Rather than recalculating the bonus every update, we cache the value
    //and only recalculate it when an attack bonus is added or removed from the list
    //this helps with performance
    bool _mustRecalculateBonus = true;
    float current_attack_bonus = 0.0f;
    float current_range_bonus = 0.0f;
    float current_cooldown_bonus = 1.0f;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        enemyDetector.SetDetectionRadius(Influence);
        Level = 1;
    }

    bool SetTarget(ITargetable t)
    {
        if (Vector3.Distance(t.Position, turretMesh.transform.position) <= Influence)
        {
            //Setting a target that is in range automatically puts you in seek mode
            current_target = t;
            atTarget = false;
            currentState = TurretState.SEEKING;
            return true;
        }
        else
        {
            //Notify that we failed to set a target
            return false;
        }

    }

    protected bool LookAtTarget()
    {
        if (current_target == null || current_target.IsKilled)
        {
            current_target = null;
            return false;
        }

        //Returns a boolean if we can actually look, if the target is out of range we return false

        if (Vector3.Distance(current_target.Position, turretMesh.transform.position) > Influence)
        {
            //Fail if the target is out of range
            return false;
        }

        Quaternion current_rotation = turretMesh.transform.rotation;
        Vector3 direction = Vector3.Normalize(turretMesh.transform.position - current_target.Position);
        float angle = (Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg);

        Quaternion target_rotation = Quaternion.Euler(0.0f, angle, 0.0f);

        float angular_diff = Quaternion.Angle(current_rotation, target_rotation);

        if (angular_diff > 1)
        {
            Quaternion interpolated = Quaternion.Slerp(current_rotation, target_rotation, rotationSpeed * Time.deltaTime);
            turretMesh.transform.rotation = interpolated;
            atTarget = false;
        }
        else
        {
            turretMesh.transform.rotation = target_rotation;
            atTarget = true;
        }

        return true;
    }

    public Dictionary<string, string> GetHoverData()
    {
        if (!Online)
        {
            return null;
        }

        return new Dictionary<string, string>()
        {
            {"type" , "offensive_turret"},
            {"name", TurretClass },
            {"level", Level.ToString() },
            {"bhp", _building.hp.ToString() },
            {"atk_bonus", current_attack_bonus.ToString() },
            {"cd_bonus", current_cooldown_bonus.ToString() },
            {"range_bonus", current_range_bonus.ToString() }
        };
    }

    public virtual void OnHoverOver(HoverInfo info)
    {
    }

    public virtual void OnHoverOff()
    {
    }

    protected void PlayTurnSound()
    {
        if (!turnSound.isPlaying)
        {
            turnSound.Play();
        }
    }

    protected void StopTurnSound()
    {
        turnSound.Stop();
    }


    //Establishes how the turret should seek its target
    //should be fairly consistent among offensive turrets
    //but just in case I decide to get creative
    protected abstract void SeekBehaviour();

    //handles the actual firing of bullets or whatever
    protected abstract void FireBehaviour();

    //handles the reloading behaviour
    protected abstract void RestBehaviour();

    //handles the idle behaviour
    protected abstract void ChillBehaviour();

    protected virtual void Update()
    {
        if (isStopped)
        {
            return;
        }

        if (_mustRecalculateBonus)
        {
            RecalculateBonuses();
        }

        if (!Online)
        {
            return;
        }

        switch (currentState)
        {
            case TurretState.SEEKING:
                {
                    SeekBehaviour();
                    break;
                }
            case TurretState.FIRING:
                {
                    FireBehaviour();
                    break;
                }
            case TurretState.RESTING:
                {
                    RestBehaviour();
                    break;
                }
            case TurretState.CHILLING:
                {
                    ChillBehaviour();
                    break;
                }
            default:
                {
                    break;
                }
        }

        //This allows the turret to reload while it does other stuff
        if (currentState != TurretState.FIRING && current_delay_time >= 0.0f)
        {
            current_delay_time -= Time.deltaTime;
            if (current_delay_time <= 0.0f)
            {
                _canFire = true;
            }
            else
            {
                _canFire = false;
            }
        }

    }

    public void OnTurretSpawn()
    {
        Online = true;
        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<OffensiveTurret>(MessageConstants.RegisterOffensiveTurretMessage, this));
    }

    virtual public void OnTurretUpgrade()
    {
        //Don't need anything here really
    }

    public void OnTurretDestroy()
    {
        //Ditto
    }

    public void AddAttackBonus(AttackBoostBonus b)
    {
        if (attack_bonuses.Contains(b))
        {
            return;
        }
        else
        {
            attack_bonuses.Add(b);
            if (enemyDetector != null)
            {
                enemyDetector.SetDetectionRadius(Influence);
            }


        }
        _mustRecalculateBonus = true;

    }

    public void RemoveAttackBonus(AttackBoostBonus b)
    {
        if (!attack_bonuses.Contains(b))
        {
            return;
        }
        else
        {
            attack_bonuses.Remove(b);

            if(enemyDetector != null)
            {
                enemyDetector.SetDetectionRadius(Influence);
            }

        }
        _mustRecalculateBonus = true;

    }

    public void ForceRecalculation()
    {
        _mustRecalculateBonus = true;
        RecalculateBonuses();
    }

    void RecalculateBonuses()
    {
        if (_mustRecalculateBonus)
        {
            float total1 = 0.0f;
            float total2 = 0.0f;
            float total3 = 0.0f;
            foreach (AttackBoostBonus b in attack_bonuses)
            {
                total1 += b.RangeBonus;
                total2 += b.AttackBonus;
                total3 += b.CooldownBonus;
            }
            //50% is the maximum range bonus a turret can have
            //25% is the maximum attack bonus a turret can have
            //15% is the maximum cooldown bonus a turret can have
            current_range_bonus = Mathf.Clamp(total1, 0.0f, 0.5f);
            current_attack_bonus = Mathf.Clamp(total2, 0.0f, 0.25f);
            current_cooldown_bonus = Mathf.Clamp(total3, 0.0f, 0.15f);

            _mustRecalculateBonus = false;

        }


    }

    float RangeBonus
    {
        get
        {
            RecalculateBonuses();
            return current_range_bonus;
        }
    }

    float AttackBonus
    {
        get
        {
            RecalculateBonuses();
            return current_attack_bonus;
        }
    }

    float CooldownBonus
    {
        get
        {
            RecalculateBonuses();
            return current_cooldown_bonus;
        }
    }

    public void Stop()
    {
        isStopped = true;
        turnSound.Stop();
        fireSound.Stop();

        if(reloadSound != null)
        {
            reloadSound.Stop();
        }
    }

}
