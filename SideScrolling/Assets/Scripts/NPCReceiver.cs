using UnityEngine;
using PixelCrushers.DialogueSystem;

public class NPCReceiver : MonoBehaviour
{
    public string giftReceivedConversation; // Inspector里填写收到礼物后触发的对话标题

    public void ReceiveGift(Opsive.UltimateInventorySystem.Core.Item item)
    {
        Debug.Log($"{gameObject.name} 收到了礼物: {item.name}");

        // 触发一段新对话
        if (!string.IsNullOrEmpty(giftReceivedConversation))
        {
            DialogueManager.StartConversation(giftReceivedConversation, transform, null);
        }
        else
        {
            Debug.LogWarning("giftReceivedConversation 未设置，无法触发对话。");
        }
    }
}