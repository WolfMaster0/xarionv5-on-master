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
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("fmg")]
    public class FamilyManagementPacket
    {
        #region Properties

        public FamilyAuthority FamilyAuthorityType { get; set; }

        public long TargetId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            FamilyManagementPacket packetDefinition = new FamilyManagementPacket();
            if (Enum.TryParse(packetSplit[2], out FamilyAuthority authority) && long.TryParse(packetSplit[3], out long targetId))
            {
                packetDefinition.FamilyAuthorityType = authority;
                packetDefinition.TargetId = targetId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyManagementPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null)
            {
                return;
            }

            if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                || session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager)
            {
                return;
            }

            long targetId = TargetId;
            if (DAOFactory.FamilyCharacterDAO.LoadByCharacterId(targetId)?.FamilyId
                != session.Character.FamilyCharacter.FamilyId)
            {
                return;
            }

            ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterId(targetId);
            if (targetSession == null)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("PLAYER_OFFLINE")));
                return;
            }

            GameLogger.Instance.LogGuildManagement(ServerManager.Instance.ChannelId, session.Character.Name,
                session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId,
                targetSession.Character.Name, targetSession.Character.CharacterId, FamilyAuthorityType);
            switch (FamilyAuthorityType)
            {
                case FamilyAuthority.Head:
                    if (session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                        return;
                    }

                    if (targetSession.Character.FamilyCharacter.Authority != FamilyAuthority.Assistant)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ONLY_PROMOTE_ASSISTANT")));
                        return;
                    }

                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Head;
                    FamilyCharacterDTO chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);

                    session.Character.Family.Warehouse.ForEach(s =>
                    {
                        s.CharacterId = targetSession.Character.CharacterId;
                        DAOFactory.ItemInstanceDAO.InsertOrUpdate(s);
                    });
                    session.Character.FamilyCharacter.Authority = FamilyAuthority.Assistant;
                    FamilyCharacterDTO chara2 = session.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara2);
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));
                    break;

                case FamilyAuthority.Assistant:
                    if (session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                        return;
                    }

                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }

                    if (DAOFactory.FamilyCharacterDAO.LoadByFamilyId(session.Character.Family.FamilyId)
                            .Count(s => s.Authority == FamilyAuthority.Assistant) == 2)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ALREADY_TWO_ASSISTANT")));
                        return;
                    }

                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Assistant;
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);
                    break;

                case FamilyAuthority.Manager:
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }

                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
                        && session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                        return;
                    }

                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Manager;
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));
                    chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);
                    break;

                case FamilyAuthority.Member:
                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("HEAD_UNDEMOTABLE")));
                        return;
                    }

                    if (targetSession.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
                        && session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                    {
                        session.SendPacket(
                            UserInterfaceHelper.GenerateInfo(
                                Language.Instance.GetMessageFromKey("ASSISTANT_UNDEMOTABLE")));
                        return;
                    }

                    targetSession.Character.FamilyCharacter.Authority = FamilyAuthority.Member;
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("DONE")));

                    chara = targetSession.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref chara);
                    break;
            }

            session.Character.Family.InsertFamilyLog(FamilyLogType.AuthorityChanged, session.Character.Name,
                targetSession.Character.Name, authority: FamilyAuthorityType);
        }

        #endregion
    }
}