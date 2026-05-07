using UnityEngine;
using TMPro;

public class HUDUI : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI _timerText;
    [SerializeField] private TextMeshProUGUI _remainingPlayersText;

    [Header("팝업")]
    [SerializeField] private GameObject _escPopup;

    private float _remainingTime;
    private PhaseChangeTimer _phaseChangeTimer;

    private void OnEnable()
    {
        _phaseChangeTimer = GameManager.Instance.GetComponent<PhaseChangeTimer>();
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool isOpen = _escPopup.activeSelf;
            _escPopup.SetActive(!isOpen);
        }

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
            _remainingTime = _phaseChangeTimer.HideAndSeekTime;
    }

    private void UpdateTimerText(float time)
    {
        int seconds = Mathf.CeilToInt(time);
        _timerText.text = seconds.ToString();
    }
}