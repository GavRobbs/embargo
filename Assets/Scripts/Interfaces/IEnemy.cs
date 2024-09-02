using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy : ITargetable, IPathFollower, IHoverable
{
    void Attack(ITurret turret);
    string Name { get; }
    Spawner Spawner { get; set; }
    
}
