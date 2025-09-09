using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Opsive.UltimateInventorySystem.ItemActions;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using System;
using Opsive.Shared.Game;
using Opsive.UltimateInventorySystem.Core.AttributeSystem;

[Serializable]
public class HealAction : ItemAction
{
    [SerializeField] protected string m_AttributeName = "HealAmount";

    /// <summary>
    /// 判断是否符合执行条件
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="itemUser"></param>
    /// <returns></returns>
    protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
    {
        var character = itemUser.gameObject.GetCachedComponent<Character>();

        if(character == null) return false;

        if(itemInfo.Item.GetAttribute(m_AttributeName) == null) return false;

        return true;
    }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="itemInfo"></param>
    /// <param name="itemUser"></param>
    protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
    {
        var character = itemUser.gameObject.GetCachedComponent<Character>();
        int healAmount = itemInfo.Item.GetAttribute<Attribute<int>>(m_AttributeName).GetValue();
        character.Heal(healAmount);

        // 使用 ItemInfo 构建删除请求，确保精确操作
        var removeRequest = (ItemInfo)(1, itemInfo);     // 新建一个相同 ItemInfo 表示移除 1 个
        var removed = itemInfo.Inventory.RemoveItem(removeRequest);

        // 可选：Debug 输出
        Debug.Log($"尝试移除 1 个 {itemInfo.Item.name}，实际移除 {removed.Amount} 个，来源堆栈中剩余 {(removed.ItemStack?.Amount ?? 0)} 个。");
    }
}
