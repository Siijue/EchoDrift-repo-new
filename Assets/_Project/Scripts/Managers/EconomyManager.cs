using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance {  get; private set; }

    [SerializeField] private int[] levelThreshods = { 100, 250, 450, 700 };

    private int currentXP;
    private int currentEcho;
    private int currentLevel;
    private int skillPoints;

    public int CurrentXP => currentXP;
    public int CurrentEcho => currentEcho;
    public int CurrentLevel => currentLevel;
    public int SkillPoints => skillPoints;
    public int[] LevelThresholds => levelThreshods;

    public UnityEvent<int> OnEchoChanged;
    public UnityEvent<int, int> OnXPChanged;
    public UnityEvent<int> OnSkillPointsChanged;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // upd
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        SaveData saveData = SaveSystem.Load();

        if (saveData == null || !saveData.hasStartedGame)
        {
            currentXP = 0;
            currentEcho = 0;
            currentLevel = 1;
            skillPoints = 0;
        }
        else LoadFromSaveData(saveData);

        InvokeEvents();
    }

    public void AddEcho(int amount)
    {
        if (amount <= 0) return;
        currentEcho += amount;
        OnEchoChanged?.Invoke(currentEcho);
        UIManager.Instance?.ShowFloatingText($"+{amount} Эхо");
    }

    public bool SpendEcho(int amount)
    {
        if(amount <= 0) return false;

        if(currentEcho >= amount)
        {
            currentEcho -= amount;
            OnEchoChanged?.Invoke(currentEcho);
            return true;
        }
        return false;
    }

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;

        CheckLevelUp();
        InvokeEvents();
        UIManager.Instance?.ShowFloatingText($"+{amount} XP");
    }

    private void CheckLevelUp()
    {
        if (levelThreshods == null || levelThreshods.Length == 0) return;

        if (currentLevel < 1) currentLevel = 1;

        while (true)
        {
            int thresholdIndex = currentLevel - 1;
            if (thresholdIndex >= levelThreshods.Length) break;

            if (currentXP >= levelThreshods[thresholdIndex])
            {
                currentLevel++;
                skillPoints++;
                OnSkillPointsChanged?.Invoke(skillPoints);
                OnXPChanged?.Invoke(currentXP, currentLevel);
            }
            else break;
        }
    }

    private void InvokeEvents()
    {
        OnEchoChanged?.Invoke(CurrentEcho);
        OnXPChanged?.Invoke(currentXP, currentLevel);
    }

    public bool SpendSkillPoints(int amount)
    {
        if(amount <= 0) return false;

        if(skillPoints >= amount)
        {
            skillPoints -= amount;
            OnSkillPointsChanged.Invoke(skillPoints);
            return true;
        }
        return false;
    }


    public void AddSkillPoints(int amount)
    {
        if (amount <= 0) return;

        skillPoints += amount;
        OnSkillPointsChanged?.Invoke(skillPoints);
    }

    public void SetParamFromData(int currentEchoData, int currentXPData, int currentLevelData, int currentSkillPointsData)
    {
        currentEcho = currentEchoData;
        currentXP = currentXPData;
        currentLevel = currentLevelData;
        skillPoints = currentSkillPointsData;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    // save

    public void LoadFromSaveData(SaveData data)
    {
        if (data == null) return;
        currentEcho = data.currentEcho;
        currentXP = data.currentXP;
        currentLevel = data.currentLevel;
        skillPoints = data.currentSkillPoints;
        InvokeEvents();
        OnSkillPointsChanged?.Invoke(skillPoints);
        Debug.Log($"[EconomyManager] Загружено: Echo={currentEcho}, XP={currentXP}, Level={currentLevel}");
    }

    public void SaveToSaveData(SaveData data)
    {
        if (data == null) return;
        data.currentEcho = currentEcho;
        data.currentXP = currentXP;
        data.currentLevel = currentLevel;
        data.currentSkillPoints = skillPoints;
    }

    // upd
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Forest-Scene-Placeholder[v.0.4.0]")
        {
            SaveData saveData = SaveSystem.Load();
            if (saveData != null && saveData.hasStartedGame) LoadFromSaveData(saveData);
        }
    }
}
