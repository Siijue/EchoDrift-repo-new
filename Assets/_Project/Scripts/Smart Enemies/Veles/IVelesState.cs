public interface IVelesState
{
    void Enter();
    void Update();
    void FixedUpdate();
    void Exit();
    void OnDamaged(int newHP);
    void OnCrystalHit();
}