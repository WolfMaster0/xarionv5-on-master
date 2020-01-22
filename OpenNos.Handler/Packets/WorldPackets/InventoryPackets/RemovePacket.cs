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
using System.Diagnostics;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("remove")]
    public class RemovePacket
    {
        #region Properties

        public byte InventorySlot { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            RemovePacket packetDefinition = new RemovePacket();
            if (byte.TryParse(packetSplit[2], out byte inventorySlot)
                && byte.TryParse(packetSplit[3], out byte type))
            {
                packetDefinition.InventorySlot = inventorySlot;
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(RemovePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            InventoryType equipment;
            Mate mate = null;
            if (Type >= 1 && Type <= 12)
            {
                // Partner inventories
                equipment = (InventoryType)(Type + 12);
                mate = session.Character.Mates.FirstOrDefault(s => s.PartnerSlot == (Type - 1) && s.MateType == MateType.Partner);
            }
            else
            {
                equipment = InventoryType.Wear;
            }


            if (session.HasCurrentMapInstance
                && session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop =>
                    mapshop.Value.OwnerId.Equals(session.Character.CharacterId)).Value == null
                && (session.Character.ExchangeInfo == null
                 || (session.Character.ExchangeInfo?.ExchangeList).Count == 0))
            {
                ItemInstance inventory =
                    session.Character.Inventory.LoadBySlotAndType(InventorySlot, equipment);
                if (inventory != null)
                {
                    double currentRunningSeconds =
                        (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastSpUsage = currentRunningSeconds - session.Character.LastSp;
                    if (Type == 0)
                    {
                        if (InventorySlot == (byte)EquipmentType.Sp && session.Character.UseSp)
                        {
                            if (session.Character.IsVehicled)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                                return;
                            }

                            if (session.Character.LastSkillUse.AddSeconds(2) > DateTime.UtcNow)
                            {
                                return;
                            }

                            session.Character.LastSp =
                                (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;
                            session.RemoveSp(inventory.ItemVNum);
                        }
                        else if (InventorySlot == (byte)EquipmentType.Sp
                                 && !session.Character.UseSp
                                 && timeSpanSinceLastSpUsage <= session.Character.SpCooldown)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"),
                                    session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)),
                                0));
                            return;
                        }
                        else if (InventorySlot == (byte)EquipmentType.Fairy
                                 && session.Character.IsUsingFairyBooster)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("REMOVE_FAIRY_WHILE_USING_BOOSTER"), 0));
                            return;
                        }

                        session.Character.EquipmentBCards.RemoveAll(o => o.ItemVNum == inventory.ItemVNum);
                    }

                    ItemInstance inv = session.Character.Inventory.MoveInInventory(InventorySlot,
                        equipment, InventoryType.Equipment);

                    if (inv == null)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                0));
                        return;
                    }

                    if (inv.Slot != -1)
                    {
                        session.SendPacket(inventory.GenerateInventoryAdd());
                    }

                    if (Type == 0)
                    {
                        session.SendPacket(session.Character.GenerateStatChar());
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.CurrentMapInstance?.Broadcast(session.Character.GeneratePairy());
                    }
                    else if (mate != null)
                    {
                        switch (inv.Item.EquipmentSlot)
                        {
                            case EquipmentType.Armor:
                                mate.ArmorInstance = null;
                                break;

                            case EquipmentType.MainWeapon:
                                mate.WeaponInstance = null;
                                break;

                            case EquipmentType.Gloves:
                                mate.GlovesInstance = null;
                                break;

                            case EquipmentType.Boots:
                                mate.BootsInstance = null;
                                break;

                            case EquipmentType.Sp:
                                mate.SpInstance = null;
                                mate.RemoveSp();
                                break;
                        }
                        session.SendPacket(mate.GenerateScPacket());
                    }
                }
            }
        }

        #endregion
    }
}