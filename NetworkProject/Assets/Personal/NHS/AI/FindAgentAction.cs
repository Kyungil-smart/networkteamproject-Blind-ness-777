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
    [SerializeReference] public BlackboardVariable<float>      Radius;
    [SerializeReference] public BlackboardVariable<bool>       CanMove;
    [SerializeReference] public BlackboardVariable<float>      RestTime;

    private float _restTimer;
    private BehaviorGraphAgent _targetBehavior;

    protected override Status OnStart()
    {
        if (Self.Value == null) 
            return Status.Failure;

        _restTimer = 0;
        _targetBehavior = null;
        Other.Value = null;

        CanMove.Value = false;

        PerformScan();

        return Status.Running;
    }

    private void PerformScan()
    {
        Collider[] colliders = Physics.OverlapSphere(Self.Value.transform.position, Radius.Value, 1 << 7);

        foreach (var col in colliders)
        {
            var otherAgent = col.GetComponentInParent<BehaviorGraphAgent>();
            if (otherAgent == null || otherAgent.gameObject == Self.Value) continue;

            if (otherAgent.BlackboardReference.GetVariableValue("CanMove", out bool otherCanMove))
            {
                if (otherCanMove == false)
                {
                    _targetBehavior = otherAgent;
                    Other.Value = _targetBehavior.gameObject;

                    SetGreetingAnimation(Self.Value, true);
                    FaceTarget(Self.Value, Other.Value);

                    _restTimer = 0;

                    _targetBehavior.BlackboardReference.SetVariableValue("RestTime", RestTime.Value);
                    _targetBehavior.BlackboardReference.SetVariableValue("CanMove", false);

                    SetGreetingAnimation(Other.Value, true);
                    FaceTarget(Other.Value, Self.Value);

                    Debug.Log($"[FindAgent] {Other.Value.name}을 만나 대기 시간을 리셋합니다. 처음부터 다시 인사!");
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
            SetGreetingAnimation(Self.Value, false);
            if (Other.Value != null) { SetGreetingAnimation(Other.Value, false); }

            CanMove.Value = true;
            Other.Value = null;

            Debug.Log("[FindAgent] 휴식 종료. 다시 이동합니다.");
            return Status.Success;
        }


        return Status.Running;
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
    }
}