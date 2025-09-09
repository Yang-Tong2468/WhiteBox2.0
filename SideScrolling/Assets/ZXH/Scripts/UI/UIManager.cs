using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI 面板引用")]
    public CharacterStatsPanel statsPanel; // 直接引用脚本本身

    void Update()
    {
        // 在这里集中处理所有UI的快捷键
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (statsPanel != null)
            {
                statsPanel.TogglePanel();
            }
        }

    }
}