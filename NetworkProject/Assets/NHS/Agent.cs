using UnityEngine;

public class Agent : MonoBehaviour
{
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
    }

    void SetDie()
    {
        //gameObject.
    }
}