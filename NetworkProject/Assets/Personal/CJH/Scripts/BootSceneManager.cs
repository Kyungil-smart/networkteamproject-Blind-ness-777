using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    [Header("Logos")]
    [SerializeField] private Image _logo1;
    [SerializeField] private Image _logo2;

    [Header("Timing")]
    [SerializeField] private float _fadeInDuration  = 0.5f;
    [SerializeField] private float _holdDuration    = 1f;
    [SerializeField] private float _fadeOutDuration = 0.5f;

    [Header("Scene")]
    [SerializeField] private string _nextSceneName = "TitleScene";

    private void Start()
    {
        SetAlpha(_logo1, 0f);
        SetAlpha(_logo2, 0f);

        StartCoroutine(PlayBootSequence());
    }

    private IEnumerator PlayBootSequence()
    {
        yield return new WaitForSeconds(0.5f);
        
        AudioManager.Instance?.PlayBootSFX();
        
        yield return StartCoroutine(ShowLogo(_logo1));
        yield return StartCoroutine(ShowLogo(_logo2));
        
        AudioManager.Instance?.PlayTitleBGM();
        
        SceneManager.LoadScene(_nextSceneName);
    }

    private IEnumerator ShowLogo(Image logo)
    {
        Debug.Log($"[Boot] {logo.name} 페이드인 시작");
        yield return StartCoroutine(Fade(logo, 0f, 1f, _fadeInDuration));
        Debug.Log($"[Boot] {logo.name} 홀드 시작");
        yield return new WaitForSeconds(_holdDuration);
        Debug.Log($"[Boot] {logo.name} 페이드아웃 시작");
        yield return StartCoroutine(Fade(logo, 1f, 0f, _fadeOutDuration));
        Debug.Log($"[Boot] {logo.name} 완료");
    }

    private IEnumerator Fade(Image logo, float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(logo, Mathf.Lerp(from, to, elapsed / duration));
            yield return null;
        }

        SetAlpha(logo, to);
    }

    private void SetAlpha(Image logo, float alpha)
    {
        if (logo == null) return;
        Color color = logo.color;
        color.a    = alpha;
        logo.color = color;
    }
}