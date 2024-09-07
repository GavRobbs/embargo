using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBoosterTurret : SupportTurret
{
    override public string TurretClass => "Attack Booster";

    public override float BuildTime => 8.0f;
    public override float Influence => 1.2f + (float)Level * 0.3f;
    public override void OnTurretSpawn()
    {

        Online = true;
        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<SupportTurret>(MessageConstants.RegisterSupportTurretMessage, this));
        Collider[] colliders = Physics.OverlapSphere(influence_center.position, Influence, LayerMask.GetMask("Turret"));
        foreach(var collider in colliders)
        {
            var offensive_turret = collider.GetComponentInParent<OffensiveTurret>();
            if(offensive_turret == null)
            {
                continue;
            }

            //If we add this as a child component, it automatically gets deleted when the GameObject for the turret gets deleted,
            //and this in turn propagates the necessary changes through the AttackBoostBonus OnDestroy method

            AttackBoostBonus ab = offensive_turret.gameObject.AddComponent<AttackBoostBonus>();
            ab.Producer = this;
            ab.Beneficiary = offensive_turret;
        }
    }

    public override void OnTurretDestroy()
    {
        MessageDispatcher.GetInstance().Dispatch(new SingleValueMessage<SupportTurret>(MessageConstants.UnregisterSupportTurretMessage, this));
    }

    public override void BestowBonus(ITurret turret)
    {
        if(turret is OffensiveTurret ot)
        {
            AttackBoostBonus ab = gameObject.AddComponent<AttackBoostBonus>();
            ab.Producer = this;
            ab.Beneficiary = ot;
        }

    }
}
