using UnityEngine;

public class ScurSummoningState : IScurState
{
    private readonly ScurAI _owner;

    private float _summonTimer;
    private float _lightExpsosureTimer;
    private float _shootTimer;

    private const float firstShootDelay = 1f;
    private const float shootCooldown = 3.2f;

    public ScurSummoningState(ScurAI owner) => _owner = owner;

    public void Enter() { _lightExpsosureTimer = 0f; _summonTimer = _owner.summonInterval; _shootTimer = _owner.summonInterval; }

    public void Update()
    {
        HandleLightDamage();
        if (_owner.sensorSystem.lightHitsScur) return;

        CheckPlayerLeft();
        HandleSummonTimer();
    }

    public void Exit() { _lightExpsosureTimer = 0f; _summonTimer = 0f; _shootTimer = 0f;}

    private void HandleLightDamage()
    {if (!_owner.sensorSystem.lightHitsScur)
        {
            _lightExpsosureTimer = 0f;
            _shootTimer = 0f;
            ReturnToPreviousColor();
            return;
        }

        _lightExpsosureTimer += Time.deltaTime;
        _shootTimer += Time.deltaTime;
        UpdateSpriteColor();

        if (_owner.isMatka) TryShoot();

        if (_lightExpsosureTimer >= _owner.lightKillTime) _owner.TransitionTo(_owner.GetDeadState());
    }

    private void CheckPlayerLeft()
    {
        if (!_owner.sensorSystem.playerInrange) _owner.TransitionTo(_owner.GetIdleState());
    }

    private void HandleSummonTimer()
    {
        _summonTimer += Time.deltaTime;

        if(_summonTimer >= _owner.summonInterval)
        {
            _summonTimer = 0f;
            TrySummon();
        }
    }

    private void TrySummon()
    {
        if (!_owner.spawnManag.CanSpawn()) return;

        _owner.spawnManag.SpawnRat();
    }

    private void TryShoot()
    {
        bool isFirstShoot = _shootTimer >= firstShootDelay && _lightExpsosureTimer <= firstShootDelay + Time.deltaTime;

        bool isCooldownReady = _shootTimer >= firstShootDelay * shootCooldown;

        if (!isFirstShoot && !isCooldownReady) return;

        if (isCooldownReady) _shootTimer = firstShootDelay;

        Shoot();
    }

    private void Shoot()
    {
        if(_owner.projectilePrefab == null) return;

        Transform playerTransform = _owner.sensorSystem.GetPlayerTransform();
        if(playerTransform == null) return;

        Vector2 toPlayer = (Vector2)(playerTransform.position - _owner.transform.position);
        Vector2 direction = toPlayer.normalized;

        Vector3 spawnOffeset = (Vector3)(direction * 0.6f);
        Vector3 spawnPos = _owner.transform.position + spawnOffeset;

        GameObject projObj = Object.Instantiate(_owner.projectilePrefab, spawnPos, Quaternion.identity);

        ScurProjectile proj = projObj.GetComponent<ScurProjectile>();
        proj?.Init(direction, _owner.projectileSpeed, _owner.projectileKnockback, _owner.projectileDamage);
    }

    private void UpdateSpriteColor()
    {
        float timer = Mathf.Clamp01(_lightExpsosureTimer / _owner.lightKillTime);

        Color normalColor = Color.white;
        Color burnColor = new Color(0.569f, 0.090f, 0.000f, 1.000f);

        _owner.sprRend.color = Color.Lerp(normalColor, burnColor, timer);
    }

    private void ReturnToPreviousColor()
    {
        Color currentColor = _owner.sprRend.color;
        Color startColor = Color.white;

        _owner.sprRend.color = Color.Lerp(currentColor, startColor, 5);
    }
}
