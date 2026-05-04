using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] public Button copybutton;
    [SerializeField] public Button startReadybutton;
    [SerializeField] private TMP_InputField _joinCode;
    
    [SerializeField] public List<TextMeshProUGUI> lobbyslots;
    
    void Start()
    {
        lobbyslots[0].text = LobbyManager.Instance.CurrentSession.Name;
        _joinCode.text = LobbyManager.Instance.CurrentSession.Code;
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
