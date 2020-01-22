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
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using OpenNos.XMLModel.Quest.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;

namespace OpenNos.Handler.Packets.WorldPackets.BasicPackets
{
    [PacketHeader("game_start")]
    public class GameStartPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (string.IsNullOrEmpty(packet))
            {
                return;
            }
            GameStartPacket gameStartPacket = new GameStartPacket();
            gameStartPacket.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GameStartPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.IsOnMap || !session.HasSelectedCharacter)
            {
                // character should have been selected in SelectCharacter
                return;
            }

            if (session.Character.MapInstance.Map.MapTypes.Any(m => m.MapTypeId == (short)MapTypeEnum.Act4)
                && ServerManager.Instance.ChannelId != 51)
            {
                session.Character.ChangeChannel(ServerManager.Instance.Configuration.Act4IP,
                    ServerManager.Instance.Configuration.Act4Port, 2);
                return;
            }

            session.CurrentMapInstance = session.Character.MapInstance;
            if (ServerManager.Instance.Configuration.SceneOnCreate
                && session.Character.GeneralLogs.CountLinq(s => s.LogType == "Connection") < 2)
            {
                session.SendPacket("scene 40");
            }

            if (ServerManager.Instance.Configuration.WorldInformation)
            {
                Assembly assembly = Assembly.GetEntryAssembly();
                string productVersion = assembly?.Location != null
                    ? FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion
                    : "1337";
                session.SendPacket(session.Character.GenerateSay("----------[World Information]----------", 10));
                session.SendPacket(
                    session.Character.GenerateSay("XARION - REUSRRECTION\n" +
                                                  $"Version : V4\n" +
                                                  $"Built by: XARION TEAM\n" +
                                                  $"Running on current Xarion supported source\n" + (ServerManager.Instance.IsDebugMode ? "DEBUG_MODE: Enabled\n" : "\n"), 11));
                session.SendPacket(session.Character.GenerateSay("-----------------------------------------------",
                    10));
            }

            session.Character.LoadSpeed();
            session.Character.LoadSkills();
            session.Character.LoadPassiveSkills();
            session.SendPacket(session.Character.GenerateTit());
            session.SendPacket(session.Character.GenerateSpPoint());
            session.SendPacket("rsfi 1 1 0 9 0 9");
            if (session.Character.Hp <= 0)
            {
                ServerManager.Instance.ReviveFirstPosition(session.Character.CharacterId);
            }
            else
            {
                ServerManager.Instance.ChangeMap(session.Character.CharacterId);
            }

            session.SendPacket(session.Character.GenerateSki());
            session.SendPacket(
                $"fd {session.Character.Reputation} 0 {(int)session.Character.Dignity} {Math.Abs(session.Character.GetDignityIco())}");
            session.SendPacket(session.Character.GenerateFd());
            session.SendPacket("rage 0 250000");
            session.SendPacket("rank_cool 0 0 18000");
            ItemInstance specialistInstance = session.Character.Inventory.LoadBySlotAndType(8, InventoryType.Wear);
            StaticBonusDTO medal = session.Character.StaticBonusList.Find(s =>
                s.StaticBonusType == StaticBonusType.BazaarMedalGold
                || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
            if (medal != null)
            {
                session.SendPacket(
                    session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOGIN_MEDAL"), 12));
            }

            if (session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.PetBasket))
            {
                session.SendPacket("ib 1278 1");
            }

            if (session.Character.MapInstance.Map.MapTypes.Any(m => m.MapTypeId == (short)MapTypeEnum.CleftOfDarkness))
            {
                session.SendPacket("bc 0 0 0");
            }

            if (specialistInstance != null)
            {
                session.SendPacket(session.Character.GenerateSpPoint());
            }

            session.SendPacket("scr 0 0 0 0 0 0");
            for (int i = 0; i < 10; i++)
            {
                session.SendPacket($"bn {i} {Language.Instance.GetMessageFromKey($"BN{i}")}");
            }

            session.SendPacket(session.Character.GenerateExts());
            session.SendPacket(session.Character.GenerateMlinfo());
            session.SendPacket(UserInterfaceHelper.GeneratePClear());

            session.SendPacket(session.Character.GeneratePinit());
            session.SendPacket(session.Character.GeneratePinit());
            session.SendPackets(session.Character.Mates.Where(s => s.IsTeamMember).OrderBy(s => s.MateType).Select(s => s.GeneratePst()));

            foreach (var mate in session.Character.Mates.Where(s => s.IsTeamMember))
            {
                mate.JoinTeam(true);
            }

            session.SendPacket("zzim");
            session.SendPacket(
                $"twk 1 {session.Character.CharacterId} {session.Account.Name} {session.Character.Name} {session.Account.Password.Substring(0, 16)}");

            long? familyId = DAOFactory.FamilyCharacterDAO.LoadByCharacterId(session.Character.CharacterId)?.FamilyId;
            if (familyId.HasValue)
            {
                session.Character.Family = ServerManager.Instance.FamilyList[familyId.Value];
            }

            if (session.Character.Family != null && session.Character.FamilyCharacter != null)
            {
                session.SendPacket(session.Character.GenerateGInfo());
                session.SendPackets(session.Character.GetFamilyHistory());
                session.SendPacket(session.Character.GenerateFamilyMember());
                session.SendPacket(session.Character.GenerateFamilyMemberMessage());
                session.SendPacket(session.Character.GenerateFamilyMemberExp());

                session.Character.Faction = session.Character.Family.FamilyCharacters
                                                .Find(s => s.Authority.Equals(FamilyAuthority.Head))?.Character
                                                ?.Faction ?? FactionType.None;

                if (!string.IsNullOrWhiteSpace(session.Character.Family.FamilyMessage))
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo("--- Family Message ---\n" +
                                                         session.Character.Family.FamilyMessage));
                }
            }

            // qstlist target sqst bf
            session.SendPacket("act6");
            session.SendPacket(session.Character.GenerateFaction());
            session.SendPackets(session.Character.GenerateScP());
            session.SendPackets(session.Character.GenerateScN());
