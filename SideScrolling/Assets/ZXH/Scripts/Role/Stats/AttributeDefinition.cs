using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Stats/Attribute Definition", fileName = "Attribute_")]
public class AttributeDefinition : ScriptableObject
{
    [Tooltip("唯一ID，例如 governance/stamina/charisma/gold")]
    public string id;

    [Tooltip("用于显示的名称")]
    public string displayName;

    [Tooltip("最小/最大值（如体力0-100）。对金币之类可以用非常大上限")]
    public float minValue = 0;
    public float maxValue = 100;

    [Tooltip("标记此属性是否为一种“资源”，即会频繁变动的值，如生命值、法力值、体力、金币等")]
    public bool isResource = false;

    [Tooltip("标记此属性的最终值是否应被当作整数处理（例如金币、技能点数）")]
    public bool useInteger = false;
}
