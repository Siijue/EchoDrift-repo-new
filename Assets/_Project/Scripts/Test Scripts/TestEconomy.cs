using UnityEngine;

public class TestEconomy : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Start test");
            EconomyManager.Instance.AddXP(120);
            EconomyManager.Instance.AddEcho(17);
            EconomyManager.Instance.AddSkillPoints(5);
            Debug.Log($"[TEST] XP: {EconomyManager.Instance.CurrentXP}");
            Debug.Log($"[TEST] Level: {EconomyManager.Instance.CurrentLevel}");
            Debug.Log($"[TEST] Skill Points: {EconomyManager.Instance.SkillPoints}");
        }
    }
}
