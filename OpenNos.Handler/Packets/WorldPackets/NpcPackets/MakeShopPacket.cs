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
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.NpcPackets
{
    [PacketHeader("m_shop")]
    public class MakeShopPacket
    {
        #region Properties

        public short Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 2)
            {
                return;
            }
            MakeShopPacket packetDefinition = new MakeShopPacket();
            if (short.TryParse(packetSplit[2], out short type))
            {
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession, packetSplit);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MakeShopPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session, string[] packetSplit)
        {
            InventoryType[] inventoryType = new InventoryType[20];
            long[] gold = new long[20];
            short[] slot = new short[20];
            byte[] qty = new byte[20];
            string shopname = string.Empty;
            if ((session.Character.HasShopOpened && Type != 1) || !session.HasCurrentMapInstance
                                                               || session.Character.IsExchanging
                                                               || session.Character.ExchangeInfo != null)
            {
                return;
            }

            if (session.CurrentMapInstance.Portals.Any(por =>
                session.Character.PositionX < por.SourceX + 6 && session.Character.PositionX > por.SourceX - 6
                                                              && session.Character.PositionY < por.SourceY + 6
                                                              && session.Character.PositionY > por.SourceY - 6))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NEAR_PORTAL"), 0));
                return;
            }

            if (session.Character.Group != null && session.Character.Group?.GroupType != GroupType.Group)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED_IN_RAID"),
                        0));
                return;
            }

            if (!session.CurrentMapInstance.ShopAllowed)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED"), 0));
                return;
            }

            switch (Type)
            {
                case 2:
                    session.SendPacket("ishop");
                    break;

                case 0:
                    if (session.CurrentMapInstance.UserShops.Any(s =>
                        s.Value.OwnerId == session.Character.CharacterId))
                    {
                        return;
                    }

                    MapShop myShop = new MapShop();

                    if (packetSplit.Length > 82)
                    {
                        short shopSlot = 0;
                        for (short j = 3, i = 0; j < 82; j += 4, i++)
                        {
                            Enum.TryParse(packetSplit[j], out inventoryType[i]);
                            short.TryParse(packetSplit[j + 1], out slot[i]);
                            byte.TryParse(packetSplit[j + 2], out qty[i]);
                            long.TryParse(packetSplit[j + 3], out gold[i]);
                            if (gold[i] < 0)
                            {
                                return;
                            }

                            if (qty[i] > 0)
                            {
                                ItemInstance inv = session.Character.Inventory.LoadBySlotAndType(slot[i], inventoryType[i]);
                                if (inv != null)
                                {
                                    if (inv.Amount < qty[i])
                                    {
                                        return;
                                    }

                                    if (!inv.Item.IsTradable || (inv.IsBound
                                        && !(inv.Item.Type == InventoryType.Equipment
                                          && (inv.Item.ItemType == ItemType.Armor
                                           || inv.Item.ItemType == ItemType.Weapon))))
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("SHOP_ONLY_TRADABLE_ITEMS"), 0));
                                        session.SendPacket("shop_end 0");
                                        return;
                                    }

                                    PersonalShopItem personalshopitem = new PersonalShopItem
                                    {
                                        ShopSlot = shopSlot,
                                        Price = gold[i],
                                        ItemInstance = inv,
                                        SellAmount = qty[i]
                                    };
                                    myShop.Items.Add(personalshopitem);
                                }
                            }
                            shopSlot++;
                        }
                    }

                    if (myShop.Items.Count != 0)
                    {
                        if (!myShop.Items.Any(s => !s.ItemInstance.Item.IsSoldable || (s.ItemInstance.IsBound
                                                   && !(s.ItemInstance.Item.Type == InventoryType.Equipment
                                                     && (s.ItemInstance.Item.ItemType == ItemType.Armor
                                                      || s.ItemInstance.Item.ItemType == ItemType.Weapon)))))
                        {
                            for (int i = 83; i < packetSplit.Length; i++)
                            {
                                shopname += $"{packetSplit[i]} ";
                            }

                            // trim shopname
                            shopname = shopname.TrimEnd(' ');

                            // create default shopname if it's empty
                            if (string.IsNullOrWhiteSpace(shopname) || string.IsNullOrEmpty(shopname))
                            {
                                shopname = Language.Instance.GetMessageFromKey("SHOP_PRIVATE_SHOP");
                            }

                            // truncate the string to a max-length of 20
                            shopname = shopname.Truncate(20);
                            myShop.OwnerId = session.Character.CharacterId;
                            myShop.Name = shopname;
                            session.CurrentMapInstance.UserShops.Add(session.CurrentMapInstance.LastUserShopId++,
                                myShop);

                            session.Character.HasShopOpened = true;

                            session.CurrentMapInstance?.Broadcast(session,
                                session.Character.GeneratePlayerFlag(session.CurrentMapInstance.LastUserShopId),
                                ReceiverType.AllExceptMe);
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateShop(shopname));
                            session.SendPacket(
                                UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));

                            session.Character.IsSitting = true;
                            session.Character.IsShopping = true;

                            session.Character.LoadSpeed();
                            session.SendPacket(session.Character.GenerateCond());
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateRest());
                        }
                        else
                        {
                            session.SendPacket("shop_end 0");
                            session.SendPacket(
                                session.Character.GenerateSay(
                                    Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"), 10));
                        }
                    }
                    else
                    {
                        session.SendPacket("shop_end 0");
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SHOP_EMPTY"), 10));
                    }
                    break;

                case 1:
                    session.Character.CloseShop();
                    break;
            }
        }

        #endregion
    }
}