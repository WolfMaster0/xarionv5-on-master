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
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.BazaarPackets
{
    [PacketHeader("c_reg")]
    public class BazaarRegisterPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public byte Durability { get; set; }

        public byte InventoryType { get; set; }

        public int IsPackage { get; set; }

        public long Price { get; set; }

        public byte Slot { get; set; }

        public short Tax { get; set; }

        public int Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 13)
            {
                return;
            }
            BazaarRegisterPacket packetDefinition = new BazaarRegisterPacket();
            if (int.TryParse(packetSplit[2], out int type)
                && byte.TryParse(packetSplit[3], out byte inventoryType)
                && byte.TryParse(packetSplit[4], out byte slot)
                && byte.TryParse(packetSplit[7], out byte durability)
                && int.TryParse(packetSplit[8], out int isPackage)
                && byte.TryParse(packetSplit[9], out byte amount)
                && long.TryParse(packetSplit[10], out long price)
                && short.TryParse(packetSplit[11], out short tax))
            {
                packetDefinition.Type = type;
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.Durability = durability;
                packetDefinition.IsPackage = isPackage;
                packetDefinition.Amount = amount;
                packetDefinition.Price = price;
                packetDefinition.Tax = tax;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(BazaarRegisterPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InBazaarRefreshMode);
            StaticBonusDTO medal = session.Character.StaticBonusList.Find(s =>
                s.StaticBonusType == StaticBonusType.BazaarMedalGold
                || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);

            long price = Price * Amount;
            long taxmax = price > 100000 ? price / 200 : 500;
            long taxmin = price >= 4000
                ? (60 + ((price - 4000) / 2000 * 30) > 10000 ? 10000 : 60 + ((price - 4000) / 2000 * 30))
                : 50;
            long tax = medal == null ? taxmax : taxmin;
            long maxGold = ServerManager.Instance.Configuration.MaxGold;
            if (session.Character.Gold < tax || Amount <= 0
                || session.Character.ExchangeInfo?.ExchangeList.Count > 0 || session.Character.IsShopping)
            {
                return;
            }

            ItemInstance it = session.Character.Inventory.LoadBySlotAndType(Slot,
                InventoryType == 4 ? 0 : (InventoryType)InventoryType);

            if (it?.Item.IsSoldable != true || !it.Item.IsTradable || (it.IsBound
                && !(it.Item.Type == Domain.InventoryType.Equipment
                  && (it.Item.ItemType == ItemType.Armor || it.Item.ItemType == ItemType.Weapon))))
            {
                return;
            }

            if (session.Character.Inventory.CountItemInAnInventory(Domain.InventoryType.Bazaar)
                > 10 * (medal == null ? 1 : 10))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LIMIT_EXCEEDED"), 0));
                return;
            }

            if (price >= (medal == null ? 1000000 : maxGold))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PRICE_EXCEEDED"), 0));
                return;
            }

            if (Price < 0)
            {
                return;
            }

            ItemInstance bazaar = session.Character.Inventory.AddIntoBazaarInventory(
                InventoryType == 4 ? 0 : (InventoryType)InventoryType, Slot,
                Amount);
            if (bazaar == null)
            {
                return;
            }

            short duration;
            switch (Durability)
            {
                case 1:
                    duration = 24;
                    break;

                case 2:
                    duration = 168;
                    break;

                case 3:
                    duration = 360;
                    break;

                case 4:
                    duration = 720;
                    break;

                default:
                    return;
            }

            DAOFactory.ItemInstanceDAO.InsertOrUpdate(bazaar);

            BazaarItemDTO bazaarItem = new BazaarItemDTO
            {
                Amount = bazaar.Amount,
                DateStart = DateTime.UtcNow,
                Duration = duration,
                IsPackage = IsPackage != 0,
                MedalUsed = medal != null,
                Price = Price,
                SellerId = session.Character.CharacterId,
                ItemInstanceId = bazaar.Id
            };

            DAOFactory.BazaarItemDAO.InsertOrUpdate(ref bazaarItem);
            ServerManager.Instance.BazaarRefresh(bazaarItem.BazaarItemId);

            session.Character.Gold -= tax;
            session.SendPacket(session.Character.GenerateGold());

            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("OBJECT_IN_BAZAAR"),
                10));
            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("OBJECT_IN_BAZAAR"),
                0));

            GameLogger.Instance.LogBazaarInsert(ServerManager.Instance.ChannelId, session.Character.Name,
                session.Character.CharacterId, bazaarItem, bazaar.ItemVNum);

            session.SendPacket("rc_reg 1");
        }

        #endregion
    }
}