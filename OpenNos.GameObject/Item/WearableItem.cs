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
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using OpenNos.Core.ConcurrencyExtensions;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class WearableItem : Item
    {
        #region Instantiation

        public WearableItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            switch (Effect)
            {
                default:
                    bool delay = false;
                    if (option == 255)
                    {
                        delay = true;
                        option = 0;
                    }
                    Mate mate = null;
                    if (option != 0)
                    {
                        if (session.Character.Mates.Count(s => s.MateType == MateType.Partner) == 1 && option == 2)
                        {
                            option = 1;
                        }

                        mate = session.Character.Mates.Find(s =>
                            s.MateType == MateType.Partner && s.PartnerSlot == (option - 1));

                    }
                    short slot = inv.Slot;
                    InventoryType equipment = InventoryType.Wear;

                    if (option >= 1 && option <= 12)
                    {
                        equipment = (InventoryType)(option + 12); // partner inventories
                    }

                    InventoryType itemToWearType = inv.Type;

                    if (inv == null)
                    {
                        return;
                    }
                    if (ItemValidTime > 0 && !inv.IsBound)
                    {
                        inv.ItemDeleteTime = DateTime.UtcNow.AddSeconds(ItemValidTime);
                    }
                    if (!inv.IsBound)
                    {
                        if (!delay && ((EquipmentSlot == EquipmentType.Fairy && (MaxElementRate == 70 || MaxElementRate == 80)) || EquipmentSlot == EquipmentType.CostumeHat || EquipmentSlot == EquipmentType.CostumeSuit || EquipmentSlot == EquipmentType.WeaponSkin))
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)itemToWearType}^{slot}^1 {Language.Instance.GetMessageFromKey("ASK_BIND")}");
                            return;
                        }
                        if (delay)
                        {
                            inv.BoundCharacterId = session.Character.CharacterId;
                        }
                    }

                    double timeSpanSinceLastSpUsage = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds - session.Character.LastSp;

                    if (EquipmentSlot == EquipmentType.Sp && inv.Rare == -2)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_EQUIP_DESTROYED_SP"), 0));
                        return;
                    }

                    if (option == 0)
                    {
                        if (EquipmentSlot == EquipmentType.Sp && timeSpanSinceLastSpUsage <= session.Character.SpCooldown && session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Specialist) != null)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"), session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage)), 0));
                            return;
                        }

                        if ((ItemType != ItemType.Weapon
                            && ItemType != ItemType.Armor
                            && ItemType != ItemType.Fashion
                            && ItemType != ItemType.Jewelery
                            && ItemType != ItemType.Specialist)
                            || LevelMinimum > (IsHeroic ? session.Character.HeroLevel : session.Character.Level) || (Sex != 0 && Sex != (byte)session.Character.Gender + 1)
                            || (ItemType != ItemType.Jewelery && EquipmentSlot != EquipmentType.Boots && EquipmentSlot != EquipmentType.Gloves && ((Class >> (byte)session.Character.Class) & 1) != 1)
                            || (inv.BoundCharacterId != null && inv.BoundCharacterId != session.Character.CharacterId) )
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                            return;
                        }

                        if (session.Character.UseSp && session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, equipment) is ItemInstance sp && sp.Item.Element != 0 && EquipmentSlot == EquipmentType.Fairy && Element != sp.Item.Element && Element != sp.Item.SecondaryElement)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                            return;
                        }

                        if (session.Character.UseSp && EquipmentSlot == EquipmentType.Sp)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SP_BLOCKED"), 10));
                            return;
                        }

                        if (session.Character.JobLevel < LevelJobMinimum)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_JOB_LVL"), 10));
                            return;
                        }
                    }
                    else if (mate != null)
                    {
                        if (mate.Level < LevelMinimum || mate.Level < inv.MinimumLevel)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                            return;
                        }
                        var isRagnar = mate.NpcMonsterVNum == 2603 || mate.NpcMonsterVNum == 2618;
                        var mateSpType = ServerManager.GetNpcMonster(mate.NpcMonsterVNum).SpecialistType;
                        var itemSpType = ServerManager.GetItem(VNum).SpecialistType;
                        switch (EquipmentSlot)
                        {
                            case EquipmentType.Armor:
                                if (ItemSubType == 4 && !inv.Item.IsHeroic && mateSpType == itemSpType)
                                {
                                    mate.ArmorInstance = inv;
                                    break;
                                }
                                else if (ItemSubType == 4 && isRagnar && !inv.Item.IsHeroic && itemSpType == PartnerSpecialistType.Close)
                                {
                                    mate.ArmorInstance = inv;
                                    break;
                                }
                                else
                                {
                                    goto default;
                                }

                            case EquipmentType.MainWeapon:
                                if (ItemSubType == 12 && !inv.Item.IsHeroic && mateSpType == itemSpType)
                                {
                                    mate.WeaponInstance = inv;
                                    break;
                                }
                                else if (ItemSubType == 4 && isRagnar && !inv.Item.IsHeroic && itemSpType == PartnerSpecialistType.Close)
                                {
                                    mate.WeaponInstance = inv;
                                    break;
                                }
                                else
                                {
                                    goto default;
                                }

                            case EquipmentType.Gloves:
                                if (!inv.Item.IsHeroic)
                                {
                                    mate.GlovesInstance = inv;
                                }
                                else
                                {
                                    goto default;
                                }
                                break;

                            case EquipmentType.Boots:
                                if (!inv.Item.IsHeroic)
                                {
                                    mate.BootsInstance = inv;
                                }
                                else
                                {
                                    goto default;
                                }
                                break;

                            case EquipmentType.Sp:
                                if (ItemSubType == 4 && mateSpType == itemSpType)
                                {
                                    mate.SpInstance = inv;
                                    break;
                                }
                                else if (ItemSubType == 4 && isRagnar && itemSpType == PartnerSpecialistType.RagnarOnly)
                                {
                                    mate.SpInstance = inv;
                                    break;
                                }
                                else
                                {
                                    goto default;
                                }

                            default:
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("BAD_EQUIPMENT"), 10));
                                return;
                        }
                    }

                    ItemInstance currentlyEquippedItem = session.Character.Inventory.LoadBySlotAndType((short)EquipmentSlot, equipment);

                    if (currentlyEquippedItem == null)
                    {
                        // move from equipment to wear
                        session.Character.Inventory.MoveInInventory(inv.Slot, itemToWearType, equipment);
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(itemToWearType, slot));
                    }
                    else
                    {
                        GameLogger.Instance.LogEquipmentUnwear(ServerManager.Instance.ChannelId, session.Character.Name,
                            session.Character.CharacterId, currentlyEquippedItem);

                        // move from wear to equipment and back
                        session.Character.Inventory.MoveInInventory(currentlyEquippedItem.Slot, equipment, itemToWearType, inv.Slot);
                        session.SendPacket(currentlyEquippedItem.GenerateInventoryAdd());
                        session.Character.EquipmentBCards.RemoveAll(o => o.ItemVNum == currentlyEquippedItem.ItemVNum);
                    }

                    GameLogger.Instance.LogEquipmentWear(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, inv);

                    session.Character.EquipmentBCards.AddRange(inv.Item.BCards);

                    switch (inv.Item.ItemType)
                    {
                        case ItemType.Armor:
                            session.Character.ShellEffectArmor.Clear();

                            foreach (ShellEffectDTO dto in inv.ShellEffects)
                            {
                                session.Character.ShellEffectArmor.Add(dto);
                            }
                            break;
                        case ItemType.Weapon:
                            switch (inv.Item.EquipmentSlot)
                            {
                                case EquipmentType.MainWeapon:
                                    session.Character.ShellEffectMain.Clear();

                                    foreach (ShellEffectDTO dto in inv.ShellEffects)
                                    {
                                        session.Character.ShellEffectMain.Add(dto);
                                    }
                                    break;

                                case EquipmentType.SecondaryWeapon:
                                    session.Character.ShellEffectSecondary.Clear();

                                    foreach (ShellEffectDTO dto in inv.ShellEffects)
                                    {
                                        session.Character.ShellEffectSecondary.Add(dto);
                                    }
                                    break;
                            }
                            break;
                    }

                    if (option == 0)
                    {
                        session.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMapInstance?.Broadcast(session.Character.GeneratePairy());

                        if (EquipmentSlot == EquipmentType.Fairy)
                        {
                            ItemInstance fairy = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, equipment);
                            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("FAIRYSTATS"), fairy.XP, CharacterHelper.LoadFairyXPData(fairy.ElementRate + fairy.Item.ElementRate)), 10));
                        }

                        if (EquipmentSlot == EquipmentType.Amulet)
                        {
                            session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 39));
                            inv.BoundCharacterId = session.Character.CharacterId;
                        }
                    }
                    else if (mate != null)
                    {
                        session.SendPacket(mate.GenerateScPacket());
                    }
                    break;
            }
        }

        #endregion
    }
}