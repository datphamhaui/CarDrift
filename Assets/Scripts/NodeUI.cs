using UnityEngine;
using TMPro;

public class NodeUI : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer nodeSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;
    [SerializeField] private Sprite completedSprite;

    [Header("UI")]
    [SerializeField] private TMP_Text levelText;

    private LevelData data;
    private MapManager mapManager;

    public void Setup(LevelData levelData, MapManager manager)
    {
        data = levelData;
        mapManager = manager;

        if (levelText != null)
            levelText.text = data.levelId.ToString();

        if (!data.isUnlocked)
            nodeSprite.sprite = lockedSprite;
        else
            nodeSprite.sprite = unlockedSprite;
    }

    void OnMouseDown()
    {
        if (data == null || !data.isUnlocked) return;
        if (SettingsPopup.IsOpen) return;
        mapManager.OnNodeClicked(data);
    }
}
