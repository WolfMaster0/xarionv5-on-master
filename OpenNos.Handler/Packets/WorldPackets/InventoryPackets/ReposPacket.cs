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

using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("repos")]
    public class ReposPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public byte NewSlot { get; set; }

        public byte OldSlot { get; set; }

        public bool PartnerBackpack { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            ReposPacket packetDefinition = new ReposPacket();
            if (byte.TryParse(packetSplit[2], out byte oldSlot)
                && byte.TryParse(packetSplit[3], out byte amount)
                && byte.TryParse(packetSplit[4], out byte newSlot))
            {
                packetDefinition.OldSlot = oldSlot;
                packetDefinition.Amount = amount;
                packetDefinition.NewSlot = newSlot;
                packetDefinition.PartnerBackpack = packetSplit[5] == "1";
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ReposPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (OldSlot.Equals(NewSlot))
            {
                return;
            }

            if (Amount == 0)
            {
                return;
            }

            // check if the destination slot is out of range
            if (NewSlot >= (PartnerBackpack
                    ? (session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBackPack)
                        ? 50
                        : 0)
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
            session.Character.Inventory.MoveItem(
                PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse,
                PartnerBackpack ? InventoryType.PetWarehouse : InventoryType.Warehouse,
                OldSlot, Amount, NewSlot, out ItemInstance previousInventory,
                out ItemInstance newInventory);
            if (newInventory == null)
            {
                return;
            }

            session.SendPacket(PartnerBackpack
                ? newInventory.GeneratePStash()
                : newInventory.GenerateStash());
            session.SendPacket(previousInventory != null
                ? (PartnerBackpack
                    ? previousInventory.GeneratePStash()
                    : previousInventory.GenerateStash())
                : (PartnerBackpack
                    ? UserInterfaceHelper.Instance.GeneratePStashRemove(OldSlot)
                    : UserInterfaceHelper.Instance.GenerateStashRemove(OldSlot)));
        }

        #endregion
    }
}