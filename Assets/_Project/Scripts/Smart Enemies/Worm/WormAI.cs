using UnityEngine;
using System.Collections;
using System.Linq;

public class WormAI : MonoBehaviour
{
    [SerializeField] public string enemyID = "enemy_zone_01";

    [SerializeField] private WormSegment[] segments = new WormSegment[3];
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float hiddentDuration = 3f;
    [SerializeField] private float activeDuration = 2f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private int xpReward = 10;
    [SerializeField] private int echoReward = 12;

    private IWormState _current;

    private WormHiddenState _hidden;
    private WormEmergingState _emerging;
    private WormActiveState _active;
    private WormRetreatingState _retreating;

    public bool IsActive => _current is WormActiveState;

    private bool _isDead;

    private void Awake()
    {
        if (segments == null || segments.Length == 0 || segments[0] == null) segments = GetComponentsInChildren<WormSegment>();

        for(int i = 0; i < segments.Length; i++)
        {
            if (segments[i] == null) continue;
            segments[i].brain = this;
            segments[i].leadSegment = i > 0 ? segments[i - 1] : null;
        }

        _hidden = new WormHiddenState(this);
        _emerging = new WormEmergingState(this);
        _active = new WormActiveState(this);
        _retreating = new WormRetreatingState(this);
    }

    private void Start()
    {
        PlaceAllAtStart();
        HideAll();
        TransitionTo(_hidden);
    }

    private void Update() => _current?.Update();

    private void FixedUpdate()
    {
        if (_isDead) return;

        if (_current is WormActiveState || _current is WormEmergingState) segments[1]?.CheckLightForSplit();
    }

    public void TransitionTo(IWormState state)
    {
        _current?.Exit();
        _current = state;
        _current.Enter();
    }

    public void Split()
    {
        if (_isDead) return;
        _isDead = true;

        _current?.Exit();
        _current = null;

        StartCoroutine(SplitSequence());
    }

    public void KillInstantly()
    {
        if(_isDead) return;
        _isDead = true;
        _current?.Exit();
        _current = null;

        GameEventBus.Instance?.SendEvent($"Died_{enemyID}", this);

        EconomyManager.Instance?.AddEcho(echoReward);
        EconomyManager.Instance?.AddXP(xpReward);

        foreach (var seg in segments) if (seg != null) seg.SetVisible(false);

        foreach (var seg in segments) if (seg != null) Destroy(seg.gameObject, 0.1f);

        Destroy(gameObject, 0.2f);
    }

    public WormSegment Head => segments.Length > 0 ? segments[0] : null;
    public WormSegment MiddleSegment => segments.Length > 1 ? segments[1] : null;
    public Transform StartPoint => startPoint;
    public Transform EndPoint => endPoint;
    public float MoveSpeed => moveSpeed;
    public float HiddentDuration => hiddentDuration;
    public float ActiveDuration => activeDuration;
    public int XpReward => xpReward;
    public int EchoReward => echoReward;
    public WormSegment[] Segments => segments;

    public WormHiddenState GetHidden() => _hidden;
    public WormEmergingState GetEmerging() => _emerging;
    public WormActiveState GetActive() => _active;
    public WormRetreatingState GetRetreating() => _retreating;


    public void PlaceAllAtStart()
    {
        if (startPoint == null) return;
        for(int i = 0; i < segments.Length; i++)
        {
            if(segments[i] == null) continue;

            Vector3 offset = Vector3.left * i * segments[i].followDistance;
            segments[i].transform.position = startPoint.position + offset;
            segments[i].ResetBurnTime();
        }
    }

    public void ShowAll()
    {
        foreach (var seg in segments) seg?.SetVisible(true);
    }

    public void HideAll()
    {
        //foreach (var seg in segments) seg?.SetVisible(false);
    }

    public bool MoveHeadTowards(Vector3 target, float speed)
    {
        if(Head == null) return false;

        float dx = target.x - Head.transform.position.x;
        bool facingLeft = dx > 0;

        foreach(var seg in segments) if (seg != null) seg.sprRend.flipY = facingLeft;

        Head.transform.position = Vector3.MoveTowards(Head.transform.position, target, speed * Time.fixedDeltaTime);

        for (int i = 0; i < segments.Length; i++) segments[i]?.FollowToHead();

        float dist = Vector3.Distance(Head.transform.position, target);
        return dist < 0.1f;
    }

    private IEnumerator SplitSequence()
    {
        float elapsed = 0f;
        const float retreatTime = 2f; //0.8f

        Vector3 headStart = segments[0] != null ? segments[0].transform.position : startPoint.position;
        Vector3 tailStart = segments[2] != null ? segments[2].transform.position : startPoint.position;
        Vector3 headTarget = startPoint.position;
        Vector3 tailTarget = EndPoint.position;

        bool headFacingLeft = headTarget.x > headStart.x;
        bool tailFacingLeft = tailTarget.x > tailStart.x;

        if (segments[0] != null && segments[0].sprRend != null)
            segments[0].sprRend.flipY = headFacingLeft;

        if (segments[2] != null && segments[2].sprRend != null)
            segments[2].sprRend.flipY = tailFacingLeft;

        while (elapsed < retreatTime)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / retreatTime;

            if (segments[0] != null)
            {
                segments[0].transform.position = Vector3.Lerp(headStart, headTarget, time);
            }
            if (segments[2] != null)
            {
                segments[2].transform.position = Vector3.Lerp(tailStart, tailTarget, time);
            }
            if (segments[1] != null) SlugAI.SetAlpha(segments[1].sprRend, Mathf.Lerp(1f, 0f, time));

            yield return null;
        }

        GameEventBus.Instance?.SendEvent($"Died_{enemyID}", this);

        EconomyManager.Instance?.AddEcho(echoReward);
        EconomyManager.Instance?.AddXP(xpReward);

        foreach(var segment in segments) if(segment != null) Destroy(segment.gameObject, 0.1f);

        Destroy(gameObject, 0.2f);
    }
}
