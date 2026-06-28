using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 12;
    [SerializeField] private float invincibilityDuration = 1f;
    [SerializeField] private string playerDeathEvent = "PlayerDied";

    private int currentHealth;
    private bool isInvicibility = false;


    private SpriteRenderer spriteRenderer;

    public UnityEvent<int, int> onHealthChanged;
    public UnityEvent onDeath;

    public int CurrentHealthInUnits => currentHealth;
    public int MaxHealthInUnits => maxHealth;
    public int MaxHealthInHealth => maxHealth / 4;
    public int CurrentHealthInHearts => currentHealth / 4;

    void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null) Debug.LogWarning("Компонент SpriteRenderer не найден у " + gameObject.name);
    }

    void Start()
    {
        if(UIManager.Instance != null)
        {
            UIManager.Instance.InitializeHealth(maxHealth);
            onHealthChanged.AddListener(UIManager.Instance.UpdateHealth);
        }
        onDeath.AddListener(OnPlayerDied);
    }

    public void TakeDamage(float dmgInHearts)
    {

        if (isInvicibility) return;

        int dmgInUnits = Mathf.RoundToInt(dmgInHearts * 4f);
        currentHealth = Mathf.Max(0, currentHealth -  dmgInUnits);

        onHealthChanged?.Invoke(currentHealth, maxHealth);
        StartInvicibility();

        if (currentHealth <= 0)
        {
            Die();
            return;
        }
    }

    private void StartInvicibility()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(InvicibilityCoroutine());
    }

    private IEnumerator InvicibilityCoroutine()
    {
        isInvicibility = true;

        StartCoroutine(BlinkEffect());

        yield return new WaitForSeconds(invincibilityDuration);

        isInvicibility = false;
    }

    private IEnumerator BlinkEffect()
    {
        float blinkDuration = invincibilityDuration;
        float blinkInterval = 0.1f;
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;

        }

        spriteRenderer.enabled = true;
    }

    private void OnPlayerDied()
    {
        var player = GetComponent<PlayerController>();
        if (player != null) player.SetInputBlocked(true);
        if (VelesAI.Instance != null) { VelesAI.Instance?.ResetToIdle(); VelesAI.Instance.HideBossUI(); }
        if(LeshyAI.Instance != null) { LeshyAI.Instance.ResetToIdle(); LeshyAI.Instance.HideBossUI(); }
        if (IstAI.Instance != null) { IstAI.Instance?.Despawn(); }
        GameEventBus.Instance?.SendEvent(playerDeathEvent, this);
        if (DeathScreen.Instance != null) DeathScreen.Instance.Show();
    }

    private void Die()
    {
        onDeath?.Invoke();
    }

    public void Heal(float amntInHearts)
    {
        int healInUnits = Mathf.RoundToInt(amntInHearts * 4);
        currentHealth = Mathf.Min(maxHealth, currentHealth + healInUnits);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(int increaseInUnits)
    {
        int maxHealthCap = 48;

        maxHealth = Mathf.Min(maxHealthCap, maxHealth + increaseInUnits);
        currentHealth = Mathf.Min(maxHealth, currentHealth +  increaseInUnits);

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetHealth(int current, int max)
    {
        maxHealth = max;
        currentHealth = Mathf.Clamp(current, 0, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void SetMaxHealthFromData(int newMax) => maxHealth = newMax;
    public void SetCurrenthealthFromData(int newCurrent) => currentHealth = newCurrent;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}
