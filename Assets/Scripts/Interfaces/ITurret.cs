public interface ITurret : IHoverable, IStoppable
{
    string TurretClass { get; }
    int Level { get; set; }

    int Cost { get; }

    bool Online { get; set; }

    float BuildTime { get; }

    Building AttachedBuilding { get; set;}

    //For offensive turrets, this is basically the attack range
    //For support turrets, this is the range they affect
    float Influence { get; }

    //A special function to be called when the turret is spawned
    //This is especially useful for the boosters to do a collision check
    //for the nearby turrets, due to the fact that OnTriggerEnter will not
    //be triggered
    void OnTurretSpawn();

    //Same idea but for turret destruction
    void OnTurretDestroy();
}
