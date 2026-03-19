using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] private List<NodeUI> nodeObjects;
    [SerializeField] private List<LevelData> levels;

    void Start()
    {
        LoadProgress();
        SetupNodes();
    }

    void LoadProgress()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            int level = levels[i].levelId;
            bool unlocked = ProgressManager.instance != null
                ? ProgressManager.instance.IsLevelUnlocked(level)
                : (i == 0);
            levels[i].isUnlocked = unlocked;
        }
    }

    void SetupNodes()
    {
        for (int i = 0; i < nodeObjects.Count; i++)
        {
            if (i < levels.Count)
                nodeObjects[i].Setup(levels[i], this);
        }
    }

    public void OnNodeClicked(LevelData data)
    {
        if (data == null || !data.isUnlocked) return;

        PlayerPrefs.SetInt("selected_map", data.levelId - 1);
        PlayerPrefs.Save();

        LoadingScene.LoadScene(data.sceneName);
    }
}
