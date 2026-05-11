using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerAnimator : NetworkBehaviour, IPhaseChangeable
{
    [Header("Animator")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float    _walkAnimSpeed  = 0.5f;
    [SerializeField] private float    _runAnimSpeed   = 1f;

    [Header("Gun")]
    [SerializeField] private GameObject _gunObject;

    [Header("Timing")]
    [SerializeField] private float _drawGunDelay = 1f; // 탑뷰 전환 후 총 꺼내기까지 대기 시간

    private static readonly int _hashSpeed   = Animator.StringToHash("Speed");
    private static readonly int _hashDrawGun = Animator.StringToHash("DrawGun");
    private static readonly int _hashShoot   = Animator.StringToHash("Shoot");

    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public override void OnNetworkSpawn()
    {
        if (_gunObject != null) _gunObject.SetActive(false);
    }

    public void SetMoveSpeed(float speed)
    {
        if (_animator != null)
            _animator.SetFloat(_hashSpeed, speed);
    }

    public void PlayShoot()
    {
        if (_animator != null)
            _animator.SetTrigger(_hashShoot);
    }

    public void DisableAnimator()
    {
        if (_animator != null)
            _animator.enabled = false;
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Shooting:
                StartCoroutine(ShootingSequence());
                break;

            case GamePhase.GameOver:
                SetMoveSpeed(0f);
                break;
        }
    }

    private IEnumerator ShootingSequence()
    {
        if (_gunObject != null) _gunObject.SetActive(true);

        yield return new WaitForSeconds(_drawGunDelay);

        if (_animator != null) _animator.SetTrigger(_hashDrawGun);

        // DrawGun 애니메이션 길이만큼 대기 후 Shoot
        float drawGunLength = GetClipLength("DrawGun");
        yield return new WaitForSeconds(drawGunLength);

        if (_animator != null) _animator.SetTrigger(_hashShoot);
        
        if (_playerController != null && _playerController.IsOwner)
            _playerController.ShootServerRpc(
                _playerController.GetComponent<PlayerAim>().AimDirection,
                _playerController.FireOrigin.position);

        // Shoot 애니메이션 길이만큼 대기 후 총 비활성화 + Idle 복귀
        float shootLength = GetClipLength("Shoot");
        yield return new WaitForSeconds(shootLength);

        if (_gunObject != null) _gunObject.SetActive(false);
        SetMoveSpeed(0f);
    }

    private float GetClipLength(string clipName)
    {
        if (_animator == null) return 0f;

        foreach (AnimationClip clip in _animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 1f; // 클립 못 찾으면 기본 1초
    }

    public float   WalkAnimSpeed => _walkAnimSpeed;
    public float   RunAnimSpeed  => _runAnimSpeed;
    public Animator Animator     => _animator;
}