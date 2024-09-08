using UnityEngine;

public interface ITargetable {
    //This isn't usually the direct world origin of the object, but a point somewhere in its forward direction
    //to help the turrets track moving objects properly
    Vector3 Position { get; }
    bool IsFriendly { get; }
    bool IsKilled { get; }
    float HitPoints { get; }

    void Damage(float value);
}