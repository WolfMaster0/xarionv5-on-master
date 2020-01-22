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
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("glrm")]
    public class FamilyDismissPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            FamilyDismissPacket packetDefinition = new FamilyDismissPacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyDismissPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null || session.Character.FamilyCharacter == null
                || session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
            {
                return;
            }

            Family fam = session.Character.Family;

            fam.FamilyCharacters.ForEach(s => DAOFactory.FamilyCharacterDAO.Delete(s.Character.Name));
            fam.FamilyLogs.ForEach(s => DAOFactory.FamilyLogDAO.Delete(s.FamilyLogId));
            DAOFactory.FamilyDAO.Delete(fam.FamilyId);
            ServerManager.Instance.FamilyRefresh(fam.FamilyId);

            GameLogger.Instance.LogGuildDismiss(ServerManager.Instance.ChannelId, session.Character.Name,
                session.Character.CharacterId, fam.Name, fam.FamilyId);

            CommunicationServiceClient.Instance.SendMessageToCharacter(new ScsCharacterMessage
            {
                DestinationCharacterId = fam.FamilyId,
                SourceCharacterId = session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = "fhis_stc",
                Type = MessageType.Family
            });
        }

        #endregion
    }
}