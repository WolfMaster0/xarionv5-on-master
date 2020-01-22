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
// ReSharper disable HeuristicUnreachableCode

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("f_deposit")]
    public class FDepositPacket
    {
        #region Properties

        public InventoryType Inventory { get; set; }

        public byte Slot { get; set; }

        public byte Amount { get; set; }

        public byte NewSlot { get; set; }

        public byte? Unknown { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            return;
#pragma warning disable 162
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            FDepositPacket packetDefinition = new FDepositPacket();
            if (Enum.TryParse(packetSplit[2], out InventoryType invType)
                && byte.TryParse(packetSplit[3], out byte slot) && byte.TryParse(packetSplit[4], out byte amount)
                && byte.TryParse(packetSplit[5], out byte newSlot))
            {
                packetDefinition.Inventory = invType;
                packetDefinition.Slot = slot;
                packetDefinition.Amount = amount;
                packetDefinition.NewSlot = newSlot;
                if (packetSplit.Length > 6 && byte.TryParse(packetSplit[6], out byte unk))
                {
                    packetDefinition.Unknown = unk;
                }
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
#pragma warning restore 162
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FDepositPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null
                || !(session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
                     || session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
                     || (session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                         && session.Character.Family.MemberAuthorityType != FamilyAuthorityType.None)
                     || (session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager
                         && session.Character.Family.ManagerAuthorityType != FamilyAuthorityType.None)))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            ItemInstance item =
                session.Character.Inventory.LoadBySlotAndType(Slot, Inventory);
            ItemInstance itemdest =
                session.Character.Family.Warehouse.LoadBySlotAndType(NewSlot,
                    InventoryType.FamilyWareHouse);

            // behave like on official, check if there is already an item in the destination slot
            if (itemdest != null)
            {
                return;
            }

            // check if the destination slot is out of range
            if (NewSlot > session.Character.Family.WarehouseSize)
            {
                return;
            }

            // check if the character is allowed to move the item
            if (session.Character.InExchangeOrTrade)
            {
                return;
            }

            // actually move the item from source to destination
            session.Character.Inventory.FDepositItem(Inventory, Slot,
                Amount, NewSlot, ref item, ref itemdest);
        }

        #endregion
    }
}