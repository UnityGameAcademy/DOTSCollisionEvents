using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateBefore(typeof(TransformSystemGroup))]
public class RemoveOnDeathSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = commandBufferSystem.CreateCommandBuffer();

        Entities.
            WithAny<PlayerTag,ChaserTag>().
            ForEach((Entity entity, in HealthData healthData) =>
            {
                if (healthData.isDead)
                {
                    entityCommandBuffer.DestroyEntity(entity);
                }

            }).Schedule();

        commandBufferSystem.AddJobHandleForProducer(this.Dependency);
    }
}