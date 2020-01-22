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
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("put")]
    public class PutPacket
    {
        #region Properties

        public byte Amount { get; set; }

        public InventoryType InventoryType { get; set; }

        public byte Slot { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            PutPacket packetDefinition = new PutPacket();
            if (Enum.TryParse(packetSplit[2], out InventoryType inventoryType)
                && byte.TryParse(packetSplit[3], out byte slot)
                && byte.TryParse(packetSplit[4], out byte amount))
            {
                packetDefinition.InventoryType = inventoryType;
                packetDefinition.Slot = slot;
                packetDefinition.Amount = amount;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(PutPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.HasShopOpened)
            {
                return;
            }

            lock (session.Character.Inventory)
            {
                ItemInstance invitem =
                    session.Character.Inventory.LoadBySlotAndType(Slot, InventoryType);
                if (invitem?.Item.IsDroppable == true && invitem.Item.IsTradable
                                                      && !session.Character.InExchangeOrTrade
                                                      && InventoryType != InventoryType.Bazaar
                                                      && InventoryType != InventoryType.FamilyWareHouse
                                                      && InventoryType != InventoryType.Warehouse
                                                      && InventoryType != InventoryType.PetWarehouse
                                                      && InventoryType != InventoryType.Miniland)
                {
                    if (Amount > 0 && Amount < 100)
                    {
                        if (session.Character.MapInstance.DroppedList.Count < 200 && session.HasCurrentMapInstance)
                        {
                            MapItem droppedItem = session.CurrentMapInstance.PutItem(InventoryType,
                                Slot, Amount, ref invitem, session);
                            if (droppedItem == null)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE_HERE"), 0));
                                return;
                            }

                            session.SendPacket(invitem.GenerateInventoryAdd());

                            GameLogger.Instance.LogItemDrop(ServerManager.Instance.ChannelId, session.Character.Name,
                                session.Character.CharacterId, invitem, droppedItem.Amount,
                                session.CurrentMapInstance?.Map.MapId ?? -1, session.Character.PositionX,
                                session.Character.PositionY);
                            if (invitem.Amount == 0)
                            {
                                session.Character.DeleteItem(invitem.Type, invitem.Slot);
                            }

                            session.CurrentMapInstance?.Broadcast(
                                $"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.Amount} 0 0");
                        }
                        else
                        {
                            session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_MAP_FULL"),
                                    0));
                        }
                    }
                    else
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DROP_AMOUNT"), 0));
                    }
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
                }
            }
        }

        #endregion
    }
}