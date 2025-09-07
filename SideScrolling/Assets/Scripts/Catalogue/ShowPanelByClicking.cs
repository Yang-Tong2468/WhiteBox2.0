using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPanelOnClick : MonoBehaviour
{
    public GameObject showPanel;
    public GameObject closePanel;

    public void ShowPanel()
    {
        if (showPanel != null)
        {
            showPanel.SetActive(true);

            Debug.Log($"{showPanel}已显示");
        }
        else
        {
            Debug.Log($"{showPanel}未正确配置");
        }
    }

    public void ClosePanel()
    {
        if (closePanel != null)
        {
            closePanel.SetActive(false);

            Debug.Log($"{closePanel}已关闭");
        }
        else
        {
            Debug.Log($"{closePanel}未正确配置");
        }
    }
}
