using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable settings popup. Supports two UI modes (assign whichever you use):
///   - Slider mode:  drag musicSlider / sfxSlider to control volume (0-1)
///   - Switch mode:  tap musicToggle / sfxToggle to turn on/off
/// Both modes can coexist on the same panel. Leave unused fields empty.
/// </summary>
public class SettingsPopup : MonoBehaviour
{
    [Header("Open / Close")]
    public Button openButton;
    public Button closeButton;
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

    float savedTimeScale = 1f;

    void Start()
    {
        if (openButton != null)  openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        // Sliders
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

        // Switch buttons
        if (musicToggle != null) musicToggle.onClick.AddListener(OnMusicToggle);
        if (sfxToggle != null)   sfxToggle.onClick.AddListener(OnSfxToggle);

        if (settingsPanel != null) settingsPanel.SetActive(false);

        UpdateVisuals();
    }

    // ─── Open / Close ───

    public void Open()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        UpdateVisuals();
    }

    public void Close()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = savedTimeScale;
    }

    // ─── Slider callbacks ───

    void OnMusicSliderChanged(float value)
    {
        if (AudioManager.instance == null) return;
        AudioManager.instance.SetMusicVolume(value);

        // Auto-sync switch visuals when slider hits 0 or leaves 0
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

    // ─── Switch callbacks ───

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

    // ─── Visual sync ───

    void UpdateVisuals()
    {
        if (AudioManager.instance == null) return;

        // Sliders
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
