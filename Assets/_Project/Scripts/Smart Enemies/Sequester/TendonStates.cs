using UnityEngine;

public class TendonIdleState : ITendonState
{
    private readonly Tendon _tendon;

    public TendonIdleState(Tendon tendon) => _tendon = tendon;

    public void Enter() {}

    public void Update()
    {
        if (LightSourceRegistry.IsPositionLit(_tendon.transform.position))
        {
            _tendon.TransitionTo(_tendon.GetBurningState());
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if(player != null)
        {
            float dist = Vector2.Distance(_tendon.transform.position, player.transform.position);
            if (dist <= _tendon.attackRadius) _tendon.TransitionTo(_tendon.GetWhippingState());
        }
    }

    public void Exit() { }
}


public class TendonWhippingState : ITendonState
{
    private readonly Tendon _tendon;

    private enum Phase { Attack, Rest}
    private Phase _phase;
    private float _phaseTimer;

    private bool _damageDeal;

    public TendonWhippingState(Tendon tendon) => _tendon=tendon;

    public void Enter()
    {
        _phase = Phase.Attack;
        _phaseTimer = 0f;
        _damageDeal = false;
    }

    public void Update()
    {
        if (LightSourceRegistry.IsPositionLit(_tendon.transform.position))
        {
            _tendon.TransitionTo(_tendon.GetBurningState());
            return;
        }

        _phaseTimer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Attack: HandleAttackPhase();
                break;
            case Phase.Rest: HandleRestPhase();
                break;
        }
    }

    public void Exit() { }

    private void HandleAttackPhase()
    {
        if (!_damageDeal)
        {
            DealDamage();
            _damageDeal = true;
        }

        if(_phaseTimer >= _tendon.attackDuration)
        {
            _phaseTimer = 0f;
            _phase = Phase.Rest;
        }
    }

    private void HandleRestPhase()
    {
        if(_phaseTimer >= _tendon.restDuration)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if(player != null)
            {
                float dist = Vector2.Distance(_tendon.transform.position, player.transform.position);

                if(dist <= _tendon.attackRadius)
                {
                    _phase = Phase.Attack;
                    _phaseTimer = 0f;
                    _damageDeal = false;
                    return;
                }
            }
            _tendon.TransitionTo(_tendon.GetIdleState());
        }
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(_tendon.transform.position, _tendon.attackRadius);

        foreach(var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>()?.TakeDamage(_tendon.attackDamage);

                PlayerController controller = hit.GetComponent<PlayerController>();
                if(controller != null)
                {
                    Vector2 dir = ((Vector2)hit.transform.position - (Vector2)_tendon.transform.position).normalized;
                    controller.ApplyKnockback(dir, 4f);
                }
            }
        }
    }
}

public class TendonBurningState : ITendonState
{
    private readonly Tendon _tendon;
    private float _burnTimer;

    public TendonBurningState(Tendon tendon) => _tendon = tendon;

    public void Enter()
    {
        _burnTimer = 0;
        _tendon.sprRend.color = new Color(0.6f, 0.3f, 0.1f);
    }

    public void Update()
    {
        if (!LightSourceRegistry.IsPositionLit(_tendon.transform.position)){
            _tendon.sprRend.color = Color.white;
            _tendon.TransitionTo(_tendon.GetIdleState());
            return;
        }

        _burnTimer += Time.deltaTime;

        float time = _burnTimer / _tendon.burnTime;
        _tendon.sprRend.color = Color.Lerp(new Color(0.6f, 0.3f, 0.1f), Color.black, time);

        if (_burnTimer >= _tendon.burnTime) _tendon.TransitionTo(_tendon.GetDeadState());
    }

    public void Exit() => _tendon.sprRend.color = Color.white;
}


public class TendonRetractedState : ITendonState
{
    private readonly Tendon _tendon;

    private float _duration;
    private float _timer;

    public TendonRetractedState(Tendon tendon) => _tendon = tendon;

    public void SetDuration(float duration) => _duration = duration;

    public void Enter()
    {
        _timer = 0f;
        _tendon.sprRend.enabled = false;
        _tendon.coll.enabled = false;
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _duration) _tendon.TransitionTo(_tendon.GetIdleState());
    }

    public void Exit()
    {
        _tendon.sprRend.enabled = true;
        _tendon.coll.enabled = true;
    }
}

public class TendonDeadState : ITendonState
{
    private readonly Tendon _tendon;

    public TendonDeadState(Tendon tendon) => _tendon = tendon;

    public void Enter()
    {
        _tendon.coll.enabled = false;

        _tendon.NotifySequesterOfDeath();

        _tendon.sprRend.color = Color.black;

        //Object.Destroy(_tendon.gameObject);
    }

    public void Update() { }
    public void Exit() { }

}