using UnityEngine;
using System.Collections;

public class ArenaBossActivator : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject activatorObject;
    [SerializeField] private string hintText = "[E] Запустить симуляцию";
    [SerializeField] private float hintDuration = 2f;
    [SerializeField] private string onBossSpawnEvent = "IstActivated";
    [SerializeField] private string onBossDeathEvent = "IstDead";
    [SerializeField] private string onPlayerDeathEvent = "PlayerDied";

    private IstAI _spawnedBoss;
    private bool _isBossAlive = false;

    private void Awake()
    {
        if (activatorObject == null) activatorObject = gameObject;
        GameEventBus.Instance?.Subscribe(onBossDeathEvent, OnBossDied);
        GameEventBus.Instance?.Subscribe(onPlayerDeathEvent, OnPlayerDied);
    }
    private void OnDestroy()
    {
        GameEventBus.Instance?.Unsubscribe(onBossDeathEvent, OnBossDied);
        GameEventBus.Instance?.Unsubscribe(onPlayerDeathEvent, OnPlayerDied);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !_isBossAlive)
            UIManager.Instance?.ShowHint(hintText);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            UIManager.Instance?.HideHint();
    }

    public void Interact(PlayerController player)
    {
        if (_isBossAlive) return;
        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null) return;

        _isBossAlive = true;
        HintManager.Instance?.RemoveHintsFromSource(this);
        if (activatorObject != null) activatorObject.SetActive(false);

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        _spawnedBoss = boss.GetComponentInChildren<IstAI>();

        if (_spawnedBoss != null)  _spawnedBoss.InitializeBossUI();
        GameEventBus.Instance?.SendEvent(onBossSpawnEvent, this);
    }

    private void OnBossDied(object sender)
    {
        _isBossAlive = false;
        _spawnedBoss = null;
        UIManager.Instance?.StartCoroutine(ReactivateBoss(3f));
    }

    private void OnPlayerDied(object sender)
    {
        if (!_isBossAlive) return;
        _isBossAlive = false;
        _spawnedBoss = null;
        UIManager.Instance?.StartCoroutine(ReactivateBoss(2f));
    }

    private IEnumerator ReactivateBoss(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (activatorObject != null) activatorObject.SetActive(true);
    }
}