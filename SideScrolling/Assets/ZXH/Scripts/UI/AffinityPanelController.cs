using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class AffinityPanelController : MonoBehaviour
{
    [Header("UI Prefab & Container")]
    [Tooltip("每一项的预制体，应包含 AffinityItem 脚本并绑定两个 TextMeshProUGUI")]
    public GameObject affinityItemPrefab;

    [Tooltip("Scroll View -> Content（用于放置动态生成的项）")]
    public Transform container;

    [Header("面板根，用于显示/隐藏")]
    public GameObject panelRoot;

    // 缓存
    private readonly Dictionary<string, AffinityItem> _items = new Dictionary<string, AffinityItem>();

    void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false); // 默认隐藏
    }

    void OnEnable()
    {
        if (affinityItemPrefab == null || container == null || panelRoot == null)
        {
            Debug.LogError("AffinityPanelController 未正确配置！请在 Inspector 指定 affinityItemPrefab / container / panelRoot", this);
            this.enabled = false;
            return;
        }

        // 订阅 AffinitySystem 的事件
        if (AffinitySystem.Instance != null)
        {
            if (AffinitySystem.Instance.onAffinityChanged != null)
                AffinitySystem.Instance.onAffinityChanged.Register(OnAffinityChanged);

            if (AffinitySystem.Instance.onTierChanged != null)
                AffinitySystem.Instance.onTierChanged.Register(OnTierChanged);
        }

        // 初始化面板项（从 AffinitySystem 拿当前快照）
        InitializePanel();
    }

    void OnDisable()
    {
        if (AffinitySystem.Instance != null)
        {
            if (AffinitySystem.Instance.onAffinityChanged != null)
                AffinitySystem.Instance.onAffinityChanged.Unregister(OnAffinityChanged);

            if (AffinitySystem.Instance.onTierChanged != null)
                AffinitySystem.Instance.onTierChanged.Unregister(OnTierChanged);
        }
    }

    /// <summary>
    /// 清空并重新构建列表（从 AffinitySystem.GetAllAffinities() 读取）
    /// </summary>
    public void InitializePanel()
    {
        Debug.Log("更新好感度UI");

        // 清除旧项
        foreach (Transform t in container) Destroy(t.gameObject);
        _items.Clear();

        if (AffinitySystem.Instance == null) return;

        var dict = AffinitySystem.Instance.GetAllAffinities();
        foreach (var kv in dict)
        {
            CreateOrUpdateItem(kv.Key, kv.Value.value, kv.Value.tier);
        }
    }

    /// <summary>
    /// 外部调用以刷新列表（方便 Button 按钮连线）
    /// </summary>
    public void RefreshList()
    {
        InitializePanel();
    }

    /// <summary>
    /// 当好感数变化时被 AffinitySystem 调用（通过 AffinityChangedPayload）
    /// </summary>
    private void OnAffinityChanged(AffinityChangedPayload payload)
    {
        if (payload.npc == null) return;
        string id = payload.npc.npcId;
        float after = payload.after;
        // 段位可能已经在系统中更新，优先读取系统（更一致）
        string tier = AffinitySystem.Instance != null ? AffinitySystem.Instance.GetTier(payload.npc) : CalcTierLocal(after);

        CreateOrUpdateItem(id, after, tier);
    }

    /// <summary>
    /// 当段位发生变化时被 AffinitySystem 调用（通过 TierChangedPayload）
    /// </summary>
    private void OnTierChanged(TierChangedPayload payload)
    {
        if (payload.npc == null) return;
        string id = payload.npc.npcId;
        string newTier = payload.toTier;
        float value = AffinitySystem.Instance != null ? AffinitySystem.Instance.GetAffinity(payload.npc) : 0f;
        CreateOrUpdateItem(id, value, newTier);
    }

    /// <summary>
    /// 创建或更新 UI 项
    /// </summary>
    private void CreateOrUpdateItem(string npcId, float value, string tier)
    {
        if (_items.TryGetValue(npcId, out var item))
        {
            item.Set(npcId, value, tier);
            return;
        }

        // 实例化新项
        GameObject go = Instantiate(affinityItemPrefab, container);
        go.name = $"Affinity_{npcId}";

        var affinityItem = go.GetComponent<AffinityItem>();
        if (affinityItem == null)
        {
            Debug.LogWarning("AffinityItemPrefab 上未找到 AffinityItem 脚本，请确认预制体已按说明配置。");
            // 尝试去拿 TextMeshProUGUI 的子组件并临时填充
            var texts = go.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts != null && texts.Length >= 2)
            {
                texts[0].text = npcId;
                texts[1].text = $"好感: {value:F1} | 段位: {tier}";
            }
            return;
        }

        affinityItem.Set(npcId, value, tier);
        _items[npcId] = affinityItem;
    }

    /// <summary>
    /// 切换面板显示（给按钮使用）
    /// </summary>
    public void TogglePanel()
    {
        if (panelRoot == null) return;
        bool isOn = panelRoot.activeSelf;
        panelRoot.SetActive(!isOn);
        if (!isOn) RefreshList(); // 打开时刷新
    }

    /// <summary>
    /// 当系统不可用时的本地小段位计算（备用）
    /// </summary>
    private string CalcTierLocal(float value)
    {
        // 简单实现：遍历 AffinitySystem 中配置（若存在）
        if (AffinitySystem.Instance != null)
            return AffinitySystem.Instance.GetTier(new NpcDefinition { npcId = "temp" }); // 仅占位 - 不推荐使用

        // 兜底
        if (value >= 85f) return "挚友";
        if (value >= 60f) return "亲密";
        if (value >= 30f) return "友好";
        if (value >= 0f) return "冷淡";
        return "陌生";
    }

    /// <summary>
    /// Inspector 右键测试：刷新列表
    /// </summary>
    [ContextMenu("Refresh Affinity Panel")]
    private void ContextRefresh() => RefreshList();
}
