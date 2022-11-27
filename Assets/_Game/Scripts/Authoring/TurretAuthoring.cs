using Unity.Entities;

// Authoring MonoBehaviours are regular GameObject components.
// They constitute the inputs for the baking systems which generates ECS data.
internal class TurretAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject CannonBallPrefab;
    public UnityEngine.Transform CannonBallSpawnPoint;

}

// Bakers convert authoring MonoBehaviours into entities and components.
internal class TurretBaker : Baker<TurretAuthoring>
{
    public override void Bake(TurretAuthoring authoring)
    {
        AddComponent(new Turret
        {
            // By default, each authoring GameObject turns into an Entity.
            // Given a GameObject (or authoring component), GetEntity looks up the resulting Entity.
            CannonBallPrefab = GetEntity(authoring.CannonBallPrefab),
            CannonBallSpawnPoint = GetEntity(authoring.CannonBallSpawnPoint)
        });

        // Enableable components are always initially enabled.

        AddComponent<Shooting>();
    }
}