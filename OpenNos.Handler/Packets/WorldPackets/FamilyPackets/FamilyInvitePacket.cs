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
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("%Familyinvite", "%Invitationdefamille", "%Familieneinladung")]
    public class FamilyInvitePacket
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
            FamilyInvitePacket packetDefinition = new FamilyInvitePacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]))
            {
                packetDefinition.Name = packetSplit[2];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyInvitePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null || session.Character.FamilyCharacter == null)
            {
                return;
            }

            if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Member
                || (session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager
                 && !session.Character.Family.ManagerCanInvite))
            {
                session.SendPacket(UserInterfaceHelper.GenerateInfo(
                    string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITATION_NOT_ALLOWED"))));
                return;
            }

            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterName(Name);
            if (otherSession == null)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(
                        string.Format(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"))));
                return;
            }

            GameLogger.Instance.LogGuildInvite(ServerManager.Instance.ChannelId, session.Character.Name,
                session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId,
                otherSession.Character.Name, otherSession.Character.CharacterId);

            if (session.Character.IsBlockedByCharacter(otherSession.Character.CharacterId))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                return;
            }

            if (session.Character.Family.FamilyCharacters.Count + 1 > session.Character.Family.MaxSize)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_FULL")));
                return;
            }

            if (otherSession.Character.Family != null || otherSession.Character.FamilyCharacter != null)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("ALREADY_IN_FAMILY")));
                return;
            }

            if (otherSession.Character.LastFamilyLeave > DateTime.UtcNow.AddDays(-1).Ticks)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_ENTER_FAMILY")));
                return;
            }

            session.SendPacket(UserInterfaceHelper.GenerateInfo(
                string.Format(Language.Instance.GetMessageFromKey("FAMILY_INVITED"), otherSession.Character.Name)));
            otherSession.SendPacket(UserInterfaceHelper.GenerateDialog(
                $"#gjoin^1^{session.Character.CharacterId} #gjoin^2^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("ASK_FAMILY_INVITED"), session.Character.Family.Name)}"));
            session.Character.FamilyInviteCharacters.Add(otherSession.Character.CharacterId);
        }

        #endregion
    }
}