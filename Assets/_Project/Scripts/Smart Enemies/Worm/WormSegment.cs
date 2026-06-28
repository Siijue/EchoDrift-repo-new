using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WormSegment : MonoBehaviour
{
    public enum SegmentRole { Head, Middle, Tail}

    [SerializeField] public SegmentRole role = SegmentRole.Head;

    [SerializeField] public float followDistance = 0.9f;
    [SerializeField] public float followSpeed = 12f;
    [SerializeField] public float burnTime = 2f;
    [SerializeField] private float knockbackForce = 5f;

    [HideInInspector] public WormSegment leadSegment;
    [HideInInspector] public WormAI brain;

    [HideInInspector] public SpriteRenderer sprRend;
    [HideInInspector] public Collider2D coll;

    private float _burnTimer;
    private bool _isBurning;

    private void Awake()
    {
        sprRend = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    private void Start()
    {
    }

    public void FollowToHead()
    {
        if (leadSegment == null) return;

        Vector2 toTarget = (Vector2)leadSegment.transform.position - (Vector2)transform.position;

        float dist = toTarget.magnitude;

        if(dist > followDistance)
        {
            Vector2 dir = toTarget.normalized;
            float move = dist - followDistance;
            transform.position += (Vector3)(dir * Mathf.Min(move, followSpeed * Time.fixedDeltaTime));
        }
    }

    public void CheckLightForSplit()
    {
        if (role != SegmentRole.Middle) return;
        if (brain == null) return;
        if(sprRend == null) return;

        bool isLit = LightSourceRegistry.IsPositionLit(transform.position);

        if (isLit)
        {
            if (!_isBurning)
            {
                _isBurning = true;
                _burnTimer = 0f;
            }
            _burnTimer += Time.deltaTime;

            float time = _burnTimer / burnTime;
            sprRend.color = Color.Lerp(Color.white, new Color(0.2f, 0.1f, 0.05f), time);

            if (_burnTimer >= burnTime)
            {
                Debug.Log("splitting");
                brain.Split();
            }
        }
            else
        {
            if (_isBurning)
            {
                _isBurning = false;
                _burnTimer = 0f;
                sprRend.color = Color.white;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (brain == null) return;
        
        if (role != SegmentRole.Head) return; 
        if (!(other is BoxCollider2D)) return;
        if (!other.CompareTag("Player")) return;

        other.GetComponent<PlayerHealth>()?.TakeDamage(0.5f);

        Vector2 dir = ((Vector2)other.transform.position - (Vector2)transform.position).normalized;
        other.GetComponent<PlayerController>()?.ApplyKnockback(dir, knockbackForce);
    }

    public void SetVisible(bool visible)
    {
        sprRend.enabled = visible;
        coll.enabled = visible;
    }

    public void ResetBurnTime()
    {
        _isBurning = false;
        _burnTimer = 0f;
        if (sprRend != null) sprRend.color = Color.white;
    }
}
