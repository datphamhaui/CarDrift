using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Wires all game UI buttons to GameManager actions.
/// Attach to Canvas or any GO in the game scene.
///
/// Setup:
///   HUD Panel:      pauseButton
///   Pause Panel:    resumeButton, pauseRestartButton, pauseSelectButton, pauseHomeButton
///   GameOver Panel: gameOverRestartButton, gameOverSelectButton, gameOverHomeButton
///   Win Panel:      nextLevelButton, winRestartButton, winSelectButton, winHomeButton
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("HUD")]
    public Button pauseButton;

    [Header("Pause Panel")]
    public Button resumeButton;
    public Button pauseRestartButton;
    public Button pauseSelectButton;
    public Button pauseHomeButton;

    [Header("Game Over Panel")]
    public Button gameOverRestartButton;
    public Button gameOverSelectButton;
    public Button gameOverHomeButton;

    [Header("Win Panel")]
    public Button nextLevelButton;
    public Button winRestartButton;
    public Button winSelectButton;
    public Button winHomeButton;

    void Start()
    {
        // HUD
        Wire(pauseButton, () => GameManager.Instance.PauseGame());

        // Pause
        Wire(resumeButton,       () => GameManager.Instance.ResumeGame());
        Wire(pauseRestartButton, () => GameManager.Instance.RestartLevel());
        Wire(pauseSelectButton,  () => GameManager.Instance.LoadSelectScene());
        Wire(pauseHomeButton,    () => GameManager.Instance.LoadMainMenu());

        // Game Over
        Wire(gameOverRestartButton, () => GameManager.Instance.RestartLevel());
        Wire(gameOverSelectButton,  () => GameManager.Instance.LoadSelectScene());
        Wire(gameOverHomeButton,    () => GameManager.Instance.LoadMainMenu());

        // Win
        Wire(nextLevelButton,  () => GameManager.Instance.LoadNextLevel());
        Wire(winRestartButton, () => GameManager.Instance.RestartLevel());
        Wire(winSelectButton,  () => GameManager.Instance.LoadSelectScene());
        Wire(winHomeButton,    () => GameManager.Instance.LoadMainMenu());
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
