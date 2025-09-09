using UnityEngine;
using System.Collections.Generic;
using TMPro; 

public class CharacterStatsPanel : MonoBehaviour
{
    [Header("要监听的目标")]
    [Tooltip("将拥有 CharacterStats 组件的角色拖到这里")]
    public CharacterStats targetStats;

    [Header("UI 设置")]
    [Tooltip("一个包含 TextMeshProUGUI 组件的UI预制体，用于显示单行属性")]
    public GameObject attributeUIPrefab;

    [Tooltip("放置所有属性文本的容器对象")]
    public Transform container;

    [Header("面板控制")]
    [Tooltip("整个面板的根对象，我们将通过开关它来控制显示和隐藏")]
    public GameObject panelRoot;

    // 运行时字典，用于快速查找和更新UI元素
    private readonly Dictionary<string, TextMeshProUGUI> _attributeUIElements = new();

    // 缓存属性定义，避免每次更新都去查找
    private readonly Dictionary<string, AttributeDefinition> _attributeDefinitions = new();


    void Start()
    {
        // 游戏开始时默认关闭面板，确保初始状态是隐藏的
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (targetStats == null || attributeUIPrefab == null || container == null)
        {
            Debug.LogError("CharacterStatsPanel 未正确配置！请在Inspector中指定所有字段。", this);
            this.enabled = false;
            return;
        }

        targetStats.onAttributeChanged.Register(OnAttributeValueChanged);

        // 首次激活时，生成整个UI面板
        InitializePanel();
    }

    void OnDisable()
    {
        if (targetStats != null)
        {
            targetStats.onAttributeChanged.Unregister(OnAttributeValueChanged);
        }
    }

    /// <summary>
    /// 初始化面板，为每个属性创建UI元素
    /// </summary>
    private void InitializePanel()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        _attributeUIElements.Clear();
        _attributeDefinitions.Clear();

        if (targetStats.baseAttributeSet == null) return;

        foreach (var attributeDefault in targetStats.baseAttributeSet.attributes)
        {
            var attrDef = attributeDefault.Definition;

            GameObject uiInstance = Instantiate(attributeUIPrefab, container);
            uiInstance.name = $"UI_{attrDef.id}"; 

            TextMeshProUGUI textComponent = uiInstance.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                // 将属性定义和UI组件存入字典，方便后续快速访问
                _attributeDefinitions[attrDef.id] = attrDef;
                _attributeUIElements[attrDef.id] = textComponent;

                // 获取当前属性的最终值并更新文本
                float currentValue = targetStats.GetFinal(attrDef);
                textComponent.text = $"{attrDef.displayName}: {currentValue}";
            }
        }
    }

    /// <summary>
    /// 更新属性值UI显示
    /// </summary>
    private void OnAttributeValueChanged(FloatChangedPayload payload)
    {
        Debug.Log("检测到属性更改");
        // 检查是否存在该属性的UI元素
        if (_attributeUIElements.ContainsKey(payload.key))
        {
            Debug.Log($"属性 '{payload.key}' 的值从 {payload.before} 更新为 {payload.after}");
            // 从缓存中获取显示名称，并用 payload 中的新值更新文本
            string displayName = _attributeDefinitions[payload.key].displayName;
            _attributeUIElements[payload.key].text = $"{displayName}: {payload.after}";
        }
    }


    /// <summary>
    /// 切换面板的显示状态
    /// </summary>
    public void TogglePanel()
    {
        if (panelRoot == null) return;

        // 检查当前状态，然后设置为相反的状态
        bool isCurrentlyActive = panelRoot.activeSelf;
        panelRoot.SetActive(!isCurrentlyActive);
    }
}