﻿using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Systems {
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UpdateSpeedSystem))]
    public class UpdateApproachSpeedSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var blobRef = GetSingleton<RoadSetupComponent>().Splines;
            var occupation = Intersections.Occupied; 
            return Entities.ForEach((Entity entity, ref CarSpeedComponent speed, in OnSplineComponent onSpline) =>
            {
                float approachSpeed = 1f;
                if (onSpline.Value.InIntersection)
                    approachSpeed = .7f;
                else
                {
                    var queue = TrackSplines.GetQueue(onSpline.Value);

                    if (queue[0].Entity != entity)
                    {
                        // someone's ahead of us - don't clip through them
                        int index = -1;
                        for (int i = 0; i < queue.Count; i++)
                        {
                            if (queue[i].Entity == entity)
                            {
                                index = i;
                                break;
                            }
                        }
                        float queueSize = blobRef.Value.Splines[onSpline.Value.Spline].CarQueueSize;
                        float maxT = queue[index - 1].SplineTimer - queueSize;
                        speed.SplineTimer = math.min(speed.SplineTimer, maxT);
                        approachSpeed = (maxT - speed.SplineTimer) * 5f;
                    }
                    else
                    {
                        // we're "first in line" in our lane, but we still might need
                        // to slow down if our next intersection is occupied
                        var s = onSpline.Value.Spline;
                        ref var spl = ref blobRef.Value.Splines[s];
                        var target = onSpline.Value.Direction == 1 ? spl.EndIntersection : spl.StartIntersection;
                        if (occupation[target][(onSpline.Value.Side + 1) / 2])
                            approachSpeed = (1f - speed.SplineTimer) * .8f + .2f;
                    }
                }
                speed.NormalizedSpeed = math.min(speed.NormalizedSpeed, approachSpeed);
            }).WithoutBurst().WithName("UpdateApproachSpeed").Schedule(inputDeps);
        }
    }
}
