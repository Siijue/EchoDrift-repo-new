using UnityEngine;
using System.Collections;

public class LeshyDeadState : ILeshyState
{
    private readonly LeshyAI _own;

    public LeshyDeadState(LeshyAI own) => _own = own;

    public void Enter()
    {
        _own.rb.linearVelocity = Vector2.zero;
        _own.rb.simulated = false;
        _own.coll.enabled = false;

        UIManager.Instance?.HideBossInfo();
        GameEventBus.Instance?.SendEvent("LeshyDead", _own);

        foreach (var eye in _own.eyeSprites) if (eye != null) eye.color = _own.eyeDimmedColor;

        GameEventBus.Instance?.SendEvent($"Died_{_own.enemyID}", this);

        EconomyManager.Instance?.AddEcho(_own.echoReward);
        EconomyManager.Instance?.AddXP(_own.xpReward);

        _own.StartCoroutine(DeathSequence());
    }

    public void Update() { }
    public void Exit() { }
    public void OnDamage(int newHP) { }

    private IEnumerator DeathSequence()
    {
        float elapsed = 0f;
        Vector3 basePos = _own.transform.position;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(elapsed * 40f) * 0.1f;
            _own.transform.position = basePos + new Vector3(shake, 0, 0);
            yield return null;
        }

        _own.transform.position = basePos;

        elapsed = 0f;
        while(elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / 0.6f;
            _own.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, time);
            _own.sprRend.color = Color.Lerp(Color.white, new Color(0.3f, 0.15f, 0.05f), time);
            yield return null;
        }

        Object.Destroy(_own.gameObject);
    }
}
