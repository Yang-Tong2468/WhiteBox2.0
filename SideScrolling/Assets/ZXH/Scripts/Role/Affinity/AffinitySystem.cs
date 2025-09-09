using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 好感度类型
/// </summary>
[System.Serializable]
public struct AffinityTier
{
    public string name;      // 如: 陌生/友好/亲密/崇拜/敌对
    public float minValue;   // 阈值：达到该值视为此段位
}

/// <summary>
/// NPC 好感值管理
/// </summary>
public class AffinitySystem : MonoBehaviour
{
    public static AffinitySystem Instance { get; private set; }

    [Header("段位阈值（按 minValue 升序）")]
    public List<AffinityTier> tiers = new()
    {
        new AffinityTier{ name="陌生", minValue = -9999 },
        new AffinityTier{ name="冷淡", minValue = 0 },
        new AffinityTier{ name="友好", minValue = 30 },
        new AffinityTier{ name="亲密", minValue = 60 },
        new AffinityTier{ name="挚友", minValue = 85 },
    };

    [Header("好感值上限/下限")]
    public float minAffinity = -100f;
    public float maxAffinity = 100f;

    [Header("事件（给UI/剧情使用）")]
    public GameEventAffinityChanged onAffinityChanged;//好感变化事件
    public GameEventAffinityTierChanged onTierChanged;//好感类型变化事件

    // 内部存储：每个 NPC 的好感值
    private readonly Dictionary<string, float> _affinity = new();//存储NPC的好感度
    private readonly Dictionary<string, string> _currentTier = new();//好感类型

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetAffinity(NpcDefinition npc)
    {
        if (npc == null) return 0;
        return _affinity.TryGetValue(npc.npcId, out var v) ? v : 0f;
    }

    public string GetTier(NpcDefinition npc)
    {
        if (npc == null) return "";
        return _currentTier.TryGetValue(npc.npcId, out var t) ? t : CalcTier(GetAffinity(npc)).name;
    }

    /// <summary>
    /// 应用好感度规则，计算更新 NPC 好感度
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="npc"></param>
    /// <param name="rule"></param>
    public void ApplyRule(CharacterStats actor, NpcDefinition npc, AffinityRule rule)
    {
        if (npc == null || rule == null) return;

        var before = GetAffinity(npc);
        var delta = Mathf.Clamp(rule.Evaluate(actor), minAffinity, maxAffinity);
        var after = Mathf.Clamp(before + delta, minAffinity, maxAffinity);

        _affinity[npc.npcId] = after;
        onAffinityChanged?.Raise(npc, before, after);

        var oldTier = GetTier(npc);
        var newTier = CalcTier(after).name;
        _currentTier[npc.npcId] = newTier;

        if (oldTier != newTier)
            onTierChanged?.Raise(npc, oldTier, newTier);
    }

    /// <summary>
    /// 设置/更新 NPC 好感度
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="value"></param>
    /// <param name="fireEvents"></param>
    public void SetAffinity(NpcDefinition npc, float value, bool fireEvents = true)
    {
        if (npc == null) return;

        var before = GetAffinity(npc);
        var after = Mathf.Clamp(value, minAffinity, maxAffinity);
        _affinity[npc.npcId] = after;

        if (fireEvents) onAffinityChanged?.Raise(npc, before, after);

        var oldTier = GetTier(npc);
        var newTier = CalcTier(after).name;
        _currentTier[npc.npcId] = newTier;

        if (fireEvents && oldTier != newTier)
            onTierChanged?.Raise(npc, oldTier, newTier);
    }

    /// <summary>
    /// 计算好感类型
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private AffinityTier CalcTier(float value)
    {
        AffinityTier last = tiers[0];
        foreach (var t in tiers)
        {
            if (value >= t.minValue) last = t;
            else break;
        }
        return last;
    }
}
