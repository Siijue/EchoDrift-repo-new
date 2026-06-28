using UnityEngine;

public class LarvaStuckState : ILarvaState
{
    private readonly LarvaAI _own;
    private float searchTimer;
    private const float maxSearchTime = 3f;
    private const float rotationSpeed = 90f;

    public LarvaStuckState(LarvaAI own) => _own = own;

    public void Enter()
    {
        searchTimer = 0f;
        _own.Crawler.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        Debug.Log("Stucked");
    }

    public void Update()
    {
        searchTimer += Time.deltaTime;

        _own.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (_own.Crawler.IsOnSurface)
        {
            _own.TransitionTo(_own.GetCrawlState());
            return;
        }

        if(searchTimer >= maxSearchTime)
        {
            searchTimer = 0f;
            _own.Crawler.Reverse();
            _own.TransitionTo(_own.GetCrawlState());
        }
    }

    public void Exit() => _own.transform.rotation = Quaternion.identity;
}
