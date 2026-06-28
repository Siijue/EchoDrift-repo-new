using UnityEngine;

public class RatAttackState : IRatState
{
    private readonly RatAI _owner;

    private enum AttackPhase { Biting, KnockingBack, Cooldown }
    private AttackPhase _phase;

    private float _phaseTimer;

    private const float biteDuration = 0.15f;
    private const float knockbackDuration = 0.3f;
    private const float knockbackSpeed = 5f;

    private float _dirKnockback;

    private bool _damageDealt;


    public RatAttackState(RatAI owner) => _owner = owner;

    public void Enter()
    {
        Debug.Log("Attack phase by rat");
        _phase = AttackPhase.Biting;
        _phaseTimer = 0f;
        _damageDealt = false;

        _dirKnockback = -Mathf.Sign(_owner.sensorSystem.directionToPlayer.x);
        if (_dirKnockback == 0) _dirKnockback = 1f;

        _owner.rb.linearVelocity = Vector2.zero;

        float dirToPlayer = _owner.sensorSystem.directionToPlayer.x;
        if (Mathf.Abs(dirToPlayer) > 0.1f)
        {
            FlipHelper.Flip(_owner.transform, dirToPlayer > 0);
        }
    }

    public void Update()
    {
        _phaseTimer += Time.deltaTime;

        switch (_phase)
        {
            case AttackPhase.Biting: HandleBiting();
                break;
            case AttackPhase.KnockingBack: HandleKnockback(); 
                break;
            case AttackPhase.Cooldown: HandleCooldown(); 
                break;
        }
    }

    public void Exit() => _owner.rb.linearVelocity = Vector2.zero;


    private void HandleBiting()
    {
        if (!_damageDealt)
        {
            DealDamage();
            _damageDealt = true;
        }

        if(_phaseTimer >= biteDuration)
        {
            _phase = AttackPhase.KnockingBack;
            _phaseTimer = 0f;
            _owner.rb.linearVelocity = new Vector2(_dirKnockback * knockbackSpeed, 2f);
        }
    }

    private void HandleKnockback()
    {
        if(_phaseTimer >= knockbackDuration)
        {
            _owner.rb.linearVelocity = Vector2.zero;
            _phase = AttackPhase.Cooldown;
            _phaseTimer = 0f;
        }
    }

    private void HandleCooldown()
    {
        if (_owner.sensorSystem.lightHitRat)
        {
            _owner.TransitionTo(_owner.GetFleeState());
            return;
        }

        if(_phaseTimer >= _owner.attackCooldown)
        {
            if (_owner.sensorSystem.playerInAttackRange) _owner.TransitionTo(_owner.GetAttackState());

            else _owner.TransitionTo(_owner.sensorSystem.playerDetected ? _owner.GetChaseState() : _owner.GetIdleState());
        }
    }

    private void DealDamage()
    {
        Transform playerTransform = _owner.sensorSystem.GetPlayerTransform();
        if (playerTransform == null) return;

        if (_owner.sensorSystem.distanceToPlayer > _owner.attackRaduis * 1.2f) return;

        PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
        health?.TakeDamage(_owner.attackDamage);

        PlayerController controller = playerTransform.GetComponent<PlayerController>();
        if(controller != null)
        {
            Vector2 knockDir = _owner.sensorSystem.directionToPlayer;
            controller.ApplyKnockback(knockDir, _owner.knockbackForce);
        }
    }

    public bool IsBitting => _phase == AttackPhase.Biting;
}
