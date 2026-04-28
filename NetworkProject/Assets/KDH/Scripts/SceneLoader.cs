using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : NetworkBehaviour
{
    public static SceneLoader Instance { get; private set; }
    private int _retryCount = 0;
    private const int MaxRetry = 2;
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
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
    
    // 결과 씬 전환
    public void LoadResultScene()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("ResultScene", LoadSceneMode.Single);
    }
    
    // 로비 이동 씬
    public void LoadLobbyScene()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.SceneManager.LoadScene("LobbyScene", LoadSceneMode.Single);
    }
    
    // 개인 로비 이동 씬
    public void individualLobby()
    {
        if (!IsOwner) return;
        SceneManager.LoadScene("LobbyScene");
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
                if (clientCompletes.Count <= 1)
                {
                    LoadLobbyScene();
                    return;
                }
                GameManager.Instance.StartGame();
                break;
        }
    }
    
    // 결과창으로 이동
    private void Result(GamePhase prev, GamePhase next)
    {
        if (!IsServer) return;
        if (next == GamePhase.GameOver) LoadResultScene();
    }
}
