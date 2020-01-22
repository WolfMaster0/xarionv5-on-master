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
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
// ReSharper disable HeuristicUnreachableCode

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("f_withdraw")]
    public class FWithdrawPacket
    {
        #region Properties

        public short Slot { get; set; }

        public byte Amount { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte? Unknown { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            return;
#pragma warning disable 162
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            FWithdrawPacket packetDefinition = new FWithdrawPacket();
            if (short.TryParse(packetSplit[2], out short slot) && byte.TryParse(packetSplit[3], out byte amount))
            {
                packetDefinition.Slot = slot;
                packetDefinition.Amount = amount;
                if (packetSplit.Length > 4 && byte.TryParse(packetSplit[4], out byte unk))
                {
                    packetDefinition.Unknown = unk;
                }

                packetDefinition.ExecuteHandler(session as ClientSession);
            }
#pragma warning restore 162
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FWithdrawPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null
                || !(session.Character.FamilyCharacter.Authority == FamilyAuthority.Head
                  || session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
                  || (session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                      && session.Character.Family.MemberAuthorityType == FamilyAuthorityType.All)
                  || (session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager
                      && session.Character.Family.ManagerAuthorityType == FamilyAuthorityType.All)))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NO_FAMILY_RIGHT")));
                return;
            }

            ItemInstance previousInventory =
                session.Character.Family.Warehouse.LoadBySlotAndType(Slot,
                    InventoryType.FamilyWareHouse);
            if (Amount <= 0 || previousInventory == null
                || Amount > previousInventory.Amount)
            {
                return;
            }

            ItemInstance item2 = previousInventory.DeepCopy();
            item2.Id = Guid.NewGuid();
            item2.Amount = Amount;
            item2.CharacterId = session.Character.CharacterId;

            previousInventory.Amount -= Amount;
            if (previousInventory.Amount <= 0)
            {
                previousInventory = null;
            }

            session.Character.Inventory.AddToInventory(item2, item2.Item.Type);
            session.SendPacket(UserInterfaceHelper.Instance.GenerateFStashRemove(Slot));
            if (previousInventory != null)
            {
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(previousInventory);
            }
            else
            {
                FamilyCharacter fhead =
                    session.Character.Family.FamilyCharacters.Find(s => s.Authority == FamilyAuthority.Head);
                if (fhead == null)
                {
                    return;
                }

                DAOFactory.ItemInstanceDAO.DeleteFromSlotAndType(fhead.CharacterId, Slot,
                    InventoryType.FamilyWareHouse);
            }

            session.Character.Family.InsertFamilyLog(FamilyLogType.WareHouseRemoved, session.Character.Name,
                message: $"{item2.ItemVNum}|{Amount}");
        }

        #endregion
    }
}