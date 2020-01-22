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
using System.Collections.Generic;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BazaarPackets
{
    [PacketHeader("c_buy")]
    public class BazaarBuyPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public int BazaarId { get; set; }

        public long Price { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public short VNum { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 6)
            {
                return;
            }
            BazaarBuyPacket packetDefinition = new BazaarBuyPacket();
            if (int.TryParse(packetSplit[2], out int bazaarId)
                && short.TryParse(packetSplit[3], out short vNum)
                && byte.TryParse(packetSplit[4], out byte amount)
                && long.TryParse(packetSplit[5], out long price))
            {
                packetDefinition.BazaarId = bazaarId;
                packetDefinition.VNum = vNum;
                packetDefinition.Amount = amount;
                packetDefinition.Price = price;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BazaarBuyPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            BazaarItemDTO bzItem = DAOFactory.BazaarItemDAO.LoadById(BazaarId);
            if (bzItem != null && Amount > 0)
            {
                long price = Amount * bzItem.Price;

                if (session.Character.Gold >= price)
                {
                    BazaarItemLink bzItemLink = new BazaarItemLink { BazaarItem = bzItem };
                    if (DAOFactory.CharacterDAO.LoadById(bzItem.SellerId) != null)
                    {
                        bzItemLink.OwnerName = DAOFactory.CharacterDAO.LoadById(bzItem.SellerId)?.Name;
                        bzItemLink.Item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bzItem.ItemInstanceId));
                    }
                    else
                    {
                        return;
                    }

                    if (Amount <= bzItemLink.Item.Amount)
                    {
                        if (!session.Character.Inventory.CanAddItem(bzItemLink.Item.ItemVNum))
                        {
                            session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                    0));
                            return;
                        }
                        if (bzItemLink.Item == null || (bzItem.IsPackage && Amount != bzItem.Amount))
                        {
                            return;
                        }

                        ItemInstance itemInstance =
                           new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bzItemLink.BazaarItem.ItemInstanceId));
                        if (Amount > itemInstance.Amount)
                        {
                            return;
                        }

                        itemInstance.Amount -= Amount;
                        session.Character.Gold -= price;
                        session.SendPacket(session.Character.GenerateGold());
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(itemInstance);
                        ServerManager.Instance.BazaarRefresh(bzItemLink.BazaarItem.BazaarItemId);
                        session.SendPacket(
                            $"rc_buy 1 {itemInstance.ItemVNum} {bzItemLink.OwnerName} {Amount} {Price} 0 0 0");

                        ItemInstance newBz = bzItemLink.Item.DeepCopy();
                        newBz.Id = Guid.NewGuid();
                        newBz.Amount = Amount;
                        newBz.Type = newBz.Item.Type;
                        List<ItemInstance> newInv = session.Character.Inventory.AddToInventory(newBz);

                        if (newInv.Count > 0)
                        {
                            session.SendPacket(session.Character.GenerateSay(
                                $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {itemInstance.Item.Name} x {Amount}",
                                10));
                        }

                        GameLogger.Instance.LogBazaarBuy(ServerManager.Instance.ChannelId, session.Character.Name,
                            session.Character.CharacterId, bzItem, itemInstance.ItemVNum, Amount, price);
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
                    }
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    session.SendPacket(
                        UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 1));
                }
            }
            else
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
            }
        }

        #endregion
    }
}