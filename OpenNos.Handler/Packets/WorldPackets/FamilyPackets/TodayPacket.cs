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
    [PacketHeader("%Today", "%Aujourd'hui", "%Heute")]
    public class TodayPacket
    {
        #region Properties

        public string Message { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(new[]{' '},3);
            if (packetSplit.Length < 3)
            {
                return;
            }
            TodayPacket packetDefinition = new TodayPacket();
            if (packetSplit[2] != null)
            {
                packetDefinition.Message = packetSplit[2];
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(TodayPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family != null && session.Character.FamilyCharacter != null)
            {
                GameLogger.Instance.LogGuildToday(ServerManager.Instance.ChannelId, session.Character.Name,
                    session.Character.CharacterId, session.Character.Family.Name, session.Character.Family.FamilyId,
                    Message);

                bool islog = session.Character.Family.FamilyLogs.Any(s =>
                    s.FamilyLogType == FamilyLogType.DailyMessage
                    && s.FamilyLogData.StartsWith(session.Character.Name, StringComparison.CurrentCulture)
                    && s.Timestamp.AddDays(1) > DateTime.UtcNow);
                if (!islog)
                {
                    session.Character.FamilyCharacter.DailyMessage = Message;
                    FamilyCharacterDTO fchar = session.Character.FamilyCharacter;
                    DAOFactory.FamilyCharacterDAO.InsertOrUpdate(ref fchar);
                    session.Character.Family.InsertFamilyLog(FamilyLogType.DailyMessage, session.Character.Name,
                        message: Message);
                }
                else
                {
                    session.SendPacket(
                        UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("CANT_CHANGE_MESSAGE")));
                }
            }
        }

        #endregion
    }
}