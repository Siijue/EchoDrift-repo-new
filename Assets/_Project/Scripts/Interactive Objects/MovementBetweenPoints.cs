using UnityEngine;

public class MovementBetweenPoints : MonoBehaviour
{
    public enum PathMode
    {
        Loop,
        PingPong,
        Once
    }

    [SerializeField] private Transform[] points;
    [SerializeField] private PathMode mode = PathMode.PingPong;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float waitTime = 0f;
    [SerializeField] private bool startMoving = true;

    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isFinished = false;

    private Rigidbody2D rb;
    private Vector3 targetPosition;

    public int CurrentIndex => currentIndex;
    public bool IsFinished => isFinished;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        if (points == null || points.Length < 2)
        {
            enabled = false;
            return;
        }

        if (startMoving)
        {
            targetPosition = points[0].position;
            rb.MovePosition(targetPosition);
        }
    }

    private void Update()
    {
        if (isFinished) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
            }
            return;
        }
        targetPosition = points[currentIndex].position;
    }

    private void FixedUpdate()
    {
        if (isFinished || isWaiting) return;

        rb.MovePosition(Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime));

        if (Vector3.Distance(rb.position, targetPosition) < 0.01f)
        {
            OnReachedPoint();
        }
    }

    private void OnReachedPoint()
    {
        if (waitTime > 0f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }

        switch (mode)
        {
            case PathMode.Loop:
                currentIndex = (currentIndex + 1) % points.Length;
                break;

            case PathMode.PingPong:
                currentIndex += direction;
                if (currentIndex >= points.Length - 1)
                {
                    currentIndex = points.Length - 1;
                    direction = -1;
                }
                else if (currentIndex <= 0)
                {
                    currentIndex = 0;
                    direction = 1;
                }
                break;

            case PathMode.Once:
                currentIndex++;
                if (currentIndex >= points.Length)
                {
                    currentIndex = points.Length - 1;
                    isFinished = true;
                }
                break;
        }
    }

    public void ResetPath()
    {
        currentIndex = 0;
        direction = 1;
        isFinished = false;
        isWaiting = false;
        if (points.Length > 0)
        {
            targetPosition = points[0].position;
            rb.MovePosition(targetPosition);
        }
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;
}
