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

using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("withdraw")]
    public class WithdrawPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public bool PartnerBackpack { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }

            WithdrawPacket packetDefinition = new WithdrawPacket();
            if (byte.TryParse(packetSplit[2], out byte slot)
                && byte.TryParse(packetSplit[3], out byte amount))
            {
                packetDefinition.Slot = slot;
                packetDefinition.Amount = amount;
                if (packetSplit.Length > 4)
                {
                    packetDefinition.PartnerBackpack = packetSplit[4] == "1";
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(WithdrawPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ItemInstance item =
                session.Character.Inventory.LoadBySlotAndType(Slot,
                    PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);

            // actually move the item from source to destination
            if (item != null && Amount <= item.Amount && Amount > 0)
            {
                short? destSlot = session.Character.Inventory.GetFreeSlot(item.Item.Type);

                // check if the destination slot is out of range
                if (destSlot == null || destSlot < 0)
                {
                    return;
                }

                // check if the character is allowed to move the item
                if (session.Character.InExchangeOrTrade)
                {
                    return;
                }

                session.Character.Inventory.MoveItem(
                    PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse, item.Item.Type, Slot,
                    Amount, destSlot.Value, out item, out ItemInstance itemdest);

                if (item != null)
                {
                    session.SendPacket(PartnerBackpack ? item.GeneratePStash() : item.GenerateStash());
                }
                else
                {
                    session.SendPacket(PartnerBackpack
                        ? UserInterfaceHelper.Instance.GeneratePStashRemove(Slot)
                        : UserInterfaceHelper.Instance.GenerateStashRemove(Slot));
                }

                if (itemdest != null)
                {
                    session.SendPacket(itemdest.GenerateInventoryAdd());
                }
            }

            /* Old code
             *
             * ItemInstance previousInventory = session.Character.Inventory.LoadBySlotAndType(Slot,
             *    PetBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);
             * if (Amount <= 0 || previousInventory == null
             *     || Amount > previousInventory.Amount
             *     || !session.Character.Inventory.CanAddItem(previousInventory.ItemVNum))
             * {
             *     return;
             * }
             *
             * ItemInstance item2 = previousInventory.DeepCopy();
             * item2.Id = Guid.NewGuid();
             * item2.Amount = Amount;
             *
             * session.Character.Inventory.RemoveItemFromInventory(previousInventory.Id, Amount);
             * session.Character.Inventory.AddToInventory(item2, item2.Item.Type);
             * session.Character.Inventory.LoadBySlotAndType(Slot,
             *     PetBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse);
             * session.SendPacket(PetBackpack
             *     ? UserInterfaceHelper.Instance.GeneratePStashRemove(Slot)
             *     : UserInterfaceHelper.Instance.GenerateStashRemove(Slot));
             */
        }

        #endregion
    }
}