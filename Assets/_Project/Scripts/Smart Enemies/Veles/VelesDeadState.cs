using UnityEngine;
using System.Collections;
  

public class VelesDeadState : IVelesState
{
    private readonly VelesAI _own;

    public VelesDeadState(VelesAI own) => _own = own;

    public void Enter()
    {
        _own.IsDead = true;
        _own.rb.linearVelocity = Vector2.zero;
        _own.rb.simulated = false;

        GameEventBus.Instance?.SendEvent("VelesDead", _own);

        Collider2D coll = _own.GetComponent<Collider2D>();
        if(coll != null) coll.enabled = false;

        EconomyManager.Instance?.AddXP(_own.xpReward);
        EconomyManager.Instance?.AddEcho(_own.echoReward);

        GameObject player = GameObject.FindWithTag("Player");
        player?.GetComponent<PlayerHealth>()?.IncreaseMaxHealth(1);
        HintManager.Instance?.RegisterHint(source: _own, "МАКС. ЗДОРОВЬЕ УВЕЛИЧЕНО: +1!", 7f, 2f);

        _own.StartCoroutine(DeathSequence());
    }

    public void Update() { }
    public void FixedUpdate() { }
    public void Exit() { }
    public void OnDamaged(int newhp) { }
    public void OnCrystalHit() { }

    private IEnumerator DeathSequence()
    {
        foreach (var eye in _own.eyeRenderers)
        {
            if (eye != null) eye.color = Color.black;
            yield return new WaitForSeconds(0.3f);
        }

        float elapsed = 0f;
        float fadeDuration = 1.5f;
        Color startColor = _own.sprRender.color;
        Vector3 startPos = _own.transform.position;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / fadeDuration;

            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, time);

            currentColor = Color.Lerp(currentColor, new Color(0.3f, 0.3f, 0.5f, currentColor.a), time * 0.5f);

            _own.sprRender.color = currentColor;

            _own.transform.position = startPos + Vector3.up * time * 0.5f;

            if (time > 0.5f)
            {
                float shake = (1f - time) * 0.05f;
                _own.transform.position += new Vector3(
                    Random.Range(-shake, shake),
                    Random.Range(-shake, shake),
                    0
                );
            }

            yield return null;
        }

        UIManager.Instance?.HideBossInfo();
        Object.Destroy(_own.gameObject);
    }
}
