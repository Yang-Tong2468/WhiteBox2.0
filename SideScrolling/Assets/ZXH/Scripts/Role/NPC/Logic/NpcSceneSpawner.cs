using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcSceneSpawner : MonoBehaviour
{
    [Tooltip("包含所有NPC Prefab映射的注册表资源")]
    public NpcRegistry npcRegistry;

    void Start()
    {
        if (GlobalNpcManager.Instance == null)
        {
            Debug.LogError("场景中找不到 GlobalNpcManager！");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        var npcsToSpawn = GlobalNpcManager.Instance.GetNpcsForScene(currentSceneName);

        Debug.Log($"场景 '{currentSceneName}' 正在刷新NPC... 找到 {npcsToSpawn.Count} 个。");

        foreach (var npcState in npcsToSpawn)
        {
            SpawnNpc(npcState);
        }
    }

    private void SpawnNpc(NpcState state)
    {
        // 1. 查找标记点
        GameObject marker = GameObject.Find(state.CurrentMarkerName);
        if (marker == null)
        {
            Debug.LogError($"在场景中找不到用于NPC '{state.NpcId}' 的标记点 '{state.CurrentMarkerName}'！");
            return;
        }

        // 2. 查找NPC Prefab
        GameObject npcPrefab = npcRegistry.GetPrefabById(state.NpcId);
        if (npcPrefab == null)
        {
            Debug.LogError($"在NPC注册表中找不到ID为 '{state.NpcId}' 的Prefab！");
            return;
        }

        // 3. 在标记点位置实例化Prefab
        Instantiate(npcPrefab, marker.transform.position, marker.transform.rotation);
        Debug.Log($"已在 '{state.CurrentMarkerName}' 位置生成NPC: {state.NpcId}");
    }
}