using UnityEngine;

[CreateAssetMenu(menuName = "RPG/NPC/NPC Definition", fileName = "NPC_")]
public class NpcDefinition : ScriptableObject
{
    public string npcId;         // 唯一ID
    public string displayName;   // 显示名
    public Sprite portrait;      // 头像（可选）
}
