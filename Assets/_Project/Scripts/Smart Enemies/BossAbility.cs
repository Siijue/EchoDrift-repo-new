using UnityEngine;

public abstract class BossAbility
{
    public string Name { get; }
    public float Cooldown {  get; }
    public int Priority { get; set; }

    private float _cooldownTimer;

    public bool IsReady => _cooldownTimer <= 0f;

    protected BossAbility(string name, float cooldown, int priority)
    {
        Name = name;
        Cooldown = cooldown;
        Priority = priority;
        _cooldownTimer = 0f;
    }

    public void Tick(float deltaTime)
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= deltaTime;
    }

    public void TriggerAndCooldown()
    {
        Execute();
        _cooldownTimer = Cooldown;
    }

    public void ResetCooldown() => _cooldownTimer = 0f;

    public void SetCooldown(float time) => _cooldownTimer = time;

    protected abstract void Execute();

    public virtual bool CanUse() => IsReady;
}
