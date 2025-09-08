using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewNpcSchedule", menuName = "RPG/NPC/NPC Schedule Asset")]
public class NpcScheduleAsset : ScriptableObject
{
    [Tooltip("用于识别的NPC ID，最好和 NpcDefinition 中的 ID 保持一致")]
    public string npcId;

    [Tooltip("该NPC的所有日程安排列表")]
    public List<NpcScheduleEntry> scheduleEntries = new List<NpcScheduleEntry>();
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