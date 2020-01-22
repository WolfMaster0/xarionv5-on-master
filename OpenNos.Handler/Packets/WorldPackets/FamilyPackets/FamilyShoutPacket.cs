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
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("%Familyshout", "%Cridefamille", "%Familienruf")]
    public class FamilyShoutPacket
    {
        #region Properties

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[] {' '}, 3);
            if (packetSplit.Length < 3)
            {
                return;
            }
            FamilyShoutPacket packetDefinition = new FamilyShoutPacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]))
            {
                packetDefinition.Message = packetSplit[2];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyShoutPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family != null && session.Character.FamilyCharacter != null)
            {
                if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Assistant
                    || (session.Character.FamilyCharacter.Authority == FamilyAuthority.Manager
                        && session.Character.Family.ManagerCanShout)
                    || session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                {
                    GameLogger.Instance.LogGuildShout(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId,
                        Message);

                    CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                    {
                        DestinationCharacterId = session.Character.Family.FamilyId,
                        SourceCharacterId = session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = UserInterfaceHelper.GenerateMsg(
                            $"<{Language.Instance.GetMessageFromKey("FAMILYCALL")}> {Message}", 0),
                        Type = MessageType.Family
                    });
                }
            }
        }

        #endregion
    }
}