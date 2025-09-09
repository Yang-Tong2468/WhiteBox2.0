using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;
using Opsive.UltimateInventorySystem.Equipping;
using Opsive.Shared.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.UltimateInventorySystem.Exchange;

[System.Serializable]
public struct CharacterAttributes
{
    public int basePhysique, currentPhysique;
    public int baseSocial, currentSocial;
    public int baseSurvival, currentSurvival;
    public int baseIntelligence, currentIntelligence;
    public int baseCharm, currentCharm;
    public int baseCombat, currentCombat;
}

public class Character : MonoBehaviour
{
    public static Character Instance { get; private set; }

    [Header("角色基础属性")]
    [SerializeField]protected string characterName;
    [SerializeField]protected CharacterAttributes attributes;
    [SerializeField]protected int currentHealth = 30;
    protected int maxHealth = 100;

    [Header("加装备")]
    protected IEquipper m_Equipper;
    public Inventory inventory;
    protected CurrencyOwner m_CurrencyOwner; // 货币拥有者组件
    //private Dictionary<string, ItemDefinition> m_DefinitionCache;

    [Header("角色")]
    [SerializeField] public List<Role> roleList; //暂存的角色

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

        inventory = GetComponent<Inventory>();
        m_Equipper = GetComponent<IEquipper>();
        m_CurrencyOwner = GetComponent<CurrencyOwner>();
        if (m_CurrencyOwner == null)
        {
            Debug.LogError("在游戏对象上找不到 CurrencyOwner 组件！请确保此脚本与 CurrencyOwner 附加在同一个对象上。", this);
        }

