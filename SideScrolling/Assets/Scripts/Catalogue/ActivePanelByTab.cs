using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class ActivePanelByTab : MonoBehaviour
{
    public GameObject targetPanel;
    public bool toggleMode = true;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("已点击M键");
            // if (toggleMode)
            // {
            //     ShowPanel();
            // }
            ShowPanel();
        }
    }

    public void ShowPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            Debug.Log($"{targetPanel.name} 已显示");
        }
        else
        {
            Debug.LogError("目标面板未正确配置");
        }
    }
}
