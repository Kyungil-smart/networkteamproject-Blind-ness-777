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

    private Animator m_Animator;
    public string     SpeedParameter = "Speed";
    public string IsTalkingParameter = "IsTalking";

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            Debug.Log("움직일 객체가 없습니다.");
            return Status.Failure;
        }

        m_Animator = Agent.Value.GetComponentInChildren<Animator>();

        Debug.Log($"도착지 {Location.Value}");

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

        if (m_Animator != null)
        {
            // 실제 이동 속도(Speed)를 파라미터로 전달합니다. 
            // 애니메이터에서는 이 값이 0보다 크면 걷기/뛰기 애니메이션으로 전환됩니다.
            m_Animator.SetFloat(SpeedParameter, Speed);
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (m_Animator != null)
        {
            m_Animator.SetFloat(SpeedParameter, 0f); // 속도를 0으로 설정
        }
    }
}

