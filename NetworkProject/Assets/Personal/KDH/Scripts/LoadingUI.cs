using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _loadingText;

    private bool _isReady = false;

    private void Start()
    {
        StartCoroutine(LoadingProgress());
    }

    private IEnumerator LoadingProgress()
    {
        float progress = 0f;
        _loadingText.text = "로딩 중....";
        while (progress < 0.9f)
        {
            progress += Time.deltaTime * 0.8f;
            UpdateUI(progress);
            yield return null;
        }

        _loadingText.text = "다른 플레이어 대기 중....";
        while (!_isReady) yield return null;

        while (progress < 1f)
        {
            progress += Time.deltaTime * 2f;
            UpdateUI(progress);
            yield return null;
        }
        
        UpdateUI(1f);
        _loadingText.text = "완료!";
    }

    private void UpdateUI(float progress)
    {
        _progressBar.value = progress;
        _progressText.text = $"{(int)(progress * 100)}%";
    }

    public void SetReady()
    {
        _isReady = true;
    }
}
