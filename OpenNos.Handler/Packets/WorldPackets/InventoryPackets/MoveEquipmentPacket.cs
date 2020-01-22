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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("mve")]
    public class MoveEquipmentPacket
    {
        #region Properties

        public InventoryType DestinationInventoryType { get; set; }

        public short DestinationSlot { get; set; }

        public InventoryType InventoryType { get; set; }

        public short Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            MoveEquipmentPacket packetDefinition = new MoveEquipmentPacket();
            if (Enum.TryParse(packetSplit[2], out InventoryType inventoryType)
                && short.TryParse(packetSplit[3], out short slot)
                && Enum.TryParse(packetSplit[4], out InventoryType destinationInventoryType)
                && short.TryParse(packetSplit[5], out short destinationSlot))
            {
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.DestinationInventoryType = destinationInventoryType;
                packetDefinition.DestinationSlot = destinationSlot;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MoveEquipmentPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            lock (session.Character.Inventory)
            {
                if (Slot.Equals(DestinationSlot)
                    && InventoryType.Equals(DestinationInventoryType))
                {
                    return;
                }

                if (DestinationSlot > 48 + ((session.Character.HaveBackpack() ? 1 : 0) * 12))
                {
                    return;
                }

                if (session.Character.InExchangeOrTrade)
                {
                    return;
                }

                ItemInstance sourceItem =
                    session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                if (sourceItem?.Item.ItemType == ItemType.Specialist
                    || sourceItem?.Item.ItemType == ItemType.Fashion)
                {
                    ItemInstance inv = session.Character.Inventory.MoveInInventory(Slot,
                        InventoryType, DestinationInventoryType, DestinationSlot,
                        false);
                    if (inv != null)
                    {
                        session.SendPacket(inv.GenerateInventoryAdd());
                        session.SendPacket(
                            UserInterfaceHelper.Instance.GenerateInventoryRemove(InventoryType,
                                Slot));
                    }
                }
            }
        }

        #endregion
    }
}