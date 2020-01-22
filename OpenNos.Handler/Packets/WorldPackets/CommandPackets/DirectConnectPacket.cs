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

using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$DirectConnect", Authority = AuthorityType.GameMaster)]
    public class DirectConnectPacket
    {
        #region Members

        private bool _isParsed;

        #endregion

        #region Properties

        public string IpAddress { get; set; }

        public int Port { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                string[] packetSplit = packet.Split(' ');
                if (packetSplit.Length < 4)
                {
                    sess.SendPacket(sess.Character.GenerateSay(ReturnHelp(), 10));
                    return;
                }
                DirectConnectPacket packetDefinition = new DirectConnectPacket();
                if (!string.IsNullOrWhiteSpace(packetSplit[2]) && int.TryParse(packetSplit[3], out int port))
                {
                    packetDefinition._isParsed = true;
                    packetDefinition.IpAddress = packetSplit[2];
                    packetDefinition.Port = port;
                }
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(DirectConnectPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$DirectConnect IP PORT";

        private void ExecuteHandler(ClientSession session)
        {
            if (_isParsed)
            {
                session.Character.ChangeChannel(IpAddress, Port, 3);
            }
            else
            {
                session.SendPacket(session.Character.GenerateSay(ReturnHelp(), 10));
            }
        }

        #endregion
    }
}