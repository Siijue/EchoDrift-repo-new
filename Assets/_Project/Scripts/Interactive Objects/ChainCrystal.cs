using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ChainCrystal : MonoBehaviour, IInteractable
{
    public enum CrystalState { Inactive, Charging, Active }
    public enum BeamDirection { Up, Down, Left, Right, Custom }
    public enum ActivationSource { TorchLight, InteractE, Both }

    [Header("Состояние")]
    [SerializeField] private CrystalState currentState = CrystalState.Inactive;

    [Header("Настройки луча")]
    [SerializeField] private BeamDirection beamDirection = BeamDirection.Right;
    [SerializeField] private Vector2 customDirection = Vector2.right;
    [SerializeField] private float beamRange = 5f;

    [Header("Тайминги")]
    [SerializeField] private float chargeTime = 0.5f;
    [SerializeField] private float beamDuration = 0.5f;
    [SerializeField] private float activationDelay = 0.2f;

    [Header("Активация игроком")]
    [SerializeField] private ActivationSource activationSource = ActivationSource.TorchLight;
    [SerializeField] private LayerMask playerLayer;

    [Header("Подсказки")]
    [SerializeField] private string hintText = "[E] Активировать кристалл";
    [SerializeField] private float hintDuration = 2f;

    [Header("Визуал")]
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite chargingSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color chargingColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color activeColor = new Color(0.2f, 0.8f, 1f);
    [SerializeField] private Color beamColor = new Color(0.3f, 0.9f, 1f, 0.8f);

    [Header("Light2D луча")]
    [SerializeField] private Light2D beamLight;
    [SerializeField] private float beamIntensity = 2f;
    [SerializeField] private bool isPermanentBeam = false;

    [Header("События")]
    [SerializeField] private string activationEvent = "CrystalActivated";
    [SerializeField] private bool sendEventOnActivate = true;

    [Header("Слой кристаллов")]
    [SerializeField] private LayerMask crystalLayer;

    [SerializeField] private InteractablesSaveTracker tracker;

    private SpriteRenderer spriteRenderer;
    private float chargeTimer = 0f;
    private float beamTimer = 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (beamLight == null) beamLight = GetComponent<Light2D>();
        if (beamLight != null) beamLight.enabled = false;

        if (tracker == null) tracker = GetComponent<InteractablesSaveTracker>();

        UpdateVisual();
    }

    private void Start()
    {
        if (tracker != null) GameEventBus.Instance?.Subscribe($"ObjectActivated_{tracker.ObjectID}", OnRestoredFromSave);
    }

    private void Update()
    {

        switch (currentState)
        {
            case CrystalState.Charging:
                chargeTimer += Time.deltaTime;

                float pulse = Mathf.PingPong(Time.time * 8f, 1f);
                spriteRenderer.color = Color.Lerp(chargingColor, Color.white, pulse);

                if (chargeTimer >= chargeTime)FireBeam();
                break;

            case CrystalState.Active:
                if (!isPermanentBeam)
                {
                    beamTimer += Time.deltaTime;

                    if (beamLight != null)
                    {
                        float fadeProgress = beamTimer / beamDuration;
                        beamLight.intensity = beamIntensity * (1f - fadeProgress);
                    }

                    if (beamTimer >= beamDuration) HideBeam();
                }
                break;
        }
    }

    public void Interact(PlayerController player)
    {
        if (currentState != CrystalState.Inactive) return;
        if (player == null) return;

        bool canActivate = false;

        switch (activationSource)
        {
            case ActivationSource.TorchLight:
                canActivate = player.IsTorchLit();
                if (!canActivate)
                {
                    UIManager.Instance?.ShowHint("Нужна горящая лучина!", 2f);
                }
                break;

            case ActivationSource.InteractE:
                canActivate = true;
                break;

            case ActivationSource.Both:
                canActivate = true;
                break;
        }

        if (canActivate)
        {
            Activate();
            HintManager.Instance?.RemoveHintsFromSource(this);
        }
    }

    public void Activate()
    {
        if (currentState != CrystalState.Inactive) return;

        currentState = CrystalState.Charging;
        chargeTimer = 0f;

        if (tracker != null) GameEventBus.Instance?.SendEvent($"Activate_{tracker.ObjectID}", this);

        UpdateVisual();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentState == CrystalState.Inactive)
        {
            Debug.Log("Сообщение от кристалла");
            HintManager.Instance?.RegisterHint(this, hintText, 10, hintDuration);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HintManager.Instance?.RemoveHintsFromSource(this);
        }
    }

    private void FireBeam()
    {

        currentState = CrystalState.Active;
        beamTimer = 0f;

        UpdateVisual();
        ShowBeam();

        Vector2 direction = GetDirection();

        float offset = 0.5f;
        Vector2 startPosition = (Vector2)transform.position + direction * offset;


        RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, direction, beamRange - offset, crystalLayer);


        ChainCrystal nextCrystal = null;
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != gameObject)
            {
                nextCrystal = hit.collider.GetComponent<ChainCrystal>();
                if (nextCrystal != null) break;
            }
        }

        if (nextCrystal != null)
        {
            if (nextCrystal.currentState == CrystalState.Inactive)Invoke(nameof(ActivateNextCrystal), activationDelay);
        }

        if (sendEventOnActivate)
        {
            GameEventBus.Instance?.SendEvent(activationEvent, this);
        }
    }

    private void ActivateNextCrystal()
    {
        Vector2 direction = GetDirection();

        float offset = 0.5f;
        Vector2 startPosition = (Vector2)transform.position + direction * offset;


        RaycastHit2D[] hits = Physics2D.RaycastAll(startPosition, direction, beamRange - offset, crystalLayer);

        ChainCrystal nextCrystal = null;
        foreach (RaycastHit2D hit in hits)
        {

            if (hit.collider.gameObject != gameObject)
            {
                nextCrystal = hit.collider.GetComponent<ChainCrystal>();
                if (nextCrystal != null) break;
            }
        }
        if (nextCrystal != null)
        {
            nextCrystal.Activate();
        }
    }

    private Vector2 GetDirection()
    {
        return beamDirection switch
        {
            BeamDirection.Up => Vector2.up,
            BeamDirection.Down => Vector2.down,
            BeamDirection.Left => Vector2.left,
            BeamDirection.Right => Vector2.right,
            BeamDirection.Custom => customDirection.normalized,
            _ => Vector2.right
        };
    }

    private void ShowBeam()
    {
        if (beamLight == null) return;

        beamLight.lightType = Light2D.LightType.Point;
        beamLight.color = beamColor;
        beamLight.intensity = beamIntensity;
        beamLight.pointLightOuterRadius = beamRange;
        beamLight.pointLightInnerRadius = beamRange * 0.3f;

        beamLight.enabled = true;
    }

    private void HideBeam()
    {
        if (beamLight != null) beamLight.enabled = false;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        switch (currentState)
        {
            case CrystalState.Inactive:
                spriteRenderer.sprite = inactiveSprite;
                spriteRenderer.color = inactiveColor;
                break;
            case CrystalState.Charging:
                spriteRenderer.sprite = chargingSprite;
                break;
            case CrystalState.Active:
                spriteRenderer.sprite = activeSprite;
                spriteRenderer.color = activeColor;
                break;
        }
    }

    public void ResetCrystal()
    {
        if (currentState == CrystalState.Inactive) return;
        currentState = CrystalState.Inactive;
        chargeTimer = 0f;
        beamTimer = 0f;

        if (tracker != null) GameEventBus.Instance?.SendEvent($"Deactivate_{tracker.ObjectID}", this);

        HideBeam();
        UpdateVisual();
    }

    private void OnRestoredFromSave(object sender)
    {
        if (this == null) return;

        if (currentState == CrystalState.Active) return;
        currentState = CrystalState.Active;
        beamTimer = 0f;

        UpdateVisual();
        ShowBeam();
    }
    private void OnDestroy()
    {
        if (tracker != null) GameEventBus.Instance?.Unsubscribe($"ObjectActivated_{tracker.ObjectID}", OnRestoredFromSave);
    }


    private void OnDrawGizmosSelected()
    {
        Vector2 direction = GetDirection();
        Vector2 endPosition = (Vector2)transform.position + direction * beamRange;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, endPosition);
        Gizmos.DrawWireSphere(endPosition, 0.2f);
    }
}