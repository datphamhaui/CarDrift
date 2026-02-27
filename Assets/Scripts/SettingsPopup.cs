using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [Header("Buttons")]
    public Button openButton;
    public Button closeButton;
    public Button musicToggle;
    public Button sfxToggle;

    [Header("Toggle Visuals")]
    public GameObject musicOnState;
    public GameObject musicOffState;
    public GameObject sfxOnState;
    public GameObject sfxOffState;

    [Header("Panel")]
    public GameObject settingsPanel;

    void Start()
    {
        if (openButton != null)  openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (musicToggle != null) musicToggle.onClick.AddListener(OnMusicToggle);
        if (sfxToggle != null)   sfxToggle.onClick.AddListener(OnSfxToggle);

        if (settingsPanel != null) settingsPanel.SetActive(false);

        UpdateVisuals();
    }

    public void Open()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        Time.timeScale = 0f;
        UpdateVisuals();
    }

    public void Close()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1f;
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

        bool musicOn = AudioManager.instance.IsMusicOn;
        bool sfxOn   = AudioManager.instance.IsSfxOn;

        if (musicOnState != null)  musicOnState.SetActive(musicOn);
        if (musicOffState != null) musicOffState.SetActive(!musicOn);
        if (sfxOnState != null)    sfxOnState.SetActive(sfxOn);
        if (sfxOffState != null)   sfxOffState.SetActive(!sfxOn);
    }
}
