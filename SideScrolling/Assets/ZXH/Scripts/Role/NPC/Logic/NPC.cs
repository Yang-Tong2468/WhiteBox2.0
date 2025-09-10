using Slax.Schedule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 引入场景管理

public class NPC : MonoBehaviour
{
    [Header("NPC核心数据")]
    public NpcDefinition npcDefinition;
    public AffinityRule[] customAffinityRules;

    [Header("视觉表现设置")]
    public float moveDistance = 5f;    // 移动的距离
    public float transitionDuration = 2f; // 渐变和移动的总时长

    // 内部组件和状态
    public CharacterStats stats;
    private Renderer objectRenderer; //使用通用的 Renderer 来处理3D对象
    private Color startColor;
    private Dictionary<string, NpcScheduleEntry> scheduleMap;

    #region Unity生命周期
    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // 确保材质可以被独立修改，不会影响到其他使用相同材质的对象
            objectRenderer.material = new Material(objectRenderer.material);
            startColor = objectRenderer.material.color;
        }
        else
        {
            Debug.LogError($"NPC {gameObject.name} 缺少 Renderer 组件，无法执行视觉效果！");
        }

        InitializeScheduleMap();
    }

    void Start()
    {
        stats = FindObjectOfType<CharacterStats>();
    }

    void OnEnable()
    {
        // 订阅日程事件
        ScheduleManager.OnScheduleEvents += HandleScheduledEvents;
        // 让对象重新可见
        StartCoroutine(FadeIn());
    }

    void OnDisable()
    {
        // 取消订阅
        ScheduleManager.OnScheduleEvents -= HandleScheduledEvents;
    }
    #endregion

    #region 日程处理
    private void InitializeScheduleMap()
    {
        scheduleMap = new Dictionary<string, NpcScheduleEntry>();
        if (npcDefinition.scheduleEntries == null)
        {
            Debug.LogWarning($"NPC {gameObject.name} 没有分配日程表资源(NpcScheduleAsset)！");
            return;
        }

        foreach (var entry in npcDefinition.scheduleEntries)
        {
            if (!scheduleMap.ContainsKey(entry.scheduleEventID))
            {
                scheduleMap.Add(entry.scheduleEventID, entry);
            }
        }
    }

    private void HandleScheduledEvents(List<ScheduleEvent> events)
    {
        foreach (var evt in events)
        {
            Debug.Log($"NPC {npcDefinition.npcId} 正在处理当前Tick的事件...");
            if (scheduleMap.TryGetValue(evt.ID, out NpcScheduleEntry matchedEntry))
            {
                Debug.Log($"NPC {npcDefinition.npcId} 匹配到日程事件: {matchedEntry.scheduleEventID}");
                string currentScene = SceneManager.GetActiveScene().name;

                if (string.IsNullOrEmpty(matchedEntry.targetSceneName) || matchedEntry.targetSceneName == currentScene)
                {
                    // 场景内移动
                    StartCoroutine(MoveToMarkerInScene(matchedEntry.targetMarkerName));
                }
                else
                {
                    // 跨场景移动
                    StartCoroutine(LeaveSceneRoutine(matchedEntry));
                }
                break;
            }
        }
    }
    #endregion

    #region 视觉与移动协程
    /// <summary>
    /// 移动到场景内的某个标记点
    /// </summary>
    private IEnumerator MoveToMarkerInScene(string markerName)
    {
        GameObject marker = GameObject.Find(markerName);
        if (marker != null)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = marker.transform.position;

            //执行移动和渐变（这里可以根据需要选择是否渐变）
            yield return FadeOutAndMove(startPos, targetPos, false); // 移动但不销毁
            transform.position = targetPos; // 确保位置
            yield return FadeIn();
        }
        else
        {
            Debug.LogError($"在场景中找不到标记点 '{markerName}'！");
        }
    }

    /// <summary>
    /// 离开当前场景的完整流程
    /// </summary>
    private IEnumerator LeaveSceneRoutine(NpcScheduleEntry destination)
    {
        //执行向右移动并渐隐消失的动画
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.right * moveDistance;
        yield return FadeOutAndMove(startPos, targetPos, true); // 移动并准备销毁

        // 更新全局状态并销毁自己
        GlobalNpcManager.Instance.UpdateNpcState(npcDefinition.npcId, destination.targetSceneName, destination.targetMarkerName);
        Destroy(gameObject);
    }

    /// <summary>
    /// 核心协程：向目标位置移动，同时渐隐，并决定完成后是否禁用对象
    /// </summary>
    private IEnumerator FadeOutAndMove(Vector3 startPosition, Vector3 targetPosition, bool disableOnComplete)
    {
        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (objectRenderer != null)
            {
                objectRenderer.material.color = Color.Lerp(startColor, Color.clear, t);
            }
            yield return null;
        }

        // 确保最终状态
        transform.position = targetPosition;
        if (disableOnComplete && objectRenderer != null)
        {
            objectRenderer.material.color = Color.clear;
        }
    }

    /// <summary>
    /// 渐现协程：当NPC被生成或重新激活时调用
    /// </summary>
    public IEnumerator FadeIn()
    {
        if (objectRenderer == null) yield break;

        // 确保开始时是透明的
        objectRenderer.material.color = Color.clear;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            objectRenderer.material.color = Color.Lerp(Color.clear, startColor, t);
            yield return null;
        }

        // 确保最终状态
        objectRenderer.material.color = startColor;
    }
    #endregion

    #region 其他功能
    [ContextMenu("更新好感度")]
    public void UpdateAffinity()
    {
        if (AffinitySystem.Instance == null) return;
        foreach (var rule in customAffinityRules)
        {
            AffinitySystem.Instance.ApplyRule(stats, npcDefinition, rule);
        }
        Debug.Log($"NPC {npcDefinition.npcId} 好感度:{AffinitySystem.Instance.GetAffinity(npcDefinition)}");
    }
    #endregion
}

[System.Serializable]
public class NpcScheduleEntry
{
    [Tooltip("与 Schedule Master 中事件的ID完全匹配")]
    public string scheduleEventID;

    [Tooltip("移动的目标场景名称。如果留空，则表示在当前场景移动。")]
    public string targetSceneName;

    [Tooltip("目标场景中一个空对象(Marker)的名称，NPC将移动到该对象的位置")]
    public string targetMarkerName;
}