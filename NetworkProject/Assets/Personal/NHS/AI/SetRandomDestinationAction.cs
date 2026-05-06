using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetRandomDestination", story: "Set [Destination] to random point within [Radius] around [MapPoint]", category: "Action", id: "0844c20085f827fd259add6dea1ef75d")]
public partial class SetRandomDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Destination;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<GameObject> MapPoint;

    protected override Status OnStart()
    {
        Vector3 centerPos = (MapPoint.Value != null) ? MapPoint.Value.transform.position : Vector3.zero;

        float halfSize = Radius.Value * 0.5f;

        float randomX = UnityEngine.Random.Range(-halfSize, halfSize);
        float randomZ = UnityEngine.Random.Range(-halfSize, halfSize);

        Vector3 finalTargetPos = centerPos + new Vector3(randomX, 0, randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(finalTargetPos, out hit, 2.0f, NavMesh.AllAreas))
        {
            Destination.Value = hit.position;
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

