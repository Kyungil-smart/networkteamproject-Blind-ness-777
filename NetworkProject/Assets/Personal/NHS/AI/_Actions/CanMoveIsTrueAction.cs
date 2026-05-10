using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CanMoveIsTrue", story: "Check [CanMove] Is True", category: "Condition", id: "da97b0f776f6ae0280e5cc38d139cf3e")]
public partial class CanMoveIsTrueAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> CanMove;

    protected override Status OnStart()
    {
        if (CanMove.Value == true) return Status.Success;
        else                       return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }
}