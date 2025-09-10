using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalNpcSpawner : MonoBehaviour
{
    public static GlobalNpcSpawner Instance { get; private set; }

    [Tooltip("所有NPC资源")]
    public NpcRegistry npcRegistry;

    void Awake()
    {
        // 设置单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 初始化场景
    /// </summary>
    /// <param name="scene">刚刚加载完成的场景</param>
    /// <param name="mode">加载模式（Single, Additive）</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查全局管理器是否存在
        if (GlobalNpcManager.Instance == null)
        {
            Debug.LogError("GlobalNpcManager 未找到！无法生成NPC。");
            return;
        }

        string currentSceneName = scene.name;
        var npcsToSpawn = GlobalNpcManager.Instance.GetNpcsForScene(currentSceneName);

        Debug.Log($"场景 '{currentSceneName}' 加载完成，准备刷新NPC... 找到 {npcsToSpawn.Count} 个。");

        foreach (var npc in npcsToSpawn)
        {
            SpawnNpc(npc);
        }
    }

    private void SpawnNpc(NpcDefinition npc)
    {
        // 查找标记点
        GameObject marker = GameObject.Find(npc.CurrentMarkerName);
        if (marker == null)
        {
            Debug.LogError($"在场景中找不到用于NPC '{npc.npcId}' 的标记点 '{npc.CurrentMarkerName}'！");
            return;
        }

        // 查找NPC Prefab
        GameObject npcPrefab = npcRegistry.GetPrefabById(npc.npcId);
        if (npcPrefab == null)
        {
            Debug.LogError($"在NPC注册表中找不到ID为 '{npc.npcId}' 的Prefab！");
            return;
        }

        // 在标记点位置实例化Prefab
        Instantiate(npcPrefab, marker.transform.position, marker.transform.rotation);
        Debug.Log($"已在 '{npc.CurrentMarkerName}' 位置生成NPC: {npc.npcId}");
    }
}