using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private bool showCursorOnDeath = true;
    [SerializeField] private bool lockCursorInGame = true;

    private bool isVisible = false;

    public static DeathScreen Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        gameObject.SetActive(false);

        respawnButton.onClick.AddListener(OnRespawnClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void Show()
    {
        if (isVisible) return;
        isVisible = true;

        gameObject.SetActive(true);
        Time.timeScale = 0f;

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
        if (showCursorOnDeath)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
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
        gameObject.SetActive(false);

        Time.timeScale = 1f;

        if (lockCursorInGame)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnRespawnClicked()
    {
        Time.timeScale = 1f;

        StartCoroutine(RespawnWithDelay(0.3f));
    }

    private IEnumerator RespawnWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Respawn();
    }

    private void Respawn()
    {
        Vector3? respawnPos = GetRespawnPositionFromSave();
        if (!respawnPos.HasValue)
        {
            Transform lastCheckpoint = GetLastCheckpoint();
            if (lastCheckpoint != null) respawnPos = lastCheckpoint.position;
            else return;
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.transform.position = respawnPos.Value;
            Debug.Log($"[DeathScreen] Возрождение в позиции: {respawnPos.Value}");

            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            var health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.SetCurrenthealthFromData(health.MaxHealthInUnits);
                health.Heal(0);
            }

            if (!player.IsTorchLit())
            {
                player.InginteTorch();
            }

            player.SetInputBlocked(false);
        }
        Hide();
    }

    private Vector3? GetRespawnPositionFromSave()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null) return null;

        if (saveData.activeCheckpoints != null && saveData.activeCheckpoints.Count > 0)
        {
            string lastCheckpointID = saveData.activeCheckpoints.Last();
            if (saveData.checkpointPositions != null && saveData.checkpointPositions.TryGetValue(lastCheckpointID, out SerializableVector3 pos))
            {
                return pos;
            }

            var checkpoint = FindCheckpointByID(lastCheckpointID);
            if (checkpoint != null)
            {
                Vector3 posCh = checkpoint.TeleportPoint != null ?
                    checkpoint.TeleportPoint.position : checkpoint.transform.position;
                return posCh;
            }
        }

        if (saveData.playerPosition != Vector3.zero) return saveData.playerPosition;
        return null;
    }

    private Transform GetLastCheckpoint()
    {
        var mapManag = FindFirstObjectByType<MapManager>();
        if (mapManag == null) return null;

        string currentId = mapManag.CurrentCheckpointID;

        if (!string.IsNullOrEmpty(currentId))
        {
            var checkpoint = FindCheckpointByID(currentId);
            if (checkpoint != null) return checkpoint.TeleportPoint != null ? checkpoint.TeleportPoint : checkpoint.transform;
        }

        var chIDs = mapManag.GetActivatedCheckpointsIDs();
        if (chIDs == null || !chIDs.Any()) return null;

        string lastID = chIDs.Last();
        var lastCh = FindCheckpointByID(lastID);
        if (lastCh != null) return lastCh.TeleportPoint != null ? lastCh.TeleportPoint : lastCh.transform;
        return null;
    }

    private Checkpoint FindCheckpointByID(string checkpointID)
    {
        var allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (var ch in allCheckpoints)
        {
            if (ch != null && ch.CheckpointID == checkpointID) return ch;
        }
        return null;
    }

    private void SaveCurrentProgress()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health == null) return;

        SaveData saveData = SaveSystem.Load();
        if (saveData == null)
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

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.SaveToSaveData(saveData);
            Debug.Log($"[DeathScreen] Экономика: Echo={saveData.currentEcho}, XP={saveData.currentXP}");
        }

        var mapManag = FindFirstObjectByType<MapManager>();
        if (mapManag != null)
        {
            saveData.activeCheckpoints = mapManag.GetActivatedCheckpointsIDs().ToList();
            saveData.discoveredZones = mapManag.GetDiscoveredZones().Where(zn => zn != null).Select(zn => zn.zoneID).ToList();

            foreach (var checkpointID in saveData.activeCheckpoints)
            {
                if (!saveData.checkpointPositions.ContainsKey(checkpointID))
                {
                    var checkpoint = FindCheckpointByID(checkpointID);
                    if (checkpoint != null)
                    {
                        Vector3 pos = checkpoint.TeleportPoint != null ?
                            checkpoint.TeleportPoint.position : checkpoint.transform.position;
                        saveData.checkpointPositions[checkpointID] = pos;
                    }
                }
            }
        }

        SaveSystem.Save(saveData);
    }

    private void OnMainMenuClicked()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (GameManager.Instance != null) GameManager.Instance.SaveGame();
        else SaveCurrentProgress();

        if (LoadingScreen.Instance != null) LoadingScreen.Instance.LoadScene(MainMenu.MENU_SCENE_NAME);
        else SceneManager.LoadScene(MainMenu.MENU_SCENE_NAME);
    }

    private void OnDestroy()
    {
        respawnButton.onClick.RemoveListener(OnRespawnClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
    }
}