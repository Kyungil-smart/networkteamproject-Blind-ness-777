using UnityEngine;
using Unity.Netcode;

public class PlayerAim : NetworkBehaviour, IPhaseChangeable
{
    private bool _isTopView = false;

    private void Update()
    {
        if (!IsOwner) return;
        AimDirection = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.Shooting)
            _isTopView = true;
    }

    public Vector3 AimDirection { get; private set; }
    public bool    IsTopView    => _isTopView;
}