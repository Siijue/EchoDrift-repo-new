using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Sequester : MonoBehaviour
{
    [SerializeField] public string enemyID = "enemy_zone_01";

    [SerializeField] private int xpReward = 15;
    [SerializeField] private int echoReward = 30;

    [SerializeField] private GameObject ashPrefab;

    [SerializeField] private bool isPuzzle = false;

    private readonly List<Tendon> _aliveTendons = new List<Tendon>();

    private SpriteRenderer sprRender;
    private Collider2D coll;
    private bool isDead;


    private void Awake()
    {
        sprRender = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
    }

    public void RegisterTendon(Tendon tendon) { if (!_aliveTendons.Contains(tendon)) _aliveTendons.Add(tendon); }

    public void OnTendonDestroyed(Tendon tendon)
    {
        _aliveTendons.Remove(tendon);

        if (_aliveTendons.Count == 0) Die();
    }

    private void Die()
    {
        if(isDead) return;
        isDead = true;

        if(coll != null) coll.enabled = false;
        if(isPuzzle) GameEventBus.Instance?.SendEvent("SequesterDead", this);

        GameEventBus.Instance?.SendEvent($"Died_{enemyID}", this);

        EconomyManager.Instance?.AddEcho(echoReward);
        EconomyManager.Instance?.AddXP(xpReward);


        StartCoroutine(CollapseSequence());
    }

    private IEnumerator CollapseSequence()
    {
        float elapsed = 0f;
        const float duration = 0.8f;
        Vector3 startScale = transform.localScale;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float time = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, time);
            sprRender.color = Color.Lerp(Color.white, Color.black, time);

            yield return null;
        }

        if(ashPrefab != null) Instantiate(ashPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
