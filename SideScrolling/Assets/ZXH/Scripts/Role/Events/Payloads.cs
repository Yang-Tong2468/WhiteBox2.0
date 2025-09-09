using UnityEngine;

/// <summary>
/// 属性值变化事件需要的数据的结构
/// </summary>
[System.Serializable]
public struct FloatChangedPayload
{
    public string key;  // attribute id
    public float before;
    public float after;
}


/// <summary>
/// 好感度变化事件需要的数据的结构
/// </summary>
[System.Serializable]
public struct AffinityChangedPayload
{
    public NpcDefinition npc;
    public float before;
    public float after;
}


/// <summary>
/// 好感度段位变化事件需要的数据的结构
/// </summary>
[System.Serializable]
public struct TierChangedPayload
{
    public NpcDefinition npc;
    public string fromTier;
    public string toTier;
}
