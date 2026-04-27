using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}
    public NetworkVariable<GamePhase> CurrentPhase = new(GamePhase.Waiting);
    public NetworkVariable<int> AlivePlayer = new(0);
    [SerializeField] public float _movingTime = 10f;
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

    public void StartGame()
    {
        if (!IsServer) return;
        AlivePlayer.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        StartCoroutine(GamePlay());
    }

    public void OnPlayerDead()
    {
        if (!IsServer) return;
        
        AlivePlayer.Value--;
        
        if (AlivePlayer.Value <= 1)
            CurrentPhase.Value = GamePhase.GameOver;
    }

    public void ShootingPhase()
    {
        if (!IsServer) return;
        if (CurrentPhase.Value != GamePhase.Shooting) return;

        CurrentPhase.Value = GamePhase.HideAndSeek;
    }

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