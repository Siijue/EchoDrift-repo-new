using UnityEngine;

public class InteractButton : MonoBehaviour, IInteractable
{
    public enum ActivationMode
    {
        PressE,
        StepOn,
        LightOn,
        AutoOnStart
    }

    public enum ButtonState
    {
        Idle,
        Pressed,
        Locked
    }

    [SerializeField] private ActivationMode mode = ActivationMode.PressE;
    [SerializeField] private string eventName = "ButtonPressed";
    [SerializeField] private string releaseEvent = "ButtonReleased";
    [SerializeField] private bool isOneShot = true;
    [SerializeField] private bool isToggle = false;
    [SerializeField] private float holdDuration = 0f;
    [SerializeField] private GameObject idleVisual;
    [SerializeField] private GameObject pressedVisual;
    [SerializeField] private float pressDistance = 0.2f;
    [SerializeField] private string hintText = "[E] Нажать";
    [SerializeField] private float hintDuration = 2f;

    private ButtonState currentState = ButtonState.Idle;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Vector3 startPosition;

    private float holdTimer = 0f;
    private bool isBeingHeld = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;

        UpdateVisuals();
    }

    private void Update()
    {
        if (mode == ActivationMode.StepOn && currentState != ButtonState.Locked)
        {
            bool isPressed = IsSomethingOnButton();
            if (isPressed && currentState == ButtonState.Idle) Press();
            else if (!isPressed && currentState == ButtonState.Pressed && !isOneShot) Release();
        }

        if (mode == ActivationMode.LightOn && currentState != ButtonState.Locked)
        {
            bool isLit = IsLitByPlayer();
            if (isLit && currentState == ButtonState.Idle) Press();
            else if (!isLit && currentState == ButtonState.Pressed && !isOneShot) Release();
        }

        if (mode == ActivationMode.PressE && isBeingHeld && holdDuration > 0f)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration && currentState == ButtonState.Idle)
            {
                Press();
                isBeingHeld = false;
            }
        }
    }
    public void Interact(PlayerController player)
    {
        if (mode != ActivationMode.PressE) return;
        if (currentState == ButtonState.Locked) return;

        if (holdDuration > 0f)
        {
            isBeingHeld = true;
            holdTimer = 0f;
            return;
        }

        if (isToggle && currentState == ButtonState.Pressed) Release();
        else if (currentState == ButtonState.Idle) Press();
    }

    public void Press()
    {
        if (currentState == ButtonState.Locked) return;

        currentState = isOneShot ? ButtonState.Locked : ButtonState.Pressed;

        UpdateVisuals();
        AnimatePress();

        GameEventBus.Instance?.SendEvent(eventName, this);

        if (!isToggle && !isOneShot && mode == ActivationMode.PressE) Invoke(nameof(Release), 0.1f);

        if (isOneShot) HintManager.Instance?.RemoveHintsFromSource(this);
    }

    public void Release()
    {
        if (currentState != ButtonState.Pressed) return;

        currentState = ButtonState.Idle;

        UpdateVisuals();
        AnimateRelease();

        if (!string.IsNullOrEmpty(releaseEvent)) GameEventBus.Instance?.SendEvent(releaseEvent, this);

    }

    public void Reset()
    {
        currentState = ButtonState.Idle;
        UpdateVisuals();
        AnimateRelease();
    }


    private void UpdateVisuals()
    {
        if (idleVisual != null) idleVisual.SetActive(currentState == ButtonState.Idle);
        if (pressedVisual != null) pressedVisual.SetActive(currentState != ButtonState.Idle);
    }

    private void AnimatePress() => transform.position = startPosition - new Vector3(0, pressDistance, 0);

    private void AnimateRelease() => transform.position = startPosition;

    private bool IsSomethingOnButton()
    {
        if (col == null) return false;
        return col.IsTouchingLayers(LayerMask.GetMask("Player", "Enemy"));
    }

    private bool IsLitByPlayer()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(transform.position, 2f, LayerMask.GetMask("Player"));
        if (playerCol == null) return false;

        PlayerController player = playerCol.GetComponent<PlayerController>();
        return player != null && player.IsTorchLit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mode == ActivationMode.PressE && other.CompareTag("Player") && currentState == ButtonState.Idle) HintManager.Instance?.RegisterHint(this, hintText, 3, hintDuration);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HintManager.Instance?.RemoveHintsFromSource(this);

            if (isBeingHeld)
            {
                isBeingHeld = false;
                holdTimer = 0f;
            }
        }
    }
}