#pragma warning disable 618
            session.Character.GenerateStartupInventory();
#pragma warning restore 618

            session.SendPacket(session.Character.GenerateGold());
            session.SendPackets(session.Character.GenerateQuicklist());

            string clinit = "clinit";
            string flinit = "flinit";
            string kdlinit = "kdlinit";
            foreach (CharacterDTO character in ServerManager.Instance.TopComplimented)
            {
                clinit +=
                    $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Compliment}|{character.Name}";
            }

            foreach (CharacterDTO character in ServerManager.Instance.TopReputation)
            {
                flinit +=
                    $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Reputation}|{character.Name}";
            }

            foreach (CharacterDTO character in ServerManager.Instance.TopPoints)
            {
                kdlinit +=
                    $" {character.CharacterId}|{character.Level}|{character.HeroLevel}|{character.Act4Points}|{character.Name}";
            }

            session.CurrentMapInstance?.Broadcast(session.Character.GenerateGidx());

            session.SendPacket(session.Character.GenerateFinit());
            session.SendPacket(session.Character.GenerateBlinit());
            session.SendPacket(clinit);
            session.SendPacket(flinit);
            session.SendPacket(kdlinit);

            session.Character.LastPvpRevive = DateTime.UtcNow;

            List<PenaltyLogDTO> warning = DAOFactory.PenaltyLogDAO.LoadByAccount(session.Character.AccountId)
                .Where(p => p.Penalty == PenaltyType.Warning).ToList();
            if (warning.Count > 0)
            {
                session.SendPacket(UserInterfaceHelper.GenerateInfo(
                    string.Format(Language.Instance.GetMessageFromKey("WARNING_INFO"), warning.Count)));
            }

            // finfo - friends info
            session.Character.LoadMail();
            session.Character.LoadSentMail();
            session.Character.DeleteTimeout();

            foreach (StaticBuffDTO staticBuff in DAOFactory.StaticBuffDAO.LoadByCharacterId(session.Character
                .CharacterId))
            {
                session.Character.AddStaticBuff(staticBuff);
            }

            if (session.Character.Authority == AuthorityType.BitchNiggerFaggot)
            {
                CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                {
                    DestinationCharacterId = null,
                    SourceCharacterId = session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message =
                        $"User {session.Character.Name} with rank BitchNiggerFaggot has logged in, don't trust *it*!",
                    Type = MessageType.Shout
                });
            }

            string parsedip = session.IpAddress.Split(':')[1].Replace("//", "");
            List<GeneralLogDTO> tmp = DAOFactory.GeneralLogDAO.LoadByLogType("DailyLogin", null, true).ToList();
            if (!tmp.Any(s => s.AccountId.Equals(session.Account.AccountId)))
            {
                session.Character.RaidDracoRuns = 0;
                session.Character.RaidGlacerusRuns = 0;
                DAOFactory.GeneralLogDAO.WriteGeneralLog(session.Account.AccountId, parsedip, null, "DailyLogin",
                    "World");
            }

            GeneralLogDTO securityLog = DAOFactory.GeneralLogDAO.LoadByLogType("TOTP", null, true)
                .LastOrDefault(s => s.AccountId == session.Account.AccountId);

            if (securityLog?.IpAddress.Split(':')[1].Replace("//", "") != parsedip || securityLog.LogData != "SUCCESS")
            {
                if (string.IsNullOrWhiteSpace(session.Account.TotpSecret))
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateModal(
                            Language.Instance.GetMessageFromKey("WELCOME ON XARION").Replace("\\n", "\n"), 1));
                    session.Account.IsVerified = true;
                }
                else
                {
                    session.SendPacket(
                        session.Character.GenerateSay(
                            Language.Instance.GetMessageFromKey("TOTP_INFO").Replace("\\n", "\n"), 12));
                    session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 11, session.Character.CharacterId, 2));
                    Observable.Timer(TimeSpan.FromSeconds(60)).Subscribe(o =>
                    {
                        if (session.Account.IsVerified == false)
                        {
                            session.Disconnect();
                        }
                    });
                }
            }
            else
            {
                session.Account.IsVerified = true;
            }


            session.Character.QuestManager = new QuestManager(session);

            QuestModel firstQuestModel =
                ServerManager.Instance.QuestModelList.FirstOrDefault(s => s.QuestGiver.Type == QuestGiverType.InitialQuest);

            if (firstQuestModel != null && DAOFactory.QuestProgressDAO.LoadByCharacterId(session.Character.CharacterId)
                .All(s => s.QuestId != firstQuestModel.QuestId))
            {
                session.Character.QuestManager.AddQuest(firstQuestModel.QuestId);
            }

            #region !Multi baguette detection!

            bool trapTriggered = false;
            bool possibleUnregisteredException = false;
            long[][] connections = CommunicationServiceClient.Instance.RetrieveOnlineCharacters(session.Character.CharacterId);
            foreach (long[] connection in connections)
            {
                if (connection != null)
                {
                    CharacterDTO characterDTO = DAOFactory.CharacterDAO.LoadById(connection[0]);
                    if (characterDTO != null)
                    {
                        MultiAccountExceptionDTO exception = DAOFactory.MultiAccountExceptionDAO.LoadByAccount(characterDTO.AccountId);
                        if (exception == null && connections.Length > 3)
                        {
                            trapTriggered = true;
                        }
                        if (exception != null && connections.Length > exception.ExceptionLimit)
                        {
                            possibleUnregisteredException = true;
                        }
                    }
                }
            }
            if (possibleUnregisteredException)
            {
                foreach (ClientSession team in ServerManager.Instance.Sessions.Where(s =>
                s.Account.Authority == AuthorityType.GameMaster || s.Account.Authority == AuthorityType.Moderator))
                {
                    if (team.HasSelectedCharacter)
                    {
                        team.SendPacket(team.Character.GenerateSay(
                            string.Format("Possible unregistered exception detected for user: " + session.Character.Name + ", CharacterId: " + session.Character.CharacterId), 12));
                    }
                }
            }
            if (trapTriggered)
            {
                foreach (ClientSession team in ServerManager.Instance.Sessions.Where(s =>
                s.Account.Authority == AuthorityType.GameMaster || s.Account.Authority == AuthorityType.Moderator))
                {
                    if (team.HasSelectedCharacter)
                    {
                        team.SendPacket(team.Character.GenerateSay(
                            string.Format("Possible multi account abusing user: " + session.Character.Name + ", CharacterId: " + session.Character.CharacterId), 12));
                    }
                }
            }

            #endregion

        }

        #endregion
    }
}