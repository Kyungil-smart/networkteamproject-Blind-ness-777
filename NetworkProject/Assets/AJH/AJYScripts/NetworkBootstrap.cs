using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkBootstrap : MonoBehaviour
{
    /*
     * 내용 요약 : 호스트, 클라이언트 접속, 연결해제 관리
     * 작성자 : 안정연
     */
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;
    //[SerializeField] private Button _disconnectButton;

    private bool _isCallbacksBound;

    private void OnEnable()
    {
        BindNetworkCallbacks();
        BindButtonEvents();
    }

    private void OnDisable()
    {
        UnbindNetworkCallbacks();
        UnbindButtonEvents();
    }

    // 버튼 이벤트 구독
    private void BindButtonEvents()
    {
        _startHostButton.onClick.AddListener(StartHost);
        _startClientButton.onClick.AddListener(StartClient);
        //_disconnectButton.onClick.AddListener(Disconnect);
    }

    // 버튼 이벤트 구독해제
    private void UnbindButtonEvents()
    {
        _startHostButton.onClick.RemoveListener(StartHost);
        _startClientButton.onClick.RemoveListener(StartClient);
        //_disconnectButton.onClick.RemoveListener(Disconnect);
    }

    // 클라이언트 접속/해제, 서버 호스트 콜백이벤트 구독
    private void BindNetworkCallbacks()
    {
        if (_isCallbacksBound) return;
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted            += OnServerStarted;
        _isCallbacksBound = true;
    }
    
    // 클라이언트 접속/해제, 서버 호스트 콜백이벤트 구독해제
    private void UnbindNetworkCallbacks()
    {
        if (!_isCallbacksBound) return;
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback  -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        NetworkManager.Singleton.OnServerStarted            -= OnServerStarted;
        _isCallbacksBound = false;
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        SceneLoader.Instance.IndividualLobby();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        SceneLoader.Instance.IndividualLobby();
    }

    private void Disconnect()  => NetworkManager.Singleton.Shutdown();

    private void OnClientConnected(ulong clientId)  => Debug.Log($"<color=green>[Network] 접속: {clientId}</color>");
    private void OnClientDisconnect(ulong clientId) => Debug.Log($"<color=red>[Network] 해제: {clientId}</color>");
    private void OnServerStarted()                  => Debug.Log("<color=green>[Network] 서버 시작</color>");
}