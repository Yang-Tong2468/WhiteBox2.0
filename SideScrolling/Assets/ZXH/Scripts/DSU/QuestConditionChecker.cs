using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 剧情条件检查器
/// </summary>
public class QuestConditionChecker: MonoBehaviour
{
    public static QuestConditionChecker Instance;

    [Header("Quest Conditions")]
    public bool playerUsedLustCardInPleasureHouse = false;
    public bool isNabhaniSupporter = false;
    public bool isNabuhaniInHand= true; // 假设奈布哈尼默认是活着的

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        Lua.RegisterFunction("CanTriggerNabuhaniVisit", this, GetType().GetMethod("CanTriggerNabuhaniVisit"));
    }

    private void OnDisable()
    {
        Lua.UnregisterFunction("CanTriggerNabuhaniVisit");
    }

    #region 接口


    /// <summary>
    /// 奈布哈尼拜访条件检查
    /// </summary>
    /// <returns></returns>
    public bool CanTriggerNabuhaniVisit()
    {
        // 检查条件 ② 和 ③
        bool isNabuhaniAlive = DialogueLua.GetVariable("isNabuhaniAlive").asBool;
        bool hasTriggeredFlowerProtector = DialogueLua.GetVariable("hasTriggeredFlowerProtector").asBool;

        // 检查条件 ① 
        bool condition1 = playerUsedLustCardInPleasureHouse;

        // 只有当所有条件都满足时，才返回 true
        if (condition1 && isNabuhaniAlive && !hasTriggeredFlowerProtector)
        {
            return true;
        }

        return false;
    }

    #endregion
}
