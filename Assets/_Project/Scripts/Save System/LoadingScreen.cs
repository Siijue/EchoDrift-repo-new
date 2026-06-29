using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;


    private string[] loadingTips = {
        "Загрузка мира...",
        "Зажигаем факелы...",
        "Будим врагов...",
        "Расставляем сундуки...",
        "Проверяем ловушки...",
        "Настраиваем кристаллы...",
        "Готовимся к приключению..."
    };
    private bool isLoading = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvasGroup == null) return;

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (loadingText != null) loadingText.gameObject.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading) return;
        isLoading = true;

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        yield return StartCoroutine(FadeIn());

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            string tip = loadingTips[Random.Range(0, loadingTips.Length)];
            loadingText.text = tip;
        }

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0;
        }
        if (progressText != null)
        {
            progressText.gameObject.SetActive(true);
            progressText.text = "0%";
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null) progressBar.value = progress;
            if (progressText != null) progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSecondsRealtime(0.3f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
        yield return StartCoroutine(FadeOut());

        isLoading = false;
    }

    private IEnumerator FadeIn()
    {
        gameObject.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;

        if (progressBar != null) progressBar.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
        if (loadingText != null) loadingText.gameObject.SetActive(false);
    }
}
