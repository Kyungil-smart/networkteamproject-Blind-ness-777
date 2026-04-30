using UnityEngine;

public class PlayerTest : MonoBehaviour
{
    private PlayerAnimator _playerAnimator;

    private void Awake()
    {
        _playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 150, 220, 100));

        if (GUILayout.Button("총 꺼내기 (DrawGun)"))
            _playerAnimator?.OnPhaseChanged(GamePhase.Shooting);

        if (GUILayout.Button("발사 (Shoot)"))
            _playerAnimator?.PlayShoot();

        GUILayout.EndArea();
    }
}