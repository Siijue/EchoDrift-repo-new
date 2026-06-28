using Unity.VisualScripting;
using UnityEngine;

public class LeshyAliveState : ILeshyState
{
    private readonly LeshyAI _owner;

    private LeshyPatrolState _patrol;
    private LeshyChaseState _chase;
    private LeshyAttackState _attack;
    private LeshyHurtState _hurt;
    private LeshyEnragedState _enraged;


    private ILeshyState _activeChild;
    private bool isEnraged;

    public LeshyAliveState(LeshyAI owner)
    {
        _owner = owner;
        _patrol = new LeshyPatrolState(owner, this);
        _chase = new LeshyChaseState(owner, this);
        _attack = new LeshyAttackState(owner, this);
        _hurt = new LeshyHurtState(owner, this);
        _enraged = new LeshyEnragedState(owner, this);
    }

    public void Enter()
    {
        isEnraged = false;
        TransitionChild(_patrol);
    }

    public void Update()
    {
        _activeChild?.Update();
        _owner.UpdateBossUIVisibility();
    }

    public void Exit()
    {
        _activeChild?.Exit();
        _activeChild = null;
    }

    public void OnDamage(int newHP)
    {
        _owner.UpdateEyes(newHP, isEnraged);
        
        if(newHP <= 0)
        {
            _owner.TransitionTo(_owner.GetDeadState());
            return;
        }

        _owner.UpdateBossHP();

        TransitionChild(_hurt);

        if(newHP <= _owner.maxHP / 2 && !isEnraged) isEnraged = true;
    }

    public void TransitionChild(ILeshyState newChild)
    {
        _activeChild?.Exit();
        _activeChild = newChild;
        _activeChild.Enter();
    }

    public void ReturnToPatrol() => TransitionChild(_patrol);

    public void ReturnAfterHurt()
    {
        if (!_owner.IsPlayerInRange()) return;
        if (isEnraged) TransitionChild(_enraged);
        else TransitionChild(_chase);
    }

    public LeshyPatrolState GetPatrol() => _patrol;
    public LeshyChaseState GetChase() => _chase;
    public LeshyAttackState GetAttack() => _attack;
    public LeshyEnragedState GetEnraged() => _enraged;
    public bool IsEnraged => isEnraged;
}
