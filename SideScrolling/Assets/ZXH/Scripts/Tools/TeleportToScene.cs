using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportToScene : MonoBehaviour
{
    [Tooltip("要切换到的场景名 (须已添加至 Build Settings)")]
    public string targetSceneName;

    [Tooltip("目标玩家出生点名称（在目标场景中命名的Transform）")]
    public string spawnPointName = "TeleportTarget";

    private bool isTeleporting = false; // 防重入锁


    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting) return; // 已经在传送中，直接忽略

        if (other.CompareTag("Player"))
        {
            isTeleporting = true; // 上锁，防止重复触发
            DontDestroyOnLoad(other.gameObject);
            StartCoroutine(LoadSceneAndTeleport(other.gameObject));
        }
    }

    private IEnumerator LoadSceneAndTeleport(GameObject player)
    {
        Debug.Log($"[TeleportToScene] 开始传送到：{targetSceneName}");

        var asyncOp = SceneManager.LoadSceneAsync(targetSceneName);
        while (!asyncOp.isDone)
            yield return null;

        var target = GameObject.Find(spawnPointName);
        if (target != null)
        {
            player.transform.position = target.transform.position;
        }
        else
        {
            Debug.LogWarning($"[TeleportToScene] 找不到物体：{spawnPointName}");
        }
    }
}
