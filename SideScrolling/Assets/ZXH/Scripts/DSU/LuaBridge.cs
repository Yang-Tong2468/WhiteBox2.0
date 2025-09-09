using System;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using Opsive.UltimateInventorySystem.Core;
using Opsive.UltimateInventorySystem.Core.DataStructures;
using Opsive.UltimateInventorySystem.Core.InventoryCollections;

public class LuaBridge : MonoBehaviour
{
    private CharacterStats _cachedStats;
    private Inventory _cachedInventory;

    private void Awake() => CachePlayerRefs();

    #region 初始化
    /// <summary>
    /// 初始化时缓存玩家的 CharacterStats 和 Inventory 组件
    /// </summary>
    private void CachePlayerRefs()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("LuaBridge: 找不到带有 'Player' 标签的对象！");
            return;
        }

        _cachedStats = player.GetComponent<CharacterStats>() ?? player.GetComponentInChildren<CharacterStats>(true);
        _cachedInventory = player.GetComponent<Inventory>() ?? player.GetComponentInChildren<Inventory>(true);

        if (_cachedInventory == null) Debug.LogError($"LuaBridge: 在玩家 '{player.name}' 及其子物体上找不到 Inventory 组件！");
        if (_cachedStats == null) Debug.LogWarning($"LuaBridge: 在玩家 '{player.name}' 及其子物体上找不到 CharacterStats 组件（仅属性相关函数需要）。");
    }

    private CharacterStats GetPlayerStats()
    {
        if (_cachedStats == null) CachePlayerRefs();
        return _cachedStats;
    }

    private Inventory GetPlayerInventory()
    {
        if (_cachedInventory == null) CachePlayerRefs();
        return _cachedInventory;
    }
    #endregion


    private void OnEnable()
    {
        Lua.RegisterFunction("GetAttribute", this, SymbolExtensions.GetMethodInfo(() => GetAttribute(null)));
        Lua.RegisterFunction("AddAttribute", this, SymbolExtensions.GetMethodInfo(() => AddAttribute(null, 0d)));
        Lua.RegisterFunction("SetAttribute", this, SymbolExtensions.GetMethodInfo(() => SetAttribute(null, 0d)));

        Lua.RegisterFunction("HasItem", this, SymbolExtensions.GetMethodInfo(() => HasItem(string.Empty)));
        Lua.RegisterFunction("GetItemCount", this, SymbolExtensions.GetMethodInfo(() => GetItemCount(string.Empty)));
        Lua.RegisterFunction("AddItem", this, SymbolExtensions.GetMethodInfo(() => AddItem(string.Empty, 0d)));
    }

    private void OnDisable()
    {
        Lua.UnregisterFunction("GetAttribute");
        Lua.UnregisterFunction("AddAttribute");
        Lua.UnregisterFunction("SetAttribute");

        Lua.UnregisterFunction("HasItem");
        Lua.UnregisterFunction("GetItemCount");
        Lua.UnregisterFunction("AddItem");
    }

    #region 属性相关
    // Lua: GetAttribute("gold") -> number
    public double GetAttribute(string attributeID)
    {
        var stats = GetPlayerStats();
        if (stats == null) return 0d;
        return (double)stats.GetAttributeValueByID(attributeID);
    }

    // Lua: AddAttribute("stamina", -10)
    public void AddAttribute(string attributeID, double delta)
    {
        var stats = GetPlayerStats();
        if (stats == null) return;
        Debug.Log($"LuaBridge: 通过 Lua 修改属性 '{attributeID}'，改变量为 {delta}");
        stats.AddAttributeByID(attributeID, (float)delta);
    }

    // Lua: SetAttribute("level", 5)
    public void SetAttribute(string attributeID, double value)
    {
        var stats = GetPlayerStats();
        if (stats == null) return;
        stats.SetAttributeBaseByID(attributeID, (float)value);
    }
    #endregion

    #region 物品相关
    // Lua: HasItem("Health Potion") -> boolean
    public bool HasItem(string itemName)
    {
        var inventory = GetPlayerInventory();
        var def = GetItemDefinition(itemName);
        if (inventory == null || def == null) return false;

        return inventory.HasItem((1, def));
    }

    // Lua: GetItemCount("Arrow") -> number
    public double GetItemCount(string itemName)
    {
        var inventory = GetPlayerInventory();
        var def = GetItemDefinition(itemName);
        if (inventory == null || def == null) return 0d;

        // 汇总所有集合中的数量，避免遗漏
        long total = 0;
        var all = inventory.AllItemInfos;
        if (all != null)
        {
            for (int i = 0; i < all.Count; i++)
            {
                var info = all[i];
                if (info.Item != null && info.Item.ItemDefinition == def)
                {
                    total += info.Amount; // Amount 为整型
                }
            }
        }
        return (double)total;
    }

    // Lua: AddItem("Gold Coin", 100) / AddItem("Health Potion", -1)
    public void AddItem(string itemName, double amount)
    {
        var inventory = GetPlayerInventory();
        if (inventory == null) return;

        int amt = (int)Math.Round(amount);
        if (amt == 0) return;

        if (amt > 0)
        {
            var def = GetItemDefinition(itemName);
            if (def == null) return;

            inventory.AddItem(def, amt);
            Debug.Log($"LuaBridge: 通过 Lua 向仓库添加了 {amt} 个 '{itemName}'。");
        }
        else // amt < 0 -> 移除
        {
            // 文档列出：按名字移除
            inventory.RemoveItem(itemName, -amt);
            Debug.Log($"LuaBridge: 通过 Lua 从仓库移除了 {-amt} 个 '{itemName}'。");
        }
    }

    private ItemDefinition GetItemDefinition(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        var def = InventorySystemManager.GetItemDefinition(itemName);
        if (def == null) Debug.LogWarning($"LuaBridge: 在数据库中找不到物品定义 '{itemName}'");
        return def;
    }
    #endregion
}
