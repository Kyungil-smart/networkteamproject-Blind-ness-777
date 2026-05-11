using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using System.Linq;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public NetworkVariable<GamePhase> CurrentPhase = new(GamePhase.Waiting);    // 게임 페이즈 변수
    public NetworkVariable<int> AlivePlayer = new(0);                           // 플레이 인원수 변수
    public MapLoader _mapSpawn;
    [SerializeField] public float _movingTime = 10f;                            // 숨는 시간 변수
    
    private AISetActive[] _aiList;

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
    
    public override void OnNetworkSpawn()
    {
        CurrentPhase.OnValueChanged += OnPhaseValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        CurrentPhase.OnValueChanged -= OnPhaseValueChanged;
    }

    private void OnPhaseValueChanged(GamePhase previous, GamePhase current)
    {
        if (!IsServer) 
        {
            foreach (var obj in FindObjectsOfType<MonoBehaviour>().OfType<IPhaseChangeable>())
                obj.OnPhaseChanged(current);
            return;
        }

        switch (current)
        {
            case GamePhase.Shooting:
                if (_aiList != null)
                    foreach (var ai in _aiList) ai.Hide();
                break;

            case GamePhase.HideAndSeek:
                if (_aiList != null)
                    foreach (var ai in _aiList) ai.Show();
                break;
        }

        foreach (var obj in FindObjectsOfType<MonoBehaviour>().OfType<IPhaseChangeable>())
            obj.OnPhaseChanged(current);
    }
    
    // 게임 시작 함수
    public void StartGame()
    {
        if (!IsServer) return;
        AlivePlayer.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        _mapSpawn = FindObjectOfType<MapLoader>();
        if (_mapSpawn != null) _mapSpawn.LoadMap();
        // ai소환
        FindObjectOfType<SpawnManager>().SpawnAll();
        // ai저장
        _aiList = FindObjectsOfType<AISetActive>();
        Debug.Log(_aiList.Length);
        StartCoroutine(GamePlay());
    }
    
    // 플레이어가 총 맞았을 때 호출
    public void OnPlayerDead()
    {
        if (!IsServer) return;
        
        AlivePlayer.Value--;
    }

    // 게임 플레이 루틴
    public IEnumerator GamePlay()
    {
        CurrentPhase.Value = GamePhase.HideAndSeek;
        while (AlivePlayer.Value > 1)
        {
            yield return new WaitUntil(() => CurrentPhase.Value == GamePhase.Shooting);
            yield return new WaitUntil(() => CurrentPhase.Value != GamePhase.Shooting);
        }
        foreach (var ai in _aiList) ai.AIDestroy();
        _aiList = null;
        CurrentPhase.Value = GamePhase.GameOver;
    }
}