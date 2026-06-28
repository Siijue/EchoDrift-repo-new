public abstract class IstChildBaseState : IIstState
{
    protected readonly IstAI _own;
    protected readonly IstAliveState _superState;

    protected IstChildBaseState(IstAI own, IstAliveState superState)
    {
        _own = own;
        _superState = superState;
    }

    public abstract void Enter();
    public abstract void Update();

    public virtual void FixedUpdate() { }
    public virtual void Exit() { }
    public virtual void OnDamaged(int newHp) { }
    public virtual void OnCrystalHit(int dmg) { }
}
