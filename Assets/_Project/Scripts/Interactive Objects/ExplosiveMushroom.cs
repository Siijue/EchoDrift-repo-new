using UnityEngine;

public class ExplosiveMushroom : MonoBehaviour
{
    public enum MushroomState { Idle, Armed }

    [Header("Настройки взрыва")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionDamage = 0.5f;
    [SerializeField] private float knockbackForce = 10f;

    [SerializeField] private float fuseTime = 1.5f;

    [Header("Визуал и звук")]
    [SerializeField] private Color armedColor = new Color(1f, 0.2f, 0.2f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float maxScale = 1.4f;
    [SerializeField] private float flashDuration = 0.15f;

    private MushroomState currentState = MushroomState.Idle;
    private float currentFuseTime;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialScale;
    private bool isDamaged;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialScale = transform.localScale;
        isDamaged = false;
    }

    private void Update()
    {
        switch (currentState)
        {
            case MushroomState.Idle:
                if (IsPlayerNearbyWithTorch())
                {
                    currentState = MushroomState.Armed;
                    currentFuseTime = fuseTime;
                }
                break;

            case MushroomState.Armed:
                currentFuseTime -= Time.deltaTime;

                float progress = 1f - (currentFuseTime / fuseTime);

                float pulseSpeed = Mathf.Lerp(4f, 25f, progress);
                float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                spriteRenderer.color = Color.Lerp(armedColor, Color.white, pulse);

                float currentScale = Mathf.Lerp(initialScale.x, initialScale.x * maxScale, progress);
                transform.localScale = Vector3.one * currentScale;

                if (currentFuseTime <= 0f)
                {
                    Explode();
                }
                break;
        }
    }

    private bool IsPlayerNearbyWithTorch()
    {
        Collider2D playerCol = Physics2D.OverlapCircle(transform.position, 2f, playerLayer);
        if (playerCol != null)
        {
            PlayerController player = playerCol.GetComponent<PlayerController>();
            return player != null && player.IsTorchLit();
        }
        return false;
    }

    private void Explode()
    {
        spriteRenderer.color = Color.white;
        float finalScale = initialScale.x * maxScale * 1.2f;
        transform.localScale = Vector3.one * finalScale;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (isDamaged) continue;
            if (hit.CompareTag("Player"))
            {
                PlayerController player = hit.GetComponent<PlayerController>();
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position).normalized;
                    player.ApplyKnockback(dir, knockbackForce);

                    if (playerHealth != null) playerHealth.TakeDamage(explosionDamage); isDamaged = true;
                }
            }
            else if (hit.CompareTag("Leshy"))
            {
                Rigidbody2D rb = hit.attachedRigidbody;
                LeshyAI leshy = hit.GetComponent<LeshyAI>();
                if (rb != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                if (leshy != null) leshy.TakeDamage(1); isDamaged = true;
            }
        }
        Destroy(gameObject, flashDuration);
    }
}