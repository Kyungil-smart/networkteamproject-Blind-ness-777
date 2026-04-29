using UnityEngine;
using Unity.Netcode;

public class PlayerAim : NetworkBehaviour, IPhaseChangeable
{
    private bool _isTopView = false;

    private void Start()
    {
        AimDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // 캐릭터가 실제로 이동 중일 때만 조준 방향 갱신
        // 카메라가 회전해도 캐릭터가 움직이지 않으면 방향 고정
        if (IsMoving())
            AimDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
    }

    private bool IsMoving()
    {
        // CharacterController의 velocity로 실제 이동 여부 판단
        CharacterController cc = GetComponent<CharacterController>();
        if (cc == null) return false;
        Vector3 horizontalVelocity = new Vector3(cc.velocity.x, 0f, cc.velocity.z);
        return horizontalVelocity.magnitude > 0.1f;
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shooting)
            _isTopView = true;
    }

    public Vector3 AimDirection { get; private set; }
    public bool    IsTopView    => _isTopView;
}