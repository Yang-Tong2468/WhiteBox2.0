// using System;
// using UnityEngine;
// using Opsive.UltimateInventorySystem.ItemActions;
// using Opsive.UltimateInventorySystem.Core.DataStructures;
// using Opsive.Shared.Game;
// using MyGame.Time; // 引入APTimeManager

// [Serializable]
// public class GiftToNPCAction : ItemAction
// {
//     [Tooltip("送礼物时要调用的NPC交互方法名（可选）")]
//     [SerializeField] protected string m_NPCGiftMethodName = "ReceiveGift";

//     public GiftToNPCAction()
//     {
//         m_Name = "GiftToNPC";
//     }

//     /// <summary>
//     /// 判断是否可以送礼物（可根据需要扩展）
//     /// </summary>
//     protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
//     {
//         // 这里简单判断有NPC对象即可
//         var npc = GetTargetNPC(itemUser);
//         return npc != null && itemInfo.Amount > 0;
//     }

//     /// <summary>
//     /// 执行送礼物逻辑并消耗AP
//     /// </summary>
//     protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
//     {
//         var npc = GetTargetNPC(itemUser);
//         if (npc == null)
//         {
//             Debug.LogWarning("未找到目标NPC，无法送礼物。");
//             return;
//         }

//         // 调用NPC的ReceiveGift方法（如有）
//         var method = npc.GetType().GetMethod(m_NPCGiftMethodName);
//         if (method != null)
//         {
//             method.Invoke(npc, new object[] { itemInfo.Item });
//         }
//         else
//         {
//             Debug.Log($"NPC {npc.name} 没有 {m_NPCGiftMethodName} 方法，仅移除物品。");
//         }

//         // 移除1个礼物
//         var removeRequest = (ItemInfo)(1, itemInfo);
//         var removed = itemInfo.Inventory.RemoveItem(removeRequest);

//         // 消耗一个行动点
//         var apManager = UnityEngine.Object.FindObjectOfType<APTimeManager>();
//         if (apManager != null)
//         {
//             apManager.ConsumeAP();
//             Debug.Log("送礼物消耗了一个行动点");
//         }
//         else
//         {
//             Debug.LogWarning("未找到APTimeManager，无法消耗行动点");
//         }

//         Debug.Log($"送出礼物 {itemInfo.Item.name} 给 {npc.name}，实际移除 {removed.Amount} 个。");
//     }

//     /// <summary>
//     /// 获取目标NPC对象（可根据你的实际需求修改）
//     /// </summary>
//     private Component GetTargetNPC(ItemUser itemUser)
//     {
//         // 假设ItemUser身上有当前交互的NPC引用（如没有请根据你的系统调整）
//         // 例如：itemUser.CurrentTargetNPC
//         // 这里简单用射线检测玩家面前的NPC
//         var player = itemUser.gameObject;
//         RaycastHit hit;
//         if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, 3f))
//         {
//             var npc = hit.collider.GetComponentInParent<MonoBehaviour>(); // 或者你的NPC基类
//             return npc;
//         }
//         return null;
//     }
// }

// GiftToNPCAction.cs
using System;
using UnityEngine;
using Opsive.UltimateInventorySystem.ItemActions;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using MyGame.Time;
using UnityEditorInternal;

[Serializable]
public class GiftToNPCAction : ItemAction
{
    protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
    {
        Debug.Log($"CanInvoke: Instance={GiftManager.Instance}, NPC={GiftManager.Instance?.CurrentTargetNPCName}, Amount={itemInfo.Amount}");

        return !string.IsNullOrEmpty(GiftManager.Instance.CurrentTargetNPCName) && itemInfo.Amount > 0;
    }

    protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
    {
        string npcName = GiftManager.Instance.CurrentTargetNPCName;
        if (string.IsNullOrEmpty(npcName))
        {
            Debug.LogWarning("未指定目标NPC，无法送礼物。");
            return;
        }

        // 场景中查找NPC GameObject（假设名字唯一）
        GameObject npcObj = GameObject.Find(npcName);
        if (npcObj == null)
        {
            Debug.LogWarning($"未找到NPC: {npcName}");
            return;
        }

        // 调用NPC的收礼方法
        var receiver = npcObj.GetComponent<NPCReceiver>();
        if (receiver != null)
        {
            receiver.ReceiveGift(itemInfo.Item);
        }

        // 移除物品
        itemInfo.Inventory.RemoveItem((ItemInfo)(1, itemInfo));

        string reason;
        // 消耗行动点
        var apManager = UnityEngine.Object.FindObjectOfType<APTimeManager>();
        if (apManager != null)
        {
            apManager.TryConsumeAP(1, out reason);
            ShowUIMessage(reason);
        }
    }

    private void ShowUIMessage(string msg)
    {
        Debug.Log($"[UI] {msg}");
        // TODO: 替换成项目内真实的 UI 提示方法（弹窗/吐司）
    }
}