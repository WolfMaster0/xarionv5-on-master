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
using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("deposit")]
    public class DepositPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public byte DestinationSlot { get; set; }

        public InventoryType InventoryType { get; set; }

        public bool PartnerBackpack { get; set; }

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
            DepositPacket packetDefinition = new DepositPacket();
            if (Enum.TryParse(packetSplit[2], out InventoryType inventoryType)
                && byte.TryParse(packetSplit[3], out byte slot)
                && byte.TryParse(packetSplit[4], out byte amount)
                && byte.TryParse(packetSplit[5], out byte destinationSlot))
            {
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.Amount = amount;
                packetDefinition.DestinationSlot = destinationSlot;
                if (packetSplit.Length > 6)
                {
                    packetDefinition.PartnerBackpack = packetSplit[6] == "1";
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DepositPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (InventoryType == InventoryType.Bazaar
                || InventoryType == InventoryType.FamilyWareHouse
                || InventoryType == InventoryType.Miniland)
            {
                return;
            }

            ItemInstance item =
                session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);

            // check if the destination slot is out of range
            if (DestinationSlot >= (PartnerBackpack
                    ? (session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBackPack) ? 50 : 0)
                    : session.Character.WareHouseSize))
            {
                return;
            }

            // check if the character is allowed to move the item
            if (session.Character.InExchangeOrTrade)
            {
                return;
            }

            // actually move the item from source to destination
            if (item != null && Amount <= item.Amount && Amount > 0)
            {
                session.Character.Inventory.MoveItem(InventoryType, PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse, Slot, Amount, DestinationSlot, out item, out ItemInstance itemdest);
                session.SendPacket(item != null ? item.GenerateInventoryAdd() : UserInterfaceHelper.Instance.GenerateInventoryRemove(InventoryType, Slot));

                if (itemdest != null)
                {
                    session.SendPacket(PartnerBackpack ? itemdest.GeneratePStash() : itemdest.GenerateStash());
                }
            }
        }

        #endregion
    }
}