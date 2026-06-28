using UnityEngine;

[RequireComponent (typeof(Collider2D))]
[RequireComponent (typeof(SpriteRenderer))]

public class Tendon : MonoBehaviour
{
    [SerializeField] private Sequester sequester;

    [SerializeField] public float attackRadius = 3f;
    [SerializeField] public float attackDamage = 0.5f;
    [SerializeField] public float attackDuration = 1f;
    [SerializeField] public float restDuration = 2f;

    [SerializeField] public float burnTime = 3f;


    [HideInInspector] public SpriteRenderer sprRend;
    [HideInInspector] public Collider2D coll;


    private ITendonState _current;

    private TendonIdleState _idleState;
    private TendonWhippingState _whippingState;
    private TendonRetractedState _retractedState;
    private TendonBurningState _burningState;
    private TendonDeadState _deadState;

    private void Awake()
    {
        sprRend = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();

        _idleState = new TendonIdleState(this);
        _whippingState = new TendonWhippingState(this);
        _retractedState = new TendonRetractedState(this);
        _burningState = new TendonBurningState(this);
        _deadState = new TendonDeadState(this);
    }

    private void Start()
    {
        if (sequester != null) sequester.RegisterTendon(this);
        else Debug.LogWarning("Поле auquester не назначено");

        TransitionTo(_idleState);
    }

    private void Update() => _current?.Update();

    public void TransitionTo(ITendonState newState)
    {
        _current?.Exit();
        _current = newState;
        _current.Enter();
    }

    public void Retract(float duration)
    {
        if (_current is TendonDeadState) return;
        (_retractedState as TendonRetractedState)?.SetDuration(duration);
        TransitionTo(_retractedState);
    }

    public void NotifySequesterOfDeath() => sequester?.OnTendonDestroyed(this);

    //public void NotifySequesterOfEnemyKill() => sequester?.OnTendonKilledEnemy();

    public TendonIdleState GetIdleState() => _idleState;
    public TendonWhippingState GetWhippingState() => _whippingState;
    public TendonRetractedState GetRetractedState() => _retractedState;
    public TendonBurningState GetBurningState() => _burningState;
    public TendonDeadState GetDeadState() => _deadState;
}
