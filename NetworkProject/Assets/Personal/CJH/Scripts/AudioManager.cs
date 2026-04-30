using System.Collections;
using UnityEngine;

/// <summary>
/// GameSystem 담당자 — 실제 게임 연동 시 OnPhaseChanged, OnTimerTick을 직접 호출할 것.
/// 테스트용 OnGUI 버튼은 연동 완료 후 제거.
/// </summary>
public class AudioManager : MonoBehaviour, IPhaseChangeable
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioClip   _hideAndSeekBGM;
    [SerializeField] private AudioClip   _epicTrapBGM;

    [Header("SFX")]
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioClip   _tickTockClip;
    [SerializeField] private AudioClip   _transitionClip;

    [Header("Volume")]
    [SerializeField] [Range(0f, 1f)] private float _bgmVolume = 0.7f;
    [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 1f;

    [Header("Fade")]
    [SerializeField] private float _fadeOutDuration = 2f; // 틱톡 시작 시 BGM 페이드 아웃
    [SerializeField] private float _fadeInDuration  = 1f; // Epic Trap 페이드 인

    private bool _transitioned = false;
    private bool _tickPlayed   = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _bgmSource.volume = _bgmVolume;
        _sfxSource.volume = _sfxVolume;
    }

    private void Start()
    {
        PlayHideAndSeekBGM();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 220, 120));

        if (!_tickPlayed && GUILayout.Button("타이머 틱 (8초 전)"))
        {
            OnTimerTick();
            _tickPlayed = true;
        }

        if (!_transitioned && GUILayout.Button("탑뷰 전환"))
        {
            _transitioned = true;
            OnTopViewTransition();
        }

        if (GUILayout.Button("처음부터"))
            ResetDemo();

        GUILayout.EndArea();
    }
    
    public float GetBGMVolume() => _bgmSource.volume;
    public float GetSFXVolume() => _sfxSource.volume;

    public void SetBGMVolume(float value)
    {
        _bgmVolume = value; _bgmSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        _sfxVolume = value; _sfxSource.volume = value;
    }

    /// <summary>
    /// GameSystem 담당자 — 타이머가 8초 이하로 떨어지는 시점에 한 번만 호출할 것.
    /// </summary>
    public void OnTimerTick()
    {
        // 틱톡과 함께 BGM을 서서히 줄여 긴장감 고조
        StartCoroutine(FadeBGM(0f, _fadeOutDuration));

        _sfxSource.clip = _tickTockClip;
        _sfxSource.loop = false;
        _sfxSource.Play();
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Shooting:
                _transitioned = true;
                OnTopViewTransition();
                break;

            case GamePhase.GameOver:
                StopAllCoroutines();
                StartCoroutine(FadeBGM(0f, 1f));
                break;
        }
    }

    private void PlayHideAndSeekBGM()
    {
        _bgmSource.clip   = _hideAndSeekBGM;
        _bgmSource.loop   = true;
        _bgmSource.volume = _bgmVolume;
        _bgmSource.Play();
    }

    private void OnTopViewTransition()
    {
        StopAllCoroutines();
        _bgmSource.Stop();
        _sfxSource.Stop();

        _sfxSource.clip = _transitionClip;
        _sfxSource.loop = false;
        _sfxSource.Play();

        // 전환 이펙트가 끝나면 Epic Trap 페이드 인
        StartCoroutine(PlayEpicTrapAfterDelay(_transitionClip.length));
    }

    private IEnumerator PlayEpicTrapAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        _bgmSource.clip   = _epicTrapBGM;
        _bgmSource.loop   = false;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        yield return StartCoroutine(FadeBGM(_bgmVolume, _fadeInDuration));
    }

    private IEnumerator FadeBGM(float targetVolume, float duration)
    {
        float startVolume = _bgmSource.volume;
        float elapsed     = 0f;

        while (elapsed < duration)
        {
            elapsed           += Time.deltaTime;
            _bgmSource.volume  = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = targetVolume;

        if (targetVolume == 0f)
            _bgmSource.Stop();
    }

    private void ResetDemo()
    {
        StopAllCoroutines();
        CancelInvoke();
        _transitioned = false;
        _tickPlayed   = false;
        _sfxSource.Stop();
        PlayHideAndSeekBGM();
    }
}