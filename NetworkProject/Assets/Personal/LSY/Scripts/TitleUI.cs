using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionButton;
    [SerializeField] private Button _creditButton;
    [SerializeField] private Button _quitButton;

    [Header("팝업")]
    [SerializeField] private GameObject _startPopup;
    [SerializeField] private GameObject _soundOptionPopup;
    [SerializeField] private GameObject _creditPopup;

    private void Awake()
    {
        _startButton.onClick.AddListener(OnStartClicked);
        _optionButton.onClick.AddListener(OnOptionClicked);
        _creditButton.onClick.AddListener(OnCreditClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnDestroy()
    {
        _startButton.onClick.RemoveListener(OnStartClicked);
        _optionButton.onClick.RemoveListener(OnOptionClicked);
        _creditButton.onClick.RemoveListener(OnCreditClicked);
        _quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    private void OnStartClicked()
    {
        _startPopup.SetActive(true);
    }

    private void OnOptionClicked()
    {
        _soundOptionPopup.SetActive(true);
    }
    
    private void OnCreditClicked()
    {
        _creditPopup.SetActive(true);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}