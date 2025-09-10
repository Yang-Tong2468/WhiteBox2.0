using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 好感度段位
/// </summary>
[System.Serializable]
public struct AffinityTier
{
    public string name;      // 如: 陌生/友好/亲密/崇拜/敌对
    public float minValue;   // 阈值：达到该值视为此段位（请按升序填写）
}

/// <summary>
/// 打包好感数据（存储在字典里）
/// </ summary >
[System.Serializable]
public struct AffinityData
{
    public float value;
    public string tier;
}

/// <summary>
/// NPC 好感值管理（单例）
/// </summary>
public class AffinitySystem : MonoBehaviour
{
    public static AffinitySystem Instance { get; private set; }

    [Header("初始化设置")]
    [Tooltip("新注册/初始化时的默认好感度（默认 50）")]
    public float defaultAffinity = 5f;

    public NpcRegistry allNPC;

    [Tooltip("是否在 Awake 时自动搜寻场景中带 NpcDefinition 的对象并初始化")]
    public bool autoRegisterSceneNpcs = false;

    [Header("段位阈值（按 minValue 升序）")]
    public List<AffinityTier> tiers = new List<AffinityTier>()
    {
        new AffinityTier{ name="陌生", minValue = -9999f },
        new AffinityTier{ name="冷漠", minValue = 0f },
        new AffinityTier{ name="友好", minValue = 30f },
        new AffinityTier{ name="亲密", minValue = 60f },
        new AffinityTier{ name="挚友", minValue = 85f },
    };

    [Header("好感值上限/下限")]
    public float minAffinity = -100f;
    public float maxAffinity = 100f;

    [Header("事件（给UI/剧情使用，保持你原来定义的 GameEvent 类型）")]
    public GameEventAffinityChanged onAffinityChanged;      // 好感变化事件 (npc, before, after)
    public GameEventAffinityTierChanged onTierChanged;     // 好感段位变化事件 (npc, oldTier, newTier)

    //所有已注册 NPC 的好感数据
    private readonly Dictionary<string, AffinityData> _affinities = new Dictionary<string, AffinityData>();

    // 在代码其它地方仍使用简单的获取方法
    private readonly Dictionary<string, float> _affinityRaw = new Dictionary<string, float>();
    private readonly Dictionary<string, string> _currentTier = new Dictionary<string, string>();

    private void Awake()
    {
        // 单例管理
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 自动注册场景 NPC
            if (autoRegisterSceneNpcs)
            {
                var defs = FindObjectsOfType<NpcDefinition>();
                List<NpcDefinition> list = new List<NpcDefinition>(defs.Length);
                foreach (var d in defs) list.Add(d);
                InitializeAll(list);
            }
        }
        else
        {
            Destroy(gameObject);
        }


