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
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.Packets.SharedPacketMethods;
// ReSharper disable UnreachableCode

namespace OpenNos.Handler.Packets.WorldPackets.MinilandPackets
{
    [PacketHeader("mg")]
    public class MinilandGamePacket
    {
        #region Properties

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public byte Id { get; set; }

        public short MinigameVNum { get; set; }

        public int? Point { get; set; }

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 5)
            {
                return;
            }
            MinilandGamePacket packetDefinition = new MinilandGamePacket();
            if (byte.TryParse(packetSplit[2], out byte type)
                && byte.TryParse(packetSplit[3], out byte id)
                && short.TryParse(packetSplit[4], out short minigameVNum))
            {
                packetDefinition.Type = type;
                packetDefinition.Id = id;
                packetDefinition.MinigameVNum = minigameVNum;
                packetDefinition.Point = packetSplit.Length >= 6 && int.TryParse(packetSplit[5], out int point) ? point : (int?)null;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(MinilandGamePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            ClientSession client =
                ServerManager.Instance.Sessions.FirstOrDefault(s =>
                    s.Character?.Miniland == session.Character.MapInstance);
            MinilandObject mlobj =
                client?.Character.MinilandObjects.Find(s => s.ItemInstance.ItemVNum == MinigameVNum);
            if (mlobj != null)
            {
                const bool full = false;
                byte game = (byte)(mlobj.ItemInstance.Item.EquipmentSlot);
                switch (Type)
                {
                    //play
                    case 1:
                        if (mlobj.ItemInstance.DurabilityPoint <= 0)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("NOT_ENOUGH_DURABILITY_POINT"), 0));
                            return;
                        }

                        if (session.Character.MinilandPoint <= 0)
                        {
                            session.SendPacket(
                                $"qna #mg^1^7^3125^1^1 {Language.Instance.GetMessageFromKey("NOT_ENOUGH_MINILAND_POINT")}");
                        }

                        session.Character.MapInstance.Broadcast(
                            UserInterfaceHelper.GenerateGuri(2, 1, session.Character.CharacterId));
                        session.Character.CurrentMinigame = (short)(game == 0 ? 5102 :
                            game == 1 ? 5103 :
                            game == 2 ? 5105 :
                            game == 3 ? 5104 :
                            game == 4 ? 5113 : 5112);
                        session.Character.MinigameLog = new MinigameLogDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            StartTime = DateTime.UtcNow.Ticks,
                            Minigame = game
                        };
                        session.SendPacket($"mlo_st {game}");
                        break;

                    //stop
                    case 2:
                        session.Character.CurrentMinigame = 0;
                        session.Character.MapInstance.Broadcast(
                            UserInterfaceHelper.GenerateGuri(6, 1, session.Character.CharacterId));
                        break;

                    case 3:
                        session.Character.CurrentMinigame = 0;
                        session.Character.MapInstance.Broadcast(
                            UserInterfaceHelper.GenerateGuri(6, 1, session.Character.CharacterId));
                        if (Point.HasValue && session.Character.MinigameLog != null)
                        {
                            session.Character.MinigameLog.EndTime = DateTime.UtcNow.Ticks;
                            session.Character.MinigameLog.Score = Point.Value;

                            int level = -1;
                            for (short i = 0; i < SharedMinilandMethods.GetMinilandMaxPoint(game).Length; i++)
                            {
                                if (Point.Value > SharedMinilandMethods.GetMinilandMaxPoint(game)[i])
                                {
                                    level = i;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            session.SendPacket(level != -1
                                ? $"mlo_lv {level}"
                                : $"mg 3 {game} {MinigameVNum} 0 0");
                        }

                        break;

                    // select gift
                    case 4:
                        if (session.Character.MinilandPoint >= 100
                            && session.Character.MinigameLog != null
                            && Point.HasValue && Point.Value > 0
                            && SharedMinilandMethods.GetMinilandMaxPoint(game)[Point.Value - 1] < session.Character.MinigameLog.Score)
                        {
                            MinigameLogDTO dto = session.Character.MinigameLog;
                            DAOFactory.MinigameLogDAO.InsertOrUpdate(ref dto);
                            session.Character.MinigameLog = null;
                            Gift obj = SharedMinilandMethods.GetMinilandGift(MinigameVNum, Point.Value);
                            if (obj != null)
                            {
                                session.SendPacket($"mlo_rw {obj.VNum} {obj.Amount}");
                                session.SendPacket(session.Character.GenerateMinilandPoint());
                                List<ItemInstance> inv =
                                    session.Character.Inventory.AddNewToInventory(obj.VNum, obj.Amount);
                                session.Character.MinilandPoint -= 100;
                                if (inv.Count == 0)
                                {
                                    session.Character.SendGift(session.Character.CharacterId, obj.VNum, obj.Amount,
                                        0, 0, false);
                                }

                                if (client != session)
                                {
                                    switch (Point.Value)
                                    {
                                        case 0:
                                            mlobj.Level1BoxAmount++;
                                            break;

                                        case 1:
                                            mlobj.Level2BoxAmount++;
                                            break;

                                        case 2:
                                            mlobj.Level3BoxAmount++;
                                            break;

                                        case 3:
                                            mlobj.Level4BoxAmount++;
                                            break;

                                        case 4:
                                            mlobj.Level5BoxAmount++;
                                            break;
                                    }
                                }
                            }
                        }

                        break;

                    case 5:
                        session.SendPacket(session.Character.GenerateMloMg(mlobj, MinigameVNum));
                        break;

                    //refill
                    case 6:
                        if (!Point.HasValue || Point.Value < 0)
                        {
                            return;
                        }

                        if (session.Character.Gold > Point)
                        {
                            session.Character.Gold -= Point.Value;
                            session.SendPacket(session.Character.GenerateGold());
                            mlobj.ItemInstance.DurabilityPoint += Point.Value / 100;
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                string.Format(Language.Instance.GetMessageFromKey("REFILL_MINIGAME"),
                                    Point.Value / 100)));
                            session.SendPacket(session.Character.GenerateMloMg(mlobj, MinigameVNum));
                        }

