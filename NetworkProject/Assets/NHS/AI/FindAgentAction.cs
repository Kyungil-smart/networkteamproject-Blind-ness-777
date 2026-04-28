using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindAgent", story: "Find other [Agent] [around] [Self]", category: "Action", id: "1537f8aa0bca68973cf0c0b81b271268")]
public partial class FindAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Around;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    public string TalkParam = "IsTalking";

    protected override Status OnStart()
    {
        if (Self.Value == null) 
            return Status.Failure;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Vector3 currentPos = Self.Value.transform.position;
        Animator animator = Self.Value.GetComponentInChildren<Animator>();

        Collider[] hitColliders = Physics.OverlapSphere(currentPos, Around.Value);

        foreach(var hitCollider in hitColliders)
        {
            if(hitCollider.gameObject != Self.Value && hitCollider.GetComponentInChildren<Animator>())
            {
                Vector3 targetPos = hitCollider.transform.position;
                Self.Value.transform.LookAt(new Vector3(targetPos.x, currentPos.y, targetPos.z));

                if (animator != null)
                {
                    animator.SetBool(TalkParam, true);
                }

                return Status.Success;
            }
        }

        if (animator != null)
        {
            animator.SetBool(TalkParam, false);
        }

        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}

