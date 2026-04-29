using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    /*
     * 내용 : 로비에서 이루어지는 로직 관리
     */
    [SerializeField] public Button copybutton;
    [SerializeField] private TMP_InputField _joinCode;

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
            _joinCode.text = HostManager.Instance.JoinCodes[AuthenticationService.Instance.PlayerId];
        
    }

    private void OnEnable()
    {
        copybutton.onClick.AddListener(OnCopyClick);
    }

    private void OnDisable()
    {
        copybutton.onClick.RemoveListener(OnCopyClick);
    }

    private void OnCopyClick()
    {
        GUIUtility.systemCopyBuffer = _joinCode.text;
    }
}
