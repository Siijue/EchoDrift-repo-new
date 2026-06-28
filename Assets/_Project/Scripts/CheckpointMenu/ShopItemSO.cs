using UnityEngine;

public enum ShopEffect
{
    LightExtension,
    DamageReduction,
    Invisibility,
    EchoBonus,
    AutoRevive
}

[CreateAssetMenu(fileName = "ShopItemSO", menuName = "Scriptable Objects/ShopItemSO")]

public class ShopItemSO : ScriptableObject
{
    [SerializeField] private string itemName = "Новый товар";
    [SerializeField] private string description = "Описание эффекта";
    [SerializeField] private int price = 10;
    [SerializeField] private Sprite icon;

    [SerializeField] private ShopEffect effectType;
    [SerializeField] private float effectValue = 10f;
    [SerializeField] private int maxStacks = 1;


    public string ItemName => itemName;
    public string Description => description;
    public int Price => price;
    public Sprite Icon => icon;
    public ShopEffect EffectType => effectType;
    public float EffectValue => effectValue;
    public int MaxStacks => maxStacks;

}
