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
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.Packets.WorldPackets.FamilyPackets
{
    [PacketHeader("glist")]
    public class GListPacket
    {
        #region Properties

        public byte Type { get; set; }

        #endregion

        #region Methods

        public static void HandlePacket(object session, string packet)
        {
            string[] packetSplit = packet.Split(' ');
            if (packetSplit.Length < 3)
            {
                return;
            }
            GListPacket packetDefinition = new GListPacket();
            if (byte.TryParse(packetSplit[2], out byte type))
            {
                packetDefinition.Type = type;
                packetDefinition.ExecuteHandler(session as ClientSession);
            }
        }

        public static void Register() => PacketFacility.AddHandler(typeof(GListPacket), HandlePacket);

        private void ExecuteHandler(ClientSession session)
        {
            if (session.Character.Family != null && session.Character.FamilyCharacter != null && Type == 2)
            {
                // TODO: Check this
                session.SendPacket(session.Character.GenerateGInfo());
                session.SendPacket(session.Character.GenerateFamilyMember());
                session.SendPacket(session.Character.GenerateFamilyMemberMessage());
                session.SendPacket(session.Character.GenerateFamilyMemberExp());
            }
        }

        #endregion
    }
}