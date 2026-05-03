using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindAgent", story: "[Self] find [other] Agent around [Radius] set [CanMove] while [RestTime]", category: "Action", id: "6a822467a8b78bb9abd7185509c99c13")]
public partial class FindAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Other;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<bool> CanMove;
    [SerializeReference] public BlackboardVariable<float> RestTime;

    private float _restTimer;

    protected override Status OnStart()
    {
        if (Self.Value == null) 
            return Status.Failure;

        _restTimer = 0;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if(_restTimer >= RestTime.Value)
        {
            CanMove.Value = true;
              Other.Value = null;
            return Status.Success;
        }

        _restTimer += Time.deltaTime;

        Collider[] colliders = Physics.OverlapSphere(Self.Value.transform.position, Radius.Value, 1 << 7);

        GameObject foundTarget = null;
        foreach(var col in colliders)
        {
            if (col.gameObject == Self.Value) continue;

            BehaviorGraphAgent otherBehavior = col.gameObject.GetComponent<BehaviorGraphAgent>();

            if (otherBehavior != null && otherBehavior.BlackboardReference != null)
            {
                if (otherBehavior.BlackboardReference.GetVariableValue("CanMove", out bool otherCanMove))
                {
                    if (otherCanMove == false)
                    {
                        foundTarget = col.gameObject;
                        Debug.Log("타겟을 찾았습니다!");

                        break;
                    }
                }
            }
        }

        // 다른 Agent를 발견했을 때
        if(foundTarget != null)
        {
              Other.Value = foundTarget;
            CanMove.Value = false;
            Debug.Log("다른 에이전트를 찾았습니다.");
            return Status.Running;

        }

        else
        {
            Debug.Log("다른 에이전트를 찾지못했습니다.");

        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        Debug.Log("Agent를 찾지 못했습니다. 이동을 시작합니다.");
    }
}