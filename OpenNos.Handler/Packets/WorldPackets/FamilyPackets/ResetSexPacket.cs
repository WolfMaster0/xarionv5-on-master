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
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("%Sex", "%Sexe", "%Geschlecht")]
    public class ResetSexPacket
    {
        #region Properties

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
            ResetSexPacket packetDefinition = new ResetSexPacket();
            if (byte.TryParse(packetSplit[2], out byte rank))
            {
                packetDefinition.Rank = rank;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ResetSexPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family != null && session.Character.FamilyCharacter?.Authority == FamilyAuthority.Head)
            {
                foreach (FamilyCharacter familyCharacter in session.Character.Family.FamilyCharacters)
                {
                    FamilyCharacterDTO familyCharacterDto = familyCharacter;
                    familyCharacterDto.Rank = 0;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref familyCharacterDto);
                }

                GameLogger.Instance.LogGuildResetSex(ServerManager.Instance.ChannelId, session.Character.Name,
                    session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId);

                FamilyDTO fam = session.Character.Family;
                fam.FamilyHeadGender = (GenderType) Rank;
                DAOFactory.FamilyDAO.InsertOrUpdate(ref fam);
                ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                {
                    DestinationCharacterId = fam.FamilyId,
                    SourceCharacterId = session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = "fhis_stc",
                    Type = MessageType.Family
                });

                CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
                {
                    DestinationCharacterId = fam.FamilyId,
                    SourceCharacterId = session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("FAMILY_HEAD_CHANGE_GENDER")), 0),
                    Type = MessageType.Family
                });
            }
        }

        #endregion
    }
}