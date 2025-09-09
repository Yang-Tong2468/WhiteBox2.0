using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DefaultExecutionOrder(-50)]
public class CharacterStats : MonoBehaviour
{
    [Header("属性集合（数据驱动）")]
    public AttributeSetDefinition baseAttributeSet;

    [Header("初始修正（可选：装备、被动等）")]
    public List<StatModifier> initialModifiers = new();

    // 运行时字典
    private readonly Dictionary<string, float> _baseValues = new();
    private readonly Dictionary<string, List<StatModifier>> _mods = new();

    // 事件：属性变更（解耦给UI/任务/AI用）
    public GameEventFloatChanged onAttributeChanged;

    void Awake()
    {
        //初始化
        if (baseAttributeSet != null)
        {
            // 遍历新的 AttributeDefault 列表
            foreach (var attrDefault in baseAttributeSet.attributes)
            {
                // 从配对中获取属性定义和初始值
                var def = attrDefault.Definition;
                var startValue = attrDefault.Value;

                if (def == null) continue; // 安全检查

                // 使用配置的初始值，并确保它在min/max范围内
                var clampedStartValue = Mathf.Clamp(startValue, def.minValue, def.maxValue);

                // 使用属性的唯一ID作为键，存入处理后的初始值
                _baseValues[def.id] = clampedStartValue;
                _mods[def.id] = new List<StatModifier>();
            }
        }

        foreach (var m in initialModifiers) AddModifier(m, invokeEvent: false);

        RecalculateAll(invokeEvent: false);
    }

    public bool HasAttribute(AttributeDefinition def) => def != null && _baseValues.ContainsKey(def.id);


    /// <summary>
    /// 通过属性ID，对属性的基础值进行加减操作
    /// </summary>
    /// <param name="attributeID">属性的唯一字符串ID</param>
    /// <param name="delta">变化的量（正数为增加，负数为减少）</param>
    public void AddAttributeByID(string attributeID, float delta)
    {
        Debug.Log($"尝试通过ID '{attributeID}' 修改属性，变化量为 {delta}", this);
        AttributeDefinition targetDef = FindAttributeDefinitionByID(attributeID);
        if (targetDef != null)
        {
            Debug.Log($"找到属性 '{targetDef.displayName}' ({targetDef.id})，准备修改基础值。", this);
            // 调用已有的、基于AttributeDefinition的Add方法
            Add(targetDef, delta, true);
        }
    }

    /// <summary>
    /// 通过属性ID，直接设置属性的基础值
    /// </summary>
    /// <param name="attributeID">属性的唯一字符串ID</param>
    /// <param name="value">要设置的新基础值</param>
    public void SetAttributeBaseByID(string attributeID, float value)
    {
        AttributeDefinition targetDef = FindAttributeDefinitionByID(attributeID);
        if (targetDef != null)
        {
            // 调用已有的、基于AttributeDefinition的SetBase方法
            SetBase(targetDef, value, true);
        }
    }

    /// <summary>
    /// 辅助函数：通过字符串ID查找对应的AttributeDefinition
    /// </summary>
    private AttributeDefinition FindAttributeDefinitionByID(string attributeID)
    {
        if (string.IsNullOrEmpty(attributeID) || baseAttributeSet == null)
        {
            Debug.LogWarning($"尝试查找属性时，ID为空或AttributeSet未设置。", this);
            return null;
        }

        // 在新的列表中查找，并返回匹配的 Definition
        // ?. 是空值条件运算符，如果 FirstOrDefault 没找到（返回null），则整个表达式返回null，避免报错
        AttributeDefinition targetDef = baseAttributeSet.attributes.FirstOrDefault(attr => attr.Definition.id == attributeID)?.Definition;

        if (targetDef == null)
        {
            Debug.LogWarning($"在 '{gameObject.name}' 的属性集中找不到ID为 '{attributeID}' 的属性。", this);
        }

        return targetDef;
    }

    /// <summary>
    /// 通过属性的字符串ID获取其最终值
    /// </summary>
    /// <param name="attributeID">要查询的属性ID</param>
    /// <returns>计算后的最终属性值,如果ID无效，则返回0</returns>
    public float GetAttributeValueByID(string attributeID)
    {
        // 检查ID和属性集合是否有效
        if (string.IsNullOrEmpty(attributeID) || baseAttributeSet == null)
        {
            Debug.LogWarning($"Attempted to get an attribute with a null or empty ID.");
            return 0f;
        }

        // 使用LINQ在属性列表中查找与ID匹配的AttributeDefinition
        AttributeDefinition targetDef = baseAttributeSet.attributes.FirstOrDefault(def => def.Definition.id == attributeID)?.Definition;

        if (targetDef == null)
        {
            Debug.LogWarning($"Attribute with ID '{attributeID}' not found on '{gameObject.name}'.", this);
            return 0f; // 返回一个安全的默认值
        }

        // 如果找到了，就调用现有的、功能完备的GetFinal方法来获取最终值
        return GetFinal(targetDef);
    }

