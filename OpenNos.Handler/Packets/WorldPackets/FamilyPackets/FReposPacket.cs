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
    [PacketHeader("f_repos")]
    public class FReposPacket
    {
        #region Properties

        public byte OldSlot { get; set; }

        public byte Amount { get; set; }

        public byte NewSlot { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte? Unknown { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            return;
#pragma warning disable 162
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            FReposPacket packetDefinition = new FReposPacket();
            if (byte.TryParse(packetSplit[2], out byte slot) && byte.TryParse(packetSplit[3], out byte amount)
                && byte.TryParse(packetSplit[4], out byte newSlot))
            {
                packetDefinition.OldSlot = slot;
                packetDefinition.Amount = amount;
                packetDefinition.NewSlot = newSlot;
                if (packetSplit.Length > 5 && byte.TryParse(packetSplit[5], out byte unk))
                {
                    packetDefinition.Unknown = unk;
                }
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
#pragma warning restore 162
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FReposPacket), HandlePacket);

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

            // check if the character is allowed to move the item
            if (session.Character.InExchangeOrTrade || Amount <= 0)
            {
                return;
            }

            if (NewSlot > session.Character.Family.WarehouseSize)
            {
                return;
            }

            ItemInstance sourceInventory =
                session.Character.Family.Warehouse.LoadBySlotAndType(OldSlot,
                    InventoryType.FamilyWareHouse);
            ItemInstance destinationInventory =
                session.Character.Family.Warehouse.LoadBySlotAndType(NewSlot,
                    InventoryType.FamilyWareHouse);

            if (sourceInventory != null && Amount <= sourceInventory.Amount)
            {
                if (destinationInventory == null)
                {
                    destinationInventory = sourceInventory.DeepCopy();
                    sourceInventory.Amount -= Amount;
                    destinationInventory.Amount = Amount;
                    destinationInventory.Slot = NewSlot;
                    if (sourceInventory.Amount > 0)
                    {
                        destinationInventory.Id = Guid.NewGuid();
                    }
                    else
                    {
                        sourceInventory = null;
                    }
                }
                else if (destinationInventory.ItemVNum == sourceInventory.ItemVNum && (byte)sourceInventory.Item.Type != 0)
                {
                    if (destinationInventory.Amount + Amount > 255)
                    {
                        int saveItemCount = destinationInventory.Amount;
                        destinationInventory.Amount = 255;
                        sourceInventory.Amount = (byte)(saveItemCount + sourceInventory.Amount - 255);
                    }
                    else
                    {
                        destinationInventory.Amount += Amount;
                        sourceInventory.Amount -= Amount;
                        if (sourceInventory.Amount == 0)
                        {
                            DAOFactory.ItemInstanceDAO.Delete(sourceInventory.Id);
                            sourceInventory = null;
                        }
                    }
                }
                else
                {
                    destinationInventory.Slot = OldSlot;
                    sourceInventory.Slot = NewSlot;
                }
            }

            if (sourceInventory?.Amount > 0)
            {
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(sourceInventory);
            }

            if (destinationInventory?.Amount > 0)
            {
                DAOFactory.ItemInstanceDAO.InsertOrUpdate(destinationInventory);
            }

            session.SendPacket((destinationInventory != null)
                ? destinationInventory.GenerateFStash()
                : UserInterfaceHelper.Instance.GenerateFStashRemove(NewSlot));
            session.SendPacket((sourceInventory != null)
                ? sourceInventory.GenerateFStash()
                : UserInterfaceHelper.Instance.GenerateFStashRemove(OldSlot));
            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
        }

        #endregion
    }
}