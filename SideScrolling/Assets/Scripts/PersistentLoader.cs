using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentLoader : MonoBehaviour
{
    void Start()
    {
        // 异步加载主游戏场景，Additive 表示叠加加载
        SceneManager.LoadSceneAsync("SideScrolling 2", LoadSceneMode.Additive);
    }
}