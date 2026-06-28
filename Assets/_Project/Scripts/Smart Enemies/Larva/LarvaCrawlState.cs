using UnityEngine;

public class LarvaCrawlState : ILarvaState
{
    private readonly LarvaAI _own;
    private float noSurfaceTimer;
    private const float noSurfaceTimeout = 0.5f;

    public LarvaCrawlState(LarvaAI own) => _own = own;

    public void Enter() => noSurfaceTimer = 0f;

    public void Update()
    {
        if (!_own.Crawler.IsOnSurface)
        {
            noSurfaceTimer += Time.deltaTime;
            if (noSurfaceTimer >= noSurfaceTimeout) _own.TransitionTo(_own.GetStuckState());
        }
        else noSurfaceTimer = 0f;
    }

    public void Exit() { }
}
