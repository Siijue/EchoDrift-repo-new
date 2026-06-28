using UnityEngine;

public enum SkillEffect
{
    MaxHealthBoost,
    MaxSpeedBoost,
    EchoDropChance,
    IncreaseMechanicDuration,
}

[CreateAssetMenu(fileName = "SkillNodeSO", menuName = "Scriptable Objects/SkillNodeSO")]
public class SkillNodeSO : ScriptableObject
{
    [SerializeField] private string nodeName;
    [SerializeField] private string nodeDescription;
    [SerializeField] private Sprite nodeIcon;
    [SerializeField] private int skillPointsForOneLevel;
    [SerializeField] private int maxLevel;
    [SerializeField] private SkillNodeSO requiredSkill;
    [SerializeField] private SkillEffect effectType;
    [SerializeField] private int effectValue;

    public string NodeName => nodeName;
    public string NodeDescription => nodeDescription;
    public Sprite NodeIcon => nodeIcon;
    public int SkillPointsForOneLevel => skillPointsForOneLevel;
    public int MaxLevel => maxLevel;
    public SkillNodeSO RequieredSkill => requiredSkill;
    public SkillEffect EffectType => effectType;
    public int EffectValue => effectValue;
}
