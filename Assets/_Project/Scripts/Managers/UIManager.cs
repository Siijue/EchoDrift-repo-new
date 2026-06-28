using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private Transform healthContainer;
    [SerializeField] private GameObject heartPrefab;

    [Header("Heart sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartThreeQuarters;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartQuarter;
    [SerializeField] private Sprite heartEmpty;

    [Header("Local mechanic")]
    [SerializeField] private Image torchFillImg;
    [SerializeField] private TextMeshProUGUI torchTimerText;

    [Header("Hint")]
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private float hintDuration;

    [Header("Economy")]
    [SerializeField] private TextMeshProUGUI textEcho;
    [SerializeField] private TextMeshProUGUI textXP;

    [Header("Floating Text")]
    [SerializeField] private GameObject floatTextPrefab;
    [SerializeField] private float floatingDuration = 1.5f;
    [SerializeField] private float floatingRiseDistance = 50f;
    [SerializeField] private Canvas canvas;

    [Header("Boss")]
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI bossSubnameText;
    [SerializeField] private Image bossHealthBar;
    [SerializeField] private float bossPanelFadeDuration = 0.5f;

    private List<Image> heartImages = new List<Image>();

    private Coroutine hintCoroutine;

    private bool _isSubscribedToEconomy = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnScebeLoaded;
    }

    private void Start()
    {
        //upd
        RefreshUIReferences();

        if (hintText != null) hintText.text = "";

        if (EconomyManager.Instance != null && !_isSubscribedToEconomy)
        {
            EconomyManager.Instance.OnEchoChanged.AddListener(UpdateEchoText);
            EconomyManager.Instance.OnXPChanged.AddListener(UpdateXPText);
            _isSubscribedToEconomy = true;
            UpdateEchoText(EconomyManager.Instance.CurrentEcho);
            UpdateXPText(EconomyManager.Instance.CurrentXP, EconomyManager.Instance.CurrentLevel);
        }
    }

    public void InitializeHealth(int maxHealthInUnits)
    {
        if (healthContainer == null)
        {
            RefreshUIReferences();
            if (healthContainer == null) return;
        }

        foreach (Transform child in healthContainer)
            if (child != null) Destroy(child.gameObject);

        heartImages.Clear();

        int heartCount = maxHealthInUnits / 4;

        for (int i = 0; i < heartCount; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, healthContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImages.Add(heartImage);
        }
    }

    public void UpdateHealth(int currentHealthInUnits, int maxHealthInUnits)
    {
        int currentHearts = currentHealthInUnits / 4;
        int maxHearts = Mathf.CeilToInt(maxHealthInUnits / 4f);

        while (heartImages.Count < maxHearts)
        {
            GameObject heartObj = Instantiate(heartPrefab, healthContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            heartImages.Add(heartImage);
        }

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < maxHearts)
            {
                int remainingHealth = currentHealthInUnits - (i * 4);
                int heartUnits = Mathf.Clamp(remainingHealth, 0, 4);

                SetHeartSprite(heartImages[i], heartUnits);
                heartImages[i].gameObject.SetActive(true);
            }
            else
            {
                heartImages[i].gameObject.SetActive(false);
            }
        }
    }


    private void SetHeartSprite(Image heartImage, int units)
    {
        Sprite selectedSprite = units switch
        {
            4 => heartFull,
            3 => heartThreeQuarters,
            2 => heartHalf,
            1 => heartQuarter,
            0 => heartEmpty,
            _ => heartEmpty
        };

        heartImage.sprite = selectedSprite;
    }

    public void UpdateTorchTimer(float currentTime, float maxTime)
    {
        if (maxTime <= 0) maxTime = 1f;
        int timeForText = Mathf.CeilToInt(currentTime);

        if (torchTimerText != null)
        {
            torchTimerText.text = $"{timeForText} сек";
            if (currentTime <= 5) torchTimerText.color = Color.red;
            else torchTimerText.color = Color.white;
        }

        if (torchFillImg != null && torchFillImg.type == Image.Type.Filled)
        {
            torchFillImg.fillAmount = currentTime / maxTime;
        }
    }

    public void ShowHint(string message, float duration = -1)
    {
        if (duration < 0) duration = hintDuration;

        if (duration == 0)
        {
            if (hintCoroutine != null) StopCoroutine(hintCoroutine);

            hintText.text = message;
            hintText.gameObject.SetActive(true);
            hintCoroutine = null;
            return;
        }

        if (hintCoroutine != null) StopCoroutine(hintCoroutine);

        hintText.text = message;
        hintText.gameObject.SetActive(true);
        hintCoroutine = StartCoroutine(HideHintAfterDuration(duration));

    }

    public void HideHint()
    {
        if (this == null) return;

        if (hintText == null)
        {
            hintCoroutine = null;
            return;
        }

        if (hintCoroutine != null) StopCoroutine(hintCoroutine);
        hintText.gameObject.SetActive(false);
        hintCoroutine = null;
    }

    private IEnumerator HideHintAfterDuration(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        hintText.gameObject.SetActive(false);
        hintCoroutine = null;
    }

    private void UpdateEchoText(int newEcho)
    {
        if (!IsReferenceValid(textEcho)) return;
        textEcho.text = $"{newEcho} эхо";
    }

    private void UpdateXPText(int newXP, int newLevel)
    {
        if (!IsReferenceValid(textXP) || EconomyManager.Instance == null) return;
        int[] thresholds = EconomyManager.Instance.LevelThresholds;
        int next = (newLevel <= thresholds.Length) ? thresholds[newLevel - 1] : 0;
        textXP.text = next > 0 ? $"Ур. {newLevel} | XP: {newXP}/{next}" : $"Ур. {newLevel} | XP: MAX";
    }


    public void ShowFloatingText(string text, Color color = default)
    {
        if (floatTextPrefab == null) return;
        if (color == default) color = new Color(1f, 0.85f, 0.2f);

        Canvas targetCanvas = floatTextPrefab != null ? canvas : GetComponent<Canvas>();
        GameObject textObj = Instantiate(floatTextPrefab, targetCanvas.transform);

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        RectTransform rectTransform = textObj.GetComponent<RectTransform>();

        rectTransform.anchoredPosition = Vector2.zero;


        StartCoroutine(AnimateFloatingText(textObj, rectTransform));
    }

    private IEnumerator AnimateFloatingText(GameObject textObj, RectTransform rectTr)
    {
        if (rectTr == null) yield break;

        Vector2 startPos = rectTr.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, floatingRiseDistance);

        float elapsed = 0f;
        TextMeshProUGUI temp = textObj.GetComponent<TextMeshProUGUI>();
        Color origColor = temp != null ? temp.color : Color.white;

        while (elapsed < floatingDuration)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / floatingDuration;

            rectTr.anchoredPosition = Vector2.Lerp(startPos, endPos, time);

            if (temp != null)
            {
                Color color = origColor;
                color.a = 1f - time;
                temp.color = color;
            }

            float scale = Mathf.Lerp(1f, 1.2f, time < 0.2f ? time * 5f : 1f);
            textObj.transform.localScale = Vector3.one * scale;

            yield return null;
        }
        Destroy(textObj);
    }


    public void RefreshAllUI()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null)
            {
                UpdateHealth(health.CurrentHealthInUnits, health.MaxHealthInUnits);
            }
        }
        if (player != null)
        {
            float torchTime = player.GetTorchTime();
            float maxTorchTime = player.GetTorchMaxTime();
            UpdateTorchTimer(torchTime, maxTorchTime);
        }
    }

    private void OnScebeLoaded(Scene scene, LoadSceneMode mode)
    {
        if(_isSubscribedToEconomy && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEchoChanged.RemoveListener(UpdateEchoText);
            EconomyManager.Instance.OnXPChanged.RemoveListener(UpdateXPText);
            _isSubscribedToEconomy = false;
        }

        ClearAllReferences();
        RefreshUIReferences();

        if(EconomyManager.Instance != null && !_isSubscribedToEconomy)
        {
            EconomyManager.Instance.OnEchoChanged.AddListener(UpdateEchoText);
            EconomyManager.Instance.OnXPChanged.AddListener(UpdateXPText);
            _isSubscribedToEconomy = true;

            UpdateEchoText(EconomyManager.Instance.CurrentEcho);
            UpdateXPText(EconomyManager.Instance.CurrentXP, EconomyManager.Instance.CurrentLevel);
        }

        if (scene.name == "Forest-Scene-Placeholder[v.0.4.0]") StartCoroutine(DelayedRefreshUI());
    }

    private IEnumerator DelayedRefreshUI()
    {
        yield return null;
        RefreshAllUI();
    }


    public void RefreshUIReferences()
    {
        if (!IsReferenceValid(canvas))
        {
            canvas = FindCanvasInActiveScene();
            if (canvas == null) return;
        }

        if (!IsReferenceValid(healthContainer))
        {
            Transform healthTransform = canvas.transform.Find("HealthContainer");
            if (healthTransform != null) healthContainer = healthTransform;
        }

        if (!IsReferenceValid(hintText))
        {
            Transform hintTransform = canvas.transform.Find("HintText");
            if (hintTransform != null)
            {
                hintText = hintTransform.GetComponent<TextMeshProUGUI>();
                if (hintText != null)
                {
                    hintText.text = "";
                    hintText.gameObject.SetActive(false);
                    Debug.Log("[UIManager] hintText найден и очищен");
                }
            }
        }

        if (!IsReferenceValid(bossPanel))
        {
            Transform bossTransform = canvas.transform.Find("BossHealthData");
            if (bossTransform != null) bossPanel = bossTransform.gameObject;
        }

        if (!IsReferenceValid(torchTimerText))
        {
            Transform t = canvas.transform.Find("LocalMechanic/LocMechText");
            if (t != null) torchTimerText = t.GetComponent<TextMeshProUGUI>();
        }

        if (!IsReferenceValid(textEcho))
        {
            Transform t = canvas.transform.Find("RewardsContainer/EchoText");
            if (t != null) textEcho = t.GetComponent<TextMeshProUGUI>();
        }

        if (!IsReferenceValid(textXP))
        {
            Transform t = canvas.transform.Find("RewardsContainer/XPtext");
            if (t != null) textXP = t.GetComponent<TextMeshProUGUI>();
        }

        if (!IsReferenceValid(torchFillImg))
        {
            Transform t = canvas.transform.Find("LocalMechanic/LocMechFill");
            if (t != null) torchFillImg = t.GetComponent<Image>();
        }

        if (!IsReferenceValid(bossNameText) && IsReferenceValid(bossPanel))
        {
            Transform t = bossPanel.transform.Find("BossHealthData/BossNameText");
            if (t != null) bossNameText = t.GetComponent<TextMeshProUGUI>();
        }

        if (!IsReferenceValid(bossSubnameText) && IsReferenceValid(bossPanel))
        {
            Transform t = bossPanel.transform.Find("BossHealthData/BossSubnameText");
            if (t != null) bossSubnameText = t.GetComponent<TextMeshProUGUI>();
        }

        if (!IsReferenceValid(bossHealthBar) && IsReferenceValid(bossPanel))
        {
            Transform t = bossPanel.transform.Find("BossHealthData/BossHealthBar");
            if (t != null) bossHealthBar = t.GetComponent<Image>();
        }
    }

    private Canvas FindCanvasInActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();

        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            if (root.scene.name == "DontDestroyOnLoad") continue;

            Canvas cnvs = root.GetComponentInChildren<Canvas>(true);
            if (cnvs != null) return cnvs;
        }

        return null;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnScebeLoaded;

        if (_isSubscribedToEconomy && EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnEchoChanged.RemoveListener(UpdateEchoText);
            EconomyManager.Instance.OnXPChanged.RemoveListener(UpdateXPText);
            _isSubscribedToEconomy = false;
        }
    }

    private void ClearAllReferences()
    {
        canvas = null;
        healthContainer = null;
        hintText = null;
        bossPanel = null;
        torchTimerText = null;
        textEcho = null;
        textXP = null;
        torchFillImg = null;
        bossNameText = null;
        bossSubnameText = null;
        bossHealthBar = null;
        heartImages.Clear();
    }

    private bool IsReferenceValid<T>(T obj) where T : Object => obj != null && obj.Equals(null) == false;

    //boss
    public void ShowBossInfo(string name, string subname, int maxHP)
    {
        if (bossPanel == null) return;

        CanvasGroup canvasGr = bossPanel.GetComponent<CanvasGroup>();
        if (canvasGr != null) canvasGr.alpha = 1f;

        bossPanel.SetActive(true);

        if (bossNameText != null) bossNameText.text = name;
        if (bossSubnameText != null) bossSubnameText.text = subname;

        UpdateBossHealth(maxHP, maxHP);
    }

    public void UpdateBossHealth(int currentHP, int maxHP)
    {
        if (bossHealthBar == null) return;

        float fill = Mathf.Clamp01((float)currentHP / maxHP);

        bossHealthBar.fillAmount = fill;

        if (fill <= 0.25f) bossHealthBar.color = new Color(1f, 0.3f, 0.3f);
        else if (fill <= 0.5f) bossHealthBar.color = new Color(1f, 0.7f, 0.2f);
        else bossHealthBar.color = Color.darkRed;
    }

    public void HideBossInfo()
    {
        if (bossPanel == null) return;
        bossPanel.SetActive(false);
    }
}

