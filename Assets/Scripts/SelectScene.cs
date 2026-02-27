using DanielLochner.Assets.SimpleScrollSnap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectScene : MonoBehaviour
{
    [Header("References")]
    public SimpleScrollSnap simpleScrollSnap;
    public Button           selectButton;
    public Button           backButton;
    public GameObject       goLock;

    [Header("Settings")]
    public string backSceneName = "Home";
    public int    totalItems    = 5;

    [Header("Scene Names (index 0 = item 1)")]
    [Tooltip("Map scene names corresponding to each panel. E.g. Map_1, Map_2...")]
    public string[] sceneNames;

    const string SELECTED_KEY = "selected_map";

    int _currentSelection;

    void Start()
    {
        selectButton.onClick.AddListener(OnSelectPressed);
        backButton.onClick.AddListener(() => SceneManager.LoadScene(backSceneName));

        // Restore last selection
        int savedIndex = PlayerPrefs.GetInt(SELECTED_KEY, 0);
        if (savedIndex >= 0 && savedIndex < totalItems)
        {
            simpleScrollSnap.GoToPanel(savedIndex);
        }
    }

    /// <summary>
    /// Called by SimpleScrollSnap's OnPanelCentered event.
    /// Wire in Inspector: SimpleScrollSnap → On Panel Centered → SelectScene.UpdateSelection
    /// Signature matches: (int centeredPanel, int selectedPanel)
    /// </summary>
    public void UpdateSelection(int centeredPanel, int selectedPanel)
    {
        _currentSelection = selectedPanel;
        int level = selectedPanel + 1;

        bool isUnlocked = ProgressManager.instance != null
            ? ProgressManager.instance.IsLevelUnlocked(level)
            : true;

        if (goLock != null) goLock.SetActive(!isUnlocked);
        selectButton.gameObject.SetActive(isUnlocked);
    }

    void OnSelectPressed()
    {
        int panelIndex = simpleScrollSnap.SelectedPanel;
        int level      = panelIndex + 1;

        if (ProgressManager.instance != null && !ProgressManager.instance.IsLevelUnlocked(level))
        {
            Debug.LogWarning($"Level {level} is locked!");
            return;
        }

        // Save selection
        PlayerPrefs.SetInt(SELECTED_KEY, panelIndex);
        PlayerPrefs.Save();

        // Load the corresponding scene
        string targetScene = GetSceneName(panelIndex);
        LoadingScene.LoadScene(targetScene);
    }

    string GetSceneName(int panelIndex)
    {
        // Use custom scene names if provided, otherwise default pattern "Map_1", "Map_2"...
        if (sceneNames != null && panelIndex < sceneNames.Length && !string.IsNullOrEmpty(sceneNames[panelIndex]))
        {
            return sceneNames[panelIndex];
        }
        return "Map_" + (panelIndex + 1);
    }
}
