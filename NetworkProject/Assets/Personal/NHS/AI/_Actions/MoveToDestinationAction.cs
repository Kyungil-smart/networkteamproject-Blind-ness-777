using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using Unity.Netcode;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToDestination", story: "[Agent] navigates to [Location] and make [CanMove] false", category: "Action", id: "c487a527e4b7936041a36a189c086be5")]
public partial class MoveToDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;
    [SerializeReference] public BlackboardVariable<bool> CanMove;

    public float speed = 3.5f;
    public float distanceThreshold = 2.0f;

    private NavMeshAgent _navAgent;
    private Animator _animator;

    public string     SpeedParameter = "Speed";

    protected override Status OnStart()
    {
        if (Agent.Value == null) return Status.Failure;

        if (_animator == null)
            _animator = Agent.Value.GetComponentInChildren<Animator>();
            
        if (_navAgent == null)
            _navAgent = Agent.Value.GetComponent<NavMeshAgent>();

        if(_navAgent != null)
        {
            _navAgent.speed = speed;
            _navAgent.stoppingDistance = distanceThreshold;
            _navAgent.SetDestination(Location.Value);

            Vector3 direction = (Location.Value - Agent.Value.transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Agent.Value.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || _navAgent == null) 
            return Status.Failure;

        if (!_navAgent.pathPending && _navAgent.remainingDistance <= distanceThreshold) return Status.Success;

        if (_animator != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            _animator.SetFloat(SpeedParameter, _navAgent.velocity.magnitude);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_navAgent != null && _navAgent.isOnNavMesh)
            _navAgent.ResetPath(); // 이동 중지

        CanMove.Value = false;

        if (_animator != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            _animator.SetFloat(SpeedParameter, 0f);
    }
}