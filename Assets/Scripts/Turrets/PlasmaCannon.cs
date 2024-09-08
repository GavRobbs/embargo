using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaCannon : OffensiveTurret
{
    [SerializeField]
    ParticleSystem cannon_ps;

    [SerializeField]
    GameObject bulletPrefab;
    public override float BuildTime => 8.0f;

    override public string TurretClass { get => "Plasma Cannon"; }

    protected override void ChillBehaviour()
    {
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
        
        current_delay_time -= Time.deltaTime;

        if (current_delay_time <= 0.0f)
        {
            if (atTarget)
            {
                Fire();
                SpawnBullet();
                currentState = TurretState.RESTING;
            }
            
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

    void Fire()
    {
        animator.SetTrigger("firing");
        fireSound.Play();
        cannon_ps.Play();
        current_delay_time = ReloadTime;
        //Debug.Log("START FIRING");

    }

    void SpawnBullet()
    {
        GameObject b1 = GameObject.Instantiate(bulletPrefab, cannon_ps.transform.position, cannon_ps.transform.rotation);
        b1.GetComponent<PlasmaBolt>().Damage = DamagePerShot;
    }

    override protected void Start()
    {
        cannon_ps.Stop();
        base.Start();
    }

    override protected void Update()
    {
        base.Update();
    }

    public override void OnHoverOver(HoverInfo info)
    {
        if (info == null)
        {
            return;
        }

        if (info.mode == GameInputManager.HOVER_MODE.UPGRADE && Online)
        {
            AttachedBuilding?.ActivateArrow();
            return;

        }

        if (info.mode == GameInputManager.HOVER_MODE.SCRAP && Online)
        {
            AttachedBuilding?.ActivateScrapIcon();
        }
    }

    public override void OnHoverOff()
    {
        if(AttachedBuilding != null)
        {
            AttachedBuilding.DeactivateArrow();
            AttachedBuilding.DeactivateScrapIcon();
        }
       
    }

}
