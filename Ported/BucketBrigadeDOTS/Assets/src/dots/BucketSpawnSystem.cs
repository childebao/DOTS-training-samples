﻿using System.ComponentModel.Design.Serialization;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class BucketSpawnSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithStructuralChanges()
            .ForEach((Entity entity, in WaterBucketSpawner spawner, in LocalToWorld ltw) =>
            {
                for (int i = 0; i < spawner.Count; ++i)
                {
                    var instance = EntityManager.Instantiate(spawner.Prefab);
                    SetComponent<Translation2D>(instance, new Translation2D { Value = 0 });
                    EntityManager.AddComponent<WaterBucketTag>(instance);
                }

                EntityManager.DestroyEntity(entity);
            }).Run();
    }
}
