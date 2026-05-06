using Unity.Behavior;
using UnityEngine;

public class Agent : MonoBehaviour
{
    private Animator _animator;
    private BehaviorGraphAgent _behaviorAgent;


    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

        _behaviorAgent = GetComponentInParent<BehaviorGraphAgent>();
    }

    void Start()
    {
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (_behaviorAgent != null && _behaviorAgent.BlackboardReference != null)
            {
                _behaviorAgent.BlackboardReference.SetVariableValue("CanMove", true);
                Debug.Log("Value 바뀜");
            }
        }
    }
}