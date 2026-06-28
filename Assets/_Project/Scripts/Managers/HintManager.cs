using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HintManager : MonoBehaviour    // синглтон
{
    public static HintManager Instance { get; private set; }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private class ActiveHint
    {
        public string Text;
        public float Priority;
        public float Duration;
        public MonoBehaviour Source;

        public ActiveHint(string text, float priority, float duration, MonoBehaviour source)
        {
            Text = text;
            Priority = priority;
            Duration = duration;
            Source = source;
        }
    }

    private readonly List<ActiveHint> _activeHints = new List<ActiveHint>();
    private float cooldownTimer = 0;
    private string currentText = "";


    [SerializeField] private Transform playerTransform;

    [SerializeField] private float maxHintDistance = 1f;
    [SerializeField] private float switchDelay = 0.1f;

    // для отладки
    [SerializeField] private bool isDebugLogsEnabled = false;


    private void Start()
    {
        if(playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        UpdateHintSelection();
    }

    private void Update()
    {
        if(cooldownTimer > 0)
        {
            cooldownTimer -= Time.unscaledDeltaTime;
            return;
        }

        bool isChanged = false;

        for(int i = _activeHints.Count - 1; i >= 0; i--)
        {
            if( _activeHints[i].Duration > 0)
            {
                _activeHints[i].Duration -= Time.unscaledDeltaTime;
                if(_activeHints[i].Duration <= 0)
                {
                    _activeHints.RemoveAt(i);
                    isChanged = true;
                }
            }
        }
        if (isChanged) UpdateHintSelection();


        if (_activeHints.Count > 0) UpdateHintSelection();
    }

    private void UpdateHintSelection()
    {
        if (playerTransform == null) return;
        if(_activeHints.Count == 0)
        {
            if(currentText != "")
            {
                UIManager.Instance?.HideHint();
                currentText = "";
                cooldownTimer = switchDelay;
            }
            return;
        }

        ActiveHint highHint = null;
        float highestPriority = 0f;

        foreach(var hint in _activeHints)
        {
            if (hint.Source == null) continue;

            float distance = Vector2.Distance(playerTransform.position, hint.Source.transform.position);
            if (distance > maxHintDistance) continue;

            if(hint.Priority > highestPriority)
            {
                highestPriority = hint.Priority;
                highHint = hint;
            }
        }

        if(highHint != null && highHint.Text != currentText)
        {
            UIManager.Instance?.ShowHint(highHint.Text, highHint.Duration);
            currentText = highHint.Text;
            cooldownTimer = switchDelay;
        }
        else if(highHint == null && currentText != "")
        {
            UIManager.Instance?.HideHint();
            currentText = "";
            cooldownTimer = switchDelay;
        }

    }



    public void RegisterHint(MonoBehaviour source, string text, float priotiry = 0f,  float duration = 0f)
    {
        if (string.IsNullOrEmpty(text)) return;

        RemoveHintsFromSource(source);

        ActiveHint newHint = new ActiveHint(text, priotiry, duration, source);
        _activeHints.Add(newHint);

        if (isDebugLogsEnabled) Debug.Log($"HintManager: добавлена подсказка '{text}' с приоритетом {priotiry}");

        UpdateHintSelection();
    }


    public void RemoveHintsFromSource(MonoBehaviour source)
    {
        if (source == null) return;

        int removedCount = _activeHints.RemoveAll(hint => hint.Source == source);

        if (removedCount > 0 && isDebugLogsEnabled) Debug.Log($"HintManager: удалено подсказок от {source.name} в кол-ве {removedCount}");

        UpdateHintSelection();
    }


    public void ClearAllHints()
    {
        _activeHints.Clear();

        currentText = "";
        cooldownTimer = 0;

        UpdateHintSelection();

        if (isDebugLogsEnabled) Debug.Log($"HintManager: очищены все подсказки");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ClearAllHints();

    private void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;
}
