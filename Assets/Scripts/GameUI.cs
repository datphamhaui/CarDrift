using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires all game UI buttons to GameManager actions.
/// Attach to Canvas or any GO in the game scene.
///
/// Setup:
///   HUD Panel:      pauseButton
///   Pause Panel:    continueButton, pauseRestartButton, quitButton
///   GameOver Panel: gameOverHomeButton, gameOverRestartButton
///   Win Panel:      winHomeButton, nextLevelButton
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public Button pauseButton;
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
        SetupHPSlider();

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

    void SetupHPSlider()
    {
        if (hpSlider == null || GameManager.Instance == null) return;

        hpSlider.minValue = 0;
        hpSlider.maxValue = GameManager.Instance.maxHP;
        hpSlider.value = GameManager.Instance.maxHP;
        hpSlider.interactable = false;

        GameManager.Instance.OnHPChanged += (current, max) =>
        {
            if (hpSlider != null) hpSlider.value = current;
        };
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
