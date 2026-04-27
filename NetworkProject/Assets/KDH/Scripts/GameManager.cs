using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public NetworkVariable<GamePhase> CurrentPhase = new(GamePhase.Waiting);    // 게임 페이즈 변수
    public NetworkVariable<int> AlivePlayer = new(0);                           // 플레이 인원수 변수
    [SerializeField] public float _movingTime = 10f;                            // 숨는 시간 변수
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
        StartCoroutine(GamePlay());
    }

    // 플레이어 죽을때마다 인원 수 감소
    public void OnPlayerDead()
    {
        if (!IsServer) return;
        
        AlivePlayer.Value--;
        
        if (AlivePlayer.Value <= 1)
            CurrentPhase.Value = GamePhase.GameOver;
    }

    // 숨는 페이즈에서 총쏘는 페이즈로 갔을 시 호출
    public void ShootingPhase()
    {
        if (!IsServer) return;
        if (CurrentPhase.Value != GamePhase.Shooting) return;

        CurrentPhase.Value = GamePhase.HideAndSeek;
    }

    // 게임 플레이 루틴
    public IEnumerator GamePlay()
    {
        while (AlivePlayer.Value > 1)
        {
            CurrentPhase.Value = GamePhase.HideAndSeek;
            yield return new WaitForSeconds(_movingTime);
            
            CurrentPhase.Value = GamePhase.Shooting;
            yield return new WaitUntil(() => CurrentPhase.Value != GamePhase.Shooting);
        }
        CurrentPhase.Value = GamePhase.GameOver;
    }
}