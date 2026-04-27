using UnityEngine;
using Unity.Netcode;
 
public class PlayerGuideLine : NetworkBehaviour, IPhaseChangeable
{
    [Header("Guide LineRenderer (로컬 전용)")]
    [SerializeField] private LineRenderer _guideLine;
    [SerializeField] private Transform    _fireOrigin;
    [SerializeField] private float        _guideLength   = 2f;
    [SerializeField] private float        _aimLineLength = 30f;
 
    private PlayerAim _playerAim;
    private bool      _isTopView = false;
 
    private void Awake()
    {
        _playerAim = GetComponent<PlayerAim>();
    }
 
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (_guideLine != null) _guideLine.enabled = false;
            return;
        }
 
        InitGuideLine();
    }
 
    private void Update()
    {
        if (!IsOwner) return;
        UpdateGuideLine();
    }
 
    private void InitGuideLine()
    {
        if (_guideLine == null) return;
 
        _guideLine.positionCount = 2;
        _guideLine.useWorldSpace = true;
        _guideLine.startWidth    = 0.03f;
        _guideLine.endWidth      = 0.01f;
 
        _guideLine.colorGradient = MakeGradient(Color.white, startAlpha: 0.5f);
    }
 
    private void UpdateGuideLine()
    {
        if (_guideLine == null || _fireOrigin == null) return;
        if (!_guideLine.enabled) return;
 
        float   len   = _isTopView ? _aimLineLength : _guideLength;
        Vector3 start = _fireOrigin.position;
        Vector3 end   = start + _playerAim.AimDirection * len;
 
        _guideLine.SetPosition(0, start);
        _guideLine.SetPosition(1, end);
    }
 
    public void OnPhaseChanged(GamePhase phase)
    {
        if (phase != GamePhase.Shooting) return;
        if (!IsOwner || _guideLine == null) return;
 
        _isTopView = true;
 
        // 흐릿한 방향 힌트 → 선명한 조준선으로 전환
        _guideLine.colorGradient = MakeGradient(new Color(1f, 0.2f, 0.2f), startAlpha: 1f);
    }
 
    private Gradient MakeGradient(Color color, float startAlpha)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(0f,         1f)
            }
        );
        return gradient;
    }
}