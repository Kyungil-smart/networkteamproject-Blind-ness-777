using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EscUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _closeButton;

    [Header("팝업")]
    [SerializeField] private GameObject _optionPopup;

    private void Awake()
    {
        _optionButton.onClick.AddListener(OnOptionClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
        _closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        _optionButton.onClick.RemoveListener(OnOptionClicked);
        _quitButton.onClick.RemoveListener(OnQuitClicked);
        _closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void OnOptionClicked()
    {
        _optionPopup.SetActive(true);
    }

    private async void OnQuitClicked()
    {
        _quitButton.interactable = false;

        if (LobbyManager.Instance != null)
            await LobbyManager.Instance.LeaveSessionAsync();

        SceneManager.LoadScene("TitleScene");
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }
}