using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitForRandomSeconds", story: "Wait for [RandomSeconds]", category: "Action", id: "93942c21d733aebd9e819a4cf52c4c6d")]
public partial class WaitForRandomSecondsAction : Action
{
    [SerializeReference] public BlackboardVariable<float> RandomSeconds;
    [CreateProperty] private float m_Timer = 0.0f;

    protected override Status OnStart()
    {
        m_Timer = RandomSeconds.Value + UnityEngine.Random.Range(-3.0f, 3.0f);

        if (m_Timer <= 0.0f)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_Timer -= Time.deltaTime;

        if (m_Timer <= 0)
        {
            return Status.Success;
        }

        return Status.Running;
    }
}