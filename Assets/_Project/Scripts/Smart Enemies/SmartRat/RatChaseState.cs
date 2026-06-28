using UnityEngine;

public class RatChaseState : IRatState
{
    private readonly RatAI _owner;

    private float _lostPlayerTimer;
    private const float lostPlayerTimeout = 2f;

    public RatChaseState(RatAI owner) => _owner = owner;

    public void Enter()
    {
        _lostPlayerTimer = 0;
    }

    public void Update()
    {
        if (CheckTransitions()) return;
        MoveToPlayer();
    }

    public void Exit()
    {
        _owner.rb.linearVelocity = Vector2.zero;
    }



    private bool CheckTransitions()
    {
        if (_owner.sensorSystem.lightHitRat)
        {
            _owner.TransitionTo(_owner.GetFleeState());
            return true;
        }

        if (_owner.sensorSystem.playerInAttackRange)
        {
            _owner.TransitionTo(_owner.GetAttackState());
            return true;
        }

        if (!_owner.sensorSystem.playerDetected)
        {
            _lostPlayerTimer += Time.deltaTime;

            if (_lostPlayerTimer >= lostPlayerTimeout)
            {
                _owner.TransitionTo(_owner.GetIdleState());
                return true;
            }
        }
        else _lostPlayerTimer = 0f;

        return false;
    }


    private void MoveToPlayer()
    {
        float vx = _owner.sensorSystem.directionToPlayer.x * _owner.chaseSpeed;
        _owner.rb.linearVelocity = new Vector2(vx, _owner.rb.linearVelocity.y);

        if (Mathf.Abs(vx) > 0.1f)
        {
            FlipHelper.Flip(_owner.transform, vx > 0);
        }
    }
}
