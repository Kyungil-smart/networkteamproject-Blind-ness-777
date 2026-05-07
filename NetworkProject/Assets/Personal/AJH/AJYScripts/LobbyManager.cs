using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    /*
     * 내용 : 로비에서 이루어지는 로직 관리
     */
    public static LobbyManager Instance { get; private set; }
    
    private ISession _session;
    
    private string _playerName = "Player";
    
    private bool _isStartingGame;
    private bool _isQuitting;
    
    private float _lastGameEndRealtime = float.NegativeInfinity;
    private Coroutine _restartCooldownRoutine;
    
    private const int JOIN_MAX_RETRY = 1;
    private const int JOIN_RETRY_DELAY_MS = 500;
    
    [SerializeField] private LobbySettings _settings;
    
    public LobbySettings Settings => _settings;
    
    public string PlayerName => _playerName;

    public bool IsHost => _session != null && _session.IsHost;
    
    public ISession CurrentSession => _session;
    
    /// 게임 시작 시점에 확정된 세션 인원수. 게임 씬 합류 판정용
    /// </summary>
    public int ExpectedPlayerCount { get; private set; }
    
    public event Action<ISession> OnSessionUpdated;
    public event Action OnSessionLeft;
    public event Action OnGameStarting;
    
    /// <summary>
    /// 게임 재시작 쿨다운이 끝난 시점에 1회 발화. 시간 기반 조건 변화를 이벤트로 전파
    /// </summary>
    public event Action OnRestartCooldownEnded;
    
    private void Awake()
    {
        SetSingleton();
        Application.wantsToQuit += OnWantsToQuit;
    }

    private void OnDestroy()
    {
        Application.wantsToQuit -= OnWantsToQuit;
    }
    
    private void BindSessionEvents(ISession session)
    {
        if (session == null) return;
        session.Changed += RaiseSessionUpdated;
        session.PlayerJoined += RaiseSessionUpdated;
        session.PlayerHasLeft += RaiseSessionUpdated;
        session.PlayerPropertiesChanged += RaiseSessionUpdated;
        session.RemovedFromSession += HandleSessionGone;
        session.Deleted += HandleSessionGone;
    }

    private void UnbindSessionEvents(ISession session)
    {
        if (session == null) return;
        session.Changed -= RaiseSessionUpdated;
        session.PlayerJoined -= RaiseSessionUpdated;
        session.PlayerHasLeft -= RaiseSessionUpdated;
        session.PlayerPropertiesChanged -= RaiseSessionUpdated;
        session.RemovedFromSession -= HandleSessionGone;
        session.Deleted -= HandleSessionGone;
    }
    
    private void RaiseSessionUpdated()
    {
        OnSessionUpdated?.Invoke(_session);
    }

    private void RaiseSessionUpdated(string _)
    {
        OnSessionUpdated?.Invoke(_session);
    }
    
    private void HandleSessionGone()
    {
        UnbindSessionEvents(_session);
        _session = null;
        OnSessionLeft?.Invoke();
    }
    
    public async Task<bool> CreateSessionAsync(string sessionName)
    {
        _playerName = sessionName;
        
        for (int attempt = 0; attempt <= JOIN_MAX_RETRY; attempt++)
        {
            await EnsureCleanNetworkStateAsync();
            try
            {
                string region = string.IsNullOrWhiteSpace(_settings.RelayRegion) ? null : _settings.RelayRegion;
                SessionOptions options = new SessionOptions
                {
                    Name = sessionName,
                    MaxPlayers = _settings.MaxPlayers,
                    IsPrivate = false,
                    PlayerProperties = BuildLocalPlayerProperties()
                }.WithRelayNetwork(region);
                _session = await MultiplayerService.Instance.CreateSessionAsync(options);
                if (!await VerifyNgoStartedOrCleanupAsync())
                {
                    if (attempt < JOIN_MAX_RETRY) continue;
                    return false;
                }
                BindSessionEvents(_session);
                OnSessionUpdated?.Invoke(_session);
                return true;
            }
            catch (Exception e) when (attempt < JOIN_MAX_RETRY && IsTransientNgoError(e))
            {
                Debug.LogWarning($"LobbyManager: 생성 일시 실패 - 자동 재시도: {e.Message}");
                await Task.Delay(JOIN_RETRY_DELAY_MS);
            }
            catch (Exception e)
            {
                Debug.LogError($"LobbyManager: 생성 실패: {e.Message}");
                return false;
            }
        }
        return false;
    }
    
    public async Task<bool> JoinSessionByCodeAsync(string sessionCode, string playerName)
    {
        _playerName = playerName;
        
        for (int attempt = 0; attempt <= JOIN_MAX_RETRY; attempt++)
        {
            await EnsureCleanNetworkStateAsync();
            try
            {
                JoinSessionOptions options = new JoinSessionOptions
                {
                    PlayerProperties = BuildLocalPlayerProperties()
                };
                _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode, options);
                if (!await VerifyNgoStartedOrCleanupAsync())
                {
                    if (attempt < JOIN_MAX_RETRY) continue;
                    return false;
                }
                BindSessionEvents(_session);
                OnSessionUpdated?.Invoke(_session);
                return true;
            }
            catch (Exception e) when (attempt < JOIN_MAX_RETRY && IsTransientNgoError(e))
            {
                Debug.LogWarning($"LobbyManager: 코드 참여 일시 실패 - 자동 재시도: {e.Message}");
                await Task.Delay(JOIN_RETRY_DELAY_MS);
            }
            catch (Exception e)
            {
                Debug.LogError($"LobbyManager: 코드 참여 실패: {e.Message}");
                return false;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 호스트가 현재 세션 기준으로 게임을 시작할 수 있는 상태인지 여부
    /// </summary>
    public bool CanHostStartGame
    {
        get
        {
            if (!IsHost || _session == null || _isStartingGame) return false;
            if (Time.realtimeSinceStartup - _lastGameEndRealtime < _settings.GameRestartCooldownSec) return false;
            if (_session.PlayerCount < _settings.MinPlayersToStart) return false;
            return AreNonHostPlayersReady();
        }
    }
    
    /// <summary>
    /// 현재 세션에서 퇴장
    /// </summary>
    public async Task LeaveSessionAsync()
    {
        if (_session == null) return;
        ISession session = _session;
        UnbindSessionEvents(session);
        _session = null;
        try
        {
            await session.LeaveAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LobbyManager: 퇴장 중 예외: {e.Message}");
        }
        OnSessionLeft?.Invoke();
    }
    
    /// <summary>
    /// 임의 플레이어의 PlayerProperty 값을 읽는 헬퍼
    /// </summary>
    public static string GetPlayerProperty(IReadOnlyPlayer player, string key)
    {
        if (player == null || player.Properties == null) return null;
        return player.Properties.TryGetValue(key, out PlayerProperty prop) ? prop.Value : null;
    }
    
    private bool AreNonHostPlayersReady()
    {
        if (_session == null || _session.Players.Count == 0) return false;
        bool hasNonHost = false;
        for (int i = 0; i < _session.Players.Count; i++)
        {
            IReadOnlyPlayer player = _session.Players[i];
            if (player.Id == _session.Host) continue;
            hasNonHost = true;
            string ready = GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
            if (ready != LobbyConstants.VALUE_TRUE) return false;
        }
        return hasNonHost;
    }
    
    /// <summary>
    /// 자신의 레디 상태 토글/설정
    /// </summary>
    /// <param name="isReady">레디 여부</param>
    public async Task SetReadyAsync(bool isReady)
    {
        try
        {
            await UpdateLocalReadyPropertyAsync(isReady);
            OnSessionUpdated?.Invoke(_session);
        }
        catch (Exception e)
        {
            Debug.LogError($"LobbyManager: 레디 갱신 실패: {e.Message}");
        }
    }
    
    // 레디 상태 변경
    private async Task UpdateLocalReadyPropertyAsync(bool isReady)
    {
        if (_session == null) return;
        string value = isReady ? LobbyConstants.VALUE_TRUE : LobbyConstants.VALUE_FALSE;
        _session.CurrentPlayer.SetProperty(
            LobbyConstants.KEY_PLAYER_READY,
            new PlayerProperty(value, VisibilityPropertyOptions.Member));
        await _session.SaveCurrentPlayerDataAsync();
    }
    
    /// <summary>
    /// 호스트가 직접 호출하는 게임 시작.
    /// 세션 잠금 후 NGO 씬 로드 (이 시점엔 모든 멤버는 이미 NGO에 연결되어 있음)
    /// </summary>
    /// <returns>실제 시작에 성공했으면 true</returns>
    public async Task<bool> TryStartGameAsHostAsync()
    {
        if (!IsHost || _session == null || _isStartingGame) return false;
        if (Time.realtimeSinceStartup - _lastGameEndRealtime < _settings.GameRestartCooldownSec) return false;
        if (_session.PlayerCount < _settings.MinPlayersToStart || !AreNonHostPlayersReady()) return false;

        _isStartingGame = true;
        ExpectedPlayerCount = _session.PlayerCount;

        try
        {
            IHostSession host = _session.AsHost();
            host.IsLocked = true;
            await host.SavePropertiesAsync();
            OnGameStarting?.Invoke();
            
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || !networkManager.IsServer || networkManager.SceneManager == null)
            {
                Debug.LogError($"Host가 아니거나 SceneManager 없음 (scene='')");
                _isStartingGame = false;
                return false;
            }
            // 로드 게임씬
            networkManager.SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"LobbyManager: 호스트 게임 시작 실패: {e.Message}");
            _isStartingGame = false;
            return false;
        }
    }
    
    /// <summary>
    /// 게임 종료 후 현재 세션을 유지한 채 룸 화면으로 복귀
    /// </summary>
    public async Task ReturnToRoomAsync()
    {
        _isStartingGame = false;
        _lastGameEndRealtime = Time.realtimeSinceStartup;
        StartRestartCooldownWatch();

        if (IsHost && _session != null)
        {
            try
            {
                IHostSession host = _session.AsHost();
                host.IsLocked = false;
                await host.SavePropertiesAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"LobbyManager: 게임 종료 후 잠금 해제 실패: {e.Message}");
            }
        }

        // 모든 멤버: 자기 ready 해제. 다른 플레이어 PlayerProperty는 host도 직접 못 바꾸므로 각자 해제
        try
        {
            await UpdateLocalReadyPropertyAsync(false);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LobbyManager: 레디 해제 실패: {e.Message}");
        }

        OnSessionUpdated?.Invoke(_session);

        // 로비로 씬전환
        if (IsHost)
        {
            //SceneLoader.LoadNetworked(SceneId.Lobby);
        }
    }
    
    private bool OnWantsToQuit()
    {
        if (_session == null || _isQuitting) return true;
        _isQuitting = true;
        _ = LeaveAndQuitAsync();
        return false;   
    }

    private async Task LeaveAndQuitAsync()
    {
        try
        {
            await _session.LeaveAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LobbyManager: quit-leave 실패: {e.Message}");
        }
        Application.Quit();
    }
    
    // 재시도로 해결되는 transient 실패인지 판별 (메시지 문자열 매칭).
    private static bool IsTransientNgoError(Exception e)
    {
        if (e == null || e.Message == null) return false;
        return e.Message.Contains("Failed to start NetworkManager")
               || e.Message.Contains("task was canceled")
               || e.Message.Contains("A task was canceled");
    }
    
    // 세션 진입 직후 NGO Network.State 가 Started 인지 검증. 비정상이면 강제 leave 후 false.
    // 클라이언트 race로 즉시 체크 시 Stopped 로 보이는 false negative 사유는
    private const float NGO_START_WAIT_SEC = 6f;
    private async Task<bool> VerifyNgoStartedOrCleanupAsync()
    {
        if (_session == null) return false;

        float deadline = Time.realtimeSinceStartup + NGO_START_WAIT_SEC;
        while (Time.realtimeSinceStartup < deadline)
        {
            if (_session == null) return false;
            if (_session.Network.State == NetworkState.Started) return true;
            await Task.Yield();
        }

        Debug.LogError($"LobbyManager: 세션 진입 후 NGO 비정상 상태: {_session?.Network.State} - 강제 leave");
        ISession failed = _session;
        _session = null;
        if (failed != null)
        {
            try { await failed.LeaveAsync(); }
            catch (Exception e) { Debug.LogWarning($"LobbyManager: 비정상 정리 중 예외: {e.Message}"); }
        }
        return false;
    }
    
    private Dictionary<string, PlayerProperty> BuildLocalPlayerProperties()
    {
        return new Dictionary<string, PlayerProperty>
        {
            { LobbyConstants.KEY_PLAYER_NAME, new PlayerProperty(_playerName, VisibilityPropertyOptions.Member) },
            { LobbyConstants.KEY_PLAYER_READY, new PlayerProperty(LobbyConstants.VALUE_FALSE, VisibilityPropertyOptions.Member) }
        };
    }
    
    // 진입 전 NGO 잔재 정리 (이전 시도 흔적이 남으면 다음 StartHost/StartClient 가 깨질 수 있음).
    private async Task EnsureCleanNetworkStateAsync()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null) return;
        if (!networkManager.IsListening && !networkManager.IsClient && !networkManager.IsServer) return;

        Debug.LogWarning("LobbyManager: 이전 NGO 잔재 감지 - Shutdown 후 진행");
        networkManager.Shutdown();

        for (int i = 0; i < 30; i++)
        {
            await Task.Yield();
            if (!networkManager.IsListening && !networkManager.IsClient && !networkManager.IsServer) break;
        }
    }
    
    // 재시작/시작 쿨다운
    private void StartRestartCooldownWatch()
    {
        if (_restartCooldownRoutine != null) StopCoroutine(_restartCooldownRoutine);
        _restartCooldownRoutine = StartCoroutine(WaitForRestartCooldownThenNotify());
    }
    
    // 재시작 코루틴 조건
    private IEnumerator WaitForRestartCooldownThenNotify()
    {
        yield return new WaitForSeconds(_settings.GameRestartCooldownSec);
        _restartCooldownRoutine = null;
        OnRestartCooldownEnded?.Invoke();
    }

    private void SetSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
