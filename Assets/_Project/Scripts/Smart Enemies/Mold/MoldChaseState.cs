using UnityEngine;

public class MoldChaseState : IMoldState
{
    private readonly MoldAI _owner;
    private float _lightTimer;

    public MoldChaseState(MoldAI owner) { _owner = owner; }

    public void Enter()
    {
        _lightTimer = 0;
        
    }

    public void Update()
    {
        if (_owner.IsInLight())
        {
            _lightTimer += Time.deltaTime;
            if (_lightTimer >= _owner.lightKillTime)
            {
                _owner.TransitionTo(_owner.GetShrinkDeadState());
                return;
            }
        }
        else _lightTimer = 0f;

        MoveTowardsToPlayer();
    }

    public void Exite() => _owner.rb.linearVelocity = Vector2.zero;

    private void MoveTowardsToPlayer()
    {
        if (_owner.playerTransfrom == null) return;

        Vector2 dir = ((Vector2)_owner.playerTransfrom.position - (Vector2)_owner.transform.position).normalized;

        _owner.rb.linearVelocity = new Vector2(dir.x * _owner.moveSpeed, _owner.rb.linearVelocity.y);

        _owner.sprRend.flipX = dir.x < 0;
    }
}
