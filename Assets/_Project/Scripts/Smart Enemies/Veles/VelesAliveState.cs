using UnityEngine;
using System.Collections;

public class VelesAliveState : IVelesState
{
    private readonly VelesAI _own;


    private VelesIdleState _idle;
    private VelesCastingState _casting;
    private VelesStunnedState _stunned;
    private VelesGazeState _gaze;
    private VelesPhaseTransitionState _phaseTransition;

    private IVelesState _activeChild;

    private float _hoverTime;

    public VelesAliveState(VelesAI own)
    {
        _own = own;
        _idle = new VelesIdleState(own, this);
        _casting = new VelesCastingState(own, this);
        _stunned = new VelesStunnedState(own, this);
        _gaze = new VelesGazeState(own, this);
        _phaseTransition = new VelesPhaseTransitionState(own, this);
    }

    public void Enter()
    {
        _hoverTime = 0f;
        TransitionChild(_idle);
    }

    public void Update() => _activeChild?.Update();

    public void FixedUpdate()
    {
        bool shouldHover = _activeChild is VelesIdleState || _activeChild is VelesCastingState;
        if(shouldHover) UpdateHover();
        _activeChild?.FixedUpdate();
    }

    public void Exit()
    {
        _activeChild?.Exit();
        _activeChild = null;
    }

    public void OnCrystalHit()
    {
        if (_activeChild is VelesStunnedState) return;
        if (_activeChild is VelesGazeState) return;
        if (_activeChild is VelesCastingState casting) casting.Interrupt();

        
        _own.TakeDamage(_own.crystalDamage);
    }

    public void OnDamaged(int newHP)
    {
        if(newHP <= 0)
        {
            Debug.Log($"NewHP: {newHP}");
            _own.TransitionTo(_own.GetDeadState());
            return;
        }
        if(newHP <= _own.maxHP / 2 && !_own.IsPhase2)
        {
            TransitionChild(_phaseTransition);
            return;
        }
        TransitionChild(_stunned);
    }

    public void TransitionChild(IVelesState newChild)
    {
        _activeChild?.Exit();
        _activeChild = newChild;
        _activeChild.Enter();
    }

    public void ReturnToIdle() => TransitionChild(_idle);

    public void StartCasting(VelesAbility ability)
    {
        _casting.SetAbility(ability);
        TransitionChild(_casting);
    }

    public void StartGaze() => TransitionChild(_gaze);



    private void UpdateHover()
    {
        if (!_own.IsPlayerAlive()) { _own.HideBossUI(); return; }
        if (!_own.IsPlayerInRange()) { _own.HideBossUI(); return; }

        _own.UpdateBossUIVisibility();
        _hoverTime += Time.deltaTime;

        float speedBonus = _own.IsPhase2 ? _own.phase2SpeedBonus : 0f;

        Vector2 targetPos = new Vector2(_own.playerTransform.position.x, _own.playerTransform.position.y + _own.hoverHeight + Mathf.Sin(_hoverTime * 2f) * _own.hoverAmplitude);

        Vector2 currentPos = _own.transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, (_own.hoverSpeed + speedBonus) * Time.fixedDeltaTime);

        _own.rb.MovePosition(newPos);

        float dx = _own.playerTransform.position.x - _own.transform.position.x;
        if (Mathf.Abs(dx) > 0.1f) FlipHelper.Flip(_own.transform, dx < 0);
    }
}
