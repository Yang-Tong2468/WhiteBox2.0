using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class GlobalNpcManager : MonoBehaviour
{
    public static GlobalNpcManager Instance { get; private set; }

    // 使用字典存储所有NPC的状态，通过NPC的唯一ID来查找
    private Dictionary<string, NpcState> _npcStates = new Dictionary<string, NpcState>();

    void Awake()
    {
        // 标准的单例模式实现
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 确保在切换场景时不被销毁
        }
        else
        {
            Destroy(gameObject);
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
            _npcStates.Add(npcId, new NpcState { NpcId = npcId, CurrentSceneName = newSceneName, CurrentMarkerName = newMarkerName });
        }
        Debug.Log($"[GlobalNpcManager] 更新状态: NPC '{npcId}' 现在位于场景 '{newSceneName}' 的 '{newMarkerName}'。");
    }

    /// <summary>
    /// 场景生成器调用此方法来获取应该在此场景中的NPC列表
    /// </summary>
    public List<NpcState> GetNpcsForScene(string sceneName)
    {
        List<NpcState> npcsInScene = new List<NpcState>();
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

[System.Serializable]
public class NpcState
{
    public string NpcId;
    public string CurrentSceneName;
    public string CurrentMarkerName;
}