using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    public GameObject settingsPanel;

    [Header("Slider Mode (volume)")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Switch Mode (on/off)")]
    public Button musicToggle;
    public Button sfxToggle;
    public GameObject musicOnState;
    public GameObject musicOffState;
    public GameObject sfxOnState;
    public GameObject sfxOffState;

    public static bool IsOpen { get; private set; }

    float savedTimeScale = 1f;

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }
        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }

        if (musicToggle != null) musicToggle.onClick.AddListener(OnMusicToggle);
        if (sfxToggle != null)   sfxToggle.onClick.AddListener(OnSfxToggle);

        UpdateVisuals();
    }

    public void Open()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayButtonClick();
        if (settingsPanel != null) settingsPanel.SetActive(true);
        IsOpen = true;
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        UpdateVisuals();
    }

    public void Close()
    {
        if (AudioManager.instance != null) AudioManager.instance.PlayButtonClick();
        if (settingsPanel != null) settingsPanel.SetActive(false);
        IsOpen = false;
        Time.timeScale = savedTimeScale;
    }

    void OnMusicSliderChanged(float value)
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.SetMusicVolume(value);
        if (AudioManager.instance.IsMusicOn && value <= 0f)
            AudioManager.instance.SetMusic(false);
        else if (!AudioManager.instance.IsMusicOn && value > 0f)
            AudioManager.instance.SetMusic(true);
        UpdateToggleVisuals();
    }

    void OnSfxSliderChanged(float value)
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.SetSfxVolume(value);
        if (AudioManager.instance.IsSfxOn && value <= 0f)
            AudioManager.instance.SetSfx(false);
        else if (!AudioManager.instance.IsSfxOn && value > 0f)
            AudioManager.instance.SetSfx(true);
        UpdateToggleVisuals();
    }

    void OnMusicToggle()
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.ToggleMusic();
        AudioManager.instance.PlayButtonClick();
        UpdateVisuals();
    }

    void OnSfxToggle()
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.ToggleSfx();
        AudioManager.instance.PlayButtonClick();
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (AudioManager.instance == null) return;
        if (musicSlider != null) musicSlider.SetValueWithoutNotify(AudioManager.instance.MusicVolume);
        if (sfxSlider != null)   sfxSlider.SetValueWithoutNotify(AudioManager.instance.SfxVolume);
        UpdateToggleVisuals();
    }

    void UpdateToggleVisuals()
    {
        if (AudioManager.instance == null) return;
        bool musicOn = AudioManager.instance.IsMusicOn;
        bool sfxOn   = AudioManager.instance.IsSfxOn;
        if (musicOnState != null)  musicOnState.SetActive(musicOn);
        if (musicOffState != null) musicOffState.SetActive(!musicOn);
        if (sfxOnState != null)    sfxOnState.SetActive(sfxOn);
        if (sfxOffState != null)   sfxOffState.SetActive(!sfxOn);
    }
}
