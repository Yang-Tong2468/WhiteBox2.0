using UnityEngine;

public enum AffinityEventType { Gift, Help, Harm, Talk, QuestComplete }

/// <summary>
/// 好感度变化规则
/// </summary>
[CreateAssetMenu(menuName = "RPG/Affinity/Rule", fileName = "AffinityRule_")]
public class AffinityRule : ScriptableObject
{
    public AffinityEventType eventType;
    [Tooltip("基础好感变化")]
    public float baseDelta = 60f;//初始值

    [Tooltip("魅力加成：delta += charisma * charismaFactor")]
    public AttributeDefinition charismaDef;
    public float charismaFactor = 0.0f;//影响系数

    /// <summary>
    /// 计算好感度值
    /// </summary>
    /// <param name="actorStats"></param>
    /// <returns></returns>
    public float Evaluate(CharacterStats actorStats)
    {
        float delta = baseDelta;
        if (actorStats != null && charismaDef != null)
            delta += actorStats.GetFinal(charismaDef) * charismaFactor;
        return delta;
    }
}
