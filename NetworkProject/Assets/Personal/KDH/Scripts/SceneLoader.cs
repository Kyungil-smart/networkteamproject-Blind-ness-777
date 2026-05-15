using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance { get; private set; }
    private int _retryCount = 0;
    private const int MaxRetry = 2;
    private bool _isGame = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 게임 시작 씬
    public void LoadGameScene()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
    }
    
    // 결과 팝업 표시
    [ClientRpc]
    private void ShowResultClientRpc()
    {
        FindObjectOfType<HUDUI>()?.ShowResultPopup();
    }
    
    // 로비 이동 씬
    public void LoadLobbyScene()
    {
        if (!IsServer) return;
        GameManager.Instance.ResetGame();
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
        AudioManager.Instance?.PlayLobbyBGM();
        _isGame = false;
    }
    
    // 개인 로비 이동 씬
    public void IndividualLobby()
    {
        if (!IsOwner) return;
        SceneManager.LoadScene("LobbyScene");
        AudioManager.Instance?.PlayLobbyBGM();
    }
    
    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnAllClientsLoaded;
        GameManager.Instance.CurrentPhase.OnValueChanged += Result;
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnAllClientsLoaded;
        GameManager.Instance.CurrentPhase.OnValueChanged -= Result;
    }

    // 클라이언트들 전부 로딩 됬는지 확인하는 함수
    private void OnAllClientsLoaded(string sceneName, LoadSceneMode _, 
        List<ulong> clientCompletes, List<ulong> clientTimeouts)
    {
        if (!IsServer) return;
        if (clientTimeouts.Count > 0)
        {
            if (_retryCount < MaxRetry)
            {
                _retryCount++;
                Debug.Log($"재접속 시도 횟수 : {_retryCount}/{MaxRetry}");
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                return;
            }
            foreach (ulong clentid in clientTimeouts) NetworkManager.Singleton.DisconnectClient(clentid);
            _retryCount = 0;
        }
        switch (sceneName)
        {
            case "GameScene":
                Debug.Log("Start game");
                if (_isGame) return;
                _isGame = true;
                if (clientCompletes.Count <= 1)
                {
                    LoadLobbyScene();
                    return;
                }
                StartCoroutine(GameManager.Instance.StartGame());
                break;
            case "LoadingScene":
                Debug.Log("Start loading");
                SetReadyClientRPC();
                StartCoroutine(Loading());
                break;
        }
    }

    [ClientRpc]
    private void SetReadyClientRPC()
    {
        FindObjectOfType<LoadingUI>()?.SetReady();
    }
    
    private IEnumerator Loading()
    {
        yield return new WaitForSeconds(1f);

        LoadingUI loadingUI = FindObjectOfType<LoadingUI>();
        if (loadingUI != null)
            loadingUI.OnLoadingComplete += () =>
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        else
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
    
    private void Result(GamePhase prev, GamePhase next)
    {
        if (!IsServer) return;
        if (next == GamePhase.GameOver)
            ShowResultClientRpc();
    }

    public bool LoadNetworked(string sceneName)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        if (networkManager == null || !networkManager.IsServer || networkManager.SceneManager == null)
        {
            Debug.LogError($"SceneLoader: NGO 동기화 로드 실패 - Host가 아니거나 SceneManager 없음 (scene='{sceneName}')");
            return false;
        }
        networkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        return true;
    }
}