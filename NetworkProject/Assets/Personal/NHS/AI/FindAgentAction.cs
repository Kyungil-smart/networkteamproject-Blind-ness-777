using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindAgent", story: "[Self] find [other] Agent around [Radius] set [CanMove] while [RestTime]", category: "Action", id: "6a822467a8b78bb9abd7185509c99c13")]
public partial class FindAgentAction : Action
{
    public static List<FindAgentAction> RestingActions = new List<FindAgentAction>();

    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Other;
    [SerializeReference] public BlackboardVariable<float>      Radius;
    [SerializeReference] public BlackboardVariable<bool>       CanMove;
    [SerializeReference] public BlackboardVariable<float>      RestTime;
    private float _restTimer;

    private BehaviorGraphAgent _myBehavior;

    protected override Status OnStart()
    {
        if (Self.Value == null) 
            return Status.Failure;

        _restTimer = 0;
        Other.Value = null;
        CanMove.Value = false;

        _myBehavior = Self.Value.GetComponent<BehaviorGraphAgent>();
        

        if (!RestingActions.Contains(this))
            RestingActions.Add(this);

        PerformScan();

        return Status.Running;
    }

    private void PerformScan()
    {
        float sqrRadius = Radius.Value * Radius.Value;
        Vector3 myPos = Self.Value.transform.position;

        for(int i=0;i<RestingActions.Count;i++)
        {
            var otherAction = RestingActions[i];

            if (otherAction == this || otherAction.Self.Value == null) continue;

            Agent targetAgent = otherAction.Self.Value.GetComponent<Agent>();

            if (targetAgent.isGreet == true) { Debug.Log("isGreet가 true 입니다."); }

            if (targetAgent == null || targetAgent.isGreet == true) return;

            Agent selfAgent = Self.Value.GetComponent<Agent>();

            float distSq = (otherAction.Self.Value.transform.position - myPos).sqrMagnitude;

            if(distSq <= sqrRadius)
            {
                Other.Value = otherAction.Self.Value;
                var otherBehavior = otherAction._myBehavior;

                if(otherBehavior != null)
                {
                    SetGreetingAnimation(Self.Value, true);
                    FaceTarget(Self.Value, Other.Value);
                    _restTimer = 0;

                    selfAgent.isGreet = true;

                    otherBehavior.BlackboardReference.SetVariableValue("RestTime", RestTime.Value);
                    otherBehavior.BlackboardReference.SetVariableValue("CanMove", false);
                    otherAction._restTimer = 0; // 상대방 타이머도 리셋 (동기화)
                    
                    targetAgent.isGreet = true;

                    SetGreetingAnimation(Other.Value, true);
                    FaceTarget(Other.Value, Self.Value);

                    return;
                }
            }
        }
    }

    protected override Status OnUpdate()
    {
        _restTimer += Time.deltaTime;

        if(_restTimer >= RestTime.Value)
        {
            Cleanup();

            CanMove.Value = true;

            SetGreetingAnimation(Self.Value, false);
            if (Other.Value != null) 
                SetGreetingAnimation(Other.Value, false);

            Other.Value = null;

            //Debug.Log("[FindAgent] 휴식 종료. 다시 이동합니다.");
            return Status.Success;
        }

        return Status.Running;
    }

    private void Cleanup()
    {
        if (RestingActions.Contains(this)) RestingActions.Remove(this);
    }

    private void SetGreetingAnimation(GameObject target, bool isGreeting)
    {
        if (target == null) 
            return;

        Animator anim = target.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsTalking", isGreeting);
        }
    }

    private void FaceTarget(GameObject simpleton, GameObject target)
    {
        if (simpleton == null || target == null) return;

        Vector3 direction = (target.transform.position - simpleton.transform.position).normalized;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            simpleton.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    protected override void OnEnd()
    {
        Cleanup();
    }
}