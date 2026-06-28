using UnityEngine;
using System.Collections;

public class LeshyPatrolState : ILeshyState
{
    private readonly LeshyAI _own;
    private readonly LeshyAliveState _superState;

    private float _patrolOroginX;
    private int _direction = 1;

    public LeshyPatrolState(LeshyAI own, LeshyAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter()
    {
        _patrolOroginX = _own.transform.position.x;
        _own.rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        if (_own.playerTransfrom != null) 
        {
            float dist = Vector2.Distance(_own.transform.position, _own.playerTransfrom.position);

            if(dist <= _own.detectionRadius)
            {
                _superState.TransitionChild(_superState.GetChase());
                return;
            }
        }

        Patrol();
    }

    public void Exit() => _own.rb.linearVelocity = Vector2.zero;

    public void OnDamage(int newHP) { }


    private void Patrol()
    {
        float x = _own.transform.position.x;
        float leftEdge = _patrolOroginX - _own.patrolRange;
        float rightEdge = _patrolOroginX + _own.patrolRange;

        if (x >= rightEdge) _direction = -1;
        if(x <= leftEdge) _direction = 1;

        _own.rb.linearVelocity = new Vector2(_direction * _own.patrolSpeed, _own.rb.linearVelocity.y);

        FlipHelper.Flip(_own.transform, _direction > 0);
    }
}

public class LeshyChaseState : ILeshyState
{
    private readonly LeshyAI _own;
    private readonly LeshyAliveState _superState;

    private const float FlipDeadzone = 0.3f;

    public LeshyChaseState(LeshyAI own, LeshyAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter() { }

    public void Update()
    {
        if (_own.playerTransfrom == null) return;

        if (!_own.IsPlayerAlive())
        {
            _superState.ReturnToPatrol();
            return;
        }

        float dist = Vector2.Distance(_own.transform.position, _own.playerTransfrom.position);

        if(dist > _own.loseRadius)
        {
            _superState.TransitionChild(_superState.GetPatrol());
            return;
        }

        if(dist <= _own.attackTriggerRange)
        {
            _superState.TransitionChild(_superState.GetAttack());
            return;
        }

        MoveTowardsPlayer();
    }

    public void Exit() => _own.rb.linearVelocity = Vector2.zero;

    public void OnDamage(int newHP) {}

    private void MoveTowardsPlayer()
    {
        float dx = _own.playerTransfrom.position.x - _own.transform.position.x;
        float vx = Mathf.Sign(dx) * _own.chaseSpeed;
        _own.rb.linearVelocity = new Vector2(vx, _own.rb.linearVelocity.y);
        if (Mathf.Abs(dx) > FlipDeadzone) FlipHelper.Flip(_own.transform, dx < 0);
    }
}

public class LeshyAttackState : ILeshyState
{
    private readonly LeshyAI _own;
    private readonly LeshyAliveState _superState;

    private enum Phase { WindUp, Strike, Cooldown}
    private Phase _phase;
    private float _phaseTimer;

    private const float WindUpDuration = 0.3f;
    private const float StrikeDuration = 0.15f;

    public LeshyAttackState(LeshyAI own, LeshyAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter()
    {
        _phase = Phase.WindUp;
        _phaseTimer = 0;
        _own.rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        _phaseTimer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.WindUp:
                if(_phaseTimer > WindUpDuration)
                {
                    _phase = Phase.Strike;
                    _phaseTimer = 0f;
                    PerformStrike();
                }
                break;

            case Phase.Strike:
                if(_phaseTimer >= StrikeDuration)
                {
                    _phase = Phase.Cooldown;
                    _phaseTimer = 0f;
                }
                break;
            case Phase.Cooldown: 
                if(_phaseTimer >= _own.attackCooldown)
                {
                    _superState.TransitionChild(_superState.IsEnraged ? (ILeshyState)_superState.GetEnraged() : _superState.GetChase());
                }
                break;
        }
    }

    public void Exit() { }

    public void OnDamage(int newHP) { }

    private void PerformStrike()
    {

        if (_own.playerTransfrom == null)
        {
            Debug.Log("вызов метода"); return;
        }

        float dx = _own.playerTransfrom.position.x - _own.transform.position.x;
        Vector2 baseDir = new Vector2(Mathf.Sign(dx), 0f);
        Vector2 orig = (Vector2)_own.transform.position + _own.attackOriginOffset * Vector2.down;

        float[] angles = { 0f, _own.attackAngleUp, _own.attackAngleDown };
        bool hitRegister = false;

        foreach(float deg in angles)
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, deg) * baseDir;

            RaycastHit2D hit = Physics2D.Raycast(orig, dir, _own.attackRayLength, _own.attackLayerMack);

            if(hit.collider != null && hit.collider.CompareTag("Player") && !hitRegister)
            {
                hit.collider.GetComponent<PlayerHealth>()?.TakeDamage(_own.attackDamage);
                hit.collider.GetComponent<PlayerController>()?.ApplyKnockback(dir, _own.attackKnockback);
                hitRegister = true;
            }
        }
    }
}

public class LeshyHurtState : ILeshyState
{
    private readonly LeshyAI _own;
    private readonly LeshyAliveState _superState;

    private float _timer;

    public LeshyHurtState(LeshyAI own, LeshyAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter()
    {
        _timer = 0f;
        _own.rb.linearVelocity = Vector2.zero;

        _own.sprRend.color = Color.yellow * 2f;
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        float time = _timer / _own.hurtDuration;
        _own.sprRend.color = Color.Lerp(Color.white * 1.8f, Color.white, time);

        if (_timer >= _own.hurtDuration) _superState.ReturnAfterHurt();
    }

    public void Exit() => _own.sprRend.color = Color.white;

    public void OnDamage(int newHP) => _timer = 0f;
}

public class LeshyEnragedState : ILeshyState
{
    private readonly LeshyAI _own;
    private readonly LeshyAliveState _superState;

    private const float FlipDeadzone = 0.3f;

    public LeshyEnragedState(LeshyAI own, LeshyAliveState superState)
    {
        _own = own;
        _superState= superState;
    }

    public void Enter()
    {
        _own.UpdateEyes(_own.CurrentHP, isEnraged: true);
    }

    public void Update()
    {
        if(_own.playerTransfrom == null) return;

        if (!_own.IsPlayerAlive())
        {
            _superState.ReturnToPatrol();
            return;
        }

        float dist = Vector2.Distance(_own.transform.position, _own.playerTransfrom.position);

        if(dist <= _own.attackTriggerRange)
        {
            _superState.TransitionChild(_superState.GetAttack());
            return;
        }

        float speed = _own.chaseSpeed + _own.enragedBonus;
        float dx = _own.playerTransfrom.position.x - _own.transform.position.x;
        _own.rb.linearVelocity = new Vector2(Mathf.Sign(dx) * speed, _own.rb.linearVelocity.y);

        if (Mathf.Abs(dx) > FlipDeadzone) FlipHelper.Flip(_own.transform, dx < 0);
    }

    public void Exit() { }
    public void OnDamage(int newHP) { }

}