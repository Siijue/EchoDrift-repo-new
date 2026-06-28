using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private SaveData currentSaveData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        LoadGame();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Forest-Scene-Placeholder[v.0.4.0]" || scene.name == "GameScene") Invoke(nameof(LoadGame), 0.1f);
    }

    private void OnApplicationQuit() => SaveGame();

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) SaveGame();
    }

    public void LoadGame()
    {
        currentSaveData = SaveSystem.Load();
        if (currentSaveData == null || !currentSaveData.hasStartedGame) return;

        //delete
        Debug.Log($"[GameManager] Загрузка сохранения: " + $"{currentSaveData.discoveredZones?.Count ?? 0} зон, " + $"{currentSaveData.activeCheckpoints?.Count ?? 0} чекпоинтов, " + $"{currentSaveData.killedEnemy?.Count ?? 0} убитых врагов, " + $"{currentSaveData.activatedObjects?.Count ?? 0} активированных объектов");

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            if (currentSaveData.playerPosition != Vector3.zero) player.transform.position = currentSaveData.playerPosition;
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.SetHealth(currentSaveData.currentHealth, currentSaveData.maxHealth);
        }

        EconomyManager.Instance?.LoadFromSaveData(currentSaveData);

        if (MapManager.Instance != null)
        {
            MapManager.Instance.LoadFromSaveData(currentSaveData);
            RestoreCheckpointsVisuals();
        }

        SkillManager.Instance?.LoadFromSaveData(currentSaveData);

        UIManager.Instance?.RefreshAllUI();
    }

    public void SaveGame()
    {
        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();
            currentSaveData.hasStartedGame = true;
        }

        SaveData actualData = SaveSystem.Load();
        if (actualData != null)
        {
            currentSaveData.killedEnemy = actualData.killedEnemy ?? new List<string>();
            currentSaveData.activatedObjects = actualData.activatedObjects ?? new List<string>();

            if (actualData.checkpointPositions != null) currentSaveData.checkpointPositions = actualData.checkpointPositions;
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            Transform lastCheckpoint = GetLastCheckpoint();
            if (lastCheckpoint != null) currentSaveData.playerPosition = lastCheckpoint.position;
            else currentSaveData.playerPosition = player.transform.position;


            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                int healthToSave = health.CurrentHealth;
                if (healthToSave <= 0) healthToSave = currentSaveData.currentHealth > 0 ? currentSaveData.currentHealth : 4;

                currentSaveData.currentHealth = healthToSave;
                currentSaveData.maxHealth = health.MaxHealth;
            }

            currentSaveData.torchCurrentTime = player.GetTorchTime();
            currentSaveData.torchMaxTime = player.GetTorchMaxTime();
        }

        EconomyManager.Instance?.SaveToSaveData(currentSaveData);
        MapManager.Instance?.SaveToSaveData(currentSaveData);
        SkillManager.Instance?.SaveToSaveData(currentSaveData);
        SaveSystem.Save(currentSaveData);
    }

    private Transform GetLastCheckpoint()
    {
        if (MapManager.Instance == null) return null;

        var checkpoints = MapManager.Instance.GetActivatedCheckpoints();
        if (checkpoints == null || !checkpoints.Any()) return null;
        var last = checkpoints.Last();
        return last != null ? last.transform : null;
    }

    public void OnCheckpointReached(string checkpointID) => SaveGame();

    private void RestoreCheckpointsVisuals()
    {
        if (currentSaveData?.activeCheckpoints == null) return;

        var allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        int restoredCount = 0;

        foreach (var checkpoint in allCheckpoints)
        {
            if (checkpoint != null && currentSaveData.activeCheckpoints.Contains(checkpoint.CheckpointID))
            {
                checkpoint.ActivateFromSave();
                restoredCount++;
            }
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}