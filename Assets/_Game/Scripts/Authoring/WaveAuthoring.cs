using Unity.Entities;

internal class WaveAuthoring : UnityEngine.MonoBehaviour
{
}

internal class WaveBaker : Baker<WaveAuthoring>
{
    public override void Bake(WaveAuthoring authoring)
    {
        AddComponent<Wave>();
    }
}