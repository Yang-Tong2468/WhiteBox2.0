using UnityEngine;
using TMPro;

/// <summary>
/// 放在每个列表项预制体上，持有两个 TextMeshProUGUI 引用，便于面板脚本快速填充文本
/// </summary>
public class AffinityItem: MonoBehaviour
{
    [Tooltip("显示 NPC 名字")]
    public TextMeshProUGUI nameText;

    [Tooltip("显示 好感数值 与 段位")]
    public TextMeshProUGUI valueText;

    /// <summary>
    /// 填充显示内容
    /// </summary>
    public void Set(string npcId, float value, string tier)
    {
        if (nameText != null) nameText.text = npcId;
        if (valueText != null) valueText.text = $"Affinite Value: {value:F1} | Level: {tier}";
    }
}
