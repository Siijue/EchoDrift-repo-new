using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settignsPanel;

    [SerializeField] private Slider musicVolSlider;
    [SerializeField] private Slider sfxVolSlider;

    [SerializeField] private string gameSceneName = "GameScene";

    [SerializeField] Vector3 startPosition;

    public const string MENU_SCENE_NAME = "MainMenuScene";
    private SaveData saveData;

    private void Start()
    {
        saveData = SaveSystem.Load();
        UpdateContinueButton();
        InitializeSettings();

        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (exitButton != null) exitButton.onClick.AddListener(OnExit);
    }

    private void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            bool hasSave = saveData != null && saveData.hasStartedGame;
            continueButton.gameObject.SetActive(hasSave);
        }
    }

    private void InitializeSettings()
    {
        if (musicVolSlider != null)
        {
            musicVolSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicVolSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        if (sfxVolSlider != null)
        {
            sfxVolSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolSlider.onValueChanged.AddListener(SetSfxVolume);
        }
    }

    private void OnNewGame()
    {
        SaveData newSave = new SaveData();
        newSave.hasStartedGame = true;

        newSave.currentHealth = 12;
        newSave.maxHealth = 12;

        newSave.currentXP = 0;
        newSave.currentEcho = 0;
        newSave.currentSkillPoints = 0;
        newSave.currentLevel = 1;
        newSave.playerPosition = startPosition;

        SaveSystem.Save(newSave);
        LoadingScreen.Instance.LoadScene(gameSceneName);
    }

    private void OnContinue()
    {
        if (saveData == null) return;
        LoadingScreen.Instance.LoadScene(gameSceneName);
    }

    private void OnSettings()
    {
        mainPanel.SetActive(false);
        settignsPanel.SetActive(true);
    }

    private void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();

#endif
    }

    public void OnBackFromSettings()
    {
        settignsPanel.SetActive(false);
        mainPanel.SetActive(true);
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

    public static void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        LoadingScreen.Instance.LoadScene(MENU_SCENE_NAME);
    }
}