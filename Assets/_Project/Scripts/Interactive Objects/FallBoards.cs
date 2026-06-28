using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]

public class FallBoards : MonoBehaviour
{
    [SerializeField] private float fallAfterTimer = 2f;
    [SerializeField] private float respawnDelay = 5f;
    [SerializeField] private bool isNeedRespawn = false;

    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector3 startPos;
    private bool isFalling = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        startPos = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isFalling) return;
        if (!other.gameObject.CompareTag("Player")) return;
        isFalling = true;
        StartCoroutine(StartFalling());
    }

    private IEnumerator StartFalling()
    {
        yield return new WaitForSeconds(fallAfterTimer);
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
        }

        yield return new WaitForSeconds(0.5f);
        if (coll != null && coll.enabled) coll.enabled = false;
        if(isNeedRespawn) FadeAndDestroy.FadeOut(gameObject, 2.5f, fadeType: FadeAndDestroy.FadeType.Alpha, easeType: FadeAndDestroy.EaseType.EaseOut, onComplete: OnFadeComplete);
        else FadeAndDestroy.FadeAndDestroyObject(gameObject, 2.5f, fadeType: FadeAndDestroy.FadeType.Alpha, easeType: FadeAndDestroy.EaseType.EaseOut, onComplete: null);
    }

    private void OnFadeComplete()
    {
        SpriteRenderer spr = GetComponent<SpriteRenderer>();
        if (spr != null) spr.enabled = false;
        if (coll != null) coll.enabled = false;
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        transform.position = startPos;
        transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (coll != null) coll.enabled = true;

        SpriteRenderer spr = GetComponent<SpriteRenderer>();
        if (spr != null)
        {
            spr.enabled = true;
            Color color = spr.color;
            color.a = 1f;
            spr.color = color;
        }
        isFalling = false;
    }
}
