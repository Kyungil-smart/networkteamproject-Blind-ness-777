using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TMPro;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToDestination", story: "[Agent] navigates to [Location] and make [CanMove] false", category: "Action", id: "c487a527e4b7936041a36a189c086be5")]
public partial class MoveToDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<bool> CanMove;
    public float Speed = 3.5f;
    public float DistanceThreshold = 2.0f;

    private Animator _animator;

    public string     SpeedParameter = "Speed";

    protected override Status OnStart()
    {
        if (Agent.Value == null) return Status.Failure;

        if (_animator == null)
            _animator = Agent.Value.GetComponentInChildren<Animator>();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null) 
            return Status.Failure;

        Vector3 currentPos = Agent.Value.transform.position;
        Vector3  targetPos = Location.Value;

        float distance = Vector3.Distance(currentPos, targetPos);

        if (distance <= DistanceThreshold)
        {
            return Status.Success;
        }

        Agent.Value.transform.position = Vector3.MoveTowards(currentPos, targetPos, Speed * Time.deltaTime);

        Vector3 direction = (targetPos - currentPos).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Agent.Value.transform.rotation = Quaternion.RotateTowards(Agent.Value.transform.rotation, targetRotation, 720f * Time.deltaTime);
        }

        if (_animator != null)
        {
            _animator.SetFloat(SpeedParameter, Speed);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        CanMove.Value = false;

        if (_animator != null)
        {
            _animator.SetFloat(SpeedParameter, 0f);
        }
    }
}