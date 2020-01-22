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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.Handler.Packets.SharedPacketMethods
{
    internal static class SharedCommandMethods
    {
        #region Methods

        internal static void AddPortal(this ClientSession session, short destinationMapId, short destinationX, short destinationY, short type,
    bool insertToDatabase)
        {
            if (session.HasCurrentMapInstance)
            {
                Portal portal = new Portal
                {
                    SourceMapId = session.Character.MapId,
                    SourceX = session.Character.PositionX,
                    SourceY = session.Character.PositionY,
                    DestinationMapId = destinationMapId,
                    DestinationX = destinationX,
                    DestinationY = destinationY,
                    DestinationMapInstanceId = insertToDatabase ? Guid.Empty :
                        destinationMapId == 20000 ? session.Character.Miniland.MapInstanceId : Guid.Empty,
                    Type = type
                };
                if (insertToDatabase)
                {
                    DAOFactory.PortalDAO.Insert(portal);
                }

                session.CurrentMapInstance.Portals.Add(portal);
                session.CurrentMapInstance?.Broadcast(portal.GenerateGp());
            }
        }

        internal static void BanMethod(this ClientSession session, string characterName, int duration, string reason)
        {
            CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(characterName);
            if (character != null)
            {
                ServerManager.Instance.Kick(characterName);
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = character.AccountId,
                    Reason = reason,
                    Penalty = PenaltyType.Banned,
                    DateStart = DateTime.UtcNow,
                    DateEnd = duration == 0 ? DateTime.UtcNow.AddYears(15) : DateTime.UtcNow.AddDays(duration),
                    AdminName = session.Character.Name
                };
                Character.InsertOrUpdatePenalty(log);
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                    10));
            }
        }

        internal static void MuteMethod(this ClientSession session, string characterName, string reason, int duration)
        {
            CharacterDTO characterToMute = DAOFactory.CharacterDAO.LoadByName(characterName);
            if (characterToMute != null)
            {
                ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(characterName);
                if (targetSession?.Character.IsMuted() == false)
                {
                    targetSession.SendPacket(UserInterfaceHelper.GenerateInfo(
                        string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                }

                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = characterToMute.AccountId,
                    Reason = reason,
                    Penalty = PenaltyType.Muted,
                    DateStart = DateTime.UtcNow,
                    DateEnd = DateTime.UtcNow.AddMinutes(duration),
                    AdminName = session.Character.Name
                };
                Character.InsertOrUpdatePenalty(log);
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                    10));
            }
        }

        internal static void SendStats(this ClientSession session, CharacterDTO characterDto)
        {
            session.SendPacket(session.Character.GenerateSay("----- CHARACTER -----", 13));
            session.SendPacket(session.Character.GenerateSay($"Name: {characterDto.Name}", 13));
            session.SendPacket(session.Character.GenerateSay($"Id: {characterDto.CharacterId}", 13));
            session.SendPacket(session.Character.GenerateSay($"State: {characterDto.State}", 13));
            session.SendPacket(session.Character.GenerateSay($"Gender: {characterDto.Gender}", 13));
            session.SendPacket(session.Character.GenerateSay($"Class: {characterDto.Class}", 13));
            session.SendPacket(session.Character.GenerateSay($"Level: {characterDto.Level}", 13));
            session.SendPacket(session.Character.GenerateSay($"JobLevel: {characterDto.JobLevel}", 13));
            session.SendPacket(session.Character.GenerateSay($"HeroLevel: {characterDto.HeroLevel}", 13));
            session.SendPacket(session.Character.GenerateSay($"Gold: {characterDto.Gold}", 13));
            session.SendPacket(session.Character.GenerateSay($"Bio: {characterDto.Biography}", 13));
            session.SendPacket(session.Character.GenerateSay($"MapId: {characterDto.MapId}", 13));
            session.SendPacket(session.Character.GenerateSay($"MapX: {characterDto.MapX}", 13));
            session.SendPacket(session.Character.GenerateSay($"MapY: {characterDto.MapY}", 13));
            session.SendPacket(session.Character.GenerateSay($"Reputation: {characterDto.Reputation}", 13));
            session.SendPacket(session.Character.GenerateSay($"Dignity: {characterDto.Dignity}", 13));
            session.SendPacket(session.Character.GenerateSay($"Rage: {characterDto.RagePoint}", 13));
            session.SendPacket(session.Character.GenerateSay($"Compliment: {characterDto.Compliment}", 13));
            session.SendPacket(session.Character.GenerateSay(
                $"Fraction: {(characterDto.Faction == FactionType.Demon ? Language.Instance.GetMessageFromKey("DEMON") : Language.Instance.GetMessageFromKey("ANGEL"))}",
                13));
            session.SendPacket(session.Character.GenerateSay("----- --------- -----", 13));
            AccountDTO account = DAOFactory.AccountDAO.LoadById(characterDto.AccountId);
            if (account != null)
            {
                session.SendPacket(session.Character.GenerateSay("----- ACCOUNT -----", 13));
                session.SendPacket(session.Character.GenerateSay($"Id: {account.AccountId}", 13));
                session.SendPacket(session.Character.GenerateSay($"Name: {account.Name}", 13));
                session.SendPacket(session.Character.GenerateSay($"Authority: {account.Authority}", 13));
                session.SendPacket(session.Character.GenerateSay($"RegistrationIP: {account.RegistrationIP}", 13));
                session.SendPacket(session.Character.GenerateSay($"Email: {account.Email}", 13));
                session.SendPacket(session.Character.GenerateSay("----- ------- -----", 13));
                IEnumerable<PenaltyLogDTO> penaltyLogs = ServerManager.Instance.PenaltyLogs
                    .Where(s => s.AccountId == account.AccountId).ToList();
                PenaltyLogDTO penalty = penaltyLogs.LastOrDefault(s => s.DateEnd > DateTime.UtcNow);
                session.SendPacket(session.Character.GenerateSay("----- PENALTY -----", 13));
                if (penalty != null)
                {
                    session.SendPacket(session.Character.GenerateSay($"Type: {penalty.Penalty}", 13));
                    session.SendPacket(session.Character.GenerateSay($"AdminName: {penalty.AdminName}", 13));
                    session.SendPacket(session.Character.GenerateSay($"Reason: {penalty.Reason}", 13));
                    session.SendPacket(session.Character.GenerateSay($"DateStart: {penalty.DateStart}", 13));
                    session.SendPacket(session.Character.GenerateSay($"DateEnd: {penalty.DateEnd}", 13));
                }

                session.SendPacket(
                    session.Character.GenerateSay($"Bans: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Banned)}",
                        13));
                session.SendPacket(
                    session.Character.GenerateSay($"Mutes: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Muted)}",
                        13));
                session.SendPacket(
                    session.Character.GenerateSay(
                        $"Warnings: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Warning)}", 13));
                session.SendPacket(session.Character.GenerateSay("----- ------- -----", 13));
            }

            session.SendPacket(session.Character.GenerateSay("----- SESSION -----", 13));
            foreach (long[] connection in CommunicationServiceClient.Instance.RetrieveOnlineCharacters(characterDto
                .CharacterId))
            {
                if (connection != null)
                {
                    CharacterDTO character = DAOFactory.CharacterDAO.LoadById(connection[0]);
                    if (character != null)
                    {
                        session.SendPacket(session.Character.GenerateSay($"Character Name: {character.Name}", 13));
                        session.SendPacket(session.Character.GenerateSay($"ChannelId: {connection[1]}", 13));
                        session.SendPacket(session.Character.GenerateSay("-----", 13));
                    }
                }
            }

            session.SendPacket(session.Character.GenerateSay("----- ------------ -----", 13));
        }

        #endregion
    }
}