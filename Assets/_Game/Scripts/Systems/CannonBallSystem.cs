using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
// IJobEntity relies on source generation to implicitly define a query from the signature of the Execute function.
partial struct CannonBallJob : IJobEntity
{
    const float SpeedMultiplier = 0.2f; // 0.8f was in tutorial
    // A regular EntityCommandBuffer cannot be used in parallel, a ParallelWriter has to be explicitly used.
    public EntityCommandBuffer.ParallelWriter ECB;
    // Time cannot be directly accessed from a job, so DeltaTime has to be passed in as a parameter.
    public float DeltaTime;

    // The ChunkIndexInQuery attributes maps the chunk index to an int parameter.
    // Each chunk can only be processed by a single thread, so those indices are unique to each thread.
    // They are also fully deterministic, regardless of the amounts of parallel processing happening.
    // So those indices are used as a sorting key when recording commands in the EntityCommandBuffer,
    // this way we ensure that the playback of commands is always deterministic.
    void Execute([ChunkIndexInQuery] int chunkIndex, ref CannonBallAspect cannonBall)
    {
        float3 gravity = new float3(0.0f, -2f, 0.0f);
        float3 invertY = new float3(1.0f, -1.0f, 1.0f);

        cannonBall.Position += cannonBall.Velocity * DeltaTime;
        if (cannonBall.Position.y < 0.0f)
        {
            cannonBall.Position *= invertY;
            cannonBall.Velocity *= invertY * SpeedMultiplier;
        }

        cannonBall.Velocity += gravity * DeltaTime;

        float speed = math.lengthsq(cannonBall.Velocity);
        if (speed < 0.1f)
        { 
            ECB.DestroyEntity(chunkIndex, cannonBall.Self);
        }
    }
}

[BurstCompile]
partial struct CannonBallSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var cannonBallJob = new CannonBallJob
        {
            // Note the function call required to get a parallel writer for an EntityCommandBuffer.
            ECB = ecb.AsParallelWriter(),
            // Time cannot be directly accessed from a job, so DeltaTime has to be passed in as a parameter.
            DeltaTime = SystemAPI.Time.DeltaTime
        };
        cannonBallJob.ScheduleParallel();
    }
}