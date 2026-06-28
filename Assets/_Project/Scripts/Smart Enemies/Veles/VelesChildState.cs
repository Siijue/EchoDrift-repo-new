using UnityEngine;

public class VelesIdleState : IVelesState
{
    private readonly VelesAI _own;
    private readonly VelesAliveState _superState;

    private float _idleTimer;

    public VelesIdleState(VelesAI own, VelesAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter() => _idleTimer = 0f;

    public void Update()
    {
        _idleTimer += Time.deltaTime;

        _own.AbilityQueue.Tick(Time.deltaTime);

        if (_idleTimer < _own.minTimeBetweenAbilities) return;

        if (!_own.IsPlayerAlive() || !_own.IsPlayerInRange()) return;

        BossAbility best = _own.AbilityQueue.SelectBest();
        if (best == null) return;

        Debug.Log($"(VelesIdleState) ВЫБРАНА СПОСОБНОСТЬ: {best}");

        if(best is GazeOfAbyssAbility)
        {
            best.TriggerAndCooldown();
            _superState.StartGaze();
            return;
        }

        _superState.StartCasting((VelesAbility)best);
    }

    public void FixedUpdate() { }
    public void Exit() { }
    public void OnDamaged(int newHp) { }
    public void OnCrystalHit() { }
}

public class VelesCastingState : IVelesState
{
    private readonly VelesAI _own;
    private readonly VelesAliveState _superState;

    private VelesAbility _ability;
    private float _castTimer;
    private bool _interrupted;

    private const float DefaultCastDuration = 0.6f;

    public VelesCastingState(VelesAI own, VelesAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void SetAbility(VelesAbility ability) => _ability = ability;

    public void Enter()
    {
        _castTimer = 0f;
        _interrupted = false;

        _own.FlashEyes(_own.eyeFlashColor);
        Debug.Log($"(VelesCastingState) КАСТ СПОСОБНОСТИ: {_ability?.Name}");
    }

    public void Update()
    {
        if (_interrupted) return;

        if(!_own.IsPlayerAlive() || !_own.IsPlayerInRange())
        {
            Interrupt();
            return;
        }

        _castTimer += Time.deltaTime;

        float castDuration = _ability is ExtinguishAbility ? _own.extinguishCastDuration : DefaultCastDuration;

        if(_castTimer >= castDuration)
        {
            _ability?.TriggerAndCooldown();
            _superState.ReturnToIdle();
        }
    }

    public void Interrupt()
    {
        _interrupted = true;
        Debug.Log($"(VelesCastingState) КАСТ ПРЕРВАН");
        _superState.ReturnToIdle();
    }

    public void FixedUpdate() { }
    public void Exit() { _own.UpdateEyes(); }
    public void OnDamaged(int newhp) { }
    public void OnCrystalHit() { }
}

public class VelesStunnedState : IVelesState
{
    private readonly VelesAI _own;
    private readonly VelesAliveState _superState;
    private float _timer;

    public VelesStunnedState(VelesAI own, VelesAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter()
    {
        _timer = 0f;
        _own.rb.linearVelocity = Vector2.zero;
        Debug.Log($"(VelesStunnedState) СТАН НА {_own.stunDuration}");
    }

    public void Update()
    {
        _timer += Time.deltaTime;

        float time = _timer / _own.stunDuration;
        _own.sprRender.color = Color.Lerp(Color.lightGreen * 1.5f, Color.white, time);

        if(_timer >= _own.stunDuration)
        {
            _own.sprRender.color = Color.white;
            _superState.ReturnToIdle();
        }
    }

    public void FixedUpdate() { }
    public void Exit() { _own.sprRender.color = Color.white; }
    public void OnDamaged(int newhp) { }
    public void OnCrystalHit() { }
}

public class VelesGazeState : IVelesState
{
    private readonly VelesAI _own;
    private readonly VelesAliveState _superState;
    private float _timer;

    public VelesGazeState(VelesAI own, VelesAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public void Enter()
    {
        _timer = 0f;
        _own.rb.linearVelocity = Vector2.zero;

        _own.FlashEyes(Color.white);
        Debug.Log($"(VelesGazeState) УЛЬТИМЕЙТ");
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if (!_own.IsPlayerAlive())
        {
            _own.UpdateEyes();
            _superState.ReturnToIdle();
            return;
        }
        if(_timer >= _own.gazeDuration)
        {
            _own.UpdateEyes();
            _superState.ReturnToIdle();
        }
    }

    public void FixedUpdate() { }
    public void Exit() { _own.UpdateEyes(); }
    public void OnDamaged(int newhp) { }
    public void OnCrystalHit() { _own.TakeDamage(_own.crystalDamage); }
}

public class VelesPhaseTransitionState : IVelesState
{
    private readonly VelesAI _own;
    private readonly VelesAliveState _superState;
    private float _timer;
    private const float Duration = 4f;

    public VelesPhaseTransitionState(VelesAI own, VelesAliveState superState)
    {
        _own = own;
        _superState=superState;
    }

    public void Enter()
    {
        _timer = 0f;

        _own.GetComponent<SpriteRenderer>().sprite = _own.sprRenderPhase2.sprite;

        _own.rb.linearVelocity = Vector2.zero;
        _own.transform.position += Vector3.up * 5f;

        _own.FlashEyes(_own.eyeFlashColor);
        _own.EnterPhase2();

        Debug.Log($"(VelesPhaseTransitionState) ПЕРЕХОД В ФАЗУ 2");
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        float time = Mathf.PingPong(_timer * 3f, 1f);
        _own.FlashEyes(Color.Lerp(_own.eyeNormalColor, _own.eyePhase2Color, time));

        if(_timer >= Duration)
        {
            _own.UpdateEyes();
            _superState.ReturnToIdle();
        }
    }

    public void FixedUpdate() { }
    public void Exit() { _own.UpdateEyes(); }
    public void OnDamaged(int newhp) { }
    public void OnCrystalHit() { }
}