using UnityEngine;
using Unity.Netcode;
 
public class PlayerAim : NetworkBehaviour, IPhaseChangeable
{
    private bool _isTopView = false;
 
    private void Update()
    {
        if (!IsOwner) return;
 
        // 현재는 이동 방향 = 조준 방향. 추후 독립적인 조준 입력 추가 시 여기서 처리.
        AimDirection = transform.forward;
    }
 
    public void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shooting)
            _isTopView = true;
    }
 
    public Vector3 AimDirection { get; private set; } = Vector3.forward;
    public bool    IsTopView    => _isTopView;
}