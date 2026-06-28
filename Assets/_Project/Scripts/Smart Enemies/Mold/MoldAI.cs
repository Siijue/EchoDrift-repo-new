using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class MoldAI : MonoBehaviour, IDamageable
{
    [SerializeField] public string enemyID = "enemy_zone_01";

    [SerializeField] public float moveSpeed = 8f;
    [SerializeField] public float explosionRadius = 3f;
    [SerializeField] public float explosionDamage = 0.5f;
    [SerializeField] public float explosionKnockback = 5f;
    [SerializeField] public GameObject sporePrefab;

    [SerializeField] public float lightKillRadius = 4f;
    [SerializeField] public float lightKillTime = 0.6f;

    [SerializeField] public float detectionRadius = 5f;

    [SerializeField] public int xpReward = 5;
    [SerializeField] public int echoReward = 8;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer sprRend;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public Transform playerTransfrom;
    [HideInInspector] public Light2D_Proxy lightProxy;

    private IMoldState _currentState;
    private MoldIdleState _idleState;
    private MoldChaseState _chaseState;
    private MoldSporeExplodeState _explodeState;
    private MoldShrinkDeadState _shinkDeadState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprRend = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransfrom = player.transform;
            lightProxy = new Light2D_Proxy(player);
        }

        _idleState = new MoldIdleState(this);
        _chaseState = new MoldChaseState(this);
        _explodeState = new MoldSporeExplodeState(this);
        _shinkDeadState = new MoldShrinkDeadState(this);
    }

    private void Start() => TransitionTo(_idleState);

    private void Update() => _currentState?.Update();

    public void TransitionTo(IMoldState newState)
    {
        _currentState?.Exite();
        _currentState = newState;
        _currentState?.Enter();
    }

    public MoldIdleState GetIdleState() => _idleState;
    public MoldChaseState GetChaseState() => _chaseState;
    public MoldSporeExplodeState GetExplodeState() => _explodeState;
    public MoldShrinkDeadState GetShrinkDeadState() => _shinkDeadState;

    public bool IsInLight() => LightSourceRegistry.IsPositionLit(transform.position);

    public float DistanceToPlayer()
    {
        if (playerTransfrom == null) return float.MaxValue;
        return Vector2.Distance(transform.position, playerTransfrom.position);
    }

    public void TakeDamage(float damage)
    {
        if (_currentState is MoldShrinkDeadState) return;
        TransitionTo(_shinkDeadState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightKillRadius);
        Gizmos.color = Color.violetRed;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (_currentState is not MoldSporeExplodeState && _currentState is not MoldShrinkDeadState) TransitionTo(_explodeState);
        }
    }
}

public class Light2D_Proxy
{
    private Light2D _light;

    public Light2D_Proxy(GameObject playerObj) => _light = playerObj.GetComponentInChildren<Light2D>();

    public bool isEnabled => _light != null && _light.enabled;
    public float outerRadius => _light != null ? _light.pointLightOuterRadius : 0f;


}