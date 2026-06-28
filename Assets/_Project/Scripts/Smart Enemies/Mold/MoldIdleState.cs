using UnityEngine;

public class MoldIdleState : IMoldState
{
    private readonly MoldAI _owner;

    private float _lightTimer;

    public MoldIdleState(MoldAI owner) => _owner = owner;

    public void Enter()
    {
        _lightTimer = 0f;
        _owner.rb.linearVelocity = Vector2.zero;
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

        if (_owner.DistanceToPlayer() <= _owner.detectionRadius) _owner.TransitionTo(_owner.GetChaseState());
    }

    public void Exite() { }
}
