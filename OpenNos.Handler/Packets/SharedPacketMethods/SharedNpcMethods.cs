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
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.Handler.Packets.SharedPacketMethods
{
    internal static class SharedNpcMethods
    {
        #region Methods

        internal static bool BuyValidate(this ClientSession session, KeyValuePair<long, MapShop> shop, short slot, byte amount)
        {
            if (!session.HasCurrentMapInstance)
            {
                return false;
            }

            PersonalShopItem shopitem = session.CurrentMapInstance.UserShops[shop.Key].Items
                .Find(i => i.ShopSlot.Equals(slot));
            if (shopitem == null)
            {
                return false;
            }

            Guid id = shopitem.ItemInstance.Id;

            ClientSession shopOwnerSession = ServerManager.Instance.GetSessionByCharacterId(shop.Value.OwnerId);
            if (shopOwnerSession == null)
            {
                return false;
            }

            if (amount > shopitem.SellAmount)
            {
                amount = shopitem.SellAmount;
            }

            ItemInstance sellerItem = shopOwnerSession.Character.Inventory.GetItemInstanceById(id);
            if (sellerItem == null || sellerItem.Amount < amount)
            {
                return false;
            }

            List<ItemInstance> inv = shopitem.ItemInstance.Type == InventoryType.Equipment
                ? session.Character.Inventory.AddToInventory(shopitem.ItemInstance)
                : session.Character.Inventory.AddNewToInventory(shopitem.ItemInstance.ItemVNum, amount,
                    shopitem.ItemInstance.Type);

            if (inv.Count == 0)
            {
                return false;
            }

            shopOwnerSession.Character.Gold += shopitem.Price * amount;
            shopOwnerSession.SendPacket(shopOwnerSession.Character.GenerateGold());
            shopOwnerSession.SendPacket(UserInterfaceHelper.GenerateShopMemo(1,
                string.Format(Language.Instance.GetMessageFromKey("BUY_ITEM"), session.Character.Name,
                    shopitem.ItemInstance.Item.Name, amount)));
            session.CurrentMapInstance.UserShops[shop.Key].Sell += shopitem.Price * amount;

            if (shopitem.ItemInstance.Type != InventoryType.Equipment)
            {
                // remove sold amount of items
                shopOwnerSession.Character.Inventory.RemoveItemFromInventory(id, amount);

                // remove sold amount from sellamount
                shopitem.SellAmount -= amount;
            }
            else
            {
                // remove equipment
                shopOwnerSession.Character.Inventory.Remove(shopitem.ItemInstance.Id);

                // send empty slot to owners inventory
                shopOwnerSession.SendPacket(
                    UserInterfaceHelper.Instance.GenerateInventoryRemove(shopitem.ItemInstance.Type,
                        shopitem.ItemInstance.Slot));

                // remove the sell amount
                shopitem.SellAmount = 0;
            }

            // remove item from shop if the amount the user wanted to sell has been sold
            if (shopitem.SellAmount == 0)
            {
                session.CurrentMapInstance.UserShops[shop.Key].Items.Remove(shopitem);
            }

            // update currently sold item
            shopOwnerSession.SendPacket($"sell_list {shop.Value.Sell} {slot}.{amount}.{shopitem.SellAmount}");

            // end shop
            if (!session.CurrentMapInstance.UserShops[shop.Key].Items.Any(s => s.SellAmount > 0))
            {
                shopOwnerSession.Character.CloseShop();
            }

            return true;
        }

        internal static void LoadShopItem(this ClientSession session, long owner, KeyValuePair<long, MapShop> shop)
        {
            string packetToSend = $"n_inv 1 {owner} 0 0";

            if (shop.Value?.Items != null)
            {
                short tmpSlot = 0;
                foreach (PersonalShopItem item in shop.Value.Items)
                {
                    if (item != null)
                    {
                        for (short i = tmpSlot; i < item.ShopSlot; i++)
                        {
                            packetToSend += " -1";
                        }
                        if (item.ItemInstance.Item.Type == InventoryType.Equipment)
                        {
                               packetToSend +=
                                $" 0.{item.ShopSlot}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rare}.{item.ItemInstance.Upgrade}.{item.Price}.";
                        }
                        else
                        {
                            packetToSend +=
                                $" {(byte)item.ItemInstance.Item.Type}.{item.ShopSlot}.{item.ItemInstance.ItemVNum}.{item.SellAmount}.{item.Price}.-1";
                        }
                        tmpSlot = item.ShopSlot;
                        tmpSlot++;
                    }
                }
                if(tmpSlot < 20)
                {
                    for (short i = tmpSlot; i < 20; i++)
                    {
                        packetToSend += " -1";
                    }
                }
            }

            packetToSend +=
                " -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1 -1";

            session.SendPacket(packetToSend);
        }

        #endregion
    }
}