        InitializeAll(allNPC.npcMappings);
    }

    #region 注册 / 初始化 API

    /// <summary>
    /// 为单个 NPC 注册并设置初始好感（如果已经注册则不会覆盖，除非 force=true）
    /// </summary>
    public void RegisterNpc(NpcDefinition npc, bool force = false)
    {
        if (npc == null) return;
        if (_affinities.ContainsKey(npc.npcId) && !force) return;

        float startVal = Mathf.Clamp(defaultAffinity, minAffinity, maxAffinity);
        string startTier = CalcTier(startVal).name;

        var data = new AffinityData { value = startVal, tier = startTier };
        _affinities[npc.npcId] = data;
        _affinityRaw[npc.npcId] = startVal;
        _currentTier[npc.npcId] = startTier;
    }

    /// <summary>
    /// 注销 NPC（移除记录）
    /// </summary>
    public void UnregisterNpc(NpcDefinition npc)
    {
        if (npc == null) return;
        _affinities.Remove(npc.npcId);
        _affinityRaw.Remove(npc.npcId);
        _currentTier.Remove(npc.npcId);
    }

    /// <summary>
    /// 批量初始化（会调用 RegisterNpc for each; force=true 表示覆盖已有值）
    /// </summary>
    public void InitializeAll(IEnumerable<NpcDefinition> npcs, bool force = false)
    {
        if (npcs == null) return;

        Debug.Log("初始化所有npc好感度");

        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            RegisterNpc(npc, force);
        }
    }

    /// <summary>
    /// 初始化场景中所有 NpcDefinition
    /// </summary>
    [ContextMenu("Initialize All Scene NPCs (Affinity)")]
    private void ContextInitializeAllSceneNpcs()
    {
        var defs = FindObjectsOfType<NpcDefinition>();
        List<NpcDefinition> list = new List<NpcDefinition>(defs.Length);
        foreach (var d in defs) list.Add(d);
        InitializeAll(list, true);
        Debug.Log($"AffinitySystem: Initialized {list.Count} scene NPC(s) with defaultAffinity={defaultAffinity}");
    }

    #endregion

    #region 查询 API

    /// <summary>
    /// 获取某 NPC 的数值（若未注册则返回 defaultAffinity）
    /// </summary>
    public float GetAffinity(NpcDefinition npc)
    {
        if (npc == null) return 0f;
        if (_affinities.TryGetValue(npc.npcId, out var d)) return d.value;
        // 未注册返回默认值（并不自动注册）
        return Mathf.Clamp(defaultAffinity, minAffinity, maxAffinity);
    }

    /// <summary>
    /// 获取某 NPC 的段位（若未注册则根据 defaultAffinity 计算并返回）
    /// </summary>
    public string GetTier(NpcDefinition npc)
    {
        if (npc == null) return "";
        if (_affinities.TryGetValue(npc.npcId, out var d)) return d.tier;
        return CalcTier(Mathf.Clamp(defaultAffinity, minAffinity, maxAffinity)).name;
    }

    /// <summary>
    /// 返回一个拷贝，供 UI 使用（防止外部误修改内部字典）
    /// </summary>
    public Dictionary<string, AffinityData> GetAllAffinities()
    {
        return new Dictionary<string, AffinityData>(_affinities);
    }

    #endregion

    #region 修改 API（Set / ApplyRule）

    /// <summary>
    /// 设置/更新 NPC 好感度（会触发事件与段位变化事件）
    /// </summary>
    public void SetAffinity(NpcDefinition npc, float value, bool fireEvents = true)
    {
        if (npc == null) return;

        // 确保已注册（方便统一管理）
        if (!_affinities.ContainsKey(npc.npcId))
        {
            RegisterNpc(npc, true); // 使用默认值再覆盖
        }

        var before = _affinities[npc.npcId].value;
        var after = Mathf.Clamp(value, minAffinity, maxAffinity);

        // 写入内部数据结构
        var newTier = CalcTier(after).name;
        _affinities[npc.npcId] = new AffinityData { value = after, tier = newTier };
        _affinityRaw[npc.npcId] = after;

        // 触发事件
        if (fireEvents)
        {
            onAffinityChanged?.Raise(npc, before, after);
            var oldTier = _currentTier.ContainsKey(npc.npcId) ? _currentTier[npc.npcId] : CalcTier(before).name;
            if (oldTier != newTier)
            {
                onTierChanged?.Raise(npc, oldTier, newTier);
            }
        }

        // 更新当前 tier 缓存
        _currentTier[npc.npcId] = newTier;
    }

    /// <summary>
    /// 应用好感度规则（例如：基于 actor 的某个行为计算 delta），并更新
    /// AffinityRule.Evaluate(actor) 应返回“变化量 delta”（可正可负）
    /// </summary>
    public void ApplyRule(CharacterStats actor, NpcDefinition npc, AffinityRule rule)
    {
        if (npc == null || rule == null) return;

        if (!_affinities.ContainsKey(npc.npcId)) RegisterNpc(npc, true);

        var before = _affinities[npc.npcId].value;

        // 先得到 delta（规则返回的应该是 delta），然后计算 after
        float delta = rule.Evaluate(actor);
        float after = Mathf.Clamp(before + delta, minAffinity, maxAffinity);

        // 写入
        var newTier = CalcTier(after).name;
        _affinities[npc.npcId] = new AffinityData { value = after, tier = newTier };
        _affinityRaw[npc.npcId] = after;

        // 事件
        onAffinityChanged?.Raise(npc, before, after);

        var oldTier = _currentTier.ContainsKey(npc.npcId) ? _currentTier[npc.npcId] : CalcTier(before).name;
        if (oldTier != newTier)
        {
            onTierChanged?.Raise(npc, oldTier, newTier);
        }
        _currentTier[npc.npcId] = newTier;
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 计算好感段位（会遍历 tiers，按 minValue 升序匹配）
    /// </summary>
    private AffinityTier CalcTier(float value)
    {
        // 防护：确保 tiers 至少有一个项
        if (tiers == null || tiers.Count == 0)
            return new AffinityTier { name = "Unknown", minValue = float.MinValue };

        AffinityTier result = tiers[0];
        foreach (var t in tiers)
        {
            if (value >= t.minValue) result = t;
            else break;
        }
        return result;
    }

    /// <summary>
    /// 在 Inspector 中确保 tiers 按 minValue 升序（可手动调用）
    /// </summary>
    [ContextMenu("Sort Tiers By minValue (ascending)")]
    private void SortTiers()
    {
        tiers.Sort((a, b) => a.minValue.CompareTo(b.minValue));
        Debug.Log("AffinitySystem: tiers sorted by minValue ascending");
    }

    #endregion
}
