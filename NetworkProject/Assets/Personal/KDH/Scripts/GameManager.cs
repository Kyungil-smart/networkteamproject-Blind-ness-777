using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public NetworkVariable<GamePhase> CurrentPhase = new(GamePhase.Waiting);    // 게임 페이즈 변수
    public NetworkVariable<int> AlivePlayer = new(0);                           // 플레이 인원수 변수
    public MapLoader _mapSpawn;
    [SerializeField] public float _movingTime = 10f;                            // 숨는 시간 변수
    private AISetActive[] _aiList;
    public GameObject _aiPrefab;
    
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
    
    // 게임 시작 함수
    public void StartGame()
    {
        if (!IsServer) return;
        AlivePlayer.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        _mapSpawn = FindObjectOfType<MapLoader>();
        if (_mapSpawn != null) _mapSpawn.LoadMap();
        // ai소환
        SpawnAI();
        // ai저장
        _aiList = FindObjectsOfType<AISetActive>();
        StartCoroutine(GamePlay());
    }

    public void SpawnAI()
    {
        Transform[] spawnPoints = _mapSpawn.AISpawnPoints;
        for (int i = 0; i < 10; i++)
        {
            Transform _aiSpawn = spawnPoints[i % spawnPoints.Length];
            Instantiate(_aiPrefab, _aiSpawn.position, _aiSpawn.rotation);
        }
    }
    
    // 플레이어가 총 맞았을 때 호출
    public void OnPlayerDead()
    {
        if (!IsServer) return;
        
        AlivePlayer.Value--;
    }

    // 슈팅 페이즈
    public IEnumerator ShootingPhase()
    {
        if (!IsServer) yield break;
        if (CurrentPhase.Value != GamePhase.Shooting) yield break;
        
        // ai비활성화
        foreach (var ai in _aiList) ai.Hide();
        yield return null;
        // 애니메이션 재생
        CurrentPhase.Value = GamePhase.HideAndSeek;
        // ai활성화
        foreach (var ai in _aiList) ai.Show();
    }

    // 게임 플레이 루틴
    public IEnumerator GamePlay()
    {
        CurrentPhase.Value = GamePhase.HideAndSeek;
        while (AlivePlayer.Value > 1)
        {
            yield return new WaitForSeconds(_movingTime);
            
            CurrentPhase.Value = GamePhase.Shooting;
            StartCoroutine(ShootingPhase());
            yield return new WaitUntil(() => CurrentPhase.Value != GamePhase.Shooting);
        }
        CurrentPhase.Value = GamePhase.GameOver;
    }
}