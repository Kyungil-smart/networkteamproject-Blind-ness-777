using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SayHello", story: "[Self] do hello with [Other] while [WaitTime]", category: "Action", id: "17194cc2eef2e357fbdc78ba98990b99")]
public partial class SayHelloAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Other;
    [SerializeReference] public BlackboardVariable<float> WaitTime;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

