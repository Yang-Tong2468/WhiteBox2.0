using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "RPG/Stats/Attribute Set", fileName = "AttrSet_")]
public class AttributeSetDefinition : ScriptableObject
{
    public List<AttributeDefinition> attributes = new();
}
