using UnityEngine;
using System.Collections;

public class IstDeadState : IIstState
{
    private readonly IstAI _own;

    public IstDeadState(IstAI own) => _own = own;

    public void Enter()
    {
        _own.IsDead = true;
        _own.rb.simulated = false;
        _own.GetComponent<Collider2D>().enabled = false;

        EconomyManager.Instance?.AddXP(_own.xpReward);
        EconomyManager.Instance?.AddEcho(_own.echoReward);

        _own.StartCoroutine(DeathSequence());
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
    public void OnDamaged(int newHp) { }
    public void OnCrystalHit(int dmg) { }

    private IEnumerator DeathSequence()
    {
        GlitchEffectSystem.Instance?.PlayPhaseTransitionGlitch();
        yield return new WaitForSecondsRealtime(0.5f);

        float elapsed = 0f;
        Vector3 startScale = _own.transform.localScale;

        while(elapsed > 1.5f)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / 1.5f;
            _own.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, time);
            _own.sprRender.color = Color.Lerp(Color.white, new Color(0.6f, 0.1f, 0.8f), time);
            yield return null;
        }

        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;

        _own.sprRender.enabled = true;
        _own.transform.localScale = startScale * 0.8f;
        _own.sprRender.color = new Color(0.6f, 0.1f, 0.8f, 0.5f);

        if(_own.memoryItemPrefab != null)
        {
            GameObject item = Object.Instantiate(_own.memoryItemPrefab, _own.transform.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(2f);

        if (_own.pixelPerfectCamera != null) _own.pixelPerfectCamera.assetsPPU = 32;
        GameEventBus.Instance?.SendEvent("IstDied", this);

        UIManager.Instance?.HideBossInfo();

        Object.Destroy(_own.gameObject);
    }
}