                        break;

                    //gift
                    case 7:
                        session.SendPacket(
                            $"mlo_pmg {MinigameVNum} {session.Character.MinilandPoint} {(mlobj.ItemInstance.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} {(mlobj.Level1BoxAmount > 0 ? $"392 {mlobj.Level1BoxAmount}" : "0 0")} {(mlobj.Level2BoxAmount > 0 ? $"393 {mlobj.Level2BoxAmount}" : "0 0")} {(mlobj.Level3BoxAmount > 0 ? $"394 {mlobj.Level3BoxAmount}" : "0 0")} {(mlobj.Level4BoxAmount > 0 ? $"395 {mlobj.Level4BoxAmount}" : "0 0")} {(mlobj.Level5BoxAmount > 0 ? $"396 {mlobj.Level5BoxAmount}" : "0 0")} 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
                        break;

                    //get gift
                    case 8:
                        if (!Point.HasValue)
                        {
                            return;
                        }
                        int amount = 0;
                        switch (Point.Value)
                        {
                            case 0:
                                amount = mlobj.Level1BoxAmount;
                                break;

                            case 1:
                                amount = mlobj.Level2BoxAmount;
                                break;

                            case 2:
                                amount = mlobj.Level3BoxAmount;
                                break;

                            case 3:
                                amount = mlobj.Level4BoxAmount;
                                break;

                            case 4:
                                amount = mlobj.Level5BoxAmount;
                                break;
                        }

                        List<Gift> gifts = new List<Gift>();
                        for (int i = 0; i < amount; i++)
                        {
                            Gift gift = SharedMinilandMethods.GetMinilandGift(MinigameVNum, Point.Value);
                            if (gift != null)
                            {
                                if (gifts.Any(o => o.VNum == gift.VNum))
                                {
                                    gifts.First(o => o.Amount == gift.Amount).Amount += gift.Amount;
                                }
                                else
                                {
                                    gifts.Add(gift);
                                }
                            }
                        }

                        string str = string.Empty;
                        for (int i = 0; i < 9; i++)
                        {
                            if (gifts.Count > i)
                            {
                                short itemVNum = gifts[i].VNum;
                                byte itemAmount = gifts[i].Amount;
                                List<ItemInstance> inv =
                                    session.Character.Inventory.AddNewToInventory(itemVNum, itemAmount);
                                if (inv.Count > 0)
                                {
                                    session.SendPacket(session.Character.GenerateSay(
                                        $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(itemVNum).Name} x {itemAmount}",
                                        12));
                                }
                                else
                                {
                                    session.Character.SendGift(session.Character.CharacterId, itemVNum, itemAmount, 0,
                                        0, false);
                                }

                                str += $" {itemVNum} {itemAmount}";
                            }
                            else
                            {
                                str += " 0 0";
                            }
                        }

                        session.SendPacket(
                            $"mlo_pmg {MinigameVNum} {session.Character.MinilandPoint} {(mlobj.ItemInstance.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} {(mlobj.Level1BoxAmount > 0 ? $"392 {mlobj.Level1BoxAmount}" : "0 0")} {(mlobj.Level2BoxAmount > 0 ? $"393 {mlobj.Level2BoxAmount}" : "0 0")} {(mlobj.Level3BoxAmount > 0 ? $"394 {mlobj.Level3BoxAmount}" : "0 0")} {(mlobj.Level4BoxAmount > 0 ? $"395 {mlobj.Level4BoxAmount}" : "0 0")} {(mlobj.Level5BoxAmount > 0 ? $"396 {mlobj.Level5BoxAmount}" : "0 0")}{str}");
                        break;

                    //coupon
                    case 9:
                        List<ItemInstance> items = session.Character.Inventory
                            .Where(s => s.ItemVNum == 1269 || s.ItemVNum == 1271).OrderBy(s => s.Slot).ToList();
                        if (items.Count > 0)
                        {
                            short itemVNum = items[0].ItemVNum;
                            session.Character.Inventory.RemoveItemAmount(itemVNum);
                            int point = itemVNum == 1269 ? 300 : 500;
                            mlobj.ItemInstance.DurabilityPoint += point;
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                string.Format(Language.Instance.GetMessageFromKey("REFILL_MINIGAME"), point)));
                            session.SendPacket(session.Character.GenerateMloMg(mlobj, MinigameVNum));
                        }

                        break;
                }
            }
        }

        #endregion
    }
}