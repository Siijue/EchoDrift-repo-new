using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private float damage = 1f;
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private Vector2 knockbackDirection = Vector2.up;
    [SerializeField] private float cooldown = 1f;
    [SerializeField] private bool affectsEnemies = true;

    private float lastDamageTime = -10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - lastDamageTime >= cooldown)
        {
            TryDamage(other);
        }
    }

    private void TryDamage(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                if (player != null)
                {
                    player.ApplyKnockback(knockbackDirection.normalized, knockbackForce);
                }
                lastDamageTime = Time.time;
            }
            return;
        }

        if (affectsEnemies && other.CompareTag("Leshy"))
        {
            LeshyAI leshy = other.GetComponent<LeshyAI>();
            if (leshy != null)
            {
                leshy.TakeDamage(1);
                Rigidbody2D rb = other.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 dir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                lastDamageTime = Time.time;
            }
            return;
        }

        if (affectsEnemies)
        {
            MoldAI mold = other.GetComponent<MoldAI>();
            if (mold != null)
            {
                mold.TransitionTo(mold.GetShrinkDeadState());
                Rigidbody2D rb = other.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 dir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                lastDamageTime = Time.time;
            }
        }
    }
}