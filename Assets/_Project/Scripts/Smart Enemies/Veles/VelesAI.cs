using UnityEngine;
using System.Collections.Generic;

public class VelesAI : MonoBehaviour
{
    public static VelesAI Instance { get; private set; }

    [Header("Здоровье")]
    [SerializeField] public int maxHP = 8;

    [Header("Движение")]
    [SerializeField] public float hoverHeight = 2f;
    [SerializeField] public float hoverSpeed = 2f;
    [SerializeField] public float hoverAmplitude = 0.3f;

    [Header("Способности")]
    [SerializeField] public float minTimeBetweenAbilities = 5f;
    [SerializeField] public float shadowDashCooldown = 8f;
    [SerializeField] public float extinguishCooldown = 12f;
    [SerializeField] public float shadowOrbCooldown = 6f;
    [SerializeField] public float summonCooldown = 15f;
    [SerializeField] public float gazeOfAbyssCooldown = 20f;

    [Header("Теневой рывок")]
    [SerializeField] public float dashDamage = 1.5f;
    [SerializeField] public float dashKnockBack = 5f;
    [SerializeField] public float dashTelegraphDuration = 1.2f;
    [SerializeField] public GameObject dashVFXPrefab;
    [SerializeField] public Collider2D dashHitBox;

    [Header("Погасание")]
    [SerializeField] public float extinguishCastDuration = 1.5f;

    [Header("Теневая сфера")]
    [SerializeField] public GameObject orbPrefab;
    [SerializeField] public float orbDamage = 1f;
    [SerializeField] public float orbSpeed = 2f;
    [SerializeField] public float orbHomingRadius = 4f;

    [Header("Призыв Вестников")]
    [SerializeField] public GameObject shadowHeraldPrefab;
    [SerializeField] public Transform[] summonPoints;

    [Header("Глаза бездны")]
    [SerializeField] public GameObject gazePrefab;
    [SerializeField] public int gazeRayCount = 6;
    [SerializeField] public float gazeDuration = 5f;

    [Header("Стан при попадании кристалла")]
    [SerializeField] public float stunDuration = 3f;
    [SerializeField] public int crystalDamage = 2;

    [Header("Глаза")]
    [SerializeField] public SpriteRenderer[] eyeRenderers = new SpriteRenderer[6];
    [SerializeField] public Color eyeNormalColor = new Color(0.8f, 0.1f, 0.1f);
    [SerializeField] public Color eyePhase2Color = new Color(1f, 0.5f, 0.0f);
    [SerializeField] public Color eyeFlashColor = Color.white;

    [Header("Фаза 2 (здоровье меньше 4)")]
    [SerializeField] public float phase2SpeedBonus = 0.5f;
    [SerializeField] public SpriteRenderer sprRenderPhase2;

    [Header("Кристаллы")]
    [SerializeField] public List<VelesCrystal> crystals;

    [Header("Лут")]
    [SerializeField] public int xpReward = 50;
    [SerializeField] public int echoReward = 100;

    [Header("Обнаружение игрока")]
    [SerializeField] public float detectionRadius = 15f;
    [SerializeField] public float maxChaseDistance = 25f;


    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer sprRender;
    [HideInInspector] public Transform playerTransform;


    public int CurrentHP { get; private set; }
    public int Phase { get; private set; } = 1;
    public bool IsPhase2 => Phase == 2;
    public bool IsDead { get; set; }



    public BossAbilityQueue AbilityQueue { get; private set; }

    public ShadowDashAbility ShadowDash { get; private set;  }
    public ExtinguishAbility Extinguish { get; private set; }
    public ShadowOrbAbility ShadowOrb { get; private set; }
    public SummonShadowsAbility SummonShadows { get; private set; }
    public GazeOfAbyssAbility GazeOfAbyss { get; private set; }


    private IVelesState _currentState;
    private VelesAliveState _aliveState;
    private VelesDeadState _deadState;
    private bool isBossUIVisible = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprRender = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        GameObject player = GameObject.FindWithTag("Player");
        if(player != null) playerTransform = player.transform;

        CurrentHP = maxHP;

        AbilityQueue = new BossAbilityQueue();

