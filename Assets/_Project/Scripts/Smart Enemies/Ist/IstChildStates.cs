using UnityEngine;
using System.Collections;

public class IstIdleState : IstChildBaseState
{
    private float _timer;

    public IstIdleState(IstAI own, IstAliveState supState) : base(own, supState) { }

    public override void Enter() => _timer = 0f;

    public override void Update()
    {
        _timer += Time.deltaTime;
        _own.MainQueue.Tick(Time.deltaTime);

        if(_own.Phase >= 3) _own.GlitchQueue.Tick(Time.deltaTime);

        if(CheckActiveRays())
        {
            _superState.StartEvading();
            return;
        }

        if (_timer < _own.minTimeBetweenAbilities) return;

        IstAbility main = _own.MainQueue.SelectBest() as IstAbility;
        if(main == null) return;

        if(_own.Phase >= 3 && Random.value <= _own.glitchTriggerChance)
        {
            BossAbility glitch = _own.GlitchQueue.SelectBest();
            glitch?.TriggerAndCooldown();
        }

        _superState.StartCasting(main);
    }

    private bool CheckActiveRays()
    {
        foreach(var crystal in _own.GetActiveCrystals()) if (crystal != null && crystal.IsRayActive && crystal.IsPositionOnRay(_own.transform.position)) return true;
        return false;
    }
}


public class IstCastingState : IstChildBaseState
{
    private IstAbility _ability;
    private float _timer;
    private const float DefaultCastTime = 0.6f;

    public IstCastingState(IstAI own, IstAliveState supState) : base(own, supState) { }

    public void SetAbility(IstAbility ability) => _ability = ability;

    public override void Enter() => _timer = 0f;

    public override void Update()
    {
        _timer += Time.deltaTime;
        if(_timer >= DefaultCastTime)
        {
            _ability?.TriggerAndCooldown();
            _superState.ReturnToIdle();
        }
    }
}

public class IstStunnedState : IstChildBaseState
{
    private float _timer;
    private const float StunDuration = 2f;

    public IstStunnedState(IstAI own, IstAliveState supState) : base(own, supState) { }

    public override void Enter()
    {
        _timer = 0f;
        _own.rb.linearVelocity = Vector2.zero;
        _own.sprRender.color = Color.white * 1.8f;
    }

    public override void Update()
    {
        _timer += Time.deltaTime;
        float time = _timer / StunDuration;
        _own.sprRender.color = Color.Lerp(Color.white * 1.8f, Color.white, time);
        if(_timer >= StunDuration)
        {
            _own.sprRender.color = Color.white;
            _superState.ReturnToIdle();
        }
    }

    public override void Exit() => _own.sprRender.color = Color.white;

    public override void OnCrystalHit(int damage) => _own.TakeDamage(damage);
}

public class IstEvadingState : IstChildBaseState
{
    private float _timer;
    private Vector2 _evadeDir;
    private const float maxEvadeTime = 1.5f;

    public IstEvadingState(IstAI own, IstAliveState supState) : base(own, supState) { }

    public override void Enter()
    {
        _timer = 0f;
        _evadeDir = CalculateEvadeDirection();
    }

    public override void Update()
    {
        _timer += Time.deltaTime;

        bool stillOnRay = false;
        foreach(var crystal in _own.GetActiveCrystals())
        {
            if(crystal != null && crystal.IsRayActive && crystal.IsPositionOnRay(_own.transform.position))
            {
                stillOnRay = true;
                break;
            }
        }
        if (!stillOnRay || _timer >= maxEvadeTime) _superState.ReturnToIdle();
    }

    public override void FixedUpdate() => _own.rb.linearVelocity = _evadeDir * _own.evadeSpeed;

    public override void Exit() => _own.rb.linearVelocity = Vector2.zero;

    public override void OnCrystalHit(int damage) => _own.TakeDamage(damage);

    private Vector2 CalculateEvadeDirection()
    {
        foreach(var crystal in _own.GetActiveCrystals())
        {
            if (crystal == null || !crystal.IsRayActive) continue;

            Vector2 rayDir = crystal.GetRayDirection();
            Vector2 perp = new Vector2(-rayDir.y, rayDir.x);
            float sign = Random.value > 0.5f ? 1 : -1;
            return perp * sign;
        }
        return Vector2.right;
    }
}

public class IstPhaseTransitionState : IstChildBaseState
{
    private int _nextPhase;

    public IstPhaseTransitionState(IstAI own, IstAliveState supState) : base(own, supState) { }

    public override void Enter()
    {
        
        _own.rb.linearVelocity = Vector2.zero;
        _nextPhase = _own.CurrentHP <= _own.phase3Threshold ? 3 : 2;
        Debug.Log($"[IstPhaseTransitionState] переход в новую фазу: {_nextPhase}");
        if (_nextPhase == 3)
        {
            _own.EnterPhase3();
            if (GlitchEffectSystem.Instance == null) Debug.Log("GlitchEffectSystem == null!");
            GlitchEffectSystem.Instance?.PlayPhaseTransitionGlitch();
            
        }

        else _own.EnterPhase2();

        _own.StartCoroutine(TransitionCrtn());
    }

    public override void Update() { }

    private IEnumerator TransitionCrtn()
    {
        float duration = _nextPhase == 3 ? 2.5f : 1.5f;
        yield return new WaitForSecondsRealtime(duration);
        _superState.ReturnToIdle();
    }
}