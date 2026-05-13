using UnityEngine;
using Unity.Netcode;

public class PlayerGuideLine : NetworkBehaviour, IPhaseChangeable
{
    [Header("Guide LineRenderer (로컬 전용)")]
    [SerializeField] private LineRenderer _guideLine;
    [SerializeField] private Transform    _fireOrigin;
    [SerializeField] private float        _guideLength   = 2f;
    [SerializeField] private float        _aimLineLength = 30f;

    [Header("Line Width")]
    [SerializeField] private float _guideStartWidth = 0.03f;
    [SerializeField] private float _guideEndWidth   = 0.01f;
    [SerializeField] private float _aimStartWidth   = 0.05f;
    [SerializeField] private float _aimEndWidth     = 0.02f;

    [Header("Line Color")]
    [SerializeField] private Color _guideColor = Color.white;
    [SerializeField] private Color _aimColor   = new Color(1f, 0.2f, 0.2f);

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

        _guideLine.colorGradient = MakeGradient(_guideColor, startAlpha: 0.5f);
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

        // Inspector 수정값 실시간 반영
        _guideLine.startWidth = _isTopView ? _aimStartWidth : _guideStartWidth;
        _guideLine.endWidth   = _isTopView ? _aimEndWidth   : _guideEndWidth;
    }

    public void DisableGuideLine()
    {
        if (_guideLine != null)
            _guideLine.enabled = false;
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        if (phase != GamePhase.Shooting) return;
        if (!IsOwner || _guideLine == null) return;

        _guideLine.enabled    = true;
        _isTopView            = true;
        _guideLine.startWidth = _aimStartWidth;
        _guideLine.endWidth   = _aimEndWidth;

        // 흐릿한 방향 힌트 → 선명한 조준선으로 전환
        _guideLine.colorGradient = MakeGradient(_aimColor, startAlpha: 1f);
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