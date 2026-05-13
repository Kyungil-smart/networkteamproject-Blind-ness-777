using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ClientConnector : MonoBehaviour
{
    public Button connectButton;

    void Start()
    {
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(ConnectToServer);
        }
    }

    public void ConnectToServer()
    {
        if (NetworkManager.Singleton != null)
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                Debug.Log("클라이언트로 접속 시도 중...");
                NetworkManager.Singleton.StartClient();
            }
        }
        else
        {
            Debug.LogError("NetworkManager가 씬에 없습니다!");
        }
    }
}