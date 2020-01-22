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

using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.CommandPackets
{
    [PacketHeader("$Invisible", Authority = AuthorityType.GameMaster)]
    public class InvisiblePacket
    {
        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            if (session is ClientSession sess)
            {
                InvisiblePacket packetDefinition = new InvisiblePacket();
                packetDefinition.ExecuteHandler(sess);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(InvisiblePacket), HandlePacket, ReturnHelp);

        public static string ReturnHelp() => "$Invisible";

        private void ExecuteHandler(ClientSession session)
        {
            Logger.LogUserEvent("GMCOMMAND", session.GenerateIdentity(), "[Invisible]");

            session.Character.Invisible = !session.Character.Invisible;
            session.Character.InvisibleGm = !session.Character.InvisibleGm;
            session.CurrentMapInstance?.Broadcast(session.Character.GenerateInvisible());
            session.SendPacket(session.Character.GenerateEq());
            if (session.Character.InvisibleGm)
            {
                session.Character.Mates.Where(s => s.IsTeamMember).ToList()
                    .ForEach(s => session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                session.CurrentMapInstance?.Broadcast(session,
                    StaticPacketHelper.Out(UserType.Player, session.Character.CharacterId), ReceiverType.AllExceptMe);
            }
            else
            {
                session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m =>
                    session.CurrentMapInstance?.Broadcast(m.GenerateIn(), ReceiverType.AllExceptMe));
                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(),
                    ReceiverType.AllExceptMe);
                session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(),
                    ReceiverType.AllExceptMe);
            }
        }

        #endregion
    }
}