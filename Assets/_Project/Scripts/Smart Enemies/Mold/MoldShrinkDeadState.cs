using UnityEngine;
using System.Collections;

public class MoldShrinkDeadState : IMoldState
{
    private readonly MoldAI _owner;
    private const float ShrinkDuration = 0.35f;

    public MoldShrinkDeadState(MoldAI owner) => _owner = owner;

    public void Enter()
    {
        _owner.rb.linearVelocity = Vector2.zero;
        _owner.rb.simulated = false;
        _owner.coll.enabled = false;

        EconomyManager.Instance?.AddEcho(_owner.echoReward);
        EconomyManager.Instance?.AddXP(_owner.xpReward);

        _owner.StartCoroutine(ShrinkAndDestroy());
    }

    public void Update() { }
    public void Exite() { }

    private IEnumerator ShrinkAndDestroy()
    {
        Vector3 origScale = _owner.transform.localScale;
        float elapsed = 0f;

        while(elapsed < ShrinkDuration)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / ShrinkDuration;

            _owner.transform.localScale = Vector3.Lerp(origScale, Vector3.zero, time);

            _owner.sprRend.color = Color.Lerp(Color.white, Color.black, time);

            yield return null;
        }

        GameEventBus.Instance?.SendEvent($"Died_{_owner.enemyID}", this);
        Object.Destroy(_owner.gameObject);
    }
}
