using UnityEngine;
using System.Collections;

public class IstAliveState : IIstState
{
    private readonly IstAI _own;

    private IstIdleState _idle;
    private IstCastingState _casting;
    private IstStunnedState _stunned;
    private IstEvadingState _evading;
    private IstPhaseTransitionState _phase;

    private IIstState _childState;

    private float _hoverTime;

    public IstAliveState(IstAI own)
    {
        _own = own;
        _idle = new IstIdleState(own, this);
        _casting = new IstCastingState(own, this);
        _stunned = new IstStunnedState(own, this);
        _evading = new IstEvadingState(own, this);
        _phase = new IstPhaseTransitionState(own, this);
    }

    public void Enter() => TransitionChild(_idle);
    public void Update() => _childState?.Update();


    public void FixedUpdate()
    {
        bool shouldHover = _childState is IstIdleState || _childState is IstCastingState;
        if (shouldHover) UpdateHover();
        else _own.rb.linearVelocity = Vector2.zero;
        _childState?.FixedUpdate();
    }

    public void Exit()
    {
        _childState?.Exit();
        _childState = null;
    }

    public void OnCrystalHit(int damage)
    {
        if (_childState is IstStunnedState) return;
        _own.IsInvulnerable = false;
        _own.TakeDamage(damage);
    }

    public void OnDamaged(int newHP)
    {
        if (newHP <= 0)
        {
            _own.TransitionTo(_own.GetDead());
            return;
        }

        if(newHP <= _own.phase3Threshold && _own.Phase < 3)
        {
            TransitionChild(_phase);
            return;
        }

        if(newHP <= _own.phase2Threshold && _own.Phase < 2)
        {
            TransitionChild(_phase);
            return;
        }

        TransitionChild(_stunned);
    }

    public void TransitionChild(IIstState state)
    {
        _childState?.Exit();
        _childState = state;
        _childState.Enter();
    }

    public void ReturnToIdle() => TransitionChild(_idle);
    public void StartEvading() => TransitionChild(_evading);

    public void StartCasting(IstAbility ability)
    {
        _casting.SetAbility(ability);
        TransitionChild(_casting);
    }

    public IstIdleState GetIdle() => _idle;
    public IstEvadingState GetEvading() => _evading;
    public IstPhaseTransitionState GetPhase() => _phase;


    private void UpdateHover()
    {
        if (_own.playerTransform == null) return;

        _hoverTime += Time.fixedDeltaTime;

        float phaseSpeedMult = _own.Phase == 1 ? 1f : _own.Phase == 2 ? 1.5f : 1.875f;

        Vector2 target = new Vector2(_own.playerTransform.position.x, _own.playerTransform.position.y + _own.hoverHeight + Mathf.Sin(_hoverTime * _own.hoverFrequency) * _own.hoverAmplitude);
        _own.rb.MovePosition(Vector2.MoveTowards(_own.transform.position, target, _own.walkSpeed * phaseSpeedMult * Time.fixedDeltaTime));
        float dx = _own.playerTransform.position.x - _own.transform.position.x;
        if (Mathf.Abs(dx) > 0.1f) FlipHelper.Flip(_own.transform, dx < 0);
    }
}