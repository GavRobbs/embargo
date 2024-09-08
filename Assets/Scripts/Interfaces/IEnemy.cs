public interface IEnemy : ITargetable, IPathFollower, IHoverable, IStoppable {
    void Attack(Building building);
    string Name { get; }
    ISpawner Spawner { get; set; }

    int CapitolDamage { get; }

    void Destroy();
}