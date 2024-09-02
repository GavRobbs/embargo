using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneBuildMessage : GameMessage
{
    public Building building;
    public DroneBuildMessage(Building b) : base(MessageConstants.CreateTurret)
    {
        building = b;
    }
}
