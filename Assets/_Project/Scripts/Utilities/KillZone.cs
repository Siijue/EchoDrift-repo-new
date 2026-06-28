using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private float damageAmount = 999f;
    [SerializeField] private bool instantKill = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health == null) return;

        if (instantKill)health.TakeDamage(999f);

        else health.TakeDamage(damageAmount);
    }
}