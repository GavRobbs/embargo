using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTurretButton : MonoBehaviour
{
    [SerializeField]
    GameObject attached_turret_prefab;

    [SerializeField]
    CommandConsoleUI owner;

    public void OnClick()
    {
        owner.RequestTurretBuild(attached_turret_prefab);
    }
}
