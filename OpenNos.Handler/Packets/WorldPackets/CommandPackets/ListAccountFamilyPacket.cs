// This file is part of the OpenNos NosTale Emulator Project.
// 
// This program is licensed under a deviated version of the Fair Source License,
// granting you a non-exclusive, non-transferable, royalty-free and fully-paid-up
// license, under all of the Licensor's copyright and patent rights, to use, copy, prepare
// derivative works of, publicly perform and display the Software, subject to the
// contitions found in the LICENSE file.
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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$ListAccountFamily", Authority = AuthorityType.GameMaster)]
    public class ListAccountFamilyPacket
    {
        #region Properties

        private bool _isParsed;

        public long AccountId { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if(session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 3)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                ListAccountFamilyPacket packetDefinition = new ListAccountFamilyPacket();
                if (long.TryParse(packetSplit[2], out long accId))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.AccountId = accId;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ListAccountFamilyPacket), HandlePacket);

        public static string ReturnHelp() => "$ListAccountFamily AccountId";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                void WriteAccountInfo(AccountDTO dto)
                {
                    session.SendPacket(session.Character.GenerateSay($"AccountId: {dto.AccountId}", 13));
                    session.SendPacket(session.Character.GenerateSay($"Name: {dto.Name}", 13));
                    session.SendPacket(session.Character.GenerateSay($"E-Mail: {dto.Email}", 13));
                    session.SendPacket(session.Character.GenerateSay("----- ------- -----", 13));
                }
                session.SendPacket(session.Character.GenerateSay("----- ACCOUNTS -----", 13));
                foreach (AccountDTO acc in DAOFactory.AccountDAO.LoadFamilyById(AccountId))
                {
                    WriteAccountInfo(acc);
                }
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}