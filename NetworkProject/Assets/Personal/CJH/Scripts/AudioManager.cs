using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameSystem 담당자 — OnPhaseChanged, OnTimerTick 호출 시점은 summary 참고.
/// </summary>
public class AudioManager : MonoBehaviour, IPhaseChangeable
{
    public static AudioManager Instance { get; private set; }

    [Header("Boot")]
    [SerializeField] private AudioClip _bootSFX; // 로고 연출용 SFX

    [Header("Title BGM")]
    [SerializeField] private List<AudioClip> _titleBGMList;

    [Header("Lobby BGM")]
    [SerializeField] private List<AudioClip> _lobbyBGMList;

    [Header("Game BGM")]
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
    [SerializeField] private float _fadeOutDuration = 2f;
    [SerializeField] private float _fadeInDuration  = 1f;

    private bool _isFading = false;

    private int _lastTitleIndex = -1;
    private int _lastLobbyIndex = -1;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgmSource.volume = _bgmVolume;
        _sfxSource.volume = _sfxVolume;
    }

    private void Update()
    {
        _sfxSource.volume = _sfxVolume;

        if (!_isFading)
            _bgmSource.volume = _bgmVolume;
    }

    /// <summary>
    /// BootScene 담당자 — 로고 연출 시작 시점에 호출할 것.
    /// </summary>
    public void PlayBootSFX()
    {
        if (_bootSFX == null) return;
        _sfxSource.clip = _bootSFX;
        _sfxSource.loop = false;
        _sfxSource.Play();
    }

    /// <summary>
    /// TitleScene 담당자 — 씬 로드 시 호출할 것.
    /// </summary>
    public void PlayTitleBGM()
    {
        PlayRandomBGM(_titleBGMList, ref _lastTitleIndex);
    }

    /// <summary>
    /// LobbyScene 담당자 — 씬 로드 시 호출할 것.
    /// </summary>
    public void PlayLobbyBGM()
    {
        PlayRandomBGM(_lobbyBGMList, ref _lastLobbyIndex);
    }

    public float GetBGMVolume() => _bgmSource.volume;
    public float GetSFXVolume() => _sfxSource.volume;

    public void SetBGMVolume(float value)
    {
        _bgmVolume        = value;
        _bgmSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        _sfxVolume        = value;
        _sfxSource.volume = value;
    }

    /// <summary>
    /// GameSystem 담당자 — 타이머가 8초 이하로 떨어지는 시점에 한 번만 호출할 것.
    /// </summary>
    public void OnTimerTick()
    {
        StartCoroutine(FadeBGM(0f, _fadeOutDuration));

        _sfxSource.clip = _tickTockClip;
        _sfxSource.loop = false;
        _sfxSource.Play();
    }

    public void OnPhaseChanged(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.HideAndSeek:
                PlayHideAndSeekBGM();
                break;

            case GamePhase.Shooting:
                OnTopViewTransition();
                break;

            case GamePhase.GameOver:
                StopAllCoroutines();
                _isFading = false;
                StartCoroutine(FadeBGM(0f, 1f));
                break;
        }
    }

    private void PlayRandomBGM(List<AudioClip> list, ref int lastIndex)
    {
        if (list == null || list.Count == 0) return;

        int index;

        if (list.Count == 1)
        {
            index = 0;
        }
        else
        {
            do { index = Random.Range(0, list.Count); }
            while (index == lastIndex); // 직전 곡 반복 방지
        }

        lastIndex = index;

        _bgmSource.clip   = list[index];
        _bgmSource.loop   = true;
        _bgmSource.volume = _bgmVolume;
        _bgmSource.Play();
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
        _isFading = false;
        _bgmSource.Stop();
        _sfxSource.Stop();

        _sfxSource.clip = _transitionClip;
        _sfxSource.loop = false;
        _sfxSource.Play();

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
        _isFading         = true;
        float startVolume = _bgmSource.volume;
        float elapsed     = 0f;

        while (elapsed < duration)
        {
            elapsed           += Time.deltaTime;
            _bgmSource.volume  = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = targetVolume;
        _isFading         = false;

        if (targetVolume == 0f)
            _bgmSource.Stop();
    }
    
    public void StopSFX()
    {
        _sfxSource.Stop();
    }
}