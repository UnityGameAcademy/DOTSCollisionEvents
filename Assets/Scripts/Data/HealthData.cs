using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct HealthData : IComponentData
{
    public int currentValue;
    public bool isDead;
    
}
