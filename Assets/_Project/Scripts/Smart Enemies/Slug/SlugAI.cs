using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class SlugAI : MonoBehaviour
{
    [SerializeField] public string enemyID = "enemy_zone_01";

    [SerializeField] public float detectionRadius = 6f;
    [SerializeField] public float loseRadius = 9f;
    [SerializeField] public LayerMask obstacleMask;
    [SerializeField] public float moveSpeed = 2.4f;
    [SerializeField] public float suctionDuration = 2f;
    [SerializeField] public float suctionDamage = 1f;
    [SerializeField] public float lightKillTime = 10f;
    [SerializeField] public SpriteRenderer slugSpr;
    [SerializeField] public SpriteRenderer crustSpr;
    [SerializeField] public int xpReward = 12;
    [SerializeField] public int echoReward = 15;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public Transform playerTransfrom;

    private ISlugState _current;

    public ISlugState CurrentState => _current;

    private SlugIdleState _idle;
    private SlugChaseState _chase;
    private SlugSuctionState _suction;
    private SlugDryingState _dry;
    private SlugDeadState _dead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        GameObject player = GameObject.FindWithTag("Player");
        if(player != null) playerTransfrom = player.transform;

        _idle = new SlugIdleState(this);
        _chase = new SlugChaseState(this);
        _suction = new SlugSuctionState(this);
        _dry = new SlugDryingState(this);
        _dead = new SlugDeadState(this);
    }

    private void Start()
    {
        SetAlpha(crustSpr, 0f);
        TransitionTo(_chase);
    }

    private void Update() => _current?.Update();

    public void TransitionTo(ISlugState newState)
    {
        //Debug.Log($"Состояние пограничника меняется с {_current} на {newState}");
        _current?.Exit();
        _current = newState;
        _current.Enter();
    }

    public void KillInstantly()
    {
        if (_current is SlugDeadState) return;
        TransitionTo(_dead);
    }

    public static void SetAlpha(SpriteRenderer sprite, float alpha)
    {
        if(sprite == null) return;
        Color color = sprite.color;
        color.a = alpha;
        sprite.color = color;
    }

    public SlugIdleState GetIdle() => _idle;
    public SlugChaseState GetChase() => _chase;
    public SlugSuctionState GetSuction() => _suction;
    public SlugDryingState GetDry() => _dry;
    public SlugDeadState GetDead() => _dead;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (_current is SlugChaseState && collision.gameObject.CompareTag("Player")) TransitionTo(_suction);
    }
}
