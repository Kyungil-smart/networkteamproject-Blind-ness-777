using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 탑뷰 CinemachineCamera 오브젝트에 부착.
/// 페이즈 전환에 따라 Priority를 조절해 카메라 블렌딩을 제어.
/// </summary>
public class TopViewCamera : MonoBehaviour, IPhaseChangeable
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private int _topViewPriority    = 20;
    [SerializeField] private int _defaultPriority    = 0;

    private void Awake()
    {
        if (_cinemachineCamera == null)
            _cinemachineCamera = GetComponent<CinemachineCamera>();

        _cinemachineCamera.Priority = _defaultPriority;
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Shooting:
                _cinemachineCamera.Priority = _topViewPriority;
                break;

            default:
                _cinemachineCamera.Priority = _defaultPriority;
                break;
        }
    }
}