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
    [Tooltip("SpeedometerController: ring = speed, gas slider = Fuel")]
    public SpeedometerController speedometerUI;
    [Tooltip("Max speed for normalizing the speedometer ring (km/h)")]
    public float maxSpeed = 200f;

    [Header("HP Display (Slider)")]
    public Slider hpSlider;

    [Header("Fuel Display (Slider - optional fallback)")]
    public Slider fuelSlider;

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
    int _lastSpeedInt = -1;

    void SetupHP()
    {
        if (GameManager.Instance == null) return;

        carController = FindAnyObjectByType<PrometeoCarController>();

        // Speedometer: ring = speed, gas slider = Fuel bar
        if (speedometerUI != null)
        {
            speedometerUI.isGasSliderVisible = true;
            speedometerUI.SpeedometerSliderValue = 0f;

            if (speedometerUI.currentSpeedText != null)
                speedometerUI.currentSpeedText.text = "0";
            if (speedometerUI.currentGearText != null)
                speedometerUI.currentGearText.text = "N";

            UpdateGasFuel(GameManager.Instance.maxFuel, GameManager.Instance.maxFuel);

            GameManager.Instance.OnFuelChanged += (current, max) =>
                UpdateGasFuel(current, max);

            GameManager.Instance.OnGameStart += () =>
            {
                if (speedometerUI != null && speedometerUI.currentGearText != null)
                    speedometerUI.currentGearText.text = "D";
            };
        }

        // HP slider
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

        // Fuel slider (optional fallback)
        if (fuelSlider != null)
        {
            fuelSlider.minValue = 0;
            fuelSlider.maxValue = GameManager.Instance.maxFuel;
            fuelSlider.value = GameManager.Instance.maxFuel;
            fuelSlider.interactable = false;

            GameManager.Instance.OnFuelChanged += (current, max) =>
            {
                if (fuelSlider != null) fuelSlider.value = current;
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
        {
            int speedInt = Mathf.RoundToInt(speed);
            if (speedInt != _lastSpeedInt)
            {
                _lastSpeedInt = speedInt;
                speedometerUI.currentSpeedText.text = speedInt.ToString();
            }
        }
    }

    void UpdateGasFuel(float current, float max)
    {
        if (speedometerUI == null) return;

        float ratio = max > 0f ? current / max : 0f;
        speedometerUI.GasSliderValue = ratio;

        // Change gas slider color: green → red based on fuel level
        Color fuelColor = Color.Lerp(Color.red, Color.green, ratio);
        speedometerUI.gasSliderColor = fuelColor;
        if (speedometerUI.gasSliderImage != null)
            speedometerUI.gasSliderImage.color = fuelColor;
        if (speedometerUI.gasIcon != null)
            speedometerUI.gasIcon.color = fuelColor;
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
