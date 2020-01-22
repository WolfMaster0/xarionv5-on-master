// This file is part of the OpenNos NosTale Emulator Project.
//
// This program is licensed under a deviated version of the Fair Source License, granting you a
// non-exclusive, non-transferable, royalty-free and fully-paid-up license, under all of the
// Licensor's copyright and patent rights, to use, copy, prepare derivative works of, publicly
// perform and display the Software, subject to the conditions found in the LICENSE file.
//
// THIS FILE IS PROVIDED "AS IS", WITHOUT WARRANTY OR CONDITION, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. THE AUTHORS HEREBY DISCLAIM ALL LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT
// OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE.

using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;
using System.Linq;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$ShowStaff", Authority = AuthorityType.GameMaster)]
    public class ShowStaff
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            ShowStaff packetDefinition = new ShowStaff();
            packetDefinition.ExecuteHandler(session as ClientSession);
        }

        public static void Register() => PacketFacility.AddHandler(typeof(ShowStaff), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$ShowStaff";

        private void ExecuteHandler(ClientSession session)
        {
            Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), "[ShowStaff]");
            string gameMasters = string.Empty;
            string moderators = string.Empty;
            string preparedString = string.Empty;
            foreach (ClientSession sess in ServerManager.Instance.Sessions.Where(s =>
                 s.Account.Authority == AuthorityType.GameMaster || s.Account.Authority == AuthorityType.Moderator))
            {
                preparedString = $"[{sess.Character.Name}]: MapId: {sess.Character.MapId} X: {sess.Character.PositionX}, Y: {sess.Character.PositionY}\n";
                if (sess.Account.Authority == AuthorityType.GameMaster)
                {
                    gameMasters += preparedString;
                }
                else
                {
                    moderators += preparedString;
                }
            }
            session.SendPacket(session.Character.GenerateSay("-------------GameMasters-------------", 10));
            session.SendPacket(session.Character.GenerateSay(gameMasters, 10));
            session.SendPacket(session.Character.GenerateSay("-------------Xarion Helpers--------------", 10));
            session.SendPacket(session.Character.GenerateSay(moderators, 10));
            session.SendPacket(session.Character.GenerateSay("-------------------------------------", 10));
        }

        #endregion
    }
}
