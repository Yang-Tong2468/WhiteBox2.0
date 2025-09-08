/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
/// This is a custom derivative of the MoveToCollectionItemAction script.
/// It has been modified to only move a single item from a stack when equipping.
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.ItemActions
{
    using Opsive.UltimateInventorySystem.Core.DataStructures;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using UnityEngine;

    /// <summary>
    /// Custom Item action used to Move a single item from a stack to another collection.
    /// It can be used to equip/unequip items. When equipping from a stack, only one is equipped.
    /// </summary>
    [System.Serializable]
    public class EquipSingleItemAction : ItemAction
    {
        [Tooltip("The main item collection (usually the inventory).")]
        [SerializeField] protected ItemCollectionID m_FirstCollectionID = new ItemCollectionID(null, ItemCollectionPurpose.Main);
        [Tooltip("The target item collection (usually the equipped collection).")]
        [SerializeField] protected ItemCollectionID m_SecondCollectionID = new ItemCollectionID(null, ItemCollectionPurpose.Equipped);
        [Tooltip("The action name when moving from inventory to equipment.")]
        [SerializeField] protected string m_MoveFromFirstToSecondActionName = "Equip";
        [Tooltip("The action name when moving from equipment to inventory.")]
        [SerializeField] protected string m_MoveFromSecondToFirstActionName = "Unequip";

        protected bool m_MoveFromFirstToSecond;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public EquipSingleItemAction()
        {
            m_Name = "Equip"; // Default name
        }

        /// <summary>
        /// Check if the action can be invoked.
        /// </summary>
        protected override bool CanInvokeInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var item = itemInfo.Item;
            var inventory = itemInfo.Inventory;

            if (inventory == null) { return false; }

            var secondCollection = inventory.GetItemCollection(m_SecondCollectionID);
            if (secondCollection == null) { return false; }

            // Check if the item is already in the second collection to determine the action name (Equip/Unequip)
            if (secondCollection.HasItem((1, item)))
            {
                m_MoveFromFirstToSecond = false;
                m_Name = m_MoveFromSecondToFirstActionName;
            }
            else
            {
                m_MoveFromFirstToSecond = true;
                m_Name = m_MoveFromFirstToSecondActionName;
            }

            return true;
        }

        /// <summary>
        /// Move an item from one collection to another.
        /// </summary>
        protected override void InvokeActionInternal(ItemInfo itemInfo, ItemUser itemUser)
        {
            var inventory = itemInfo.Inventory;
            var firstCollection = inventory.GetItemCollection(m_FirstCollectionID);
            var secondCollection = inventory.GetItemCollection(m_SecondCollectionID);

            var originalCollection = m_MoveFromFirstToSecond ? firstCollection : secondCollection;
            var destinationCollection = m_MoveFromFirstToSecond ? secondCollection : firstCollection;

            // --- CUSTOM LOGIC START ---
            // If we are equipping (moving from first to second collection) and the stack is greater than 1
            if (m_MoveFromFirstToSecond && itemInfo.Amount > 1)
            {

                // Define the single item to be moved
                ItemInfo singleItemToMove = (1, itemInfo.Item, itemInfo.ItemCollection);

                // Attempt to remove just one item from the original collection
                var removedItem = originalCollection.RemoveItem(singleItemToMove);
                if (removedItem.Amount == 0)
                {
                    Debug.LogWarning("Failed to remove single item from stack.");
                    return; // Removal failed
                }

                // Attempt to add that single item to the destination
                var addedItem = destinationCollection.AddItem(removedItem);

                // If adding failed, return the item to the original collection
                if (addedItem.Amount == 0)
                {
                    originalCollection.AddItem(removedItem);
                    Debug.LogWarning("Failed to add single item to destination, returning to source.");
                }

                // The action is complete for a single item, so we exit the function.
                return;
            }
            // --- CUSTOM LOGIC END ---

            // --- ORIGINAL LOGIC ---
            // If we are unequipping, or if the item stack is just 1, run the original logic to move the whole ItemInfo.
            var originalItem = originalCollection.RemoveItem(itemInfo);
            var movedItemInfo = ItemInfo.None;

            // This part handles swapping items in equipment slots correctly
            if (destinationCollection is ItemSlotCollection itemSlotCollection)
            {
                var slotIndex = itemSlotCollection.GetTargetSlotIndex(originalItem.Item);
                if (slotIndex != -1)
                {
                    var previousItemInSlot = itemSlotCollection.GetItemInfoAtSlot(slotIndex);

                    if (previousItemInSlot.Item != null)
                    {
                        // Don't swap if it's the same kind of stackable item.
                        if (previousItemInSlot.Item.StackableEquivalentTo(originalItem.Item))
                        {
                            previousItemInSlot = ItemInfo.None;
                        }
                        else
                        {
                            previousItemInSlot = itemSlotCollection.RemoveItem(slotIndex);
                        }
                    }

                    movedItemInfo = itemSlotCollection.AddItem(originalItem, slotIndex);

                    if (previousItemInSlot.Item != null)
                    {
                        firstCollection.AddItem(previousItemInSlot);
                    }
                }
            }
            else
            {
                movedItemInfo = destinationCollection.AddItem(originalItem);
            }

            // If not all items were moved, return the remainder to the default collection.
            if (movedItemInfo.Amount != originalItem.Amount)
            {
                var amountToReturn = originalItem.Amount - movedItemInfo.Amount;
                firstCollection.AddItem((ItemInfo)(amountToReturn, originalItem));
            }
        }
    }
}