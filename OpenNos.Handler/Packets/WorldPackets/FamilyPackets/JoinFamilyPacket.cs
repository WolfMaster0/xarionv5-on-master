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
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("gjoin")]
    public class JoinFamilyPacket
    {
        #region Properties

        public byte Type { get; set; }

        public long CharacterId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 4)
            {
                return;
            }
            JoinFamilyPacket packetDefinition = new JoinFamilyPacket();
            if (byte.TryParse(packetSplit[2], out byte type) && long.TryParse(packetSplit[3], out long charId))
            {
                packetDefinition.Type = type;
                packetDefinition.CharacterId = charId;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(JoinFamilyPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (Type == 1)
            {
                ClientSession inviteSession = ServerManager.Instance.GetSessionByCharacterId(CharacterId);
                if (inviteSession?.Character.FamilyInviteCharacters.GetAllItems()
                        .Contains(session.Character.CharacterId) == true && inviteSession.Character.Family?.FamilyCharacters != null)
                {
                    if (inviteSession.Character.Family.FamilyCharacters.Count + 1
                        > inviteSession.Character.Family.MaxSize || session.Character.Family != null)
                    {
                        return;
                    }

                    FamilyCharacterDTO familyCharacter = new FamilyCharacterDTO
                    {
                        CharacterId = session.Character.CharacterId,
                        DailyMessage = string.Empty,
                        Experience = 0,
                        Authority = FamilyAuthority.Member,
                        FamilyId = inviteSession.Character.Family.FamilyId,
                        Rank = 0
                    };
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacter);
                    inviteSession.Character.Family.InsertFamilyLog(FamilyLogType.UserManaged,
                        inviteSession.Character.Name, session.Character.Name);

                    GameLogger.Instance.LogGuildJoin(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, inviteSession.Character.Family.Name,
                        inviteSession.Character.Family.FamilyId);

                    CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                    {
                        DestinationCharacterId = inviteSession.Character.Family.FamilyId,
                        SourceCharacterId = session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("FAMILY_JOINED"), session.Character.Name,
                                inviteSession.Character.Family.Name), 0),
                        Type = MessageType.Family
                    });
                }
            }
        }

        #endregion
    }
}