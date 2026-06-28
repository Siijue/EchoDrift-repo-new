using UnityEngine;

[RequireComponent (typeof(ScurSensorSystem))]
[RequireComponent(typeof(SpriteRenderer))]

public class ScurAI : MonoBehaviour, IDamageable
{
    [SerializeField] public string enemyID = "enemy_zone_01";

    [SerializeField] public GameObject ratPrefab;
    [SerializeField] public int maxRats = 3;
    [SerializeField] public float patrolRadius = 5f;
    [SerializeField] public float patrolYOffset = 4f;
    [SerializeField] public float summonInterval = 8f;
    [SerializeField] public float lightKillTime = 3f;
    [SerializeField] public bool isMatka = false;
    [SerializeField] public GameObject projectilePrefab;
    [SerializeField] public float projectileSpeed = 4f;
    [SerializeField] public float projectileDamage = 0.5f;
    [SerializeField] public float projectileKnockback = 4f;

    [Header("награды")]
    [SerializeField] public int xpReward = 10;
    [SerializeField] public int echoReward = 25;
    [SerializeField] public GameObject dropItemPrefab;

    [HideInInspector] public SpriteRenderer sprRend;
    [HideInInspector] public ScurSensorSystem sensorSystem;
    [HideInInspector] public RatSpawnManager spawnManag;

    private ScurIdleState _idleState;
    private ScurSummoningState _summoningState;
    private ScurDeadState _deadState;

    private IScurState _currentState;


    private void Awake()
    {
        sprRend = GetComponent<SpriteRenderer>();
        sensorSystem = GetComponent<ScurSensorSystem>();

        spawnManag = gameObject.AddComponent<RatSpawnManager>();
        spawnManag.Initialize(ratPrefab, maxRats, transform, patrolRadius, patrolYOffset);

        if (isMatka) lightKillTime = 8f;

        _idleState = new ScurIdleState(this);
        _summoningState = new ScurSummoningState(this);
        _deadState = new ScurDeadState(this);
    }

    private void Start() => TransitionTo(_idleState);

    private void Update() => _currentState?.Update();

    public void TakeDamage(float damage) => KillInstantly();

    public void TransitionTo(IScurState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void KillInstantly()
    {
        if (_currentState is ScurDeadState) return;
        TransitionTo(_deadState);
    }

    public ScurIdleState GetIdleState() => _idleState;
    public ScurSummoningState GetSummoningState() => _summoningState;
    public ScurDeadState GetDeadState() => _deadState;
} 
