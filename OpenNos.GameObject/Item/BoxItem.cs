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
using System.Collections.Generic;
using System.Linq;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.GameObject
{
    public class BoxItem : Item
    {
        #region Instantiation

        public BoxItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0,
            string[] packetsplit = null)
        {
            switch (Effect)
            {
                case 0:
                    if (option == 0)
                    {
                        if (packetsplit?.Length == 9)
                        {
                            ItemInstance box =
                                session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot,
                                    InventoryType.Equipment);
                            if (box != null)
                            {
                                if (box.Item.ItemSubType == 3)
                                {
                                    session.SendPacket(
                                        $"qna #guri^300^8023^{inv.Slot} {Language.Instance.GetMessageFromKey("ASK_OPEN_BOX")}");
                                }
                                else if (box.HoldingVNum == 0)
                                {
                                    session.SendPacket(
                                        $"qna #guri^300^8023^{inv.Slot}^{packetsplit[3]} {Language.Instance.GetMessageFromKey("ASK_STORE_PET")}");
                                }
                                else
                                {
                                    session.SendPacket(
                                        $"qna #guri^300^8023^{inv.Slot}^{packetsplit[3]} {Language.Instance.GetMessageFromKey("ASK_RELEASE_PET")}");
                                }
                            }
                        }
                    }
                    else
                    {
                        //u_i 2 2000000 0 21 0 0
                        if (inv.Item.ItemSubType == 3)
                        {
                            OpenBoxItem(session, inv);
                        }
                        else if (inv.HoldingVNum == 0)
                        {
                            if (packetsplit?.Length == 1 && int.TryParse(packetsplit[0], out int petId)
                                && session.Character.Mates.Find(s => s.MateTransportId == petId) is Mate mate)
                            {
                                if (mate.IsTeamMember)
                                {
                                    session.SendPacket(
                                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_STORE_YOUR_TEAMMEM"), 10));
                                }
                                else
                                {
                                    inv.HoldingVNum = mate.NpcMonsterVNum;
                                    inv.SpLevel = mate.Level;
                                    inv.SpDamage = mate.Attack;
                                    inv.SpDefence = mate.Defence;
                                    session.Character.Mates.Remove(mate);
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateInfo(
                                            Language.Instance.GetMessageFromKey("PET_STORED")));
                                    session.SendPacket(UserInterfaceHelper.GeneratePClear());
                                    session.SendPackets(session.Character.GenerateScP());
                                    session.SendPackets(session.Character.GenerateScN());
                                    session.CurrentMapInstance?.Broadcast(mate.GenerateOut());
                                }
                            }
                        }
                        else
                        {
                            NpcMonster heldMonster = ServerManager.GetNpcMonster(inv.HoldingVNum);
                            if (heldMonster != null)
                            {
                                Mate mate = new Mate(session.Character, heldMonster, 1, MateType.Pet)
                                {
                                    Attack = inv.SpDamage,
                                    Defence = inv.SpDefence
                                };
                                if (session.Character.AddPet(mate))
                                {
                                    session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateInfo(
                                            Language.Instance.GetMessageFromKey("PET_LEAVE_BEAD")));
                                }
                            }
                        }
                    }

                    break;

                case 1:
                    if (option == 0)
                    {
                        session.SendPacket(
                            $"qna #guri^300^8023^{inv.Slot} {Language.Instance.GetMessageFromKey("ASK_RELEASE_PET")}");
                    }
                    else
                    {
                        NpcMonster heldMonster = ServerManager.GetNpcMonster((short) EffectValue);
                        if (session.CurrentMapInstance == session.Character.Miniland && heldMonster != null)
                        {
                            Mate mate = new Mate(session.Character, heldMonster, LevelMinimum,
                                ItemSubType == 1 ? MateType.Partner : MateType.Pet);
                            if (session.Character.AddPet(mate))
                            {
                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateInfo(
                                        Language.Instance.GetMessageFromKey("PET_LEAVE_BEAD")));
                            }
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_IN_MINILAND"),
                                    12));
                        }
                    }

                    break;

                case 6969:
                    if (EffectValue == 1 || EffectValue == 2)
                    {
                        var box =
                            session.Character.Inventory.LoadBySlotAndType(inv.Slot,
                                InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"wopen 44 {inv.Slot} 1");
                            }
                            else
                            {
                                List<ItemInstance> newInv =
                                    session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv.Any())
                                {
                                    ItemInstance itemInstance = newInv.First();
                                    var specialist =
                                        session.Character.Inventory.LoadBySlotAndType(
                                            itemInstance.Slot, itemInstance.Type);
                                    if (specialist != null)
                                    {
                                        specialist.FirstPartnerSkillRank = box.FirstPartnerSkillRank;
                                        specialist.SecondPartnerSkillRank = box.SecondPartnerSkillRank;
                                        specialist.ThirdPartnerSkillRank = box.ThirdPartnerSkillRank;
                                        specialist.FirstPartnerSkill = box.FirstPartnerSkill;
                                        specialist.SecondPartnerSkill = box.SecondPartnerSkill;
                                        specialist.ThirdPartnerSkill = box.ThirdPartnerSkill;
                                    }

                                    short slot = inv.Slot;
                                    if (slot != -1)
                                    {
                                        if (specialist != null)
                                        {
                                            session.SendPacket(session.Character.GenerateSay(
                                                $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {specialist.Item.Name}",
                                                12));
                                            newInv.ForEach(s => session.SendPacket(specialist.GenerateInventoryAdd()));
                                        }

                                        session.Character.Inventory.RemoveItemFromInventory(box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }
                    break;


                case 69:
                    if (EffectValue == 1 || EffectValue == 2)
                    {
                        ItemInstance box =
                            session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot,
                                InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"wopen 44 {inv.Slot}");
                            }
                            else
                            {
                                List<ItemInstance> newInv =
                                    session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv.Count > 0)
                                {
                                    ItemInstance itemInstance = newInv[0];
                                    ItemInstance specialist =
                                        session.Character.Inventory.LoadBySlotAndType<ItemInstance>(itemInstance.Slot,
                                            itemInstance.Type);
                                    if (specialist != null)
                                    {
                                        specialist.SlDamage = box.SlDamage;
                                        specialist.SlDefence = box.SlDefence;
                                        specialist.SlElement = box.SlElement;
                                        specialist.SlHP = box.SlHP;
                                        specialist.SpDamage = box.SpDamage;
                                        specialist.SpDark = box.SpDark;
                                        specialist.SpDefence = box.SpDefence;
                                        specialist.SpElement = box.SpElement;
                                        specialist.SpFire = box.SpFire;
                                        specialist.SpHP = box.SpHP;
                                        specialist.SpLevel = box.SpLevel;
                                        specialist.SpLight = box.SpLight;
                                        specialist.SpStoneUpgrade = box.SpStoneUpgrade;
                                        specialist.SpWater = box.SpWater;
                                        specialist.Upgrade = box.Upgrade;
                                        specialist.EquipmentSerialId = box.EquipmentSerialId;
                                        specialist.XP = box.XP;
                                    }

                                    if (inv.Slot != -1)
                                    {
                                        if (specialist != null)
                                        {
                                            session.SendPacket(session.Character.GenerateSay(
                                                $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {specialist.Item.Name} + {specialist.Upgrade}",
                                                12));
                                            newInv.ForEach(s => session.SendPacket(specialist.GenerateInventoryAdd()));
                                        }

                                        session.Character.Inventory.RemoveItemFromInventory(box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }

                    if (EffectValue == 3)
                    {
                        ItemInstance box =
                            session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot,
                                InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"guri 26 0 {inv.Slot}");
                            }
                            else
                            {
                                List<ItemInstance> newInv =
                                    session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv.Count > 0)
                                {
                                    ItemInstance itemInstance = newInv[0];
                                    ItemInstance fairy =
                                        session.Character.Inventory.LoadBySlotAndType<ItemInstance>(itemInstance.Slot,
                                            itemInstance.Type);
                                    if (fairy != null)
                                    {
                                        fairy.ElementRate = box.ElementRate;
                                    }

                                    if (inv.Slot != -1)
                                    {
                                        if (fairy != null)
                                        {
                                            session.SendPacket(session.Character.GenerateSay(
                                                $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {fairy.Item.Name} ({fairy.ElementRate}%)",
                                                12));
                                            newInv.ForEach(s => session.SendPacket(fairy.GenerateInventoryAdd()));
                                        }

                                        session.Character.Inventory.RemoveItemFromInventory(box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }

                    if (EffectValue == 4)
                    {
                        ItemInstance box =
                            session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot,
                                InventoryType.Equipment);
                        if (box != null)
                        {
                            if (box.HoldingVNum == 0)
                            {
                                session.SendPacket($"guri 24 0 {inv.Slot}");
                            }
                            else
                            {
                                List<ItemInstance> newInv =
                                    session.Character.Inventory.AddNewToInventory(box.HoldingVNum);
                                if (newInv.Count > 0)
                                {
                                    if (inv.Slot != -1)
                                    {
                                        session.SendPacket(session.Character.GenerateSay(
                                            $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newInv[0].Item.Name} x 1)",
                                            12));
                                        newInv.ForEach(s => session.SendPacket(s.GenerateInventoryAdd()));
                                        session.Character.Inventory.RemoveItemFromInventory(box.Id);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                }
                            }
                        }
                    }

                    break;

                default:
                    Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(), VNum,
                        Effect, EffectValue));
                    break;
            }
        }

        #endregion
    }
}