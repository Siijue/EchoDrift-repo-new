using UnityEngine;
using System.Collections;

public class ShadowDashAbility : VelesAbility
{
    private readonly VelesAI _own;

    public ShadowDashAbility(VelesAI own) : base("ShadowDash", own.shadowDashCooldown, priority: 5) => _own = own;

    protected override void Execute() 
    {
        if (_own.playerTransform == null || Mathf.Abs(_own.playerTransform.position.x - _own.transform.position.x) < 1) return;
        Debug.LogWarning($"(ShadowDashAbility) ПРИСТУПИЛ К ВЫПОЛНЕНИЮ КОРУТИНЫ");
        _own.StartCoroutine(DashSequence());
    }

    private IEnumerator DashSequence()
    {

        Debug.Log("DASH COROUTINE");
        if(_own.playerTransform == null) yield break;

        Vector2 backDir = (_own.playerTransform.position.x > _own.transform.position.x) ? Vector2.left : Vector2.right;

        Vector2 backPos = (Vector2)_own.transform.position + backDir * 2f;
        

        float elapsed = 0f;
        float telegraphDuration = _own.dashTelegraphDuration;

        while (elapsed < telegraphDuration)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / telegraphDuration;

            _own.rb.MovePosition(Vector2.Lerp(_own.transform.position, backPos, elapsed / _own.dashTelegraphDuration));

            float flashSpeed = Mathf.Lerp(3f, 12f, time);

            float flash = Mathf.PingPong(elapsed * flashSpeed, 1f);
            _own.FlashEyes(Color.Lerp(_own.eyeNormalColor, Color.red, flash));
            yield return null;
        }

        if (_own.dashVFXPrefab != null)
        {
            GameObject vfx = Object.Instantiate(_own.dashVFXPrefab, _own.transform.position, Quaternion.identity);
            _own.StartCoroutine(FadeAndDestroyVFX(vfx, 1.5f));
        }

        Vector2 dashTarget = (Vector2)_own.playerTransform.position + Vector2.up * _own.hoverHeight;
        _own.transform.position = dashTarget;

        float strikeDelay = 0.5f;
        float delayElapsed = 0f;

        while (delayElapsed < strikeDelay)
        {
            delayElapsed += Time.deltaTime;
            _own.FlashEyes(Color.red);
            yield return null;
        }

        yield return new WaitForFixedUpdate();

        if (_own.dashHitBox != null)
        {
            Debug.Log("enemy dash");
            Debug.Log($"dashHitBox enabled: {_own.dashHitBox.enabled}, isTrigger: {_own.dashHitBox.isTrigger}");
            Collider2D[] results = new Collider2D[8];
            int count = _own.dashHitBox.Overlap(ContactFilter2D.noFilter, results);

            for(int i = 0; i < count; i++) {
                {
                    Collider2D hit = results[i];
                    Debug.Log($"Hit {i}: {hit.gameObject.name}, tag: {hit.tag}, layer: {LayerMask.LayerToName(hit.gameObject.layer)}");

                    if (hit == null || !hit.CompareTag("Player")) continue;

                    

                    hit.GetComponent<PlayerHealth>()?.TakeDamage(_own.dashDamage);
                    Vector2 knockDir = ((Vector2)hit.transform.position - (Vector2)_own.transform.position).normalized;
                    hit.GetComponent<PlayerController>()?.ApplyKnockback(knockDir, _own.dashKnockBack);
                }
        }
    }

        _own.UpdateEyes();
    }

    private IEnumerator FadeAndDestroyVFX(GameObject obj, float fadeDuration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            float elapsed = 0f;
            float startAlpha = sr.color.a;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                Color c = sr.color;
                c.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                sr.color = c;
                yield return null;
            }
        }

        Object.Destroy(obj);
    }
}

public class ExtinguishAbility : VelesAbility
{
    private readonly VelesAI _own;

    public ExtinguishAbility(VelesAI own) : base("ExtinguishAbility", own.extinguishCooldown, priority: 4) => _own = own;

    public override bool CanUse()
    {
        if(!IsReady) return false;
        return _own.GetActiveCrystals().Count > 0;
    }

    protected override void Execute()
    {
        var active = _own.GetActiveCrystals();
        if (active.Count == 0) return;

        int rand = Random.Range(0, active.Count);
        active[rand].Extinguish(10f);
    }
}

public class ShadowOrbAbility : VelesAbility
{
    private readonly VelesAI _own;

    public ShadowOrbAbility(VelesAI own) : base("ShadowOrb", own.shadowOrbCooldown, priority: 6) => _own = own;

    protected override void Execute()
    {
        if(_own.orbPrefab == null || _own.playerTransform == null) return;

        float[] angles = { -30f, 0f, 30f };
        Vector2 baseDir = ((Vector2)_own.playerTransform.position - (Vector2)_own.transform.position).normalized;

        foreach(float angle in angles)
        {
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;
            GameObject orb = Object.Instantiate(_own.orbPrefab, _own.transform.position, Quaternion.identity);
            orb.GetComponent<ShadowOrb>()?.Init(dir, _own.orbSpeed, _own.orbDamage, _own.orbHomingRadius, _own.playerTransform);
        }
    }
}


public class SummonShadowsAbility : VelesAbility
{
    private readonly VelesAI _own;

    public SummonShadowsAbility(VelesAI own) : base("SummonShadows", own.summonCooldown, priority: 3) => _own = own;

    protected override void Execute()
    {
        if (_own.shadowHeraldPrefab == null) return;

        int count = 2;
        for(int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetSpawnPoints(i);
            Object.Instantiate(_own.shadowHeraldPrefab, spawnPos, Quaternion.identity);
        }
    }

    private Vector3 GetSpawnPoints(int index)
    {
        if (_own.summonPoints != null && index < _own.summonPoints.Length && _own.summonPoints[index] != null) return _own.summonPoints[index].position;

        return _own.transform.position + new Vector3(Random.Range(-3f, 3f), 0f, 0f);
    }
}


public class GazeOfAbyssAbility : VelesAbility
{
    private readonly VelesAI _own;

    public GazeOfAbyssAbility(VelesAI own) : base("GazeOfAbyss", own.gazeOfAbyssCooldown, priority: 10) => _own=own;

    public override bool CanUse() => IsReady && _own.IsPhase2;

    protected override void Execute()
    {
        if (_own.gazePrefab == null) return;

        for(int i = 0; i < _own.gazeRayCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float rotationZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, rotationZ);
            GameObject ray = Object.Instantiate(_own.gazePrefab, _own.transform.position, rotation);
            ray.GetComponent<GazeRay>()?.Init(dir, _own.gazeDuration);
        }
    }
}