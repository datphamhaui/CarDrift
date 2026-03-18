using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Ready, Playing, Paused, GameOver, Win }

    [Header("UI Panels")]
    public GameObject readyPanel;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;

    [Header("HP")]
    public int maxHP = 3;

    [Header("Fuel")]
    [Tooltip("Maximum fuel amount")]
    public float maxFuel = 100f;
    [Tooltip("Fuel consumed per second while driving")]
    public float fuelDrainRate = 5f;

    public GameState CurrentState { get; private set; } = GameState.Ready;
    public bool IsPaused => CurrentState == GameState.Paused;
    public int CurrentHP { get; private set; }
    public float CurrentFuel { get; private set; }

    float _lastFuelNotified = -1f;

    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnGameWin;
    public event Action OnGamePause;
    public event Action OnGameResume;
    public event Action<int, int> OnHPChanged; // (currentHP, maxHP)
    public event Action<float, float> OnFuelChanged; // (currentFuel, maxFuel)

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        SetState(GameState.Ready);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(AudioManager.instance.gameMusic);
    }

    void Update()
    {
        if (CurrentState == GameState.Ready && Input.anyKeyDown)
        {
            StartGame();
        }

        if (CurrentState == GameState.Playing)
        {
            DrainFuel();
        }
    }

    void DrainFuel()
    {
        CurrentFuel -= fuelDrainRate * Time.deltaTime;
        CurrentFuel = Mathf.Max(0f, CurrentFuel);

        // Chỉ notify UI khi fuel thay đổi >= 0.5 unit, hoặc khi về 0
        if (CurrentFuel <= 0f || Mathf.Abs(CurrentFuel - _lastFuelNotified) >= 0.5f)
        {
            _lastFuelNotified = CurrentFuel;
            OnFuelChanged?.Invoke(CurrentFuel, maxFuel);
        }

        if (CurrentFuel <= 0f)
            TriggerGameOver();
    }

    public void AddFuel(float amount)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentFuel = Mathf.Min(maxFuel, CurrentFuel + amount);
        OnFuelChanged?.Invoke(CurrentFuel, maxFuel);
    }

    public void AddHP(int amount = 1)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        OnHPChanged?.Invoke(CurrentHP, maxHP);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        CurrentHP = maxHP;
        CurrentFuel = maxFuel;
        _lastFuelNotified = maxFuel;
        OnHPChanged?.Invoke(CurrentHP, maxHP);
        OnFuelChanged?.Invoke(CurrentFuel, maxFuel);
        SetState(GameState.Playing);
        OnGameStart?.Invoke();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.Paused);
        Time.timeScale = 0f;
        OnGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;

        SetState(GameState.Playing);
        Time.timeScale = 1f;
        OnGameResume?.Invoke();
    }

    public void TakeDamage(int damage = 1)
    {
        if (CurrentState != GameState.Playing) return;

        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        OnHPChanged?.Invoke(CurrentHP, maxHP);

        if (CurrentHP <= 0)
            TriggerGameOver();
    }

    public void TriggerGameOver()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.GameOver);
        OnGameOver?.Invoke();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayLose();

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.StopScoring();
    }

    public void TriggerWin()
    {
        if (CurrentState != GameState.Playing) return;

        SetState(GameState.Win);
        OnGameWin?.Invoke();

        if (AudioManager.instance != null)
            AudioManager.instance.PlayWin();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.StopScoring();
            ScoreManager.Instance.SaveHighScore();
        }

        if (ProgressManager.instance != null)
        {
            // Level number = panelIndex + 1, panelIndex is stored in PlayerPrefs
            int level = PlayerPrefs.GetInt("selected_map", 0) + 1;
            Debug.Log($"[GameManager] TriggerWin: selected_map={level - 1}, level={level}, completing -> unlock level {level + 1}");
            ProgressManager.instance.CompleteLevel(level);
            Debug.Log($"[GameManager] After CompleteLevel: unlocked_level={ProgressManager.instance.GetUnlockedLevel()}");
        }
        else
        {
            Debug.LogWarning("[GameManager] TriggerWin: ProgressManager.instance is NULL!");
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextIndex);
        else
            LoadSelectScene();
    }

    public void LoadSelectScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SelectScene");
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    void SetState(GameState newState)
    {
        CurrentState = newState;

        if (readyPanel != null)    readyPanel.SetActive(newState == GameState.Ready);
        if (hudPanel != null)      hudPanel.SetActive(newState == GameState.Playing);
        if (pausePanel != null)    pausePanel.SetActive(newState == GameState.Paused);
        if (gameOverPanel != null) gameOverPanel.SetActive(newState == GameState.GameOver);
        if (winPanel != null)      winPanel.SetActive(newState == GameState.Win);
    }
}
