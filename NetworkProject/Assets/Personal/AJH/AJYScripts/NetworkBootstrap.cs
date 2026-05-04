using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;

public class NetworkBootstrap : MonoBehaviour
{
    /*
     * 내용 요약 : 호스트, 클라이언트 접속, 연결해제 관리
     * 작성자 : 안정연
     */
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;
    [SerializeField] private Button _nicknameSubmitButton;

    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _joincodeInput;
    
    private bool _isCallbacksBound;
    private bool _isNaming;

    private void Start()
    {
        _isNaming = false;
    }

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
        _nicknameSubmitButton.onClick.AddListener(OnNaming);
    }

    // 버튼 이벤트 구독해제
    private void UnbindButtonEvents()
    {
        _startHostButton.onClick.RemoveListener(StartHost);
        _startClientButton.onClick.RemoveListener(StartClient);
        _nicknameSubmitButton.onClick.RemoveListener(OnNaming);
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

    private async void StartHost()
    {
        // 닉네임이 있어야만 넘어가게
        if (_isNaming)
        {
            await LobbyManager.Instance.CreateSessionAsync(_nicknameInput.text);
            
            SceneLoader.Instance.IndividualLobby();
        }

        else
        {
            Debug.LogError("닉네임을 정해주세요");
        }
    }

    private async void StartClient()
    {
        string joincode = _joincodeInput.text.Trim();
        if (_isNaming && !string.IsNullOrEmpty(joincode))
        {
            await LobbyManager.Instance.JoinSessionByCodeAsync(joincode, _nicknameInput.text);

            SceneLoader.Instance.IndividualLobby();
        }

        else
        {
            Debug.LogError("닉네임을 정해주세요");
        }
    }

    private void OnClientConnected(ulong clientId)  => Debug.Log($"<color=green>[Network] 접속: {clientId}</color>");
    private void OnClientDisconnect(ulong clientId) => Debug.Log($"<color=red>[Network] 해제: {clientId}</color>");
    private void OnServerStarted()                  => Debug.Log("<color=green>[Network] 서버 시작</color>");

    // 닉네임 설정
    private async void OnNaming()
    {
        try
        {
            // 초기화/로그인 상태체크
            if(UnityServices.State != ServicesInitializationState.Initialized ||
               AuthenticationService.Instance.IsSignedIn == false)
                await AuthService.Instance.InitializeAsync();
            
            if(!string.IsNullOrEmpty(_nicknameInput.text))
                _isNaming = true;
            
            else Debug.LogError("사용할 수 없는 닉네임입니다.");
            
        }
        catch (Exception e)
        {
            Debug.LogError($"닉네임 설정 실패 : {e.Message}");
            throw;
        }
    }
}