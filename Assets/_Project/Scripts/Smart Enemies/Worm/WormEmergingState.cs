using UnityEngine;

public class WormEmergingState : IWormState
{
    private readonly WormAI _own;

    public WormEmergingState(WormAI own) => _own = own;

    public void Enter() => _own.ShowAll();

    public void Update()
    {
        if(_own.EndPoint == null)
        {
            _own.TransitionTo(_own.GetActive());
            return;
        }

        bool reached = _own.MoveHeadTowards(_own.EndPoint.position, _own.MoveSpeed);

        if (reached) _own.TransitionTo(_own.GetActive());
    }

    public void Exit() { }
}
