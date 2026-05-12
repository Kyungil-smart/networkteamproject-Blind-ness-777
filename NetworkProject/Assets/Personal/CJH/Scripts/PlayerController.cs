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
    [SerializeField] private GameObject _cameraPrefab;

    [Header("Raycast")]
    [SerializeField] private Transform _fireOrigin;
    [SerializeField] private float     _aimRayLength = 30f;
    [SerializeField] private LayerMask _shootLayerMask;

    private CharacterController _characterController;
    private PlayerInput         _playerInput;
    private PlayerAim           _playerAim;
    private PlayerRagdoll       _playerRagdoll;
    private PlayerAnimator      _playerAnimator;
    private PlayerGuideLine     _playerGuideLine;
    private ThirdPersonCamera   _thirdPersonCamera;

    private bool _canShoot = false;
    private bool _isDead   = false;

    private Vector2 _moveInput;
    private bool    _isSprinting;
    private Camera  _mainCamera;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerInput         = GetComponent<PlayerInput>();
        _playerAim           = GetComponent<PlayerAim>();
        _playerRagdoll       = GetComponent<PlayerRagdoll>();
        _playerAnimator      = GetComponent<PlayerAnimator>();
        _playerGuideLine     = GetComponent<PlayerGuideLine>();

        if (_fireOrigin == null)
            Debug.LogWarning("[PlayerController] _fireOrigin이 연결되지 않았습니다.", this);
    }
    
    private void OnEnable()
    {
        if (_playerInput == null) return;

        _playerInput.actions["Move"].performed += OnMove;
        _playerInput.actions["Move"].canceled  += OnMove;

        _playerInput.actions["Sprint"].performed += OnSprint;
        _playerInput.actions["Sprint"].canceled  += OnSprint;
    }

    private void OnDisable()
    {
        if (_playerInput == null) return;

        _playerInput.actions["Move"].performed -= OnMove;
        _playerInput.actions["Move"].canceled  -= OnMove;

        _playerInput.actions["Sprint"].performed -= OnSprint;
        _playerInput.actions["Sprint"].canceled  -= OnSprint;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            _characterController.enabled = false;
            if (_playerInput != null) _playerInput.enabled = false;
            return;
        }

        GameObject camObj = Instantiate(_cameraPrefab);
        _thirdPersonCamera = camObj.GetComponentInChildren<ThirdPersonCamera>();
        _thirdPersonCamera.SetTarget(transform);
        _mainCamera = camObj.GetComponentInChildren<Camera>();

        TopViewCamera topViewCam = FindObjectOfType<TopViewCamera>();
        if (topViewCam != null)
            topViewCam.SetThirdPersonCamera(_thirdPersonCamera);
    }

    private void Update()
    {
        if (!CanMove())
            return;

        HandleMovement();
    }

    private bool CanMove()
    {
        return IsOwner &&
               !_isDead &&
               GameManager.Instance.CurrentPhase.Value != GamePhase.Shooting;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.ReadValue<float>() > 0.1f;
    }

    private void HandleMovement()
    {
        if (_mainCamera == null) return;
        
        Vector3 camForward = Vector3.ProjectOnPlane(_mainCamera.transform.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(_mainCamera.transform.right,   Vector3.up).normalized;

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
            case GamePhase.HideAndSeek:
                _thirdPersonCamera?.OnPhaseChanged(phase);
                break;
            
            case GamePhase.Shooting:
                _canShoot = true;
                _moveInput = Vector2.zero;
                _isSprinting = false;
                if (_playerAnimator != null)
                    _playerAnimator.SetMoveSpeed(0f);
                _thirdPersonCamera?.OnPhaseChanged(phase);
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