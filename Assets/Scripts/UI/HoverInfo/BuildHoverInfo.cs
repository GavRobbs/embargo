using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildHoverInfo : HoverInfo
{
    public GameObject turretPrefab;

    public BuildHoverInfo(GameObject turretPrefab) : base(GameInputManager.HOVER_MODE.BUILD, 0)
    {
        this.turretPrefab = turretPrefab;
    }

}
