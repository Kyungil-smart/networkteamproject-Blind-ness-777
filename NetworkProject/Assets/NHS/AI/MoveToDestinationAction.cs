using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToDestination", story: "[Agent] navigates to [Location]", category: "Action", id: "c487a527e4b7936041a36a189c086be5")]
public partial class MoveToDestinationAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Location;

    public float Speed = 3.5f;
    public float DistanceThreshold = 0.2f;

    private Animator _animator;

    public string     SpeedParameter = "Speed";
    public string IsTalkingParameter = "IsTalking";

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            Debug.Log("움직일 객체가 없습니다.");
            return Status.Failure;
        }

        if(_animator == null)
        {
            _animator = Agent.Value.GetComponentInChildren<Animator>();
        }

        if (_animator != null)
        {
            _animator.SetBool(IsTalkingParameter, false); // 이동 시작 시 인사 중단
        }

        //Debug.Log($"도착지 {Location.Value}");

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

        Agent.Value.transform.LookAt(new Vector3(targetPos.x, currentPos.y, targetPos.z));
        
        //Debug.Log("이동중");

        if (_animator != null)
        {
            _animator.SetFloat(SpeedParameter, Speed);
            _animator.SetBool(IsTalkingParameter, false);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_animator != null)
        {
            _animator.SetFloat(SpeedParameter, 0f);
        }
    }
}

