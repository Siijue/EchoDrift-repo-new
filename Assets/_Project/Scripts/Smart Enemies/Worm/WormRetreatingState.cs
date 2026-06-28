using Unity.VisualScripting;
using UnityEngine;

public class WormRetreatingState : IWormState
{
    private readonly WormAI _own;

    public WormRetreatingState(WormAI own) => _own = own;

    public void Enter() { }

    public void Update() 
    { 
        if(_own.StartPoint == null)
        {
            _own.TransitionTo(_own.GetHidden());
            return;
        }

        bool reached = _own.MoveHeadTowards(_own.StartPoint.position, _own.MoveSpeed);


        if (reached) _own.TransitionTo(_own.GetHidden());
    }

    public void Exit() => _own.HideAll();
}
