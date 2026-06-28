using UnityEngine;
using System.Collections;

public class MoldSporeExplodeState : IMoldState
{
    private readonly MoldAI _owner;
    private bool _exploded;

    public MoldSporeExplodeState(MoldAI owner) => _owner = owner;

    public void Enter()
    {
        if(_exploded) return;
        _exploded = true;

        _owner.rb.linearVelocity = Vector2.zero;
        _owner.rb.simulated = false;
        _owner.coll.enabled = false;

        Explode();

        _owner.StartCoroutine(DestroyAfterDelay());
    }

    public void Update() { }
    public void Exite() { }

    private void Explode()
    {
        if(_owner.sporePrefab != null) Object.Instantiate(_owner.sporePrefab, _owner.transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(_owner.transform.position, _owner.explosionRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            health?.TakeDamage(_owner.explosionDamage);

            PlayerController playerController = hit.GetComponent<PlayerController>();
            if(playerController != null)
            {
                Vector2 knockDir = ((Vector2)hit.transform.position - (Vector2)_owner.transform.position).normalized;
                playerController.ApplyKnockback(knockDir, _owner.explosionKnockback);
            }

            StatusDataSystem statuses = hit.GetComponent<StatusDataSystem>();
            statuses?.AddStatus(StatusData.CreateCough());
        }

        EconomyManager.Instance?.AddEcho(_owner.echoReward);
        EconomyManager.Instance?.AddXP(_owner.xpReward);
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(0.4f);  // подстроить под аним
        Object.Destroy(_owner.gameObject);
    }
}
