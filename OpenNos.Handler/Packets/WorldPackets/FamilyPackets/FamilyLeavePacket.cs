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
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameLog.LogHelper;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("%Familyleave", "%Congédefamille", "%Familienaustritt")]
    public class FamilyLeavePacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            FamilyLeavePacket packetDefinition = new FamilyLeavePacket();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(FamilyLeavePacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family == null || session.Character.FamilyCharacter == null)
            {
                return;
            }

            if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANNOT_LEAVE_FAMILY")));
                return;
            }

            DAOFactory.FamilyCharacterDAO.Delete(session.Character.Name);

            GameLogger.Instance.LogGuildLeave(ServerManager.Instance.ChannelId, session.Character.Name,
                session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId);

            session.Character.Family.InsertFamilyLog(FamilyLogType.FamilyManaged, session.Character.Name);
            session.Character.Family = null;
            session.Character.LastFamilyLeave = DateTime.UtcNow.Ticks;
        }

        #endregion
    }
}