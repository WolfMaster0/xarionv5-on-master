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
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("%Familydismiss", "%Rejetdefamille", "%Familienentlassung")]
    public class FamilyKickPacket
    {
        #region Properties

        public string Name { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            FamilyKickPacket packetDefinition = new FamilyKickPacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]))
            {
                packetDefinition.Name = packetSplit[2];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyKickPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
                if (session.Character.Family == null || session.Character.FamilyCharacter == null)
                {
                    return;
                }

                if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                    || session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager)
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(
                            string.Format(Language.Instance.GetMessageFromKey("NOT_ALLOWED_KICK"))));
                    return;
                }

                ClientSession kickSession = ServerManager.Instance.GetSessionByCharacterName(Name);
                if (kickSession != null && kickSession.Character.Family?.FamilyId == session.Character.Family.FamilyId)
                {
                    if (kickSession.Character.FamilyCharacter?.Authority == FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_KICK_HEAD")));
                        return;
                    }

                    if (kickSession.Character.CharacterId == session.Character.CharacterId)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("CANT_KICK_YOURSELF")));
                        return;
                    }

                    GameLogger.Instance.LogGuildKick(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId,
                        kickSession.Character.Name, kickSession.Character.CharacterId);

                    DAOFactory.FamilyCharacterDAO.Delete(Name);
                    session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, kickSession.Character.Name);
                    kickSession.Character.Family = null;
                    kickSession.Character.LastFamilyLeave = DateTime.UtcNow.Ticks;
                }
                else
                {
                    CharacterDTO dbCharacter = DAOFactory.CharacterDAO.LoadByName(Name);
                    if (dbCharacter != null)
                    {
                        if (CommunicationServiceClient.Instance.IsCharacterConnected(ServerManager.Instance.ServerGroup,
                            dbCharacter.CharacterId))
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("CANT_KICK_PLAYER_ONLINE_OTHER_CHANNEL")));
                            return;
                        }

                        FamilyCharacterDTO dbFamilyCharacter =
                            DAOFactory.FamilyCharacterDAO.LoadByCharacterId(dbCharacter.CharacterId);
                        if (dbFamilyCharacter != null
                            && dbFamilyCharacter.FamilyId == session.Character.Family.FamilyId)
                        {
                            if (dbFamilyCharacter.Authority == FamilyAuthority.Head)
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateInfo(
                                        Language.Instance.GetMessageFromKey("CANT_KICK_HEAD")));
                                return;
                            }

                            GameLogger.Instance.LogGuildKick(ServerManager.Instance.ChannelId, session.Character.Name,
                                session.Character.CharacterId, session.Character.Family.Name,
                                session.Character.Family.FamilyId, dbCharacter.Name, dbCharacter.CharacterId);

                            DAOFactory.FamilyCharacterDAO.Delete(Name);
                            session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, dbCharacter.Name);
                            dbCharacter.LastFamilyLeave = DateTime.UtcNow.Ticks;
                            DAOFactory.CharacterDAO.InsertOrUpdate(ref dbCharacter);
                        }
                    }
                }
        }

        #endregion
    }
}