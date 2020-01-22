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
using System.Threading;
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
    [PacketHeader("c_scalc")]
    public class BazaarGetPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public long BazaarId { get; set; }

        public byte MaxAmount { get; set; }

        public long Price { get; set; }

        public short VNum { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 7)
            {
                return;
            }
            BazaarGetPacket packetDefinition = new BazaarGetPacket();
            if (int.TryParse(packetSplit[2], out int bazaarId)
                && short.TryParse(packetSplit[3], out short vNum)
                && byte.TryParse(packetSplit[4], out byte amount)
                && byte.TryParse(packetSplit[5], out byte maxAmount)
                && long.TryParse(packetSplit[6], out long price))
            {
                packetDefinition.BazaarId = bazaarId;
                packetDefinition.VNum = vNum;
                packetDefinition.Amount = amount;
                packetDefinition.MaxAmount = maxAmount;
                packetDefinition.Price = price;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BazaarGetPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InBazaarRefreshMode);
            BazaarItemDTO bz = DAOFactory.BazaarItemDAO.LoadById(BazaarId);
            if (bz != null)
            {
                ItemInstance itemInstance = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bz.ItemInstanceId));
                if (bz.SellerId != session.Character.CharacterId)
                {
                    return;
                }

                int solddamount = bz.Amount - itemInstance.Amount;
                long taxes = bz.MedalUsed ? 0 : (long)(bz.Price * 0.10 * solddamount);
                long price = (bz.Price * solddamount) - taxes;
                if (session.Character.Inventory.CanAddItem(itemInstance.ItemVNum))
                {
                    if (session.Character.Gold + price <= ServerManager.Instance.Configuration.MaxGold)
                    {
                        session.Character.Gold += price;
                        session.SendPacket(session.Character.GenerateGold());
                        session.SendPacket(session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("REMOVE_FROM_BAZAAR"), price), 10));

                        // Edit this soo we dont generate new guid every single time we take something out.
                        if (itemInstance.Amount != 0)
                        {
                            ItemInstance newBz = itemInstance.DeepCopy();
                            newBz.Id = Guid.NewGuid();
                            newBz.Type = newBz.Item.Type;
                            session.Character.Inventory.AddToInventory(newBz);
                        }

                        session.SendPacket(
                            $"rc_scalc 1 {bz.Price} {bz.Amount - itemInstance.Amount} {bz.Amount} {taxes} {price + taxes}");

                        GameLogger.Instance.LogBazaarRemove(ServerManager.Instance.ChannelId, session.Character.Name,
                            session.Character.CharacterId, bz, itemInstance, price, taxes);

                        if (DAOFactory.BazaarItemDAO.LoadById(bz.BazaarItemId) != null)
                        {
                            DAOFactory.BazaarItemDAO.Delete(bz.BazaarItemId);
                        }

                        DAOFactory.ItemInstanceDAO.Delete(itemInstance.Id);

                        ServerManager.Instance.BazaarRefresh(bz.BazaarItemId);
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                        session.SendPacket($"rc_scalc 1 {bz.Price} 0 {bz.Amount} 0 0");
                    }
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE")));
                    session.SendPacket($"rc_scalc 1 {bz.Price} 0 {bz.Amount} 0 0");
                }
            }
            else
            {
                session.SendPacket("rc_scalc 1 0 0 0 0 0");
            }
        }

        #endregion
    }
}