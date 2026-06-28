using UnityEngine;

public class WormActiveState : IWormState
{
    private readonly WormAI _own;
    private float timer;
    private float headY;

    public WormActiveState(WormAI own) => _own = own;

    public void Enter()
    {
        timer = 0f;
        headY = _own.Head != null ? _own.Head.transform.position.y : 0f;
    }

    public void Update()
    {
        timer += Time.deltaTime;

        if(_own.Head != null)
        {
            float sway = Mathf.Sin(Time.deltaTime * 3f) * 0.06f;
            Vector3 pos = _own.Head.transform.position;
            _own.Head.transform.position = new Vector3(pos.x, headY + sway, pos.z);

            for (int i = 1; i < _own.Segments.Length; i++) _own.Segments[i]?.FollowToHead();
        }

        if (timer >= _own.ActiveDuration) _own.TransitionTo(_own.GetRetreating());
    }

    public void Exit() { }
}
