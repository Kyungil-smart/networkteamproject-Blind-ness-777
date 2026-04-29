using UnityEngine;

public class Agent : MonoBehaviour
{
    private Animator _animator;
    private Agent _targetAgent;

    public  float interactionDuration = 3.0f;
    private float _interactionTimer;
    public bool IsGreeting => _targetAgent != null;

    public float greetingCooltime = 10.0f;
    private float  _cooltimeTimer = 0f;

    public bool IsOnCooltime => _cooltimeTimer > 0f;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 1. 인사 로직
        if (IsGreeting)
        {
            LookAtTarget(_targetAgent.transform.position);
            _interactionTimer += Time.deltaTime;

            if (_interactionTimer >= interactionDuration)
            {
                StopGreeting();
            }
        }
        // 2. 쿨타임 로직 (인사 중이 아닐 때만 쿨타임이 깎임)
        else if (_cooltimeTimer > 0f)
        {
            _cooltimeTimer -= Time.deltaTime;
        }
    }

    public void StartGreeting(Agent other)
    {
        if (IsOnCooltime || IsGreeting) return; // 쿨타임이거나 이미 인사 중이면 무시

        _targetAgent = other;
        _interactionTimer = 0f;

        if (_animator != null)
            _animator.SetBool("IsTalking", true);
    }

    public void StopGreeting()
    {
        _targetAgent = null;
        _interactionTimer = 0f;
        _cooltimeTimer = greetingCooltime; // 인사가 끝나는 순간 쿨타임 시작

        if (_animator != null)
            _animator.SetBool("IsTalking", false);
    }

    private void LookAtTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public float GetSpeed()
    {
        return _animator != null ? _animator.GetFloat("Speed") : 0f;
    }
}