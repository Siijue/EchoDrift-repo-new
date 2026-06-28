using UnityEngine;

public class SlugDeadState : ISlugState
{
    private readonly SlugAI _own;
    private bool _rewarded;

    public SlugDeadState(SlugAI own) => _own = own;

    public void Enter()
    {
        Debug.Log("DEAD");
        if (_rewarded) return;
        _rewarded = true;
        _own.rb.simulated = false;
        _own.coll.enabled = false;

        bool cameFromDrying = _own.crustSpr != null && _own.crustSpr.color.a >= 0.9f;

        if (!cameFromDrying)
        {
            SlugAI.SetAlpha(_own.slugSpr, 0f);
            SlugAI.SetAlpha(_own.crustSpr, 1f);
            _own.transform.localScale *= 0.3f;
        }

        if (_own.slugSpr != null) _own.slugSpr.enabled = false;

        GameEventBus.Instance?.SendEvent($"Died_{_own.enemyID}", this);

        EconomyManager.Instance?.AddEcho(_own.echoReward);
        EconomyManager.Instance?.AddXP(_own.xpReward);
    }

    public void Update() { }
    public void Exit() { }
}