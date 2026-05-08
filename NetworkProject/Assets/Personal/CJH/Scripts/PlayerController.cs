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

    private CharacterController _characterController;
    private PlayerAim           _playerAim;
    private PlayerRagdoll       _playerRagdoll;
    private PlayerAnimator      _playerAnimator;
    private PlayerGuideLine     _playerGuideLine;

    private bool _canShoot = false;
    private bool _isDead   = false;

    private Vector2 _moveInput;
    private bool    _isSprinting;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerAim           = GetComponent<PlayerAim>();
        _playerRagdoll       = GetComponent<PlayerRagdoll>();
        _playerAnimator      = GetComponent<PlayerAnimator>();
        _playerGuideLine     = GetComponent<PlayerGuideLine>();

        if (_fireOrigin == null)
            Debug.LogWarning("[PlayerController] _fireOrigin이 연결되지 않았습니다.", this);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            _virtualCamera.SetActive(false);
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null) playerInput.enabled = false;
            _characterController.enabled = false;
            return;
        }

        // 오너: CharacterController 잠깐 끄고 초기 위치 설정 후 다시 켜기
        _characterController.enabled = false;
        _characterController.enabled = true;
    }

    private void Update()
    {
        if (!IsOwner || _isDead) return;

        HandleMovement();
    }

    private void OnMove(InputValue value)   => _moveInput   = value.Get<Vector2>();
    private void OnSprint(InputValue value) => _isSprinting = value.isPressed;

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
        transform.position += moveDir * speed * Time.deltaTime;

        if (_playerAnimator != null)
        {
            float animSpeed = moveDir.magnitude > 0.1f
                ? (_isSprinting ? _playerAnimator.RunAnimSpeed : _playerAnimator.WalkAnimSpeed)
                : 0f;
            _playerAnimator.SetMoveSpeed(animSpeed);
        }
    }

    /// <summary>
    /// 안정연님 담당 파트.
    /// 여기서는 서버에 발사 의도 + 방향을 전달하는 것까지만 구현.
    /// </summary>
    [ServerRpc]
    public void ShootServerRpc(Vector3 aimDir, Vector3 attackerPosition)
    {
        if (_fireOrigin == null) return;

        PlayShootAnimClientRpc();

        if (Physics.Raycast(_fireOrigin.position, aimDir, out RaycastHit hit, _aimRayLength, _shootLayerMask))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null)
                target.Die(attackerPosition);
        }
    }

    [ClientRpc]
    private void PlayShootAnimClientRpc()
    {
        _playerAnimator?.PlayShoot();
    }

    /// <summary>
    /// 안정연님 — ClientRpc로 모든 클라이언트에 동시 호출할 것.
    /// attackerPosition은 공격자 위치, 래그돌 방향 계산에 사용.
    /// </summary>
    [ClientRpc]
    public void DieClientRpc(Vector3 attackerPosition)
    {
        _isDead   = true;
        _canShoot = false;

        _playerGuideLine?.DisableGuideLine();
        _playerRagdoll?.ActivateRagdoll(attackerPosition);

        if (IsServer)
            GameManager.Instance.OnPlayerDead();

        Debug.Log($"[PlayerController] {OwnerClientId}번 플레이어 사망");
    }

    public void Die(Vector3 attackerPosition) => DieClientRpc(attackerPosition);

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Shooting:
                _canShoot = true;
                break;

            case GamePhase.GameOver:
                _isDead   = true;
                _canShoot = false;
                break;
        }
    }

    public bool      IsDead     => _isDead;
    public Transform FireOrigin => _fireOrigin;
}