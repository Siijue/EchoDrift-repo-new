using UnityEngine;

public class WormHiddenState : IWormState
{
    private readonly WormAI _own;
    private float timer;

    public WormHiddenState(WormAI own) => _own = own;

    public void Enter()
    {
        timer = 0f;
        _own.PlaceAllAtStart();
        _own.HideAll();
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= _own.HiddentDuration) _own.TransitionTo(_own.GetEmerging());
    }

    public void Exit() {  } 
}
