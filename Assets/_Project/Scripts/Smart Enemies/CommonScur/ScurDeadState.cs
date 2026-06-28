using System.Collections;
using UnityEngine;

public class ScurDeadState : IScurState
{
    private readonly ScurAI _owner;

    private const float deathAnimationDuration = 1.5f;

    public ScurDeadState(ScurAI owner) => _owner = owner;

    public void Enter()
    {
        Collider2D coll = _owner.GetComponent<Collider2D>();
        if(coll != null ) coll.enabled = false;

        _owner.spawnManag.DismissAllRats(delay: 5f);

        EconomyManager.Instance?.AddXP(_owner.xpReward);
        EconomyManager.Instance?.AddEcho(_owner.echoReward);

        if(_owner.isMatka && _owner.dropItemPrefab != null) Object.Instantiate(_owner.dropItemPrefab, _owner.transform.position, Quaternion.identity);

        _owner.StartCoroutine(DeathSequence());
    }

    public void Update() { }

    public void Exit() { }


    private IEnumerator DeathSequence()
    {
        float elapsed = 0f;
        Vector3 startScale = _owner.transform.localScale;

        while (elapsed < deathAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float tm = elapsed / deathAnimationDuration;

            _owner.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, tm);

            yield return null;
        }

        GameEventBus.Instance?.SendEvent($"Died_{_owner.enemyID}", this);
        Object.Destroy(_owner.gameObject);
    }
}
