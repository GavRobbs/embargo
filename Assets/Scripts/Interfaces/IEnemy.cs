using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy : ITargetable, IPathFollower
{
    void Attack(ITurret turret);
    
}
