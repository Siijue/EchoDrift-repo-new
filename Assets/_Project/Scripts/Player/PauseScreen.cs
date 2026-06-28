using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using Unity.VisualScripting;
using System.Collections;

public class PauseScreen : MonoBehaviour
{
    public static PauseScreen Instance {  get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Slider musicVolSlider;
    [SerializeField] private Slider sfxVolSlider;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private bool showCursor = true;
    [SerializeField] private bool lockCursor = true;

    private bool isVisible = false;
    private bool ignoreNextEscape = false;


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (backButton != null) backButton.onClick.AddListener(OnBackClicked); 
        resumeButton.onClick.AddListener(OnResumeClicked);

        InitializeSettings();
    }

    public void IgnoreNextEscape() => ignoreNextEscape = true;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ignoreNextEscape)
            {
                ignoreNextEscape = false;
                return;
            }
            if (DeathScreen.Instance != null && DeathScreen.Instance.gameObject.activeInHierarchy) return;

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsInputEnabled) return;

            if (isVisible) Hide();
            else Show();
        }
    }

    public void Show()
    {
        if (isVisible) return;
        isVisible = true;
        gameObject.SetActive(true);
        Time.timeScale = 0f;

        if (mainPanel != null) mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        RefreshSettings();
        StartCoroutine(FadeIn());
    }

    public void Hide()
    {
        if (!isVisible) return;
        isVisible = false;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        if (showCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        float elapsed = 0f;
        while(elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float time = elapsed / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 0.9f, time);
            yield return null;
        }

        canvasGroup.alpha = 0.9f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private IEnumerator FadeOut()
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        float elapsed = 0f;
        while(elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float time = elapsed / fadeOutDuration;
            canvasGroup.alpha = Mathf.Lerp(0.9f, 0f, time);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        Time.timeScale = 1f;
        if (lockCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnResumeClicked() => Hide();

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SaveCurrentProgress();
        SceneManager.LoadScene(MainMenu.MENU_SCENE_NAME);
    }

    private void OnSettingsClicked()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null) mainPanel.SetActive(true);
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
        if (healthToSave <= 0) healthToSave = saveData.currentHealth > 0 ? saveData.currentHealth : 4;
        saveData.currentHealth = healthToSave;
        saveData.maxHealth = health.MaxHealthInUnits;

        saveData.torchCurrentTime = player.GetTorchTime();
        saveData.torchMaxTime = player.GetTorchMaxTime();

        Transform lastCheckpoint = GetLastCheckpoint();
        if (lastCheckpoint != null) saveData.playerPosition = lastCheckpoint.position;
        else saveData.playerPosition = player.transform.position;

        if (EconomyManager.Instance != null) EconomyManager.Instance.SaveToSaveData(saveData);

        var mapManag = FindFirstObjectByType<MapManager>();
        if(mapManag != null)
        {
            saveData.activeCheckpoints = mapManag.GetActivatedCheckpoints().Where(ch => ch != null).Select(ch => ch.CheckpointID).ToList();
            saveData.discoveredZones = mapManag.GetDiscoveredZones().Where(zn => zn != null).Select(zn => zn.zoneID).ToList();
        }

        SaveSystem.Save(saveData);
    }

    private Transform GetLastCheckpoint()
    {
        var mapManag = FindFirstObjectByType<MapManager>();
        if (mapManag == null) return null;

        var checkpoints = mapManag.GetActivatedCheckpoints();
        if (checkpoints == null || !checkpoints.Any()) return null;

        var last = checkpoints.Last();
        return last != null ? last.transform : null;
    }

    private void InitializeSettings()
    {
        if(musicVolSlider != null)
        {
            musicVolSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if(sfxVolSlider != null)
        {
            sfxVolSlider.value = PlayerPrefs.GetFloat("SFXVolume");
            sfxVolSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }

    private void RefreshSettings()
    {
        if (musicVolSlider != null) musicVolSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxVolSlider != null) sfxVolSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private void SetMusicVolume(float vol)
    {
        PlayerPrefs.SetFloat("MusicVolume", vol);
        PlayerPrefs.Save();
        // AudioManager.Instance?.SetMusicVolume(vol);
    }

    private void SetSfxVolume(float vol)
    {
        PlayerPrefs.SetFloat("SFXVolume", vol);
        PlayerPrefs.Save();
        // AudioManager.Instance?.SetSfxVolume(vol);
    }

    private void OnDestroy()
    {
        resumeButton.onClick.RemoveListener(OnResumeClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OnSettingsClicked);
        if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);
        if (musicVolSlider != null) musicVolSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxVolSlider != null) sfxVolSlider.onValueChanged.RemoveListener(SetSfxVolume);
    }
}
