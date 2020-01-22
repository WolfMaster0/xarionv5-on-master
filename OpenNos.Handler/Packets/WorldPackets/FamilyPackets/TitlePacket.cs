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

using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("%Title", "%Titre", "%Titel")]
    public class TitlePacket
    {
        #region Properties

        public string CharacterName { get; set; }

        public byte Rank { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 2)
            {
                return;
            }
            TitlePacket packetDefinition = new TitlePacket();
            if (!string.IsNullOrWhiteSpace(packetSplit[2]) && byte.TryParse(packetSplit[3], out byte rank))
            {
                packetDefinition.CharacterName = packetSplit[2];
                packetDefinition.Rank = rank;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(TitlePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family != null && session.Character.FamilyCharacter?.Authority == FamilyAuthority.Head)
            {
                FamilyCharacterDTO fchar =
                    session.Character.Family.FamilyCharacters.Find(s => s.Character.Name == CharacterName);
                if (fchar != null)
                {
                    fchar.Rank = (FamilyMemberRank) Rank;

                    GameLogger.Instance.LogGuildTitle(ServerManager.Instance.ChannelId, session.Character.Name,
                        session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId,
                        CharacterName, fchar.CharacterId, fchar.Rank);

                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref fchar);
                    ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                    CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                    {
                        DestinationCharacterId = session.Character.Family.FamilyId,
                        SourceCharacterId = session.Character.CharacterId,
                        SourceWorldId = ServerManager.Instance.WorldId,
                        Message = "fhis_stc",
                        Type = MessageType.Family
                    });
                }
            }        }

        #endregion
    }
}