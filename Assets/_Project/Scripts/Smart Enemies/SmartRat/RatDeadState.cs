using System.Collections;
using UnityEngine;

public class RatDeadState : IRatState
{
    private readonly RatAI _owner;

    private const float destroyDelay = 0.5f;    // подобрать под анимацию


    public RatDeadState(RatAI owner) => _owner = owner;

    public void Enter()
    {
        _owner.rb.linearVelocity = Vector2.zero;
        _owner.rb.simulated = false;

        Collider2D coll = _owner.GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        EconomyManager.Instance?.AddXP(_owner.xpReward);
        EconomyManager.Instance?.AddEcho(_owner.echoReward);

        _owner.StartCoroutine(DestroyAfterDelay());
    }

    public void Update() { }

    public void Exit() { }

    private IEnumerator DestroyAfterDelay()
    {
        float elapsed = 0f;
        while (elapsed < destroyDelay)
        {
            _owner.sprRender.enabled = !_owner.sprRender.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        GameObject.Destroy(_owner.gameObject);
    }
}
