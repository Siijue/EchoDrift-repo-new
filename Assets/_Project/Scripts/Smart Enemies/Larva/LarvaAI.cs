using UnityEngine;

[RequireComponent(typeof(SurfaceCrawler))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class LarvaAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float damage = 0.25f;
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float damageCooldown = 0.8f;

    private SurfaceCrawler _crawler;
    private SpriteRenderer _spriteRenderer;

    private ILarvaState _current;
    private LarvaCrawlState _crawlState;
    private LarvaStuckState _stuckState;

    private float _cooldownTimer;

    private void Awake()
    {
        _crawler = GetComponent<SurfaceCrawler>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _crawlState = new LarvaCrawlState(this);
        _stuckState = new LarvaStuckState(this);
    }

    private void Start()
    {
        InitialNormalOverride();
        TransitionTo(_crawlState);
    }

    private void Update()
    {
        _current?.Update();

        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate() => _crawler.Tick(moveSpeed);

    public void TransitionTo(ILarvaState newState)
    {
        _current?.Exit();
        _current = newState;
        _current.Enter();
    }

    public LarvaCrawlState GetCrawlState()=> _crawlState;
    public LarvaStuckState GetStuckState() => _stuckState;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) DamagePlayer(collision.gameObject);
        else
        {
            _crawler.Reverse();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) DamagePlayer(collision.gameObject);
    }

    private void DamagePlayer(GameObject player)
    {
        if (_cooldownTimer > 0f) return;
        _cooldownTimer = damageCooldown;

        player.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        Vector2 dir = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        player.GetComponent<PlayerController>()?.ApplyKnockback(dir, knockbackForce);
    }

    public SurfaceCrawler Crawler => _crawler;
    public float MoveSpeed => moveSpeed;

    private void InitialNormalOverride()
    {
        Vector2[] directions =
        {
            Vector2.down,
            Vector2.up,
            Vector2.left,
            Vector2.right,
        };

        float closestDist = float.MaxValue;
        Vector2 closestNormal = Vector2.up;
        bool isFound = false;

        float initRayLength = _crawler.rayLength * 3f;
        LayerMask layer = _crawler.surfaceLayer;

        foreach(Vector2 dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, initRayLength, layer);
            if(hit.collider != null && hit.distance < closestDist)
            {
                closestDist = hit.distance;
                closestNormal = hit.normal;
                isFound = true;
            }
        }

        if (isFound) _crawler.SetSurfaceNormal(closestNormal);
        else Debug.Log("поверхность не найдена");
    }
}
