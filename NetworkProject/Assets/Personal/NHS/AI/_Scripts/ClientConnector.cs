using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI; // UI 사용을 위해 필수

public class ClientConnector : MonoBehaviour
{
    // 유니티 인스펙터에서 버튼을 드래그해서 연결할 변수
    public Button connectButton;

    void Start()
    {
        if (connectButton != null)
        {
            // 버튼 클릭 시 ConnectToServer 함수가 실행되도록 등록
            connectButton.onClick.AddListener(ConnectToServer);
        }
    }

    public void ConnectToServer()
    {
        if (NetworkManager.Singleton != null)
        {
            // 이미 실행 중인지 확인 후 클라이언트 시작
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