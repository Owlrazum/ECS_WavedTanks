using Unity.Burst;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

// Unmanaged systems based on ISystem can be Burst compiled, but this is not yet the default.
// So we have to explicitly opt into Burst compilation with the [BurstCompile] attribute.
// It has to be added on BOTH the struct AND the OnCreate/OnDestroy/OnUpdate functions to be
// effective.
[BurstCompile]
partial struct WaveSystem : ISystem
{
    const float Speed = 2;
    const float Offset = 5;
    const float Height = 5;

    bool m_IsStartPosAdjusted;

    // Every function defined by ISystem has to be implemented even if empty.
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    // TODO: understand why second update cycle is needed
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float time = (float)SystemAPI.Time.ElapsedTime;

        if (!m_IsStartPosAdjusted && time > 0.01f)
        {
            AdjustStartPosition(ref state);
            m_IsStartPosAdjusted = true;
        }


        time *= Speed;

        foreach ((TransformAspect transform, RefRW<Wave> waveRef, Entity entity) in SystemAPI.Query<TransformAspect, RefRW<Wave>>().WithEntityAccess())
        {
            time += entity.Index * Speed * Offset;
            float prevPeriod = waveRef.ValueRW.Period;
            float prevSin = math.sin(prevPeriod);
            float currentSin = math.sin(time);
            waveRef.ValueRW.Period = time;

            float3 delta = float3.zero;
            delta.y = (currentSin - prevSin) * Height;
            transform.Position += delta;
        }
    }

    private void AdjustStartPosition(ref SystemState state)
    {
        foreach (TransformAspect transform in SystemAPI.Query<TransformAspect>().WithAll<Wave>())
        {
            float3 delta = float3.zero;
            delta.y = Height;
            transform.Position += delta; // add Height, so tanks will not fall below zero. CannonBalls are not.
        }
    }
}
