using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NpcRegistry", menuName = "RPG/NPC/NPC Registry")]
public class NpcRegistry : ScriptableObject
{
    public List<NpcDefinition> npcMappings;

    /// <summary>
    /// 根据ID获取NPC预制体
    /// </summary>
    /// <param name="npcId"></param>
    /// <returns></returns>
    public GameObject GetPrefabById(string npcId)
    {
        foreach (var mapping in npcMappings)
        {
            if (mapping.npcId == npcId)
            {
                return mapping.NpcPrefab;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据ID获取NPC定义
    /// </summary>
    public NpcDefinition GetNpcById(string npcId)
    {
        foreach (var mapping in npcMappings)
        {
            if (mapping.npcId == npcId)
            {
                return mapping;
            }
        }
        return null;
    }
}