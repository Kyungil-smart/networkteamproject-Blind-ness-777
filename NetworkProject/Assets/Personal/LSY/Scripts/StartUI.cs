using UnityEngine;
using UnityEngine.UI;

public class StartUI : MonoBehaviour
{
    [SerializeField] private Button _closeButton;

    private void Awake()
    {
        _closeButton.onClick.AddListener(OnCloseClicked);
    }

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveListener(OnCloseClicked);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }
}