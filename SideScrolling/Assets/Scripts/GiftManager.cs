using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftManager : MonoBehaviour
{
    public static GiftManager Instance { get; private set; }
    public string CurrentTargetNPCName { get; private set; }

    void Awake() { Instance = this; }

    public void SetTargetNPC(string npcName)
    {
        CurrentTargetNPCName = npcName;
        Debug.Log("当前目标NPC已设置为: " + npcName);
    }
}