        //初始化角色属性以及属性UI更新
        InitCharacterStat();
    }

    private void OnEnable()
    {
        Opsive.Shared.Events.EventHandler.RegisterEvent(m_Equipper, EventNames.c_Equipper_OnChange, OnEquipmentChanged);
    }

    private void OnDisable()
    {
        Opsive.Shared.Events.EventHandler.UnregisterEvent(m_Equipper, EventNames.c_Equipper_OnChange, OnEquipmentChanged);
    }

    /// <summary>
    /// 初始化当前属性为基础属性
    /// </summary>
    private void InitCharacterStat()
    {
        attributes.currentPhysique = attributes.basePhysique;
        attributes.currentSocial = attributes.baseSocial;
        attributes.currentSurvival = attributes.baseSurvival;
        attributes.currentIntelligence = attributes.baseIntelligence;
        attributes.currentCharm = attributes.baseCharm;
        attributes.currentCombat = attributes.baseCombat;
    }

    /// <summary>
    /// 当装备发生变化时调用
    /// </summary>
    /// <param name="equipper"></param>
    private void OnEquipmentChanged()
    {
        UpdateAllAttributes();
    }

    /// <summary>
    /// 重新计算角色所有属性：基础 + 装备加成
    /// </summary>
    public void UpdateAllAttributes()
    {
        // 从装备中获取属性加成
        attributes.currentPhysique = attributes.basePhysique + m_Equipper.GetEquipmentStatInt("physique");
        attributes.currentSocial = attributes.baseSocial + m_Equipper.GetEquipmentStatInt("social");
        attributes.currentSurvival = attributes.baseSurvival + m_Equipper.GetEquipmentStatInt("survival");
        attributes.currentIntelligence = attributes.baseIntelligence + m_Equipper.GetEquipmentStatInt("intelligence");
        attributes.currentCharm = attributes.baseCharm + m_Equipper.GetEquipmentStatInt("charm");
        attributes.currentCombat = attributes.baseCombat + m_Equipper.GetEquipmentStatInt("combat");
   
        // 可在此同步 UI等
        Debug.Log($"Updated Attributes: P{attributes.currentPhysique}, S{attributes.currentSocial}, " +
                  $"Sur{attributes.currentSurvival}, Int{attributes.currentIntelligence}, " +
                  $"C{attributes.currentCharm}, Combat{attributes.currentCombat}");

        //// 这里可以添加事件通知系统，通知其他系统属性已更新
        //foreach( var baseEvent in CharacterEventManager.Instance?.activeEventsDic.Values)
        //{
        //    baseEvent.UpdateAttributeRequirementValues();
        //}
    }


    [ContextMenu("加入2张Armor到背包")]
    public void AddToInventory()
    {
        
        inventory.AddItem("Armor",2);
    }

    #region action
    public void Heal(int amount)
    {
        if(currentHealth <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    #endregion

    #region 属性操作——装备属于加成的，剧情的属性属于基础的

    public int GetAttribute(string attrName)
    {
        return attrName switch
        {
            "physique" => attributes.currentPhysique,
            "social" => attributes.currentSocial,
            "survival" => attributes.currentSurvival,
            "intelligence" => attributes.currentIntelligence,
            "charm" => attributes.currentCharm,
            "combat" => attributes.currentCombat,
            _ => 0
        };
    }

    public void SetBaseAttribute(string attrName, int value)
    {
        switch (attrName)
        {
            case "physique": attributes.basePhysique = value; break;
            case "social": attributes.baseSocial = value; break;
            case "survival": attributes.baseSurvival = value; break;
            case "intelligence": attributes.baseIntelligence = value; break;
            case "charm": attributes.baseCharm = value; break;
            case "combat": attributes.baseCombat = value; break;
        }
        UpdateAllAttributes();
    }


    /// <summary>
    /// 为一个基础属性增加指定的值
    /// </summary>
    /// <param name="attrName">属性名称</param>
    /// <param name="valueToAdd">要增加的值</param>
    public void AddToBaseAttribute(string attrName, int valueToAdd)
    {
        // 这个函数会先获取当前基础值，加上增量，然后再设置回去
        // 这样可以复用你已经写好的SetBaseAttribute逻辑，非常方便
        switch (attrName)
        {
            case "physique": SetBaseAttribute("physique", attributes.basePhysique + valueToAdd); break;
            case "social": SetBaseAttribute("social", attributes.baseSocial + valueToAdd); break;
            case "survival": SetBaseAttribute("survival", attributes.baseSurvival + valueToAdd); break;
            case "intelligence": SetBaseAttribute("intelligence", attributes.baseIntelligence + valueToAdd); break;
            case "charm": SetBaseAttribute("charm", attributes.baseCharm + valueToAdd); break;
            case "combat": SetBaseAttribute("combat", attributes.baseCombat + valueToAdd); break;
        }
        Debug.Log($"属性 {attrName} 的基础值增加了 {valueToAdd}!");
    }

    #endregion

    #region 装备相关
    /// <summary>
    /// 判断角色是否装备了指定物品（通过物品定义名称）
    /// </summary>
    /// <param name="itemDefinitionName">物品定义名称</param>
    /// <returns>是否已装备该物品</returns>
    public bool IsEquippedByDefinitionName(string itemDefinitionName)
    {
        if (string.IsNullOrEmpty(itemDefinitionName))
        {
            Debug.LogWarning("物品定义名称为空！");
            return false;
        }

        if (m_Equipper == null)
        {
            Debug.LogWarning("角色未绑定 IEquipper 组件，无法判断装备。");
            return false;
        }

        var itemDef = InventorySystemManager.GetItemDefinition(itemDefinitionName);
        if (itemDef == null)
        {
            Debug.LogError($"未找到物品定义: {itemDefinitionName}");
            return false;
        }

        // 遍历所有装备槽
        // 推荐用 GetEquippedItem(index) 或 GetEquippedItems()，具体看你的 Equipper 实现
        int slotCount = 3; // 可根据实际装备槽数量调整
        for (int i = 0; i < slotCount; i++)
        {
            var equippedItem = m_Equipper.GetEquippedItem(i);
            if (equippedItem != null && equippedItem.ItemDefinition == itemDef)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断角色装备是否拥有列表中的所有装备
    /// </summary>
    /// <param name="itemDefinitionNames"></param>
    /// <returns></returns>
    public bool HasAllEquipByDefinitionNames(List<string> itemDefinitionNames)
    {
        if (itemDefinitionNames == null || itemDefinitionNames.Count == 0)
            return false;

        foreach (var name in itemDefinitionNames)
        {
            if (!IsEquippedByDefinitionName(name))
                return false;
        }
        return true;
    }
    #endregion

    #region 货币相关
    /// <summary>
    /// 为玩家增加指定数量的某种货币
    /// </summary>
    /// <param name="currencyName">货币在数据库中的定义名称</param>
    /// <param name="amount">要增加的数量</param>
    public void AddCurrency(string currencyName, int amount)
    {
        if (IsInvalidRequest(currencyName, amount, "AddCurrency"))
        {
            return;
        }

        var currency = InventorySystemManager.GetCurrency(currencyName);
        if (currency == null)
        {
            Debug.LogWarning($"[PlayerCurrencyManager] 增加货币失败：名为 '{currencyName}' 的货币未在数据库中定义");
            return;
        }

        // 直接调用 CurrencyOwner 的方法增加货币
        m_CurrencyOwner.AddCurrency(currency, amount);
        Debug.Log($"成功为玩家增加了 {amount} {currencyName}当前总数: {GetCurrencyAmount(currencyName)}");
    }

    /// <summary>
    /// 消耗玩家一定数目的货币。
    /// 此方法会先检查玩家是否有足够的货币，如果不足则不会进行任何操作。
    /// </summary>
    /// <param name="currencyName">货币在数据库中的定义名称</param>
    /// <param name="amount">要消耗的数量 </param>
    /// <returns>如果成功消耗返回 true，如果货币不足则返回 false</returns>
    public bool SpendCurrency(string currencyName, int amount)
    {
        if (IsInvalidRequest(currencyName, amount, "SpendCurrency"))
        {
            return false;
        }

        // 1. 获取货币定义
        var currency = InventorySystemManager.GetCurrency(currencyName);
        if (currency == null)
        {
            Debug.LogWarning($"[PlayerCurrencyManager] 消耗货币失败：名为 '{currencyName}' 的货币未在数据库中定义。");
            return false;
        }

        // 2. 检查当前拥有的货币数量
        int currentAmount = GetCurrencyAmount(currencyName);
        if (currentAmount < amount)
        {
            Debug.Log($"消耗货币失败：玩家试图花费 {amount} {currencyName}，但只拥有 {currentAmount}。");
            return false;
        }

        // 3. 如果货币充足，则执行消耗操作
        m_CurrencyOwner.RemoveCurrency(currency, amount);
        Debug.Log($"成功消耗了 {amount} {currencyName}。剩余数量: {GetCurrencyAmount(currencyName)}");
        return true;
    }

    /// <summary>
    /// 获取玩家当前拥有某种货币的具体数量。
    /// </summary>
    /// <param name="currencyName">货币在数据库中的定义名称。</param>
    /// <returns>拥有的货币数量。如果货币未定义或玩家不曾拥有，则返回 0。</returns>
    public int GetCurrencyAmount(string currencyName)
    {
        if (m_CurrencyOwner == null || string.IsNullOrEmpty(currencyName))
        {
            return 0;
        }

        var currency = InventorySystemManager.GetCurrency(currencyName);
        if (currency == null)
        {
            // 如果货币定义不存在，那数量肯定是0
            return 0;
        }

        // 正确调用 CurrencyCollection.GetAmountOf
        return m_CurrencyOwner.CurrencyAmount.GetAmountOf(currency);
    }

    /// <summary>
    /// 检查请求是否有效（组件已初始化，数量为正数）
    /// </summary>
    private bool IsInvalidRequest(string currencyName, int amount, string methodName)
    {
        if (m_CurrencyOwner == null)
        {
            Debug.LogError($"[PlayerCurrencyManager] {methodName} 调用失败：CurrencyOwner 未初始化。");
            return true;
        }
        if (amount <= 0)
        {
            Debug.LogWarning($"[PlayerCurrencyManager] {methodName} 调用失败：数量必须是正数，但收到了 {amount}。");
            return true;
        }
        return false;
    }

    #endregion

}
