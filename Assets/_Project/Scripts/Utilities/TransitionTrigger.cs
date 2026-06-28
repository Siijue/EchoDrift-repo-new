using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class TransitionTrigger : MonoBehaviour, IInteractable
{
    public enum ActivationMode
    {
        Auto,
        PressE
    }

    [Header("Переход")]
    [SerializeField] private string sceneToLoad = "MainMenuScene";
    [SerializeField] private ActivationMode mode = ActivationMode.PressE;

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Подсказка")]
    [SerializeField] private string hintText = "[E] Вернуться в главное меню";
    [SerializeField] private float hintPriority = 10f;

    [Header("UI перехода")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private bool _triggered = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }
        if (fadeCanvasGroup == null)
        {
            CreateFadeUI();
        }
        else
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    private void CreateFadeUI()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        fadeCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
        GameObject fadeObj = new GameObject("FadeImage");
        fadeObj.transform.SetParent(transform, false);

        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = Color.black;

        RectTransform rt = fadeObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || _triggered) return;

        if (mode == ActivationMode.PressE) HintManager.Instance?.RegisterHint(this, hintText, hintPriority, 0);
        else if (mode == ActivationMode.Auto) TriggerTransition();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) HintManager.Instance?.RemoveHintsFromSource(this);
    }

    public void Interact(PlayerController player)
    {
        if (mode != ActivationMode.PressE || _triggered) return;
        TriggerTransition();
    }

    private void TriggerTransition()
    {
        if (_triggered) return;
        _triggered = true;

        HintManager.Instance?.RemoveHintsFromSource(this);

        StartCoroutine(FadeAndLoadScene());
    }

    private IEnumerator FadeAndLoadScene()
    {
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
        //upd
        SaveCurrentProgress();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        yield return null;
        LoadingScreen.Instance.LoadScene(sceneToLoad);
    }

    private void SaveCurrentProgress()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        SaveData saveData = SaveSystem.Load();
        if(saveData == null)
        {
            saveData = new SaveData();
            saveData.hasStartedGame = true;
        }

        int healthToSave = health.CurrentHealthInUnits;
        if (healthToSave <= 0) healthToSave = 4;

        saveData.currentHealth = healthToSave;
        saveData.maxHealth = health.MaxHealthInUnits;

        Transform lastCheckpoint = GetLastCheckpoint();
        if (lastCheckpoint != null) saveData.playerPosition = lastCheckpoint.position;
        else saveData.playerPosition = player.transform.position;

        if (EconomyManager.Instance != null) EconomyManager.Instance.SaveToSaveData(saveData);

        SaveSystem.Save(saveData);
    }

    private Transform GetLastCheckpoint()
    {
        var mapManager = FindFirstObjectByType<MapManager>();
        if (mapManager == null) return null;

        var checkpoints = mapManager.GetActivatedCheckpoints();
        if (checkpoints == null || !checkpoints.Any()) return null;

        var last = checkpoints.Last();
        return last != null ? last.transform : null;
    }
}