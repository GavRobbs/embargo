using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NanoReparator : SupportTurret
{
    override public string TurretClass => "Nano Reparator";

    public override float BuildTime => 8.0f;
    public override float Influence => 1.5f + (float)Level * 0.4f;
    public float HealInterval => 50.0f * (1.0f - ((float)Level - 1) * 0.1f);

    float healCounter = 0.0f;

    List<Building> covered_buildings = new List<Building>();
    public override void OnTurretSpawn()
    {

        Online = true;
        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<SupportTurret>(MessageConstants.RegisterSupportTurretMessage, this));
        Collider[] colliders = Physics.OverlapSphere(influence_center.position, Influence, LayerMask.GetMask("Building"));
        foreach (var collider in colliders)
        {
            var covered_building = collider.GetComponentInParent<Building>();
            if (covered_building == null)
            {
                continue;
            }

            //If we add this as a child component, it automatically gets deleted when the GameObject for the turret gets deleted,
            //and this in turn propagates the necessary changes through the AttackBoostBonus OnDestroy method

            covered_buildings.Add(covered_building);
        }

        healCounter = HealInterval;
    }

    public override void OnTurretDestroy()
    {
        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<SupportTurret>(MessageConstants.UnregisterSupportTurretMessage, this));
    }

    public override void OnTurretUpgrade()
    {
        Collider[] colliders = Physics.OverlapSphere(influence_center.position, Influence, LayerMask.GetMask("Building"));
        covered_buildings.Clear();
        foreach (var collider in colliders)
        {
            var covered_building = collider.GetComponentInParent<Building>();
            if (covered_building == null)
            {
                continue;
            }

            //If we add this as a child component, it automatically gets deleted when the GameObject for the turret gets deleted,
            //and this in turn propagates the necessary changes through the AttackBoostBonus OnDestroy method

            covered_buildings.Add(covered_building);
        }

    }

    void Update()
    {
        healCounter -= Time.deltaTime;

        if(healCounter <= 0.0f)
        {
            healCounter = HealInterval;
            foreach(var b in covered_buildings)
            {
                if(b != null)
                {
                    b.IncreaseHP();
                }
            }
        }

    }

    public override void Start()
    {
        base.Start();
    }
}
