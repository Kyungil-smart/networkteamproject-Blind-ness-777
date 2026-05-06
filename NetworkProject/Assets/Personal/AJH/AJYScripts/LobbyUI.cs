using System.Collections.Generic;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] public Button copybutton;
    [SerializeField] public Button startReadybutton;
    [SerializeField] private TMP_Text _readyButtonLabel;
    // 방에서 나가는 버튼
    [SerializeField] private Button _leaveButton;
    
    [SerializeField] private TMP_InputField _joinCode;
    
    [SerializeField] private List<RoomPlayerSlotUI> _playerSlots = new List<RoomPlayerSlotUI>();
    
    private bool _isLocalPlayerReady;
    private bool _isProcessingReady;
    
    private void OnEnable()
    {
        BindEvents();
        ResetInteractables();
        if (LobbyManager.Instance.CurrentSession != null)
        {
            Refresh(LobbyManager.Instance.CurrentSession);
        }
    }

    private void OnDisable()
    {
        UnbindEvents();
    }
    
    private void BindEvents()
    {
        copybutton.onClick.AddListener(OnCopyClick);
        startReadybutton.onClick.AddListener(OnStartReadyButtonClick);
        BindLobbyManagerEvents();
    }

    private void UnbindEvents()
    {
        copybutton.onClick.RemoveListener(OnCopyClick);
        startReadybutton.onClick.RemoveListener(OnStartReadyButtonClick);
        UnbindLobbyManagerEvents();
    }
    
    private void BindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated += Refresh;
        LobbyManager.Instance.OnGameStarting += OnGameStarting;
        LobbyManager.Instance.OnRestartCooldownEnded += RefreshReadyButton;
    }

    private void UnbindLobbyManagerEvents()
    {
        LobbyManager.Instance.OnSessionUpdated -= Refresh;
        LobbyManager.Instance.OnGameStarting -= OnGameStarting;
        LobbyManager.Instance.OnRestartCooldownEnded -= RefreshReadyButton;
    }
    
    private void Refresh(ISession session)
    {
        if (session == null) return;
        _joinCode.text = session.Code;
        RefreshPlayerSlots(session);
        RefreshReadyButton();
        //RefreshStatusText(session);
    }
    
    private void RefreshPlayerSlots(ISession session)
    {
        bool isLocalHost = session.CurrentPlayer != null && session.CurrentPlayer.Id == session.Host;
        if (!isLocalHost && session.CurrentPlayer != null)
        {
            string readyValue = LobbyManager.GetPlayerProperty(session.CurrentPlayer, LobbyConstants.KEY_PLAYER_READY);
            _isLocalPlayerReady = readyValue == LobbyConstants.VALUE_TRUE;
        }
        else
        {
            _isLocalPlayerReady = false;
        }

        for (int i = 0; i < _playerSlots.Count; i++)
        {
            if (i < session.Players.Count)
            {
                ApplyPlayerToSlot(session, i);
            }
            else
            {
                _playerSlots[i].SetEmpty();
            }
        }
    }

    private void ApplyPlayerToSlot(ISession session, int index)
    {
        IReadOnlyPlayer player = session.Players[index];
        string playerName = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_NAME) ?? "Player";
        string readyValue = LobbyManager.GetPlayerProperty(player, LobbyConstants.KEY_PLAYER_READY);
        bool isReady = readyValue == LobbyConstants.VALUE_TRUE;
        bool isHost = player.Id == session.Host;
        _playerSlots[index].SetPlayer(playerName, isReady, isHost);
    }
    
    private void RefreshReadyButton()
    {
        bool isHost = LobbyManager.Instance.IsHost;

        _readyButtonLabel.text = isHost
            ? "GameStart"
            : (_isLocalPlayerReady ? "CancelReady" : "Ready");

        if (!_isProcessingReady)
        {
            startReadybutton.interactable = !isHost || LobbyManager.Instance.CanHostStartGame;
        }
        _leaveButton.interactable = true;
    }
    
    private void ResetInteractables()
    {
        // 이전 진입에서 OnLeaveClicked / OnGameStarting에 의해 false로 남아있을 수 있어 재진입 시 복구
        startReadybutton.interactable = true;
        //_leaveButton.interactable = true;
        _isProcessingReady = false;
        _isLocalPlayerReady = false;
    }
    
    private void OnCopyClick()
    {
        GUIUtility.systemCopyBuffer = _joinCode.text;
    }

    private async void OnStartReadyButtonClick()
    {
        if (_isProcessingReady) return;
        
        _isProcessingReady = true;
        startReadybutton.interactable = false;
        
        try
        {
            if (LobbyManager.Instance.IsHost)
            {
                await LobbyManager.Instance.TryStartGameAsHostAsync();
            }
            else
            {
                await LobbyManager.Instance.SetReadyAsync(!_isLocalPlayerReady);
            }
        }
        
        finally
        {
            _isProcessingReady = false;
            RefreshReadyButton();
        }
    }
    
    private void OnGameStarting()
    {
        //_statusText.text = "게임에 입장합니다...";
        startReadybutton.interactable = false;
        //_leaveButton.interactable = false;
    }
}
