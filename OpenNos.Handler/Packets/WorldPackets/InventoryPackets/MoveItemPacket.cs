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
    [PacketHeader("mvi")]
    public class MoveItemPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public byte DestinationSlot { get; set; }

        public InventoryType InventoryType { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            MoveItemPacket packetDefinition = new MoveItemPacket();
            if (Enum.TryParse(packetSplit[2], out InventoryType inventoryType)
                && byte.TryParse(packetSplit[3], out byte slot)
                && byte.TryParse(packetSplit[4], out byte amount)
                && byte.TryParse(packetSplit[5], out byte destinationSlot))
            {
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.Amount = amount;
                packetDefinition.DestinationSlot = destinationSlot;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MoveItemPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            lock (session.Character.Inventory)
            {
                if (Amount == 0 || InventoryType == InventoryType.Bazaar || InventoryType == InventoryType.Wear || InventoryType == InventoryType.FamilyWareHouse || InventoryType == InventoryType.Warehouse || InventoryType == InventoryType.FirstPartnerInventory || InventoryType == InventoryType.SecondPartnerInventory || InventoryType == InventoryType.ThirdPartnerInventory || Slot == DestinationSlot)
                {
                    return;
                }

                // check if the destination slot is out of range
                if (DestinationSlot > 48 + ((session.Character.HaveBackpack() ? 1 : 0) * 12))
                {
                    return;
                }

                // check if the character is allowed to move the item
                if (session.Character.InExchangeOrTrade)
                {
                    return;
                }

                // actually move the item from source to destination
                session.Character.Inventory.MoveItem(InventoryType, InventoryType,
                    Slot, Amount, DestinationSlot, out ItemInstance previousInventory,
                    out ItemInstance newInventory);
                if (newInventory == null)
                {
                    return;
                }

                session.SendPacket(newInventory.GenerateInventoryAdd());

                session.SendPacket(previousInventory != null
                    ? previousInventory.GenerateInventoryAdd()
                    : UserInterfaceHelper.Instance.GenerateInventoryRemove(InventoryType,
                        Slot));
            }
        }

        #endregion
    }
}