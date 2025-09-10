using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class GlobalNpcManager : MonoBehaviour
{
    public static GlobalNpcManager Instance { get; private set; }
    [Tooltip("所有NPC资源")]
    public NpcRegistry npcRegistry;
    // 使用字典存储所有NPC的状态，通过NPC的唯一ID来查找
    private Dictionary<string, NpcDefinition> _npcStates = new Dictionary<string, NpcDefinition>();

    void Awake()
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

        InitAllNPCStates();
    }

    /// <summary>
    /// 初始化——把所有角色的状态都初始化到字典中
    /// </summary>
    public void InitAllNPCStates()
    {
        foreach(var npc in npcRegistry.npcMappings)
        {
            if (!_npcStates.ContainsKey(npc.npcId))
            {
                _npcStates.Add(npc.npcId, new NpcDefinition
                {
                    npcId = npc.npcId,
                    displayName = npc.displayName,
                    NpcPrefab = npc.NpcPrefab,
                    CurrentSceneName = npc.CurrentSceneName,
                    CurrentMarkerName = npc.CurrentMarkerName
                });
            }
        }
    }

    /// <summary>
    /// NPC调用此方法来更新自己的状态
    /// </summary>
    public void UpdateNpcState(string npcId, string newSceneName, string newMarkerName)
    {
        if (_npcStates.ContainsKey(npcId))
        {
            _npcStates[npcId].CurrentSceneName = newSceneName;
            _npcStates[npcId].CurrentMarkerName = newMarkerName;
        }
        else
        {
            Debug.Log("bug: NPC状态不存在，无法更新。请确保NPC已正确注册。");  
            //_npcStates.Add(npcId, new NpcDefinition { NpcId = npcId, CurrentSceneName = newSceneName, CurrentMarkerName = newMarkerName });
        }
        Debug.Log($"[GlobalNpcManager] 更新状态: NPC '{npcId}' 现在位于场景 '{newSceneName}' 的 '{newMarkerName}'。");
    }

    /// <summary>
    /// 场景生成器调用此方法来获取应该在此场景中的NPC列表
    /// </summary>
    public List<NpcDefinition> GetNpcsForScene(string sceneName)
    {
        List<NpcDefinition> npcsInScene = new List<NpcDefinition>();
        foreach (var state in _npcStates.Values)
        {
            if (state.CurrentSceneName == sceneName)
            {
                npcsInScene.Add(state);
            }
        }
        return npcsInScene;
    }
}

