// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using System;
using System.Linq;
using OpenNos.ChatLog.Networking;
using OpenNos.ChatLog.Shared;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Core.Otp;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("guri")]
    public class GuriPacket
    {
        #region Properties

        public int Argument { get; set; }

        public int? Data { get; set; }

        public long Parameter { get; set; }

        public int Type { get; set; }

        public string Value { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] { ' ' }, 7);
            if (packetSplit.Length < 3)
            {
                return;
            }
            GuriPacket packetDefinition = new GuriPacket();
            if (int.TryParse(packetSplit[2], out int type))
            {
                packetDefinition.Type = type;
                packetDefinition.Argument = packetSplit.Length > 3
                    && int.TryParse(packetSplit[3], out int argument) ? argument : 0;
                packetDefinition.Parameter = packetSplit.Length > 4
                    && long.TryParse(packetSplit[4], out long parameter) ? parameter : 0;
                packetDefinition.Data = packetSplit.Length > 5
                    && int.TryParse(packetSplit[5], out int data) ? data : (int?)null;
                packetDefinition.Value = packetSplit.Length > 6 ? packetSplit[6] : string.Empty;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GuriPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Account.IsVerified || Type == 4 && Argument == 11)
            {
                if (Data.HasValue && Type == 10 && Data.Value >= 973
                    && Data.Value <= 999 && !session.Character.EmoticonsBlocked)
                {
                    if (Parameter == session.Character.CharacterId)
                    {
                        session.CurrentMapInstance?.Broadcast(session,
                            StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId,
                                Data.Value + 4099), ReceiverType.AllNoEmoBlocked);
                    }
                    else if (Parameter.TryCastToInt(out int mateTransportId))
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == mateTransportId);
                        if (mate != null)
                        {
                            session.CurrentMapInstance?.Broadcast(session,
                                StaticPacketHelper.GenerateEff(UserType.Npc, mate.MateTransportId,
                                    Data.Value + 4099), ReceiverType.AllNoEmoBlocked);
                        }
                    }
                }
                else if (Type == 204)
                {
                    if (Argument == 0 && short.TryParse(Parameter.ToString(), out short slot))
                    {
                        ItemInstance shell =
                            session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                        if (shell?.ShellEffects.Count == 0 && shell.Upgrade > 0 && shell.Rare > 0
                            && session.Character.Inventory.CountItem(1429) >= ((shell.Upgrade / 10) + shell.Rare))
                        {
                            shell.SetShellEffects();
                            session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("OPTION_IDENTIFIED"), 0));
                            session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                                session.Character.CharacterId, 3006));
                            session.Character.Inventory.RemoveItemAmount(1429, (shell.Upgrade / 10) + shell.Rare);
                        }
                    }
                }
                else if (Type == 205)
                {
                    if (Parameter.TryCastToShort(out short slot))
                    {
                        ItemInstance inv = session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                        if (inv.Rare < 1 || inv.Rare > 8 || inv.Item.LevelMinimum > 99 ||
                            inv.BoundCharacterId == null)
                        {
                            return;
                        }

                        short[][] parfumeReq =
                        {
                            new short[] {0, 0, 0, 0, 0, 5, 10, 10, 20},
                            new short[] {0, 0, 0, 0, 5, 10, 10, 20, 40},
                            new short[] {0, 0, 0, 5, 10, 10, 20, 40, 80},
                            new short[] {0, 0, 5, 5, 10, 20, 40, 80, 120},
                            new short[] {0, 0, 5, 10, 20, 40, 80, 120, 160},
                            new short[] {0, 0, 5, 20, 40, 80, 120, 160, 200},
                            new short[] {0, 0, 10, 40, 80, 120, 160, 200, 300},
                            new short[] {0, 0, 10, 40, 80, 120, 160, 200, 400}
                        };
                        int[] goldReq =
                        {
                            1000,
                            2000,
                            5000,
                            8000,
                            10000,
                            12500,
                            15000,
                            17500,
                            20000,
                            30000
                        };

                        if (session.Character.Inventory.CountItem(1428)
                            >= parfumeReq[inv.Rare - 1][(inv.Item.LevelMinimum / 10) - 1]
                            && session.Character.Gold >= goldReq[(inv.Item.LevelMinimum / 10) - 1])
                        {
                            session.Character.Inventory.RemoveItemAmount(1428,
                                parfumeReq[inv.Rare - 1][(inv.Item.LevelMinimum / 10) - 1]);
                            session.Character.Gold -= goldReq[(inv.Item.LevelMinimum / 10) - 1];
                            session.SendPacket(session.Character.GenerateGold());
                            inv.BoundCharacterId = session.Character.CharacterId;
                            session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("SHELL_PERFUMED"), 0));
                        }
                    }
                }
                else if (Type == 300)
                {
                    if (Argument == 8023 && Parameter.TryCastToShort(out short slot))
                    {
                        ItemInstance box = session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                        if (box != null)
                        {
                            box.Item.Use(session, ref box, 1, new[] {Data?.ToString() ?? string.Empty});
                        }
                    }
                }
                else if (Type == 506)
                {
                    session.Character.IsWaitingForEvent |= ServerManager.Instance.EventInWaiting;
                }
                else if (Type == 199 && Argument == 2)
                {
                    short[] listWingOfFriendship = {2160, 2312, 10048};
                    short vnumToUse = -1;
                    foreach (short vnum in listWingOfFriendship)
                    {
                        if (session.Character.Inventory.CountItem(vnum) > 0)
                        {
                            vnumToUse = vnum;
                        }
                    }

                    if (vnumToUse != -1 || session.Character.IsSpouseOfCharacter(Parameter))
                    {
                        ClientSession sess = ServerManager.Instance.GetSessionByCharacterId(Parameter);
                        if (sess != null)
                        {
                            if (session.Character.IsFriendOfCharacter(Parameter))
                            {
                                if (sess.CurrentMapInstance?.MapInstanceType == MapInstanceType.BaseMapInstance)
                                {
                                    if (session.Character.MapInstance.MapInstanceType
                                        != MapInstanceType.BaseMapInstance
                                        || (ServerManager.Instance.ChannelId == 51
                                            && session.Character.Faction != sess.Character.Faction))
                                    {
                                        session.SendPacket(
                                            session.Character.GenerateSay(
                                                Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                                        return;
                                    }

                                    short mapy = sess.Character.PositionY;
                                    short mapx = sess.Character.PositionX;
                                    short mapId = sess.Character.MapInstance.Map.MapId;

                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, mapId, mapx, mapy);
                                    if (!session.Character.IsSpouseOfCharacter(Parameter))
                                    {
                                        session.Character.Inventory.RemoveItemAmount(vnumToUse);
                                    }
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("USER_ON_INSTANCEMAP"), 0));
                                }
                            }
                        }
                        else
                        {
                            session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                        }
                    }
                    else
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_WINGS"), 10));
                    }
                }
                else if (Type == 400)
                {
                    if (!session.HasCurrentMapInstance)
                    {
                        return;
                    }

                    MapNpc npc = session.CurrentMapInstance.Npcs.Find(n => n.MapNpcId.Equals(Argument));
                    if (npc != null)
                    {
                        NpcMonster mapobject = ServerManager.GetNpcMonster(npc.NpcVNum);

                        int rateDrop = ServerManager.Instance.Configuration.RateDrop;
                        int delay = (int) Math.Round(
                            (3 + (mapobject.RespawnTime / 1000d)) * session.Character.TimesUsed);
                        delay = delay > 11 ? 8 : delay;
                        if (session.Character.LastMapObject.AddSeconds(delay) < DateTime.UtcNow)
                        {
                            if (mapobject.Drops.Any(s => s.MonsterVNum != null) && mapobject.VNumRequired > 10
                                                                                && session.Character.Inventory
                                                                                    .CountItem(mapobject.VNumRequired)
                                                                                < mapobject.AmountRequired)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEM"), 0));
                                return;
                            }

                            Random random = new Random();
                            double randomAmount = ServerManager.RandomNumber() * random.NextDouble();
                            DropDTO drop = mapobject.Drops.Find(s => s.MonsterVNum == npc.NpcVNum);
                            if (drop != null)
                            {
                                int dropChance = drop.DropChance;
                                if (randomAmount <= (double) dropChance * rateDrop / 5000.000)
                                {
                                    short vnum = drop.ItemVNum;
                                    ItemInstance newInv = session.Character.Inventory.AddNewToInventory(vnum)
                                        .FirstOrDefault();
                                    session.Character.LastMapObject = DateTime.UtcNow;
                                    session.Character.TimesUsed++;
                                    if (session.Character.TimesUsed >= 4)
                                    {
                                        session.Character.TimesUsed = 0;
                                    }

                                    if (newInv != null)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"),
                                                newInv.Item.Name), 0));
                                        session.SendPacket(session.Character.GenerateSay(
                                            string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"),
                                                newInv.Item.Name), 11));
                                    }
                                    else
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                                    }
                                }
                                else
                                {
                                    session.SendPacket(
                                        UserInterfaceHelper.GenerateMsg(
                                            Language.Instance.GetMessageFromKey("TRY_FAILED"), 0));
                                }
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                string.Format(Language.Instance.GetMessageFromKey("TRY_FAILED_WAIT"),
                                    (int) (session.Character.LastMapObject.AddSeconds(delay) - DateTime.UtcNow)
                                    .TotalSeconds), 0));
                        }
                    }
                }
                else if (Type == 710)
                {
                    if (Value != null)
                    {
                        // TODO: MAP TELEPORTER
                    }
                }
                else if (Type == 750)
                {
                    const short baseVnum = 1623;
                    if (Argument.TryCastToByte(out byte faction)
                        && (Enum.IsDefined(typeof(FactionType), faction)
                            || Enum.IsDefined(typeof(FactionType), (byte) (faction / 2)))
                        && session.Character.Inventory.CountItem(baseVnum + faction) > 0)
                    {
                        if (faction < 3)
                        {
                            if (session.Character.Family != null)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("IN_FAMILY"),
                                        0));
                                return;
                            }

                            session.Character.Faction = (FactionType) faction;
                            session.Character.Inventory.RemoveItemAmount(baseVnum + faction);
                            session.SendPacket("scr 0 0 0 0 0 0 0");
                            session.SendPacket(session.Character.GenerateFaction());
                            session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                                session.Character.CharacterId, 4799 + faction));
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction}"), 0));
                        }
                        else
                        {
                            if (session.Character.Family == null || session.Character.Family.FamilyCharacters
                                    .Find(s => s.Authority.Equals(FamilyAuthority.Head))?.CharacterId
                                    .Equals(session.Character.CharacterId) != true)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAMILY"),
                                        0));
                                return;
                            }

                            if (session.Character.Family.LastFactionChange > DateTime.UtcNow.AddDays(-1).Ticks)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("CHANGE_NOT PERMITTED"), 0));
                                return;
                            }

                            session.Character.Faction = (FactionType) (faction / 2);
                            session.Character.Inventory.RemoveItemAmount(baseVnum + faction);
                            session.SendPacket("scr 0 0 0 0 0 0 0");
                            session.SendPacket(session.Character.GenerateFaction());
                            session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                                session.Character.CharacterId, 4799 + (faction / 2)));
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_{faction / 2}"), 0));
                            session.Character.Family.LastFactionChange = DateTime.UtcNow.Ticks;
                            session.Character.Save();
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                            CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                            {
                                DestinationCharacterId = session.Character.Family.FamilyId,
                                SourceCharacterId = 0,
                                SourceWorldId = ServerManager.Instance.WorldId,
                                Message = "fhis_stc",
                                Type = MessageType.Family
                            });
                        }
                    }
                }
                else if (Type == 2)
                {
                    session.CurrentMapInstance?.Broadcast(
                        UserInterfaceHelper.GenerateGuri(2, 1, session.Character.CharacterId),
                        session.Character.PositionX, session.Character.PositionY);
                }
                else if (Type == 4)
                {
                    const int speakerVNum = 2173;
                    const int petnameVNum = 2157;
                    if (Argument == 1 && Data.HasValue)
                    {
                        Mate mate = session.Character.Mates.Find(s => s.MateTransportId == Data.Value);
                        if (mate != null && session.Character.Inventory.CountItem(petnameVNum) > 0)
                        {
                            mate.Name = Value.Truncate(16);
                            session.CurrentMapInstance?.Broadcast(mate.GenerateOut(), ReceiverType.AllExceptMe);
                            session.CurrentMapInstance?.Broadcast(mate.GenerateIn());
                            session.SendPacket(mate.GenerateCond());
                            session.SendPacket(
                                UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NEW_NAME_PET")));
                            session.SendPacket(session.Character.GeneratePinit());
                            session.SendPackets(session.Character.GeneratePst());
                            session.SendPackets(session.Character.GenerateScP());
                            session.Character.Inventory.RemoveItemAmount(petnameVNum);
                        }
                    }

                    // presentation message
                    if (Argument == 2)
                    {
                        int presentationVNum = session.Character.Inventory.CountItem(1117) > 0
                            ? 1117
                            : (session.Character.Inventory.CountItem(9013) > 0 ? 9013 : -1);
                        if (presentationVNum != -1)
                        {
                            string message = string.Empty;
                            string[] valuesplit = Value?.Split(' ');
                            if (valuesplit == null)
                            {
                                return;
                            }

                            for (int i = 0; i < valuesplit.Length; i++)
                            {
                                message += valuesplit[i] + "^";
                            }

                            message = message.Substring(0, message.Length - 1); // Remove the last ^
                            message = message.Trim();
                            if (message.Length > 60)
                            {
                                message = message.Substring(0, 60);
                            }

                            session.Character.Biography = message;
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("INTRODUCTION_SET"),
                                    10));
                            session.Character.Inventory.RemoveItemAmount(presentationVNum);
                        }
                    }

                    // Speaker
                    if (Argument == 3 && session.Character.Inventory.CountItem(speakerVNum) > 0)
                    {
                        string message =
                            $"[{session.Character.Name}]:";
                        int baseLength = message.Length;
                        string[] valuesplit = Value?.Split(' ');
                        if (valuesplit == null)
                        {
                            return;
                        }

                        for (int i = 0; i < valuesplit.Length; i++)
                        {
                            message += valuesplit[i] + " ";
                        }

                        if (message.Length > 120 + baseLength)
                        {
                            message = message.Substring(0, 120 + baseLength);
                        }

                        message = message.Replace("\n", string.Empty).Replace("\r", string.Empty)
                            .Replace($"<{Language.Instance.GetMessageFromKey("SPEAKER")}>", string.Empty).Trim();
                        message = $"<{Language.Instance.GetMessageFromKey("SPEAKER")}> {message}";
                        if (session.Character.IsMuted())
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(
                                    Language.Instance.GetMessageFromKey("SPEAKER_CANT_BE_USED"), 10));
                            return;
                        }

                        session.Character.Inventory.RemoveItemAmount(speakerVNum);
                        ServerManager.Instance.Broadcast(session.Character.GenerateSay(message, 13));

                        if (ServerManager.Instance.Configuration.UseChatLogService)
                        {
                            ChatLogServiceClient.Instance.LogChatMessage(new ChatLogEntry
                            {
                                Sender = session.Character.Name,
                                SenderId = session.Character.CharacterId,
                                Receiver = null,
                                ReceiverId = null,
                                MessageType = ChatLogType.Speaker,
                                Message = message
                            });
                        }
                    }

                    if (Argument == 11 && !string.IsNullOrWhiteSpace(Value) &&
                        !string.IsNullOrWhiteSpace(session.Account.TotpSecret))
                    {
                        Totp totp = new Totp(Base32Encoding.ToBytes(session.Account.TotpSecret));
                        if (totp.VerifyTotp(Value, out long _, VerificationWindow.RfcSpecifiedNetworkDelay))
                        {
                            session.Character.GeneralLogs.Add(new GeneralLogDTO
                            {
                                AccountId = session.Account.AccountId,
                                IpAddress = session.IpAddress,
                                LogType = GeneralLogType.TOTP.ToString(),
                                LogData = "SUCCESS",
                                Timestamp = DateTime.UtcNow
                            });
                            session.Account.IsVerified = true;
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TOTP_VERIFIED"), 12));
                        }
                        else
                        {
                            session.Character.GeneralLogs.Add(new GeneralLogDTO
                            {
                                AccountId = session.Account.AccountId,
                                IpAddress = session.IpAddress,
                                LogType = GeneralLogType.TOTP.ToString(),
                                LogData = "FAIL",
                                Timestamp = DateTime.UtcNow
                            });
                            session.Disconnect();
                        }
                    }
                }
                else if (Type == 199 && Argument == 1)
                {
                    if (!session.Character.IsFriendOfCharacter(Parameter))
                    {
                        session.SendPacket(Language.Instance.GetMessageFromKey("CHARACTER_NOT_IN_FRIENDLIST"));
                        return;
                    }

                    session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 4, $"#guri^199^2^{Parameter}"));
                }
                else if (Type == 201)
                {
                    if (session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBasket))
                    {
                        session.SendPacket(session.Character.GenerateStashAll());
                    }
                }
                else if (Type == 202)
                {
                    session.SendPacket(
                        session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PARTNER_BACKPACK"), 10));
                    session.SendPacket(session.Character.GeneratePStashAll());
                }
                else if (Type == 208 && Argument == 0)
                {
                    if (short.TryParse(Value, out short mountSlot)
                        && Parameter.TryCastToShort(out short pearlSlot))
                    {
                        ItemInstance mount =
                            session.Character.Inventory.LoadBySlotAndType<ItemInstance>(mountSlot, InventoryType.Main);
                        ItemInstance pearl =
                            session.Character.Inventory.LoadBySlotAndType(pearlSlot, InventoryType.Equipment);
                        if (mount != null && pearl != null)
                        {
                            pearl.HoldingVNum = mount.ItemVNum;
                            session.Character.Inventory.RemoveItemFromInventory(mount.Id);
                        }
                    }
                }
                else if (Type == 209 && Argument == 0)
                {
                    if (short.TryParse(Value, out short mountSlot)
                        && Parameter.TryCastToShort(out short pearlSlot))
                    {
                        ItemInstance fairy =
                            session.Character.Inventory.LoadBySlotAndType(mountSlot, InventoryType.Equipment);
                        ItemInstance pearl =
                            session.Character.Inventory.LoadBySlotAndType(pearlSlot, InventoryType.Equipment);
                        if (fairy != null && pearl != null)
                        {
                            pearl.HoldingVNum = fairy.ItemVNum;
                            pearl.ElementRate = fairy.ElementRate;
                            session.Character.Inventory.RemoveItemFromInventory(fairy.Id);
                        }
                    }
                }
                else if (Type == 203 && Argument == 0)
                {
                    // SP points initialization
                    int[] listPotionResetVNums = {1366, 1427, 5115, 9040};
                    int vnumToUse = -1;
                    foreach (int vnum in listPotionResetVNums)
                    {
                        if (session.Character.Inventory.CountItem(vnum) > 0)
                        {
                            vnumToUse = vnum;
                        }
                    }

                    if (vnumToUse != -1)
                    {
                        if (session.Character.UseSp)
                        {
                            ItemInstance specialistInstance =
                                session.Character.Inventory.LoadBySlotAndType((byte) EquipmentType.Sp,
                                    InventoryType.Wear);
                            if (specialistInstance != null)
                            {
                                specialistInstance.SlDamage = 0;
                                specialistInstance.SlDefence = 0;
                                specialistInstance.SlElement = 0;
                                specialistInstance.SlHP = 0;

                                specialistInstance.DamageMinimum = 0;
                                specialistInstance.DamageMaximum = 0;
                                specialistInstance.HitRate = 0;
                                specialistInstance.CriticalLuckRate = 0;
                                specialistInstance.CriticalRate = 0;
                                specialistInstance.DefenceDodge = 0;
                                specialistInstance.DistanceDefenceDodge = 0;
                                specialistInstance.ElementRate = 0;
                                specialistInstance.DarkResistance = 0;
                                specialistInstance.LightResistance = 0;
                                specialistInstance.FireResistance = 0;
                                specialistInstance.WaterResistance = 0;
                                specialistInstance.CriticalDodge = 0;
                                specialistInstance.CloseDefence = 0;
                                specialistInstance.DistanceDefence = 0;
                                specialistInstance.MagicDefence = 0;
                                specialistInstance.HP = 0;
                                specialistInstance.MP = 0;

                                session.Character.Inventory.RemoveItemAmount(vnumToUse);
                                session.Character.Inventory.DeleteFromSlotAndType((byte) EquipmentType.Sp,
                                    InventoryType.Wear);
                                session.Character.Inventory.AddToInventoryWithSlotAndType(specialistInstance,
                                    InventoryType.Wear, (byte) EquipmentType.Sp);
                                session.SendPacket(session.Character.GenerateCond());
                                session.SendPacket(specialistInstance.GenerateSlInfo());
                                session.SendPacket(session.Character.GenerateLev());
                                session.SendPacket(session.Character.GenerateStatChar());
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("POINTS_RESET"),
                                        0));
                            }
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(
                                    Language.Instance.GetMessageFromKey("TRANSFORMATION_NEEDED"), 10));
                        }
                    }
                    else
                    {
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_POINTS"),
                                10));
                    }
                }
            }
        }

        #endregion
    }
}