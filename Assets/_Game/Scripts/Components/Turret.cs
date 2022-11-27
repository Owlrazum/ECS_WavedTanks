using Unity.Entities;

// An empty component is called a "tag component".
struct Turret : IComponentData
{
    public Entity CannonBallPrefab;
    public Entity CannonBallSpawnPoint;
}