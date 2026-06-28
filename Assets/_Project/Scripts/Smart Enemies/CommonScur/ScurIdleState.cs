using UnityEngine;

public class ScurIdleState : IScurState
{
    private readonly ScurAI _owner;

    private float _lightExposureTimer;

    public ScurIdleState(ScurAI owner) => _owner = owner;

    public void Enter() => _lightExposureTimer = 0f;

    public void Update()
    {
        if (_owner.sensorSystem.lightHitsScur)
        {
            _lightExposureTimer += Time.deltaTime;
            if(_lightExposureTimer >= _owner.lightKillTime)
            {
                _owner.TransitionTo(_owner.GetDeadState());
                return;
            }
        }
        else _lightExposureTimer = 0f;

        if (_owner.sensorSystem.playerInrange) _owner.TransitionTo(_owner.GetSummoningState());
    }

    public void Exit() => _lightExposureTimer = 0f;
}
