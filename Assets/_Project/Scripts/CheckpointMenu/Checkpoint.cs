using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class Checkpoint : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform player;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private GameObject checkpointMenu;
    [SerializeField] private GameObject interactionText;
    [SerializeField] private SpriteRenderer checkpointSprite;
    [SerializeField] private Sprite activeCheckpointSprite;

    [SerializeField] private string checkpointName;
    [SerializeField] private string checkpointID;

    [SerializeField] private TextMeshProUGUI currentHealthUI;
    [SerializeField] private Button healButton;


    [SerializeField] private GameObject buttonsContainer;


    // ShopPage
    [SerializeField] private GameObject[] pages;
    [SerializeField] private Transform shopContainer;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private ShopItemSO[] shopItems;
    [SerializeField] private TextMeshProUGUI shopEchoText;

    // SkillTree
    [SerializeField] private Transform skillContainer;
    [SerializeField] private SkillNodeSO[] skillNodes;
    [SerializeField] private GameObject skillNodePrefab;
    [SerializeField] private TextMeshProUGUI skillPointsText;

    // Teleport and map
    [SerializeField] private string zoneID;
    [SerializeField] private Vector2 mapIconPosition;
    [SerializeField] private Transform teleportPoint;
    [SerializeField] private GameObject mapButton;
    [SerializeField] private MapUI mapUI;


    // СЕРИАЛИЗАЦИЯ ДЛЯ ДЕБАГА! НЕ МЕНЯТЬ В ИНСПЕКТОРЕ!
    [Header("НЕ МЕНЯТЬ! READ ONLY!")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private bool isMenuOpen = false;
    [SerializeField] protected bool isPlayerInRadius = false;
    [SerializeField] private bool isNeedHint= false;

    private PlayerController _playerController;
    private PlayerHealth _playerHealth;

    private int currentPageIndex = -1;
    private List<ShopItemUI> activeShopButtons = new List<ShopItemUI>();

    private List<SkillNodeUI> activeSkillNodes = new List<SkillNodeUI>();
    private List<string> activatedCheckpoints = new List<string>();

    public string CheckpointID => checkpointID;
    public string ZoneID => zoneID;
    public Vector2 MapIconPosition => mapIconPosition;
    public Transform TeleportPoint => teleportPoint;



    private void Awake()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if(player != null) _playerController = player.GetComponent<PlayerController>();

        if (player != null) _playerHealth = player.GetComponent<PlayerHealth>();

        if (uiManager == null) uiManager = UIManager.Instance;

        if (mapButton != null) mapButton.GetComponent<Button>()?.onClick.AddListener(OnMapButtonClicked);

        healButton?.onClick.AddListener(OnHealButtonClicked);
        mapUI.OnTeleportRequested.AddListener(CloseMenu);


        if (checkpointMenu != null) checkpointMenu.SetActive(false);

        Cursor.visible = false;
    }

    private void Start()
    {
        EconomyManager.Instance.OnEchoChanged.AddListener(OnEchoChangeInShop);
        EconomyManager.Instance.OnSkillPointsChanged.AddListener(RefreshAllSkills);
    }

    private void Update()
    {
        if (!isMenuOpen) return;

        if (mapUI != null && mapUI.IsMapOpen) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(currentPageIndex != -1)
            {
                ResetPages();
                Debug.Log("Вкладка закрыта");
            }
            else
            {
                CloseMenu();
                Debug.Log("Меню закрыто");
            }
        }
    }

    void IInteractable.Interact(PlayerController player)
    {
        if (!isPlayerInRadius || isMenuOpen) return;

        if (!isActivated) ActivateCheckpoint();

        OpenMenu();
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;

        if(checkpointSprite != null && activeCheckpointSprite != null)
        {
            checkpointSprite.GetComponent<SpriteRenderer>().sprite = activeCheckpointSprite;
            Debug.Log($"Checkpoint: активация визуала {checkpointName}");
        }

        MapManager.Instance?.RegisterActivatedCheckpoint(checkpointID);
        MapManager.Instance?.DiscoverZone(zoneID);
        MapManager.Instance?.SetCurrentCheckpoint(checkpointID);

        isNeedHint = true;

        GameManager.Instance?.OnCheckpointReached(checkpointID);

        // upd
        //SaveCurrentProgress();

        UpdateCheckpointUI();
    }

    // upd
    private void SaveCurrentProgress()
    {
        if (_playerController == null || _playerHealth == null) return;

        SaveData saveData = SaveSystem.Load();
        if (saveData == null)
        {
            saveData = new SaveData();
            saveData.hasStartedGame = true;
        }

        int healthToSave = _playerHealth.CurrentHealthInUnits;
        if (healthToSave <= 0) healthToSave = saveData.currentHealth > 0 ? saveData.currentHealth : 4;

        saveData.currentHealth = healthToSave;
        saveData.maxHealth = _playerHealth.MaxHealthInUnits;

        saveData.playerPosition = teleportPoint != null ? teleportPoint.position : transform.position;

        saveData.torchCurrentTime = _playerController.GetTorchTime();
        saveData.torchMaxTime = _playerController.GetTorchMaxTime();

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.SaveToSaveData(saveData);
        }

        if (MapManager.Instance != null)
        {
            saveData.activeCheckpoints = MapManager.Instance.GetActivatedCheckpointsIDs().ToList();

            foreach (var checkpoint in MapManager.Instance.GetActivatedCheckpoints())
            {
                if (checkpoint != null && !saveData.checkpointPositions.ContainsKey(checkpoint.CheckpointID))
                {
                    Vector3 pos = checkpoint.TeleportPoint != null ? checkpoint.TeleportPoint.position : checkpoint.transform.position;
                    saveData.checkpointPositions[checkpoint.CheckpointID] = pos;
                }
            }

            saveData.discoveredZones = MapManager.Instance.GetDiscoveredZones().Where(z => z != null).Select(z => z.zoneID).ToList();
        }

        SaveSystem.Save(saveData);
    }

    public void ActivateFromSave()
    {
        if (isActivated) return;
        isActivated = true;
        if (checkpointSprite != null && activeCheckpointSprite != null) checkpointSprite.sprite = activeCheckpointSprite;
    }

    private void UpdateCheckpointUI()
    {
        if(isMenuOpen && isNeedHint)
        {
            string message = $"Активирован чекпоинт {checkpointName}. Сохранение успешно";
            HintManager.Instance?.RegisterHint(this, message, priotiry: 15f, duration: 3f);
        }
    }

    private void OpenMenu()
    {
        if (isMenuOpen) return;
        isMenuOpen = true;
        Time.timeScale = 0f;

        if (checkpointMenu != null) checkpointMenu.SetActive(true);

        if (mapButton != null) mapButton.SetActive(MapManager.Instance != null);

        UIManager.Instance?.HideHint();
        HintManager.Instance?.ClearAllHints();
        _playerController?.SetInputBlocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ResetPages();

        Debug.Log($"Checkpoint: открыто меню чекпоинта {checkpointName}");
        UpdateHealUI();

        UpdateCheckpointUI();
    }

    public void CloseMenu()
    {
        if (!isMenuOpen) return;
        isMenuOpen = false;
        Time.timeScale = 1f;

        if(checkpointMenu != null) checkpointMenu.SetActive(false);
        _playerController?.SetInputBlocked(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ResetPages();

        if (PauseScreen.Instance != null) PauseScreen.Instance.IgnoreNextEscape();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRadius = true;
            _playerController = other.GetComponent<PlayerController>();

            string hint = isActivated ? "[E] открыть меню чекпоинта" : $"[E] активировать чекпоинт {checkpointName}";

            HintManager.Instance?.RegisterHint(this, hint, priotiry: 10f, duration: 0);

            Debug.Log($"Checkpoint: игрок в радиусе чекпоинта {checkpointName}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRadius = false;

            HintManager.Instance?.RemoveHintsFromSource(this);

            if (isMenuOpen) CloseMenu();

            Debug.Log($"Checkpoint: игрок покинул радиус чекпоинта {checkpointName}");
        }
    }

    private void UpdateHealUI()
    {
        float maxHealth = _playerHealth.MaxHealthInHealth;
        float currentHealth = _playerHealth.CurrentHealthInHearts;
        bool isHealthMaxed = maxHealth == currentHealth;
        if (isHealthMaxed)
        {
            healButton.interactable = false;
            healButton.GetComponentInChildren<TextMeshProUGUI>().text = "Здоровье полное";
        }
        else
        {
            healButton.interactable = true;
            healButton.GetComponentInChildren<TextMeshProUGUI>().text = "Восстановить";
        }

        if(currentHealthUI != null) currentHealthUI.text = $"{currentHealth} / {maxHealth}";
    }

    private void PopulateShop()
    {
        foreach(Transform child in shopContainer) Destroy(child.gameObject);

        activeShopButtons.Clear();

        shopEchoText.text = $"Эхо: {EconomyManager.Instance?.CurrentEcho}";

        foreach(var item in shopItems)
        {
            if (shopItemPrefab != null)
            {
                GameObject newObj = Instantiate(shopItemPrefab, shopContainer);
                ShopItemUI itemUI = newObj.GetComponent<ShopItemUI>();

                if (itemUI != null)
                {
                    Debug.Log($"Название: {itemUI.name}");
                    itemUI.Initialize(item, this);
                    activeShopButtons.Add(itemUI);
                }
                else Debug.Log("itemUI НЕ НАЙДЕН");
            }
            else Debug.Log("shopItemPrefab НЕ НАЙДЕН");
        }
        Debug.Log($"Checkpoint: Магазин заполнен: {shopItems.Length} товаров");
    }

    private void ApplyShopEffect(ShopItemSO item)
    {
        switch (item.EffectType)
        {
            case ShopEffect.LightExtension:
                _playerController?.ExtendTorchTime(item.EffectValue);
                break;
        }
    }


    public void OnHealButtonClicked()
    {
        Debug.Log("Button Pressed");

        _playerHealth.Heal(_playerHealth.MaxHealthInHealth);
        if (_playerHealth != null && isMenuOpen)
        {
            UpdateHealUI();
        }
    }
        
    public void SwitchPage(int index)
    {
        if(index < 0 || pages.Length <= index || index == currentPageIndex) return;

        if (buttonsContainer != null) buttonsContainer.SetActive(false);
        ResetContentPages();

        pages[index].SetActive(true);
        currentPageIndex = index;

        switch (index)
        {
            case 0: PopulateShop(); break;
            case 1: PopulateSkills(); break;
            default: break;
        }
    }

    public void TryBuyItem(ShopItemSO item, ShopItemUI buttonUI)
    {
        bool purchaseSuccess = EconomyManager.Instance.SpendEcho(item.Price);

        if (purchaseSuccess)
        {
            Debug.Log("ПОКУПКА");

            buttonUI.SetButtonPurchased();

            ApplyShopEffect(item);
        }
        else Debug.Log("ПОКУПКА БЕЗУСПЕШНА");
    }

    private void ResetContentPages()
    {
        if (pages == null) return;

        foreach (var page in pages)
        {
            if (page != null)page.SetActive(false);
        }
        currentPageIndex = -1;
    }

    private void ResetPages()
    {
        ResetContentPages();

        if (buttonsContainer != null) buttonsContainer.SetActive(true);
    }

    private void OnEchoChangeInShop(int newEcho)
    {
        if(shopEchoText != null) shopEchoText.text = $"Эхо: {newEcho}";

        if(currentPageIndex == 0)
        {
            foreach(ShopItemUI button in activeShopButtons)
            {
                button.UpdateAvailability(newEcho);
            }
        }
    }

    private void OnDestroy()
    {
        EconomyManager.Instance.OnEchoChanged.RemoveListener(OnEchoChangeInShop);
        EconomyManager.Instance.OnSkillPointsChanged.RemoveListener(RefreshAllSkills);
    }


    private void PopulateSkills()
    {
        if(skillContainer == null) return;
        foreach (Transform child in skillContainer) Destroy(child.gameObject);
        activeSkillNodes.Clear();

        foreach (SkillNodeSO skill in skillNodes)
        {
            if (skill == null) continue;
            SkillNodeUI nodeUI = Instantiate(skillNodePrefab, skillContainer).GetComponent<SkillNodeUI>();
            if (nodeUI == null) continue;

            int currentLevel = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(skill) : 0;

            nodeUI.Initialize(skill, this, currentLevel);
            activeSkillNodes.Add(nodeUI);
        }

        Canvas.ForceUpdateCanvases();
        int currentSkillPoints = EconomyManager.Instance != null ? EconomyManager.Instance.SkillPoints : 0;
        RefreshAllSkills(currentSkillPoints);
    }


    private void RefreshAllSkills(int newSkillPoints)
    {
        if (skillPointsText != null) skillPointsText.text = $"Очки: {newSkillPoints}";

        foreach(SkillNodeUI node in activeSkillNodes)
        {
            if (node == null) continue;
            SkillNodeSO data = node?.CurrentData;
            if(data == null) continue;

            bool isUnlocked = false;
            if(data.RequieredSkill == null) isUnlocked = true;
            else
            {
                int requiredLevel = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(data.RequieredSkill) : 0;
                isUnlocked = requiredLevel >= data.RequieredSkill.MaxLevel;
            }

            int currentLevel = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(data) : 0;
            bool isMaxed = currentLevel >= data.MaxLevel;
            bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.SkillPoints >= data.SkillPointsForOneLevel;

            node.SetCurrentLevel(currentLevel);
            node.UpdateVisualState(isUnlocked, canAfford, isMaxed);
        }
    }

    
    

    private void ApplySkillEffect(SkillNodeSO data, int level)
    {
        switch (data.EffectType)
        {
            case SkillEffect.MaxHealthBoost: _playerHealth?.IncreaseMaxHealth(data.EffectValue); break;

            default: break;
        }
    }

    public void UpgradeSkills(SkillNodeUI nodeUI)
    {
        if (nodeUI == null) return;

        var data = nodeUI.CurrentData;
        if (data == null) return;

        int currentLevel = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(data) : 0;

        if(data.RequieredSkill != null)
        {
            int requieredLevel = SkillManager.Instance != null ? SkillManager.Instance.GetSkillLevel(data.RequieredSkill) : 0;
            if(requieredLevel < 1)
            {
                HintManager.Instance?.RegisterHint(this, $"Сначала изучите {data.RequieredSkill.NodeName}", priotiry: 20f, 2f);
                return;
            }
        }

        if(currentLevel >= data.MaxLevel)
        {
            HintManager.Instance?.RegisterHint(this, $"{data.NodeName} уже на макс. уровне", priotiry: 10f, 1.5f);
            return;
        }

        if(EconomyManager.Instance == null || EconomyManager.Instance.SkillPoints < data.SkillPointsForOneLevel)
        {
            HintManager.Instance?.RegisterHint(this, "Недостаточно очков навыков", priotiry: 15f, duration: 2f);
            return;
        }

        bool success = EconomyManager.Instance.SpendSkillPoints(data.SkillPointsForOneLevel);
        if (!success) return;

        if(SkillManager.Instance != null)
        {
            SkillManager.Instance.UpgradeSkill(data);
            int newLevel = SkillManager.Instance.GetSkillLevel(data);

            ApplySkillEffect(data, newLevel);
            RefreshAllSkills(EconomyManager.Instance.SkillPoints);
            HintManager.Instance?.RegisterHint(this, $"{data.NodeName} улучшен (Ур. {newLevel})", priotiry: 15f, duration: 2f);
        }
    }

    // Map

    public void OnMapButtonClicked() => mapUI.OpenMap();
}
