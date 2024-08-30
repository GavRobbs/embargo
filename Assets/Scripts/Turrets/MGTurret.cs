using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGTurret : OffensiveTurret
{
    bool isFiring = false;

    [SerializeField]
    ParticleSystem cannon1_ps;

    [SerializeField]
    ParticleSystem cannon2_ps;

    //This is a targeting laser for decoration
    [SerializeField]
    GameObject laser;

    //How long the machinegun fires for, default 2s, but tweak for gameplay
    [SerializeField]
    float fireTime = 2.0f;

    [SerializeField]
    float bullet_delay_seconds = 0.3f;

    [SerializeField]
    GameObject bulletPrefab;

    int _level = 1;

    override public string TurretClass { get => "Machinegun";  }
    override public int Level { get => _level; }

    //These help to determine how long a machinegun volley lasts
    [SerializeField]
    float last_bullet_time = 0.0f;

    [SerializeField]
    float current_fire_time = 0.0f;
    protected override void ChillBehaviour()
    {
        LaserOff();
        StopTurnSound();

        current_target = enemyDetector.PickEnemy();

        if (current_target == null || current_target.IsKilled)
        {
            return;
        }

        bool inRange = LookAtTarget();

        if (inRange)
        {
            if (CanFire)
            {
                currentState = TurretState.FIRING;
            }
            else
            {
                currentState = TurretState.RESTING;
            }
            return;
        }
    }

    protected override void FireBehaviour()
    {
        bool inRange = LookAtTarget();

        if (!inRange)
        {
            currentState = TurretState.CHILLING;
            //Debug.Log("STOPPED because not in range");
            StopFiring(false);
            return;
        }

        LaserOn();
        StartFiring();
        if (atTarget)
        {
            StopTurnSound();
        }
        else
        {
            PlayTurnSound();
        }
        current_fire_time -= Time.deltaTime;
        last_bullet_time -= Time.deltaTime;

        if (last_bullet_time <= 0.0f)
        {
            SpawnBullets();
            last_bullet_time = bullet_delay_seconds;
        }

        if (current_fire_time <= 0.0f)
        {
            currentState = TurretState.RESTING;
            StopFiring(true);
            current_delay_time = ReloadTime;

        }
    }

    protected override void RestBehaviour()
    {
        bool inRange = LookAtTarget();
        if (!inRange)
        {
            currentState = TurretState.CHILLING;
            return;
        }

        if (atTarget)
        {
            StopTurnSound();
        }
        else
        {
            PlayTurnSound();
        }


        if (current_delay_time <= 0.0f)
        {
            currentState = TurretState.SEEKING;
        }
    }

    protected override void SeekBehaviour()
    {
        bool inRange = LookAtTarget();
        LaserOff();

        if (!inRange)
        {
            currentState = TurretState.CHILLING;
            return;
        }

        if (atTarget)
        {
            StopTurnSound();
            if (CanFire)
            {
                currentState = TurretState.FIRING;
            }
            else
            {
                currentState = TurretState.RESTING;
            }
        }
        else
        {
            PlayTurnSound();
        }
    }

    void StartFiring()
    {
        if (isFiring)
        {
            return;
        }

        animator.SetBool("firing", true);
        fireSound.Play();
        cannon1_ps.Play();
        cannon2_ps.Play();
        current_fire_time = fireTime;
        current_delay_time = ReloadTime;
        isFiring = true;
        //Debug.Log("START FIRING");

    }

    void StopFiring(bool play_reload = true)
    {
        cannon1_ps.Stop();
        cannon2_ps.Stop();
        animator.SetBool("firing", false);
        fireSound.Stop();

        if (play_reload)
        {
            reloadSound.Play();
        }
        isFiring = false;
        //Debug.Log("STOP FIRING");
    }

    void LaserOn()
    {
        laser.SetActive(true);
    }

    void LaserOff()
    {
        laser.SetActive(false);
    }

    void SpawnBullets()
    {
        //Half damage because it fires two bullets at a time
        GameObject b1 = GameObject.Instantiate(bulletPrefab, cannon1_ps.transform.position, cannon1_ps.transform.rotation);
        b1.GetComponent<MGBullet>().Damage = DamagePerShot / 2.0f;
        GameObject b2 = GameObject.Instantiate(bulletPrefab, cannon2_ps.transform.position, cannon1_ps.transform.rotation);
        b2.GetComponent<MGBullet>().Damage = DamagePerShot / 2.0f;
    }

    override protected void Start()
    {
        cannon1_ps.Stop();
        cannon2_ps.Stop();
        base.Start();
    }

    override protected void Update()
    {
        base.Update();
    }

    public override void OnHoverOver()
    {
        Debug.Log("Hovering over MG " + gameObject.name);
    }

    public override void OnHoverOff()
    {
        
    }

}
