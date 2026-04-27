using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

/// <summary>
/// 3인칭 플레이어 컨트롤러 (NGO 리슨 서버 기반)
/// Day 1: 이동, 카메라
/// Day 2: 조준 방향, 방향 가이드 LineRenderer, 발사 입력
/// Day 3: 사망 처리, 탑뷰 전환 시 조준선 연출
/// </summary>
public class PlayerController : NetworkBehaviour, IDamageable, IPhaseChangeable
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed   = 5f;
    [SerializeField] private float _sprintSpeed = 10f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] private GameObject _virtualCamera; // Cinemachine Virtual Camera 오브젝트

    [Header("Aim & Shoot")]
    [SerializeField] private Transform _fireOrigin; // 몸 중앙 빈 오브젝트
    [SerializeField] private float     _aimRayLength = 30f;

    [Header("Guide LineRenderer (로컬 전용)")]
    [SerializeField] private LineRenderer _guideLine;
    [SerializeField] private float        _guideLength   = 2f; // 1페이즈 짧은 길이
    [SerializeField] private float        _aimLineLength = 30f; // 탑뷰 전환 후 긴 조준선
    
    [Header("Raycast")]
    [SerializeField] private LayerMask _shootLayerMask; // Player 레이어만

    [Header("Gun")]
    [SerializeField] private GameObject _gunObject; // RightHand Bone 하위 권총 오브젝트

    [Header("Animator")]
    [SerializeField] private Animator _animator;
    [SerializeField] private float _walkAnimSpeed   = 0.5f;
    [SerializeField] private float _runAnimSpeed    = 1f;

    private CharacterController _characterController;
    private Vector3 _aimDirection = Vector3.forward;

    // 페이즈 상태
    private bool _isTopView = false;
    private bool _canShoot  = false;
    private bool _isDead    = false;

    private Vector2 _moveInput;
    private bool    _shootInput;
    private bool    _isSprinting;
    
    private static readonly int _hashSpeed   = Animator.StringToHash("Speed");
    private static readonly int _hashDrawGun = Animator.StringToHash("DrawGun");
    private static readonly int _hashShoot   = Animator.StringToHash("Shoot");

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // 다른 플레이어 캐릭터 — 카메라, 가이드라인 비활성화
            _virtualCamera.SetActive(false);
            PlayerInput playerInput = GetComponent<PlayerInput>();
            if (playerInput != null) playerInput.enabled = false;
            if (_guideLine != null) _guideLine.enabled = false;
            return;
        }

        InitGuideLine();
        if (_gunObject != null) _gunObject.SetActive(false);
    }

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner || _isDead) return;

        HandleMovement();
        HandleAimDirection();
        UpdateGuideLine();

        if (_canShoot && _shootInput)
        {
            _shootInput = false;
            ShootServerRpc(_aimDirection);
        }
    }
    
    private void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
    }
    
    private void OnSprint(InputValue value)
    {
        _isSprinting = value.isPressed;
    }

    private void OnFire(InputValue value)
    {
        if (value.isPressed) _shootInput = true;
    }

    private void HandleMovement()
    {
        Vector3 camForward = Vector3.ProjectOnPlane(
            Camera.main.transform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(
            Camera.main.transform.right, Vector3.up).normalized;

        Vector3 moveDir = (camForward * _moveInput.y + camRight * _moveInput.x).normalized;

        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, _rotationSpeed * Time.deltaTime);
        }

        float speed = _isSprinting ? _sprintSpeed : _moveSpeed;
        _characterController.SimpleMove(moveDir * speed);

        float animSpeed = moveDir.magnitude > 0.1f ? (_isSprinting ? _runAnimSpeed : _walkAnimSpeed) : 0f;
        if (_animator != null)
            _animator.SetFloat(_hashSpeed, animSpeed);
    }

    private void HandleAimDirection()
    {
        _aimDirection = transform.forward;
    }

    private void InitGuideLine()
    {
        if (_guideLine == null) return;

        _guideLine.positionCount = 2;
        _guideLine.useWorldSpace = true;
        
        // 1페이즈 초기 상태 — 흐릿하고 짧은 막대
        _guideLine.startWidth    = 0.03f;
        _guideLine.endWidth      = 0.01f;
        
        // Gradient로 알파 처리
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.5f, 0f), // 시작 반투명
                new GradientAlphaKey(0f,   1f)  // 끝 투명
            }
        );
        _guideLine.colorGradient = gradient;
    }

    private void UpdateGuideLine()
    {
        if (_guideLine == null || _fireOrigin == null) return;
        if (!_guideLine.enabled) return;

        float   len   = _isTopView ? _aimLineLength : _guideLength;
        Vector3 start = _fireOrigin.position;
        Vector3 end   = start + _aimDirection * len;

        _guideLine.SetPosition(0, start);
        _guideLine.SetPosition(1, end);
    }
    
    /// <summary>
    /// 탑뷰 전환 시 GameSystem 혹은 NetworkManager에서 호출.
    /// ClientRpc로 모든 클라이언트에 동시 호출할 것.
    /// </summary>
    public void OnTopViewTransition()
    {
        _isTopView = true;
        _canShoot  = true;

        // 권총 활성화 + 꺼내는 애니메이션
        if (_gunObject != null) _gunObject.SetActive(true);
        if (_animator  != null) _animator.SetTrigger(_hashDrawGun);
        
        // 오너에게만 조준선 스타일 변경
        if (!IsOwner || _guideLine == null) return;
        Gradient aimGradient = new Gradient();
        aimGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.2f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.2f, 0.2f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f), // 시작 선명
                new GradientAlphaKey(0f, 1f)  // 끝 투명
            }
        );
        _guideLine.colorGradient = aimGradient;
    }

    /// <summary>
    /// 안정연님 담당 파트.
    /// 여기서는 서버에 발사 의도 + 방향을 전달하는 것까지만 구현.
    /// </summary>
    [ServerRpc]
    private void ShootServerRpc(Vector3 aimDir)
    {
        // 서버에서 레이캐스트 판정
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
    
    [ClientRpc]
    public void DieClientRpc()
    {
        _isDead   = true;
        _canShoot = false;

        // 입력 완전 비활성화는 Update의 _isDead 체크로 처리됨
        // 추후 사망 애니메이션 트리거 추가 가능
        if (_animator != null)
            _animator.SetFloat(_hashSpeed, 0f);
        // _animator.SetTrigger("Die"); // 사망 애니메이션 추가 시 활성화

        Debug.Log($"[PlayerController] {OwnerClientId}번 플레이어 사망");
    }
    
    public void Die()
    {
        DieClientRpc();
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.HideAndSeek:
                // 현재는 초기 스폰 시 기본 상태가 HideAndSeek이라 별도 처리 없음
                break;
            case GamePhase.Shooting:
                OnTopViewTransition();
                break;
            case GamePhase.GameOver:
                _isDead   = true;
                _canShoot = false;
                break;
        }
    }
    
    // 외부 접근용 프로퍼티
    public bool    IsDead       => _isDead;
    public Vector3 AimDirection => _aimDirection;
}