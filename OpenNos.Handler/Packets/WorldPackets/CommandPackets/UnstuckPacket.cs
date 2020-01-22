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
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Unstuck", Authority = AuthorityType.User)]
    public class UnstuckPacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                UnstuckPacket packetDefinition = new UnstuckPacket();
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(UnstuckPacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Unstuck ";

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Miniland == session.Character.MapInstance)
            {
                ServerManager.Instance.JoinMiniland(session, session);
            }
            else
            {
                ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                    session.Character.MapInstanceId, session.Character.PositionX, session.Character.PositionY,
                    true);
                session.SendPacket(StaticPacketHelper.Cancel(2));
            }
        }

        #endregion
    }
}