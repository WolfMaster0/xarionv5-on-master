// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core.Threading;
using OpenNos.GameLog.LogHelper;

namespace OpenNos.GameObject
{
    public class Inventory : ThreadSafeSortedList<Guid, ItemInstance>
    {
        #region Members

        private const short DefaultBackpackSize = 48;

        private const byte MaxItemAmount = 255;

        private readonly object _lockObject = new object();

        #endregion

        #region Instantiation

        public Inventory(Character character) => Character = character;

        #endregion

        #region Properties

        private Character Character { get; }

        #endregion

        #region Methods

        public static ItemInstance InstantiateItemInstance(short vnum, long ownerId, byte amount = 1)
        {
            ItemInstance newItem = new ItemInstance { ItemVNum = vnum, Amount = amount, CharacterId = ownerId };
            if (newItem.Item != null)
            {
                switch (newItem.Item.Type)
                {
                    case InventoryType.Miniland:
                        newItem.DurabilityPoint = newItem.Item.MinilandObjectPoint / 2;
                        break;

                    case InventoryType.Equipment:
                        newItem = newItem.Item.ItemType == ItemType.Specialist ? new ItemInstance
                        {
                            ItemVNum = vnum,
                            SpLevel = 1,
                            Amount = amount
                        } : new ItemInstance
                        {
                            ItemVNum = vnum,
                            Amount = amount
                        };
                        break;
                }
            }

            // set default itemType
            if (newItem.Item != null)
            {
                newItem.Type = newItem.Item.Type;
            }

            return newItem;
        }

