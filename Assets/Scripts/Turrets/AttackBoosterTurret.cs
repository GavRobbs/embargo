using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBoosterTurret : SupportTurret
{
    override public string TurretClass => "Attack Booster";

    override public int Level => _level;

    int _level = 1;

    override public float Influence => (float)_level * 1.0f;
    public override void OnTurretSpawn()
    {
        Collider[] colliders = Physics.OverlapSphere(influence_center.position, Influence, LayerMask.GetMask("Turret"));
        foreach(var collider in colliders)
        {
            var offensive_turret = GetComponentInParent<OffensiveTurret>();
            if(offensive_turret == null)
            {
                continue;
            }

            //If we add this as a child component, it automatically gets deleted when the GameObject for the turret gets deleted,
            //and this in turn propagates the necessary changes through the AttackBoostBonus OnDestroy method

            AttackBoostBonus ab = gameObject.AddComponent<AttackBoostBonus>();
            ab.Producer = this;
            ab.Beneficiary = offensive_turret;
        }
    }

    public override void OnTurretDestroy()
    {
    }
}
