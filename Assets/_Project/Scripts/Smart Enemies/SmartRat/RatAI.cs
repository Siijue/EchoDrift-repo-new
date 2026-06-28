using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(RatSensorSystem))]

public class RatAI : MonoBehaviour
{
    [Header("Скорости")]
    [SerializeField] public float idleSpeed = 1.2f;
    [SerializeField] public float chaseSpeed = 3.5f;
    [SerializeField] public float fleeSpeed = 4.5f;

    [SerializeField] public float patrolRadiusDefault = 4f;

    [Header("Атака")]
    [SerializeField] public float attackDamage = 0.25f;
    [SerializeField] public float attackRaduis = 1.2f;
    [SerializeField] public float attackCooldown = 1.4f;
    [SerializeField] public float knockbackForce = 3f;

    [Tooltip("Смерть от света")]
    [SerializeField] public float lightKillTime = 1f;

    [Header("Награда")]
    [SerializeField] public int xpReward = 3;
    [SerializeField] public int echoReward = 8;

    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer sprRender;
    [HideInInspector] public RatSensorSystem sensorSystem;

    public RatPatrolZone patrolZone {  get; private set; }


    private RatIdleState _idleState;
    private RatChaseState _chaseState;
    private RatAttackState _attackState;
    private RatFleeState _fleeState;
    private RatDeadState _deadState;

    private IRatState _currentState;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprRender = GetComponent<SpriteRenderer>();
        sensorSystem = GetComponent<RatSensorSystem>();

        _idleState = new RatIdleState(this);
        _chaseState = new RatChaseState(this);
        _attackState = new RatAttackState(this);
        _fleeState = new RatFleeState(this);
        _deadState = new RatDeadState(this);
    }

    private void Start() 
    {
        if(patrolZone == null) patrolZone = new RatPatrolZone(transform.position.x, patrolRadiusDefault, transform.position.y);

        TransitionTo(_idleState);
    }

    private void Update()
    {
        sensorSystem.Tick();
        _currentState?.Update();
    }

    public void SetPatrolZone(RatPatrolZone zone) => patrolZone = zone;
    public void TransitionTo(IRatState newState)
    {
        _currentState?.Exit();

        _currentState = newState;

        _currentState.Enter();

        Debug.Log($"{gameObject.name} переходит в {newState.GetType().Name}");
    }

    public RatIdleState GetIdleState() => _idleState;
    public RatChaseState GetChaseState() => _chaseState;
    public RatAttackState GetAttackState() => _attackState;
    public RatFleeState GetFleeState() => _fleeState;
    public RatDeadState GetDeadState() => _deadState;

}
