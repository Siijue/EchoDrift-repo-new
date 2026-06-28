using UnityEngine;
using System;

public class RatIdleState : IRatState
{
    private readonly RatAI _owner;

    private Vector2[] _patrolPoints;
    private int _currentPatrolIndex;

    private const int pointsCount = 4;
    private const float reachThreshold = 0.25f;

    private float _waitTimer;
    private const float waitToPointDuration = 0.8f;
    private bool _isWaiting;

    private float _playerDetectedTimer;
    private const float playerReactDelay = 0.3f;
    private float _lightDetectedTimer;
    private const float lightReactDelay = 0.15f;

    public RatIdleState(RatAI owner) => _owner = owner;


    public void Enter()
    {
        _owner.rb.linearVelocity = Vector2.zero;
        _isWaiting = false;
        _waitTimer = 0f;
        _playerDetectedTimer = 0f;
        _lightDetectedTimer = 0f;

        GenerateNewpoints();
        
        Debug.Log("Начало патрулирования");
    }

    public void Update()
    {
        if (CheckTransitions()) return;

        if (_isWaiting) HandleWait();
        else Patrol();
    }

    public void Exit() 
    { 
        _owner.rb.linearVelocity = Vector2.zero;

        _playerDetectedTimer = 0f;
        _lightDetectedTimer = 0f;
    }

    private bool CheckTransitions()
    {
        if (_owner.sensorSystem.lightHitRat)
        {
            _lightDetectedTimer += Time.deltaTime;

            _owner.rb.linearVelocity = Vector2.zero;

            if (_lightDetectedTimer >= lightReactDelay)
            {
                _owner.TransitionTo(_owner.GetFleeState());
                return true;
            }
        }
        else _lightDetectedTimer = 0f;

        if (_owner.sensorSystem.playerDetected)
        {
            _playerDetectedTimer += Time.deltaTime;

            _owner.rb.linearVelocity = Vector2.zero;

            if (_playerDetectedTimer >= playerReactDelay)
            {
                _owner.TransitionTo(_owner.GetChaseState());
                return true;
            }
        }
        else _playerDetectedTimer = 0f;

        return false;
    }

    private void GenerateNewpoints()
    {
        _patrolPoints = _owner.patrolZone.GeneratePoints(pointsCount);
        _currentPatrolIndex = 0;

        Array.Sort(_patrolPoints, (a, b) => a.x.CompareTo(b.x));
    }

    private void Patrol()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            Debug.Log($"ТОЧЕК НЕТ");
            return;
        }

        Vector2 target = _patrolPoints[_currentPatrolIndex];
        float dx = target.x - _owner.transform.position.x;

        if(Mathf.Abs(dx) < reachThreshold)
        {
            _owner.rb.linearVelocity = Vector2.zero;
            _isWaiting = true;
            _waitTimer = 0f;
        }
        else
        {
            float vx = MathF.Sign(dx) * _owner.idleSpeed;
            _owner.rb.linearVelocity = new Vector2(vx, _owner.rb.linearVelocity.y);
            FlipHelper.Flip(_owner.transform, dx > 0);
        }
    }

    private void HandleWait()
    {
        _waitTimer += Time.deltaTime;

        if(_waitTimer >= waitToPointDuration)
        {
            _isWaiting = false;
            _currentPatrolIndex++;

            if (_currentPatrolIndex >= _patrolPoints.Length) GenerateNewpoints();
        }
    }
}
