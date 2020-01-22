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
using System.Linq;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("eqinfo")]
    public class EquipmentInfoPacket
    {
        #region Properties

        public long? PartnerSlot { get; set; }

        public long? ShopOwnerId { get; set; }

        public short Slot { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            EquipmentInfoPacket packetDefinition = new EquipmentInfoPacket();
            if (byte.TryParse(packetSplit[2], out byte type)
                && byte.TryParse(packetSplit[3], out byte slot))
            {
                packetDefinition.Type = type;
                packetDefinition.Slot = slot;
                packetDefinition.ShopOwnerId = packetSplit.Length >= 6
                    && long.TryParse(packetSplit[5], out long shopOwnerId) ? shopOwnerId : (long?)null;
                packetDefinition.PartnerSlot = packetSplit.Length >= 5
                    && long.TryParse(packetSplit[4], out long partnerSlot) ? partnerSlot : (long?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(EquipmentInfoPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            bool isNpcShopItem = false;
            ItemInstance inventory = null;
            switch (Type)
            {
                case 0:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot,
                        InventoryType.Wear);
                    break;

                case 1:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot,
                        InventoryType.Equipment);
                    break;

                case 2:
                    isNpcShopItem = true;
                    if (ServerManager.GetItem(Slot) != null)
                    {
                        inventory = new ItemInstance(Slot, 1);
                        break;
                    }

                    return;

                case 5:
                    if (session.Character.ExchangeInfo != null)
                    {
                        ClientSession sess =
                            ServerManager.Instance.GetSessionByCharacterId(session.Character.ExchangeInfo
                                .TargetCharacterId);
                        if (sess?.Character.ExchangeInfo?.ExchangeList?.ElementAtOrDefault(Slot) != null)
                        {
                            Guid id = sess.Character.ExchangeInfo.ExchangeList[Slot].Id;
                            inventory = sess.Character.Inventory.GetItemInstanceById(id);
                        }
                    }

                    break;

                case 6:
                    if (ShopOwnerId.HasValue)
                    {
                        KeyValuePair<long, MapShop> shop =
                            session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop =>
                            mapshop.Value.OwnerId.Equals(ShopOwnerId.Value));
                        PersonalShopItem item = shop.Value?.Items.Find(i => i.ShopSlot.Equals(Slot));
                        if (item != null)
                        {
                            inventory = item.ItemInstance;
                        }
                    }

                    break;

                case 7:
                    if (!PartnerSlot.HasValue)
                    {
                        break;
                    }

                    if (Slot >= 1 && Slot <= 12)
                    {
                        inventory = session.Character.Inventory.LoadBySlotAndType((short)PartnerSlot.Value, (InventoryType)(Slot + 12));
                    }
                    break;

                case 10:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType.Specialist);
                    break;

                case 11:
                    inventory = session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType.Costume);
                    break;
            }

            if (inventory?.Item != null)
            {
                if (inventory.IsEmpty || isNpcShopItem)
                {
                    session.SendPacket(inventory.GenerateEInfo());
                    return;
                }

                session.SendPacket(inventory.Item.EquipmentSlot != EquipmentType.Sp ? inventory.GenerateEInfo() :
                    inventory.Item.SpType == 0 && inventory.Item.ItemSubType == 4 ? inventory.GeneratePslInfo() :
                    inventory.GenerateSlInfo());
            }
        }

        #endregion
    }
}