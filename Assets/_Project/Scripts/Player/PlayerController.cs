using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(StatusDataSystem))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;

    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpHoldTime = 0.4f;
    [SerializeField] private float jumpHoldMultiplier = 0.2f;
    [SerializeField] private int maxJumps = 2;

    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashCooldown;

    [SerializeField] private float torchMaxTime;
    [SerializeField] private Transform interactPoint;
    [SerializeField] private float interactRadius = 1f;

    [SerializeField] private float knockbackDuration = 0.02f;

    [SerializeField] private SpriteRenderer sprRender;

    private float torchCurrentTime;
    private bool isTorchLit = true;

    private PlayerActions playerActions;
    private InputAction move;
    private InputAction run;
    private InputAction jump;
    private InputAction dash;
    private InputAction interact;
    private Rigidbody2D rb;
    private Light2D torchLight;
    private StatusDataSystem _statuses;

    public Light2D TorchLight => torchLight;

    private int jumpsRemaining;
    private bool isJumpHolding;
    private float jumpHoldTimer;

    private bool canDash = true;
    private bool isDashing = false;

    private bool isKnockedBack;
    private float knockbackEndTime;
    private bool isInputBlocked;

    private bool _inputInverted = false;
    private bool _torchVisualEnabled = true;

    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    private float _environmentSpeedMult = 1f;
    private float _environmentJumpMult = 1f;

    public bool IsDashing => isDashing;

    void Awake()
    {
        if (!TryGetComponent<Rigidbody2D>(out rb)) Debug.LogError("Rigidbody2D не найден у " + gameObject.name);
        if (!TryGetComponent<Light2D>(out torchLight)) Debug.Log("Компонент света не найден у " + gameObject.name);

        _statuses = GetComponent<StatusDataSystem>();
        sprRender = GetComponent<SpriteRenderer>();
    }

    void Start()
    {

        playerActions = new PlayerActions();

        move = playerActions.PlayerController.Move;
        move.Enable();

        run = playerActions.PlayerController.Run;
        run.Enable();

        jump = playerActions.PlayerController.Jump;
        jump.Enable();

        dash = playerActions.PlayerController.Dash;
        dash.Enable();

        interact = playerActions.PlayerController.Interact;
        interact.Enable();

        torchCurrentTime =torchMaxTime;
        if (UIManager.Instance != null) UIManager.Instance.UpdateTorchTimer(torchCurrentTime, torchMaxTime);
    }

    void Update()
    {
        if (isInputBlocked) return;

        if (dash.triggered && canDash && !isDashing) StartCoroutine(Dash());

        // НАЧАЛО ПРЫЖКА
        if (jump.triggered)
        {
            bool canJump = _statuses == null || _statuses.CanJump;
            if (jumpsRemaining > 0 && canJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * _environmentJumpMult);
                jumpsRemaining--;
                isJumpHolding = true;
                jumpHoldTimer = 0f;
            }
        }

        // УДЕРЖАНИЕ ПРЫЖКА
        if (jump.IsPressed() && isJumpHolding)
        {
            jumpHoldTimer += Time.deltaTime;
            if (jumpHoldTimer >= jumpHoldTime)
            {
                isJumpHolding = false;
            }
        }

        // ОТПУСКАНИЕ ПРЫЖКА
        if (jump.WasReleasedThisFrame() && isJumpHolding)
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y *= jumpHoldMultiplier;
            rb.linearVelocity = velocity;
            isJumpHolding = false;
        }

        if (!isInputBlocked && interact.IsPressed())
        {
            TryInteract();
        }

        UpdateTorch();
    }

    void FixedUpdate()
    {
        if (isInputBlocked) return;
        if (isDashing) return;

        if (isKnockedBack && Time.time >= knockbackEndTime)
        {
            isKnockedBack = false;
        }

        bool canMove = _statuses == null || _statuses.CanMove;
        float statusSpeedMult = _statuses != null ? _statuses.SpeedMultiplyer : 1f;
        float speedMult = statusSpeedMult * _environmentSpeedMult;

        if (!canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Vector2 axis = move.ReadValue<Vector2>();
        if (_inputInverted) axis.x = -axis.x;

        if (isKnockedBack)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y);
            return;
        }

        if (axis.x != 0)
        {
            if (sprRender != null) sprRender.flipX = axis.x < 0;
            if (run.IsPressed()) Run(axis, speedMult);
            else Move(axis, speedMult);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (currentPlatform != null)
        {
            Rigidbody2D platformRb = currentPlatform.GetComponent<Rigidbody2D>();
            Vector3 platformVelocity;

            if (platformRb != null)
            {
                platformVelocity = platformRb.linearVelocity;
            }
            else
            {
                platformVelocity = (currentPlatform.position - lastPlatformPosition) / Time.fixedDeltaTime;
                lastPlatformPosition = currentPlatform.position;
            }
            float velocityX = rb.linearVelocity.x + platformVelocity.x;
            float velocityY = rb.linearVelocity.y;

            if (platformVelocity.y > 0.1f)
            {
                velocityY = Mathf.Max(rb.linearVelocity.y, platformVelocity.y);
            }
            else if (platformVelocity.y < -0.1f)
            {
                velocityY = Mathf.Min(rb.linearVelocity.y, platformVelocity.y);
            }

            rb.linearVelocity = new Vector2(velocityX, velocityY);
        }
    }

    private void Move(Vector2 axis, float speedMult = 1f)
    {
        rb.linearVelocity = new Vector2(axis.x * walkSpeed * speedMult, rb.linearVelocity.y);
    }

    private void Run(Vector2 axis, float speedMult = 1f)
        => rb.linearVelocity = new Vector2(axis.x * runSpeed * speedMult, rb.linearVelocity.y);

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            jumpsRemaining = maxJumps;
            isJumpHolding = false;
        }
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            jumpsRemaining = maxJumps;
            isJumpHolding = false;
            transform.SetParent(collision.transform);
            currentPlatform = collision.transform;
            lastPlatformPosition = currentPlatform.position;
            rb.gravityScale = 0;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            jumpsRemaining = maxJumps;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null, true);
            currentPlatform = null;
            rb.gravityScale = 1;
        }
    }

    private IEnumerator Dash()
    {
        Debug.Log("DASH");

        _statuses?.RemoveStatus(StatusType.Root);

        isDashing = true;
        canDash = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        float direction = move.ReadValue<Vector2>().x;

        if (direction == 0)
        {
            direction = transform.localScale.x > 0 ? 1 : -1;
        }

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void InginteTorch()
    {
        isTorchLit = true;
        torchCurrentTime = torchMaxTime;

        if (torchLight != null) torchLight.enabled = true;
        GetComponent<LightSource>().enabled = true;

        UIManager.Instance.UpdateTorchTimer(torchCurrentTime, torchMaxTime);
    }

    public void ExtinguishTorch()
    {
        isTorchLit = false;

        if (torchLight != null) torchLight.enabled = false;
        GetComponent<LightSource>().enabled = false;
    }

    private void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(interactPoint.position, interactRadius);

        IInteractable closestInteractable = null;
        float closestDistance = Mathf.Pow(1000, 1000);

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable != null)
            {
                float distance = Vector2.Distance(interactPoint.position, hit.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        if (closestInteractable != null) closestInteractable.Interact(this);
    }

    private void UpdateTorch()
    {
        if (!isTorchLit) return;

        torchCurrentTime -= Time.deltaTime;
        UIManager.Instance.UpdateTorchTimer(torchCurrentTime, torchMaxTime);

        if (torchCurrentTime <= 0)
        {
            ExtinguishTorch();
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        if (isKnockedBack) return;

        isKnockedBack = true;
        knockbackEndTime = Time.time + knockbackDuration;

        rb.AddForce(direction * force, ForceMode2D.Impulse);

        Debug.Log("игрок отброшен");
    }

    public void SetInputBlocked(bool blocked)
    {
        isInputBlocked = blocked;
        Debug.Log($"PlayerController: Ввод {(blocked ? "заблокирован" : "разблокирован")}");

        if (isInputBlocked) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public bool IsInputEnabled => !isInputBlocked;

    public void ExtendTorchTime(float seconds)
    {
        if (seconds <= 0) return;

        if (!isTorchLit) InginteTorch();

        torchCurrentTime += seconds;

        torchCurrentTime = Mathf.Min(torchCurrentTime, torchMaxTime * 2f);

        UIManager.Instance?.UpdateTorchTimer(torchCurrentTime, torchMaxTime);
    }

    public void DrainTorch(float seconds)
    {
        if (seconds <= 0) return;
        if (!isTorchLit) return;

        torchCurrentTime -= seconds;

        if(torchCurrentTime <= 0f)
        {
            torchCurrentTime = 0f;
            ExtinguishTorch();
        }

        UIManager.Instance?.UpdateTorchTimer(torchCurrentTime, torchMaxTime);
    }

    public bool IsTorchLit() => isTorchLit;

    public void SetInputInverted(bool inverted) => _inputInverted = inverted;

    public void SetTorchVisualOnly(bool visible)
    {
        _torchVisualEnabled = visible;
        if(torchLight != null) torchLight.enabled = visible;
    }

    public float GetTorchTime() => torchCurrentTime;
    public float GetTorchMaxTime() => torchMaxTime;

    public void SetEnvironmentSpeedMult(float mult) => _environmentSpeedMult = Mathf.Clamp(mult, 0.1f, 3f);
    public void SetEnvironmentJumpMult(float mult) => _environmentJumpMult = Mathf.Clamp(mult, 0.1f, 2f);
    public void ResetEnvironmentEffects()
    {
        _environmentSpeedMult = 1f;
        _environmentJumpMult = 1f;
    }

    // save
    public void LoadFromSaveData(SaveData data)
    {
        if (data == null) return;
        if (data.playerPosition != Vector3.zero)
        {
            transform.position = data.playerPosition;
        }

        ResetEnvironmentEffects();

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.SetHealth(data.currentHealth, data.maxHealth);
        }

        if (data.torchMaxTime > 0)
        {
            torchMaxTime = data.torchMaxTime;
            torchCurrentTime = data.torchCurrentTime > 0 ? data.torchCurrentTime : torchMaxTime;
        }

        if (data.activeCheckpoints != null && data.activeCheckpoints.Count > 0)
        {
            RestoreCheckpoints(data.activeCheckpoints);
        }
    }

    private void RestoreCheckpoints(List<string> checkpointIDs)
    {
        var allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        int restoredCount = 0;

        foreach (var checkpoint in allCheckpoints)
        {
            if (checkpoint != null && checkpointIDs.Contains(checkpoint.CheckpointID))
            {
                checkpoint.ActivateFromSave();
                MapManager.Instance?.RegisterActivatedCheckpoint(checkpoint.CheckpointID);
                restoredCount++;
            }
        }

        Debug.Log($"[PlayerController] Восстановлено {restoredCount} чекпоинтов");
    }

    private void OnDestroy()
    {
        if (playerActions != null)
        {
            playerActions.PlayerController.Disable();
            playerActions.Dispose();
            playerActions = null;
        }
    }
}