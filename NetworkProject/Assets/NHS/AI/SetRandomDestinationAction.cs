using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetRandomDestination", story: "Set [Destination] to random point within [Radius] around [Self]", category: "Action", id: "0844c20085f827fd259add6dea1ef75d")]
public partial class SetRandomDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Destination;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * Radius.Value;
        randomDirection += Self.Value.transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, Radius.Value, NavMesh.AllAreas))
        {
            Destination.Value = hit.position;
            Debug.Log($"Destination.Value {Destination.Value}");
            return Status.Success;
        }

        return Status.Failure;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

