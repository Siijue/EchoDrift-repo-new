using UnityEngine;

public class SlugChaseState : ISlugState
{
    private readonly SlugAI _own;

    private float _loseTimer;
    private const float LoseTimeout = 1.5f;

    public SlugChaseState(SlugAI own) => _own = own;

    public void Enter() => _loseTimer = 0f;

    public void Update()
    {
        if (LightSourceRegistry.IsPositionLit(_own.transform.position))
        {
                _own.TransitionTo(_own.GetDry());
                return;
        }

        if (ShouldLosePlayer())
        {
            _loseTimer += Time.deltaTime;
            if (_loseTimer >= LoseTimeout)
            {
                _own.TransitionTo(_own.GetIdle());
                return;
            }
        }
        else _loseTimer = 0f;

        MoveToPlayer();
    }

    public void Exit() => _own.rb.linearVelocity = Vector2.zero;

    public bool ShouldLosePlayer()
    {
        if (_own.playerTransfrom == null) return true;

        float dist = Vector2.Distance(_own.transform.position, _own.playerTransfrom.position);

        if (dist > _own.loseRadius) return true;

        RaycastHit2D hit = Physics2D.Linecast(_own.transform.position, _own.playerTransfrom.position, _own.obstacleMask);

        return hit.collider != null;
    }

    private void MoveToPlayer()
    {
        if (_own.playerTransfrom == null) return;

        float dx = _own.playerTransfrom.position.x - _own.transform.position.x;
        _own.rb.linearVelocity = new Vector2(Mathf.Sign(dx) * _own.moveSpeed, _own.rb.linearVelocity.y);
        FlipHelper.Flip(_own.transform, dx < 0);
    }
}
