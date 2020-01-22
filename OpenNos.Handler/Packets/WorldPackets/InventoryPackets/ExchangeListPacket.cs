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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("exc_list")]
    public class ExchangeListPacket
    {
        #region Properties

        public long Gold { get; set; }

        public long GoldBank { get; set; }

        public List<Item> Items { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            ClientSession sess = session as ClientSession;
            if (packetSplit.Length < 4)
            {
                sess?.CloseExchange(
                    ServerManager.Instance.GetSessionByCharacterId(sess.Character.ExchangeInfo.TargetCharacterId));
                return;
            }
            ExchangeListPacket packetDefinition = new ExchangeListPacket();
            if (long.TryParse(packetSplit[2], out long gold)
                && long.TryParse(packetSplit[3], out long goldBank))
            {
                packetDefinition.Gold = gold;
                packetDefinition.GoldBank = goldBank;
                packetDefinition.Items = new List<Item>();
                for (int j = 7, i = 0; j <= packetSplit.Length && i < 10; j += 3, i++)
                {
                    if (Enum.TryParse(packetSplit[j - 3], out InventoryType type)
                    && short.TryParse(packetSplit[j - 2], out short slot)
                    && byte.TryParse(packetSplit[j - 1], out byte amount))
                    {
                        packetDefinition.Items.Add(new Item() { Type = type, Slot = slot, Amount = amount });
                    }
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ExchangeListPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (Gold < 0 || GoldBank < 0 || session.Character.Gold < Gold || session.Character.GoldBank < GoldBank || session.Character.ExchangeInfo == null
                || session.Character.ExchangeInfo.ExchangeList.Count > 0)
            {
                return;
            }

            ClientSession targetSession =
                ServerManager.Instance.GetSessionByCharacterId(session.Character.ExchangeInfo.TargetCharacterId);
            if (session.Character.HasShopOpened || targetSession?.Character.HasShopOpened == true)
            {
                session.CloseExchange(targetSession);
                return;
            }

            int i = 0;
            string packetList = string.Empty;
            foreach (Item item in Items)
            {
                if (item.Type == InventoryType.Bazaar || item.Type == InventoryType.FamilyWareHouse)
                {
                    session.CloseExchange(targetSession);
                    return;
                }

                ItemInstance itemInstance = session.Character.Inventory.LoadBySlotAndType(item.Slot, item.Type);
                if (itemInstance == null)
                {
                    return;
                }

                if (item.Amount <= 0 || itemInstance.Amount < item.Amount)
                {
                    return;
                }
                ItemInstance it = itemInstance.DeepCopy();
                if (it.Item.IsTradable && (!it.IsBound || (it.Item.Type == InventoryType.Equipment
                    && (it.Item.ItemType == ItemType.Armor || it.Item.ItemType == ItemType.Weapon))))
                {
                    it.Amount = item.Amount;
                    session.Character.ExchangeInfo.ExchangeList.Add(it);
                    if (item.Type != InventoryType.Equipment)
                    {
                        packetList += $"{i}.{(byte)item.Type}.{it.ItemVNum}.{item.Amount} ";
                    }
                    else
                    {
                        packetList += $"{i}.{(byte)item.Type}.{it.ItemVNum}.{it.Rare}.{it.Upgrade} ";
                    }
                }
                else if (it.IsBound && !(it.Item.Type == InventoryType.Equipment
                                         && (it.Item.ItemType == ItemType.Armor || it.Item.ItemType == ItemType.Weapon)))

                {
                    session.SendPacket("exc_close 0");
                    session.CurrentMapInstance?.Broadcast(session, "exc_close 0", ReceiverType.OnlySomeone,
                        string.Empty, session.Character.ExchangeInfo.TargetCharacterId);

                    if (targetSession != null)
                    {
                        targetSession.Character.ExchangeInfo = null;
                    }

                    session.Character.ExchangeInfo = null;
                    return;
                }

                i++;
            }
            session.Character.ExchangeInfo.Gold = Gold;
            session.Character.ExchangeInfo.GoldBank = GoldBank;
            session.CurrentMapInstance?.Broadcast(session,
                $"exc_list 1 {session.Character.CharacterId} {Gold} {GoldBank} {packetList}", ReceiverType.OnlySomeone,
                string.Empty, session.Character.ExchangeInfo.TargetCharacterId);
            session.Character.ExchangeInfo.Validated = true;
        }

        #endregion

        #region Structs

        public struct Item
        {
            #region Properties

            public byte Amount { get; set; }

            public short Slot { get; set; }

            public InventoryType Type { get; set; }

            #endregion
        }

        #endregion
    }
}