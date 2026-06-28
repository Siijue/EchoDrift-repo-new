using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ContactAbility : IstAbility
{
    private readonly IstAI _own;
    public ContactAbility(IstAI own) : base("ContactAbility", own.contactCooldown, priority: 3) => _own = own;
    protected override void Execute() 
    {
        Debug.Log("Атака 'Предельная рефракция'");
    }
}

public class BeamAbility : IstAbility
{
    private IstAI _own;
    public BeamAbility(IstAI own) : base("BeamDrain", own.beamCooldown, priority: 5) => _own = own;
    protected override void Execute()
    {
        if (_own.playerTransform == null) return;
        Debug.Log("Атака 'Завершенность воздаяния'");
        _own.StartCoroutine(BeamSequence());
    }

    private IEnumerator BeamSequence()
    {
        if (_own.playerTransform == null) yield break;

        _own.sprRender.color = new Color(1f, 0.6f, 0.1f);
        yield return new WaitForSeconds(_own.beamCastTime);
        _own.sprRender.color = Color.white;

        if (_own.beamPrefab == null) yield break;

        Vector2 dir = ((Vector2)_own.playerTransform.position - (Vector2)_own.transform.position).normalized;

        GameObject beam = Object.Instantiate(_own.beamPrefab, _own.transform.position, Quaternion.identity);
        beam.transform.up = dir;
        beam.transform.localScale = Vector3.zero;

        beam.GetComponent<IstBeam>()?.Init(dir, _own.beamDuration, _own.beamTorchDrain);

        float maxBeamLength = 10f;
        float beamSpriteLength = 2f;
        float growSpeed = 50f;
        float currentLength = 0f;

        LayerMask layerMask = LayerMask.GetMask("Player", "Ground", "Obstacles");

        while (currentLength < maxBeamLength)
        {
            RaycastHit2D hit = Physics2D.Raycast(_own.transform.position, dir, currentLength + Time.deltaTime * growSpeed, layerMask);

            if (hit.collider != null)
            {
                float hitDistance = hit.distance;

                currentLength = Mathf.MoveTowards(currentLength, hitDistance, Time.deltaTime * growSpeed);

                if (Mathf.Abs(currentLength - hitDistance) < 0.1f)
                {
                    currentLength = hitDistance;
                    break;
                }
            }
            else
            {
                currentLength += Time.deltaTime * growSpeed;
            }

            if (_own.playerTransform != null)
            {
                dir = ((Vector2)_own.playerTransform.position - (Vector2)_own.transform.position).normalized;
                beam.transform.up = dir;
            }

            float scale = currentLength / beamSpriteLength;
            beam.transform.localScale = new Vector3(1f, scale, 1f);

            yield return null;
        }

        float finalScale = currentLength / beamSpriteLength;
        beam.transform.localScale = new Vector3(1f, finalScale, 1f);
    }
}

public class DebrisAbility : IstAbility
{
    private IstAI _own;

    public DebrisAbility(IstAI own) : base("DebrisRain", own.debrisCooldown, priority: 4) => _own = own;

    protected override void Execute()
    {
        Debug.Log("Атака 'Философия падшего'");
        _own.StartCoroutine(DebrisSequence());
    }

    private IEnumerator DebrisSequence()
    {
        if (_own.debrisPrefab == null) yield break;

        var shadows = new List<GameObject>();
        var shadowRenderers = new List<SpriteRenderer>();
        var positions = new List<Vector3>();

        for (int i = 0; i < _own.debrisCount; i++)
        {
            float x = Random.Range(_own.arenaBounds.min.x, _own.arenaBounds.max.x);
            float y = _own.arenaBounds.min.y;
            Vector3 pos = new Vector3(x, y, 0f);
            positions.Add(pos);

            GameObject shadow = Object.Instantiate(_own.shadowPrefab, pos, Quaternion.identity);
            shadows.Add(shadow);

            SpriteRenderer spr = shadow.GetComponent<SpriteRenderer>();
            if (spr != null) shadowRenderers.Add(spr);
        }

        yield return new WaitForSeconds(_own.debrisTelegraph);

        foreach (var pos in positions)
        {
            Vector3 spawnPos = pos + Vector3.up * _own.debrisSpawnHeight;
            GameObject debris = Object.Instantiate(_own.debrisPrefab, spawnPos, Quaternion.identity);
            debris.GetComponent<IstDebris>()?.Init(_own.debrisDamage, pos);
        }

        yield return _own.StartCoroutine(FadeShadows(shadowRenderers, 1f));

        foreach (var shadow in shadows) Object.Destroy(shadow);
    }

    private IEnumerator FadeShadows(List<SpriteRenderer> renderers, float duration)
    {
        float elapsed = 0f;

        var startColors = new List<Color>();
        foreach (var spr in renderers)
        {
            startColors.Add(spr.color);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null) continue;

                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, 0f, t);
                renderers[i].color = c;
            }

            yield return null;
        }
    }
}

