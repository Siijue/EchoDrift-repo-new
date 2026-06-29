using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]

public class IstAI : MonoBehaviour
{
    public static IstAI Instance { get; private set; }

    [SerializeField] public PixelPerfectCamera pixelPerfectCamera;
    [Header("Здоровье")]
    [SerializeField] public int maxHP = 15;
    [SerializeField] public int phase2Threshold = 9;
    [SerializeField] public int phase3Threshold = 4;

    [Header("Движение")]
    [SerializeField] public float walkSpeed = 2f;
    [SerializeField] public float hoverHeight = 2f;
    [SerializeField] public float hoverAmplitude = 0.5f;
    [SerializeField] public float hoverFrequency = 1f;

    [Header("Уклонение")]
    [SerializeField] public float evadeSpeed = 5f;
    [SerializeField] public float evadeDist = 1.5f;

    [Header("Поведение")]
    [SerializeField] public float minTimeBetweenAbilities = 2f;
    [SerializeField] public float glitchTriggerChance = 0.6f;

    [Header("Атака 'Предельная рефракция'")]
    [SerializeField] public float contactDamage = 0.5f;
    [SerializeField] public float contactCooldown = 4f;

    [Header("Атака 'Завершенность воздаяния'")]
    [SerializeField] public float beamTorchDrain = 4f;
    [SerializeField] public float beamCastTime = 2f;
    [SerializeField] public float beamDuration = 2f;
    [SerializeField] public float beamCooldown = 8f;
    [SerializeField] public GameObject beamPrefab;

    [Header("Атака 'Философия падшего'")]
    [SerializeField] public float debrisDamage = 0.5f;
    [SerializeField] public int debrisCount = 13;
    [SerializeField] public float debrisTelegraph = 0.8f;
    [SerializeField] public float debrisCooldown = 12f;
    [SerializeField] public float debrisSpawnHeight = 10f;
    [SerializeField] public GameObject debrisPrefab;
    [SerializeField] public Bounds arenaBounds;

    [Header("Атака 'Под гравитацией'")]
    [SerializeField] public float vortexPullForce = 6f;
    [SerializeField] public float vortexDuration = 3f;
    [SerializeField] public float vortexCooldown = 12f;
    [SerializeField] public Sprite playerVortexSprite;
    [SerializeField] public GameObject shadowPrefab;

    [Header("Атака 'Суперпозиция тирана'")]
    [SerializeField] public float supDashDamage = 1.5f;
    [SerializeField] public float supDashInvulTime = 3f;
    [SerializeField] public float supDashCooldown = 10f;

    [Header("Атака 'Перехват резонанса'")]
    [SerializeField] public float resonanseCooldown = 16f;
    [SerializeField] public List<ArenaCrystal> arenaCrystals;

    [Header("Спрайты фаз")]
    [SerializeField] public Sprite spritePhase1;
    [SerializeField] public Sprite spritePhase2;
    [SerializeField] public Sprite spritePhase3;

    [Header("Награда")]
    [SerializeField] public int xpReward = 50;
    [SerializeField] public int echoReward = 150;
    [SerializeField] public GameObject memoryItemPrefab;


    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer sprRender;
    [HideInInspector] public Transform playerTransform;


    public int CurrentHP { get; private set; }
    public int Phase { get; private set; } = 1;
    public bool IsDead { get; set; }
    public bool IsInvulnerable { get; set; }

    public BossAbilityQueue MainQueue { get; private set; }
    public BossAbilityQueue GlitchQueue { get; private set; }

    public ContactAbility Contact { get; private set; }
    public BeamAbility Beam { get; private set; }
    public DebrisAbility Debris { get; private set; }
    public VortexAbility Vortex { get; private set; }
    public SuperpositionAbility Superposition { get; private set; }
    public ResonanceAbility Resonance { get; private set; }


    public ReverseWillAbility ReverseWill { get; private set; }
    public FragmentationAbility Fragmentation { get; private set; }
    public MirrorAbility Mirror { get; private set; }
    public CameraRiftAbility CameraRift { get; private set; }
    public TorchDebtAbility TorchDebt { get; private set; }
    public SpectrumAbility Spectrum { get; private set; }


    private IIstState _current;
    private IstAliveState _alive;
    private IstDeadState _dead;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprRender = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;

        if (pixelPerfectCamera == null) pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();

        CurrentHP = maxHP;

        BuildAbilityQueues();

        _alive = new IstAliveState(this);
        _dead = new IstDeadState(this);

