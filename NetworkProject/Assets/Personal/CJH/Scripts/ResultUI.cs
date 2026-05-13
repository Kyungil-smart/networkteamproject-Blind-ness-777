using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private Button _lobbyButton;
    [SerializeField] private Button _titleButton;

    private void Awake()
    {
        _lobbyButton.onClick.AddListener(OnLobbyClicked);
        _titleButton.onClick.AddListener(OnTitleClicked);
    }

    private void OnDestroy()
    {
        _lobbyButton.onClick.RemoveListener(OnLobbyClicked);
        _titleButton.onClick.RemoveListener(OnTitleClicked);
    }

    private async void OnLobbyClicked()
    {
        _lobbyButton.interactable = false;
        _titleButton.interactable = false;
        await LobbyManager.Instance.ReturnToRoomAsync();
    }

    private async void OnTitleClicked()
    {
        _lobbyButton.interactable = false;
        _titleButton.interactable = false;

        if (LobbyManager.Instance != null)
            await LobbyManager.Instance.LeaveSessionAsync();

        SceneManager.LoadScene("TitleScene");
    }
}