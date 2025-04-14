using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line Of Sight Check", story: "Check [Target] With Line Of [Sight]", category: "Conditions", id: "8015ec968aaa388679ceb5f7f20548f4")]
public partial class LineOfSightCheckCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<LineOfSightDetector> Sight;


    public override bool IsTrue()
    {
        bool canSee = Sight.Value.CanSeeTarget(Target.Value);
        Debug.Log("Can see target: " + canSee);
        return canSee;
    }
}
