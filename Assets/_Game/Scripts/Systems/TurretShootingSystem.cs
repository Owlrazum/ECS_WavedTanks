using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
internal partial struct TurretShootingSystem : ISystem
{
    // A ComponentLookup provides random access to a component (looking up an entity).
    // We'll use it to extract the world space position and orientation of the spawn point (cannon nozzle).
    private ComponentLookup<LocalToWorldTransform> _localToWorldTransformFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // ComponentLookup structures have to be initialized once.
        // The parameter specifies if the lookups will be read only or if they should allow writes.
        _localToWorldTransformFromEntity = state.GetComponentLookup<LocalToWorldTransform>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // ComponentLookup structures have to be updated every frame.
        _localToWorldTransformFromEntity.Update(ref state);

        // Creating an EntityCommandBuffer to defer the structural changes required by instantiation.
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Creating an instance of the job.
        // Passing it the ComponentLookup required to get the world transform of the spawn point.
        // And the entity command buffer the job can write to.
        var turretShootJob = new TurretShoot
        {
            LocalToWorldTransformFromEntity = _localToWorldTransformFromEntity,
            ECB = ecb
        };

        // Schedule execution in a single thread, and do not block main thread.
        turretShootJob.Schedule();
    }
}

// Requiring the Shooting tag component effectively prevents this job from running
// for the tanks which are in the safe zone.
[WithAll(typeof(Shooting))]
[BurstCompile]
partial struct TurretShoot : IJobEntity
{
    [ReadOnly] public ComponentLookup<LocalToWorldTransform> LocalToWorldTransformFromEntity;
    public EntityCommandBuffer ECB;

    const float InitialSpeed = 5.0f;

    // Note that the TurretAspects parameter is "in", which declares it as read only.
    // Making it "ref" (read-write) would not make a difference in this case, but you
    // will encounter situations where potential race conditions trigger the safety system.
    // So in general, using "in" everywhere possible is a good principle.
    private void Execute(in TurretAspect turret)
    {
        Entity instance = ECB.Instantiate(turret.CannonBallPrefab);
        LocalToWorldTransform spawnLocalToWorld = LocalToWorldTransformFromEntity[turret.CannonBallSpawnPoint];
        UniformScaleTransform cannonBallTransform = UniformScaleTransform.FromPosition(spawnLocalToWorld.Value.Position);

        // We are about to overwrite the transform of the new instance. If we didn't explicitly
        // copy the scale it would get reset to 1 and we'd have oversized cannon balls.
        cannonBallTransform.Scale = LocalToWorldTransformFromEntity[turret.CannonBallPrefab].Value.Scale;
        ECB.SetComponent(instance, new LocalToWorldTransform
        {
            Value = cannonBallTransform
        });
        ECB.SetComponent(instance, new CannonBall
        {
            Velocity = spawnLocalToWorld.Value.Forward() * InitialSpeed
        });

        ECB.SetComponent(instance, new URPMaterialPropertyBaseColor { Value = turret.Color });
    }
}