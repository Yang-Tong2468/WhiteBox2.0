using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class NpcPrefabMapping
{
    public string NpcId;
    public GameObject NpcPrefab;
}

[CreateAssetMenu(fileName = "NpcRegistry", menuName = "RPG/NPC/NPC Registry")]
public class NpcRegistry : ScriptableObject
{
    public List<NpcPrefabMapping> npcMappings;

    /// <summary>
    /// 根据ID获取NPC预制体
    /// </summary>
    /// <param name="npcId"></param>
    /// <returns></returns>
    public GameObject GetPrefabById(string npcId)
    {
        foreach (var mapping in npcMappings)
        {
            if (mapping.NpcId == npcId)
            {
                return mapping.NpcPrefab;
            }
        }
        return null;
    }
}