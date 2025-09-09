using UnityEngine;

public enum StatModType { Flat, PercentAdd, PercentMult }

[CreateAssetMenu(menuName = "RPG/Stats/Stat Modifier", fileName = "Mod_")]
public class StatModifier : ScriptableObject
{
    public AttributeDefinition target;
    public StatModType type = StatModType.Flat;
    public float value = 0f;
    public int order = 100; // 排序，先加法再乘法方便预测

    public float Apply(float current)
    {
        return type switch
        {
            StatModType.Flat => current + value,
            StatModType.PercentAdd => current * (1f + value),
            StatModType.PercentMult => current * value,
            _ => current
        };
    }
}