        public ItemInstance AddIntoBazaarInventory(InventoryType inventory, byte slot, byte amount)
        {
            ItemInstance inv = LoadBySlotAndType(slot, inventory);
            if (inv == null || amount > inv.Amount || amount == 0)
            {
                return null;
            }

            if (inv.Item.Type == InventoryType.Equipment)
            {
                for (short i = 0; i < 255; i++)
                {
                    if (LoadBySlotAndType<ItemInstance>(i, InventoryType.Bazaar) == null)
                    {
                        inv.Type = InventoryType.Bazaar;
                        inv.Slot = i;
                        inv.CharacterId = Character.CharacterId;
                        DeleteFromSlotAndType(inv.Slot, inv.Type);
                        PutItem(inv);
                        break;
                    }
                }
                Character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));
                return inv;
            }
            if (amount == inv.Amount)
            {
                for (short i = 0; i < 255; i++)
                {
                    if (LoadBySlotAndType<ItemInstance>(i, InventoryType.Bazaar) == null)
                    {
                        inv.Type = InventoryType.Bazaar;
                        inv.Slot = i;
                        inv.CharacterId = Character.CharacterId;
                        DeleteFromSlotAndType(inv.Slot, inv.Type);
                        PutItem(inv);
                        break;
                    }
                }
                Character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));
                return inv;
            }

            ItemInstance invClone = inv.DeepCopy();
            invClone.Id = Guid.NewGuid();
            invClone.Amount = amount;
            inv.Amount -= amount;

            for (short i = 0; i < 255; i++)
            {
                if (LoadBySlotAndType<ItemInstance>(i, InventoryType.Bazaar) == null)
                {
                    invClone.Type = InventoryType.Bazaar;
                    invClone.Slot = i;
                    invClone.CharacterId = Character.CharacterId;
                    PutItem(invClone);
                    break;
                }
            }

            Character.Session.SendPacket(inv.GenerateInventoryAdd());
            return invClone;
        }

        public List<ItemInstance> AddNewToInventory(short vnum, byte amount = 1, InventoryType? type = null, sbyte rare = 0, byte upgrade = 0, byte design = 0)
        {
            ItemInstance newItem = InstantiateItemInstance(vnum, Character.CharacterId, amount);
            newItem.Rare = rare;
            newItem.Upgrade = upgrade == 0 ? newItem.Item.ItemType == ItemType.Shell ? (byte)ServerManager.RandomNumber(90, 90) : upgrade : upgrade;
            newItem.Design = design;
            return AddToInventory(newItem, type);
        }

        public List<ItemInstance> AddToInventory(ItemInstance newItem, InventoryType? type = null)
        {
            List<ItemInstance> invlist = new List<ItemInstance>();

            // override type if necessary
            if (type.HasValue)
            {
                newItem.Type = type.Value;
            }

            if (newItem.Item.Effect == 420 && newItem.Item.EffectValue == 911)
            {
                newItem.BoundCharacterId = Character.CharacterId;
                newItem.DurabilityPoint = (int)newItem.Item.ItemValidTime;
            }

            // check if item can be stapled
            if (newItem.Type != InventoryType.Bazaar && (newItem.Item.Type == InventoryType.Etc || newItem.Item.Type == InventoryType.Main))
            {
                int backpackSize = BackpackSize();
                List<ItemInstance> slotNotFull = Where(i => i.Type != InventoryType.Bazaar && i.Type != InventoryType.PetWarehouse && i.Type != InventoryType.Warehouse && i.Type != InventoryType.FamilyWareHouse && i.ItemVNum.Equals(newItem.ItemVNum) && i.Amount < MaxItemAmount && i.Slot < backpackSize);
                int freeslot = backpackSize - CountLinq(s => s.Type == newItem.Type && s.Slot < backpackSize);
                if (newItem.Amount <= (freeslot * MaxItemAmount) + slotNotFull.Sum(s => MaxItemAmount - s.Amount))
                {
                    foreach (ItemInstance slot in slotNotFull)
                    {
                        int max = slot.Amount + newItem.Amount;
                        max = max > MaxItemAmount ? MaxItemAmount : max;
                        newItem.Amount = (byte)(slot.Amount + newItem.Amount - max);
                        GameLogger.Instance.LogItemCreate(ServerManager.Instance.ChannelId, Character.Name,
                            Character.CharacterId, newItem, Character.MapInstance?.Map.MapId ?? -1, Character.PositionX,
                            Character.PositionY);
                        slot.Amount = (byte)max;
                        invlist.Add(slot);
                        Character.Session?.SendPacket(slot.GenerateInventoryAdd());
                    }
                }
            }
            if (newItem.Amount > 0)
            {
                // create new item
                short? freeSlot = newItem.Type == InventoryType.Wear ? (LoadBySlotAndType((short)newItem.Item.EquipmentSlot, InventoryType.Wear) == null
                                                                     ? (short?)newItem.Item.EquipmentSlot
                                                                     : null)
                                                                     : GetFreeSlot(newItem.Type);
                if (freeSlot.HasValue)
                {
                    invlist.Add(AddToInventoryWithSlotAndType(newItem, newItem.Type, freeSlot.Value));
                }
            }
            return invlist;
        }

        /// <summary>
        /// Add iteminstance to inventory with specified slot and type, iteminstance will be overridden.
        /// </summary>
        /// <param name="itemInstance"></param>
        /// <param name="type"></param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public ItemInstance AddToInventoryWithSlotAndType(ItemInstance itemInstance, InventoryType type, short slot)
        {
            GameLogger.Instance.LogItemCreate(ServerManager.Instance.ChannelId, Character.Name,
                Character.CharacterId, itemInstance, Character.MapInstance?.Map.MapId ?? -1, Character.PositionX,
                Character.PositionY);

            itemInstance.Slot = slot;
            itemInstance.Type = type;
            itemInstance.CharacterId = Character.CharacterId;

            if (ContainsKey(itemInstance.Id))
            {
                Logger.Error(new InvalidOperationException("Cannot add the same ItemInstance twice to inventory."));
                return null;
            }

            string inventoryPacket = itemInstance.GenerateInventoryAdd();
            if (!string.IsNullOrEmpty(inventoryPacket))
            {
                Character.Session?.SendPacket(inventoryPacket);
            }

            if (Any(s => s.Slot == slot && s.Type == type))
            {
                return null;
            }
            this[itemInstance.Id] = itemInstance;
            return itemInstance;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public int BackpackSize() => DefaultBackpackSize + (Character.HaveBackpack() ? 12 : 0);

        public bool CanAddItem(short itemVnum) => CanAddItem(ServerManager.GetItem(itemVnum).Type);

        public int CountItem(int itemVNum) => Where(s => s.ItemVNum == itemVNum && s.Type != InventoryType.FamilyWareHouse && s.Type != InventoryType.Bazaar && s.Type != InventoryType.Warehouse && s.Type != InventoryType.PetWarehouse).Sum(i => i.Amount);

        public int CountItemInAnInventory(InventoryType inv) => CountLinq(s => s.Type == inv);

        public Tuple<short, InventoryType> DeleteById(Guid id)
        {
            Tuple<short, InventoryType> removedPlace;
            ItemInstance inv = this[id];

            if (inv != null)
            {
                removedPlace = new Tuple<short, InventoryType>(inv.Slot, inv.Type);
                Remove(inv.Id);
            }
            else
            {
                Logger.Error(new InvalidOperationException("Expected item wasn't deleted, Type or Slot did not match!"));
                return null;
            }

            return removedPlace;
        }

        public void DeleteFromSlotAndType(short slot, InventoryType type)
        {
            ItemInstance inv = FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));

            if (inv != null)
            {
                if (Character.Session.Character.MinilandObjects.Any(s => s.ItemInstanceId == inv.Id))
                {
                    return;
                }

                Remove(inv.Id);
            }
            else
            {
                Logger.Error(new InvalidOperationException("Expected item wasn't deleted, Type or Slot did not match!"));
            }
        }

        public bool EnoughPlace(List<ItemInstance> itemInstances)
        {
            Dictionary<InventoryType, int> place = new Dictionary<InventoryType, int>();
            foreach (IGrouping<short, ItemInstance> itemgroup in itemInstances.GroupBy(s => s.ItemVNum))
            {
                if (itemgroup.FirstOrDefault()?.Type is InventoryType type)
                {
                    List<ItemInstance> listitem = Where(i => i.Type == type);
                    if (!place.ContainsKey(type))
                    {
                        place.Add(type, (type != InventoryType.Miniland ? BackpackSize() : 50) - listitem.Count);
                    }

                    int amount = itemgroup.Sum(s => s.Amount);
                    int rest = amount % (type == InventoryType.Equipment ? 1 : 99);
                    bool needanotherslot = listitem.Where(s => s.ItemVNum == itemgroup.Key).Sum(s => MaxItemAmount - s.Amount) <= rest;
                    place[type] -= (amount / (type == InventoryType.Equipment ? 1 : 99)) + (needanotherslot ? 1 : 0);

                    if (place[type] < 0)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void FDepositItem(InventoryType inventory, byte slot, byte amount, byte newSlot, ref ItemInstance item, ref ItemInstance itemdest)
        {
            if (item != null && amount <= item.Amount && amount > 0 && item.Item.IsTradable && !item.IsBound)
            {
                FamilyCharacter fhead = Character.Family?.FamilyCharacters.Find(s => s.Authority == FamilyAuthority.Head);
                if (fhead == null)
                {
                    return;
                }
                MoveItem(inventory, InventoryType.FamilyWareHouse, slot, amount, newSlot, out item, out itemdest);
                itemdest.CharacterId = fhead.CharacterId;
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(itemdest);
                Character.Session.SendPacket(item != null ? item.GenerateInventoryAdd() : UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory, slot));
                if (itemdest != null)
                {
                    Character.Session.SendPacket(itemdest.GenerateFStash());
                    Character.Family?.InsertFamilyLog(FamilyLogType.WareHouseAdded, Character.Name, message: $"{itemdest.ItemVNum}|{amount}");
                    DeleteById(itemdest.Id);
                }
            }
        }

        public ItemInstance GetItemInstanceById(Guid id) => this[id];

        public ItemInstance LoadBySlotAndType(short slot, InventoryType type)
        {
            ItemInstance retItem = null;
            try
            {
                lock (_lockObject)
                {
                    retItem = SingleOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
                }
            }
            catch (InvalidOperationException ioEx)
            {
                Logger.LogUserEventError(nameof(LoadBySlotAndType), Character.Session?.GenerateIdentity(), "Multiple items in slot, Splitting...", ioEx);
                bool isFirstItem = true;
                foreach (ItemInstance item in Where(i => i.Slot.Equals(slot) && i.Type.Equals(type)))
                {
                    if (isFirstItem)
                    {
                        retItem = item;
                        isFirstItem = false;
                        continue;
                    }
                    ItemInstance itemInstance = FirstOrDefault(i => i.Slot.Equals(slot) && i.Type.Equals(type));
                    if (itemInstance != null)
                    {
                        short? freeSlot = GetFreeSlot(type);
                        if (freeSlot.HasValue)
                        {
                            itemInstance.Slot = freeSlot.Value;
                        }
                        else
                        {
                            Remove(itemInstance.Id);
                        }
                    }
                }
            }
            return retItem;
        }

        public T LoadBySlotAndType<T>(short slot, InventoryType type) where T : ItemInstance
        {
            T retItem = null;
            try
            {
                lock (_lockObject)
                {
                    retItem = (T)SingleOrDefault(i => i?.GetType() == typeof(T) && i.Slot == slot && i.Type == type);
                }
            }
            catch (InvalidOperationException ioEx)
            {
                Logger.LogUserEventError(nameof(LoadBySlotAndType), Character.Session?.GenerateIdentity(), "Multiple items in slot, Splitting...", ioEx);
                bool isFirstItem = true;
                foreach (ItemInstance item in Where(i => i?.GetType() == typeof(T) && i.Slot == slot && i.Type == type))
                {
                    if (isFirstItem)
                    {
                        retItem = (T)item;
                        isFirstItem = false;
                        continue;
                    }
                    ItemInstance itemInstance = FirstOrDefault(i => i?.GetType() == typeof(T) && i.Slot == slot && i.Type == type);
                    if (itemInstance != null)
                    {
                        short? freeSlot = GetFreeSlot(type);
                        if (freeSlot.HasValue)
                        {
                            itemInstance.Slot = freeSlot.Value;
                        }
                        else
                        {
                            Remove(itemInstance.Id);
                        }
                    }
                }
            }
            return retItem;
        }

        /// <summary>
        /// Moves one item from one <see cref="Inventory"/> to another. Example: Equipment &lt;-&gt;
        /// Wear, Equipment &lt;-&gt; Costume, Equipment &lt;-&gt; Specialist
        /// </summary>
        /// <param name="sourceSlot"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="targetSlot"></param>
        /// <param name="wear"></param>
        public ItemInstance MoveInInventory(short sourceSlot, InventoryType sourceType, InventoryType targetType, short? targetSlot = null, bool wear = true)
        {
            ItemInstance sourceInstance = LoadBySlotAndType(sourceSlot, sourceType);

            if (sourceInstance == null && wear)
            {
                Logger.Error(new InvalidOperationException("SourceInstance to move does not exist."));
                return null;
            }
            if (sourceInstance != null)
            {
                if (targetSlot.HasValue)
                {
                    if (wear)
                    {
                        // swap
                        ItemInstance targetInstance = LoadBySlotAndType(targetSlot.Value, targetType);

                        sourceInstance.Slot = targetSlot.Value;
                        sourceInstance.Type = targetType;

                        targetInstance.Slot = sourceSlot;
                        targetInstance.Type = sourceType;
                    }
                    else
                    {
                        // move source to target
                        short? freeTargetSlot = GetFreeSlot(targetType);
                        if (freeTargetSlot.HasValue)
                        {
                            sourceInstance.Slot = freeTargetSlot.Value;
                            sourceInstance.Type = targetType;
                        }
                    }

                    return sourceInstance;
                }

                // check for free target slot
                short? nextFreeSlot;

                if (targetType >= InventoryType.FirstPartnerInventory && targetType <= InventoryType.TwelvthPartnerInventory || targetType == InventoryType.Wear)
                {
                    nextFreeSlot = (LoadBySlotAndType((short)sourceInstance.Item.EquipmentSlot, targetType) == null
                        ? (short)sourceInstance.Item.EquipmentSlot
                        : (short)-1);
                }
                else
                {
                    nextFreeSlot = GetFreeSlot(targetType);
                }

                if (nextFreeSlot.HasValue)
                {
                    sourceInstance.Type = targetType;
                    sourceInstance.Slot = nextFreeSlot.Value;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            return sourceInstance;
        }

        public void MoveItem(InventoryType sourcetype, InventoryType desttype, short sourceSlot, byte amount, short destinationSlot, out ItemInstance sourceInventory, out ItemInstance destinationInventory)
        {
            // Load source and destination slots
            sourceInventory = LoadBySlotAndType(sourceSlot, sourcetype);
            destinationInventory = LoadBySlotAndType(destinationSlot, desttype);

            GameLogger.Instance.LogItemMove(ServerManager.Instance.ChannelId, Character.Name, Character.CharacterId,
                sourceInventory, amount, desttype, destinationSlot);
            if (sourceInventory != null && amount <= sourceInventory.Amount)
            {
                if (destinationInventory == null)
                {
                    if (sourceInventory.Amount == amount)
                    {
                        sourceInventory.Slot = destinationSlot;
                        sourceInventory.Type = desttype;
                    }
                    else
                    {
                        ItemInstance itemDest = sourceInventory.DeepCopy();
                        sourceInventory.Amount -= amount;
                        itemDest.Amount = amount;
                        itemDest.Type = desttype;
                        itemDest.Id = Guid.NewGuid();
                        AddToInventoryWithSlotAndType(itemDest, desttype, destinationSlot);
                    }
                }
                else
                {
                    if (destinationInventory.ItemVNum == sourceInventory.ItemVNum && (byte)sourceInventory.Item.Type != 0)
                    {
                        if (destinationInventory.Amount + amount > MaxItemAmount)
                        {
                            int saveItemCount = destinationInventory.Amount;
                            destinationInventory.Amount = MaxItemAmount;
                            sourceInventory.Amount = (byte)(saveItemCount + sourceInventory.Amount - MaxItemAmount);
                        }
                        else
                        {
                            destinationInventory.Amount += amount;
                            sourceInventory.Amount -= amount;

                            // item with amount of 0 should be removed
                            if (sourceInventory.Amount == 0)
                            {
                                DeleteFromSlotAndType(sourceInventory.Slot, sourceInventory.Type);
                            }
                        }
                    }
                    else
                    {
                        // add and remove save inventory
                        destinationInventory = TakeItem(destinationInventory.Slot, destinationInventory.Type);
                        if (destinationInventory == null)
                        {
                            return;
                        }

                        destinationInventory.Slot = sourceSlot;
                        destinationInventory.Type = sourcetype;
                        sourceInventory = TakeItem(sourceInventory.Slot, sourceInventory.Type);
                        if (sourceInventory == null)
                        {
                            return;
                        }

                        sourceInventory.Slot = destinationSlot;
                        sourceInventory.Type = desttype;
                        PutItem(destinationInventory);
                        PutItem(sourceInventory);
                    }
                }
            }
            sourceInventory = LoadBySlotAndType(sourceSlot, sourcetype);
            destinationInventory = LoadBySlotAndType(destinationSlot, desttype);
        }

        public void RemoveItemAmount(int vnum, int amount = 1)
        {
            int remainingAmount = amount;
            foreach (ItemInstance inventory in Where(s => s.ItemVNum == vnum && s.Type != InventoryType.Wear && s.Type != InventoryType.Bazaar && s.Type != InventoryType.Warehouse && s.Type != InventoryType.PetWarehouse && s.Type != InventoryType.FamilyWareHouse).OrderBy(i => i.Slot))
            {
                if (remainingAmount > 0)
                {
                    if (inventory.Amount > remainingAmount)
                    {
                        // Amount completely removed
                        inventory.Amount -= (byte)remainingAmount;
                        remainingAmount = 0;
                        Character.Session.SendPacket(inventory.GenerateInventoryAdd());
                    }
                    else
                    {
                        // Amount partly removed
                        remainingAmount -= inventory.Amount;
                        DeleteById(inventory.Id);
                        Character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory.Type, inventory.Slot));
                    }
                }
                else
                {
                    // Amount to remove reached
                    break;
                }
            }
        }

        public void DeleteByVNum(short vNum)
        {
            foreach (ItemInstance inventory in Where(s => s.ItemVNum == vNum))
            {
                DeleteById(inventory.Id);
                Character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inventory.Type, inventory.Slot));
            }
        }

        public void RemoveItemFromInventory(Guid id, byte amount = 1)
        {
            ItemInstance inv = FirstOrDefault(i => i.Id.Equals(id));
            if (inv != null)
            {
                inv.Amount -= amount;
                if (inv.Amount <= 0)
                {
                    Character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inv.Type, inv.Slot));
                    Remove(inv.Id);
                    return;
                }
                Character.Session.SendPacket(inv.GenerateInventoryAdd());
            }
        }

        /// <summary>
        /// Reorders item in given inventorytype
        /// </summary>
        /// <param name="session"></param>
        /// <param name="inventoryType"></param>
        public void Reorder(ClientSession session, InventoryType inventoryType)
        {
            List<ItemInstance> itemsByInventoryType;
            switch (inventoryType)
            {
                case InventoryType.Costume:
                    itemsByInventoryType = Where(s => s.Type == InventoryType.Costume).OrderBy(s => s.ItemVNum).ToList();
                    break;

                case InventoryType.Specialist:
                    itemsByInventoryType = Where(s => s.Type == InventoryType.Specialist).OrderBy(s => s.Item.LevelJobMinimum).ToList();
                    break;

                default:
                    itemsByInventoryType = Where(s => s.Type == inventoryType).OrderBy(s => s.Item.Price).ToList();
                    break;
            }
            GenerateClearInventory(inventoryType);
            for (short i = 0; i < itemsByInventoryType.Count; i++)
            {
                ItemInstance item = itemsByInventoryType[i];
                item.Slot = i;
                this[item.Id].Slot = i;
                session.SendPacket(item.GenerateInventoryAdd());
            }
        }

        private bool CanAddItem(InventoryType type) => GetFreeSlot(type).HasValue;

        private void GenerateClearInventory(InventoryType type)
        {
            for (short i = 0; i < DefaultBackpackSize; i++)
            {
                if (LoadBySlotAndType(i, type) != null)
                {
                    Character.Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(type, i));
                }
            }
        }

        /// <summary>
        /// Gets free slots in given inventory type
        /// </summary>
        /// <param name="type"></param>
        /// <returns>short?; based on given inventory type</returns>
        public short? GetFreeSlot(InventoryType type)
        {
            IEnumerable<int> itemInstanceSlotsByType = Where(i => i.Type == type).OrderBy(i => i.Slot).Select(i => (int)i.Slot);
            IEnumerable<int> instanceSlotsByType = itemInstanceSlotsByType as int[] ?? itemInstanceSlotsByType.ToArray();
            int backpackSize = BackpackSize();
            int maxRange = (type != InventoryType.Miniland ? backpackSize : 50) + 1;
            int? nextFreeSlot = instanceSlotsByType.Any() ? Enumerable.Range(0, maxRange).Except(instanceSlotsByType).Cast<int?>().FirstOrDefault() : 0;
            return (short?)nextFreeSlot < (type != InventoryType.Miniland ? backpackSize : 50) ? (short?)nextFreeSlot : null;
        }

        /// <summary>
        /// Puts a Single ItemInstance to the Inventory
        /// </summary>
        /// <param name="itemInstance"></param>
        private void PutItem(ItemInstance itemInstance) => this[itemInstance.Id] = itemInstance;

        /// <summary>
        /// Takes a Single Inventory including ItemInstance from the List and removes it.
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private ItemInstance TakeItem(short slot, InventoryType type)
        {
            ItemInstance itemInstance = SingleOrDefault(i => i.Slot == slot && i.Type == type);
            if (itemInstance != null)
            {
                Remove(itemInstance.Id);
                return itemInstance;
            }
            return null;
        }

        #endregion
    }
}