        Debug.Log("START FINAL BOSS FIGHT");
    }

    private void Start() 
    {
        if (pixelPerfectCamera != null) pixelPerfectCamera.assetsPPU = 16;

        FindArenaCrystals();
        TransitionTo(_alive);
    }

    private void Update() => _current?.Update();
    private void FixedUpdate() => _current?.FixedUpdate();

    private void BuildAbilityQueues()
    {
        MainQueue = new BossAbilityQueue();
        GlitchQueue = new BossAbilityQueue();

        Contact = new ContactAbility(this);
        Beam = new BeamAbility(this);
        Debris = new DebrisAbility(this);

        MainQueue.Register(Contact);
        MainQueue.Register(Beam);
        MainQueue.Register(Debris);

        Vortex = new VortexAbility(this);
        Superposition = new SuperpositionAbility(this);
        Resonance = new ResonanceAbility(this);

        ReverseWill = new ReverseWillAbility(this);
        Fragmentation = new FragmentationAbility(this);
        Mirror = new MirrorAbility(this);
        CameraRift = new CameraRiftAbility(this);
        TorchDebt = new TorchDebtAbility(this);
        Spectrum = new SpectrumAbility(this);

    }

    public void OnCrystalHit(int damage = 2)
    {
        if (IsDead || IsInvulnerable) return;
        _current?.OnCrystalHit(damage);
    }

    public void TakeDamage(int dmg)
    {
        Debug.Log($"УРОН ИСТУКАНУ: {dmg}");
        if(IsDead || IsInvulnerable) return;
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        _current?.OnDamaged(CurrentHP);
        UIManager.Instance?.UpdateBossHealth(CurrentHP, maxHP);
        Debug.Log($"Текущее хп Истукана: {CurrentHP}");
    }

    public void EnterPhase2()
    {
        Debug.Log("ПЕРЕХОД В ФАЗУ 2");
        if (Phase >= 2) return;
        Phase = 2;
        sprRender.sprite = spritePhase2;
        MainQueue.Register(Vortex);
        MainQueue.Register(Superposition);
        MainQueue.Register(Resonance);
    }

    public void EnterPhase3()
    {
        Debug.Log("ПЕРЕХОД В ФАЗУ 3");
        if (Phase >= 3) return;
        Phase = 3;
        sprRender.sprite = spritePhase3;
        GlitchQueue.Register(ReverseWill);
        GlitchQueue.Register(Fragmentation);
        GlitchQueue.Register(Mirror);
        GlitchQueue.Register(CameraRift);
        GlitchQueue.Register(TorchDebt);
        GlitchQueue.Register(Spectrum);
        UIManager.Instance?.ShowBossInfo("Аваддон Новисс", "Последний убийца света", CurrentHP);
    }

    public void TransitionTo(IIstState newState)
    {
        _current?.Exit();
        _current = newState;
        _current.Enter();
    }

    public IstAliveState GetAlive() => _alive;
    public IstDeadState GetDead() => _dead;

    public List<ArenaCrystal> GetActiveCrystals()
    {
        var list = new List<ArenaCrystal>();
        foreach(var cryst in arenaCrystals) if(cryst != null && cryst.IsActive) list.Add(cryst);
        return list;
    }

    private void FindArenaCrystals()
    {
        bool hasAssignedCrystals = false;
        if (arenaCrystals != null)
        {
            foreach (var crystal in arenaCrystals)
            {
                if (crystal != null)
                {
                    hasAssignedCrystals = true;
                    break;
                }
            }
        }

        if (hasAssignedCrystals) return;
        arenaCrystals = new List<ArenaCrystal>();
        ArenaCrystal[] crystals = FindObjectsByType<ArenaCrystal>(FindObjectsSortMode.None);

        foreach (var crystal in crystals)
        {
            arenaCrystals.Add(crystal);
        }
    }

    public void InitializeBossUI() => UIManager.Instance?.ShowBossInfo("Истукан", "Оскверненная цветосмерть", maxHP);

    public void Despawn()
    {
        Debug.Log("[IstAI] Босс исчезает");
        UIManager.Instance?.HideBossInfo();
        if (Instance == this) Instance = null;
        StopAllCoroutines();
        IsDead = true;
        IsInvulnerable = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        Destroy(gameObject);
    }

    private float _contactDamageTimer;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (IsDead || IsInvulnerable) return;
        if (_current is not IstAliveState) return;

        _contactDamageTimer -= Time.deltaTime;

        if (_contactDamageTimer > 0f) return;
        _contactDamageTimer = contactCooldown;

        collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);
        Vector2 dir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
        collision.gameObject.GetComponent<PlayerController>()?.ApplyKnockback(dir, 4f);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
