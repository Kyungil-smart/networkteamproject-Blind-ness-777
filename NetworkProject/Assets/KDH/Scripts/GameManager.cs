using System;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; private set;}

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
    }

    public void OnPlayerDead()
    {
        if (!IsServer) return;
    }

    public void ShootingPhase()
    {
        
    }
}
