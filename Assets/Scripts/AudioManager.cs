using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("SFX Clips")]
    public AudioClip buttonClick;
    public AudioClip winSfx;
    public AudioClip loseSfx;
    public AudioClip driftSfx;

    const string MUSIC_KEY = "music_enabled";
    const string SFX_KEY   = "sfx_enabled";
    const string MUSIC_VOL_KEY = "music_volume";
    const string SFX_VOL_KEY   = "sfx_volume";

    public bool IsMusicOn { get; private set; }
    public bool IsSfxOn   { get; private set; }
    public float MusicVolume { get; private set; }
    public float SfxVolume   { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        IsMusicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        IsSfxOn   = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.5f);
        SfxVolume   = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        ApplyMusicState();
    }

    // --- Music ---

    public void ToggleMusic()
    {
        IsMusicOn = !IsMusicOn;
        PlayerPrefs.SetInt(MUSIC_KEY, IsMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicState();
    }

    public void SetMusic(bool on)
    {
        IsMusicOn = on;
        PlayerPrefs.SetInt(MUSIC_KEY, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicState();
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
        ApplyMusicState();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, MusicVolume);
        PlayerPrefs.Save();
        ApplyMusicState();
    }

    void ApplyMusicState()
    {
        if (musicSource != null)
        {
            musicSource.mute = !IsMusicOn;
            musicSource.volume = MusicVolume;
        }
    }

    // --- SFX ---

    public void ToggleSfx()
    {
        IsSfxOn = !IsSfxOn;
        PlayerPrefs.SetInt(SFX_KEY, IsSfxOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSfx(bool on)
    {
        IsSfxOn = on;
        PlayerPrefs.SetInt(SFX_KEY, on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, SfxVolume);
        PlayerPrefs.Save();
        if (sfxSource != null) sfxSource.volume = SfxVolume;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null || !IsSfxOn) return;
        sfxSource.PlayOneShot(clip, SfxVolume);
    }

    // --- Shortcut methods ---

    public void PlayButtonClick() => PlaySfx(buttonClick);
    public void PlayWin()         => PlaySfx(winSfx);
    public void PlayLose()        => PlaySfx(loseSfx);
    public void PlayDrift()       => PlaySfx(driftSfx);
}
