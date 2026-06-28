using UnityEngine;

public class SlugIdleState : ISlugState
{
    private readonly SlugAI _own;

    public SlugIdleState(SlugAI own) => _own = own;

    public void Enter()
    {
        _own.rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        if (LightSourceRegistry.IsPositionLit(_own.transform.position))
        {
                _own.TransitionTo(_own.GetDry());
                return;
        }

        if (CanDetectPlayer()) _own.TransitionTo(_own.GetChase());
    }

    public void Exit() { }

    private bool CanDetectPlayer()
    {
        if (_own.playerTransfrom == null) return false;

        float dist = Vector2.Distance(_own.transform.position, _own.playerTransfrom.position);

        if(dist > _own.detectionRadius) return false;

        RaycastHit2D hit = Physics2D.Linecast(_own.transform.position, _own.playerTransfrom.position, _own.obstacleMask);

        return hit.collider == null;
    }
}
