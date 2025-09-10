using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG/NPC/NPC Definition", fileName = "NPC_")]
public class NpcDefinition : ScriptableObject
{
    public string npcId;         // 唯一ID
    public string displayName;   // 显示名
    public List<NpcScheduleEntry> scheduleEntries = new List<NpcScheduleEntry>();
    public GameObject NpcPrefab;
    public string CurrentSceneName;
    public string CurrentMarkerName = "Marker";
}
