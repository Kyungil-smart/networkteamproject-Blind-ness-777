using System.Collections;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// GameManager와 같은 오브젝트에 부착.
/// 페이즈별 타이머를 관리하고, HideAndSeek 종료 8초 전 틱톡 SFX를 트리거.
/// </summary>
public class PhaseChangeTimer : NetworkBehaviour
{
    [SerializeField] private float _hideAndSeekTime = 90f;
    [SerializeField] private float _shootingTime    = 30f;
    [SerializeField] private float _tickTockOffset  = 8f;

    private Coroutine _timerRoutine;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        GameManager.Instance.CurrentPhase.OnValueChanged += OnPhaseChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        GameManager.Instance.CurrentPhase.OnValueChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(GamePhase previous, GamePhase current)
    {
        if (_timerRoutine != null)
            StopCoroutine(_timerRoutine);

        switch (current)
        {
            case GamePhase.HideAndSeek:
                _timerRoutine = StartCoroutine(HideAndSeekTimer());
                break;

            case GamePhase.Shooting:
                _timerRoutine = StartCoroutine(ShootingTimer());
                break;
        }
    }

    private IEnumerator HideAndSeekTimer()
    {
        float tickPoint = _hideAndSeekTime - _tickTockOffset;

        if (tickPoint > 0f)
        {
            yield return new WaitForSeconds(tickPoint);
            AudioManager.Instance?.OnTimerTick();
            yield return new WaitForSeconds(_tickTockOffset);
        }
        else
        {
            yield return new WaitForSeconds(_hideAndSeekTime);
        }

        GameManager.Instance.CurrentPhase.Value = GamePhase.Shooting;
    }

    private IEnumerator ShootingTimer()
    {
        yield return new WaitForSeconds(_shootingTime);

        if (GameManager.Instance.AlivePlayer.Value > 1)
            GameManager.Instance.CurrentPhase.Value = GamePhase.HideAndSeek;
        else
            GameManager.Instance.CurrentPhase.Value = GamePhase.GameOver;
    }
}