using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FindAgent", story: "Find [Other] [around] [Self]", category: "Action", id: "1537f8aa0bca68973cf0c0b81b271268")]
public partial class FindAgentAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Other;
    [SerializeReference] public BlackboardVariable<float> Around;
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    private Animator _animator;
    public string TalkParam = "IsTalking";

    protected override Status OnStart()
    {
        if (Self.Value == null) 
            return Status.Failure;

        if (_animator == null)
        {
            _animator = Self.Value.GetComponentInChildren<Animator>();
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Agent myAgent = Self.Value.GetComponentInChildren<Agent>();

        if (myAgent.IsGreeting) 
            return Status.Running;

        if (myAgent.IsOnCooltime) 
            return Status.Failure;

        Vector3 currentPos = Self.Value.transform.position;
        int layerMask = LayerMask.GetMask("Agent");
        Collider[] hitColliders = Physics.OverlapSphere(currentPos, Around.Value, layerMask);

        foreach (var hitCollider in hitColliders)
        {
            GameObject targetObj = hitCollider.transform.root.gameObject;
            if (targetObj == Self.Value.transform.root.gameObject) continue;

            Agent otherAgent = hitCollider.GetComponentInParent<Agent>();

            if (otherAgent != null && !otherAgent.IsOnCooltime && otherAgent.GetSpeed() < 0.1f)
            {
                if (Other != null) Other.Value = targetObj;

                myAgent.StartGreeting(otherAgent);
                otherAgent.StartGreeting(myAgent);

                return Status.Running;
            }
        }
        return Status.Failure;
    }

    protected override void OnEnd()
    {
    }
}