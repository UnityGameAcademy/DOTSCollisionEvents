using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class DeathOnCollisionSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }

    [BurstCompile]
    struct DeathOnCollisionSystemJob : ICollisionEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<DeathColliderTag> deathColliderGroup;
        [ReadOnly] public ComponentDataFromEntity<ChaserTag> chaserGroup;

        public ComponentDataFromEntity<HealthData> healthGroup;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.Entities.EntityA;
            Entity entityB = collisionEvent.Entities.EntityB;

            bool entityAIsChaser = chaserGroup.Exists(entityA);
            bool entityAIsDeathCollider = deathColliderGroup.Exists(entityA);
            bool entityBIsChaser = chaserGroup.Exists(entityB);
            bool entityBIsDeathCollider = deathColliderGroup.Exists(entityB);

            if (entityAIsDeathCollider && entityBIsChaser)
            {
                HealthData modifiedHealth = healthGroup[entityB];
                modifiedHealth.isDead = true;
                healthGroup[entityB] = modifiedHealth;
            }

            if (entityAIsChaser && entityBIsDeathCollider)
            {
                HealthData modifiedHealth = healthGroup[entityA];
                modifiedHealth.isDead = true;
                healthGroup[entityA] = modifiedHealth;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new DeathOnCollisionSystemJob();
        job.deathColliderGroup = GetComponentDataFromEntity<DeathColliderTag>(true);
        job.chaserGroup = GetComponentDataFromEntity<ChaserTag>(true);
        job.healthGroup = GetComponentDataFromEntity<HealthData>(false);

        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld,
            inputDependencies);
        jobHandle.Complete();

        return jobHandle;
    }
}