// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// conditions found in the LICENSE file.
// 
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR
// CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN
// AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE.

using System;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("up_gr")]
    public class UpgradePacket
    {
        #region Properties

        public InventoryType InventoryType { get; set; }

        public InventoryType? InventoryType2 { get; set; }

        public byte Slot { get; set; }

        public byte? Slot2 { get; set; }

        public byte UpgradeType { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            UpgradePacket packetDefinition = new UpgradePacket();
            if (byte.TryParse(packetSplit[2], out byte upgradeType)
                && Enum.TryParse(packetSplit[3], out InventoryType inventoryType)
                && byte.TryParse(packetSplit[4], out byte slot))
            {
                packetDefinition.UpgradeType = upgradeType;
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.InventoryType2 = packetSplit.Length >= 6
                    && Enum.TryParse(packetSplit[5], out InventoryType inventoryType2) ? inventoryType2 : (InventoryType?)null;
                packetDefinition.Slot2 = packetSplit.Length >= 7
                    && byte.TryParse(packetSplit[6], out byte slot2) ? slot2 : (byte?)null;
                packetDefinition.ExecuteHandler(session as ClientSession, packet);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UpgradePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session, string originalPacket)
        {
            if (session.Character.ExchangeInfo?.ExchangeList.Count > 0
                || session.Character.Speed == 0 || session.Character.LastDelay.AddSeconds(5) > DateTime.UtcNow)
            {
                return;
            }

            session.Character.LastDelay = DateTime.UtcNow;
            ItemInstance inventory;
            switch (UpgradeType)
            {
                case 0:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);

                    if (inventory == null)
                    {
                        break;
                    }

                    if (inventory.Item.IsHeroic)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_CONVERT_HEROIC")));
                        session.SendPacket("shop_end 1");
                        break;
                    }

                    const short donaVnum = 1027;
                    var price = 2300 + (inventory.Item.LevelMinimum - 1) * 300;

                    if (inventory.Item.EquipmentSlot != EquipmentType.Armor && inventory.Item.EquipmentSlot != EquipmentType.MainWeapon)
                    {
                        // Not a good item type
                        session.SendPacket("shop_end 1");
                        break;
                    }

                    if (session.Character.Gold < price)
                    {
                        // Not enough gold
                        session.SendPacket("shop_end 1");
                        return;
                    }

                    if (session.Character.Inventory.CountItem(donaVnum) < inventory.Item.LevelMinimum)
                    {
                        // Not enough dona
                        session.SendPacket("shop_end 1");
                        return;
                    }

                    ItemInstance newItem = inventory.HardItemCopy();
                    switch (inventory.Item.EquipmentSlot)
                    {
                        case EquipmentType.Armor:
                            switch (inventory.Item.Class)
                            {
                                case 4:
                                    newItem.ItemVNum = 996;
                                    break;
                                case 2:
                                    newItem.ItemVNum = 997;
                                    break;
                                case 8:
                                    newItem.ItemVNum = 995;
                                    break;
                                default:
                                    session.SendPacket("shop_end 1");
                                    return;
                            }

                            break;
                        case EquipmentType.MainWeapon:
                            switch (inventory.Item.Class)
                            {
                                case 4:
                                    newItem.ItemVNum = 991;
                                    break;
                                case 2:
                                    newItem.ItemVNum = 990;
                                    break;
                                case 8:
                                    newItem.ItemVNum = 992;
                                    break;
                                default:
                                    session.SendPacket("shop_end 1");
                                    return;
                            }

                            break;
                        default:
                            session.SendPacket("shop_end 1");
                            return;
                    }

                    session.Character.Inventory.DeleteFromSlotAndType(Slot, InventoryType);
                    session.Character.Inventory.AddToInventory(newItem, InventoryType.Equipment);
                    session.Character.Inventory.RemoveItemAmount(donaVnum, inventory.Item.LevelMinimum);
                    session.Character.Gold -= price;
                    session.SendPacket(session.Character.GenerateGold());
                    session.SendPacket("shop_end 1");
                    break;

                case 1:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (inventory != null && (inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                    {
                        inventory.UpgradeItem(session, UpgradeMode.Normal, UpgradeProtection.None);
                    }
                    break;

                case 3:

                    // check if that cannot be changed
                    //up_gr 3 0 0 7 1 1 20 99
                    string[] originalSplit = originalPacket.Split(' ');
                    if (originalSplit.Length == 10
                        && byte.TryParse(originalSplit[5], out byte firstSlot)
                        && byte.TryParse(originalSplit[8], out byte secondSlot))
                    {
                        inventory = session.Character.Inventory.LoadBySlotAndType(firstSlot, InventoryType.Equipment);
                        if (inventory != null
                            && (inventory.Item.EquipmentSlot == EquipmentType.Necklace
                             || inventory.Item.EquipmentSlot == EquipmentType.Bracelet
                             || inventory.Item.EquipmentSlot == EquipmentType.Ring)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            ItemInstance cellon =
                                session.Character.Inventory.LoadBySlotAndType<ItemInstance>(secondSlot,
                                    InventoryType.Main);
                            if (cellon?.ItemVNum > 1016 && cellon.ItemVNum < 1027)
                            {
                                inventory.OptionItem(session, cellon.ItemVNum);
                            }
                        }
                    }
                    break;

                case 7:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (inventory != null)
                    {
                        if ((inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            inventory.RarifyItem(session, RarifyMode.Normal, RarifyProtection.None);
                        }

                        session.SendPacket("shop_end 1");
                    }
                    break;

                case 8:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (InventoryType2.HasValue && Slot2.HasValue)
                    {
                        ItemInstance inventory2 =
                            session.Character.Inventory.LoadBySlotAndType(Slot2.Value,
                                InventoryType2.Value);

                        if (inventory != null && inventory2 != null && !Equals(inventory, inventory2))
                        {
                            inventory.Sum(session, inventory2);
                        }
                    }
                    break;

                case 9:
                    ItemInstance specialist = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(session, UpgradeProtection.None);
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 20:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (inventory != null && (inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                    {
                        inventory.UpgradeItem(session, UpgradeMode.Normal, UpgradeProtection.Protected);
                    }
                    break;

                case 21:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (inventory != null && (inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                    {
                        inventory.RarifyItem(session, RarifyMode.Normal, RarifyProtection.Scroll);
                    }
                    break;

                case 25:
                    specialist = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(session, UpgradeProtection.Protected);
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 26:
                    specialist = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(session, UpgradeProtection.Protected);
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 35:
                case 38:
                case 42:
                    specialist = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if(specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(session, UpgradeProtection.Event);
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 41:
                    specialist = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.PerfectSp(session);
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 43:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                    if (inventory != null && (inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                    {
                        inventory.UpgradeItem(session, UpgradeMode.Reduced, UpgradeProtection.Protected);
                    }
                    break;
            }
        }

        #endregion
    }
}