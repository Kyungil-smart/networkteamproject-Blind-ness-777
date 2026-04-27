using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour, IDamageable, IPhaseChangeable
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed     = 5f;
    [SerializeField] private float _sprintSpeed   = 10f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] private GameObject _virtualCamera;

    [Header("Raycast")]
    [SerializeField] private Transform _fireOrigin;
    [SerializeField] private float     _aimRayLength = 30f;
    [SerializeField] private LayerMask _shootLayerMask;

    [Header("Gun")]
    [SerializeField] private GameObject _gunObject;

    [Header("Animator")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float    _walkAnimSpeed = 0.5f;
    [SerializeField] private float    _runAnimSpeed  = 1f;

    private CharacterController _characterController;
    private PlayerAim           _playerAim;

    private bool _canShoot  = false;
    private bool _isDead    = false;

    private Vector2 _moveInput;
    private bool    _shootInput;
    private bool    _isSprinting;

    private static readonly int _hashSpeed   = Animator.StringToHash("Speed");
    private static readonly int _hashDrawGun = Animator.StringToHash("DrawGun");
    private static readonly int _hashShoot   = Animator.StringToHash("Shoot");

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerAim           = GetComponent<PlayerAim>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            _virtualCamera.SetActive(false);
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null) playerInput.enabled = false;
            return;
        }

        if (_gunObject != null) _gunObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner || _isDead) return;

        HandleMovement();

        if (_canShoot && _shootInput)
        {
            _shootInput = false;
            ShootServerRpc(_playerAim.AimDirection);
        }
    }

    private void OnMove(InputValue value)   => _moveInput   = value.Get<Vector2>();
    private void OnSprint(InputValue value) => _isSprinting = value.isPressed;
    private void OnFire(InputValue value)   { if (value.isPressed) _shootInput = true; }

    private void HandleMovement()
    {
        Vector3 camForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(Camera.main.transform.right,   Vector3.up).normalized;

        Vector3 moveDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }

        float speed = _isSprinting ? _sprintSpeed : _moveSpeed;
        _characterController.SimpleMove(moveDir * speed);

        float animSpeed = moveDir.magnitude > 0.1f ? (_isSprinting ? _runAnimSpeed : _walkAnimSpeed) : 0f;
        if (_animator != null)
            _animator.SetFloat(_hashSpeed, animSpeed);
    }

    /// <summary>
    /// 안정연님 담당 파트.
    /// 여기서는 서버에 발사 의도 + 방향을 전달하는 것까지만 구현.
    /// </summary>
    [ServerRpc]
    private void ShootServerRpc(Vector3 aimDir)
    {
        if (_fireOrigin == null) return;

        PlayShootAnimClientRpc();

        if (Physics.Raycast(_fireOrigin.position, aimDir, out RaycastHit hit, _aimRayLength, _shootLayerMask))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
                target.Die();
        }

        // 총구 이펙트는 확장 기능 — 여기에 추가 예정
    }

    [ClientRpc]
    private void PlayShootAnimClientRpc()
    {
        if (_animator != null)
            _animator.SetTrigger(_hashShoot);
    }

    /// <summary>
    /// 안정연님 — ClientRpc로 모든 클라이언트에 동시 호출할 것.
    /// </summary>
    [ClientRpc]
    public void DieClientRpc()
    {
        _isDead   = true;
        _canShoot = false;

        if (_animator != null)
            _animator.SetFloat(_hashSpeed, 0f);
        // _animator.SetTrigger("Die"); // 사망 애니메이션 추가 시 활성화

        Debug.Log($"[PlayerController] {OwnerClientId}번 플레이어 사망");
    }

    public void Die() => DieClientRpc();

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Shooting:
                _canShoot = true;
                if (_gunObject != null) _gunObject.SetActive(true);
                if (_animator  != null) _animator.SetTrigger(_hashDrawGun);
                break;

            case GamePhase.GameOver:
                _isDead   = true;
                _canShoot = false;
                break;
        }
    }

    public bool IsDead => _isDead;
}