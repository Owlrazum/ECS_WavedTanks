using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

// Instead of directly accessing the Turret component, we are creating an aspect.
// Aspects allows you to provide a customized API for accessing your components.
readonly partial struct TurretAspect : IAspect
{
    // This reference provides read only access to the Turret component.
    // Trying to use ValueRW (instead of ValueRO) on a read-only reference is an error.
    readonly RefRO<Turret> _turret;
    readonly RefRO<URPMaterialPropertyBaseColor> m_BaseColor;
 

    // Note the use of ValueRO in the following properties.
    public Entity CannonBallSpawnPoint => _turret.ValueRO.CannonBallSpawnPoint;
    public Entity CannonBallPrefab => _turret.ValueRO.CannonBallPrefab;
    public float4 Color => m_BaseColor.ValueRO.Value;
}