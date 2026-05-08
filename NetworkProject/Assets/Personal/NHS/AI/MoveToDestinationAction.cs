using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

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
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || _navAgent == null) 
            return Status.Failure;

        if (!_navAgent.pathPending && _navAgent.remainingDistance <= distanceThreshold) return Status.Success;

        /// 기존 이동 로직
        /*
        Vector3 currentPos = Agent.Value.transform.position;
        Vector3  targetPos = Location.Value;
        
        float distance = Vector3.Distance(currentPos, targetPos);
        
        if (distance <= distanceThreshold)
        {
            return Status.Success;
        }
        
        Agent.Value.transform.position = Vector3.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);
        
        Vector3 direction = (targetPos - currentPos).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            Agent.Value.transform.rotation = Quaternion.RotateTowards(Agent.Value.transform.rotation, targetRotation, 720f * Time.deltaTime);
        }
        */

        if (_animator != null)
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

        if (_animator != null)
            _animator.SetFloat(SpeedParameter, 0f);
    }
}