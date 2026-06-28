using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Torch : MonoBehaviour, IInteractable
{
    public enum TorchState { Extinguished, Lit }

    [Header("Настройки")]
    [SerializeField] private TorchState initialState = TorchState.Extinguished;
    [SerializeField] private string hintText = "[E] Зажечь факел";
    [SerializeField] private float hintDuration = 2f;
    [SerializeField] private float lightRadius = 5f;
    [SerializeField] private Color lightColor = new Color(1f, 0.7f, 0.3f);
    [Header("Визуал")]
    [SerializeField] private Sprite extinguishedSprite;
    [SerializeField] private Sprite litSprite;
    [SerializeField] private Light2D torchLight;

    [Header("События")]
    [SerializeField] private string onLitEvent = "TorchLit";

    [Header("Состояние")]
    [SerializeField] private TorchState currentState;
    [SerializeField] private bool isPermanent = true;
    [SerializeField] private float burnDuration = 10f;

    [Header("Сброс")]
    [SerializeField] private string puzzleResetEvent = "ResetPuzzleEvent";
    [SerializeField] private float resetFlashDuration = 0.3f;

    private float burnTimer;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentState = initialState;
        UpdateVisuals();
    }

    private void Update()
    {
        if (currentState == TorchState.Lit && !isPermanent)
        {
            burnTimer -= Time.deltaTime;
            if (burnTimer <= 0f)
            {
                Extinguish();
            }
        }
    }

    private void OnEnable() => GameEventBus.Instance?.Subscribe(puzzleResetEvent, OnPuzzleReset);

    private void OnDisable() => GameEventBus.Instance?.Unsubscribe(puzzleResetEvent, OnPuzzleReset);

    public void Interact(PlayerController player)
    {
        if (player == null || !player.IsTorchLit())
        {
            return;
        }

        if (currentState == TorchState.Lit) return;

        LightTorch();
    }

    public void LightTorch()
    {
        if (currentState == TorchState.Lit) return;

        currentState = TorchState.Lit;
        burnTimer = burnDuration;

        UpdateVisuals();
        EnableLight();

        GameEventBus.Instance?.SendEvent(onLitEvent, this);

        HintManager.Instance?.RemoveHintsFromSource(this);
    }

    public void Extinguish()
    {
        if (currentState == TorchState.Extinguished) return;

        currentState = TorchState.Extinguished;

        UpdateVisuals();
        DisableLight();

        GameEventBus.Instance?.SendEvent("TorchExtinguished", this);
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = currentState == TorchState.Lit ? litSprite : extinguishedSprite;
        }
    }

    private void EnableLight()
    {
        if (torchLight != null)
        {
            torchLight.enabled = true;
            torchLight.pointLightOuterRadius = lightRadius;
            torchLight.color = lightColor;
        }
    }

    private void DisableLight()
    {
        if (torchLight != null)
        {
            torchLight.enabled = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == TorchState.Extinguished)
        {
            HintManager.Instance?.RegisterHint(this, hintText, priotiry: 3, duration: hintDuration);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HintManager.Instance?.RemoveHintsFromSource(this);
        }
    }

    private void OnPuzzleReset(object sender)
    {
        if (currentState == TorchState.Extinguished) return;
        currentState = TorchState.Extinguished;
        UpdateVisuals();
        DisableLight();
        StartCoroutine(ResetFlash());
    }

    private IEnumerator ResetFlash()
    {
        if (spriteRenderer == null) yield break;
        Color origColor = spriteRenderer.color;
        float elapsed = 0f;
        while (elapsed < resetFlashDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.PingPong(elapsed * 4f, 1f);
            spriteRenderer.color = Color.Lerp(origColor, Color.darkRed, time);
            yield return null;
        }
        spriteRenderer.color = origColor;
    }
}