public class VortexAbility : IstAbility
{
    private readonly IstAI _own;

    public VortexAbility(IstAI own) : base("Vortex", own.vortexCooldown, priority: 6) => _own = own;

    protected override void Execute()
    {
        Debug.Log("Атака 'Под гравитацией'");
        _own.StartCoroutine(VortexSequence());
    }

    private IEnumerator VortexSequence()
    {
        if (_own.playerTransform == null) yield break;

        PlayerController player = _own.playerTransform.GetComponent<PlayerController>();
        StatusDataSystem effects = _own.playerTransform.GetComponent<StatusDataSystem>();
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        Sprite defaultPlayerSprite = playerSprite?.sprite;

        _own.sprRender.color = Color.lightGreen;
        yield return new WaitForSeconds(1f);
        _own.sprRender.color = Color.white;

        effects?.AddStatus(new StatusData(
            type: StatusType.Root,
            duration: _own.vortexDuration,
            blockMovement: true,
            blockJump: true,
            exitOnDash: true
        ));

        float elapsed = 0f;
        while (elapsed < _own.vortexDuration)
        {
            elapsed += Time.fixedDeltaTime;

            if (_own.playerTransform != null)
            {
                Rigidbody2D prefRb = _own.playerTransform.GetComponent<Rigidbody2D>();

                if (prefRb != null)
                {
                    UIManager.Instance?.ShowHint("Вы попали под гравитацию Истукана! Вырвитесь рывком!", -1);

                    if (_own.playerVortexSprite != null && playerSprite != null)
                    {
                        playerSprite.sprite = _own.playerVortexSprite;
                    }

                    Vector2 toCenter = ((Vector2)_own.transform.position - (Vector2)_own.playerTransform.position).normalized;
                    prefRb.AddForce(toCenter * _own.vortexPullForce, ForceMode2D.Force);
                }

                if (effects != null && !effects.HasStatus(StatusType.Root))
                {
                    UIManager.Instance?.HideHint();
                    break;
                }
            }
            else
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        if (playerSprite != null && defaultPlayerSprite != null)
        {
            playerSprite.sprite = defaultPlayerSprite;
        }
    }
}

public class SuperpositionAbility : IstAbility
{
    private readonly IstAI _own;

    public SuperpositionAbility(IstAI own) : base("SuperpositionAbility", own.supDashCooldown, priority: 7) => _own = own;

    protected override void Execute()
    {
        Debug.Log("Атака 'Суперпозиция тирана'");
        _own.StartCoroutine(SuperposCrtn());
    }

    private IEnumerator SuperposCrtn()
    {
        if (_own.playerTransform == null) yield break;

        Color color = _own.sprRender.color;
        color.a = 0.4f;
        _own.sprRender.color = color;

        _own.IsInvulnerable = true;
        yield return new WaitForSeconds(_own.supDashInvulTime);
        

        color.a = 1f;
        _own.sprRender.color = color;

        Vector2 target = (Vector2)_own.playerTransform.position + Vector2.up * _own.hoverHeight;

        float elapsed = 0f;
        Vector2 startPos = _own.transform.position;

        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            _own.transform.position = Vector2.Lerp(startPos, target, elapsed / 0.2f);
            yield return null;
        }

        _own.IsInvulnerable = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(_own.transform.position, 2f);
        foreach(var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            PlayerController player = hit.GetComponent<PlayerController>();
            if (player != null && player.IsDashing) continue; 
            hit.GetComponent<PlayerHealth>()?.TakeDamage(_own.supDashDamage);
            Vector2 dir = ((Vector2)hit.transform.position - (Vector2)_own.transform.position).normalized;
            hit.GetComponent<PlayerController>()?.ApplyKnockback(dir, 6f);
        }
    }
}

public class ResonanceAbility : IstAbility
{
    private readonly IstAI _own;

    public ResonanceAbility(IstAI own) : base("ResonanceAbility", own.resonanseCooldown, priority: 8) => _own = own;

    public override bool CanUse() => IsReady && _own.GetActiveCrystals().Count > 0;

    protected override void Execute()
    {
        Debug.Log("Атака 'Перехват резонанса'");
        _own.StartCoroutine(ResonanceCrtn());
    }

    private IEnumerator ResonanceCrtn()
    {
        var crystals = _own.GetActiveCrystals();

        for(int shot = 0; shot < 2; shot++)
        {
            foreach(var crystal in crystals)
            {
                Color defaultCrystalColor = crystal.crystalLight.color;
                crystal.crystalLight.color = Color.red;
                crystal.crystalLight.enabled = true;
                crystal?.FireAtPlayer();
                yield return new WaitForSeconds(0.3f);
                crystal.crystalLight.enabled = false;
                crystal.crystalLight.color = defaultCrystalColor;
            }
            yield return new WaitForSeconds(1f);
        }
    }
}