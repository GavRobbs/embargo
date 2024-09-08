using UnityEngine;

public class BuildHoverInfo : HoverInfo
{
    public GameObject turretPrefab;

    public BuildHoverInfo(GameObject turretPrefab) : base(GameInputManager.HoverMode.BUILD, 0)
    {
        this.turretPrefab = turretPrefab;
    }

}
