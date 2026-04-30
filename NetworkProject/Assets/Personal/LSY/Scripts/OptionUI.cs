using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionUI : MonoBehaviour
{
    [Header("슬라이더")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
 
    [Header("볼륨 수치 텍스트")]
    [SerializeField] private TextMeshProUGUI _bgmValueText;
    [SerializeField] private TextMeshProUGUI _sfxValueText;
 
    [Header("닫기 버튼")]
    [SerializeField] private Button _closeButton;
 
    private void Awake()
    {
        _bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        _closeButton.onClick.AddListener(OnCloseClicked);
    }
 
    private void OnDestroy()
    {
        _bgmSlider.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        _sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        _closeButton.onClick.RemoveListener(OnCloseClicked);
    }
 
    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;
 
        float bgm = AudioManager.Instance.GetBGMVolume();
        float sfx = AudioManager.Instance.GetSFXVolume();
 
        _bgmSlider.value = bgm;
        _sfxSlider.value = sfx;
 
        UpdateBGMText(bgm);
        UpdateSFXText(sfx);
    }
 
    private void OnBGMVolumeChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.SetBGMVolume(value);
        UpdateBGMText(value);
    }
 
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.SetSFXVolume(value);
        UpdateSFXText(value);
    }
 
    private void UpdateBGMText(float value)
    {
        if (_bgmValueText != null)
            _bgmValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }
 
    private void UpdateSFXText(float value)
    {
        if (_sfxValueText != null)
            _sfxValueText.text = Mathf.RoundToInt(value * 100) + "%";
    }
 
    private void OnCloseClicked()
    {
        gameObject.SetActive(false);
    }
}