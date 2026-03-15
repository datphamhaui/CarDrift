using UnityEngine;
using UnityEngine.UI;
using CarUICompletePack;

/// <summary>
/// Wires all game UI buttons to GameManager actions.
/// Attach to Canvas or any GO in the game scene.
///
/// Setup:
///   HUD Panel:      pauseButton, speedometerHP (SpeedometerController)
///   Pause Panel:    continueButton, pauseRestartButton, quitButton
///   GameOver Panel: gameOverHomeButton, gameOverRestartButton
///   Win Panel:      winHomeButton, nextLevelButton
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public Button pauseButton;

    [Header("HP & Speed Display (Speedometer)")]
    [Tooltip("SpeedometerController: ring = speed, gas slider = HP")]
    public SpeedometerController speedometerUI;
    [Tooltip("Max speed for normalizing the speedometer ring (km/h)")]
    public float maxSpeed = 200f;

    [Header("HP Display (Legacy Slider - optional)")]
    public Slider hpSlider;

    [Header("Pause Panel")]
    public Button continueButton;
    public Button pauseRestartButton;
    public Button quitButton;

    [Header("Game Over Panel")]
    public Button gameOverHomeButton;
    public Button gameOverRestartButton;

    [Header("Win Panel")]
    public Button winHomeButton;
    public Button nextLevelButton;

    void Start()
    {
        // HUD
        Wire(pauseButton, () => GameManager.Instance.PauseGame());
        SetupHP();

        // Pause
        Wire(continueButton,    () => GameManager.Instance.ResumeGame());
        Wire(pauseRestartButton, () => GameManager.Instance.RestartLevel());
        Wire(quitButton,         () => GameManager.Instance.LoadSelectScene());

        // Game Over
        Wire(gameOverHomeButton,    () => GameManager.Instance.LoadMainMenu());
        Wire(gameOverRestartButton, () => GameManager.Instance.RestartLevel());

        // Win
        Wire(winHomeButton,   () => GameManager.Instance.LoadMainMenu());
        Wire(nextLevelButton,  () => GameManager.Instance.LoadNextLevel());
    }

    PrometeoCarController carController;

    void SetupHP()
    {
        if (GameManager.Instance == null) return;

        carController = FindAnyObjectByType<PrometeoCarController>();

        // Speedometer: ring = speed, gas slider = HP bar
        if (speedometerUI != null)
        {
            speedometerUI.isGasSliderVisible = true;
            speedometerUI.SpeedometerSliderValue = 0f;

            if (speedometerUI.currentSpeedText != null)
                speedometerUI.currentSpeedText.text = "0";
            if (speedometerUI.currentGearText != null)
                speedometerUI.currentGearText.text = "N";

            UpdateGasHP(GameManager.Instance.maxHP, GameManager.Instance.maxHP);

            GameManager.Instance.OnHPChanged += (current, max) =>
                UpdateGasHP(current, max);

            GameManager.Instance.OnGameStart += () =>
            {
                if (speedometerUI != null && speedometerUI.currentGearText != null)
                    speedometerUI.currentGearText.text = "D";
            };
        }

        // Legacy slider fallback
        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = GameManager.Instance.maxHP;
            hpSlider.value = GameManager.Instance.maxHP;
            hpSlider.interactable = false;

            GameManager.Instance.OnHPChanged += (current, max) =>
            {
                if (hpSlider != null) hpSlider.value = current;
            };
        }
    }

    void Update()
    {
        if (speedometerUI == null || carController == null) return;
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        // Ring = car speed
        float speed = carController.carSpeed;
        speedometerUI.SpeedometerSliderValue = Mathf.Clamp01(speed / maxSpeed);

        if (speedometerUI.currentSpeedText != null)
            speedometerUI.currentSpeedText.text = Mathf.RoundToInt(speed).ToString();
    }

    void UpdateGasHP(int current, int max)
    {
        if (speedometerUI == null) return;

        float ratio = max > 0 ? (float)current / max : 0f;
        speedometerUI.GasSliderValue = ratio;

        // Change gas slider color: green → red based on HP
        Color hpColor = Color.Lerp(Color.red, Color.green, ratio);
        speedometerUI.gasSliderColor = hpColor;
        if (speedometerUI.gasSliderImage != null)
            speedometerUI.gasSliderImage.color = hpColor;
        if (speedometerUI.gasIcon != null)
            speedometerUI.gasIcon.color = hpColor;
    }

    void Wire(Button btn, UnityEngine.Events.UnityAction action)
    {
        if (btn == null) return;
        btn.onClick.AddListener(() =>
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlayButtonClick();
            action();
        });
    }
}
