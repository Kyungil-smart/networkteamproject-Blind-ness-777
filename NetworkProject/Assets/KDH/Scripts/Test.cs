using System;
using Unity.Netcode;
using UnityEngine;

// Test용 스크립트
public class Test : NetworkBehaviour
{
    private void Update()
    {
        if (!IsServer) return;
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("호스트접속");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("클라이언트 접속");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            MapLoader mapLoader = FindObjectOfType<MapLoader>();
            mapLoader.DestroyMap();
            SceneLoader.Instance.LoadResultScene();
            Debug.Log("결과창으로 전환");
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneLoader.Instance.LoadLobbyScene();
            Debug.Log("로비씬으로 전환");
        }
        
        if (Input.GetKeyDown(KeyCode.G))
        {
            SceneLoader.Instance.LoadGameScene();
            Debug.Log("[Test] 게임 시작");
            Debug.Log($"{GameManager.Instance.CurrentPhase.Value}");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            GameManager.Instance.OnPlayerDead();
            Debug.Log($"[Test] 플레이어 사망 → 생존자 {GameManager.Instance.AlivePlayer.Value}명");
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            GameManager.Instance.CurrentPhase.Value = GamePhase.GameOver;
        }
    }
}
