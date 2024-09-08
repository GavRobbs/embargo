﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy : ITargetable, IPathFollower, IHoverable, IStoppable
{
    void Attack(Building building);
    string Name { get; }
    ISpawner Spawner { get; set; }

    int CapitolDamage { get; }

    void KillMe();
    
}
