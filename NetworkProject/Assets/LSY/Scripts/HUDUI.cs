using UnityEngine;
using TMPro;

public class HUDUI : MonoBehaviour
{
    [Header("HUD 요소")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _remainingPlayersText;

    private float _remainingTime;

    private void OnEnable()
    {
        GameManager.Instance.AlivePlayer.OnValueChanged += OnAlivePlayerChanged;
        GameManager.Instance.CurrentPhase.OnValueChanged += OnPhaseChanged;
    }

    private void OnDisable()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.AlivePlayer.OnValueChanged -= OnAlivePlayerChanged;
        GameManager.Instance.CurrentPhase.OnValueChanged -= OnPhaseChanged;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentPhase.Value != GamePhase.HideAndSeek) return;

        _remainingTime -= Time.deltaTime;
        _remainingTime = Mathf.Max(0f, _remainingTime);
        UpdateTimerText(_remainingTime);
    }

    private void OnAlivePlayerChanged(int previous, int current)
    {
        _remainingPlayersText.text = "" + current;
    }

    private void OnPhaseChanged(GamePhase previous, GamePhase current)
    {
        if (current == GamePhase.HideAndSeek)
        {
            _remainingTime = GameManager.Instance._movingTime;
        }
    }

    private void UpdateTimerText(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        _timerText.text = seconds.ToString();
    }
}