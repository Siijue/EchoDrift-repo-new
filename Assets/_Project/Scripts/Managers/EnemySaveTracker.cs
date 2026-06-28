using UnityEngine;

public class EnemySaveTracker : MonoBehaviour
{
    [SerializeField] private string enemyID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(enemyID)) enemyID = $"{gameObject.name}_{transform.position.x: F1}_{transform.position.y: F1}";
    }

    private void Start()
    {
        CheckKilledInSave();
        GameEventBus.Instance?.Subscribe($"Died_{enemyID}", OnEnemyDied);
    }

    private void CheckKilledInSave()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null || saveData.killedEnemy == null) return;
        if (saveData.killedEnemy.Contains(enemyID)) gameObject.SetActive(false);
    }

    private void OnEnemyDied(object sender) => RegisterDeath();

    public void RegisterDeath()
    {
        SaveData saveData = SaveSystem.Load();
        if (saveData == null)
        {
            saveData = new SaveData();
            saveData.hasStartedGame = true;
        }

        if (saveData.killedEnemy == null) saveData.killedEnemy = new System.Collections.Generic.List<string>();

        if (!saveData.killedEnemy.Contains(enemyID))
        {
            saveData.killedEnemy.Add(enemyID);
            SaveSystem.Save(saveData);
        }
    }

    private void OnDestroy()
    {
        GameEventBus.Instance?.Unsubscribe($"Died_{enemyID}", OnEnemyDied);
    }
}