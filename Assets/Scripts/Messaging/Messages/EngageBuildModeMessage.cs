using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageBuildModeMessage : GameMessage
{
    public GameObject turret_prefab;
    public EngageBuildModeMessage(GameObject pfb) : base(MessageConstants.EngageBuildMode)
    {
        turret_prefab = pfb;
    }
}