        ShadowDash = new ShadowDashAbility(this);
        Extinguish = new ExtinguishAbility(this);
        ShadowOrb = new ShadowOrbAbility(this);
        SummonShadows = new SummonShadowsAbility(this);
        GazeOfAbyss = new GazeOfAbyssAbility(this);


        AbilityQueue.Register(ShadowDash);
        AbilityQueue.Register(Extinguish);
        AbilityQueue.Register(SummonShadows);
        AbilityQueue.Register(GazeOfAbyss);

        _aliveState = new VelesAliveState(this);
        _deadState = new VelesDeadState(this);
    }

    private void Start()
    {
        InitEyes();
        TransitionTo(_aliveState);
    }

    private void Update() => _currentState?.Update();

    private void FixedUpdate() => _currentState?.FixedUpdate();

    public void OnCrystalHit()
    {
        if (IsDead) return;
        _currentState?.OnCrystalHit();
    }

    public List<VelesCrystal> GetActiveCrystals()
    {
        var active = new List<VelesCrystal>();
        foreach(var crystal in crystals) if(crystal != null && crystal.IsActive) active.Add(crystal);
        return active;
    }

    public void EnterPhase2()
    {
        if (Phase == 2) return;
        Phase = 2;

        AbilityQueue.Register(GazeOfAbyss);

        ShadowDash.Priority = 8;
        ShadowOrb.Priority = 7;
        Extinguish.Priority = 6;
    }

    public void TakeDamage(int dmg)
    {
        Debug.Log($"лог TakeDameg: dmg={dmg}, CurrentHp={CurrentHP}");
        if (IsDead) return;
        CurrentHP = Mathf.Max(0, CurrentHP - dmg);
        UpdateEyes();
        _currentState?.OnDamaged(CurrentHP);
        UIManager.Instance?.UpdateBossHealth(CurrentHP, maxHP);
        Debug.Log($"After damage={CurrentHP}");
    }

    public void TransitionTo(IVelesState state)
    {
        Debug.Log($"Велес переходит из {_currentState} в {state}");
        _currentState?.Exit();
        _currentState = state;
        _currentState.Enter();
    }

    public VelesAliveState GetAliveState() => _aliveState;
    public VelesDeadState GetDeadState() => _deadState;

    public bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        float dist = Vector2.Distance(transform.position, playerTransform.position);
        return dist <= detectionRadius;
    }

    public bool IsPlayerAlive()
    {
        if (playerTransform == null) return false;
        PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
        return health != null && health.CurrentHealth > 0;
    }

    public void UpdatePlayerRef()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    public void ResetToIdle()
    {
        if (_currentState is VelesAliveState aliveState) aliveState.ReturnToIdle();
    }


    public void UpdateEyes()
    {
        int litCount = Mathf.CeilToInt((float)CurrentHP / maxHP * eyeRenderers.Length);

        Color activeColor = IsPhase2 ? eyePhase2Color : eyeNormalColor;

        for(int i = 0; i < eyeRenderers.Length; i++)
        {
            if (eyeRenderers[i] == null) continue;
            eyeRenderers[i].color = i < litCount ? activeColor : new Color(0.05f, 0.02f, 0.02f);
        }
    }

    public void FlashEyes(Color color)
    {
        foreach (var eye in eyeRenderers) if (eye != null) eye.color = color;
    }

    private void InitEyes() => UpdateEyes();


    public void UpdateBossUIVisibility()
    {
        bool playerInRange = IsPlayerInRange() && IsPlayerAlive();
        if(playerInRange && !isBossUIVisible)
        {
            UIManager.Instance?.ShowBossInfo("ТЕНЕВОЙ ВЕЛЕС", "Таинство скорой Смерти", maxHP);
            isBossUIVisible = true;
        }
        else if(!playerInRange && isBossUIVisible)
        {
            UIManager.Instance?.HideBossInfo();
            isBossUIVisible = false;
        }
    }

    public void HideBossUI()
    {
        if (isBossUIVisible)
        {
            UIManager.Instance?.HideBossInfo();
            isBossUIVisible = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.mediumPurple;
        Gizmos.DrawWireSphere(transform.position, orbHomingRadius);
    }
}
