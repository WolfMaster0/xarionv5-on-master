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
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.EventArguments;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.XMLModel.Quest.Model;

namespace OpenNos.Handler.Packets.WorldPackets.InventoryPackets
{
    [PacketHeader("get")]
    public class GetPacket
    {
        #region Properties

        public int PickerId { get; set; }

        public byte PickerType { get; set; }

        public long TransportId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            GetPacket packetDefinition = new GetPacket();
            if (byte.TryParse(packetSplit[2], out byte pickerType)
                && int.TryParse(packetSplit[3], out int pickerId)
                && long.TryParse(packetSplit[4], out long transportId))
            {
                packetDefinition.PickerType = pickerType;
                packetDefinition.PickerId = pickerId;
                packetDefinition.TransportId = transportId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GetPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.LastSkillUse.AddSeconds(1) > DateTime.UtcNow
                || (session.Character.IsVehicled
                 && session.CurrentMapInstance?.MapInstanceType != MapInstanceType.EventGameInstance)
                || !session.HasCurrentMapInstance)
            {
                return;
            }

            if (TransportId < 100000)
            {
                MapButton button = session.CurrentMapInstance.Buttons.Find(s => s.MapButtonId == TransportId);
                if (button != null)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateDelay(2000, 1, $"#git^{button.MapButtonId}"));
                }
            }
            else
            {
                if (!session.CurrentMapInstance.DroppedList.ContainsKey(TransportId))
                {
                    return;
                }

                MapItem mapItem = session.CurrentMapInstance.DroppedList[TransportId];

                if (mapItem != null)
                {
                    bool canpick = false;
                    switch (PickerType)
                    {
                        case 1:
                            canpick = session.Character.IsInRange(mapItem.PositionX, mapItem.PositionY, 8);
                            break;

                        case 2:
                            Mate mate = session.Character.Mates.Find(s =>
                                s.MateTransportId == PickerId && s.CanPickUp);
                            if (mate != null)
                            {
                                canpick = mate.IsInRange(mapItem.PositionX, mapItem.PositionY, 8);
                            }

                            break;
                    }

                    if (canpick && session.HasCurrentMapInstance)
                    {
                        if (mapItem is MonsterMapItem item)
                        {
                            MonsterMapItem monsterMapItem = item;
                            if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.LodInstance
                                && monsterMapItem.OwnerId.HasValue && monsterMapItem.OwnerId.Value != -1)
                            {
                                Group group = ServerManager.Instance.Groups.Find(g =>
                                    g.IsMemberOfGroup(monsterMapItem.OwnerId.Value)
                                    && g.IsMemberOfGroup(session.Character.CharacterId));
                                if (item.CreatedDate.AddSeconds(30) > DateTime.UtcNow
                                    && !(monsterMapItem.OwnerId == session.Character.CharacterId
                                      || (group?.SharingMode == (byte)GroupSharingType.Everyone)))
                                {
                                    session.SendPacket(
                                            session.Character.GenerateSay(
                                                Language.Instance.GetMessageFromKey("NOT_YOUR_ITEM"), 10));
                                    return;
                                }
                            }

                            // initialize and rarify
                            item.Rarify(null);
                        }

                        if (mapItem.ItemVNum != 1046)
                        {
                            ItemInstance mapItemInstance = mapItem.GetItemInstance();
                            if (mapItemInstance.Item.ItemType == ItemType.Map)
                            {
                                if (mapItemInstance.Item.Effect == 71)
                                {
                                    session.Character.SpPoint += mapItem.GetItemInstance().Item.EffectValue;
                                    if (session.Character.SpPoint > 10000)
                                    {
                                        session.Character.SpPoint = 10000;
                                    }

                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                        string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"),
                                            mapItem.GetItemInstance().Item.EffectValue), 0));
                                    session.SendPacket(session.Character.GenerateSpPoint());
                                }

                                if (ServerManager.Instance.QuestModelList.FirstOrDefault(s =>
                                    s.QuestGiver.QuestGiverId == mapItemInstance.Item.VNum
                                    && s.QuestGiver.Type == QuestGiverType.ItemLoot) is QuestModel model)
                                {
                                    session.Character.QuestManager.AddQuest(model.QuestId);
                                }

                                session.CurrentMapInstance.DroppedList.Remove(TransportId);
                                session.CurrentMapInstance?.Broadcast(
                                    session.Character.GenerateGet(PickerId, TransportId));
                            }
                            else
                            {
                                lock (session.Character.Inventory)
                                {
                                    short amount = mapItem.Amount;
                                    ItemInstance inv = session.Character.Inventory.AddToInventory(mapItemInstance)
                                        .FirstOrDefault();
                                    if (inv != null)
                                    {
                                        session.CurrentMapInstance.DroppedList.Remove(TransportId);
                                        session.CurrentMapInstance?.Broadcast(
                                            session.Character.GenerateGet(PickerId, TransportId));
                                        if (PickerType == 2)
                                        {
                                            Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == PickerId && s.CanPickUp);
                                            if (mate != null)
                                            {
                                                session.SendPacket(session.Character.GenerateIcon(1, 1, inv.ItemVNum));
                                                mate.Owner?.Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId, 5004), mate.PositionX, mate.PositionY);
                                            }
                                            else
                                            {
                                                return;
                                            }
                                        }

                                        session.SendPacket(session.Character.GenerateSay(
                                            $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {inv.Item.Name} x {amount}",
                                            12));
                                        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.LodInstance)
                                        {
                                            session.CurrentMapInstance?.Broadcast(
                                                session.Character.GenerateSay(
                                                    $"{string.Format(Language.Instance.GetMessageFromKey("ITEM_ACQUIRED_LOD"), session.Character.Name)}: {inv.Item.Name} x {mapItem.Amount}",
                                                    10));
                                        }

                                        session.Character.OnPickupItem(new PickupItemEventArgs(inv.Item));
                                    }
                                    else
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                    }
                                }
                            }
                        }
                        else
                        {
                            // handle gold drop
                            long maxGold = ServerManager.Instance.Configuration.MaxGold;
                            double multiplier =
                                1 + (session.Character.GetBuff(BCardType.CardType.Item,
                                         (byte)AdditionalTypes.Item.IncreaseEarnedGold)[0] / 100D);
                            multiplier +=
                            (session.Character.ShellEffectMain.FirstOrDefault(s =>
                                 s.Effect == (byte)ShellWeaponEffectType.GainMoreGold)?.Value ?? 0) / 100D;
                            if (mapItem is MonsterMapItem droppedGold)
                            {
                                if (session.Character.Gold + (droppedGold.GoldAmount * multiplier) <= maxGold)
                                {
                                    if (PickerType == 2)
                                    {
                                        session.SendPacket(session.Character.GenerateIcon(1, 1, 1046));
                                    }

                                    session.Character.Gold += (int) (droppedGold.GoldAmount * multiplier);
                                    GameLogger.Instance.LogPickupGold(ServerManager.Instance.ChannelId,
                                        session.Character.Name, session.Character.CharacterId,
                                        (int) (droppedGold.GoldAmount * multiplier), false);
                                    session.SendPacket(session.Character.GenerateSay(
                                        $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {mapItem.GetItemInstance().Item.Name} x {droppedGold.GoldAmount}{(multiplier > 1 ? $" + {(int) (droppedGold.GoldAmount * multiplier) - droppedGold.GoldAmount}" : string.Empty)}",
                                        12));
                                }
                                else
                                {
                                    session.Character.Gold = maxGold;
                                    GameLogger.Instance.LogPickupGold(ServerManager.Instance.ChannelId,
                                        session.Character.Name, session.Character.CharacterId,
                                        (int) (droppedGold.GoldAmount * multiplier), true);
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"),
                                            0));
                                }
                            }

                            session.SendPacket(session.Character.GenerateGold());
                            session.CurrentMapInstance.DroppedList.Remove(TransportId);
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateGet(PickerId, TransportId));
                        }
                    }
                }
            }
        }

        #endregion
    }
}