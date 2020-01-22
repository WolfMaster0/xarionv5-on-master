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
using System.Diagnostics;
using System.Linq;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject.Event.TALENTARENA;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.GameObject.Npc
{
    public static class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession session, int npcId, short runner, short type, short value)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }
            MapNpc npc = session.CurrentMapInstance.Npcs.Find(s => s.MapNpcId == npcId);
            TeleporterDTO tp;
            switch (runner)
            {
                case 1:
                    if (session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }
                    if (session.Character.Level < 15 || session.Character.JobLevel < 20)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                        return;
                    }
                    if (type == (byte)session.Character.Class || type < 1 || type > 3)
                    {
                        return;
                    }
                    if (session.Character.Inventory.All(i => i.Type != InventoryType.Wear))
                    {
                        session.Character.Inventory.AddNewToInventory((short)(4 + (type * 14)), type: InventoryType.Wear);
                        session.Character.Inventory.AddNewToInventory((short)(81 + (type * 13)), type: InventoryType.Wear);
                        switch (type)
                        {
                            case 1:
                                session.Character.Inventory.AddNewToInventory(68, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2082, 10);
                                break;

                            case 2:
                                session.Character.Inventory.AddNewToInventory(78, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2083, 10);
                                break;

                            case 3:
                                session.Character.Inventory.AddNewToInventory(86, type: InventoryType.Wear);
                                break;
                        }
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.Character.ChangeClass((ClassType)type);
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    session.SendPacket("wopen 1 0");
                    break;

                case 4:
                    Mate mate = session.Character.Mates.Find(s => s.MateTransportId == npcId);
                    switch (type)
                    {
                        case 2:
                            if (mate != null)
                            {
                                if (session.Character.Level >= mate.Level)
                                {
                                    Mate teammate = session.Character.Mates.Where(s => s.IsTeamMember).FirstOrDefault(s => s.MateType == mate.MateType);
                                    teammate?.LeaveTeam();

                                    mate.JoinTeam();
                                }
                                else
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                                }
                            }
                            break;

                        case 3:
                            if (mate != null && session.Character.Miniland == session.Character.MapInstance)
                            {
                                mate.LeaveTeam();
                            }
                            break;

                        case 4:
                            if (mate != null)
                            {
                                if (session.Character.Miniland == session.Character.MapInstance)
                                {
                                    mate.LeaveTeam();
                                }
                                else
                                {
                                    session.SendPacket($"qna #n_run^4^5^3^{mate.MateTransportId} {Language.Instance.GetMessageFromKey("ASK_KICK_PET")}");
                                }
                            }
                            break;

                        case 5:
                            if (mate != null)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 10, $"#n_run^4^6^3^{mate.MateTransportId}"));
                            }
                            break;

                        case 6:
                            if (mate != null && session.Character.Miniland != session.Character.MapInstance)
                            {
                                mate.BackToMiniland();
                                session.CurrentMapInstance.Broadcast(mate.GenerateOut());

                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name), 11));
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name), 0));
                            }
                            break;

                        case 7:
                            if (mate != null)
                            {
                                if (!mate.IsSummonable)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NEED_SUMMONING_SCROLL")));
                                    return;
                                }

                                if (session.Character.Mates.Any(s => s.MateType == mate.MateType && s.IsTeamMember))
                                {
                                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 11));
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 0));
                                }
                                else
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 10, $"#n_run^4^9^3^{mate.MateTransportId}"));
                                }
                            }
                            break;

                        case 9:
                            if (mate != null)
                            {
                                mate.PositionX = (short)(session.Character.PositionX + 1);
                                mate.PositionY = (short)(session.Character.PositionY + 1);
                                mate.JoinTeam();
                                session.CurrentMapInstance.Broadcast(mate.GenerateIn());
                            }
                            else
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                            }
                            break;
                    }
                    session.SendPacket(session.Character.GeneratePinit());
                    session.SendPackets(session.Character.GeneratePst());
                    break;

                case 10:
                    session.SendPacket("wopen 3 0");
                    break;

                case 12:
                    session.SendPacket($"wopen {type} 0");
                    break;

                case 14:
                    session.SendPacket("wopen 27 0");
                    string recipelist = "m_list 2";
                    if (npc != null)
                    {
                        List<Recipe> tps = npc.Recipes;
                        recipelist = tps.Where(s => s.Amount > 0).Aggregate(recipelist, (current, s) => current + $" {s.ItemVNum}");
                        recipelist += " -100";
                        session.SendPacket(recipelist);
                    }
                    break;

                case 15:
                    if (npc != null)
                    {
                        if (value == 2)
                        {
                            session.SendPacket($"qna #n_run^15^1^1^{npc.MapNpcId} {Language.Instance.GetMessageFromKey("ASK_CHANGE_SPAWNLOCATION")}");
                        }
                        else
                        {
                            switch (npc.MapId)
                            {
                                case 1:
                                    session.Character.SetRespawnPoint(1, 79, 116);
                                    break;

                                case 20:
                                    session.Character.SetRespawnPoint(20, 9, 92);
                                    break;

                                case 145:
                                    session.Character.SetRespawnPoint(1, 81, 3);
                                    break;
                            }
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("RESPAWNLOCATION_CHANGED"), 0));
                        }
                    }
                    break;

                case 16:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 1000 * type)
                        {
                            session.Character.Gold -= 1000 * type;
                            session.SendPacket(session.Character.GenerateGold());
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                        }
                    }
                    break;

                case 17:
                    double currentRunningSeconds = (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.AddHours(-2).AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastPortal = currentRunningSeconds - session.Character.LastPortal;
                    if (!(timeSpanSinceLastPortal >= 4) || !session.HasCurrentMapInstance || ServerManager.Instance.ChannelId == 51 || session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.ArenaInstance.MapInstanceId || session.CurrentMapInstance.MapInstanceId == ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                        return;
                    }
                    if (session.Character.Gold >= 500 * (1 + type) && type > -1)
                    {
                        session.Character.LastPortal = currentRunningSeconds;
                        session.Character.Gold -= 500 * (1 + type);
                        session.SendPacket(session.Character.GenerateGold());
                        MapCell pos = type == 0 ? ServerManager.Instance.ArenaInstance.Map.GetRandomPosition() : ServerManager.Instance.FamilyArenaInstance.Map.GetRandomPosition();
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, type == 0 ? ServerManager.Instance.ArenaInstance.MapInstanceId : ServerManager.Instance.FamilyArenaInstance.MapInstanceId, pos.X, pos.Y);
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    }
                    break;

                case 18:
                    session.SendPacket(session.Character.GenerateNpcDialog(17));
                    break;

                case 26:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 5000 * type)
                        {
                            session.Character.Gold -= 5000 * type;
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                        }
                    }
                    break;

                case 45:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 500)
                        {
                            session.Character.Gold -= 500;
                            session.SendPacket(session.Character.GenerateGold());
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                        }
                    }
                    break;

                case 132:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }
                    break;

                case 135:
                    if (TalentArena.IsRunning)
                    {
                        TalentArena.RegisteredParticipants[session.Character.CharacterId] = session;
                        session.SendPacket(UserInterfaceHelper.GenerateBsInfo(0, 3, 300, 5));
                    }
                    break;

                case 150:
                    if (npc != null)
                    {
                        if (session.Character.Family != null)
                        {
                            if (session.Character.Family.LandOfDeath != null && npc.EffectActivated)
                            {
                                if (session.Character.Level >= 55)
                                {
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, session.Character.Family.LandOfDeath.MapInstanceId, 153, 145);
                                }
                                else
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOD_REQUIERE_LVL"), 0));
                                }
                            }
                            else
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOD_CLOSED"), 0));
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FAMILY"), 0));
                        }
                    }
                    break;

                case 301:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }
                    break;

                case 1600:
                    session.SendPacket(session.Character.OpenFamilyWarehouse());
                    break;

                case 1601:
                    session.SendPackets(session.Character.OpenFamilyWarehouseHist());
                    break;

                case 1602:
                    if (session.Character.Family?.FamilyLevel >= 3 && session.Character.Family.WarehouseSize < 21)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (500000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.WarehouseSize = 21;
                            session.Character.Gold -= 500000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1603:
                    if (session.Character.Family?.FamilyLevel >= 7 && session.Character.Family.WarehouseSize < 49)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (2000000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.WarehouseSize = 49;
                            session.Character.Gold -= 2000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1604:
                    if (session.Character.Family?.FamilyLevel >= 5 && session.Character.Family.MaxSize < 70)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (5000000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.MaxSize = 70;
                            session.Character.Gold -= 5000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1605:
                    if (session.Character.Family?.FamilyLevel >= 9 && session.Character.Family.MaxSize < 100)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (10000000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.MaxSize = 100;
                            session.Character.Gold -= 10000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 23:
                    if (type == 0)
                    {
                        if (session.Character.Group?.CharacterCount == 3)
                        {
                            foreach (ClientSession s in session.Character.Group.Characters.GetAllItems())
                            {
                                if (s.Character.Family != null)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_MEMBER_ALREADY_IN_FAMILY")));
                                    return;
                                }
                            }
                        }
                        if (session.Character.Group == null || session.Character.Group.CharacterCount != 3)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
                            return;
                        }
                        session.SendPacket(UserInterfaceHelper.GenerateInbox($"#glmk^ {14} 1 {Language.Instance.GetMessageFromKey("CREATE_FAMILY").Replace(' ', '^')}"));
                    }
                    else
                    {
                        if (session.Character.Family == null)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_IN_FAMILY")));
                            return;
                        }
                        if (session.Character.Family != null && session.Character.FamilyCharacter != null && session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                            return;
                        }
                        session.SendPacket($"qna #glrm^1 {Language.Instance.GetMessageFromKey("DISMISS_FAMILY")}");
                    }

                    break;

                case 60:
                    StaticBonusDTO medalDTO = session.Character.StaticBonusList.Find(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
                    byte medal = 0;
                    int time = 0;
                    if (medalDTO != null)
                    {
                        medal = medalDTO.StaticBonusType == StaticBonusType.BazaarMedalGold ? (byte)MedalType.Gold : (byte)MedalType.Silver;
                        time = (int)(medalDTO.DateEnd - DateTime.UtcNow).TotalHours;
                    }
                    session.SendPacket($"wopen 32 {medal} {time}");
                    break;

                case 5002:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        //Session.SendPacket("it 3");
                        if (ServerManager.Instance.ChannelId == 51)
                        {
                            string connection = CommunicationServiceClient.Instance.RetrieveOriginWorld(session.Account.AccountId);
                            if (string.IsNullOrWhiteSpace(connection))
                            {
                                return;
                            }
                            session.Character.MapId = tp.MapId;
                            session.Character.MapX = tp.MapX;
                            session.Character.MapY = tp.MapY;
                            int port = Convert.ToInt32(connection.Split(':')[1]);
                            session.Character.ChangeChannel(connection.Split(':')[0], port, 3);
                        }
                        else
                        {
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                    }
                    break;

                case 5001:
                    if (npc != null)
                    {
                        MapInstance map = null;
                        switch (session.Character.Faction)
                        {
                            case FactionType.None:
                                session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to be part of a faction to join Act 4"));
                                return;

                            case FactionType.Angel:
                                map = ServerManager.GetAllMapInstances().Find(s => s.MapInstanceType.Equals(MapInstanceType.Act4ShipAngel));

                                break;

                            case FactionType.Demon:
                                map = ServerManager.GetAllMapInstances().Find(s => s.MapInstanceType.Equals(MapInstanceType.Act4ShipDemon));

                                break;
                        }
                        if (map == null || npc.EffectActivated)
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHIP_NOTARRIVED"), 0));
                            return;
                        }
                        if (3000 > session.Character.Gold)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            return;
                        }
                        session.Character.Gold -= 3000;
                        MapCell pos = map.Map.GetRandomPosition();
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, map.MapInstanceId, pos.X, pos.Y);
                    }
                    break;

                case 5004:
                    if (npc != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, 1, 81, 3);
                    }
                    break;

                case 5011:
                    if (npc != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, 170, 127, 46);
                    }
                    break;

                case 5012:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }
                    break;

                default:
                    Logger.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"), runner));
                    break;
            }
        }

        #endregion
    }
}