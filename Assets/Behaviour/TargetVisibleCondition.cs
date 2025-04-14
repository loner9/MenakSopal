using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Target visible", story: "Detector succesfully detect Enemy", category: "Conditions", id: "29d381c1b0db118869e7358cd156ed3c")]
public partial class TargetVisibleCondition : Condition
{

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
