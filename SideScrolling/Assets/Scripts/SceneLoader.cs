using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName; // 目标场景名称

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            try
            {
                SceneManager.LoadScene(sceneName);
                Debug.Log($"正在加载场景: {sceneName}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载场景失败: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("场景名称未设置!");
        }
    }

    // 可选：使用场景索引加载
    public void LoadSceneByIndex(int sceneIndex)
    {
        try
        {
            SceneManager.LoadScene(sceneIndex);
            Debug.Log($"正在加载场景索引: {sceneIndex}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载场景失败: {e.Message}");
        }
    }
}