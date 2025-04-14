using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Detect Target", story: "[Detector] Detect [Target]", category: "Action", id: "338ab08e682e97b379729f0d10ecaa6c")]
public partial class DetectTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<FieldOfViewDetector> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnUpdate()
    {
        return Detector.Value.FindTargetInView() ? Status.Success : Status.Failure;
    }
}

