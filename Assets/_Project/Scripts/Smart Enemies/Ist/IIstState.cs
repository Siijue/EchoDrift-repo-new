public interface IIstState
{
    void Enter();
    void Update();
    void FixedUpdate();
    void Exit();
    void OnDamaged(int newHP);
    void OnCrystalHit(int damage);
}
