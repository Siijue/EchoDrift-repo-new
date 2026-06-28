using UnityEngine;

public class SurfaceCrawler : MonoBehaviour
{
    [SerializeField] public LayerMask surfaceLayer;
    [SerializeField] public float rayLength = 0.4f;
    [SerializeField] public float normalLerpSpeed = 10f;
    [SerializeField] public float edgeCheckDist = 0.3f;

    [SerializeField] public bool isPlayerControlled = false;

    public Vector2 SurfaceNormal { get; private set;  } = Vector2.up;

    public void SetSurfaceNormal(Vector2 normal) => SurfaceNormal = normal.normalized;

    public bool IsOnSurface { get; private set; }

    public float MoveDirection { get; private set; } = 1f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    public void Tick(float speed)
    {
        UpdateSurfaceNormal();

        if (!IsOnSurface) return;

        if (!isPlayerControlled) CheckEdgeAndTurn();

        Move(speed);
    }

    public void Reverse() => MoveDirection *= -1;

    private void UpdateSurfaceNormal()
    {
        Vector2 rayDir = -SurfaceNormal;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, rayLength, surfaceLayer);

        if(hit.collider != null)
        {
            IsOnSurface = true;
            SurfaceNormal = Vector2.Lerp(SurfaceNormal, hit.normal, normalLerpSpeed * Time.fixedDeltaTime).normalized;
        } 
        else IsOnSurface = false;
    }

    private void CheckEdgeAndTurn()
    {
        Vector2 along = GetAlongVector() * MoveDirection;

        Vector2 edgeCheckOrig = (Vector2)transform.position + along * edgeCheckDist;

        RaycastHit2D edgeHit = Physics2D.Raycast(edgeCheckOrig, -SurfaceNormal, rayLength * 1.5f, surfaceLayer);

        if (edgeHit.collider == null)
        {
            RaycastHit2D cornerHit = Physics2D.Raycast((Vector2)transform.position + along * 0.1f, along, edgeCheckDist, surfaceLayer);

            if (cornerHit.collider != null) SurfaceNormal = cornerHit.normal;
            else
            {
                MoveDirection *= -1f;

                if (GetComponent<SpriteRenderer>() != null)
                {
                    GetComponent<SpriteRenderer>().flipY = MoveDirection < 0;
                }
            }
        }
    }

    private void Move(float speed)
    {
        Vector2 moveVector = GetAlongVector() * MoveDirection * speed;
        rb.linearVelocity = moveVector;
    }

    private Vector2 GetAlongVector() => new Vector2(-SurfaceNormal.y, SurfaceNormal.x);


    //debug
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, (Vector3)SurfaceNormal * 0.5f);
    }
}
