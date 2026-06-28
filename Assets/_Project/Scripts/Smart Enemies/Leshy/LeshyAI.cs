using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class LeshyAI : MonoBehaviour
{
    public static LeshyAI Instance { get; private set; }

    [SerializeField] public string enemyID = "enemy_zone_01";

    [SerializeField] private string bossName = "Леший";
    [SerializeField] private string bossSubname = "Изгнанный хранитель скверны";

    [SerializeField] public int maxHP = 4;

    [SerializeField] public float chaseSpeed = 2.0f;
    [SerializeField] public float patrolSpeed = 1.2f;
    [SerializeField] public float enragedBonus = 1.0f;
    [SerializeField] public float patrolRange = 4f;

    [SerializeField] public float detectionRadius = 7f;
    [SerializeField] public float loseRadius = 10f;

    [SerializeField] public float attackTriggerRange = 2f;
    [SerializeField] public float attackRayLength = 3f;
    [SerializeField] public float attackOriginOffset = 0.5f;
    [SerializeField] public float attackAngleUp = 25f;
    [SerializeField] public float attackAngleDown = 20f;
    [SerializeField] public float attackDamage = 1f;
    [SerializeField] public float attackKnockback = 5f;
    [SerializeField] public float attackCooldown = 2.5f;
    [SerializeField] public LayerMask attackLayerMack;
    
    [SerializeField] public SpriteRenderer[] eyeSprites = new SpriteRenderer[4];
    [SerializeField] public Color eyeActiveColor = Color.white;
    [SerializeField] public Color eyeDimmedColor = Color.black;

    [SerializeField] public float hurtDuration = 0.4f;

    [SerializeField] public int xpReward = 15;
    [SerializeField] public int echoReward = 25;


    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer sprRend;
    [HideInInspector] public Collider2D coll;
    [HideInInspector] public Transform playerTransfrom;

    public int CurrentHP { get; private set;  }

    private LeshyAliveState _aliveState;

    private LeshyDeadState _deadState;

    private ILeshyState _currentState;
    private bool _isBossUIVisible = false;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        rb = GetComponent<Rigidbody2D>();
        sprRend = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj != null) playerTransfrom = playerObj.transform;

        CurrentHP = maxHP;

        _aliveState = new LeshyAliveState(this);
        _deadState = new LeshyDeadState(this);
    }

    private void Start()
    {
        InitEyes();
        TransitionTo(_aliveState);
    }

    private void Update() => _currentState?.Update();

    public void TakeDamage(int dmg = 1)
    {
        if (_currentState is LeshyDeadState) return;

        CurrentHP = Mathf.Max(0, CurrentHP - dmg);

        _currentState?.OnDamage(CurrentHP);
    }

    public void TransitionTo(ILeshyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void UpdateEyes(int currentHP, bool isEnraged = false)
    {
        Color activeColor = isEnraged ? new Color(1f, 0.2f, 0.1f) : eyeActiveColor;

        for(int i = 0; i < eyeSprites.Length; i++)
        {
            if (eyeSprites[i] == null) continue;
            bool isActive = i < currentHP;
            eyeSprites[i].color = isActive ? activeColor : eyeDimmedColor;
        }
    }

    public void UpdateBossUIVisibility()
    {
        bool playerInRange = IsPlayerAlive() && IsPlayerInRange();

        if(playerInRange && !_isBossUIVisible)
        {
            UIManager.Instance?.ShowBossInfo(bossName, bossSubname, maxHP);
            _isBossUIVisible = true;
        }
        else if(!playerInRange && _isBossUIVisible)
        {
            UIManager.Instance?.HideBossInfo();
            _isBossUIVisible = false;
        }
    }

    public void HideBossUI()
    {
        if (_isBossUIVisible)
        {
            UIManager.Instance?.HideBossInfo();
            _isBossUIVisible = false;
        }
    }

    public bool IsPlayerInRange()
    {
        if (playerTransfrom == null) return false;
        float distance = Vector2.Distance(transform.position, playerTransfrom.position);
        return distance <= detectionRadius;
    }

    public bool IsPlayerAlive()
    {
        if (playerTransfrom == null) return false;
        PlayerHealth health = playerTransfrom.GetComponent<PlayerHealth>();
        return health != null && health.CurrentHealth > 0;
    }

    public void ResetToIdle()
    {
        if (_currentState is LeshyAliveState aliveState) aliveState.ReturnToPatrol();
    }

    public void UpdateBossHP()
    {
        if (_isBossUIVisible) UIManager.Instance?.UpdateBossHealth(CurrentHP, maxHP);
    }

    private void InitEyes() => UpdateEyes(maxHP);

    public LeshyAliveState GetAliveState() => _aliveState;
    public LeshyDeadState GetDeadState() => _deadState;

    // для дебага
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackTriggerRange);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.right * attackRayLength);
        Gizmos.DrawRay(transform.position, Vector2.left * attackRayLength);
    }
}
