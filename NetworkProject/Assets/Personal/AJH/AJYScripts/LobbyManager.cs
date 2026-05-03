using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    /*
     * 내용 : 로비에서 이루어지는 로직 관리
     */
    [SerializeField] public Button copybutton;
    [SerializeField] private TMP_InputField _joinCode;
    
    [SerializeField] public List<TextMeshProUGUI> lobbyslots;

    private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            _joinCode.text = HostManager.Instance.JoinCodes[AuthenticationService.Instance.PlayerId];
            lobbyslots[0].text = HostManager.Instance.HostStorages[_joinCode.text].Hostname;
        }

        if (NetworkManager.Singleton.IsClient)
        {
            
        }
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
