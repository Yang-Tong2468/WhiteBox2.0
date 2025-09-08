using UnityEngine;
using System.Collections.Generic;
using Slax.Schedule;

[RequireComponent(typeof(NpcMovementController))]
public class NpcScheduler : MonoBehaviour
{
    [Tooltip("分配给这个NPC的日程表资源文件")]
    public NpcScheduleAsset scheduleAsset;

    [Tooltip("这个NPC的唯一ID，必须和日程表及注册表中的ID一致")]
    public string npcId;

    private Dictionary<string, NpcScheduleEntry> scheduleMap;

    void Awake()
    {
        InitializeScheduleMap();
        npcId = GetComponent<NPC>().npcDefinition.npcId;
    }

    void OnEnable()
    {
        // 订阅 ScheduleManager 的全局事件
        ScheduleManager.OnScheduleEvents += HandleScheduledEvents;
    }

    void OnDisable()
    {
        ScheduleManager.OnScheduleEvents -= HandleScheduledEvents;
    }

    /// <summary>
    /// 将列表转换为字典，方便快速查找
    /// </summary>
    private void InitializeScheduleMap()
    {
        scheduleMap = new Dictionary<string, NpcScheduleEntry>();
        if (scheduleAsset == null)
        {
            Debug.LogWarning($"NPC {gameObject.name} 没有分配日程表资源(NpcScheduleAsset)！");
            return;
        }

        foreach (var entry in scheduleAsset.scheduleEntries)
        {
            if (!scheduleMap.ContainsKey(entry.scheduleEventID))
            {
                scheduleMap.Add(entry.scheduleEventID, entry);
            }
            else
            {
                Debug.LogWarning($"日程表 '{scheduleAsset.name}' 中有重复的事件ID: {entry.scheduleEventID}");
            }
        }
    }

    /// <summary>
    /// 处理对应的事件——当 ScheduleManager 广播事件时，这个函数会被调用
    /// </summary>
    /// <param name="events"></param>
    private void HandleScheduledEvents(List<ScheduleEvent> events)
    {
        Debug.Log($"NPC {npcId} 正在处理当前Tick的事件...");

        // 遍历当前Tick发生的所有事件
        foreach (var evt in events)
        {
            Debug.Log($"NPC {npcId} 收到事件: {evt.ID}");
            if (scheduleMap.TryGetValue(evt.ID, out NpcScheduleEntry matchedEntry))
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                Debug.Log($"NPC {npcId} 匹配到事件: {matchedEntry.scheduleEventID}，目标场景: {matchedEntry.targetSceneName}，目标标记点: {matchedEntry.targetMarkerName}");
                // 检查这个日程是场景内移动还是跨场景移动
                if (string.IsNullOrEmpty(matchedEntry.targetSceneName) || matchedEntry.targetSceneName == currentScene)
                {
                    // 场景内移动：查找并移动
                    // (可以调用一个简单的移动组件来处理，或者直接传送)
                    GameObject marker = GameObject.Find(matchedEntry.targetMarkerName);
                    if (marker != null) transform.position = marker.transform.position;
                }
                else
                {
                    // 跨场景移动：更新全局状态，然后销毁自己
                    GlobalNpcManager.Instance.UpdateNpcState(npcId, matchedEntry.targetSceneName, matchedEntry.targetMarkerName);
                    Destroy(gameObject); // 或者使用对象池回收
                }
                break;
            }
        }
    }
}