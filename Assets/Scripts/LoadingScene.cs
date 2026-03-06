using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScene : MonoBehaviour
{
    [Header("Target")]
    public string nextSceneName = "SelectScene";

    [Header("UI References")]
    public Slider progressBar;
    public TMP_Text progressText;

    [Header("Settings")]
    [Range(1f, 10f)]
    public float minimumLoadTime = 3f;

    static string targetSceneOverride;

    public static void LoadScene(string sceneName)
    {
        targetSceneOverride = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    void Start()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(AudioManager.instance.menuMusic);

        if (!string.IsNullOrEmpty(targetSceneOverride))
        {
            nextSceneName = targetSceneOverride;
            targetSceneOverride = null;
        }

        if (progressBar != null)
        {
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
            progressBar.value = 0f;
        }

        StartCoroutine(LoadRoutine());
    }

    IEnumerator LoadRoutine()
    {
        float elapsed = 0f;
        float displayProgress = 0f;

        // Phase 1: Fake progress 0% → 70%
        float fakeTarget = 0.7f;
        float fakeDuration = minimumLoadTime * 0.7f;

        while (elapsed < fakeDuration)
        {
            elapsed += Time.deltaTime;
            displayProgress = Mathf.Lerp(0f, fakeTarget, elapsed / fakeDuration);
            UpdateUI(displayProgress);
            yield return null;
        }

        // Phase 2: Real async loading 70% → 90%
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            float realProgress = fakeTarget + asyncLoad.progress / 0.9f * 0.2f;
            displayProgress = Mathf.Max(displayProgress, realProgress);
            UpdateUI(displayProgress);
            yield return null;
        }

        // Phase 3: Fill to 100%
        float fillStart = displayProgress;
        float fillElapsed = 0f;
        float fillDuration = 0.5f;

        while (fillElapsed < fillDuration)
        {
            fillElapsed += Time.deltaTime;
            displayProgress = Mathf.Lerp(fillStart, 1f, fillElapsed / fillDuration);
            UpdateUI(displayProgress);
            yield return null;
        }

        UpdateUI(1f);
        yield return new WaitForSeconds(0.2f);

        asyncLoad.allowSceneActivation = true;
    }

    void UpdateUI(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = "Loading... " + Mathf.RoundToInt(progress * 100) + "%";
    }
}
