using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Text hiển thị dạng COLLECT: X/Y")]
    public TMP_Text collectText;

    public int CoinCount { get; private set; }
    public int TotalCoins { get; private set; }

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
        TotalCoins = FindObjectsOfType<CoinCollectible>().Length;
        CoinCount = 0;
        UpdateUI();
    }

    public void AddCoin()
    {
        CoinCount++;
        UpdateUI();
    }

    public void StopScoring() { }

    public void SaveHighScore() { }

    void UpdateUI()
    {
        if (collectText != null)
            collectText.text = $"COLLECT: {CoinCount}/{TotalCoins}";
    }
}
