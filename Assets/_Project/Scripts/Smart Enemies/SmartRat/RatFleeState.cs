using UnityEngine;

public class RatFleeState : IRatState
{
    private readonly RatAI _owner;

    private float _lightExposureTimer;

    private Vector2 _fleeDir;

    private float _updateDirTimer;

    private float _safeTimer;
    private const float safeTimeRequired = 0.5f;

    private const float dirUpdateInterval = 0.3f;

    public RatFleeState(RatAI owner) => _owner = owner;


    public void Enter()
    {
        _lightExposureTimer = 0f;
        _updateDirTimer = 0f;
        _safeTimer = 0f;

        UpdateFleeDirection(_owner.transform);
    }

    public void Update()
    {
        if (_owner.sensorSystem.lightHitRat)
        {
            _safeTimer = 0f;

            _lightExposureTimer += Time.deltaTime;
            _updateDirTimer += Time.deltaTime;

            if(_updateDirTimer >= dirUpdateInterval)
            {
                UpdateFleeDirection(_owner.transform);
                _updateDirTimer = 0f;
            }

            if(_lightExposureTimer >= _owner.lightKillTime)
            {
                _owner.TransitionTo(_owner.GetDeadState());
                return;
            }

            Flee();
        }
        else
        {
            _safeTimer += Time.deltaTime;

            Flee();

            _lightExposureTimer = 0f;

            if(_safeTimer >= safeTimeRequired) _owner.TransitionTo(_owner.sensorSystem.playerDetected ? _owner.GetChaseState() : _owner.GetIdleState());
        }
    }

    public void Exit() => _owner.rb.linearVelocity = Vector2.zero;


    private void UpdateFleeDirection(Transform ownerTransfrom)
    {
        LightSource nearest = LightSourceRegistry.GetNearestLitSource(ownerTransfrom.position);

        Vector2 fleeDir;

        if (nearest != null) fleeDir = ((Vector2)ownerTransfrom.position - nearest.position).normalized;
        else fleeDir = Vector2.right;

        _fleeDir = fleeDir;
    }

    private void Flee()
    {
        float vx = _fleeDir.x * _owner.fleeSpeed;
        _owner.rb.linearVelocity = new Vector2(vx, _owner.rb.linearVelocity.y);

        if (Mathf.Abs(vx) > 0.1f) FlipHelper.Flip(_owner.transform, vx > 0);
    }

}