    /// <summary>
    /// 获得一个属性的最终值（在应用了所有修正之后） 
    /// 这是外部系统获取角色当前属性值的主要接口
    /// </summary>
    /// <param name="def">要查询的属性定义</param>
    /// <returns>计算和钳制后的最终值</returns>
    public float GetFinal(AttributeDefinition def)
    {
        if (!HasAttribute(def)) return 0f;

        float v = _baseValues[def.id];
        foreach (var m in _mods[def.id].OrderBy(x => x.order))
            v = m.Apply(v);

        var clamped = Mathf.Clamp(v, def.minValue, def.maxValue);
        return def.useInteger ? Mathf.Round(clamped) : clamped;
    }

    /// <summary>
    /// 设置一个属性的基础值
    /// </summary>
    /// <param name="def">要设置的属性定义</param>
    /// <param name="value">新的基础值</param>
    /// <param name="invokeEvent">是否触发属性变更事件</param>
    public void SetBase(AttributeDefinition def, float value, bool invokeEvent = true)
    {
        if (!HasAttribute(def)) return;
        Debug.Log("包含这个定义");
        var old = GetFinal(def);
        _baseValues[def.id] = Mathf.Clamp(value, def.minValue, def.maxValue);
        var now = GetFinal(def);
        Debug.Log($"{invokeEvent}以及{value},old:{old},now:{now}");
        if (invokeEvent && !Mathf.Approximately(old, now))
        {
            Debug.Log($"属性 '{def.displayName}' ({def.id}) 变更: {old} -> {now}", this);
            onAttributeChanged?.Raise(def.id, old, now);
        }
    }

    /// <summary>
    /// 对一个属性的基础值进行加减操作
    /// 这是一个便利方法，内部调用SetBase
    /// </summary>
    /// <param name="def">要操作的属性定义</param>
    /// <param name="delta">变化的量（正数为加，负数为减）</param>
    /// <param name="invokeEvent">是否触发事件</param>
    public void Add(AttributeDefinition def, float delta, bool invokeEvent = true)
    {
        if (!HasAttribute(def)) return;
        Debug.Log($"尝试对属性 '{def.displayName}' ({def.id}) 进行加减操作，变化量为 {delta}", this);
        SetBase(def, _baseValues[def.id] + delta, invokeEvent);
    }

    /// <summary>
    /// 为一个属性添加一个修正效果（例如：穿上装备、获得一个Buff）
    /// </summary>
    /// <param name="mod">要添加的修正器</param>
    /// <param name="invokeEvent">是否触发事件</param>
    public void AddModifier(StatModifier mod, bool invokeEvent = true)
    {
        if (mod == null || mod.target == null) return;

        if (!_mods.ContainsKey(mod.target.id)) _mods[mod.target.id] = new List<StatModifier>();
        
        var old = GetFinal(mod.target);
        _mods[mod.target.id].Add(mod);
        var now = GetFinal(mod.target);

        if (invokeEvent && !Mathf.Approximately(old, now))
            onAttributeChanged?.Raise(mod.target.id, old, now);
    }

    public void RemoveModifier(StatModifier mod, bool invokeEvent = true)
    {
        if (mod == null || mod.target == null) return;
        if (!_mods.ContainsKey(mod.target.id)) return;

        var old = GetFinal(mod.target);
        _mods[mod.target.id].Remove(mod);
        var now = GetFinal(mod.target);

        if (invokeEvent && !Mathf.Approximately(old, now))
            onAttributeChanged?.Raise(mod.target.id, old, now);
    }

    /// <summary>
    /// 重新计算所有属性的值并触发相应的事件
    /// 主要用于某些全局性变化（可能影响多个修正效果）后，强制刷新所有状态和UI
    /// </summary>
    /// <param name="invokeEvent">是否触发事件</param>
    public void RecalculateAll(bool invokeEvent = true)
    {
        if (baseAttributeSet == null) return;

        // 遍历新的 AttributeDefault 列表
        foreach (var attrDefault in baseAttributeSet.attributes)
        {
            var def = attrDefault.Definition;
            if (def == null) continue;

            var old = GetFinal(def);
            var now = GetFinal(def); // 读取即应用修正
            if (invokeEvent && !Mathf.Approximately(old, now))
                onAttributeChanged?.Raise(def.id, old, now);
        }
    }
}
