using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [SerializeField] private SkillNodeSO[] allSkills;

    private Dictionary<SkillNodeSO, int> skillProgress = new Dictionary<SkillNodeSO, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (SkillNodeSO skill in allSkills)
        {
            skillProgress[skill] = 0;
        }
    }

    public void LoadFromSaveData(SaveData data)
    {
        if (data == null || data.skillProgress == null) return;

        foreach (var kvp in data.skillProgress)
        {
            SkillNodeSO skill = System.Array.Find(allSkills, s => s.name == kvp.Key);
            if (skill != null)
            {
                skillProgress[skill] = kvp.Value;
            }
        }

        Debug.Log($"[SkillManager] Загружено {data.skillProgress.Count} навыков");
    }

    public void SaveToSaveData(SaveData data)
    {
        if (data == null) return;

        data.skillProgress = new Dictionary<string, int>();
        foreach (var kvp in skillProgress)
        {
            if (kvp.Key != null)
            {
                data.skillProgress[kvp.Key.name] = kvp.Value;
            }
        }
    }

    public int GetSkillLevel(SkillNodeSO skill)
    {
        if (skill == null) return 0;
        skillProgress.TryGetValue(skill, out int level);
        return level;
    }

    public bool UpgradeSkill(SkillNodeSO skill)
    {
        if (skill == null) return false;
        if (GetSkillLevel(skill) >= skill.MaxLevel) return false;

        skillProgress[skill]++;
        return true;
    }
}