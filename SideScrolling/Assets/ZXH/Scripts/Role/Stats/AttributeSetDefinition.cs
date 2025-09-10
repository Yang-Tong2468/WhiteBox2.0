using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "RPG/Stats/Attribute Set", fileName = "AttrSet_")]
public class AttributeSetDefinition : ScriptableObject
{
    public List<AttributeDefault> attributes = new();
}

/// <summary>
/// 一个数据容器，用于将一个属性定义(AttributeDefinition)和它的默认初始值(Value)配对
/// </summary>
[System.Serializable]
public class AttributeDefault
{
    [Tooltip("要引用的属性定义 (ScriptableObject)")]
    public AttributeDefinition Definition;

    [Tooltip("这个属性在这个集合中的初始基础值")]
    public float Value;
}